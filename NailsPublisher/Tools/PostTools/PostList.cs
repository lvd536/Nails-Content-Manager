using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.PostTools;

public static class PostList
{
        public static async Task PostListCmdAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var post = user?.Posts.LastOrDefault();
            var message = "Список ваших постов:\n";
            var postList = user.Posts.ToList();
            
            if (post?.Step != "Finally")
            {
                await botClient.SendMessage(msg.Chat.Id,"У вас есть незаконченная форма создания поста. Напишите /pcancel чтобы остановить создание поста", ParseMode.Html);
                await PostLoop.PostLoopAsync(botClient, msg);
                return;
            }
            if (user.Posts.Count <= 0)
            {
                await botClient.SendMessage(msg.Chat.Id, "У вас нет созданных постов. Напишите /post чтобы создать его", ParseMode.Html);
                return;
            }

            foreach (var p in postList)
            {
                message += $"<blockquote><b>Номер поста:</b> <i>{p.Id}</i>\n" +
                           $"<b>Описание поста:</b> <code>{p.Description}</code>\n" +
                           $"<b>Цена:</b> <code>{p.Price}</code>\n" +
                           $"<b>Дата создания:</b> <code>{p.Date}</code></blockquote>\n";
            }
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }
}