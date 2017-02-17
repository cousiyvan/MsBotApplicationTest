using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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

            TextInput input = new TextInput
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

            if (IsQuestion(message))
            {
                await context.PostAsync($"Please don't bother about me :P I'm always ok ^^");
            }

            double moodLevel;
            string myMood = this.WhatIsYourMood(jsonResponseSentiment, out moodLevel);

            //await context.PostAsync($"It's good to hear that you're feeling {message}");
            await context.PostAsync($"{myMood}");

            string myTopics = WhatAreYourTopics(jsonResponseKeyPhrases, moodLevel);

            string youtubeLink = this.LinkYoutube(new List<string> { "Jeux", "people" });
            await context.PostAsync($"Your daily youtube link {youtubeLink}");

            // We go on with the text answered
            PromptDialog.Text(context, IGotAnAnswer, $"{myTopics}", $"Please say something ...");
            // context.Wait(MessageReceivedAsync);
        }

        private string WhatIsYourMood(BatchResult sentiment, out double moodLevel)
        {
            string answer = string.Empty;
            string keys = string.Empty;
            double average = 0;

            foreach (var res in sentiment.documents)
            {
                average += res.score;
            }

            average = average / sentiment.documents.Count;

            moodLevel = average;

            if (average > 0.5)
            {
                answer = "Cool that you're feeling quit good :D Let's keep on like this!";
            }
            else if (average == 0.5)
            {
                answer = "Mmmmmmmhhhhhhhhh ... cosi cosi .....";
            }
            else if (average < 0.5)
            {
                answer = ".... not cool .. what is keeping you down ... :/";
            }

            return answer;
        }

        private string WhatAreYourTopics(BatchResult keyPhrases, double moodLevel)
        {
            string answer = answer = "so ... ";
            bool haveTopic = false;
            string keys = string.Empty;
            List<string> topics = new List<string>();

            foreach (var res in keyPhrases.documents)
            {
                foreach (var key in res.keyPhrases)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        haveTopic = true;
                        topics.Add(key);
                    }
                }
            }

            answer += $" ... ";

            if (haveTopic)
            {
                answer += $"I feel like ";
                topics.ForEach(x => answer += $"{x} ");
                if (topics.Count > 1)
                    answer += $"are topics";
                else
                    answer += $"is a topic";
                if (moodLevel > 0.5)
                    answer += $" that make you happy :)";
                else
                    answer += $" that takes you down ...)";
            }
            else
            {
                answer = string.Empty;
                if (moodLevel > 0.5)
                    answer += $":)";
                else
                    answer += $":(";
            }
            return answer;
        }

        private bool IsQuestion(string message)
        {
            //List of common question words
            List<string> questionWords = new List<string>() { "who", "what", "why", "how", "when" };

            //Question word present in the message
            Regex questionPattern = new Regex(@"\b(" + string.Join("|", questionWords.Select(Regex.Escape).ToArray()) + @"\b)", RegexOptions.IgnoreCase);

            //Return true if a question word present, or the message ends with "?"
            if (questionPattern.IsMatch(message) || message.EndsWith("?"))
                return true;
            else
                return false;
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

        private string LinkYoutube(List<string> searchTerms)
        {
            string youtubeLink = ConfigurationManager.AppSettings["YoutubeLink"];

            var youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApplicationName = "Bot Application",
                ApiKey = ConfigurationManager.AppSettings["GoogleApiKey"]
            });

            var searchList = youTubeService.Search.List("snippet");
            searchTerms.ForEach(x => searchList.Q += $"{x} ");
            searchList.MaxResults = 5;
            searchList.PublishedAfter = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0 ,0);
            searchList.Type = "video";

            // We call the search
            var searchListResponse = searchList.Execute();

            List<string> videosId = new List<string>();

            foreach (var searchResult in searchListResponse.Items)
            {
                videosId.Add(searchResult.Id.VideoId);
            }

            // we take on of the videos and return it with the full link
            return string.Format($"{youtubeLink}", videosId[new Random().Next(videosId.Count - 1)]);
        }
    }
}