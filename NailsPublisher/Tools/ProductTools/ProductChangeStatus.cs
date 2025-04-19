using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.PersonalTools;

public static class ProductChangeStatus
{
    public static async Task ChangeProductStatusAsync(ITelegramBotClient botClient, Message msg, int id, bool status)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var targetProduct = user.Products.FirstOrDefault(p => p.Id == id);

            if (targetProduct == null)
            {
                await botClient.SendMessage(msg.Chat.Id, $"<i>Продукт под номером {id} не найден</i>", ParseMode.Html);
                return;
            }
            if (targetProduct.IsPurchased == status)
            {
                await botClient.SendMessage(msg.Chat.Id, $"<i>Продукт под номером {id} уже имеет такой статус</i>", ParseMode.Html);
                return;
            }
            
            targetProduct.IsPurchased = status;
            await db.SaveChangesAsync();
            await ProductList.ShopperListCmdAsync(botClient, msg);
            await botClient.SendMessage(msg.Chat.Id, $"<i>Установил статус {(status ? "Куплено" : "Некуплено")} продукту под номером {id}!</i>", ParseMode.Html);
        }
    }
}