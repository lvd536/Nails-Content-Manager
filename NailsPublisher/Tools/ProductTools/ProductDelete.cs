using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.PersonalTools;

public static class ProductDelete
{
    public static async Task DeleteProductAsync(ITelegramBotClient botClient, Message msg, int id)
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
            user.Products.Remove(targetProduct);
            await db.SaveChangesAsync();
            await botClient.SendMessage(msg.Chat.Id, $"<i>Успешно удалил продукт под номером {id}!</i>", ParseMode.Html);
        }
    }
}