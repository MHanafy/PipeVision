using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PipeVision.Application;
using PipeVision.Domain;

namespace PipeVision.Web
{
    public static class CacheHelper
    {
        public const string FailingTestsKey = "FailingTests";
        public static void Configure(ITimerService timerService)
        {
            timerService.Schedule(x =>
                {
                    var serviceProvider = (IServiceProvider) x;
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                        var pipelineService = scope.ServiceProvider.GetRequiredService<IPipelineService>();
                        var result = pipelineService.GetFailingTests(DateTime.Today.AddDays(-30)).Result;
                        cache.Set(FailingTestsKey, result);
                    }
                }, 1000 * 60 * 5);
        }
    }
}
