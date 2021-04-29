using System;
using ActorModule;

namespace ActorHandlerModuleJob
{
    class WaitingActivityJob : IActivity
    {
        public int Priority { get; set; } = 1;
        public TimeInterval JobTime { get; }
        public WaitingActivityJob(TimeInterval jobtime)
        {
            JobTime = jobtime;
        }
        public bool Update(Actor actor, double deltaTime)
        {
            string NowTime = DateTime.Now.ToString("HH:mm:ss");

            Console.WriteLine("NOW:  " + NowTime);
            Console.WriteLine("NEED:  " + JobTime.End.ToString());

            if (NowTime == JobTime.End.ToString())
            {
                return true;
            }
            return false;
        }
    }
}
