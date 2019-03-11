using System.Threading;

namespace PipeVision.Application
{
    public interface ITimerService
    {
        void Schedule(TimerCallback func, int interval);
    }
}
