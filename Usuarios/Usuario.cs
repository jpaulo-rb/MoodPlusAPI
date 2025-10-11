using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MoodPlusAPI.Usuarios
{
    [BsonIgnoreExtraElements]
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string SenhaCriptografada { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? EmpresaId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Regra Regra { get; set; }
    }

    public enum Regra
    {
        Admin,
        Gerente,
        Usuario
    }
}
