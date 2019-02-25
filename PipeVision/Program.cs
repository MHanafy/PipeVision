using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GoPipeline;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;
using PipeVision.Application;
using PipeVision.Data;
using PipeVision.Domain;
using PipeVision.LogParsers.Test;
using PipeVisionConsole.Config;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PipeVisionConsole
{
    class Program
    {
        private static ILogger<Program> _logger;
        private static PvcConfig _config;

        public static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("use -? to list available options");
                return 0;
            }
           return await CommandLineApplication.ExecuteAsync<Program>(args);
        } 

        [Option(CommandOptionType.MultipleValue,
            Description = "Pipeline name, supports multiple values by including the option multiple times",
            ShortName = "p")]
        public List<string> Pipeline { get; }

        [Option(CommandOptionType.SingleValue, Description = "Number of pipeline runs to refresh, defaults to 10",
            ShortName = "n")]
        [Range(1, 100)]
        public int Count { get; }

        [Option(CommandOptionType.MultipleValue,
            Description = "Name of the pipeline group defined in Pipelines.json, supports multiple values",
            ShortName = "g")]
        public List<string> Group { get; }

        [Option(CommandOptionType.NoValue, Description = "Refreshes all pipelines in Pipelines.json")]
        public bool All { get; }

        // ReSharper disable once UnusedMember.Local
        private async Task OnExecute()
        {
            List<string> pipelineNames = null;
            if (All)
            {
                if (Group != null || Pipeline != null)
                {
                    _logger.LogWarning(" option -a can't be used in combination with -g or -p");
                    return;
                }

                pipelineNames = GetGroupPipelines();
            }
            else if (Group != null)
            {
                if (Pipeline != null)
                {
                    {
                        _logger.LogWarning(" option -g can't be used in combination with -p");
                        return;
                    }
                }

                pipelineNames = GetGroupPipelines(Group);
            }
            else if (Pipeline != null) pipelineNames = Pipeline;
            else
            {
                _logger.LogWarning("Please use one of the options -a, -g, or -p. use -? for all available options");
            }

            if (pipelineNames == null || pipelineNames.Count == 0)
            {
                _logger.LogWarning("No pipelines found, aborting.");
                return;
            }

            if (!ReadConfig()) return;
            var serviceProvider = BuildDi();
            _logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var service = serviceProvider.GetRequiredService<IPipelineUpdateService>();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var count = Count == 0 ? 10 : Count;
            foreach (var pipelineName in pipelineNames)
            {
                await service.UpdatePipelines(pipelineName, count);
            }

            _logger.LogInformation("Pipeline refresh completed.");
        }

        private List<string> GetGroupPipelines(List<string> groupNames = null)
        {
            const string fileName = "pipelines.json";
            try
            {
                if (!File.Exists(fileName))
                {
                    _logger.LogWarning(
                        $"Can't access {fileName}!, Please either update the file or use -p option to manually specify pipelines");
                    return null;
                }

                var config = File.ReadAllText(fileName);
                var groups = JsonConvert.DeserializeObject<PipelineConfig>(config).Groups;

                if (groupNames == null)
                {
                    var allGroups = groups.SelectMany(x => x.Pipelines).ToList();
                    if (allGroups.Count > 0) return allGroups;
                    _logger.LogWarning($"No groups defined in {fileName}!");
                    return null;
                }

                var result = groups.Where(x => groupNames.Contains(x.Name)).ToList();
                if (result.Count == groupNames.Count) return result.SelectMany(x => x.Pipelines).ToList();

                var undefinedGroups = groupNames.Where(x => groups.All(y => y.Name != x));
                _logger.LogWarning(
                    $"The following groups aren't defined, Please review {fileName}\r\n{string.Join(",", undefinedGroups)}");
                return null;
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Can't read {fileName}:\r\n" + e);
                return null;
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.ExceptionObject as Exception, "Unexpected Exception");
        }

        private static bool ReadConfig()
        {
            const string fileName = "pvc.json";
            try
            {
                if (!File.Exists(fileName))
                {
                    Console.WriteLine(
                        $"Can't access {fileName}!, Please update the file and try again");
                    return false;
                }

                _config = JsonConvert.DeserializeObject<PvcConfig>(File.ReadAllText(fileName));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't read {fileName}:\r\n" + e);
                return false;
            }
        }

        private static ServiceProvider BuildDi()
        {
            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true
                    });
                })
                .AddDbContext<PipelineContext>(options =>
                {
                    options.UseSqlServer(_config.DbConnection, cfg=>cfg.CommandTimeout(_config.DbCommandTimeout));
                    options.EnableSensitiveDataLogging();
                })
                .AddScoped<IPipelineRepository, PipelineRepository>()
                .AddScoped<IPipelineUpdateService, PipelineUpdateService>()
                .AddTransient<IWhiteStackLogParser, WhiteStackLogParser>()
                .AddTransient<IMsTestLogParser, MsTestLogParser>();

            var provider = services.BuildServiceProvider();
            var parsers = new List<ITestLogParser>
                {provider.GetService<IWhiteStackLogParser>(), provider.GetService<IMsTestLogParser>()};

            services.AddSingleton<IList<ITestLogParser>>(parsers);
            services.AddSingleton<IPipelineProvider>(new GoPipelineProvider(_config.GoServerBaseAddress,
                _config.GoServerUser, _config.GoServerPass, provider.GetService<ILogger<GoPipelineProvider>>()));
            return services.BuildServiceProvider();
        }
    }
}
