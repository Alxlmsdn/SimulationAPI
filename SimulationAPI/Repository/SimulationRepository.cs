
using System.Collections.Concurrent;
using SimulationAPI.Simulate;

namespace SimulationAPI.Repository;

public class SimulationRepository(SimulationFactory simulationFactory) : ISimulationRepository<Guid, int>
{
  readonly ConcurrentDictionary<Guid, Task<int>> simulationTasks = new ConcurrentDictionary<Guid, Task<int>>();
  readonly ConcurrentDictionary<Guid, int> simulationTaskProgress = new ConcurrentDictionary<Guid, int>();

  private void ValidateGuidArgument(Guid id)
  {
    if (Guid.Empty == id) throw new ArgumentNullException();
  }
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

  public void CreateAndRun(Guid id)
  {
    ValidateGuidArgument(id);
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

  public int CheckProgress(Guid id)
  {
    ValidateGuidArgument(id);
    if (!simulationTaskProgress.TryGetValue(id, out var simulationProgress))
    {
      throw new KeyNotFoundException();
    }
    return simulationProgress;
  }


  public async Task<int> GetResults(Guid id)
  {
    ValidateGuidArgument(id);
    if (!simulationTasks.TryGetValue(id, out var simulationTask) ||
        !simulationTaskProgress.TryGetValue(id, out int progress))
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