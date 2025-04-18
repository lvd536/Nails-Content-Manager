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
         * Всего купленных товаров +
         * Всего не купленных товаров +
         * Коэф купленных на некупленные +
         * Средняя цена купленных товаров +
         * Средняя цена не купленных товаров +
         * Средняя стоимость товаров в общем +
         * Соотношение цены купленных и некупленных товаров к заработку с постов +
         * Калькуляция кол-ва времени, которое понадобится для покупки всех некупленных товаров
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
                $"<code> Соотношение цены всех товаров к заработку с постов: </code> <b>{postsEarnings}/{productsEarnings}</b>\n" +
                $"<code> Соотношение цены купленных товаров к заработку с постов: </code> <b>{postsEarnings}/{purchasedProducts.Sum(p => p.Price)}</b>\n" +
                $"<code> Соотношение цены некупленных товаров к заработку с постов: </code> <b>{postsEarnings}/{unPurchasedProducts.Sum(p => p.Price)}</b>\n" +
                $"<code> Коэфф купленных на некупленные товары: </code> <b>{purchasedUnPurchasedCoefficient}</b></blockquote>\n";
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }

    public static async Task OrdersAsync(ITelegramBotClient botClient, Message msg) // статистика по заказам
    {
        /*
         * Всего постов + цена постов за все время
         * Постов + цена постов за месяц
         * Постов + цена постов за неделю
         * Соотношение общей прибыли с постов к расходам на купленные продукты
         */
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
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