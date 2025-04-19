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
    { 
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var posts = user.Posts.ToList();
            var products = user.Products.ToList();
            var openDates = user.OpenDates.ToList();

            var totalEarnings = posts.Sum(p => (long)p.Price);
            var totalExpenses = products.Where(p => p.IsPurchased).Sum(p => (long)p.Price);
            var netProfit = totalEarnings - totalExpenses;
            var earningsLast30Days = posts.Where(p => p.Date >= DateTime.Now.AddDays(-30)).Sum(p => (long)p.Price);
            var maxPostEarning = posts.Any() ? posts.Max(p => p.Price) : 0;
            var minPostEarning = posts.Any() ? posts.Min(p => p.Price) : 0;

            var totalPosts = posts.Count;
            var postsLast30Days = posts.Count(p => p.Date >= DateTime.Now.AddDays(-30));
            var postsLast7Days = posts.Count(p => p.Date >= DateTime.Now.AddDays(-7));
            var avgEarningsPerPost = totalPosts > 0 ? (double)totalEarnings / totalPosts : 0;
            var oldestPostDate = posts.Any() ? posts.Min(p => p.Date).ToString("dd.MM.yyyy") : "Нет данных";
            var latestPostDate = posts.Any() ? posts.Max(p => p.Date).ToString("dd.MM.yyyy") : "Нет данных";
            var daysSinceLastPost = posts.Any() ? (DateTime.Now - posts.Max(p => p.Date)).Days : 0;

            var totalProducts = products.Count;
            var purchasedProducts = products.Count(p => p.IsPurchased);
            var productsToPurchase = products.Count(p => !p.IsPurchased);
            var totalCostPurchased = products.Where(p => p.IsPurchased).Sum(p => (long)p.Price);
            var totalCostToPurchase = products.Where(p => !p.IsPurchased).Sum(p => (long)p.Price);
            var avgPricePurchased = purchasedProducts > 0 ? (double)totalCostPurchased / purchasedProducts : 0;
            var avgPriceToPurchase = productsToPurchase > 0 ? (double)totalCostToPurchase / productsToPurchase : 0;
            var mostExpensivePurchased = products.Where(p => p.IsPurchased).Any() ? products.Where(p => p.IsPurchased).Max(p => p.Price) : 0;
            var mostExpensiveToPurchase = products.Where(p => !p.IsPurchased).Any() ? products.Where(p => !p.IsPurchased).Max(p => p.Price) : 0;

            var totalOpenDates = openDates.Count(od => od.IsOpen);
            var totalBookedDates = openDates.Count(od => !od.IsOpen);
            var upcomingOpenDates30Days = openDates.Count(od => od.IsOpen && od.Date >= DateTime.Now && od.Date <= DateTime.Now.AddDays(30));
            var upcomingOpenDates7Days = openDates.Count(od => od.IsOpen && od.Date >= DateTime.Now && od.Date <= DateTime.Now.AddDays(7));
            var nextOpenDate = openDates.Where(od => od.IsOpen && od.Date >= DateTime.Now).Any() ? openDates.Where(od => od.IsOpen && od.Date >= DateTime.Now).Min(od => od.Date).ToString("dd.MM.yyyy") : "Нет данных";
            var daysUntilNextOpen = openDates.Where(od => od.IsOpen && od.Date >= DateTime.Now).Any() ? (openDates.Where(od => od.IsOpen && od.Date >= DateTime.Now).Min(od => od.Date) - DateTime.Now).Days : 0;

            var earningsPerDayLast30 = posts.Where(p => p.Date >= DateTime.Now.AddDays(-30)).Any() ? (double)earningsLast30Days / 30 : 0;
            var expenseRatio = totalEarnings > 0 ? (double)totalExpenses / totalEarnings * 100 : 0;
            var activityRateLast30 = totalPosts > 0 ? (double)postsLast30Days / totalPosts * 100 : 0;

            var message =
                "<b>📊 Общая сводка</b>\n\n" +
                "<b>💰 Финансовый обзор</b>\n" +
                $"<code>Общий доход:</code> <b>{totalEarnings} ₽</b>\n" +
                $"<code>Доход за 30 дней:</code> <b>{earningsLast30Days} ₽</b>\n" +
                $"<code>Общие расходы:</code> <b>{totalExpenses} ₽</b>\n" +
                $"<code>Чистая прибыль:</code> <b>{netProfit} ₽</b>{(netProfit < 0 ? " <i>(дефицит)</i>" : "")}\n" +
                $"<code>Макс. доход с поста:</code> <b>{maxPostEarning} ₽</b>\n" +
                $"<code>Мин. доход с поста:</code> <b>{minPostEarning} ₽</b>\n\n" +
                "<b>📈 Сводка активности</b>\n" +
                $"<code>Всего постов:</code> <b>{totalPosts}</b>\n" +
                $"<code>Постов за 30 дней:</code> <b>{postsLast30Days}</b>\n" +
                $"<code>Постов за 7 дней:</code> <b>{postsLast7Days}</b>\n" +
                $"<code>Средний доход за пост:</code> <b>{avgEarningsPerPost:F2} ₽</b>\n" +
                $"<code>Дата первого поста:</code> <b>{oldestPostDate}</b>\n" +
                $"<code>Дата последнего поста:</code> <b>{latestPostDate}</b>\n" +
                $"<code>Дней с последнего поста:</code> <b>{daysSinceLastPost}</b>\n\n" +
                "<b>🛒 Информация о товарах</b>\n" +
                $"<code>Всего товаров:</code> <b>{totalProducts}</b>\n" +
                $"<code>Купленные товары:</code> <b>{purchasedProducts}</b>\n" +
                $"<code>Товары к покупке:</code> <b>{productsToPurchase}</b>\n" +
                $"<code>Стоимость купленных:</code> <b>{totalCostPurchased} ₽</b>\n" +
                $"<code>Средняя цена купленных:</code> <b>{avgPricePurchased:F2} ₽</b>\n" +
                $"<code>Макс. цена купленного:</code> <b>{mostExpensivePurchased} ₽</b>\n" +
                $"<code>Стоимость к покупке:</code> <b>{totalCostToPurchase} ₽</b>\n" +
                $"<code>Средняя цена к покупке:</code> <b>{avgPriceToPurchase:F2} ₽</b>\n" +
                $"<code>Макс. цена к покупке:</code> <b>{mostExpensiveToPurchase} ₽</b>\n\n" +
                "<b>📅 Доступность</b>\n" +
                $"<code>Всего свободных дат:</code> <b>{totalOpenDates}</b>\n" +
                $"<code>Забронированных дат:</code> <b>{totalBookedDates}</b>\n" +
                $"<code>Свободно на 30 дней:</code> <b>{upcomingOpenDates30Days}</b>\n" +
                $"<code>Свободно на 7 дней:</code> <b>{upcomingOpenDates7Days}</b>\n" +
                $"<code>Следующая свободная дата:</code> <b>{nextOpenDate}</b>\n" +
                $"<code>Дней до след. даты:</code> <b>{daysUntilNextOpen}</b>\n\n" +
                "<b>📉 Дополнительные метрики</b>\n" +
                $"<code>Доход в день (30 дн):</code> <b>{earningsPerDayLast30:F2} ₽</b>\n" +
                $"<code>Доля расходов:</code> <b>{expenseRatio:F1}%</b>\n" +
                $"<code>Активность (30 дн):</code> <b>{activityRateLast30:F1}%</b>";

            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }
}