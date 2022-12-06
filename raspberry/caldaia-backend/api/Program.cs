using api.dependencyInjection;
using application.dependencyInjection;
using rotex;
using rotex.dependencyInjection;


using NLog;
using NLog.Web;
using LogLevel = NLog.LogLevel;

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


var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls(new string[] { "http://0.0.0.0:32767" });

// NLog Configuration

// Adds the caldaia application to the Service Collection
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


var caldaiaApp = app.Services.StartCaldaiaApplication();

app.Lifetime.ApplicationStopping.Register(() => caldaiaApp.Dispose());
app.Run();
