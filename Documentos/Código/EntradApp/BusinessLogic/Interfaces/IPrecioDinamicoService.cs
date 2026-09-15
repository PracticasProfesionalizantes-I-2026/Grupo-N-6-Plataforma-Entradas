using EntradApp.Shared.Common;

namespace EntradApp.BusinessLogic.Interfaces;

public interface IPrecioDinamicoService
{
    Task<Result> EvaluarYAplicarIncrementoAsync();
}