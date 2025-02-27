using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.AdminTools;

public static class CheckAdmin
{
    public static async Task<bool> IsAdminCheckAsync(ITelegramBotClient botClient, Message msg)
    {
        if (msg.Chat.Type == ChatType.Channel) return true;
        bool isAdmin = false;
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
                user = chat?.Users.FirstOrDefault(u => u.UserId == msg.From?.Id);
            }
            if (user.UserId == 1016623551) return isAdmin = true;
            if (user.IsAdmin) return isAdmin = true;
            else return isAdmin = false;
        }
        return isAdmin;
    }
}