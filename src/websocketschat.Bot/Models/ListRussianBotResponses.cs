using System;
using websocketschat.Bot.Abstracts;

namespace websocketschat.Bot
{
    class ListRussianBotResponses : ListResponses
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
                "Привет!!!",
                "Приветствую, рад Вам.",
                "Здравствуйте!",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string QuestionResolve()
        {
            string[] response = new string[] {
                "Хорошо!",
                "Хорошо, а у вас?",
                "Хорошо",
                "Замечательно!",
                "Замечательно, а у вас?",
                "Замечательно",
                "Отлично!",
                "Отлично, а у вас?",
                "Отлично"
            };

            return response[RandomIndex(response.Length)];
        }

        public override string Time()
        {
            string[] response = new string[] {
                $"Сейчас {DateTime.Now.ToShortTimeString()} времени",
                $"Хозяин, сейчас {DateTime.Now.ToShortTimeString()} времени",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string Farewell()
        {
            string[] response = new string[] {
                "Пока",
                "До свидания!!!",
                "До скорой встречи!",
            };

            return response[RandomIndex(response.Length)];
        }

        public override string RecommendMovie()
        {
            string[] response = new string[] {
                "Зеленая миля",
                "Леон",
                "1+1",
                "Криминальное чтиво",
                "Крестный отец",
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
