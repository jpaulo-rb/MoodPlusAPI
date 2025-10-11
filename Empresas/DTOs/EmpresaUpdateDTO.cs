using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Empresas.DTOs
{
    public class EmpresaUpdateDTO
    {
        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "Id inválido")]
        [Required(ErrorMessage = "Id é obrigatório")]
        public string Id { get; set; }
        public string? Nome { get; set; }
        public string? NomeFantasia { get; set; }
        public string? CnpjCpf { get; set; }
    }
}
