using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.PostTools;

public static class PostSetChannel
{
    public static async Task SetChannelAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);

            if (msg.Chat.Type == ChatType.Channel)
            {
                user.ChannelId = msg.Chat.Id;
                await db.SaveChangesAsync();
                await botClient.DeleteMessage(msg.Chat.Id, msg.MessageId);
            }
            else
                await botClient.SendMessage(msg.Chat.Id,
                    "Вы не можете установить канал для отправки поста в личных сообщениях!", ParseMode.Html);
        }
    }
}