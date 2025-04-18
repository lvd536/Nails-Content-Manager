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
            var purchased = products.Count(p => p.IsPurchased);
            var unPurchased = products.Count(p => !p.IsPurchased);
            var purchasedExpense = products.Where(p => p.IsPurchased).Sum(p => p.Price);
            var unPurchasedExpense = products.Where(p => !p.IsPurchased).Sum(p => p.Price);

            var message =
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
        /*
         * TODO: Калькуляция кол-ва времени, которое понадобится для покупки всех некупленных товаров
        */
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var products = user.Products.ToList();
            var postsEarnings = user.Posts.ToList().Sum(p => p.Price);
            var productsEarnings = products.Sum(p => p.Price);
            var purchasedProducts = products.Where(p => p.IsPurchased);
            var unPurchasedProducts = products.Where(p => !p.IsPurchased);
            var purchasedUnPurchasedCoefficient = purchasedProducts.Count() / unPurchasedProducts.Count();
            var purchasedAvg = purchasedProducts.Average(p => p.Price);
            var unPurchasedAvg = unPurchasedProducts.Average(p => p.Price);
            var allProductsAvg = purchasedAvg + unPurchasedAvg;
            
            var message =
                $"<b>Аналитика товаров</b>\n" +
                $"<blockquote> <i>Купленные</i>\n" +
                $"<code> Всего тваров: </code> <b>{purchasedProducts.Count()}</b>\n" +
                $"<code> Ср.Цена: </code> <b>{purchasedAvg}₽</b> </blockquote>\n" +
                $"<blockquote> <i>Некупленные</i>\n" +
                $"<code> Всего тваров: </code> <b>{unPurchasedProducts.Count()}</b>\n" +
                $"<code> Ср.Цена: </code> <b>{unPurchasedAvg}₽</b></blockquote>\n" +
                $"<blockquote> <i>Общая аналитика</i>\n" +
                $"<code> Ср.Цена всех тваров: </code> <b>{allProductsAvg}</b>\n" +
                $"<code> Соотношение цены всех товаров к заработку с постов: </code> <b>{productsEarnings}/{postsEarnings}</b>\n" +
                $"<code> Соотношение цены купленных товаров к заработку с постов: </code> <b>{purchasedProducts.Sum(p => p.Price)}/{postsEarnings}</b>\n" +
                $"<code> Соотношение цены некупленных товаров к заработку с постов: </code> <b>{unPurchasedProducts.Sum(p => p.Price)}/{postsEarnings}</b>\n" +
                $"<code> Коэфф купленных на некупленные товары: </code> <b>{purchasedUnPurchasedCoefficient}</b></blockquote>\n";
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }

    public static async Task OrdersAsync(ITelegramBotClient botClient, Message msg) // статистика по заказам
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var posts = user.Posts.ToList();
            var products = user.Products.ToList();
            var allPostsEarnings = posts.Sum(p => p.Price);
            var monthlyPostsEarnings = posts.Where(p => p.Date >= DateTime.Now.AddMonths(-1));
            var weeklyPostsEarnings = posts.Where(p => p.Date >= DateTime.Now.AddDays(-7));
            var avgPostsEarningsToPurchasedProducts = allPostsEarnings - products.Where(p => p.IsPurchased).Sum(p => p.Price);
            var status = avgPostsEarningsToPurchasedProducts < 0 ? "В минусе" : "В плюсе";

            var message =
                $"<b>Аналитика постов</b>\n" +
                $"<blockquote> <i>За все время</i>\n" +
                $"<code> Всего постов: </code> <b>{posts.Count()}</b>\n" +
                $"<code> Общ.прибыль: </code> <b>{allPostsEarnings}₽</b>\n" +
                $"<code> Соотношение общ. прибыли с постов к расходам на купленные товары: </code> <b>{avgPostsEarningsToPurchasedProducts}₽</b>\n" +
                $"<code> Статус: </code> <b>{status}</b> </blockquote>\n" +
                $"<blockquote> <i>За месяц</i>\n" +
                $"<code> Всего постов за мес.: </code> <b>{monthlyPostsEarnings.Count()}</b>\n" +
                $"<code> Общ.прибыль за мес.: </code> <b>{monthlyPostsEarnings.Sum(p => p.Price)}₽</b></blockquote>\n" +
                $"<blockquote> <i>За неделю</i>\n" +
                $"<code> Всего постов за нед.: </code> <b>{weeklyPostsEarnings.Count()}</b>\n" +
                $"<code> Общ.прибыль за нед.: </code> <b>{weeklyPostsEarnings.Sum(p => p.Price)}₽</b></blockquote>\n";
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }
    
    public static async Task SummaryAsync(ITelegramBotClient botClient, Message msg) // общая сводка
        /*
         * 
         */
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
        }
    }
}