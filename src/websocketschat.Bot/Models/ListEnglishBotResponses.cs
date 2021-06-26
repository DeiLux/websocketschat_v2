using System;
using websocketschat.Bot.Abstracts;

namespace websocketschat.Bot
{
    class ListEnglishBotResponses : ListResponses
    {
        /// <summary>
        /// Рандомный индекс
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        protected int RandomIndex(int maxInterval)
        {
            Random random = new Random();

            return random.Next(0, maxInterval);

        }

        public override string Greeting()
        {
            string[] response = new string[] {
                "Hello!!!",
                "Hello, glad to see You.",
                "Hi!",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string QuestionResolve()
        {
            string[] response = new string[] {
                "Well!",
                "Well, do you?",
                "Well",
                "Wonderful!",
                "Fine, and you?",
                "Good",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string Time()
        {
            string[] response = new string[] {
                $"Now {DateTime.Now.ToShortTimeString()} time",
                $"Mister, now {DateTime.Now.ToShortTimeString()} time",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string Farewell()
        {
            string[] response = new string[] {
                "Bye",
                "Goodbye!!!",
                "See you soon!",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string RecommendMovie()
        {
            string[] response = new string[] {
                "The Green Mile",
                "Leon",
                "1+1",
                "Pulp Fiction",
                "The GodFather",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string RecommendTrack()
        {
            string[] response = new string[] {
                "Pop Smoke – Dior",
                "Young Thug – Safe",
                "Future – Never Stop",
                "Roddy Ricch – The Box",
                "Travis Scott - 3500",
            };

            return response[RandomIndex(response.Length)];
        }
    }
}
