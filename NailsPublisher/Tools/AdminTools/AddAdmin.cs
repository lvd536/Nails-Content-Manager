using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.AdminTools;

public static class AddAdmin
{
    public static async Task AddAdminAsync(ITelegramBotClient botClient, Message msg, long targetUid)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var user = await DbMethods.SearchUserByChatAsync(db, botClient, msg, targetUid);
            if (msg.From.Id != 1016623551) return;
            if(user.UserId == targetUid)
            {
                user.IsAdmin = true;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id, $"Выдал пользователю <code>{user.UserId}</code> роль администратора!", ParseMode.Html);
            }
        }
    }
}