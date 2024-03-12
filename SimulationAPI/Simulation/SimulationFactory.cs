namespace SimulationAPI.Simulate;

public abstract class SimulationFactory
{
  public abstract ISimulation CreateSimulation(Guid id);
}
