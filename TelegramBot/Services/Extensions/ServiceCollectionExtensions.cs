using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mindee;
using OpenAI.Chat;
using Telegram.Bot;
using TelegramBot.Services.Abstractions;
using TelegramBot.Services.Implementations;
using TelegramBot.Services.Abs; 
using TelegramBot.Services.Imp;
using TelegramBot.Services.Business;
using TelegramBot.Services.Implementations.Handlers;

namespace TelegramBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("telegram_bot_client")
               .AddTypedClient<ITelegramBotClient>((http, sp) =>
               {
                   var token = configuration["TELEGRAM_BOT_TOKEN"];
                   if (string.IsNullOrWhiteSpace(token))
                       throw new InvalidOperationException("TELEGRAM_BOT_TOKEN is not configured.");
                   return new TelegramBotClient(token, http);
               });
            services.AddSingleton(sp =>
            {
                var key = configuration["MINDEE_API_KEY"];
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("MINDEE_API_KEY is not configured.");
                return new MindeeClient(key);
            });
            services.AddSingleton(sp =>
            {
                var key = configuration["OPENAI_API_KEY"];
                if (string.IsNullOrWhiteSpace(key))
                    throw new InvalidOperationException("OPENAI_API_KEY is not configured.");
                var model = configuration["OPENAI_MODEL"] ?? "gpt-4o-mini";
                return new ChatClient(model, key);
            });
            services.AddHttpClient();

            services.AddSingleton<IUserSessionService, InMemoryUserSessionService>();
            services.AddScoped<IUpdateHandler, UpdateHandler>();
            services.AddScoped<IBotFlowService, BotFlowService>();
            services.AddScoped<IMindeeService, MindeeService>();
            services.AddScoped<IFileService, TelegramFileService>();
            services.AddScoped<IOpenAiService, OpenAiService>();

            services.AddScoped<ITextMessageHandler, TextMessageHandler>();
            services.AddScoped<IPhotoMessageHandler, PhotoMessageHandler>();
            services.AddScoped<ICallbackQueryHandler, CallbackQueryHandler>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IChatHistoryService, ChatHistoryService>();
            services.AddScoped<IPassportProcessor, PassportProcessor>();
            services.AddScoped<IVehicleRegistrationProcessor, VehicleRegistrationProcessor>();
            services.AddScoped<ICompletionService, CompletionService>();

            return services;
        }
    }
}
