using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Usuarios.DTOs
{
    public class UsuarioViewDTO
    {
        [RegularExpression(@"^[0-9a-fA-F]{24}$")]
        public string Id { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }

        [RegularExpression(@"^[0-9a-fA-F]{24}$")]
        public string EmpresaId { get; set; }
        public Regra Regra { get; set; }
    }
}
