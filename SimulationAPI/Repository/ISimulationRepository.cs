namespace SimulationAPI.Repository;

public interface ISimulationRepository<T, TOut>
{
  void CreateAndRun(T simulationId);
  int CheckProgress(T simulationId);
  Task<TOut?> GetResults(T simulationId);
}