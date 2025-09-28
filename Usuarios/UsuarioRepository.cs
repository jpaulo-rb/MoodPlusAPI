using MongoDB.Driver;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;

namespace MoodPlusAPI.Usuarios
{
    public class UsuarioRepository : CoreRepository<Usuario>
    {
        public UsuarioRepository(MongoDbContext dbContext, RequestContext requestContext) : base(dbContext, requestContext)
        {
        }

        public async Task<Usuario> FindByEmail(string email)
        {
            var filter = Builders<Usuario>.Filter.Eq(nameof(Usuario.Email), email);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> RoleExists(Regra role)
        {
            var filter = Builders<Usuario>.Filter.Eq(nameof(Regra), role);
            return await _collection.Find(filter).AnyAsync();
        }
    }
}
