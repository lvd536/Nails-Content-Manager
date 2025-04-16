using NailsPublisher.Database;
using NailsPublisher.PostTools;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailsPublisher.Tools.PersonalTools;

public class ProductStart
{
    public static async Task ProductCmdAsync(ITelegramBotClient botClient, Message msg)
    {
        using (ApplicationContext db = new ApplicationContext())
        {
            var chat = await DbMethods.GetChatByMessageAsync(db, msg);
            var user = await DbMethods.GetUserByChatAsync(db, chat, msg);
            var product = user.Products.LastOrDefault();

            if (product?.Step == 5 || user?.Products.Count < 1)
            {
                EntityList.Product newProduct = new EntityList.Product
                {
                    Step = 0,
                    Name = string.Empty,
                    Description = string.Empty,
                    Price = 0,
                    IsPurchased = false,
                    User = user
                };
                user.Products.Add(newProduct);
                await db.SaveChangesAsync();
            }
        }
        await ProductLoop.ProductLoopAsync(botClient, msg);
    }
}