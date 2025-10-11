using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Core;
using MoodPlusAPI.Extensions;
using MoodPlusAPI.MongoDb;
using MoodPlusAPI.Moods.DTOs;

namespace MoodPlusAPI.Moods
{
    public class MoodRepository : CoreRepository<Mood>
    {
        public MoodRepository(MongoDbContext dbContext, RequestContext requestContext) : base(dbContext, requestContext)
        {
        }

        public async Task<Mood> AddMoodDiario(Mood mood)
        {
            if (mood.MoodDiarios.Count == 0)
            {
                throw new ArgumentException("MoodDiario não pode ser nulo");
            }

            var filter = Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), mood.UsuarioId) & EmpresaFilter();
            var update = Builders<Mood>.Update.Push(m => m.MoodDiarios, mood.MoodDiarios.First());

            var updatedMood = await _collection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<Mood>
                {
                    ReturnDocument = ReturnDocument.After
                }
            );

            if (updatedMood != null)
            {
                return updatedMood;
            }

            await _collection.InsertOneAsync(mood);
            return mood;
        }

        public async Task<bool> DayExists(ObjectId userId, DateOnly date)
        {
            var filter = Builders<Mood>.Filter.And(
                Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), userId),
                EmpresaFilter(),
                Builders<Mood>.Filter.ElemMatch(m => m.MoodDiarios, d => d.Dia == date)
            );
            return await _collection.Find(filter).AnyAsync();
        }

        public async Task<Mood> FindMoodByUser(ObjectId userId)
        {
            var filter = Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), userId) & EmpresaFilter();
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<MoodResumoDTO>> CountMoodByUser(ObjectId userId)
        {
            var mood = await FindMoodByUser(userId);

            if (mood == null)
            {
                return [];
            }

            var moodsDiarios = mood.MoodDiarios;

            var sentimento = CountAnswers("Sentimento", moodsDiarios.Select(x => x.Sentimento).ToList());
            var influencia = CountAnswers("Influência", moodsDiarios.Select(x => x.Influencia).ToList());
            var relacaoLideranca = CountAnswers("Relação Liderença", moodsDiarios.Select(x => x.RelacaoLiderenca).ToList());
            var humor = CountAnswers("Humor", moodsDiarios.Select(x => x.Humor).ToList());
            var impactoTrabalho = CountAnswers("Impacto Trabalho", moodsDiarios.Select(x => x.ImpactoTrabalho).ToList());
            var sono = CountAnswers("Sono", moodsDiarios.Select(x => x.Sono).ToList());

            return [
                sentimento,
                influencia,
                relacaoLideranca,
                humor,
                impactoTrabalho,
                sono
            ];
        }

        private static MoodResumoDTO CountAnswers(string pergunta, List<string> respostas)
        {
            Dictionary<string, int> analise = [];

            foreach (string resposta in respostas)
            {
                if (!analise.ContainsKey(resposta))
                {
                    analise[resposta] = 0;
                }

                analise[resposta]++;
            }

            var moodResumo = new MoodResumoDTO
            {
                Pergunta = pergunta
            };

            var moodResumosRespostas = new List<MoodResumoResposta>();

            foreach (var kvp in analise)
            {
                moodResumosRespostas.Add(new MoodResumoResposta
                {
                    Resposta = kvp.Key,
                    Quantidade = kvp.Value,
                });
            }

            moodResumo.MoodResumoRespostas = moodResumosRespostas;

            return moodResumo;
        }

        public async Task<bool> UpdateMoodDiario(ObjectId id, DateOnly dia, Dictionary<string, JsonElement> updates)
        {
            var updateDef = new List<UpdateDefinition<Mood>>();

            foreach (var kvp in updates)
            {
                object value = kvp.Value.ValueKind switch
                {
                    JsonValueKind.String => kvp.Value.GetString(),
                    JsonValueKind.Number => kvp.Value.GetDecimal(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => kvp.Value.ToString()
                };

                // adiciona $ para indicar que é dentro do array
                updateDef.Add(Builders<Mood>.Update.Set($"MoodDiarios.$.{kvp.Key}", value));
            }

            var combinedUpdate = Builders<Mood>.Update.Combine(updateDef);

            // filtro pelo documento e pelo elemento do array
            var filter = Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), id)
                & EmpresaFilter() & Builders<Mood>.Filter.Eq("MoodDiarios.Dia", dia);

            var result = await _collection.UpdateOneAsync(filter, combinedUpdate);

            return result.MatchedCount > 0;
        }

        public async Task<List<MoodDiario>> GetMoodDiariosByMonth(ObjectId userId, int month)
        {
            var filter = Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), userId) & EmpresaFilter();
            var mood = await _collection.Find(filter).FirstOrDefaultAsync();

            if (mood == null)
            {
                return [];
            }

            return mood.MoodDiarios.Where(m => m.Dia!.Value.Month == month).ToList();
        }

        public async Task<bool> DeleteMoodDiario(ObjectId userId, DateOnly dia)
        {
            var filter = Builders<Mood>.Filter.And(
                Builders<Mood>.Filter.Eq(nameof(Mood.UsuarioId), userId),
                EmpresaFilter()
            );

            var update = Builders<Mood>.Update.PullFilter(
                m => m.MoodDiarios,
                d => d.Dia == dia
            );

            var result = await _collection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }
    }
}
