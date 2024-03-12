using System.Threading.Tasks.Dataflow;

namespace SimulationAPI.Message;

public class MessageBroker<T> : IMessageBroker<T>
{
  private BufferBlock<T> bufferBlock = new BufferBlock<T>();

  public bool AddMessage(T id)
  {
    return bufferBlock.Post(id);
  }

  public void SubscribeToMessages(Action<T> action)
  {
    var actionBlock = new ActionBlock<T>(action);
    bufferBlock.LinkTo(actionBlock);
  }
}