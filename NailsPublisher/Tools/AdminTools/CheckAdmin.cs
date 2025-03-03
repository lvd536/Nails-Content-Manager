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
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            if (user.UserId == 1016623551) return isAdmin = true;
            if (user.IsAdmin) return isAdmin = true;
            else return isAdmin = false;
        }
        return isAdmin;
    }
}