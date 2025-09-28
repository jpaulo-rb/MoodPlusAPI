using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Empresas;
using MoodPlusAPI.Usuarios;
using MoodPlusAPI.Usuarios.DTOs;
using MoodPlusAPI.Utils;

namespace UsuarioPlusAPI.Usuarios
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly TokenJwt _tokenJwt;
        private readonly EmpresaRepository _empresaRepository;

        public UsuarioService(UsuarioRepository usuarioRepository, TokenJwt tokenJwt, EmpresaRepository empresaRepository)
        {
            _empresaRepository = empresaRepository;
            _usuarioRepository = usuarioRepository;
            _tokenJwt = tokenJwt;
        }

        public async Task<Result<UsuarioTokenDTO>> Login(UsuarioLoginDTO usuarioLogin)
        {
            var usuario = await _usuarioRepository.FindByEmail(usuarioLogin.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(usuarioLogin.Senha, usuario.SenhaCriptografada))
            {
                return Result<UsuarioTokenDTO>.Failure(ApiError.Unauthorized());
            }

            var tokenDTO = _tokenJwt.GerarTokenDTO(usuario);

            return Result<UsuarioTokenDTO>.Success(tokenDTO);
        }

        public async Task<Result<UsuarioViewDTO>> BuscarPorId(ObjectId id)
        {
            var usuario = await _usuarioRepository.FindByIdAsync(id);

            if (usuario == null)
            {
                return Result<UsuarioViewDTO>.Failure(ApiError.NotFound("Usuário não encontrado"));
            }

            var usuarioDto = ParaUsuarioView(usuario);

            return Result<UsuarioViewDTO>.Success(usuarioDto);
        }

        public async Task<Result<List<UsuarioViewDTO>>> BuscarTodos()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();

            var usuariosView = new List<UsuarioViewDTO>();

            usuarios.ForEach(usuario =>
            {
                usuariosView.Add(ParaUsuarioView(usuario));
            });

            return Result<List<UsuarioViewDTO>>.Success(usuariosView);
        }

        public async Task<Result<UsuarioViewDTO>> CriarUsuario(UsuarioCreateDTO usuarioCreate)
        {
            try
            {
                if (!await EmpresaExiste(usuarioCreate.EmpresaId))
                {
                    return Result<UsuarioViewDTO>.Failure(ApiError.NotFound("EmpresaId não encontrado"));
                }

                var usuario = await _usuarioRepository.InsertAsync(ParaUsuario(usuarioCreate));

                return Result<UsuarioViewDTO>.Success(ParaUsuarioView(usuario));
            }
            catch (MongoWriteException E)
            {
                if (E.WriteError.Code == 11000)
                {
                    return Result<UsuarioViewDTO>.Failure(ApiError.Conflict("Usuário já existe"));
                }

                return Result<UsuarioViewDTO>.Failure(ApiError.Internal());
            }
        }

        private async Task<bool> EmpresaExiste(string? id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var exists = await _empresaRepository.AnyAsync(new ObjectId(id));

                if (!exists)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<Result> DeletarUsuario(ObjectId id)
        {
            var deleted = await _usuarioRepository.DeleteAsync(id);

            if (!deleted)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        public async Task<Result> AtualizarUsuario(ObjectId id, UsuarioUpdateDTO usuarioUpdate)
        {
            if (!await EmpresaExiste(usuarioUpdate.EmpresaId))
            {
                return Result<UsuarioViewDTO>.Failure(ApiError.NotFound("EmpresaId não encontrado"));
            }

            var updates = DicionarioUpdate(usuarioUpdate);

            var updated = await _usuarioRepository.UpdateAsync(id, updates);

            if (!updated)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        private static UsuarioViewDTO ParaUsuarioView(Usuario usuario)
        {
            return new UsuarioViewDTO()
            {
                Id = usuario.Id,
                Email = usuario.Email,
                EmpresaId = usuario.EmpresaId,
                Nome = usuario.Nome,
                Regra = usuario.Regra
            };
        }
        private static Usuario ParaUsuario(UsuarioCreateDTO usuarioCreate)
        {
            return new Usuario()
            {
                Id = null,
                Nome = usuarioCreate.Nome,
                Email = usuarioCreate.Email,
                EmpresaId = usuarioCreate.EmpresaId,
                Regra = usuarioCreate.Regra,
                SenhaCriptografada = BCrypt.Net.BCrypt.HashPassword(usuarioCreate.Senha)
            };
        }

        private static Usuario ParaUsuario(UsuarioUpdateDTO usuarioUpdate)
        {
            var usuario = new Usuario();

            usuario.Id = usuarioUpdate.Id;

            if (usuarioUpdate.Nome != null) usuario.Nome = usuarioUpdate.Nome;
            if (usuarioUpdate.Email != null) usuario.Email = usuarioUpdate.Email;
            if (usuarioUpdate.EmpresaId != null) usuario.EmpresaId = usuarioUpdate.EmpresaId;
            if (usuarioUpdate.Regra != null) usuario.Regra = usuarioUpdate.Regra.Value;
            if (usuarioUpdate.Senha != null) usuario.SenhaCriptografada = BCrypt.Net.BCrypt.HashPassword(usuarioUpdate.Senha);

            return usuario;
        }

        private static Dictionary<string, JsonElement> DicionarioUpdate(UsuarioUpdateDTO usuarioUpdate)
        {
            var usuario = ParaUsuario(usuarioUpdate);

            Dictionary<string, JsonElement> updates = [];

            foreach (var prop in usuario.GetType().GetProperties())
            {
                var valor = prop.GetValue(usuario);

                if (valor != null)
                {
                    JsonElement element;

                    if (prop.PropertyType.IsEnum)
                    {
                        // Converte enum para string
                        element = JsonSerializer.SerializeToElement(valor.ToString());
                    }
                    else
                    {
                        element = JsonSerializer.SerializeToElement(valor);
                    }

                    updates.Add(prop.Name, element);
                }
            }

            return updates;
        }

        public async Task<Result<UsuarioViewDTO>> CriarPrimeiroAdmin(UsuarioCreateDTO usuarioCreate)
        {
            if (await _usuarioRepository.RoleExists(usuarioCreate.Regra))
            {
                return Result<UsuarioViewDTO>.Failure(ApiError.Conflict("Admin já cadastrado"));
            }

            usuarioCreate.Regra = Regra.Admin;
            usuarioCreate.EmpresaId = null;

            return await CriarUsuario(usuarioCreate);
        }

        public async Task<Result> SubstituirUsuario(ObjectId id, UsuarioCreateDTO usuarioCreate)
        {
            var usuario = ParaUsuario(usuarioCreate);

            usuario.Id = id.ToString();

            var replaced = await _usuarioRepository.ReplaceAsync(id, usuario);

            if (!replaced)
            {
                return Result.Failure(ApiError.Conflict());
            }

            return Result.Success();
        }
    }
}
