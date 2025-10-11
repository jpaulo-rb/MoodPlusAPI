
using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Empresas.DTOs
{
    public class EmpresaCreateDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "CNPJ é obrigatório")]
        public string CNPJ { get; set; }
    }
}
