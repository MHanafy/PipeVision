using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

namespace PipeVision.LogParsers.Test
{
    public class WhiteStackLogParser : IWhiteStackLogParser
    {
        private readonly ILogger<WhiteStackLogParser> _logger;

        private const string RgxHeader = "TestStack.White.Configuration.UIItemIdAppXmlConfiguration";

        private const string RgxSummary = @"(?m:(?>^(.{3})?(?'time'\d{2}:\d{2}:\d{2}\.\d{3}) Tests run:))";


        private const string RgxTestStart = @"(?m:^(.{3})?(?'time'\d{2}:\d{2}:\d{2}\.\d{3})\s\*{5}\s(?'name'.*)\r?)";

        //private const string RgxTestEnd =
        //    @"(?m:(?>\&\d\|(?'endtime'\d{2}:\d{2}:\d{2}\.\d{3})\s.*-----Finish scenario.*)|(x\d\|(?'canceltime'\d{2}:\d{2}:\d{2}\.\d{3})\s\[go\] On Cancel Task completed))";

        //public const string RgxTest = RgxTestStart + @"(?s:(?'log'.*?))" + RgxTestEnd;
        private const string RgxTestError = @"(?m)^(.{3})?\d{2}:\d{2}:\d{2}\.\d{3}\s-\> (?'error'error: .*)\r?";

        private const string RgxTaskCancel =
            @"(?m)^(.{3})?(?'time'\d{2}:\d{2}:\d{2}\.\d{3})\s\[go\] On Cancel Task completed";

        private const string RgxStacks =
            @"(?s)(.{3})?\d{2}:\d{2}:\d{2}\.\d{3} Errors and Failures:(?'stacks'.*?)\r?\n(?>(.{3})?\d{2}:\d{2}:\d{2}\.\d{3} \r?\n){2}";

        private const string RgxStack =
            @"(?s)(?>(.{3})?\d{2}:\d{2}:\d{2}\.\d{3} \d+\) (?'stack'.*?)(?=(?>(.{3})?\d{2}:\d{2}:\d{2}\.\d{3} \d+\)|$)))";

        private const string RgxRemoveDate = @"(?m)^(.{3})?\d{2}:\d{2}:\d{2}\.\d{3} ";

        public const string TimeFormat = "HH:mm:ss.fff";

        private readonly Regex _rHeader = new Regex(RgxHeader, RegexOptions.Compiled);
        //private readonly Regex _rTest = new Regex(RgxTest, RegexOptions.Compiled);
        private readonly Regex _rTestError = new Regex(RgxTestError, RegexOptions.Compiled);
        private readonly Regex _rStacks = new Regex(RgxStacks, RegexOptions.Compiled);
        private readonly Regex _rStack = new Regex(RgxStack, RegexOptions.Compiled);
        private readonly Regex _rRemoveDate = new Regex(RgxRemoveDate, RegexOptions.Compiled);
        private readonly Regex _rTaskCancel = new Regex(RgxTaskCancel, RegexOptions.Compiled);
        private readonly Regex _rSummary = new Regex(RgxSummary, RegexOptions.Compiled);
        private readonly Regex _rTestStart = new Regex(RgxTestStart, RegexOptions.Compiled);

        public WhiteStackLogParser(ILogger<WhiteStackLogParser> logger)
        {
            _logger = logger;
        }

        public IList<T> Parse<T>(string log) where T : ITest
        {
            try
            {
                if (!_rHeader.IsMatch(log)) return null;
                _logger.LogInformation($"Started parsing WhiteStack log file");
                var result = new List<T>();

                var stacksStr = _rStacks.Match(log).Groups["stacks"].Value;
                var stacksList = new List<string>();
                foreach (Match match in _rStack.Matches(stacksStr))
                {
                    stacksList.Add(_rRemoveDate.Replace(match.Groups["stack"].Value, ""));
                }
                _logger.LogInformation($"Found {stacksList.Count} error call stacks");

                DateTime? endTime = null;
                var lastIndex = 0;
                var errIndex = stacksList.Count - 1;
                var matchSummary = _rSummary.Match(log);
                var cancelled = false;
                if (matchSummary.Success)
                {
                    endTime = DateTime.ParseExact(matchSummary.Groups["time"].Value, TimeFormat, null);
                    lastIndex = matchSummary.Index - 1;
                }
                else
                {
                    var match = _rTaskCancel.Match(log);
                    if (match.Success)
                    {
                        endTime = DateTime.ParseExact(match.Groups["time"].Value, TimeFormat, null);
                        lastIndex = match.Index - 1;
                        cancelled = true;
                    }
                }

                var matches = _rTestStart.Matches(log);
                for (var i = matches.Count - 1; i >= 0; i--)
                {
                    var test = Activator.CreateInstance<T>();
                    result.Add(test);
                    test.Name = matches[i].Groups["name"].Value;
                    var startTime = DateTime.ParseExact(matches[i].Groups["time"].Value, TimeFormat, null);
                    if (endTime.HasValue)
                    {
                        //Since time format doesn't include the date, we add a day if EndTime is earlier than StartTime.
                        if (endTime < startTime) endTime = endTime.Value.AddDays(1);
                        test.Duration = endTime.Value - startTime;
                    }

                    if (lastIndex == 0) lastIndex = log.Length - 1;
                    var startIndex = matches[i].Index + matches[i].Length;
                    if (lastIndex < startIndex)
                    {
                        _logger.LogWarning("Invalid log structure: Test log beyond task summary, skipping test.");
                        lastIndex = startIndex;
                        continue;
                    }
                    var testLog = log.Substring(startIndex, lastIndex - startIndex);

                    lastIndex = startIndex;

                    var matchErr = _rTestError.Match(testLog);
                    if (!matchErr.Success) continue;
                    test.Error = matchErr.Groups["error"].Value;
                    if (errIndex < stacksList.Count && errIndex >= 0)
                    {
                        test.CallStack = stacksList[errIndex];
                    }
                    else
                    {
                        if (!cancelled) _logger.LogWarning($"Failed to get the stacktrace for test '{test.Name}'");
                    }
                    errIndex--;
                }

                if (errIndex != -1 && !cancelled)
                    _logger.LogWarning(
                        $"Detected a mismatching number of call stacks to errors: Stacks={stacksList.Count}, Errors = {stacksList.Count - errIndex + 1}");
                _logger.LogInformation($"found {result.Count} UI tests");

                return result;

            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unexpected parsing error");
                return null;
            }
        }

        public TestType TestType => TestType.UI;
    }
}
