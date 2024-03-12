
using System.Collections.Concurrent;
using SimulationAPI.Simulate;

namespace SimulationAPI.Repository;

public class SimulationRepository(SimulationFactory simulationFactory) : ISimulationRepository<Guid, int>
{
  ConcurrentDictionary<Guid, Task<int>> simulationTasks = new ConcurrentDictionary<Guid, Task<int>>();
  ConcurrentDictionary<Guid, int> simulationTaskProgress = new ConcurrentDictionary<Guid, int>();

  private void UpdateTaskProgress((Guid, int) TaskAndProgress)
  {
    (Guid id, int progressUpdate) = TaskAndProgress;
    if (!simulationTaskProgress.TryGetValue(id, out int currentProgress))
    {
      simulationTaskProgress.TryAdd(id, progressUpdate);
    }
    else if (currentProgress < progressUpdate)
    {
      simulationTaskProgress.TryUpdate(id, progressUpdate, currentProgress);
    }
  }

  public int CheckProgress(Guid simulationId)
  {
    if (!simulationTaskProgress.TryGetValue(simulationId, out var simulationProgress))
    {
      throw new KeyNotFoundException();
    }
    return simulationProgress;
  }

  public void CreateAndRun(Guid id)
  {
    if (simulationTasks.ContainsKey(id))
    {
      throw new ArgumentException();
    }
    ISimulation sim = simulationFactory.CreateSimulation(id);
    Progress<(Guid, int)> recordProgress = new Progress<(Guid, int)>(UpdateTaskProgress);

    Task<int> simulationTask = Task.Run(() => sim.Start(recordProgress));
    if (!simulationTasks.TryAdd(id, simulationTask))
    {
      throw new ArgumentException();
    }
  }


  public async Task<int> GetResults(Guid simulationId)
  {
    if (!simulationTasks.TryGetValue(simulationId, out var simulationTask) ||
        !simulationTaskProgress.TryGetValue(simulationId, out int progress))
    {
      throw new KeyNotFoundException();
    }
    if (progress < 100)
    {
      throw new NotSupportedException();
    }
    return await simulationTask;
  }
}