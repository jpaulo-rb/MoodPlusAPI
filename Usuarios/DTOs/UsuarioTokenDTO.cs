namespace MoodPlusAPI.Usuarios.DTOs
{
    public class UsuarioTokenDTO
    {
        public string Type { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
