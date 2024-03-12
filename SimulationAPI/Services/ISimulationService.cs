namespace SimulationAPI.Service;

public interface ISimulationService
{
  bool QueueSimulation(Guid id);
}