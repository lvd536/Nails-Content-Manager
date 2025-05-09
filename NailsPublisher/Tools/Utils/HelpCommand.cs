﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools;

public static class HelpCommand
{
    public static async Task HelpCommandAsync(ITelegramBotClient botClient, Message msg)
    {
        var helpText = "<b>📚 Справка по командам бота для мастера маникюра</b>\n\n" +
                       "<b>🔹 Основные команды:</b>\n" +
                       "<code>/help</code> - показать эту справку\n" +
                       "<code>/me</code> - показать ваш Telegram ID\n\n" +
                       
                       "<b>🔹 Управление постами:</b>\n" +
                       "<code>/post</code> - начать создание нового поста\n" +
                       "<code>/pcancel</code> - отменить создание текущего поста\n" +
                       "<code>/pset</code> - установить канал для публикации (запускать в канале)\n" +
                       "<code>/plist</code> - показать список всех созданных постов\n\n" +
                       "<code>/pdelete НОМЕР</code> - удалить пост из списка\n\n" +
                       
                       "<b>🔹 Управление календарём записей:</b>\n" +
                       "<code>/ccreate ДД.ММ ЧЧ:ММ</code> - создать новую дату для записи\n" +
                       "<blockquote>Пример: <code>/ccreate 15.03 14:30</code></blockquote>\n" +
                       "<code>/csend</code> - отправить календарь свободных дат в канал\n" +
                       "<code>/cdelete НОМЕР</code> - удалить дату по номеру\n" +
                       "<blockquote>Пример: <code>/cdelete 5</code></blockquote>\n" +
                       "<code>/crewrite</code> - обнулить отслеживаемое сообщение с календарем\n" +
                       "<code>/clist</code> - показать список всех добавленных дат\n\n" +
                       
                       "<b>🔹 Управление продуктами:</b>\n" +
                       "<code>/shopper</code> - показать список всех продуктов\n" +
                       "<code>/product</code> - запустить цепочку создания продукта\n" +
                       "<code>/productStatus НОМЕР СТАТУС</code> - изменить статус продукта\n" +
                       "<blockquote>Пример: <code>/productStatus 5 +</code></blockquote>\n" +
                       "<code>/productDelete НОМЕР</code> - удалить продукт\n" +
                       "<code>/productCancel</code> - отменить создание продукта\n" +
                       
                       "<b>🔹 Метрики и аналитика:</b>\n" +
                       "<code>/mprofit ДНЕЙ</code> - показать статистику прибыли за последние ДНЕЙ дней (0 для всего времени)\n" +
                       "<blockquote>Пример: <code>/mprofit 30</code></blockquote>\n" +
                       "<code>/mexpense</code> - показать статистику расходов\n" +
                       "<code>/mproducts</code> - показать аналитику товаров, включая расчет времени для покупки некупленных товаров\n" +
                       "<code>/morders</code> - показать статистику по заказам, включая доходы за разные периоды\n" +
                       "<code>/msummary</code> - показать общую сводку с подробной информацией о финансах, активности, товарах и доступности\n\n" +
                       
                       "<b>🔹 Администрирование (только для текущих администраторов):</b>\n" +
                       "<code>/makeadmin ID</code> - выдать права на исп-е бота пользователю\n" +
                       "<blockquote>Пример: <code>/makeadmin 123456789</code></blockquote>\n\n" +
                       
                       "<b>📝 Как создать пост:</b>\n" +
                       "1. Отправьте <code>/post</code>\n" +
                       "2. Введите описание работы\n" +
                       "3. Введите цену (только цифры)\n" +
                       "4. Отправьте фотографию работы\n" +
                       "Готово! Пост будет автоматически опубликован в канале\n\n" +
                       
                       "<b>📚 Как создать продукт:</b>\n" +
                       "1. Отправьте <code>/product</code>\n" +
                       "2. Введите навание продукта\n" +
                       "3. Введите описание продукта\n" +
                       "4. Ввведите цену продукта (только цифры)\n" +
                       "5. Ввведите статус продукта (Куплен или нет. Только + или -)\n" +
                       "Готово! Продукт будет добавлен в ваш шоппер - /shopper\n\n" +
                       
                       "<b>📆 Как управлять календарем записей:</b>\n" +
                       "1. Установите канал с помощью <code>/pset</code> (отправьте в канал)\n" +
                       "2. Создайте даты с помощью <code>/ccreate ДД.ММ ЧЧ:ММ</code>\n" +
                       "3. Отправьте календарь в канал с помощью <code>/csend</code>\n" +
                       "4. Календарь будет автоматически обновляться при добавлении новых дат\n\n" +
                       
                       "<i>Бот автоматически удаляет устаревшие даты из календаря</i>";

        await botClient.SendMessage(msg.Chat.Id, helpText, ParseMode.Html);
    }
}