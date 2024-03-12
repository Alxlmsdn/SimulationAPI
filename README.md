# SimulationAPI

## Key Design Assumptions

- For the purpose of this implementation the assumption was made that the simulation computation would be carried out on the same infrastructure the simulation API is hosted on. In a production scenario, this would unlikely be the case, in the **Enhancement** section I elaborate.

- Multiple simulations can be run in parallel. This implementation assumes that if a simulation is currently being processed and a request is received to begin another simulation, if the infrastructure has capacity this new simulation can be started alongside the already running simulations.

## Implementation

### API

The Solution was implemented using a ASP.NET Minimal API. This was decided based on the required microservice only serving a single resource and the overhead of API controllers is not necessary.

3 endpoints where implemented to interact with a simulation.

Includes Swagger middleware to interact with API in test.

1. **POST /**

   The ability to request the creation and processing of a simulation. Endpoint returns the Id of the simulation.

2. **GET /progress/{id}**

   Returns the current percentage progress of a simulation as an integer.

3. **GET /results{id}**
   Returns the resulting output of a completed simulation, if the simulation is still being processed, a 422 UnprocessableEntity response is returned.

### Simulation

This implementation makes use of the created `ISumulation` interface to mimic the behavior of a production simulation workload. This interface exposes the `Start(IProgress)` method for processing a simulation, with the optional ability to log progress updates to an instance implementing the [IProgress](https://learn.microsoft.com/en-us/dotnet/api/system.iprogress-1?view=net-8.0) interface.

For the purpose of this task, the concrete implementation of ISimulation sleeps for a random number of seconds, logging an updated progress value every second corresponding to the completion percentage of simulation [0, 100].

When the simulation has reached the end of its processing time, the result of a simulation is a randomly generated int between [1000, 5000], mimicking a simulation output.

A Simulation Factory Class is used to control the instantiation of a Simulation object at runtime.

### Simulation Repository

A simulation interface & concrete implementation was created as an in-memory store for facilitating the processing of simulations. This implementation processes simulations asynchronously, making use of thread safe ConcurrentDictionary objects to track a simulations lifecycle.

The repository exposes the ability to create and run a simulation, provided with an Id. Check the current state of a simulations progress, provided an Id. If a simulation has ran to completion, get the results of a simulation provided an Id.

### Message Broker

To buffer requests as they are received an internal Message Broker was implemented using a DataFlow BufferBlock. This decouples the endpoint responsible for receiving simulation requests from the Simulation Repository responsible for processing the requests.

The broker interface exposes 2 methods, the ability to add a message to the broker, the other the ability to pass in an Action that is invoked when a message is pushed to the broker.

Adding a message to the broker from the API endpoint and
invoking the Simulation Repository method to create and run a simulation when a message is received is configured in the Simulation Service - loosely coupling the repository to the message broker.

## Hurdles

## Enhancements

For the purpose of this microservice implementation, all the simulation processing and message brokering are done in a single project/solution but have been implemented by design to minimize the required refactoring if further effort was put into building this for a production scenario. In a production scenario, the simulation computation would likely be executed on separate infrastructure and independent from the microservice API, this would support the ability to scale the simulation compute resources independent of the API service, and vice versa if the API needed to scale to handle a surge of requests.

This could be achieved by using an external datastore to store the progress/tracking of the simulation, so status updates can be read from the API, while the simulation repository can write updates as simulations are processed.

This would further be enhanced by using a lightweight external message broker such as Azure Service Bus or Azure Storage Queue, with support for message re-try logic and multiple subscribers to support the scaling activities of the simulation compute infrastructure.

Given more time, the solution would benefit from a more comprehensive logging implementation and verbose exception messages.

Refactoring the endpoints to better support consumer self-discovery would also be beneficial. Ror example if a request was made to the progress endpoint and the response indicated that the simulation was completed, including the URI to fetch the result of that simulation.
