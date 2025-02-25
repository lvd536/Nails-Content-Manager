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
            var chat = db.Chats
                .Include(u => u.Users)
                .FirstOrDefault(u => u.ChatId == targetUid);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == targetUid);
            if (chat is null || user is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .FirstOrDefault(u => u.ChatId == targetUid);
                user = chat?.Users.FirstOrDefault(u => u.UserId == targetUid);
            }
            if (msg.From.Id != 1016623551) return;
            if (user is null) await botClient.SendMessage(msg.From.Id, "Такого пользователя нет в базе данных", ParseMode.Html);
            else
            {
                user.IsAdmin = true;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id, $"Выдал пользователю <code>{user.UserId}</code> роль администратора!", ParseMode.Html);
            }
        }
    }
}