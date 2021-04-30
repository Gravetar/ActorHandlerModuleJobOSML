using System;
using OSMLSGlobalLibrary.Modules;

namespace ActorHandlerModuleJob
{
    public class ActorHandlerModuleJob : OSMLSModule
    {

        /// <summary>
        /// Инициализация модуля. В отладочной конфигурации выводит сообщение
        /// </summary>
        protected override void Initialize()
        {
            Console.WriteLine("ActorHandlerModuleJob: Initialize");
        }
        /// <summary>
        /// Вызывает Update на всех акторах
        /// </summary>
        public override void Update(long elapsedMilliseconds)
        {
        }
    }
}