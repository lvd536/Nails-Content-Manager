using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
 
namespace NailsPublisher.OpenDatesTools;

public class OpenDates
{
    public async Task CreateOpenDatesAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.Posts)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
        }
    }
}