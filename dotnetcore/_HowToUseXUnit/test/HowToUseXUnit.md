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

## $$$ VSCode integration

## $$$ coverage

## $$$ Parallelism

## $$$ Jenkins integration

* Use xUnit logger
  * Jenkins supports xUnit v2 format
  * [xunit.testlogger @GitHub](https://github.com/spekt/xunit.testlogger)
  * If the above doesn't work, nUnit format is available, too
    * [[.NET Core] VS Code を使った xUnit ユニットテストの導入](https://mseeeen.msen.jp/dotnet-core-xunit-test-project/)

## $$$ 
