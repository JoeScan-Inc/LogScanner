using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
   
    public static int Main () => Execute<Build>(x => x.PublishAll);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "dist";
    
    [Solution] readonly Solution Solution;
    
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            OutputDirectory.CreateOrCleanDirectory();
        });


    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s=>s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target PublishDesktop => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("JoeScan.LogScanner.Desktop"))
                .SetConfiguration(Configuration)
                .EnablePublishSingleFile()
                .SetRuntime("win-x64")
                .DisableSelfContained()
                .SetProcessArgumentConfigurator(_ => _
                    .Add("/p:DebugType=None /p:DebugSymbols=false"))
                .SetOutput(OutputDirectory));
            CopyFile(RootDirectory / "src" / "JoeScan.LogScanner.Desktop"  / "nlog.config", OutputDirectory / "nlog.config", FileExistsPolicy.Skip);
            CopyDirectoryRecursively(RootDirectory / "Config" / "Default", OutputDirectory / "config" / "Default");
        });
    
    Target PublishService => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("JoeScan.LogScanner.Service"))
                .SetConfiguration(Configuration)
                .EnablePublishSingleFile()
                .SetRuntime("win-x64")
                .DisableSelfContained()
                .SetProcessArgumentConfigurator(_ => _
                    .Add("/p:DebugType=None /p:DebugSymbols=false"))
                .SetOutput(OutputDirectory));
        });
    Target PublishReview => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(Solution.GetProject("JoeScan.LogScanner.LogReview"))
                .SetConfiguration(Configuration)
                .EnablePublishSingleFile()
                .SetRuntime("win-x64")
                .DisableSelfContained()
                .SetProcessArgumentConfigurator(_ => _
                    .Add("/p:DebugType=None /p:DebugSymbols=false"))
                .SetOutput(OutputDirectory));
            
        });
    
    Target PublishRawViewer => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject(SourceDirectory/"tools"/"RawViewer"/"RawViewer.csproj")
                .SetConfiguration(Configuration)
                .EnablePublishSingleFile()
                .SetRuntime("win-x64")
                .DisableSelfContained()
                .SetProcessArgumentConfigurator(_ => _
                    .Add("/p:DebugType=None /p:DebugSymbols=false"))
                .SetOutput(OutputDirectory));
            
        });
    
    Target PublishProblemLogArchiver=> _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
           DotNetPublish(s => s
               .SetProject(SourceDirectory / "plugins" / "ProblemLogArchiver" / "ProblemLogArchiver.csproj")
               .SetConfiguration(Configuration)
               .SetRuntime("win-x64")
               .DisableSelfContained()
               .SetOutput(OutputDirectory / "extensions" / "ProblemLogArchiver"));
        });
    Target PublishSamplePlugin=> _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPublish(s =>
            {
                return s
                    .SetProject(SourceDirectory / "plugins" / "SamplePlugin" / "SamplePlugin.csproj")
                    .SetConfiguration(Configuration)
                    .SetRuntime("win-x64")
                    .DisableSelfContained()
                    .SetOutput(OutputDirectory / "extensions" / "SamplePlugin");
            });
            
        });
    Target Plugins => _ => _
        .DependsOn(PublishProblemLogArchiver)
        .DependsOn(PublishSamplePlugin);
    
    Target PublishAll => _ => _
        .DependsOn(PublishDesktop)
        .DependsOn(PublishReview)
        .DependsOn(PublishService)
        .DependsOn(PublishRawViewer)
        .DependsOn(Plugins);
    
}
