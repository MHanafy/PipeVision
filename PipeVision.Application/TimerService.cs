using System;
using System.Collections.Generic;
using System.Threading;

namespace PipeVision.Application
{
    public class TimerService : ITimerService, IDisposable
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly List<Timer> _timers;

        public TimerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _timers = new List<Timer>();
        }

        public void Schedule(TimerCallback func, int interval)
        {
            var timer = new Timer(func, _serviceProvider, 0, interval);
           _timers.Add(timer);
        }

        public void Dispose()
        {
            foreach (var timer in _timers)
            {
                timer.Dispose();
            }
        }
    }
}
