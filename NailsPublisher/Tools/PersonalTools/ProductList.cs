using NailsPublisher.Database;
using NailsPublisher.PostTools;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.PersonalTools;

public static class ProductList
{
    public static async void ShopperListCmdAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var message = "Список ваших товаров:\n";
            var productList = user.Products.ToList();
            var totalPrice = 0;

            foreach (var p in productList)
            {
                string purchaseStatus = p.IsPurchased ? "Куплен" : "Не куплен";
                message += $"<blockquote><b>Товар №:</b> <i>{p.Id}</i>\n" +
                           $"<b>Название:</b> <code>{p.Description}</code>\n" +
                           $"<b>Описание:</b> <code>{p.Description}</code>\n" +
                           $"<b>Цена:</b> <code>{p.Price}</code>\n" +
                           $"<b>Статус покупки:</b> <code>{purchaseStatus}</code></blockquote>\n";
                if (p.IsPurchased) totalPrice += p.Price;
            }
            message += $"\n<blockquote><b>Общие затраты:</b> <code>{totalPrice}</code>\n" +
                       $"<b>Кол-во товаров:</b> <code>{user.Products.Count}</code>" +
                       "<i>Для просмотра подробной статистики по затратам - /smetrics</i>/blockquote>";
            
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }
}