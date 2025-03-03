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
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
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