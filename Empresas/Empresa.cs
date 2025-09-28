
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MoodPlusAPI.Empresas
{
    [BsonIgnoreExtraElements]
    public class Empresa
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nome { get; set; }
        public string CNPJ { get; set; }
    }
}
