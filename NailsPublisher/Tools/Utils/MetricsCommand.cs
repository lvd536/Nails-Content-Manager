using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailsPublisher.Tools.Metrics;

public static class MetricsCommand
{
    private static async Task ProfitAsync(ITelegramBotClient botClient, Message msg,  int daysPeriod) // статистика прибыли
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }

    private static async Task ExpensesAsync(ITelegramBotClient botClient, Message msg,  int daysPeriod) // статистика по расходам
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }

    private static async Task ProductsAsync(ITelegramBotClient botClient, Message msg) // аналитика товаров
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }

    private static async Task OrdersAsync(ITelegramBotClient botClient, Message msg) // статистика по заказам
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }
    
    private static async Task SummaryAsync(ITelegramBotClient botClient, Message msg) // общая сводка
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }
}