# LogScanner Documentation
LogScanner is an application for creating accurate measurements of logs, using the JS-20/JS-25/JS-50 series of laser scanners manufactured by JoeScan Inc. 

## Contents

## Licensing
JoeScan LogScanner is available under the MIT license. 
## Technical Overview
LogScanner is predominantly written in C#, based on .NET Core 6. 
### Platforms
Supported platforms are based on the availability of a .NET Core Runtime. Some components are only available for Microsoft Windows, such as the operator UI and the Review Tool.  
 - Microsoft Windows 10
 - macOS
 - Linux

## Architecture and Design
Unlike the previous versions of JoeScan LogScanner, the current version is heavily modularized, and has clean lines of separation
between the constituent components. This is made possible through a strong focus on Dependency Injection, with Autofac as the DI 
container of choice. The source and solution structure mirrors this choice, with the emphasis on components that adhere to SOLID
principles. We can roughly separate the areas of concern:

 1. hardware adapters (Js25Adapter, Js50Adapter, ReplayAdapter) 
 1. processing engine (LogScannerEngine, SingleZoneLogAssember, MultizoneLogAssembler) - LogScannerCore
 1. user interface (LogScanner, LogScanner.Headless)

 The 

### Hardware Adapters

We use [Autofac Modules](https://autofac.readthedocs.io/en/latest/configuration/modules.html#modules) 
extensively to keep a clean configuration. If you're unfamiliar with how Dependency Injection in Autofac works, it is highly
recommended to read the (very good) [documentation](https://autofac.readthedocs.io/en/latest/).

The main LogScanner Engine is de-coupled from the source of profile data, i.e. the actual scan heads. 
The profile data stream is fed into the engine via adapter modules. 

In order to build LogScanner for your specific hardware, 
 the DI Container (Autofac) must be 
 instructed to use a specific module for resolving the ```IScannerAdapter``` service. This is done in the ```ConfigureContainer```
 method for the application:

	// -- Adapter Modules --
    // only one adapter should be registered. The adapter module 
    // must provide at least one registration for an IScannerAdapter

    // builder.RegisterModule<ReplayModule>();
       builder.RegisterModule<Js25Module>();
    // builder.RegisterModule<Js50Module>();

Several adapter modules are provided:

 - `JS25Adapter` (works with JS20/25 series scanheads), based on JCamNet for scanner communication
 - `JS50Adapter` (works with JS50-WX and JS50-WSC), based on Pinchot API v13.1
 - `ReplayAdapter`  plays back previously recorded data for offline testing and debugging


 Each adapter is introduced to LogScanner via an Autofac module. For example, the Js25 project contains the following
 module definition in ```JoeScan.LogScanner.Js25/Js25Module.cs```:

    public  class Js25Module : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Js25Adapter>().As<IScannerAdapter>();
            // ...
        }
    }
 When the ```LogScannerEngine``` is built up as part of application start-up, the Autofac DI container looks for any classes that
 were registered to provide a ```IScannerAdapter``` service. It instantiates the service and injects it into the ```LogScannerEngine```.
 The engine only interacts with the scanners via this interface that the adapter exposes and knows nothing of the underlying 
 hardware. 

 ### Adapter Configuration
 Each adapter is free to choose what configuration mechanism to use. The adapter configuration is where you store things
 like Cable IDs (for the JS-20/25 series) or scanhead serial numbers (for the JS-50 series), phasing, laser exposure parameters etc.

 For example, the ```Js25Adapter``` uses an INI file, largely based on older versions of the software: 
    
    [ScanThread]
	    InternalProfileQueueLength = 10
	    BaseAddress = 192.168.0.1
	    EncoderUpdateIncrement = 1000
	    MaxRequestedProfileCount = 4
	    CableIdList = 0,1,2
	    ParamFile = '..\..\config\param.dat'
	    SyncMode = PulseSyncMode
	    Pulseinterval = 5000 # in microseconds, between 200 and 5,000,000 for JS-20/25


It is completely up to the developer to devise a mechanism to store the configuration. There is one specific aspect that needs
some explanation, though: The following line 

    builder.Register(c => new IniConfigSource("Js25Adapter.ini")).Keyed<IConfigSource>("Js25Adapter.ini").SingleInstance();

is needed to tell the DI container how to resolve the constructor parameters for ```Js25Adapter```. Within LogScanner, we user
```IConfigSource``` in many places as a generic mechanism to access data in an INI file. The DI Container needs to know which
INI file goes with which module. We use [Named and Keyed Services](https://autofac.readthedocs.io/en/latest/advanced/keyed-services.html#named-and-keyed-services) 
to tell Autofac which specific service to instantiate for a given constructor:
    
    public Js25Adapter([KeyFilter("Js25Adapter.ini")] IConfigSource config, ILogger? logger = null)
    {
        //...
    }

This causes Autofac to instantiate a new `IConfigSource` with the correct argument for the `Js25Adapter`. The alternative would 
be to implement a specific subclass for the adapter configuration.

The ```Js50Adapter``` uses a JSON file format. 

### Adapter Logging
LogScanner uses [NLog](https://nlog-project.org/) for low-level logging. The adapters are free to choose their own logging 
mechanism if desired, but can participate in the logging that the LogScanner engine provides. By including an ```ILogger``` 
argument in your constructor, the Engine's logger is automatically injected. 

### Profile Stream
The profiles acquired by the scanheads need to be transported to the engine. We use a technology called
[TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) - it 
is part of the Microsoft Task Parallel Library and provides a pipeline model through which our measured data flows. 
The adapter interface `IScannerAdapter` provides a single member, `AvailableProfiles`, which acts as the source of all measured 
profiles, i.e. the beginning of a stream. 
```
public BufferBlock<Profile> AvailableProfiles { get; private set; }
```
The LogScanner engine connects this stream to a pipeline of various processing (transform) and broadcast blocks. In this pipeline, 
the order of profiles is preserved, as we have the following steps:
 - a Bounding Box block transforms the profile by computing a 2-D bounding box
 - a Flights and Window filter transforms the profile by removing points that are outside of specific windows
 - a broadcast block that allows other components to 'see' the raw profile stream. The GUI uses this 'tee' in the pipeline 
  to display a live view of the data. 


  ## Requirements

## Building and Running


