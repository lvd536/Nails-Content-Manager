using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.OpenDatesTools;

public static class CreateOpenDates
{
    public static async Task CreateOpenDatesAsync(ITelegramBotClient botClient, Message msg, string date, string isOpenCheck)
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
                IsOpen = isOpenCheck == "+"
            };
            user.OpenDates.Add(openDate);
            await db.SaveChangesAsync();
            if (user.ChannelId != 0 && chat.LastDateMessageId != 0) await SendOpenDates.SendOpenDatesAsync(botClient, msg, false);
            await botClient.SendMessage(msg.Chat.Id, "Вы успешно добавили дату в календарь записей!", ParseMode.Html);
        }
    }
}