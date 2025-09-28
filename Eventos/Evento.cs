using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MoodPlusAPI.Eventos
{
    [BsonIgnoreExtraElements]
    public class Evento
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string EmpresaId { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
    }
}
