using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Database;

public static class DbMethods
{
    public static async Task InitializeDbAsync(Message message)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var userData = db.Chats
                .Include(u => u.Users)
                .FirstOrDefault(u => u.ChatId == message.Chat.Id);
            var currentUser = userData?.Users?.FirstOrDefault(u => u.UserId == message.From?.Id);
            if (userData is not null && currentUser is not null) return;
            if (userData?.ChatId is null)
            {
                var newChat = new EntityList.Chat
                {
                    ChatId = message.Chat.Id,
                    IsChannel = message.Chat.Type == ChatType.Private
                };
                db.Chats.Add(newChat);
                await db.SaveChangesAsync();
            }

            userData = db.Chats
                .Include(u => u.Users)
                .FirstOrDefault(u => u.ChatId == message.Chat.Id);
            if (currentUser is null)
            {
                var newUser = new EntityList.User
                {
                    UserId = message.From.Id
                };
                userData?.Users.Add(newUser);
                await db.SaveChangesAsync();
            }
        }
    }
}