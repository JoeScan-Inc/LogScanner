
## Building 
### Prerequisites

LogScanner is a .NET application, based on .NET Core 7. The core library LogScanner.Core, is platform agnostic and can be used in any .NET application. The UI applications are based on WPF and are only available for Windows.

You will need to have the following installed on your machine:

 - .NET Core 7 SDK (https://dotnet.microsoft.com/download/dotnet-core/7.0)
 - Visual Studio 2022 (https://visualstudio.microsoft.com/downloads/) or JetBrains Rider (https://www.jetbrains.com/rider/download/)
 - Git (https://git-scm.com/downloads)
 - GitVersion global tool (dotnet tool install --global GitVersion.Tool --version 5.*)
 - Nuke  (https://nuke.build/) - install as a global tool (dotnet tool install Nuke.GlobalTool --global)

## Building the solution
For development, open the solution file, LogScanner.sln, in Visual Studio or Rider. The solution contains the following projects:

 - JoeScan.LogScanner.Core - the core library
 - JoeScan.LogScanner.Desktop - the WPF UI scanning application
 - JoeScan.LogScanner.LogReview - the WPF UI log review application
 - JoeScan.LogScanner.Service - a platform independent application that performs log scanning
 - JoeScan.LogScanner.Shared - shared code between the UI applications
 - Tools/RawViewer - a graphical tool for viewing recorded raw scan data
 - Plugins/SamplePlugin - a sample plugin for vendors to integrate with LogScanner
 - Plugins/ProblemLogArchiver - a plugin for archiving problem logs

The solution can be built from the command line using the following command:
```
nuke [target] 
```
If no target is specified, the default target is used, which builds the solution, publishes the UI applications, and copies all configuration data to the output directory ```dist```.
Run 
```
nuke --target
```
to see a list of all available targets.

Building within the IDE will also build the solution, but will not publish the UI applications or copy configuration data. However, the UI applications can be run from within the IDE, and will use the configuration data in the ```config/Default``` directory.

## Architecture and Design

In this section, some key concepts of LogScanner are explained. 

### LogModel
For measured data on the LogModel, see [Logmodel Properties](logmodel/logmodel.md)