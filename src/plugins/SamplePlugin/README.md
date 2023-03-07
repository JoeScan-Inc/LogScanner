# Sample implementation of a LogScannerEngine plugin

This is an example on how to write a plugin. 
The SamplePlugin loaded on Engine startup, and listens to the results of 
the LogModelBuilder. The method Consume() is called for each log. 

The plugin must be published before the Engine will recognize it. 
You can do this either manually in the Visual Studio IDE (Publish...) or 
via 
	
	dotnet publish /p:Configuration=Debug /p:PublishProfile=DebugProfile

or

	dotnet publish /p:Configuration=Release /p:PublishProfile=ReleaseProfile

The profiles are part of the project and have been preset to write the output to the correct
folder. 

The plugin *MUST* be contained in an assembly with the same name as it's folder, e.g.:

	$LOGSCANNER_INSTALLATION_DIR/bin/extensions/SamplePlugin/SamplePlugin.dll