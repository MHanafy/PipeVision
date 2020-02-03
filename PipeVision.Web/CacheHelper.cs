using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PipeVision.Application;
using PipeVision.Domain;
using PipeVision.Web.Models;

namespace PipeVision.Web
{
    public static class CacheHelper
    {
        public const string FailingTestsKey = "FailingTests";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static void Configure(ITimerService timerService)
        {
            timerService.Schedule(x =>
                {
                    using (var scope = x.CreateScope())
                    {
                        var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                        var pipelineService = scope.ServiceProvider.GetRequiredService<IPipelineService>();
                        var urlResolver = scope.ServiceProvider.GetService<IPipelineUrlResolver>();
                        var mapper = scope.ServiceProvider.GetService<IMapper>();
                        var config = scope.ServiceProvider.GetService<IConfiguration>();
                        var result = GetFailingTestViewModels(urlResolver, pipelineService, mapper, config);
                        cache.Set(FailingTestsKey, result);
                    }
                }, 1000 * 60 * 5, "GetFailingTestViewModels");
        }

        private static IList<TestViewModel> GetFailingTestViewModels(IPipelineUrlResolver urlResolver,
            IPipelineService pipelineService, IMapper mapper, IConfiguration config)
        {
            try
            {
                var tests = pipelineService.GetFailingTests(DateTime.Today.AddDays(-30)).Result;
                var result = mapper.Map<IEnumerable<TestViewModel>>(tests)
                    .OrderBy(x => x.PipelineName)
                    .ThenByDescending(x => x.StartTime).ToList();
                var serverBaseAddress = new Uri(config.GetValue<string>("PipelineServerBaseAddress"));
                foreach (var test in result)
                {
                    var jobLogUrl = urlResolver.GetPipelineJobLogUrl(test.PipelineName,
                        test.PipelineRun, test.StageName, test.StageRun, test.JobName);
                    var jobUrl = urlResolver.GetPipelineJobUrl(test.PipelineName,
                        test.PipelineRun, test.StageName, test.StageRun, test.JobName);

                    test.JobLogUrl = new Uri(serverBaseAddress, jobLogUrl).AbsoluteUri;
                    test.JobUrl = new Uri(serverBaseAddress, jobUrl).AbsoluteUri;
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"Error refreshing cached failing tests: {e.Message}" );
                return null;
            }
        }
    }
}
