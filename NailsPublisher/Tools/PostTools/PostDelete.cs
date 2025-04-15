using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.PostTools;

public static class PostDelete
{
    public static async Task PostDeleteCmdAsync(ITelegramBotClient botClient, Message msg, short postId)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var postStep = user.Posts.LastOrDefault()?.Step;

            if (postStep != "Finally")
            {
                await botClient.SendMessage(msg.Chat.Id,
                    "У вас есть незаконченная форма создания поста. Напишите /pcancel чтобы остановить создание поста",
                    ParseMode.Html);
                await PostLoop.PostLoopAsync(botClient, msg);
                return;
            }
            
            if (user.Posts.Any(p => p.Id == postId))
            {
                var targetPost = user.Posts.FirstOrDefault(p => p.Id == postId);
                user.Posts.Remove(targetPost);
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.Chat.Id, $"Вы успешно удалили пост под номером {postId}", ParseMode.Html);
            }
            else await botClient.SendMessage(msg.Chat.Id, $"У вас нет поста под номером {postId}", ParseMode.Html);
            
            await PostList.PostListCmdAsync(botClient, msg);
        }
    }
}