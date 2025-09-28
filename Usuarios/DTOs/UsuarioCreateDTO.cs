using System.ComponentModel.DataAnnotations;
using MoodPlusAPI.Validations;

namespace MoodPlusAPI.Usuarios.DTOs
{
    public class UsuarioCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatório")]
        public string Senha { get; set; }

        [NotEmptyIfNotNull(ErrorMessage = "EmpresaId não pode ser vazio")]
        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "EmpresaId inválido")]
        public string? EmpresaId { get; set; }

        public Regra Regra { get; set; }
    }
}
