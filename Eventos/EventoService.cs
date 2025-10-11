using System.Text.Json;
using MongoDB.Bson;
using MoodPlusAPI.Eventos.DTOs;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Eventos
{
    public class EventoService
    {
        private readonly EventoRepository _eventoRepository;
        private readonly RequestContext _requestContext;
        public EventoService(EventoRepository eventoRepository, RequestContext requestContext)
        {
            _eventoRepository = eventoRepository;
            _requestContext = requestContext;
        }

        public async Task<Result<List<Evento>>> BuscarPorPeriodo(DateOnly dataInicio, DateOnly dataFinal)
        {
            var startDateTime = dataInicio.ToDateTime(TimeOnly.MinValue);
            var endDateTime = dataFinal.ToDateTime(TimeOnly.MaxValue);

            var eventos = await _eventoRepository.GetEventoByTime(startDateTime, endDateTime);
            return Result<List<Evento>>.Success(eventos);
        }

        public async Task<Result<Evento>> AdicionarEvento(EventoCreateDTO eventoCreate)
        {
            var evento = await _eventoRepository.InsertAsync(ParaEvento(eventoCreate));

            if (evento == null)
            {
                return Result<Evento>.Failure(ApiError.Conflict());
            }

            return Result<Evento>.Success(evento);
        }

        public async Task<Result> AtualizarEvento(ObjectId id, EventoUpdateDTO eventoUpdate)
        {
            var updates = DicionarioUpdate(eventoUpdate);

            var updated = await _eventoRepository.UpdateAsync(id, updates);

            if (!updated)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        public async Task<Result> DeletarEvento(ObjectId id)
        {
            var deleted = await _eventoRepository.DeleteAsync(id);

            if (!deleted)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        private Evento ParaEvento(EventoCreateDTO eventoCreate)
        {
            var empresaId = _requestContext.UsuarioSessao.EmpresaId
                ?? throw new ArgumentException("Usuário precisa estar vinculado a empresa para prosseguir");

            return new Evento()
            {
                EmpresaId = empresaId,
                Descricao = eventoCreate.Descricao,
                Fim = eventoCreate.Fim,
                Inicio = eventoCreate.Inicio,
                Titulo = eventoCreate.Titulo
            };
        }

        private Evento ParaEvento(EventoUpdateDTO eventoUpdate)
        {
            var empresaId = _requestContext.UsuarioSessao.EmpresaId
                ?? throw new ArgumentException("Usuário precisa estar vinculado a empresa para prosseguir");

            var evento = new Evento()
            {
                EmpresaId = empresaId,
            };

            if (eventoUpdate.Inicio != null) evento.Inicio = eventoUpdate.Inicio.Value;
            if (eventoUpdate.Descricao != null) evento.Descricao = eventoUpdate.Descricao;
            if (eventoUpdate.Fim != null) evento.Fim = eventoUpdate.Fim.Value;
            if (eventoUpdate.Titulo != null) evento.Titulo = eventoUpdate.Titulo;

            return evento;
        }

        private Dictionary<string, JsonElement> DicionarioUpdate(EventoUpdateDTO eventoUpdate)
        {
            var evento = ParaEvento(eventoUpdate);

            Dictionary<string, JsonElement> updates = [];

            foreach (var prop in evento.GetType().GetProperties())
            {
                var valor = prop.GetValue(evento);

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
    }
}
