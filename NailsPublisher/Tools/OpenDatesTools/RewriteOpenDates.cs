using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.OpenDatesTools;

public static class RewriteOpenDates
{
    public static async Task RewriteOpenDatesAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = db.Chats
                .Include(u => u.Users)
                .ThenInclude(u => u.OpenDates)
                .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            if (chat is null)
            {
                await DbMethods.InitializeDbAsync(msg);
                chat = db.Chats
                    .Include(u => u.Users)
                    .ThenInclude(u => u.OpenDates)
                    .FirstOrDefault(u => u.ChatId == msg.Chat.Id);
            }
            if (chat.LastDateMessageId == 0) await botClient.SendMessage(msg.From.Id,"У вас не установлено отслеживаемое сообщение. Чтобы установить его необходимо 1 раз отправить сообщение в канал: /pset в канале, /csend после этого", ParseMode.Html);
            else
            {
                chat.LastDateMessageId = 0;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id,"Успешно обнулил отслеживаемое сообщение!", ParseMode.Html);
            }
        }
    }
}