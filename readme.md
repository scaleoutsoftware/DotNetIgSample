# ScaleOut .NET Invocation Grid Sample

This solution illustrates how to implement an Invocation Grid (IG) project that can be launched by the ScaleOut StateServer service.

Several advanced ScaleOut features take advantage of server-side events for fast, local processing of ScaleOut objects. ScaleOut's Invocation Grid hosting model allows you to deploy and host event-handling code in a .NET worker process that is managed by the ScaleOut service. 

This sample solution consists of three projects:

- *ShoppingCartIG*: An Invocation Grid worker project, based on the *igworker* project template that is available in the *Scaleout.Templates* NuGet package. This example worker hosts a [PMI Reduce operation](https://static.scaleoutsoftware.com/docs/dotnet_client/articles/pmi/about/about_pmi.html) that analyzes shopping carts stored in the ScaleOut service.

- *ShoppingCart*: A class library containing the `Cart` data transfer object (DTO) class and supporting types, which are stored in the ScaleOut service.

- *Client*: A command-line client program that loads the ScaleOut service with shopping cart objects and illustrates how to execute a PMI invoke operation against the Invocation Grid.

## Prerequisites

- One or more hosts running the ScaleOut service using a [ScaleOut StateServer® Pro](https://www.scaleoutsoftware.com/products/stateserver-pro/) or [ScaleOut StreamServer®](https://www.scaleoutsoftware.com/products/streamserver/) license
- .NET 5.0 SDK

## Running the Sample

1. Install the ScaleOut IG command-line utility. This [.NET tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) simplifies the packaging and deployment of IG worker projects.

        dotnet tool install -g Scaleout.InvocationGrid.Cli

2. Open a command prompt in the *ShoppingCartIG* project directory and run:

        ig start -c "bootstrapGateways=localhost:721"

   Modify the [connection string](https://static.scaleoutsoftware.com/docs/dotnet_client/articles/configuration/connecting.html#connection-strings) to specify the address and port used by one of your ScaleOut host systems.
   
   This command will build, package, and upload the ShoppingCartIG project to the ScaleOut servers in your cluster. The ScaleOut hosts will then decompress the package to a local temporary directory and start the worker process.
   
   Tip: Run `ig start --help` for a list of IG startup options.
   
3. Run the Client application. This will add shopping cart objects to the ScaleOut service and then invoke the IG worker's "TotalBackorderedValue" operation.

   If the client's `Cache.Invoke` call throws a `NotReadyException`, this indicates that the IG worker process has not been started on the ScaleOut hosts. Run the `ig start` command from step 2 above to start the IG worker process.
