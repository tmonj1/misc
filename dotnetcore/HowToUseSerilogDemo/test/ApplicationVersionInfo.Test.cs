using System;
using System.Reflection;
using Xunit;
using SerilogDemo;
using FluentAssertions;

namespace SerilogDemoTest
{
    public class ApplicationVersionInfoTest
    {
        private Assembly _assembly = null;

        /// <summary>
        /// Setup
        /// </summary>
        public ApplicationVersionInfoTest()
        {
            _assembly = Assembly.GetExecutingAssembly();
        }

        [Fact]
        public void GetApplicationVersionInfo()
        {
            // Act
            var versionInfo = _assembly.GetApplicationVersionInfo();

            // Assert
            versionInfo.Should().NotBeNull().And.BeOfType<ApplicationVersionInfo>();
        }
        [Fact]
        public void HasSemanticVersion()
        {
            // Arrange
            var versionInfo = _assembly.GetApplicationVersionInfo();

            // Act
            string name = versionInfo.Name;

            // Assert
            name.Should().NotBeNull();
        }

        public void HasVersion()
        {
            // Arrange
            var versionInfo = _assembly.GetApplicationVersionInfo();

            // Act
            string version = versionInfo.Version;

            // Assert
            version.Should().NotBeNull();
        }
    }
}
