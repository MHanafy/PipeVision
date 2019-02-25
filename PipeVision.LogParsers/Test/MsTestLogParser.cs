using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PipeVision.Domain;

namespace PipeVision.LogParsers.Test
{
    public class MsTestLogParser : IMsTestLogParser
    {
        private const string RgxLinePrefix = @"\&\d\|(?'time'\d{2}:\d{2}:\d{2}\.\d{3})\s";
        private const string RgxHeader =  @"Microsoft \(R\) Test Execution Command Line Tool";
        //todo: get the last line before first test for more accurate duration reporting, as this would include any preparation time in first test's duration.
        private const string RgxExecutionStart = RgxLinePrefix + @"Starting test execution, please wait";
        private readonly Regex _rExecutionStart = new Regex(RgxExecutionStart, RegexOptions.Compiled);

        private readonly Regex _rHeader = new Regex(RgxHeader, RegexOptions.Compiled);
        private const string RgxTestFail = @"(?(fail)(?>\&\d\|\d{2}:\d{2}:\d{2}\.\d{3}\sError\sMessage\:\s*\r\n)(?>(?>\&\d\|\d{2}:\d{2}:\d{2}\.\d{3}\s+)(?'error'.*\r\n))+?";
        private const string RgxTestStack = @"(?>\&\d\|\d{2}:\d{2}:\d{2}\.\d{3}\sStack\sTrace\:\s*\r\n)(?>(?>\&.\|\d{2}:\d{2}:\d{2}\.\d{3}\s+)(?'stack'at\s.*\r\n))+)";
        public const string RgxTest = @"\&\d\|(?'time'\d{2}:\d{2}:\d{2}\.\d{3})\s(?>(?'pass'Passed)|(?'fail'Failed))\s*(?'name'.*)\r\n" + RgxTestFail + RgxTestStack;

        private readonly Regex _rTest = new Regex(RgxTest, RegexOptions.Compiled);

        public const string TimeFormat = "HH:mm:ss.fff";

        public IList<T> Parse<T>(string log)
        where T:ITest
        {
            if (!_rHeader.IsMatch(log)) return null;

            var result = new List<T>();
            var lastTime = DateTime.ParseExact(_rExecutionStart.Match(log).Groups["time"].Value, TimeFormat, null).TimeOfDay;

            foreach (Match match in _rTest.Matches(log))
            {
                var currentTime = DateTime.ParseExact(match.Groups["time"].Value, TimeFormat, null).TimeOfDay;
                var test = Activator.CreateInstance<T>();

                test.Name = match.Groups["name"].Value;
                test.Duration = currentTime - lastTime;
                if (match.Groups["fail"].Success)
                {
                    var sb = new StringBuilder();
                    foreach (Capture capture in match.Groups["error"].Captures)
                    {
                        sb.Append(capture.Value);
                    }
                    test.Error = sb.ToString();

                    sb = new StringBuilder();
                    foreach (Capture capture in match.Groups["stack"].Captures)
                    {
                        sb.Append(capture.Value);
                    }
                    test.CallStack = sb.ToString();
                }
                result.Add(test);
                lastTime = currentTime;
            }
            return result;
        }

        public TestType TestType => TestType.Integration;
    }
}
