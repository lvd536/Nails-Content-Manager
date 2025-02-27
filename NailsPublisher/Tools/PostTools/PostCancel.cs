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
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.Posts)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            var post = user?.Posts.LastOrDefault();

            if (chat is null || user is null) await DbMethods.InitializeDbAsync(msg);

            if (post.Step == "Finally")
                await botClient.SendMessage(msg.Chat.Id, "У вас нет активных форм создания постов", ParseMode.Html);
            else
            {
                post.Step = "Finally";
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.Chat.Id, "Отменил создание поста!", ParseMode.Html);
            }
        }
    }
}