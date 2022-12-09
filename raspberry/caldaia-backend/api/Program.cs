using api.dependencyInjection;
using application.dependencyInjection;
using rotex;
using rotex.dependencyInjection;
using application.infrastructure;



using NLog;
using NLog.Web;
using LogLevel = NLog.LogLevel;
using api.arduinoMimic;
using api.signalr;

LogManager.Setup().LoadConfiguration(logBuilder =>
{
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
});


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = "www",
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

builder.Services.AddSerialRotexReader(new RaspberryRotexReaderConfig());
builder.Services.AddRaspberryGpio();
builder.Services.AddRaspberryIOSet();
builder.Services.AddCaldaiaApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<DataHub>("datahub");
app.MapControllers();

var signalRNotificationAdapter = app.Services.GetService<SignalRNotificationAdapter>();
signalRNotificationAdapter.Start();

var caldaiaApp = app.Services.StartCaldaiaApplication();

app.Lifetime.ApplicationStopping.Register(() => caldaiaApp.Stop());
app.Run();
