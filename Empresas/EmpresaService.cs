using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MoodPlusAPI.Empresas.DTOs;
using MoodPlusAPI.Utils;

namespace MoodPlusAPI.Empresas
{
    public class EmpresaService
    {
        private readonly EmpresaRepository _empresaRepository;

        public EmpresaService(EmpresaRepository empresaRepository)
        {
            _empresaRepository = empresaRepository;
        }

        public async Task<Result<EmpresaViewDTO>> BuscarPorId(ObjectId id)
        {
            var empresa = await _empresaRepository.FindByIdAsync(id);

            if (empresa == null)
            {
                return Result<EmpresaViewDTO>.Failure(ApiError.NotFound("Empresa não encontrado"));
            }

            var empresaDto = ParaEmpresaView(empresa);

            return Result<EmpresaViewDTO>.Success(empresaDto);
        }

        public async Task<Result<List<EmpresaViewDTO>>> BuscarTodos()
        {
            var empresas = await _empresaRepository.GetAllAsync();

            var empresasView = new List<EmpresaViewDTO>();

            empresas.ForEach(empresa =>
            {
                empresasView.Add(ParaEmpresaView(empresa));
            });

            return Result<List<EmpresaViewDTO>>.Success(empresasView);
        }

        public async Task<Result<EmpresaViewDTO>> CriarEmpresa(EmpresaCreateDTO EmpresaCreate)
        {
            try
            {
                var empresa = await _empresaRepository.InsertAsync(ParaEmpresa(EmpresaCreate));

                return Result<EmpresaViewDTO>.Success(ParaEmpresaView(empresa));
            }
            catch (MongoWriteException E)
            {
                if (E.WriteError.Code == 11000)
                {
                    return Result<EmpresaViewDTO>.Failure(ApiError.Conflict("Empresa já existe"));
                }

                return Result<EmpresaViewDTO>.Failure(ApiError.Internal());
            }
        }

        public async Task<Result> DeletarEmpresa(ObjectId id)
        {
            var deleted = await _empresaRepository.DeleteAsync(id);

            if (!deleted)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        public async Task<Result> SubstituirEmpresa(ObjectId id, EmpresaCreateDTO empresaCreate)
        {
            var empresa = ParaEmpresa(empresaCreate);

            var substituted = await _empresaRepository.ReplaceAsync(id, empresa);

            if (!substituted)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        public async Task<Result> AtualizarEmpresa(ObjectId id, EmpresaUpdateDTO empresaUpdate)
        {
            var updates = DicionarioUpdate(empresaUpdate);

            var updated = await _empresaRepository.UpdateAsync(id, updates);

            if (!updated)
            {
                return Result.Failure(ApiError.NotFound());
            }

            return Result.Success();
        }

        private static EmpresaViewDTO ParaEmpresaView(Empresa empresa)
        {
            return new EmpresaViewDTO()
            {
                Id = empresa.Id,
                CNPJ = empresa.CNPJ,
                Nome = empresa.Nome
            };
        }
        private static Empresa ParaEmpresa(EmpresaCreateDTO empresaCreate)
        {
            return new Empresa()
            {
                Id = null,
                Nome = empresaCreate.Nome,
                CNPJ = empresaCreate.CNPJ
            };
        }

        private static Dictionary<string, JsonElement> DicionarioUpdate(EmpresaUpdateDTO EmpresaUpdate)
        {
            Dictionary<string, JsonElement> updates = [];

            foreach (var prop in EmpresaUpdate.GetType().GetProperties())
            {
                var valor = prop.GetValue(EmpresaUpdate);

                if (valor != null)
                {
                    var element = JsonSerializer.SerializeToElement(valor);

                    updates.Add(prop.Name, element);
                }
            }

            return updates;
        }
    }
}
