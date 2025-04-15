using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace NailsPublisher.OpenDatesTools;

public static class OpenDateDelete
{
    public static async Task OpenDateDeleteCmdAsync(ITelegramBotClient botClient, Message msg, int id)
    {
        if (id == -1)
        {
            await botClient.SendMessage(msg.Chat.Id, "Вы не указали <b>номер</b> даты, которую хотите удалить!", ParseMode.Html);
            await OpenDateList.OpenDateListCmdAsync(botClient, msg);
            return;
        }
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var datesList = user?.OpenDates.ToList();

            if (user.OpenDates.Count <= 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных записей. Создайте хотябы 1 запись с помощью /ccreate", ParseMode.Html);
                return;
            }
            if (!datesList.Any(d => d.Id == id))
            {
                await botClient.SendMessage(msg.Chat.Id, "ID слишком большое, даты под этим номером нет!", ParseMode.Html);
                await OpenDateList.OpenDateListCmdAsync(botClient, msg);
                return;
            }
            var removeEntity = datesList.FirstOrDefault(d => d.Id == id);
            user.OpenDates.Remove(removeEntity);
            await db.SaveChangesAsync();
            await botClient.SendMessage(msg.Chat.Id, $"Дата {removeEntity.Date} успешно удалена!", ParseMode.Html);
            if (user.ChannelId != 0 && chat.LastDateMessageId != 0) await SendOpenDates.SendOpenDatesAsync(botClient, msg, false);
        }
    }
}