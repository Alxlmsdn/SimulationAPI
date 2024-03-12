using Moq;
using SimulationAPI.Repository;
using SimulationAPI.Simulate;

namespace SimulationAPITests;

[TestClass]
public class SimulationRepositoryTests
{
  [TestMethod]
  public void SimulationRepository_CreateAndRun_Enables_Simulation_Progress()
  {
    //Arrange
    var simulationStub = new Mock<ISimulation>();

    var simulationFactoryStub = new Mock<SimulationFactory>();
    simulationFactoryStub.Setup(m => m.CreateSimulation(It.IsAny<Guid>())).Returns(simulationStub.Object);

    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act
    repository.CreateAndRun(Guid.NewGuid());
    //Assert
    simulationStub.Verify(s => s.Start(It.IsAny<IProgress<(Guid, int)>>()), Times.Once());
  }
  [TestMethod]
  public void SimulationRepository_CreateAndRun_Throws_Exception_For_Already_Existing_Id()
  {
    //Arrange
    var simulationStub = new Mock<ISimulation>();
    var simulationFactoryStub = new Mock<SimulationFactory>();
    simulationFactoryStub.Setup(m => m.CreateSimulation(It.IsAny<Guid>())).Returns(simulationStub.Object);
    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    var idToDuplicate = Guid.NewGuid();
    repository.CreateAndRun(idToDuplicate);
    //Act & Assert
    Assert.ThrowsException<ArgumentException>(() => repository.CreateAndRun(idToDuplicate));
  }
  [TestMethod]
  public void SimulationRepository_CreateAndRun_ThrowsException_For_Invalid_Id()
  {
    //Arrange
    var simulationFactoryStub = new Mock<SimulationFactory>();
    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act & Assert
    Assert.ThrowsException<ArgumentNullException>(() => repository.CreateAndRun(Guid.Empty));
  }
  [TestMethod]
  public void SimulationRepository_CheckProgress_Throws_Exception_For_Simulation_That_Does_Not_Exist()
  {
    //Arrange
    var simulationFactoryStub = new Mock<SimulationFactory>();
    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act & Assert
    Assert.ThrowsException<KeyNotFoundException>(() => repository.CheckProgress(Guid.NewGuid()));
  }
  [TestMethod]
  public void SimulationRepository_CheckProgress_Returns_Correct_Progress_Value()
  {
    var simulationId = Guid.NewGuid();
    var expectedProgress = 25;
    var simulationStubProcess = (IProgress<(Guid, int)> prog) => prog.Report((simulationId, expectedProgress));
    //Arrange
    Progress<(Guid, int)> progressStub = new Progress<(Guid, int)>();
    var simulationStub = new Mock<ISimulation>();
    simulationStub.Setup(s => s.Start(It.IsAny<IProgress<(Guid, int)>>())).Callback(simulationStubProcess);

    var simulationFactoryStub = new Mock<SimulationFactory>();
    simulationFactoryStub.Setup(m => m.CreateSimulation(It.IsAny<Guid>())).Returns(simulationStub.Object);

    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act
    repository.CreateAndRun(simulationId);
    //Assert
    simulationStub.Verify(s => s.Start(It.IsAny<IProgress<(Guid, int)>>()), Times.Once());
    int simulationProgress = repository.CheckProgress(simulationId);
    Assert.AreEqual(expectedProgress, simulationProgress);
  }
  [TestMethod]
  public void SimulationRepository_GetResults_Throws_Exception_For_Simulation_That_Does_Not_Exist()
  {
    //Arrange
    var simulationFactoryStub = new Mock<SimulationFactory>();
    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act & Assert
    Assert.ThrowsExceptionAsync<KeyNotFoundException>(() => repository.GetResults(Guid.NewGuid()));
  }
  [TestMethod]
  public void SimulationRepository_GetResults_Throws_Exception_For_Simulation_That_Has_Not_Completed()
  {
    var simulationId = Guid.NewGuid();
    var expectedProgress = 25;
    var simulationStubProcess = (IProgress<(Guid, int)> prog) => prog.Report((simulationId, expectedProgress));
    //Arrange
    Progress<(Guid, int)> progressStub = new Progress<(Guid, int)>();
    var simulationStub = new Mock<ISimulation>();
    simulationStub.Setup(s => s.Start(It.IsAny<IProgress<(Guid, int)>>())).Callback(simulationStubProcess);

    var simulationFactoryStub = new Mock<SimulationFactory>();
    simulationFactoryStub.Setup(m => m.CreateSimulation(It.IsAny<Guid>())).Returns(simulationStub.Object);

    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act
    repository.CreateAndRun(simulationId);
    //Assert
    simulationStub.Verify(s => s.Start(It.IsAny<IProgress<(Guid, int)>>()), Times.Once());
    Assert.ThrowsExceptionAsync<NotSupportedException>(() => repository.GetResults(simulationId));
  }
  [TestMethod]
  public async void SimulationRepository_GetResults_Correctly_Returns_Result_For_Completed_Simulation()
  {
    var simulationId = Guid.NewGuid();
    var expectedProgress = 100;
    var expectedResult = 12345;
    var simulationStubProcess = (IProgress<(Guid, int)> prog) =>
    {
      prog.Report((simulationId, expectedProgress));
      return expectedResult;
    };
    //Arrange
    Progress<(Guid, int)> progressStub = new Progress<(Guid, int)>();
    var simulationStub = new Mock<ISimulation>();
    simulationStub.Setup(s => s.Start(It.IsAny<IProgress<(Guid, int)>>())).Callback(simulationStubProcess);

    var simulationFactoryStub = new Mock<SimulationFactory>();
    simulationFactoryStub.Setup(m => m.CreateSimulation(It.IsAny<Guid>())).Returns(simulationStub.Object);

    SimulationRepository repository = new SimulationRepository(simulationFactoryStub.Object);
    //Act
    repository.CreateAndRun(simulationId);
    //Assert
    simulationStub.Verify(s => s.Start(It.IsAny<IProgress<(Guid, int)>>()), Times.Once());
    int simulationResult = await repository.GetResults(simulationId);
    Assert.AreEqual(expectedResult, simulationResult);
  }
}