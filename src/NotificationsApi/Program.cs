using NotificationsApi.Consumers;
using NotificationsApi.Messaging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<WelcomeEmailNotifier>();
builder.Services.AddSingleton<PurchaseConfirmationNotifier>();
builder.Services.AddHostedService<UserCreatedConsumer>();
builder.Services.AddHostedService<PaymentProcessedConsumer>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
