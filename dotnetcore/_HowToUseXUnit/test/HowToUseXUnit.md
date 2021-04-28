# How to use xUnit

There are several unit-test frameworks available in ASP.NET Core such as xUnit, nUnit and MSTest,
but xUnit is the tool of choice because it is the latest among them and is designed to be very lean.

## Setup

Just type `dotnet new xunit`, and you will find a .cspoj and a sample test file (UnitTest1.cs) already setup.
Then add a project reference for the target project.

```bash:
$ dotnet new xunit
$ dotnet add reference ../src/example.cspoj
```

## Configuration

Create a new file named *xunit.runner.json* and put a following JSON content into it.

```JSON
{
    "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
}
```

* see Configuration section in [xUnit documentation page](https://xunit.net/#documentation) for details.

## Write and run tests


* see sample code
  * [Fact] and [Theory]
  * [Skip]
  * message and Dispname
  * watch  
  * guidelines (directory structure, filename, namespace, coding conventions)

## VSCode integration

* You can run tests by clicking "Run All Tests", "Run Test" commands and so on, shown on top of each class/method in test code files.
* **dotnet core test explorer** show the list of all tests for you, but actually I couldn't show the list for unknown reason.
    * It has something to do with dotnet cli tool language settings, but after I changed language to English I still have the same issue.
    * [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)

## coverage

You can collect test coverage using `dotnet test` command, then visualize it using `ReportGenerator`. It would be convienient when you use `ReportGenerator` if you install `dotnet-config`.

```Bash
#install ReportGenerator
$ dotnet add package ReportGenerator

#install dotnet-config
$ dotnet tool install --global dotnet-config --version 1.0.0-rc.2

#collect coverage (in covertura format)
# echo "XPlat Code Coverage" | xarges -I {} dotnet test --collect:{} -r testresults [--logger:html]  #Git Bashだとこの形式
$ dotnet test --collect:"XPlat Code Coverage" -r testresults [--logger:html]

#prepare .netconfig file
$ vi .netconfig
$ cat .netconfig
[ReportGenerator]
  reports=testresults/**/coverage.cobertura.xml
  reporttype = HtmlChart
  reporttype = Html
  reporttype = Badges
  reporttype = PngChart
  targetdir = coverage/report
  historydir = coverage/history
  classfilters = -HowToUseXUnit.Program
  title = Unit Test 2 
  verbosity = Verbose

#visualize coverage info
$ dotnet ~/.nuget/packages/reportgenerator/4.8.4/tools/net5.0/ReportGenerator.dll

#show coverage in brawser
$ open coverage/report/index.html
```

* You can use `[ExcludeFromCodeCoverage]` attribute for classes or methods to exclude them from coverage instead of `classfilters`.



**resources**

* [Microsoft - Use code coverage for unit testing](https://docs.microsoft.com/ja-jp/dotnet/core/testing/unit-testing-code-coverage?tabs=linux)
* [GitHub - ReportGenerator](https://github.com/danielpalme/ReportGenerator)
* [GitHub - dotnet-config](https://github.com/dotnetconfig/dotnet-config)

## $$$ Parallelism

## $$$ Jenkins integration

* Use xUnit logger
  * Jenkins supports xUnit v2 format
  * [xunit.testlogger @GitHub](https://github.com/spekt/xunit.testlogger)
  * If the above doesn't work, nUnit format is available, too
    * [[.NET Core] VS Code を使った xUnit ユニットテストの導入](https://mseeeen.msen.jp/dotnet-core-xunit-test-project/)

