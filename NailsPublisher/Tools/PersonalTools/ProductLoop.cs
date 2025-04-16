using NailsPublisher.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.Tools.PersonalTools;

public static class ProductLoop
{
    public static async Task ProductLoopAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var product = user.Products.LastOrDefault();
            if (product?.Step == 5) return;

            switch (product?.Step)
            {
                case 0:
                {
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Установите название продукта: ", ParseMode.Html);
                    product.Step = 1;
                    await db.SaveChangesAsync();
                    break;
                }
                case 1:
                {
                    product.Name = msg.Text;
                    product.Step = 2;
                    await botClient.SendMessage(msg.Chat.Id,
                        $"Вы успешно установили название товара на {msg.Text}. Установите описание товара",
                        ParseMode.Html);
                    await db.SaveChangesAsync();
                    break;
                }
                case 2:
                {
                    if (msg.Text == "-")
                    {
                        product.Description = msg.Text;
                        await botClient.SendMessage(msg.Chat.Id,
                            $"Товар не будет содержать описание. Напишите Цену товара: ", ParseMode.Html);
                    }
                    else
                    {
                        product.Description = msg.Text;
                        await botClient.SendMessage(msg.Chat.Id,
                            $"Вы установили описание товара на {msg.Text}. Напишите Цену товара: ", ParseMode.Html);
                    }

                    product.Step = 3;
                    await db.SaveChangesAsync();
                    break;
                }
                case 3:
                {
                    try
                    {
                        product.Price = short.Parse(msg.Text);
                    }
                    catch (FormatException)
                    {
                        await botClient.SendMessage(msg.Chat.Id,
                            "Для цены товара необходимо указать <b>только цифры</b>!", ParseMode.Html);
                        return;
                    }

                    await botClient.SendMessage(msg.Chat.Id,
                        $"Вы установили цену товара на {msg.Text}. Куплен товар или нет? +/- ", ParseMode.Html);
                    product.Step = 4;
                    await db.SaveChangesAsync();
                    break;
                }
                case 4:
                {
                    product.IsPurchased = msg.Text == "+";
                    string purchaseStatus = product.IsPurchased ? "Куплен" : "Не куплен";
                    var message = $"<blockquote><b>Товар №:</b> <i>{product.Id}</i>\n" +
                                  $"<b>Название:</b> <code>{product.Name}</code>\n" +
                                  $"<b>Описание:</b> <code>{product.Description}</code>\n" +
                                  $"<b>Цена:</b> <code>{product.Price}</code>\n" +
                                  $"<b>Статус покупки:</b> <code>{purchaseStatus}</code></blockquote>\n";
                    await botClient.SendMessage(msg.From.Id, $"Вы успешно создали товар!\n" + message, ParseMode.Html);
                    product.Step = 5;
                    await db.SaveChangesAsync();
                    break;
                }
            }
        }
    }
}