using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.Moods.DTOs;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Moods
{
    public class MoodService
    {
        private readonly MoodRepository _moodRepository;
        private readonly RequestContext _requestContext;

        public MoodService(MoodRepository moodRepository, RequestContext requestContext)
        {
            _moodRepository = moodRepository;
            _requestContext = requestContext;
        }

        public async Task<Result<Mood>> BuscarPorUsuarioId(ObjectId id)
        {
            var mood = await _moodRepository.FindMoodByUser(id);

            if (mood == null)
            {
                return Result<Mood>.Failure(ApiError.NotFound("Mood não encontrado"));
            }

            return Result<Mood>.Success(mood);
        }

        public async Task<Result<Mood>> AdicionarMood(MoodDiarioCreateDTO moodCreate)
        {
            try
            {
                if (await _moodRepository.DayExists(new ObjectId(_requestContext.UsuarioSessao.Id), moodCreate.Dia))
                {
                    return Result<Mood>.Failure(ApiError.Conflict("Usuário já possuí mood cadastrado nesse dia"));
                }

                var mood = await _moodRepository.AddMoodDiario(ParaMood(moodCreate));

                return Result<Mood>.Success(mood);
            }
            catch (MongoWriteException E)
            {
                if (E.WriteError.Code == 11000)
                {
                    return Result<Mood>.Failure(ApiError.Conflict("Mood já existe"));
                }

                return Result<Mood>.Failure(ApiError.Internal());
            }
            catch (ArgumentException)
            {
                return Result<Mood>.Failure(ApiError.BadRequest());
            }
        }

        public async Task<Result> DeletarMood(DateOnly data)
        {
            var id = new ObjectId(_requestContext.UsuarioSessao.Id);

            var deleted = await _moodRepository.DeleteMoodDiario(id, data);

            if (!deleted)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        public async Task<Result> AtualizarMood(ObjectId id, DateOnly data, MoodDiarioUpdateDTO moodDiarioUpdate)
        {
            try
            {
                var updates = DicionarioUpdate(moodDiarioUpdate);

                var updated = await _moodRepository.UpdateMoodDiario(id, data, updates);

                if (!updated)
                {
                    return Result.Failure(ApiError.NotFound());
                }

                return Result.Success();
            }
            catch (ArgumentException)
            {
                return Result.Failure(ApiError.BadRequest());
            }
        }

        public async Task<Result<List<MoodResumoDTO>>> RelatorioRespostas(ObjectId usuarioId)
        {
            var resumos = await _moodRepository.CountMoodByUser(usuarioId);

            return Result<List<MoodResumoDTO>>.Success(resumos);
        }

        public async Task<Result<Dictionary<DateOnly, string>>> RelatorioHumorMensal(ObjectId usuarioId, int mes)
        {
            var moodsDiarios = await _moodRepository.GetMoodDiariosByMonth(usuarioId, mes);

            return Result<Dictionary<DateOnly, string>>.Success(moodsDiarios.DistinctBy(x => x.Dia!.Value).ToDictionary(x => x.Dia!.Value, x => x.Humor));
        }

        private Mood ParaMood(MoodDiarioCreateDTO moodCreate)
        {
            return new Mood()
            {
                EmpresaId = _requestContext.UsuarioSessao.EmpresaId ?? throw new ArgumentException("EmpresaId é obrigatório"),
                UsuarioId = _requestContext.UsuarioSessao.Id,
                MoodDiarios = [
                    new MoodDiario()
                    {
                        Dia = moodCreate.Dia,
                        Humor = moodCreate.Humor,
                        ImpactoTrabalho = moodCreate.ImpactoTrabalho,
                        Influencia = moodCreate.Influencia,
                        RelacaoLiderenca = moodCreate.RelacaoLiderenca,
                        Sentimento = moodCreate.Sentimento,
                        Sono = moodCreate.Sono
                    }
                ]
            };
        }

        private Mood ParaMood(MoodDiarioUpdateDTO moodDiarioUpdate)
        {
            var moodDiario = new MoodDiario();

            if (moodDiarioUpdate.ImpactoTrabalho != null) moodDiario.ImpactoTrabalho = moodDiarioUpdate.ImpactoTrabalho;
            if (moodDiarioUpdate.RelacaoLiderenca != null) moodDiario.RelacaoLiderenca = moodDiarioUpdate.RelacaoLiderenca;
            if (moodDiarioUpdate.Influencia != null) moodDiario.Influencia = moodDiarioUpdate.Influencia;
            if (moodDiarioUpdate.Humor != null) moodDiario.Humor = moodDiarioUpdate.Humor;
            if (moodDiarioUpdate.Sentimento != null) moodDiario.Sentimento = moodDiarioUpdate.Sentimento;
            if (moodDiarioUpdate.Sono != null) moodDiario.Sono = moodDiarioUpdate.Sono;

            List<MoodDiario> moodsDiario = [moodDiario];

            return new Mood()
            {
                EmpresaId = _requestContext.UsuarioSessao.EmpresaId ?? throw new ArgumentException("EmpresaId é obrigatório"),
                UsuarioId = _requestContext.UsuarioSessao.Id,
                MoodDiarios = moodsDiario
            };
        }

        private static Dictionary<string, JsonElement> DicionarioUpdate(MoodDiarioUpdateDTO moodUpdate)
        {
            Dictionary<string, JsonElement> updates = [];

            foreach (var prop in moodUpdate.GetType().GetProperties())
            {
                var valor = prop.GetValue(moodUpdate);

                if (valor != null)
                {
                    JsonElement element;

                    if (prop.PropertyType.IsEnum)
                    {
                        // Converte enum para string
                        element = JsonSerializer.SerializeToElement(valor.ToString());
                    }
                    else
                    {
                        element = JsonSerializer.SerializeToElement(valor);
                    }

                    updates.Add(prop.Name, element);
                }
            }

            return updates;
        }
    }
}
