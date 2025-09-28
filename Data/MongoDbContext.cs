using MongoDB.Driver;
using MoodPlusAPI.Empresas;
using MoodPlusAPI.Moods;
using MoodPlusAPI.Usuarios;

namespace MoodPlusAPI.MongoDb
{
    public class MongoDbContext
    {
        private readonly MongoClient _client;
        private IMongoDatabase? _mongoDatabase;

        public MongoDbContext(string connectionString)
        {
            _client = new MongoClient(connectionString);
        }

        public void SetDatabase(string databaseName)
        {
            _mongoDatabase = _client.GetDatabase(databaseName);
        }

        public IMongoCollection<TEntity> GetCollection<TEntity>(string collectionName)
        {
            if (_mongoDatabase == null)
            {
                throw new InvalidOperationException("Conexão com o banco de dados não definida");
            }

            return _mongoDatabase.GetCollection<TEntity>(collectionName);
        }

        public async Task ConfigMongoIndexes()
        {
            if (_mongoDatabase == null)
            {
                throw new InvalidOperationException("Conexão com o banco de dados não definida");
            }

            // Usuários
            var users = _mongoDatabase.GetCollection<Usuario>(nameof(Usuario));
            var userIndexes = new[]
            {
                new CreateIndexModel<Usuario>(
                    Builders<Usuario>.IndexKeys.Ascending(u => u.Email),
                    new CreateIndexOptions { Unique = true })
            };
            await users.Indexes.CreateManyAsync(userIndexes);

            // Empresas
            var enterprises = _mongoDatabase.GetCollection<Empresa>(nameof(Empresa));
            var enterprisesIndexes = new[]
            {
                new CreateIndexModel<Empresa>(
                    Builders<Empresa>.IndexKeys.Ascending(e => e.CNPJ),
                    new CreateIndexOptions { Unique = true }),
            };
            await enterprises.Indexes.CreateManyAsync(enterprisesIndexes);

            // Moods
            var moods = _mongoDatabase.GetCollection<Mood>(nameof(Mood));
            var moodsIndexes = new[]
            {
                new CreateIndexModel<Mood>(
                    Builders<Mood>.IndexKeys.Ascending(m => m.EmpresaId).Ascending(m => m.UsuarioId),
                    new CreateIndexOptions { Unique = true })
            };
            await moods.Indexes.CreateManyAsync(moodsIndexes);
        }
    }
}
