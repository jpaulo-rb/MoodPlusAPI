using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Eventos.DTOs
{
    public class EventoCreateDTO
    {
        [Required]
        public DateTime Inicio { get; set; }
        [Required]
        public DateTime Fim { get; set; }
        [Required]
        public string Titulo { get; set; }
        [Required]
        public string Descricao { get; set; }
    }
}
