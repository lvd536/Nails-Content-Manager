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
            var targetUser = chat?.Users.FirstOrDefault(u => u.UserId == targetUid);
            if (user?.UserId != 1016623551) return;
            if (targetUser is null) await botClient.SendMessage(msg.Chat.Id, "Такого пользователя нет в базе данных", ParseMode.Html);
            else
            {
                targetUser.IsAdmin = true;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.Chat.Id, $"Выдал пользователю <code>{targetUser.UserId}</code> роль администратора!", ParseMode.Html);
            }
        }
    }
}