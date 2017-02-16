using Microsoft.Bot.Connector;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Bot_ApplicationTest
{
    [Serializable]
    public class CognitiveServiceCall
    {
        public enum ApiSelection
        {
            Sentiment = 0,
            KeyPhrase,
            LanguagesDetection
        }

        #region Constants
        private const string sentimentApiUrl = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
        private const string keyPhrasesApiUrl = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";
        private const string languagesApiUrl = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/languages";

        public readonly string[] headerContents = { string.Format("Ocp-Apim-Subscription-Key:{0}", ConfigurationManager.AppSettings["TextAnalysisApiKey"]), "Accept:application/json" };
        #endregion

        #region Properties
        public List<string> header { get; set; }

        public string textToAnalyze { get; set; }

        // private HttpClient webClient;

        public Uri url { get; set; }

        public ApiSelection apiSelection { get;set;} 
        #endregion

        #region Members

        #endregion

        public CognitiveServiceCall(ApiSelection apiSelection)
        {
            // this.webClient = new HttpClient();
            // this.headerContents.ToList().ForEach(h => this.webClient.DefaultRequestHeaders.Add(h.Substring(0, h.IndexOf(':')), h.Substring(h.IndexOf(':')+1)));
            this.apiSelection = apiSelection;

            switch (this.apiSelection)
            {
                case ApiSelection.Sentiment:
                    this.url = new Uri(sentimentApiUrl);
                    break;
                case ApiSelection.KeyPhrase:
                    this.url = new Uri(keyPhrasesApiUrl);
                    break;
                case ApiSelection.LanguagesDetection:
                    this.url = new Uri(languagesApiUrl);
                    break;
                default:
                    this.url = new Uri(string.Empty);
                    break;
            }
        }

        //public async Task<JsonReader> RequestData(string text)
        //{
        //    string data =  await this.webClient.GetStringAsync(this.url);

        //    // Convert the data in JSON object
        //    JsonTextReader jsonReader = new JsonTextReader(new StringReader(data));

        //    return jsonReader;
        //}
    }
}