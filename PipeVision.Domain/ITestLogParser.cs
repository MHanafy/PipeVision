using System.Collections.Generic;

namespace PipeVision.Domain
{
    public interface ITestLogParser
    {
        IList<T> Parse<T>(string log)
            where T : ITest;
        TestType TestType { get; }
    }
}
