namespace SimulationAPI.Message;

public interface IMessageBroker<T>
{
  bool AddMessage(T message);
  void SubscribeToMessages(Action<T> action);
}