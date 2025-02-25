using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.PostTools;

public static class PostCreator
{
    public static async Task PostCmd(ITelegramBotClient botClient, Message msg)
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
                if (user?.Posts.Count < 1)
                {
                    EntityList.Post newPost = new EntityList.Post
                    {
                        Step = "Start",
                        Description = string.Empty,
                        Price = 0,
                        User = user
                    };
                    user.Posts.Add(newPost);
                    await db.SaveChangesAsync();
                }
                else if (user?.Posts.Count >= 1 && post?.Step == "Finally")
                {
                    user.Posts.Clear();
                    EntityList.Post newPost = new EntityList.Post()
                    {
                        Step = "Start",
                        Description = string.Empty,
                        Price = 0,
                        User = user
                    };
                    user.Posts.Add(newPost);
                    await db.SaveChangesAsync();
                }

                await PostLoop(botClient, msg);
            }
        }
    }

    public static async Task PostLoop(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.Posts)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            var post = user?.Posts.LastOrDefault();

            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.Posts)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
                post = user?.Posts.LastOrDefault();
            }

            if (post?.Step == "Finally") return;
            
            if (post?.Step == "Start")
            {
                await botClient.SendMessage(msg.Chat.Id,
                    $"Установите описание для поста: ", ParseMode.Markdown);
                post.Step = "Description";
                await db.SaveChangesAsync();
            }
            else if (post?.Step == "Description")
            {
                post.Description = msg.Text;
                await botClient.SendMessage(msg.Chat.Id,
                    $"Вы установили описание поста на {msg.Text}. Напишите Цену для поста: ", ParseMode.Markdown);
                post.Step = "Price";
                await db.SaveChangesAsync();
            }
            else if (post?.Step == "Price")
            {
                try
                {
                    post.Price = short.Parse(msg.Text);
                }
                catch (FormatException)
                {
                    await botClient.SendMessage(msg.Chat.Id, "Для цены необходимо указать <b>только цифры</b>!",
                        ParseMode.Html);
                    return;
                }

                await botClient.SendMessage(msg.Chat.Id,
                    $"Вы установили цену поста на {msg.Text}. Отправьте Фото для поста: ", ParseMode.Markdown);
                post.Step = "Photo";
                await db.SaveChangesAsync();
            }
            else if (post?.Step == "Photo")
            {
                var message =
                    $"<blockquote><b>Описание:</b> <code>{post.Description}</code></blockquote>\n" +
                    $"<blockquote><b>Цена:</b> <code>{post.Price}Р</code></blockquote>";
                var channel = user.ChannelId != 0 ? user.ChannelId : msg.Chat.Id;
                try
                {
                    await botClient.SendPhoto(channel, msg.Photo.Last(), message, ParseMode.Html);
                    await botClient.SendMessage(msg.From.Id, "Успешно отправил пост!", ParseMode.Html);
                }
                catch (ArgumentNullException)
                {
                    await botClient.SendMessage(msg.From.Id,
                        "Вам необходимо отправить фото. Другие виды медиа не принимаются", ParseMode.Html);
                    return;
                }

                post.Step = "Finally";
                await db.SaveChangesAsync();
            }
        }
    }

    public static async Task PostCancel(ITelegramBotClient botClient, Message msg)
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

    public static async Task SetChannel(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.Posts)
                .FirstOrDefault(u => u.ChatId == msg.From.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            if (user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.Posts)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From.Id);
            }

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