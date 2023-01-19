using Marvin.Application.Sdk;

using (var appBuilder = MarvinApplicationBuilder.Create(args))
{
    appBuilder.AddDefaultServices();
    
    appBuilder
        .ConfigureLogging()
        .AddConsole()
        .Apply();
    
    var application = appBuilder.BuildApplication();

    await application.RunAsync();
}