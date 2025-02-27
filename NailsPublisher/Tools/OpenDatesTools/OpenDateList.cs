using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.OpenDatesTools;

public static class OpenDateList
{
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
}