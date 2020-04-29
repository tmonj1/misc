# How to debug ASP.NET Core Source Code in Visual Studio Code

This document explains how to debug into ASP.NET Core source code (NOT YOUR
source code) in Visual Studio Code (NOT Visual Studio).

`The explanation here only applies to ASP.NET Core 3.x (and
probably 2.x).`

## How to set up

Just open launch.json in your project, and set `justMyCode` to false, enable
`sourceLinkOptions`, and set `searchMicrosoftSymbolServer` in `symbolOptions`
to true as shown below:

```:launch.json
// launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "justMyCode": false,
      "sourceLinkOptions": {
        "*": {
          "enabled": true
        }
      },
      "symbolOptions": {
        "searchPaths": [],
        "searchMicrosoftSymbolServer": true,
        "searchNuGetOrgSymbolServer": false
      },
  :
  : (omitted.)
  :
```

## Why it works

Debugging ASP.NET Core source code is made possible via `Source Link` feature,
which enables you to debug ASP.NET Core source code even when you have no
correspondent symbol files (.pdb) installed on your local machine; they are
downloaded from Microsoft symbol server on demand when you debug into ASP.NET
source code.

## References

* [DEBUGGING ASP.NET CORE 2.0 SOURCE CODE](https://www.stevejgordon.co.uk/debugging-asp-net-core-2-source)
* [Debugging with Symbols](https://docs.microsoft.com/ja-jp/windows/win32/dxtecharts/debugging-with-symbols?redirectedfrom=MSDN)
