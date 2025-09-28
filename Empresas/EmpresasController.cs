using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MoodPlusAPI.Core;
using MoodPlusAPI.Empresas.DTOs;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Empresas
{
    [ApiVersion(1.1)]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class EmpresaController : CoreController
    {
        private readonly EmpresaService _empresaService;
        private readonly RequestContext _requestContext;
        public EmpresaController(EmpresaService empresaService, RequestContext requestContext)
        {
            _empresaService = empresaService;
            _requestContext = requestContext;
        }

        [MapToApiVersion(1.1)]
        [HttpGet("{id}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<EmpresaViewDTO>> BuscarPorId(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"Id {id} inválido, favor informar um ObjectId"));
            }

            var result = await _empresaService.BuscarPorId(objectId);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpGet]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult<List<EmpresaViewDTO>>> ListarTodos()
        {
            var result = await _empresaService.BuscarTodos();
            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult<EmpresaViewDTO>> CriarEmpresa([FromBody] EmpresaCreateDTO empresaCreate)
        {
            var result = await _empresaService.CriarEmpresa(empresaCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return StatusCode(201, result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpDelete("{id}")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult> DeletarEmpresa(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"Id {id} inválido, favor informar um ObjectId"));
            }

            var result = await _empresaService.DeletarEmpresa(objectId);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }

        [MapToApiVersion(1.1)]
        [HttpPatch("{id}")]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult> AtualizarEmpresa(string id, [FromBody] EmpresaUpdateDTO empresaUpdate)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"ID '{id}' inválido. Favor informar um ObjectId válido."));
            }

            if (id != empresaUpdate.Id)
            {
                return ProblemResponse(ApiError.BadRequest("Favor informar o mesmo ID no corpo e na URL."));
            }

            if (empresaUpdate.Id != _requestContext.UsuarioSessao.EmpresaId)
            {
                return ProblemResponse(ApiError.Unauthorized("Somente pode alterar os dados da própria empresa."));
            }

            var result = await _empresaService.AtualizarEmpresa(objectId, empresaUpdate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }
    }
}
