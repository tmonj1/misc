using System;
using Xunit;
using HowToUseXUnit;
using System.Threading.Tasks;

namespace Xunit.Plain
{
    /// <summary>
    /// A test class inherit from IDisposable.
    /// </summary>
    public class FakeHttpTest2 : IDisposable
    {
        private FakeHttp _fakeHttp = null;

        /// <summary>
        /// Do setup using ctor.
        /// </summary>
        public FakeHttpTest2()
        {
            // Setup: DI by hand
            _fakeHttp = new FakeHttp(new MyHttpClient());
        }

        /// <summary>
        /// Do tear down in Dispose method.
        /// </summary>
        public void Dispose()
        {
            // Teardown
            _fakeHttp = null;
        }

        /// <summary>
        /// xUnit test runnner uses [Fact] or [Theory] attribute to find test cases.
        /// </summary>
        [Fact]
        public void TestGetString1()
        {
            // Arrange
            string url = "http://a.com";

            // Act
            var value = _fakeHttp.GetString(url);

            // Assert
            Assert.True(value == url);
        }

        /// <summary>
        /// Specify one or more parameters using InlineData for [Theory] method.
        /// <param name="url"></param>
        /// </summary>
        [Theory]
        [InlineData("http://xxx")]
        [InlineData("xyz")]
        public void TestGetString2(string url)
        {
            // Act
            var value = _fakeHttp.GetString(url);

            // Assert
            Assert.True(value == url);
        }

        /// <summary>
        /// Return type is `async Task` when testing an async method.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TestGetStringAsync()
        {
            // Arrange
            string url = "http://a.com";

            // Act
            var value = await _fakeHttp.GetStringAsync(url);

            // Assert
            Assert.True(value == url);
        }
    }
}
