using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
 
namespace NailsPublisher.OpenDatesTools;

public static class OpenDates
{
    public static async Task CreateOpenDatesAsync(ITelegramBotClient botClient, Message msg, string date)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            }
            var openDate = new EntityList.OpenDate
            {
                Date = DateTime.Parse(date),
            };
            user.OpenDates.Add(openDate);
            await db.SaveChangesAsync();
            await SendOpenDatesAsync(botClient, msg);
            await botClient.SendMessage(msg.Chat.Id, "Вы успешно добавили дату в календарь записей!", ParseMode.Html);
        }
    }
    public static async Task SendOpenDatesAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            }
            var channel = user.ChannelId != 0 ? user.ChannelId : msg.Chat.Id;
            var message = "Owww nails౨ৎ\n" +
                          "Записи:\n";
            var expiredDates = "Удаленные истекшие даты:\n";
            if (user.OpenDates.Count <= 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных записей. Чтобы отправить календарь в канал создайте хотябы 1 запись с помощью /ccreate", ParseMode.Html);
                return;
            }
            else
            {
                foreach (var d in user.OpenDates.OrderBy(d => d.Date))
                {
                    if (d.Date < DateTime.Today)
                    {
                        expiredDates +=$"{d.Date:dd.MM HH:mm}\n";
                        db.Remove(d);
                        await db.SaveChangesAsync();
                        continue;
                    }
                    message += $"<blockquote><b>Номер записи:</b> <i>{d.Id}</i>\n" +
                               $"<b>Дата:</b> <code>{d.Date:dd.MM HH:mm}</code></blockquote>\n";
                }
            }
            if (chat.LastDateMessageId != 0)
            {
                try
                { 
                    await botClient.EditMessageText(user.ChannelId, chat.LastDateMessageId, message, ParseMode.Html);
                    await botClient.SendMessage(msg.From.Id,"Календарь успешно обновлен\n" + expiredDates, ParseMode.Html);
                } catch (Exception)
                {
                    await botClient.SendMessage(msg.From.Id,"Прошлое сообщение небыло найдено или не имеет изменений. Если хотите обнулить отслеживаемое сообщение, напишите: /crewrite", ParseMode.Html);
                }
            }
            else
            {
                var sendMessage = await botClient.SendMessage(channel, message, ParseMode.Html);
                await botClient.PinChatMessage(sendMessage.Chat.Id, sendMessage.MessageId);
                chat.LastDateMessageId = sendMessage.MessageId;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id,"Календарь свободных и занятых дат был успешно отправлен. Впредь изменения будут появляться в этом сообщении. Если вы хотите пересоздать календарь - просто удалите сообщение с календарем", ParseMode.Html);
            }
        }
    }
    public static async Task RewriteOpenDatesAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            if (chat is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            }
            if (chat.LastDateMessageId == 0) await botClient.SendMessage(msg.From.Id,"У вас не установлено отслеживаемое сообщение. Чтобы установить его необходимо 1 раз отправить сообщение в канал: /pset в канале, /csend после этого", ParseMode.Html);
            else
            {
                chat.LastDateMessageId = 0;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id,"Успешно обнулил отслеживаемое сообщение!", ParseMode.Html);
            }
        }
    }
    public static async Task OpenDateListCmdAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            var datesList = user.OpenDates.OrderBy(d => d.Date).ToList();
            var message = "Календарь:\n";
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            }
            if (user.OpenDates.Count <= 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных записей. Чтобы посмотреть календарь создайте хотябы 1 запись с помощью /ccreate", ParseMode.Html);
                return;
            }
            foreach (var d in datesList)
            {
                message += $"<blockquote><b>Номер записи:</b> <i>{d.Id}</i>\n" +
                           $"<b>Дата:</b> <code>{d.Date:dd.MM HH:mm}</code></blockquote>\n";
            }
            await botClient.SendMessage(msg.Chat.Id, message, ParseMode.Html);
        }
    }
    public static async Task OpenDateDeleteCmdAsync(ITelegramBotClient botClient, Message msg, int id)
    {
        if (id == -1)
        {
            await botClient.SendMessage(msg.Chat.Id, "Вы не указали <b>номер</b> даты, которую хотите удалить!", ParseMode.Html);
            await OpenDateListCmdAsync(botClient, msg);
            return;
        }
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            var datesList = user?.OpenDates.ToList();
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            }
            if (user.OpenDates.Count <= 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных записей. Создайте хотябы 1 запись с помощью /ccreate", ParseMode.Html);
                return;
            }
            if (!datesList.Any(d => d.Id == id))
            {
                await botClient.SendMessage(msg.Chat.Id, "ID слишком большое, даты под этим номером нет!", ParseMode.Html);
                await OpenDateListCmdAsync(botClient, msg);
                return;
            }
            var removeEntity = datesList.FirstOrDefault(d => d.Id == id);
            user.OpenDates.Remove(removeEntity);
            await db.SaveChangesAsync();
            await botClient.SendMessage(msg.Chat.Id, $"Дата {removeEntity.Date} успешно удалена!", ParseMode.Html);
        }
    }
}