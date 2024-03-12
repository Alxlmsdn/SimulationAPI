using SimulationAPI.Message;
using SimulationAPI.Repository;

namespace SimulationAPI.Service;

public class SimulationService : ISimulationService
{
  private readonly IMessageBroker<Guid> messageBroker;

  public SimulationService(ISimulationRepository<Guid, int> simulationRepository, IMessageBroker<Guid> messageBroker)
  {
    this.messageBroker = messageBroker;
    this.messageBroker.SubscribeToMessages(simulationRepository.CreateAndRun);
  }

  public bool QueueSimulation(Guid id) => messageBroker.AddMessage(id);
}