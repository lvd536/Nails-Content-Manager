using Microsoft.Extensions.WebEncoders.Testing;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient("7371147310:AAEwln2CDIWVzYNTFHMdUwbzyzHod1qgDDQ", cancellationToken: cts.Token);
var me = await bot.GetMe();

Dictionary<long, string> postUserSteps = new Dictionary<long, string>(); 
Dictionary<long, string> postUserTitle = new Dictionary<long, string>(); 
Dictionary<long, string> postUserDescription = new Dictionary<long, string>(); 

bot.OnMessage += OnMessage;
bot.OnUpdate += OnCallbackQuery;
bot.OnError += OnError;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();
async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null) return;
    var commandParts = msg.Text.Split(' ');
    var command = commandParts[0];
    var argument = commandParts.Length >= 2 ? commandParts[1] : null;
    var defArgument = commandParts.Length >= 3 ? commandParts[2] : null;
    await PostLoop(msg);
    if (msg.Text.StartsWith('/'))
    {
        switch (command)
        {
            case "/post":
                if (!postUserSteps.ContainsKey(msg.From.Id) || postUserSteps[msg.From.Id] == "Finally")
                {
                    postUserSteps.Add(msg.From.Id, "Start");
                    await bot.SendMessage(msg.Chat.Id, "Вы начали создание поста. Напишите название для поста: ", ParseMode.Markdown);
                    postUserSteps[msg.From.Id] = "Title";
                }
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

async Task PostLoop(Message msg)
{
    if (postUserSteps.ContainsKey(msg.From.Id) && postUserSteps[msg.From.Id] != "Finally")
    {
        if (postUserSteps[msg.From.Id] == "Title")
        {
            postUserTitle[msg.From.Id] = msg.Text;
            await bot.SendMessage(msg.Chat.Id,
                $"Вы установили название поста на {msg.Text}. Напишите описание для поста: ", ParseMode.Markdown);
            postUserSteps[msg.From.Id] = "Description";
        }
        else if (postUserSteps[msg.From.Id] == "Description")
        {
            postUserDescription[msg.From.Id] = msg.Text;
            var post = postUserTitle[msg.From.Id] + $"\n{postUserDescription[msg.From.Id]}";
            await bot.SendMessage(msg.Chat.Id, $"Ваш пост: {post}",
                ParseMode.Markdown);
            postUserSteps[msg.From.Id] = "Finally";
        }
    }
}