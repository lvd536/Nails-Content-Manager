using Microsoft.Extensions.WebEncoders.Testing;
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
    if (msg.Text is null || !msg.Text.StartsWith('/'))  await PostCreator.PostLoop(bot, msg);
    if (msg.Text is null) return;
    var commandParts = msg.Text.Split(' ');
    var command = commandParts[0];
    var argument = commandParts.Length >= 2 ? commandParts[1] : null;
    var defArgument = commandParts.Length >= 3 ? commandParts[2] : null;
    
    if (msg.Text.StartsWith('/'))
    {
        switch (command)
        {
            case "/post":
                await PostCreator.PostCmd(bot, msg);
                break;
            case "/cancel":
                await PostCreator.PostCancel(bot, msg);
                break;
            case "/set":
                await PostCreator.SetChannel(bot, msg);
                break;
            case "/create":
                if (argument is not null)
                {
                    if (defArgument is null) defArgument = "00:00";
                    argument += $".{DateTime.Now.Year} {defArgument}";
                    await OpenDates.CreateOpenDatesAsync(bot, msg, argument);
                }
                break;
            case "/send":
                await OpenDates.SendOpenDatesAsync(bot, msg);
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