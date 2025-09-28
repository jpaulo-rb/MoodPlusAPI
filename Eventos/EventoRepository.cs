using MongoDB.Driver;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;

namespace MoodPlusAPI.Eventos
{
    public class EventoRepository : CoreRepository<Evento>
    {
        public EventoRepository(MongoDbContext dbContext, RequestContext requestContext) : base(dbContext, requestContext)
        {
        }

        public async Task<List<Evento>> GetEventoByTime(DateTime start, DateTime end)
        {
            var filter = Builders<Evento>.Filter.Gte(e => e.Inicio, start) &
                         Builders<Evento>.Filter.Lte(e => e.Inicio, end) &
                         EmpresaFilter();

            var eventos = await _collection.Find(filter).ToListAsync();
            return eventos;
        }
    }
}
