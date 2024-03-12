namespace SimulationAPI.Simulate;

public class Simulation : ISimulation
{
  private readonly int _simulationDurationSeconds = Random.Shared.Next(20, 30);
  private DateTime _startTime;

  private Guid _id;

  public Guid Id { get => _id; init => _id = value; }

  private int CheckProgress()
  {
    var now = DateTime.Now;
    double elapsedSeconds = Math.Max((now - _startTime).TotalSeconds, 1);
    Console.WriteLine($"{Id} - elapsedSeconds: {elapsedSeconds}");
    return (int)Math.Clamp(elapsedSeconds / _simulationDurationSeconds * 100, 0, 100);
  }
  public int Start(IProgress<(Guid, int)>? progress)
  {
    if (_startTime != default) return default;

    _startTime = DateTime.Now;
    Console.WriteLine($"{Id} -  sim time: {_simulationDurationSeconds}");
    for (int i = 0; i < _simulationDurationSeconds; i++)
    {
      Thread.Sleep(1000);
      progress?.Report((Id, CheckProgress()));
    }
    return Random.Shared.Next(1000, 50000);
  }
}
