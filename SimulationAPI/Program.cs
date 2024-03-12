using SimulationAPI.Repository;
using SimulationAPI.Simulate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<SimulationFactory, ConcreteSimulationFactory>();
builder.Services.AddSingleton<ISimulationRepository<Guid, int>, SimulationRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/", (HttpContext context, ISimulationRepository<Guid, int> simulationRepository) =>
{

    Guid simulationId = Guid.NewGuid();
    simulationRepository.CreateAndRun(simulationId);
    return simulationId.ToString();
    //1. Accept request
    //2. Add message to message broker, requesting start of simulation
    //3. return 200 response with ID of simulation.
})
.WithName("CreateSimulation")
.WithOpenApi();

app.MapGet("/getProgress/{id:guid}", (Guid id, ISimulationRepository<Guid, int> simulationRepository) =>
{
    //1. Accept request, parse simulation ID
    //2. call method to fetch simulation progress %
    //3. return response with simulation progress.
    return simulationRepository.CheckProgress(id);
})
.WithName("GetSimulationProgress")
.WithOpenApi();

app.MapGet("/getResults/{id:guid}", async (Guid id, ISimulationRepository<Guid, int> simulationRepository) =>
{
    //1. accept request and parse simulation ID
    //2. call method to fetch results if available for simulation.
    //3. return response with results
    return await simulationRepository.GetResults(id);
})
.WithName("GetSimulationResults")
.WithOpenApi();

app.Run();
