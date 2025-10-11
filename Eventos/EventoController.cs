using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MoodPlusAPI.Core;
using MoodPlusAPI.Eventos.DTOs;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Eventos
{
    [ApiVersion(1.1)]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class EventoController : CoreController
    {
        private readonly EventoService _eventoService;
        public EventoController(EventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [MapToApiVersion(1.1)]
        [HttpGet]
        [Authorize(Policy = "Usuario")]
        public async Task<ActionResult<List<Evento>>> BuscarPorData([FromQuery] DateOnly dataInicio, [FromQuery] DateOnly dataFinal)
        {
            if (dataInicio > dataFinal)
            {
                return ProblemResponse(ApiError.BadRequest("Data inicial maior que data final"));
            }

            var result = await _eventoService.BuscarPorPeriodo(dataInicio, dataFinal);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return Ok(result.Value);
        }


        [MapToApiVersion(1.1)]
        [HttpPost]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult<Evento>> CriarEvento([FromBody] EventoCreateDTO eventoCreate)
        {
            if (eventoCreate.Inicio > eventoCreate.Fim)
            {
                return ProblemResponse(ApiError.BadRequest("Data inicial maior que data final"));
            }

            var result = await _eventoService.AdicionarEvento(eventoCreate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return StatusCode(201, result.Value);
        }

        [MapToApiVersion(1.1)]
        [HttpPatch("{id}")]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult> AtualizarEvento(string id, [FromBody] EventoUpdateDTO eventoUpdate)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"ID '{id}' inválido. Favor informar um ObjectId válido."));
            }

            if (id != eventoUpdate.Id)
            {
                return ProblemResponse(ApiError.BadRequest("Favor informar o mesmo ID no corpo e na URL."));
            }

            if (eventoUpdate.Inicio.HasValue && eventoUpdate.Fim.HasValue && eventoUpdate.Inicio.Value > eventoUpdate.Fim.Value)
            {
                return ProblemResponse(ApiError.BadRequest("Data inicial maior que data final"));
            }

            var result = await _eventoService.AtualizarEvento(objectId, eventoUpdate);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }

        [MapToApiVersion(1.1)]
        [HttpDelete("{id}")]
        [Authorize(Policy = "Gerente")]
        public async Task<ActionResult> DeletarMood(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return ProblemResponse(ApiError.BadRequest($"ID '{id}' inválido. Favor informar um ObjectId válido."));
            }

            var result = await _eventoService.DeletarEvento(objectId);

            if (!result.IsSuccess)
            {
                return ProblemResponse(result.ApiError);
            }

            return NoContent();
        }
    }
}