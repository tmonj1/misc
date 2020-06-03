using System;
using Xunit;
using SerilogDemo;
using FluentAssertions;

namespace SerilogDemoTest
{
    public class AppAssemblyInfoTest
    {
        [Fact]
        public void TestAppName()
        {
            AppAssemblyInfo.AppName.Should().Be("testhost");
        }
        [Fact]
        public void TestAppVersion()
        {
            AppAssemblyInfo.AppVersion.Should().Be("16.5.0");
        }
    }
}
