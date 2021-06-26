namespace websocketschat.Bot.Abstracts
{
    abstract class ListResponses
    {
        /// <summary>
        /// Приветсвие
        /// </summary>
        /// <returns></returns>
        public abstract string Greeting();
        
        /// <summary>
        /// Вопрос "как дела"
        /// </summary>
        /// <returns></returns>
        public abstract string QuestionResolve();

        /// <summary>
        /// Время
        /// </summary>
        /// <returns></returns>
        public abstract string Time();

        /// <summary>
        /// Прощание
        /// </summary>
        /// <returns></returns>
        public abstract string Farewell();

        /// <summary>
        /// Посоветуйте фильм
        /// </summary>
        /// <returns></returns>
        public abstract string RecommendMovie();

        /// <summary>
        /// Посоветуйте трек
        /// </summary>
        /// <returns></returns>
        public abstract string RecommendTrack();
    }
}
