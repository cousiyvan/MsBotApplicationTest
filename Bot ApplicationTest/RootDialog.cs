using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Bot_ApplicationTest
{
    [Serializable]
    public class RootDialog : IDialog<string>
    {
        CognitiveServiceCall sentimentServiceCall;
        CognitiveServiceCall keyPhrasesServiceCall;

        public async Task StartAsync(IDialogContext context)
        {
            this.sentimentServiceCall = new CognitiveServiceCall(CognitiveServiceCall.ApiSelection.Sentiment);
            this.keyPhrasesServiceCall = new CognitiveServiceCall(CognitiveServiceCall.ApiSelection.KeyPhrase);

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
            // PromptDialog.Choice(context, ResumeAfter, new List<string> { "Option 1", "Option 2" }, "Hello! Comment vas-tu ajourd'hui!");
            PromptDialog.Text(context, IGotAnAnswer, "Hello! How are you doing today!", $"Please say something ...");
        }

        private async Task IGotAnAnswer(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            // var answer = this.RequestData(message);
            HttpClient httpClient = new HttpClient();
            this.sentimentServiceCall.headerContents.ToList().ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Substring(0, h.IndexOf(':')), h.Substring(h.IndexOf(':') + 1)));

            SentimentInput input = new SentimentInput
            {
                documents = new List<DocumentInput>
                {
                    new DocumentInput
                    {
                        id = 1,
                        text = message
                    }
                }
            };

            var jsonInput = JsonConvert.SerializeObject(input);
            var responseSentiment = await httpClient.PostAsync(this.sentimentServiceCall.url, new StringContent(jsonInput, Encoding.UTF8, "application/json"));

            var rawResponseSentiment = await responseSentiment.Content.ReadAsStringAsync();
            var jsonResponseSentiment = JsonConvert.DeserializeObject<BatchResult>(rawResponseSentiment);

            var responseKeyPhrases = await httpClient.PostAsync(this.keyPhrasesServiceCall.url, new StringContent(jsonInput, Encoding.UTF8, "application/json"));

            var rawResponseKeyPhrases = await responseKeyPhrases.Content.ReadAsStringAsync();
            var jsonResponseKeyPhrases = JsonConvert.DeserializeObject<BatchResult>(rawResponseKeyPhrases);

            string myAnswer = WhatIsYourMood(jsonResponseSentiment);

            //await context.PostAsync($"It's good to hear that you're feeling {message}");
            await context.PostAsync($"{myAnswer}");
            context.Wait(MessageReceivedAsync);
        }

        private string WhatIsYourMood(BatchResult batchResult)
        {
            string answer = string.Empty;
            double average = 0;
            foreach(var res in batchResult.documents)
            {
                average += res.score;
            }

            average = average / batchResult.documents.Count;

            if (average > 0.5)
            {
                answer = "Cool that you're feeling quit good :D Let's keep on like this!";
            }
            else if(average == 0.5)
            {
                answer = "Mmmmmmmhhhhhhhhh ... cosi cosi .....";
            }
            else if (average < 0.5)
            {
                answer = ".... not cool .. what is keeping you down ... :/";
            }

            return answer;
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

        private async Task<BatchResult> RequestData(string text)
        {
            HttpClient httpClient = new HttpClient();
            this.sentimentServiceCall.headerContents.ToList().ForEach(h => httpClient.DefaultRequestHeaders.Add(h.Substring(0, h.IndexOf(':')), h.Substring(h.IndexOf(':') + 1)));

            SentimentInput input = new SentimentInput
            {
                documents = new List<DocumentInput>
                {
                    new DocumentInput
                    {
                        id = 1,
                        text = text
                    }
                }
            };

            var jsonInput = JsonConvert.SerializeObject(input);
            var response = await httpClient.PostAsJsonAsync(this.sentimentServiceCall.url, jsonInput);

            var rawResponse = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<BatchResult>(rawResponse);

            return jsonResponse;
        }
    }
}