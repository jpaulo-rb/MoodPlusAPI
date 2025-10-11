using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Eventos.DTOs
{
    public class EventoUpdateDTO
    {
        [Required(ErrorMessage = "Id é obrigatório")]
        [RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "Id inválido")]
        public string Id { get; set; }

        public DateTime? Inicio { get; set; }
        public DateTime? Fim { get; set; }
        public string? Titulo { get; set; }
        public string? Descricao { get; set; }
    }
}
