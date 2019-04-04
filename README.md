https://github.com/MHanafy/PipeVision/blob/develop/docs/Architecture.PNG
# PipeVision
Pipelines are amazing in enabling agile development, making continuous deployment possible.
As software projects grow in complexity, as the number of tests increase, and pipelines grow and become less maintainable.
The primary purpose of piplines is to provide feedback on the release readiness, and when they grow beyond a certain point, the feedback becomes fuzzy, and ownership of breaking changes becomes blurry.
Therefore, I saw a need for a tool to parse the logs and extract the failing tests, call stack, and the relevant code changes.

## Architecture
![alt text](https://raw.githubusercontent.com/MHanafy/PipeVision/develop/Docs/Architecture.PNG "PipeVision deployment architecture")

PipeVision consists of two main components, a data aggregation tool that executes periodically to analyze pipelines and parse test execution logs, then updates the database; and a web interface to show analytical data about failed tests.

## Screenshots
![alt text](https://raw.githubusercontent.com/MHanafy/PipeVision/develop/Docs/TestsSummary.PNG "Summary for currently failing tests")
![alt text](https://raw.githubusercontent.com/MHanafy/PipeVision/develop/Docs/TestFailedRuns.PNG "Test failed runs analysis")