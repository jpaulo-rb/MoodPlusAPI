using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Empresas.DTOs
{
    public class EmpresaViewDTO
    {
        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "Id inválido")]
        public string Id { get; set; }
        public string Nome { get; set; }
        public string CNPJ { get; set; }
    }
}
