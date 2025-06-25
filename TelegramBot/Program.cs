using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBot.Extensions;

await Host.CreateDefaultBuilder(args)
  .ConfigureAppConfiguration((ctx, config) =>
  {
      config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

      if (ctx.HostingEnvironment.IsDevelopment())
      {
          config.AddUserSecrets<Program>(optional: true);
      }
      else
      {
          config.AddAzureKeyVault(
            new Uri("https://TeleBotVault.vault.azure.net/"),
            new DefaultAzureCredential());
      }
  })
  .ConfigureServices((hostContext, services) =>
  {
      services.AddBotServices(hostContext.Configuration);
      services.AddHostedService<TelegramBot.Services.PollingService>();
  })
  .ConfigureLogging(logging =>
  {
      logging.ClearProviders();
      logging.AddConsole();
      Console.OutputEncoding = System.Text.Encoding.UTF8;
  })
  .RunConsoleAsync();
