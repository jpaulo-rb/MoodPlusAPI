using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Usuarios.DTOs
{
    public class UsuarioUpdateDTO
    {
        [Required(ErrorMessage = "Id é obrigatório")]
        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "Id inválido")]
        public string Id { get; set; }
        public string? Nome { get; set; }

        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }
        public string? Senha { get; set; }

        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "EmpresaId inválido")]
        public string? EmpresaId { get; set; }
        public Regra? Regra { get; set; }
    }
}
