using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.Metrics;

public static class MetricsCommand
{
    public static async Task ProfitAsync(ITelegramBotClient botClient, Message msg,  int daysPeriod) // статистика прибыли
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var posts = user.Posts.ToList();
            var profit = 0;
            var postCount = 0;
            var message = string.Empty;
            foreach (var p in posts)
            {
                if (p.Date >= DateTime.Now.AddDays(-daysPeriod) && daysPeriod is not 0)
                {
                    profit += p.Price;
                    postCount++;
                }
                else if (daysPeriod is 0)
                {
                    profit += p.Price;
                    postCount++;
                }
            }

            if (daysPeriod is not 0)
            {
                message =
                    $"<blockquote> <b>Статистика по прибыли за промежуток {DateTime.Now.AddDays(-daysPeriod)} - {DateTime.Now}</b>\n" +
                    $"<code> Всего постов за промежуток: </code> <b>{postCount}</b>\n" +
                    $"<code> Общая прибыль за промежуток: </code> <b>{profit}</b> </blockquote>\n";
            }
            else
            {
                message =
                    $"<blockquote> <b>Статистика по прибыли за все время</b>\n" +
                    $"<code> Всего постов: </code> <b>{postCount}</b>\n" +
                    $"<code> Общая прибыль: </code> <b>{profit}</b> </blockquote>\n";
            }
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }

    public static async Task ExpensesAsync(ITelegramBotClient botClient, Message msg) // статистика по расходам
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var products = user.Products.ToList();
            var purchasedExpense = 0;
            var unPurchasedExpense = 0;
            var purchased = 0;
            var unPurchased = 0;
            var message = string.Empty;
            foreach (var p in products)
            {
                if (p.IsPurchased)
                {
                    purchasedExpense += p.Price;
                    purchased++;
                }
                else
                {
                    unPurchasedExpense += p.Price;
                    unPurchased++;
                }
            }


            message =
                $"<b>Статистика по расходам</b>\n" +
                $"<blockquote> <i>Купленные</i>\n" +
                $"<code> Всего тваров: </code> <b>{purchased}</b>\n" +
                $"<code> Расход: </code> <b>{purchasedExpense}₽</b> </blockquote>\n" +
                $"<blockquote> <i>Некупленные</i>\n" +
                $"<code> Всего тваров: </code> <b>{unPurchased}</b>\n" +
                $"<code> Расход: </code> <b>{unPurchasedExpense}₽</b></blockquote>\n" +
                $"<blockquote> <i>Общая аналитика расходов</i>\n" +
                $"<code> Всего тваров: </code> <b>{purchased + unPurchased}</b>\n" +
                $"<code> Расход: </code> <b>{purchasedExpense + unPurchasedExpense}₽</b></blockquote>\n";
            
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }

    public static async Task ProductsAsync(ITelegramBotClient botClient, Message msg) // аналитика товаров
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }

    public static async Task OrdersAsync(ITelegramBotClient botClient, Message msg) // статистика по заказам
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }
    
    public static async Task SummaryAsync(ITelegramBotClient botClient, Message msg) // общая сводка
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }
}