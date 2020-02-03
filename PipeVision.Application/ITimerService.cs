
namespace PipeVision.Application
{
    public interface ITimerService
    {
        void Schedule(TimerService.TimerDelegate func, int interval, string desc);
    }
}
