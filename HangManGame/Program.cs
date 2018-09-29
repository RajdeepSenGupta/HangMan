using Newtonsoft.Json.Linq;
using OxfordDictionary;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HangManGame
{
    class Program
    {
        static void Main(string[] args)
        {
            WordAC[] letters = GetWord();
            Console.WriteLine(String.Format("It's a {0} letters word. The word is related to {1}. Guess the word...", letters.Length, letters.Select(x => x.Domain).First()));
            int guessNumber = 0;

            while (!letters.All(x => x.IsVisible))
            {
                Console.WriteLine();
                PrintLetters(letters);
                Console.WriteLine();
                Console.WriteLine();

                Console.Write("Enter character (Can contain '-'): ");
                char guess = Convert.ToChar(Console.ReadKey(true).KeyChar);
                guessNumber++;
                Console.Write(guess);
                foreach (WordAC letter in letters.Where(x => !x.IsVisible && x.Letter == guess))
                {
                    letter.IsVisible = true;
                }

                if (letters.All(x => x.IsVisible))
                {
                    Console.WriteLine();
                    Console.WriteLine(String.Format("Congratulations! You have found the word: {0}!!", new String(letters.Select(x => x.Letter).ToArray())));
                    Console.WriteLine(String.Format("You took {0} tries", guessNumber));
                }
            }

            Console.ReadLine();
        }

        static WordAC[] GetWord()
        {
            // Documentation = https://developer.oxforddictionaries.com/documentation#!/Wordlist/get_wordlist_source_lang_filters_basic

            Console.Write("Finding Word...");
            Random random = new Random();

            #region Set up HTTP Client

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("app_id", StringConstants.appId);
            client.DefaultRequestHeaders.Add("app_key", StringConstants.appKey);

            #endregion

            #region Get domains

            string api = String.Concat(StringConstants.dictionaryApi, "/", StringConstants.domains, "/", StringConstants.englishLanguage);
            HttpResponseMessage response = client.GetAsync(api).Result;

            JToken json = JObject.Parse(response.Content.ReadAsStringAsync().Result)["results"];
            int randomCount = random.Next(0, json.Values().ToList().Count);
            string domain = json.Values().ToArray()[randomCount]["en"].ToString();

            #endregion

            #region Get word

            api = String.Concat(StringConstants.dictionaryApi, StringConstants.wordlist, StringConstants.englishLanguage, "/", StringConstants.domains, "=", domain);
            response = client.GetAsync(api).Result;
            json = JObject.Parse(response.Content.ReadAsStringAsync().Result)["results"];
            randomCount = random.Next(0, json.ToArray().Length);
            var word = json.ToArray()[randomCount]["word"].ToString();

            #endregion

            Console.Clear();

            WordAC[] wordAc = new WordAC[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                wordAc[i] = new WordAC
                {
                    Letter = word.ToCharArray()[i],
                    IsVisible = word.ToCharArray()[i] == ' ',
                    Domain = domain
                };
            }
            return wordAc;
        }

        static void PrintLetters(WordAC[] letters)
        {
            foreach (WordAC letter in letters)
            {
                if (letter.IsVisible) Console.Write(letter.Letter);
                else Console.Write(" _");
            }
            Console.WriteLine();
        }
    }
}
