using System;
using ActorModule;

namespace ActorHandlerModuleJob
{
    class WaitingActivityJob : IActivity
    {
        // Приоритет делаем авто-свойством, со значением по умолчанию
        public int Priority { get; set; } = 1;
        //Интервал времени на работу
        public TimeInterval JobTime { get; }
        public WaitingActivityJob(TimeInterval jobtime)
        {
            JobTime = jobtime;
        }
        //Update-работает постоянно, пока не return true
        public bool Update(Actor actor, double deltaTime)
        {
            //Текущее время, переведенное в строку
            string NowTime = DateTime.Now.ToString("HH:mm:ss");

            //Console.WriteLine("NOW:  " + NowTime);
            //Console.WriteLine("NEED:  " + JobTime.End.ToString());

            //Если текущее время равно времени окончания работы
            if (NowTime == JobTime.End.ToString())
            {
                return true;//Выходим из активити
            }
            return false;
        }
    }
}
