using application.dependencyInjection;
using application.infrastructure;
using Microsoft.AspNetCore.Builder;



using NLog;
using NLog.Web;
using LogLevel = NLog.LogLevel;
using api.signalr;
using application;
using rotex;
using api.dependencyInjection;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.IIS.Core;

LogManager.Setup().LoadConfiguration(logBuilder =>
{
#if RELEASE
    logBuilder.ForLogger()
        .FilterMinLevel(LogLevel.Warn)
        .WriteToConsole();

    logBuilder.ForLogger()
        .FilterMinLevel(LogLevel.Info)
        .WriteToFile(
            fileName: "../logs/INFO.log",
            archiveAboveSize: 9 * 1024 * 1024,
            maxArchiveFiles: 1
        );

    logBuilder
        .ForLogger()
        .FilterMinLevel(LogLevel.Debug)
        .WriteToFile(
            fileName: "../logs/DEBUG.log",
            archiveAboveSize: 9 * 1024 * 1024,
            maxArchiveFiles: 2
        );
#else
    logBuilder.ForLogger()
        .FilterMinLevel(LogLevel.Debug)
        .WriteToConsole();
#endif
});

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args
});

builder.Host.UseNLog();

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.EnableDetailedErrors = true;
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls(new string[] { "http://0.0.0.0:32767" });

// Adds the caldaia application to the Service Collection
builder.Services.AddSingleton<InProcessNotificationHub>();
builder.Services.AddSingleton<INotificationPublisher, InProcessNotificationHub>();
builder.Services.AddSingleton<INotificationSubscriber, InProcessNotificationHub>();
builder.Services.AddSingleton<SignalRNotificationAdapter>();

#if RELEASE
var rotexConfig = new RaspberryRotexReaderConfig();
builder.Services.AddRaspberryIOSet(rotexConfig);
#else
builder.Services.AddMockIOSet();
#endif

var config = new CaldaiaConfig(TimeSpan.FromSeconds(5));
builder.Services.AddCaldaiaApplication(config);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseDefaultFiles();
app.UseStaticFiles();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<DataHub>("datahub");
app.MapControllers();
app.UseRewriter(new RewriteOptions()
    .AddRedirect("^$", "/app/")
);
var signalRNotificationAdapter = app.Services.GetService<SignalRNotificationAdapter>();
signalRNotificationAdapter?.Start();

var caldaiaApp = app.Services.StartCaldaiaApplication();

app.Lifetime.ApplicationStopping.Register(() => {
    Console.WriteLine("Stopping Caldaia Application!");
    caldaiaApp.Stop();
    caldaiaApp.Dispose();
});

app.Run();
