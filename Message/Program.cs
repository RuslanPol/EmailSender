using System.Diagnostics;
using System.Text.Json;
using Massege;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger(); //означает, что глобальный логер будет заменен на вариант из Host.UseSerilog
Log.Information("Starting up");
try
{
    // var smtpConfig = new SmtpConfig();
    var builder = WebApplication.CreateBuilder(args);
    // smtpConfig.Host = builder.Configuration["Host"];
    // smtpConfig.UserName = builder.Configuration["UserName"];
    // smtpConfig.Password = builder.Configuration["Password"];

    builder.Services.Configure<SmtpConfig>(
         builder.Configuration.GetSection("SmtpConfig"));
    builder.Services.Configure<ServerStartupNotificationBackgroundService>(
        builder.Configuration.GetSection("ServerStartupNotificationBackgroundService"));


    builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();

    builder.Services.AddHostedService<ServerStartupNotificationBackgroundService>();
    builder.Host.UseSerilog((_, conf) =>
    {
        conf
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day);

        //.ReadFrom.Configuration(ctx.Configuration);
    });


    var app = builder.Build();
    app.UseStaticFiles();
    app.UseSerilogRequestLogging();


    app.MapGet("/",
        async (IEmailSender sender) =>
        {
             await sender.Send("rpolisuk72@gmail.com", $"Открыли  ", "Главная страница открыта");
        });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush(); //перед выходом дожидаемся пока все логи будут записаны
}