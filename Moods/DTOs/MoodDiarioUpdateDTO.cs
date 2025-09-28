using System.ComponentModel.DataAnnotations;

namespace MoodPlusAPI.Moods.DTOs
{
    public class MoodDiarioUpdateDTO
    {
        [RegularExpression(@"^[0-9a-fA-F]{24}$")]
        [Required(ErrorMessage = "Id é obrigatório")]
        public string Id { get; set; }

        public string? Humor { get; set; }

        public string? Sentimento { get; set; }

        public string? Influencia { get; set; }

        public string? Sono { get; set; }

        public string? RelacaoLiderenca { get; set; }

        public string? ImpactoTrabalho { get; set; }

        public DateOnly Dia { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
