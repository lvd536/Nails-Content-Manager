using Microsoft.Extensions.WebEncoders.Testing;
using NailsPublisher.AdminTools;
using NailsPublisher.Database;
using NailsPublisher.OpenDatesTools;
using NailsPublisher.PostTools;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7371147310:AAEwln2CDIWVzYNTFHMdUwbzyzHod1qgDDQ", cancellationToken: cts.Token);
var me = await bot.GetMe();

bot.OnMessage += OnMessage;
bot.OnUpdate += OnCallbackQuery;
bot.OnError += OnError;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();
async Task OnMessage(Message msg, UpdateType type)
{
    if (!CheckAdmin.IsAdminCheckAsync(bot, msg).Result) return;
    if (msg.Chat.Type is ChatType.Channel && !msg.Text.StartsWith("/pset")) return;
    if (msg.Text is null)
    {
        if (msg.Type == MessageType.Photo) await PostCreator.PostLoopAsync(bot, msg);
        return;
    }
    if (!msg.Text.StartsWith('/')) await PostCreator.PostLoopAsync(bot, msg);
    var commandParts = msg.Text.Split(' ');
    var command = commandParts[0];
    var argument = commandParts.Length >= 2 ? commandParts[1] : null;
    var defArgument = commandParts.Length >= 3 ? commandParts[2] : null;
    
    if (msg.Text.StartsWith('/'))
    {
        switch (command)
        {
            case "/post":
                await PostCreator.PostCmdAsync(bot, msg);
                break;
            case "/pcancel":
                await PostCreator.PostCancelAsync(bot, msg);
                break;
            case "/pset":
                await PostCreator.SetChannelAsync(bot, msg);
                break;
            case "/plist":
                await PostCreator.PostListCmdAsync(bot, msg);
                break;
            case "/ccreate":
                if (argument is not null)
                {
                    try {
                        if (defArgument is null) defArgument = "00:00";
                        argument += $".{DateTime.Now.Year} {defArgument}";
                        await OpenDates.CreateOpenDatesAsync(bot, msg, argument);
                    } catch (Exception)
                    {
                        await bot.SendMessage(msg.Chat.Id, "Неправильно указана комманда. Пример: /ccreate 01.01 22:40 | /ccreate дата время", ParseMode.Html);
                    }
                }
                break;
            case "/csend":
                await OpenDates.SendOpenDatesAsync(bot, msg, true);
                break;
            case "/cdelete":
                if (argument is null) await OpenDates.OpenDateDeleteCmdAsync(bot, msg, -1);
                else
                {
                    try {
                        await OpenDates.OpenDateDeleteCmdAsync(bot, msg, int.Parse(argument));
                    } catch (Exception)
                    {
                        await bot.SendMessage(msg.Chat.Id, "Вам необходимо установить номер записи после комманды. Пример: /cdelete 5", ParseMode.Html);
                    }
                }
                break;
            case "/crewrite":
                await OpenDates.RewriteOpenDatesAsync(bot, msg);
                break;
            case "/clist":
                await OpenDates.OpenDateListCmdAsync(bot, msg);
                break;
            case "/makeadmin":
                if (argument is null) return;
                try {
                    await AddAdmin.AddAdminAsync(bot, msg, long.Parse(argument));
                } catch (Exception)
                {
                    await bot.SendMessage(msg.Chat.Id, "Вам необходимо установить UID юзера после комманды. Пример: /makeadmin 123465789", ParseMode.Html);
                }
                break;
            case "/me":
                await bot.SendMessage(msg.Chat.Id, $"Ваш TG ID: <code>{msg.From.Id}</code>", ParseMode.Html);
                break;
        }
    }
}

async Task OnCallbackQuery(Update update)
{
    if (update.Type != UpdateType.CallbackQuery) return;
    switch (update.CallbackQuery?.Data)
    {
    }
}

async Task OnError(Exception exception, HandleErrorSource handler)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}