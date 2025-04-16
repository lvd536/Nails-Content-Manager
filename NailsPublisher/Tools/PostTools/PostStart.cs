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
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var post = user.Posts.LastOrDefault();

            if (post?.Step == "Finally" || user.Posts.Count < 1)
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
            }
        }
        await PostLoop.PostLoopAsync(botClient, msg);
    }
}