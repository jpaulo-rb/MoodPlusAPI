using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.Usuarios.DTOs;
using MoodPlusAPI.Utils;
using UsuarioPlusAPI.Usuarios;

namespace MoodPlusAPI.Usuarios
{
    [ApiVersion(1.0, Deprecated = true)]
    [ApiVersion(1.1)]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class UsuarioController : CoreController
    {
        private readonly UsuarioService _usuarioService;
        private readonly RequestContext _requestContext;
        public UsuarioController(UsuarioService usuarioService, RequestContext requestContext)
        {
            _usuarioService = usuarioService;
            _requestContext = requestContext;
        }

        [MapToApiVersion(1.1)]
        [HttpGet("{id}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<UsuarioViewDTO>> BuscarPorId(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"Id {id} inválido, favor informar um ObjectId"));
            }

            var result = await _usuarioService.BuscarPorId(objectId);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpGet]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult<List<UsuarioViewDTO>>> ListarTodos()
        {
            var result = await _usuarioService.BuscarTodos();
            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPost]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult<UsuarioViewDTO>> CriarUsuario([FromBody] UsuarioCreateDTO usuarioCreate)
        {
            if (usuarioCreate.Regra == Regra.Admin && _requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                return ProblemResponse(ApiError.Unauthorized("Usuário não autorizado para definir a regra " + usuarioCreate.Regra.ToString()));
            }

            if (_requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                usuarioCreate.EmpresaId = _requestContext.UsuarioSessao.EmpresaId;
            }

            if (usuarioCreate.Regra != Regra.Admin && string.IsNullOrWhiteSpace(usuarioCreate.EmpresaId))
            {
                return ProblemResponse(ApiError.BadRequest("Cadastro de gerente ou usuário é obrigatório informar a empresa"));
            }

            var result = await _usuarioService.CriarUsuario(usuarioCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return StatusCode(201, result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPost("admin")]
        [AllowAnonymous]
        public async Task<ActionResult<UsuarioViewDTO>> CriarPrimeiroAdmin([FromBody] UsuarioCreateDTO usuarioCreate)
        {
            var result = await _usuarioService.CriarPrimeiroAdmin(usuarioCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return StatusCode(201, result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpDelete("{id}")]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult> DeletarUsuario(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"Id {id} inválido, favor informar um ObjectId"));
            }

            var result = await _usuarioService.DeletarUsuario(objectId);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }

        [MapToApiVersion(1.1)]
        [HttpPatch("{id}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult> AtualizarUsuario(string id, [FromBody] UsuarioUpdateDTO usuarioUpdate)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"ID '{id}' inválido. Favor informar um ObjectId válido."));
            }

            if (id != usuarioUpdate.Id)
            {
                return ProblemResponse(ApiError.BadRequest("Favor informar o mesmo ID no corpo e na URL."));
            }

            if (usuarioUpdate.Id != _requestContext.UsuarioSessao.Id && _requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                return ProblemResponse(ApiError.Unauthorized("Somente o próprio usuário pode alterar seus dados."));
            }

            if (usuarioUpdate.Regra == Regra.Admin && _requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                return ProblemResponse(ApiError.Unauthorized("Somente administrador pode utilizar essa regra."));
            }

            if (!string.IsNullOrWhiteSpace(usuarioUpdate.EmpresaId) && usuarioUpdate.EmpresaId != _requestContext.UsuarioSessao.EmpresaId && _requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                return ProblemResponse(ApiError.Unauthorized("Somente administrador pode alterar a empresa do usuário."));
            }

            var result = await _usuarioService.AtualizarUsuario(objectId, usuarioUpdate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }

        [MapToApiVersion(1.1)]
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<UsuarioTokenDTO>> LoginUsuario([FromBody] UsuarioLoginDTO usuarioLogin)
        {
            var result = await _usuarioService.Login(usuarioLogin);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.0)]
        [HttpPut("{id}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult> SubstituirUsuario(string id, [FromBody] UsuarioCreateDTO usuarioCreate)
        {
            if (usuarioCreate.Regra == Regra.Admin && _requestContext.UsuarioSessao.Regra != Regra.Admin)
            {
                return ProblemResponse(ApiError.Unauthorized("Uusuário não autorizado para definir a regra " + usuarioCreate.Regra.ToString()));
            }

            if (usuarioCreate.Regra == Regra.Gerente && _requestContext.UsuarioSessao.Regra == Regra.Usuario)
            {
                return ProblemResponse(ApiError.Unauthorized("Uusuário não autorizado para definir a regra " + usuarioCreate.Regra.ToString()));
            }

            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"Id {id} inválido, favor informar um ObjectId"));
            }

            var result = await _usuarioService.SubstituirUsuario(objectId, usuarioCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }
    }
}
