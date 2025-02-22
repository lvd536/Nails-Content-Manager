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
            var post = user.Posts.LastOrDefault();
            
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
            var _chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.Posts)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var _user = _chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            var _post = _user.Posts.Last();
        
            if (_post.Step != "Finally")
            {
                if (_post.Step == "Start")
                {
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Установите описание для поста: ", ParseMode.Markdown);
                    _post.Step = "Description";
                    await db.SaveChangesAsync();
                }
                else if (_post.Step == "Description")
                {
                    _post.Description = msg.Text;
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Вы установили описание поста на {msg.Text}. Напишите Цену для поста: ", ParseMode.Markdown);
                    _post.Step = "Price";
                    await db.SaveChangesAsync();
                }
                else if (_post.Step == "Price")
                {
                    try {
                        _post.Price = short.Parse(msg.Text);
                    } catch (FormatException) {
                        await botClient.SendMessage(msg.Chat.Id, "Для цены необходимо указать <b>только цифры</b>!", ParseMode.Html);
                        return;
                    }
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Вы установили цену поста на {msg.Text}. Отправьте Фото для поста: ", ParseMode.Markdown);
                    _post.Step = "Photo";
                    await db.SaveChangesAsync();
                }
                else if (_post.Step == "Photo")
                {
                    var post = "Ваш пост:" +
                               $"<blockquote><b>Описание:</b> <code>{_post.Description}</code></blockquote>\n" +
                               $"<blockquote><b>Цена:</b> <code>{_post.Price}Р</code></blockquote>";
                    try
                    {
                        await botClient.SendPhoto(msg.Chat.Id, msg.Photo.Last(), post, ParseMode.Html);
                    } catch (ArgumentNullException) {
                        await botClient.SendMessage(msg.Chat.Id, "Вам необхожимо отправить фото, другие виды медиа не принимаются", ParseMode.Html);
                        return;
                    }
                    _post.Step = "Finally";
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}