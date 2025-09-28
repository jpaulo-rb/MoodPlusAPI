using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Moods.DTOs
{
    public class MoodDiarioCreateDTO
    {
        [Required(ErrorMessage = "Humor é obrigatório")]
        public string Humor { get; set; }

        [Required(ErrorMessage = "Sentimento é obrigatório")]
        public string Sentimento { get; set; }

        [Required(ErrorMessage = "Influência é obrigatório")]
        public string Influencia { get; set; }

        [Required(ErrorMessage = "Sono é obrigatório")]
        public string Sono { get; set; }

        [Required(ErrorMessage = "Relação liderença é obrigatório")]
        public string RelacaoLiderenca { get; set; }

        [Required(ErrorMessage = "Impacto Trabalho é obrigatório")]
        public string ImpactoTrabalho { get; set; }

        public DateOnly Dia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
