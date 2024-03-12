using Moq;
using SimulationAPI.Message;

namespace SimulationAPITests;

[TestClass]
public class MessageBrokerTests
{
    [TestMethod]
    public void MessageBroker_Correct_Invokes_Action_When_Message_Received()
    {
        var brokerMock = new MessageBroker<int>();
        var messageReceiverMock = new Mock<Action<int>>();
        brokerMock.SubscribeToMessages(messageReceiverMock.Object);
        brokerMock.AddMessage(10);
        Assert.AreEqual(1, messageReceiverMock.Invocations.Count);
    }
}