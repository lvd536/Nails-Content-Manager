using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.PostTools;

public static class PostLoop
{
    public static async Task PostLoopAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var post = user.Posts.LastOrDefault();

            if (post?.Step == "Finally") return;

            switch (post?.Step)
            {
                case "Start":
                {
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Установите описание для поста: ", ParseMode.Html);
                    post.Step = "Description";
                    await db.SaveChangesAsync();
                    break;
                }
                case "Description":
                {
                    if (msg.Text == "-")
                    {
                        post.Description = msg.Text;
                        await botClient.SendMessage(msg.Chat.Id,
                            $"Пост не будет содержать описание. Напишите Цену для поста: ", ParseMode.Html);
                        post.Step = "Price";
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        post.Description = msg.Text;
                        await botClient.SendMessage(msg.Chat.Id,
                            $"Вы установили описание поста на {msg.Text}. Напишите Цену для поста: ", ParseMode.Html);
                        post.Step = "Price";
                        await db.SaveChangesAsync();
                    }

                    break;
                }
                case "Price":
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
                        $"Вы установили цену поста на {msg.Text}. Отправьте Фото или Видео для поста: ",
                        ParseMode.Html);
                    post.Step = "Photo";
                    await db.SaveChangesAsync();
                    break;
                }
                case "Photo":
                {
                    var channel = user.ChannelId != 0 ? user.ChannelId : msg.Chat.Id;
                    var message = String.Empty;
                    if (post.Description == "-")
                    {
                        message = $"<blockquote><b>Цена:</b> <code>{post.Price}Р</code></blockquote>";
                    }
                    else
                    {
                        message =
                            $"<blockquote><b>Описание:</b> <code>{post.Description}</code></blockquote>\n" +
                            $"<blockquote><b>Цена:</b> <code>{post.Price}Р</code></blockquote>";
                    }

                    try
                    {
                        if (msg.Type == MessageType.Photo)
                        {
                            await botClient.SendPhoto(channel, msg.Photo.Last(), message, ParseMode.Html);
                        }
                        else if (msg.Type == MessageType.Video)
                        {
                            await botClient.SendVideo(channel, msg.Video, message, ParseMode.Html);
                        }
                        else
                        {
                            await botClient.SendMessage(msg.From.Id,
                                "Вы можете указать в этом пункте <b>только</b> фото или видео!", ParseMode.Html);
                            return;
                        }

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
                    break;
                }
            }
        }
    }
}