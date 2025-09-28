using MoodPlusAPI.Usuarios;

namespace MoodPlusAPI.Extensions
{
    public class RequestContext
    {
        public UsuarioSessao UsuarioSessao { get; set; }
    }

    public class UsuarioSessao
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? EmpresaId { get; set; }
        public Regra Regra { get; set; }
    }
}
