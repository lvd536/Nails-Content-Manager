using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.PostTools;

public static class PostStart
{
    public static async Task PostCmdAsync(ITelegramBotClient botClient, Message msg)
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

            if (post?.Step == "Finally" || user?.Posts.Count < 1)
            {
                EntityList.Post newPost = new EntityList.Post
                {
                    Step = "Start",
                    Description = string.Empty,
                    Price = 0,
                    Date = DateTime.Now,
                    User = user
                };
                user.Posts.Add(newPost);
                await db.SaveChangesAsync();

                await PostLoop.PostLoopAsync(botClient, msg);
            }
        }
    }
}