using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoodPlusAPI.Core;
using MoodPlusAPI.Empresas;
using MoodPlusAPI.Eventos;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;
using MoodPlusAPI.Moods;
using MoodPlusAPI.Usuarios;
using MoodPlusAPI.Utils;
using Swashbuckle.AspNetCore.SwaggerGen;
using UsuarioPlusAPI.Usuarios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Configurações

// Configuração para a rota [controller] ficar minuscula

// Configuração do Swagger
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(s =>
{
    s.OperationFilter<SwaggerDefaultValues>();

    // Configuração para o Swagger não especificar Enums
    s.UseInlineDefinitionsForEnums();


    // Configurações para informar o JWT no Swagger
    s.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT com prefixo 'Bearer '"
    });

    s.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers()
    // Configuração para enums serem passados como string
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<MongoDbContext>(serviceProvider =>
{
    var mongoDbConnectionString = builder.Configuration.GetConnectionString("MongoDB");
    ArgumentNullException.ThrowIfNull(mongoDbConnectionString, "Connection string 'MongoDB' não encontrada");

    var appDbConnection = new MongoDbContext(mongoDbConnectionString);

    // Pode ser migrado para um middleware para definir o banco de dados em tempo de execução
    appDbConnection.SetDatabase("MoodPlusAPI");
    return appDbConnection;
});

// Configuração necessária para utilizar JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Configuração regras JWT 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", regra => regra.RequireRole("Admin"));
    options.AddPolicy("Gerente", regra => regra.RequireRole("Gerente", "Admin"));
    options.AddPolicy("Usuario", regra => regra.RequireRole("Usuario", "Gerente", "Admin"));
});


// Configuração para versionamento
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1.1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"));
})
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });


builder.Services.AddSingleton<TokenJwt>();

builder.Services.AddScoped<RequestContext>();

builder.Services.AddScoped(typeof(CoreRepository<>));

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<UsuarioRepository>();

builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<EmpresaRepository>();

builder.Services.AddScoped<MoodService>();
builder.Services.AddScoped<MoodRepository>();

builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<EventoRepository>();

#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Configuração do Swagger para versionamento
    app.UseSwaggerUI(options =>
    {
        foreach (var description in app.DescribeApiVersions().OrderByDescending(d => d.ApiVersion))
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName);
        }
    });
}

app.UseHttpsRedirection();

// Configuração para o JWT
app.UseAuthentication();

app.UseAuthorization();

// Middleware para retornar problem details em caso de exception interna
app.UseMiddleware<ExceptionMiddleware>();
// Middleware para popular o request context
app.UseMiddleware<RequestContextMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var mongoDb = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await mongoDb.ConfigMongoIndexes();
}

app.Run();