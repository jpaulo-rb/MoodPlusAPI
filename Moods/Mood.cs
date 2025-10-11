using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MoodPlusAPI.Moods
{
    [BsonIgnoreExtraElements]
    public class Mood
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string EmpresaId { get; set; }

        public List<MoodDiario> MoodDiarios { get; set; } = [];
    }

    [BsonIgnoreExtraElements]
    public class MoodDiario
    {
        public DateOnly? Dia { get; set; }
        public string Humor { get; set; }
        public string Sentimento { get; set; }
        public string Influencia { get; set; }
        public string Sono { get; set; }
        public string RelacaoLiderenca { get; set; }
        public string ImpactoTrabalho { get; set; }
    }
}
