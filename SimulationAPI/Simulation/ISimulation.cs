namespace SimulationAPI.Simulate;

public interface ISimulation
{
  int Start(IProgress<(Guid, int)>? progress);
}