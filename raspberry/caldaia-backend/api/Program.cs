using NLog;
using NLog.Web;
using application.dependencyInjection;
using rotex.dependencyInjection;

using LogLevel = NLog.LogLevel;
using rotex;
using api.dependencyInjection;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls(new string[] { "http://0.0.0.0:32767" });

// NLog Configuration
LogManager.Setup().LoadConfiguration(builder =>
{
    builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole();
    builder.ForLogger().FilterMinLevel(LogLevel.Debug).WriteToFile("../logs/DEBUG.log");
});

builder.Host.UseNLog();

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


app.Services.StartCaldaiaApplication();

app.Run();