using System.Text.RegularExpressions;
using websocketschat.Bot.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using websocketschat.Bot.Parsing.Response;
using System;
using Microsoft.Extensions.Logging;

namespace websocketschat.Bot
{
    public class BotManager : IBotManager
    {
        private string _name;
        private string _password;
        private byte[] _byteArrayBot;
        private HubConnection _hub;
        private readonly ILogger<BotManager> _logger;

        public BotManager(ILogger<BotManager> logger)
        {
            _name = "Bot";
            _password = "bot123";
            _byteArrayBot = System.Text.Encoding.UTF8.GetBytes($"username={_name}&password={_password}");
            _logger = logger;
        }

        /// <summary>
        /// регистрация бота
        /// </summary>
        /// <param name="urlRegister"></param>
        /// <returns>статус регистрации</returns>
        public async Task<int> RegisterBotAsync(string urlRegister)
        {

            WebRequest request = WebRequest.Create(urlRegister);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = _byteArrayBot.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(_byteArrayBot, 0, _byteArrayBot.Length);
            }


            try
            {
                WebResponse response = await request.GetResponseAsync();
                _logger.LogInformation($"The bot has registered.");
                return 201;
            }
            catch (WebException e)
            {
                var webResponse = (HttpWebResponse)e.Response;
                _logger.LogInformation($"The bot didn't register");
                return (int)webResponse.StatusCode;
            }
        }

        /// <summary>
        /// Авторизация бота 
        /// </summary>
        /// <param name="urlToken"></param>
        /// <param name="postQueryAfterGetToken"></param>
        /// <param name="webToken"></param>
        /// <returns></returns>
        public async Task AuthBotAsync(string urlToken, string postQueryAfterGetToken, string webToken)
        {
            try
            {
                Root responseObject = await AuthBotUrlTokenAsync(urlToken);
                _logger.LogInformation($"Received a token and is preparing to send a post request.");
                Parsing.ResponseOtDeda.Root root = await AuthBotPostQueryAfterGetTokenAsync(postQueryAfterGetToken, responseObject);
                _logger.LogInformation($"Received a response after the post request.");
                _hub = new HubConnectionBuilder()
                    .WithUrl(webToken, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(responseObject.access_token);
                    })
                    .WithAutomaticReconnect()
                    .Build();
                await _hub.StartAsync();
                _logger.LogInformation($"The bot was authorized.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"message: {e.Message}");
            }
        }

        /// <summary>
        /// Обработчик сообщения от пользователя
        /// </summary>
        /// <returns></returns>
        public async Task OnAsync()
        {
            _hub.On<object>("Receive", async (data) => {
                data = (JsonElement)data;
                Parsing.ResponseMessage.Root responseObject = JsonSerializer.Deserialize<Parsing.ResponseMessage.Root>(data.ToString());
                if (responseObject.message.StartsWith("Bot"))
                {
                    string responseMessage;
                    responseObject.message = responseObject.message.Remove(0, 4);
                    responseMessage = Process(responseObject.message);
                    await Send(responseMessage);
                }
            });
        }
        /// <summary>
        /// Отключение 
        /// </summary>
        public async Task DisconnectAsync()
        {
            await _hub.StopAsync();
        }

        /// <summary>
        /// Обработчик сообщения на русском 
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Отевет бота</returns>
        private string ProcessRussian(string command)
        {
            ListRussianBotResponses listRussianResponses = new ListRussianBotResponses();
            command = command.ToLower();

            if (command.IndexOf("привет") != -1 || command.IndexOf("здравств") != -1)
            {
                return listRussianResponses.Greeting();
            }
            else if (command.IndexOf("как дела") != -1 || command.IndexOf("как ты") != -1)
            {
                return listRussianResponses.QuestionResolve();
            }
            else if (command.IndexOf("время") != -1)
            {
                return listRussianResponses.Time();
            }
            else if (command.IndexOf("пока") != -1 || command.IndexOf("до свидания") != -1)
            {
                return listRussianResponses.Farewell();
            }
            else if (command.IndexOf("посоветуй трек") != -1)
            {
                return listRussianResponses.RecommendTrack();
            }
            else if (command.IndexOf("посоветуй фильм") != -1)
            {
                return listRussianResponses.RecommendMovie();
            }


            return "Я вас не понимаю";
        }

        /// <summary>
        /// Обработчик сообщения на английском
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Отевет бота</returns>
        private string ProcessEnglish(string command)
        {
            ListEnglishBotResponses listEnglishResponses = new ListEnglishBotResponses();
            command = command.ToLower();

            if (command.IndexOf("hi") != -1 || command.IndexOf("hello") != -1)
            {
                return listEnglishResponses.Greeting();
            }
            else if (command.IndexOf("how are you") != -1)
            {
                return listEnglishResponses.QuestionResolve();
            }
            else if (command.IndexOf("time") != -1)
            {
                return listEnglishResponses.Time();
            }
            else if (command.IndexOf("bye") != -1 || command.IndexOf("goodbye") != -1)
            {
                return listEnglishResponses.Farewell();
            }
            else if (command.IndexOf("recommend a track") != -1)
            {
                return listEnglishResponses.RecommendTrack();
            }
            else if (command.IndexOf("recommend a movie") != -1)
            {
                return listEnglishResponses.RecommendMovie();
            }


            return "I don't understand you";
        }

        /// <summary>
        /// Обработчик
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Отевет бота</returns>
        private string Process(string command)
        {
            return Regex.IsMatch(command, "[а-яА-ЯеЁ]") ? ProcessRussian(command) : ProcessEnglish(command);
        }

        /// <summary>
        /// Первый пост запрос на сервер для получения токена
        /// </summary>
        /// <param name="urlToken"></param>
        /// <returns></returns>
        private async Task<Root> AuthBotUrlTokenAsync(string urlToken)
        {
            WebRequest request = WebRequest.Create(urlToken);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = _byteArrayBot.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(_byteArrayBot, 0, _byteArrayBot.Length);
            }

            WebResponse response = await request.GetResponseAsync();

            Root responseObject;

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return responseObject = JsonSerializer.Deserialize<Root>(reader.ReadToEnd());
                }
            }

        }

        /// <summary>
        /// Второй пост запрос на сервер для получения токена
        /// </summary>
        /// <param name="postQueryAfterGetToken"></param>
        /// <param name="responseObject"></param>
        /// <returns></returns>
        private async Task<Parsing.ResponseOtDeda.Root> AuthBotPostQueryAfterGetTokenAsync(string postQueryAfterGetToken, Root responseObject)
        {
            WebRequest request = WebRequest.Create(postQueryAfterGetToken);
            request.Method = "POST";
            request.ContentType = "text/plain;charset=UTF-8";
            request.Headers.Add("Authorization", "Bearer " + responseObject.access_token);
            request.ContentLength = _byteArrayBot.Length;

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(_byteArrayBot, 0, _byteArrayBot.Length);
            }

            WebResponse response = await request.GetResponseAsync();
            
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonSerializer.Deserialize<Parsing.ResponseOtDeda.Root>(reader.ReadToEnd());
                }
            }

        }
        
        /// <summary>
        /// отправка сообщения 
        /// </summary>
        /// <param name="messadg"></param>
        /// <returns></returns>
        private async Task Send(string messadg)
        {
            await _hub.InvokeAsync("Send", _name, messadg);
        }
    }
}
