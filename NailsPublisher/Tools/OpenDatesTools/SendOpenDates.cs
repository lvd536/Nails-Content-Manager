﻿using Microsoft.EntityFrameworkCore;
using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailsPublisher.OpenDatesTools;

public static class SendOpenDates
{
    public static async Task SendOpenDatesAsync(ITelegramBotClient botClient, Message msg, bool update)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var channel = user.ChannelId != 0 ? user.ChannelId : msg.Chat.Id;
            var message = "Owww nails౨ৎ\n" +
                          "Записи:\n";
            var expiredDates = "Удаленные истекшие даты:\n";
            var buttons = new InlineKeyboardMarkup()
                .AddButton(InlineKeyboardButton.WithUrl("Записаться", "https://t.me/oliweeshka"))
                .AddNewRow()
                .AddButton(InlineKeyboardButton.WithUrl("Заказать бота", "https://t.me/lvdshka"));
            if (user.OpenDates.Count <= 0)
            {
                await botClient.SendMessage(msg.From.Id,"У вас нет добавленных/измененных записей. Чтобы отправить календарь в канал создайте хотябы 1 запись с помощью /ccreate", ParseMode.Html);
                return;
            }

            foreach (var d in user.OpenDates.OrderBy(d => d.Date))
            {
                if (d.Date < DateTime.Today)
                {
                    expiredDates +=$"{d.Date:dd.MM HH:mm}\n";
                    db.Remove(d);
                    await db.SaveChangesAsync();
                    continue;
                }
                var closedCheck = d.IsOpen ? "✅" : "❌";
                message += $"<blockquote><b>Дата:</b> <code>{d.Date:dd.MM}</code>\n" +
                           $"<b>Время:</b> <code>{d.Date:HH:mm}</code>\n" +
                           $"<b>Занято:</b> <code>{closedCheck} </code></blockquote>\n";
            }
            
            if (chat.LastDateMessageId != 0)
            {
                try
                { 
                    await botClient.EditMessageText(user.ChannelId, chat.LastDateMessageId, message, ParseMode.Html, replyMarkup: buttons);
                    if (update) await botClient.SendMessage(msg.From.Id,"Календарь успешно обновлен\n" + expiredDates, ParseMode.Html);
                } catch (Exception ex)
                {
                    if (ex.Message.Contains("message is not modified"))
                    {
                        await botClient.SendMessage(msg.From.Id, "Вы не добавили новых дат чтобы изменять сообщение", ParseMode.Html);
                        return;
                    }
                    if (ex.Message.Contains("MESSAGE_ID_INVALID"))
                    {
                        await botClient.SendMessage(msg.From.Id, "Сообщение с календарем было удалено. Чтобы обнулить отслеживаемое сообщение, напишите: /crewrite", ParseMode.Html);
                        return;
                    }
                    await botClient.SendMessage(msg.From.Id,"Прошлое сообщение небыло найдено. Если хотите обнулить отслеживаемое сообщение, напишите: /crewrite", ParseMode.Html);
                }
            }
            else
            {
                var sendMessage = await botClient.SendMessage(channel, message, ParseMode.Html, replyMarkup: buttons);
                await botClient.PinChatMessage(sendMessage.Chat.Id, sendMessage.MessageId);
                chat.LastDateMessageId = sendMessage.MessageId;
                await db.SaveChangesAsync();
                await botClient.SendMessage(msg.From.Id,"Календарь свободных и занятых дат был успешно отправлен. Впредь изменения будут появляться в этом сообщении. Если вы хотите пересоздать календарь - просто удалите сообщение с календарем", ParseMode.Html);
            }
        }
    }
}