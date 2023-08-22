using System;
using FluentAssertions;
using HfsChargesContainer.Helpers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace HfsChargesContainer.Tests
{
    public class StartupTests
    {
        private Mock<IRuntimeEnvVarsHandler> _runtimeEnvVarHandlerMock;
        private IServiceCollection _serviceCollection = new ServiceCollection();
        private Startup _classUnderTest;

        public StartupTests()
        {

            _runtimeEnvVarHandlerMock = new Mock<IRuntimeEnvVarsHandler>();
            _classUnderTest = new Startup(_serviceCollection, _runtimeEnvVarHandlerMock.Object);
        }

        [Fact]
        public void StartupShouldFailUponInitializingIfHostEnvironmentIsNotSpecified()
        {
            // arrange
            Environment.SetEnvironmentVariable("ENVIRONMENT", null);

            // act
            Action initializeStartup = () => new Startup();

            // arrange
            initializeStartup.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StartupConfigureEnvironmentDoesNotOverrideEnvVarsInLocalEnvironment()
        {
            // arrange
            Environment.SetEnvironmentVariable("ENVIRONMENT", "local");

            // act
            _classUnderTest.ConfigureEnvironment();

            // assert
            _runtimeEnvVarHandlerMock.Verify(h => h.LoadRuntimeEnvironmentVariables(), Times.Never);
        }

        [Theory]
        [InlineData("development")]
        [InlineData("staging")]
        [InlineData("production")]
        public void StartupConfigureEnvironmentLoadsRuntimeVarsInNonLocalEnvironment(string environment)
        {
            // arrange
            Environment.SetEnvironmentVariable("ENVIRONMENT", environment);

            // act
            _classUnderTest.ConfigureEnvironment();

            // assert
            _runtimeEnvVarHandlerMock.Verify(h => h.LoadRuntimeEnvironmentVariables(), Times.Once);
        }

    }
}
