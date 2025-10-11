using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.Moods.DTOs;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Moods
{
    [ApiVersion(1.1)]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class MoodController : CoreController
    {
        private readonly MoodService _moodService;
        private readonly RequestContext _requestContext;
        public MoodController(MoodService moodService, RequestContext requestContext)
        {
            _moodService = moodService;
            _requestContext = requestContext;
        }

        [MapToApiVersion(1.1)]
        [HttpGet]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<Mood>> BuscarPorUsuarioId()
        {
            var usuarioId = _requestContext.UsuarioSessao.Id;

            var result = await _moodService.BuscarPorUsuarioId(new ObjectId(usuarioId));

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPost]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<MoodDiario>> CriarMoodDiario([FromBody] MoodDiarioCreateDTO moodDiarioCreate)
        {
            var result = await _moodService.AdicionarMood(moodDiarioCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return StatusCode(201, result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPatch("{id}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult> AtualizarMood(string id, [FromBody] MoodDiarioUpdateDTO moodDiarioUpdate)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"ID '{id}' inválido. Favor informar um ObjectId válido."));
            }

            if (id != moodDiarioUpdate.Id)
            {
                return ProblemResponse(ApiError.BadRequest("Favor informar o mesmo ID no corpo e na URL."));
            }

            var result = await _moodService.AtualizarMood(objectId, moodDiarioUpdate.Dia, moodDiarioUpdate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }

        [MapToApiVersion(1.1)]
        [HttpGet("relatoriorespostas")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<List<MoodResumoDTO>>> RelatorioRespostas()
        {
            var usuarioId = _requestContext.UsuarioSessao.Id;

            var result = await _moodService.RelatorioRespostas(new ObjectId(usuarioId));

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpGet("respostasmensal/{mes}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult> RelatorioHumorMensal(int mes)
        {
            var usuarioId = _requestContext.UsuarioSessao.Id;

            var result = await _moodService.RelatorioHumorMensal(new ObjectId(usuarioId), mes);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpDelete("{data}")]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult> DeletarMood(DateOnly data)
        {
            var result = await _moodService.DeletarMood(data);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }
    }
}
