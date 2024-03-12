namespace SimulationAPI.Simulate;

public class ConcreteSimulationFactory : SimulationFactory
{
  public override ISimulation CreateSimulation(Guid id)
  {
    return new Simulation { Id = id };
  }
}