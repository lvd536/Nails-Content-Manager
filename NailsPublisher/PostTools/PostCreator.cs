using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailsPublisher.PostTools;

public class PostCreator
{
    async Task PostLoop(Message msg)
    {
        /*if (postUserSteps.ContainsKey(msg.From.Id) && postUserSteps[msg.From.Id] != "Finally")
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
        }*/
    }
}