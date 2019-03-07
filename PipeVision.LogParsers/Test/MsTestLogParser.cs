using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using PipeVision.Domain;
// ReSharper disable StringLiteralTypo

namespace PipeVision.LogParsers.Test
{
    public class MsTestLogParser : IMsTestLogParser
    {
        private const string PLineLookBehind = @"(?<=\n)";
        private const string PLinePrefix = @"(?>.{3})?";
        private const string PTime = @"\d{2}:\d{2}:\d{2}\.\d{3}";
        private static readonly string PTimeGroup = $@"(?'time'{PTime})";
        private const string PLineSuffix = @"\r?\n";

        private static readonly string PLine = $@"{PLinePrefix}{PTime}(?'line'[^\n]*){PLineSuffix}";

        private static readonly string PTestName = $@"{PLineLookBehind}{PLinePrefix}{PTimeGroup} (?'status'Passed|Failed) *(?'name'[^\n]*){PLineSuffix}";
        private static readonly string PError = $@"{PLinePrefix}{PTime} Error Message[^\n]*{PLineSuffix}(?'error'.*?)";
        private static readonly string PStack = $@"{PLinePrefix}{PTime} Stack Trace[^\n]*{PLineSuffix}(?'stack'.*?){PLineSuffix}{PLinePrefix}{PTime} {PLineSuffix}";
        private static readonly string PTest = $@"(?s){PTestName}(?>{PError}{PStack})?";

        private const string RgxLinePrefix = @"(?m)^(?>.{3})?(?'time'\d{2}:\d{2}:\d{2}\.\d{3})\s";
        private const string RgxHeader =  @"Microsoft \(R\) Test Execution Command Line Tool";
        //todo: get the last line before first test for more accurate duration reporting, as this would include any preparation time in first test's duration.
        private const string RgxExecutionStart = RgxLinePrefix + @"Starting (?>test )?execution";
        private readonly Regex _rExecutionStart = new Regex(RgxExecutionStart, RegexOptions.Compiled);

        private readonly Regex _rHeader = new Regex(RgxHeader, RegexOptions.Compiled);

        private readonly Regex _rTest = new Regex(PTest, RegexOptions.Compiled);
        private readonly Regex _rLine = new Regex(PLine, RegexOptions.Compiled);

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
                if (currentTime < lastTime) currentTime = currentTime.Add(TimeSpan.FromDays(1));
                test.Duration = currentTime - lastTime;

                if (match.Groups["error"].Success)
                {
                    test.Error = ExtractLines(match.Groups["error"].Value);
                    test.CallStack = ExtractLines(match.Groups["stack"].Value);
                }
                else if (match.Groups["status"].Value == "Failed")
                {
                    //No error/stack found for a failed test, populating error with a generic error message to flag the error
                    test.Error = "No Error/Call stack found!";
                }
                result.Add(test);
                lastTime = currentTime;
            }
            return result;
        }

        private string ExtractLines(string rawLines)
        {
            var sb = new StringBuilder();
            foreach (Match error in _rLine.Matches(rawLines))
            {
                sb.AppendLine(error.Groups["line"].Value);
            }

            return sb.ToString();
        }

        public TestType TestType => TestType.Integration;
    }
}
