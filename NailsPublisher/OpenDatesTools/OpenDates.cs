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
            var openDate = new EntityList.OpenDate
            {
                Date = DateTime.Parse(date),
            };
            user.OpenDates.Add(openDate);
            await db.SaveChangesAsync();
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
            var channel = user.ChannelId != 0 ? user.ChannelId : msg.Chat.Id;
            var message = "Календарь записей:\n";
            if (user.OpenDates.Count < 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных записей. Чтобы отправить календарь в канал создайте хотябы 1 запись с помощью /create", ParseMode.Html);
                message += "Пусто.";
            }
            else
            {
                foreach (var d in user.OpenDates.OrderBy(d => d.Date))
                {
                    message +=$"{d.Date:dd.MM HH:mm}\n";
                }
            }
            var sendMessage = await botClient.SendMessage(channel, message, ParseMode.Html);
            await botClient.PinChatMessage(sendMessage.Chat.Id, sendMessage.MessageId);
        }
    }
}