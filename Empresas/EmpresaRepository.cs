using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;

namespace MoodPlusAPI.Empresas
{
    public class EmpresaRepository : CoreRepository<Empresa>
    {
        public EmpresaRepository(MongoDbContext dbContext, RequestContext requestContext) : base(dbContext, requestContext)
        {
        }

        public async Task<bool> AnyAsync(ObjectId id)
        {
            var filter = Builders<Empresa>.Filter.Eq(nameof(Empresa.Id), id);
            return await _collection.Find(filter).AnyAsync();
        }
    }
}
