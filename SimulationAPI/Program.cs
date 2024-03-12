using SimulationAPI.Message;
using SimulationAPI.Repository;
using SimulationAPI.Service;
using SimulationAPI.Simulate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<SimulationFactory, ConcreteSimulationFactory>();
builder.Services.AddSingleton<ISimulationRepository<Guid, int>, SimulationRepository>();
builder.Services.AddSingleton<IMessageBroker<Guid>, MessageBroker<Guid>>();
builder.Services.AddSingleton<ISimulationService, SimulationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/", CreateSimulation)
.WithName("CreateSimulation")
.WithOpenApi();

app.MapGet("/progress/{id:guid}", GetSimulationProgress)
.WithName("GetSimulationProgress")
.WithOpenApi();

app.MapGet("/results/{id:guid}", GetSimulationResults)
.WithName("GetSimulationResults")
.WithOpenApi();

static IResult CreateSimulation(HttpContext context, ISimulationService simulationService)
{
    Guid simulationId = Guid.NewGuid();
    var enqueued = simulationService.QueueSimulation(simulationId);
    if (!enqueued)
    {
        Console.WriteLine("Simulation Not Queued");
        return TypedResults.Problem();
    }
    return TypedResults.Ok(simulationId.ToString());
}

static IResult GetSimulationProgress(Guid id, ISimulationRepository<Guid, int> simulationRepository)
{
    try
    {
        int simulationPercentComplete = simulationRepository.CheckProgress(id);
        return TypedResults.Ok(simulationPercentComplete);
    }
    catch (ArgumentNullException ane)
    {
        return TypedResults.BadRequest("Invalid id");
    }
    catch (KeyNotFoundException knfe)
    {
        return TypedResults.BadRequest("Invalid id");
    }
}

static async Task<IResult> GetSimulationResults(Guid id, ISimulationRepository<Guid, int> simulationRepository)
{
    try
    {
        int simulationResult = await simulationRepository.GetResults(id);
        return TypedResults.Ok(simulationResult);
    }
    catch (ArgumentNullException ane)
    {
        return TypedResults.BadRequest("Invalid id");
    }
    catch (KeyNotFoundException knfe)
    {
        return TypedResults.BadRequest("Invalid id");
    }
    catch (NotSupportedException nse)
    {
        return TypedResults.UnprocessableEntity("Simulation still in progress");
    }
}

app.Run();
