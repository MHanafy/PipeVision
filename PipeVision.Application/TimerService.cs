using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace PipeVision.Application
{


    public class TimerService : ITimerService, IDisposable
    {
        public delegate void TimerDelegate(IServiceProvider serviceProvider);

        public class TimerData
        {
            public readonly TimerDelegate Delegate;
            public readonly int Interval;
            public readonly string Desc;
            public bool InProgress { get; set; }

            public TimerData(TimerDelegate tDelegate, int interval, string desc)
            {
                Delegate = tDelegate;
                Interval = interval;
                Desc = desc;
            }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly List<Timer> _timers;
        private readonly ILogger<TimerService> _logger;

        public TimerService(IServiceProvider serviceProvider, ILogger<TimerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timers = new List<Timer>();
        }

        public void Schedule(TimerDelegate func, int interval, string desc)
        {
            var tData = new TimerData(func, interval, desc);
            var timer = new Timer(Process, tData, 0, interval);
            _timers.Add(timer);
        }

        [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
        private void Process(object state)
        {
            var data = (TimerData) state;
            lock (data)
            {
                if (data.InProgress)
                {
                    _logger.LogInformation($"Skipped execution of {data.Desc}");
                    return;
                }
                //Avoid multi invocation
                data.InProgress = true;
            }
            try
            {
                _logger.LogInformation($"Started execution of {data.Desc}");
                data.Delegate.Invoke(_serviceProvider);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Exception processing timer delegate");
            }
            finally
            {
                lock (data)
                {
                    data.InProgress = false;
                }
                _logger.LogInformation($"Finished execution of {data.Desc}");
            }

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
