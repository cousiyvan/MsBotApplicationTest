using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bot_ApplicationTest
{
    [Serializable]
    public class RootDialog: IDialog<string>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        //public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    var message = await result;
        //    await context.PostAsync($"Vous avez dit {message.Text}");
        //    context.Wait(MessageReceivedAsync);
        //}

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            PromptDialog.Choice(context, ResumeAfter, new List<string> { "Option 1", "Option 2" }, "Hello! Choisissez une option!");
        }

        private async Task ResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            await context.PostAsync($"Vous avez choisi l'option {message}");

            if (message == "Option 1")
            {
                context.Call(new RootDialog(), ResumeAfter);
            }
            else if (message == "Option 2")
            {
                context.Call(new RootDialog(), ResumeAfter);
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}