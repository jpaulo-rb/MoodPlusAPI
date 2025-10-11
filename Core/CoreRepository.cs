using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Empresas;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;
using MoodPlusAPI.Usuarios;

namespace MoodPlusAPI.Core
{
    public class CoreRepository<TEntity> where TEntity : class
    {
        protected readonly IMongoCollection<TEntity> _collection;
        protected readonly RequestContext _requestContext;

        public CoreRepository(MongoDbContext dbContext, RequestContext requestContext)
        {
            _collection = dbContext.GetCollection<TEntity>(typeof(TEntity).Name);
            _requestContext = requestContext;
        }

        public async Task<TEntity?> FindByIdAsync(ObjectId id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id) & EmpresaFilter();
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            return await _collection.Find(EmpresaFilter()).ToListAsync();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> ReplaceAsync(ObjectId id, TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id) & EmpresaFilter();
            var result = await _collection.ReplaceOneAsync(filter, entity);

            return result.MatchedCount > 0;
        }

        public async Task<bool> UpdateAsync(ObjectId id, Dictionary<string, JsonElement> updates)
        {
            var updateDef = new List<UpdateDefinition<TEntity>>();

            foreach (var kvp in updates)
            {
                object value = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value.GetString(),
                    JsonValueKind.Number => kvp.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.ToString()
                };

                updateDef.Add(Builders<TEntity>.Update.Set(kvp.Key, value));

            }

            var combinedUpdate = Builders<TEntity>.Update.Combine(updateDef);

            var filter = Builders<TEntity>.Filter.Eq("_id", id) & EmpresaFilter();

            var result = await _collection.UpdateOneAsync(filter, combinedUpdate);

            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteAsync(ObjectId id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id) & EmpresaFilter();
            var result = await _collection.DeleteOneAsync(filter);

            return result.DeletedCount > 0;
        }

        protected FilterDefinition<TEntity> EmpresaFilter()
        {
            if (typeof(TEntity) == typeof(Empresa) || _requestContext.UsuarioSessao.Regra == Regra.Admin)
            {
                return Builders<TEntity>.Filter.Empty;
            }

            ObjectId? empresaId = null;

            if (!string.IsNullOrEmpty(_requestContext.UsuarioSessao?.EmpresaId))
            {
                empresaId = new ObjectId(_requestContext.UsuarioSessao?.EmpresaId);
            }

            return Builders<TEntity>.Filter.Eq("EmpresaId", empresaId);
        }
    }
}
