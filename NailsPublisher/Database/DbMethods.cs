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
                .FirstOrDefault(u => u.ChatId == message.From.Id);
            var currentUser = userData?.Users?.FirstOrDefault(u => u.UserId == message.From?.Id);
            if (userData is not null && currentUser is not null) return;
            if (userData?.ChatId is null)
            {
                var newChat = new EntityList.Chat
                {
                    ChatId = message.From.Id,
                    IsChannel = message.Chat.Type == ChatType.Channel
                };
                db.Chats.Add(newChat);
                await db.SaveChangesAsync();
            }

            userData = db.Chats
                .Include(u => u.Users)
                .FirstOrDefault(u => u.ChatId == message.From.Id);
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
    public static async Task<EntityList.Chat> GetChatByMessageAsync(ApplicationContext db, Message msg)
    {
            var chat = await db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .Include(u => u.Users)
                .ThenInclude(p => p.Posts)
                .FirstOrDefaultAsync(u => u.ChatId == msg.From.Id);
            if (chat is null)
            {
                await InitializeDbAsync(msg);
                chat = await db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .Include(u => u.Users)
                    .ThenInclude(p => p.Posts)
                    .FirstOrDefaultAsync(u => u.ChatId == msg.From.Id);
            }
            return chat;
    }
    public static async Task<EntityList.User> GetUserByChatAsync(ApplicationContext db, EntityList.Chat chat, Message msg)
    {
            var user = chat.Users.FirstOrDefault(u => u.UserId == msg.From.Id);
            if (user is null)
            {
                await InitializeDbAsync(msg);
                user = chat.Users.FirstOrDefault(u => u.UserId == msg.From.Id);
            }
            return user;
    }
    public static async Task<EntityList.User> SearchUserByChatAsync(ApplicationContext db, ITelegramBotClient bot, Message msg, long targetUid)
    {
            var chat = await db.Chats
                .Include(u => u.Users)
                .FirstOrDefaultAsync(u => u.ChatId == targetUid);
            var user = chat?.Users.FirstOrDefault(u => u.UserId == targetUid);
            if (chat is null || user is null) await bot.SendMessage(msg.Chat.Id, "Данного пользователя нет в базе данных", ParseMode.Html);
            return user;
    }
}