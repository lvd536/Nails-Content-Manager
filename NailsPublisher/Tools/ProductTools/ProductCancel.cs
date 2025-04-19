using NailsPublisher.Database;
using NailsPublisher.PostTools;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.PersonalTools;

public static class ProductCancel
{
    public static async Task ProductCancelAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var product = user.Products.LastOrDefault();
            if (product?.Step == 5)
            {
                await botClient.SendMessage(msg.Chat.Id, "У вас нет активных форм создания продуктов", ParseMode.Html);
            }
            else
            {
                user.Products.Remove(product);
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.Chat.Id, "Отменил создание продукта!", ParseMode.Html);
            }
        }
    }
}