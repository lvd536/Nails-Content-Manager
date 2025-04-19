using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.PostTools;

public static class PostCancel
{
    public static async Task PostCancelAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var post = user.Posts.LastOrDefault();
            if (post == null) await PostStart.PostCmdAsync(botClient, msg);
            if (post?.Step == "Finally")
            {
                await botClient.SendMessage(msg.Chat.Id, "У вас нет активных форм создания постов", ParseMode.Html);
            }
            else
            {
                user.Posts.Remove(post);
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.Chat.Id, "Отменил создание поста!", ParseMode.Html);
            }
        }
    }
}