namespace MoodPlusAPI.Moods.DTOs
{
    public class MoodResumoDTO
    {
        public string Pergunta { get; set; }

        public List<MoodResumoResposta> MoodResumoRespostas { get; set; }
    }

    public class MoodResumoResposta
    {
        public string Resposta { get; set; }
        public int Quantidade { get; set; }
    }
}
