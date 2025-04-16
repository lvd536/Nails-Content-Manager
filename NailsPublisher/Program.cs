using Microsoft.Extensions.WebEncoders.Testing;
using NailsPublisher.AdminTools;
using NailsPublisher.Database;
using NailsPublisher.OpenDatesTools;
using NailsPublisher.PostTools;
using NailsPublisher.Tools;
using NailsPublisher.Tools.PersonalTools;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7943623939:AAEUAhQHU8rdxQEsEk9HeEfkXwyR_X5PgLI", cancellationToken: cts.Token);
var me = await bot.GetMe();

bot.OnMessage += OnMessage;
bot.OnError += OnError;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();
async Task OnMessage(Message msg, UpdateType type)
{
    if (!CheckAdmin.IsAdminCheckAsync(bot, msg).Result && msg.Text != "/me")
    {
        await bot.SendMessage(msg.Chat.Id, "<b>Использовать бота может ТОЛЬКО ограниченный круг лиц!</b>", ParseMode.Html);
        return;
    }
    if (msg.Text is null)
    {
        if (msg.Type == MessageType.Photo || msg.Type == MessageType.Video)
        {
            await PostLoop.PostLoopAsync(bot, msg);
        }
        return;
    }
    if (msg.Chat.Type is ChatType.Channel && !msg.Text.StartsWith("/pset")) return;
    if (!msg.Text.StartsWith('/'))
    {
        await PostLoop.PostLoopAsync(bot, msg);
        await ProductLoop.ProductLoopAsync(bot, msg);
    }
    var commandParts = msg.Text.Split(' ');
    var command = commandParts[0];
    var firstArgument = commandParts.Length >= 2 ? commandParts[1] : null;
    var secondArgument = commandParts.Length >= 3 ? commandParts[2] : null;
    var thirdArgument = commandParts.Length >= 4 ? commandParts[3] : null;
    
    if (msg.Text.StartsWith('/'))
    {
        switch (command)
        {
            case "/product":
                await ProductStart.ProductCmdAsync(bot, msg);
                break;
            case "/shopper":
                await ProductList.ShopperListCmdAsync(bot, msg);
                break;
            case "/post":
                await PostStart.PostCmdAsync(bot, msg);
                break;
            case "/pcancel":
                await PostCancel.PostCancelAsync(bot, msg);
                break;
            case "/pset":
                await PostSetChannel.SetChannelAsync(bot, msg);
                break;
            case "/plist":
                await PostList.PostListCmdAsync(bot, msg);
                break;
            case "/pdelete":
                if (short.TryParse(firstArgument, out short shortValue))
                {
                    await PostDelete.PostDeleteCmdAsync(bot, msg, shortValue);
                }
                else await bot.SendMessage(msg.Chat.Id, "Вы неверно указали номер поста. Пример: /pdelete 5 | /pdelete №поста", ParseMode.Html);
                break;
            case "/ccreate":
                if (firstArgument is not null)
                {
                    try {
                        if (secondArgument is null) secondArgument = "00:00";
                        if (thirdArgument != "+" && thirdArgument != "-")
                        {
                            await bot.SendMessage(msg.Chat.Id, "Неправильно указана комманда. Пример: /ccreate 01.01 22:40 + | /ccreate дата время свободно/занято", ParseMode.Html);
                            break;
                        }
                        firstArgument += $".{DateTime.Now.Year} {secondArgument}";
                        await CreateOpenDates.CreateOpenDatesAsync(bot, msg, firstArgument, thirdArgument);
                    } catch (Exception)
                    {
                        await bot.SendMessage(msg.Chat.Id, "Неправильно указана комманда. Пример: /ccreate 01.01 22:40 + | /ccreate дата время свободно/занято", ParseMode.Html);
                    }
                }
                break;
            case "/csend":
                await SendOpenDates.SendOpenDatesAsync(bot, msg, true);
                break;
            case "/cdelete":
                if (firstArgument is null) await OpenDateDelete.OpenDateDeleteCmdAsync(bot, msg, -1);
                else
                {
                    try {
                        await OpenDateDelete.OpenDateDeleteCmdAsync(bot, msg, int.Parse(firstArgument));
                    } catch (Exception)
                    {
                        await bot.SendMessage(msg.Chat.Id, "Вам необходимо установить номер записи после комманды. Пример: /cdelete 5", ParseMode.Html);
                    }
                }
                break;
            case "/crewrite":
                await RewriteOpenDates.RewriteOpenDatesAsync(bot, msg);
                break;
            case "/clist":
                await OpenDateList.OpenDateListCmdAsync(bot, msg);
                break;
            case "/makeadmin":
                if (firstArgument is null) return;
                try {
                    await AddAdmin.AddAdminAsync(bot, msg, long.Parse(firstArgument));
                } catch (Exception)
                {
                    await bot.SendMessage(msg.Chat.Id, "Вам необходимо установить UID юзера после комманды. Пример: /makeadmin 123465789", ParseMode.Html);
                }
                break;
            case "/me":
                await bot.SendMessage(msg.Chat.Id, $"Ваш TG ID: <code>{msg.From.Id}</code>", ParseMode.Html);
                break;
            case "/help":
                await HelpCommand.HelpCommandAsync(bot, msg);
                break;
        }
    }
}

async Task OnError(Exception exception, HandleErrorSource handler)
{
    Console.WriteLine(exception.Message);
    await Task.Delay(2000, cts.Token);
}