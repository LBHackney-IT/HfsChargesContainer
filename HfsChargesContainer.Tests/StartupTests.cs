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

        [Theory]
        [InlineData("CHARGES_BATCH_YEARS")]
        [InlineData("BATCH_SIZE")]
        public void StartupConfigureOptionsThrowsWhenBatchYearsOrBatchSizeEnvVarsAreMissing(string appVarKey)
        {
            // arrange
            // setting these to avoid early failure:
            Environment.SetEnvironmentVariable("CHARGES_BATCH_YEARS", "2022;2023");
            Environment.SetEnvironmentVariable("BATCH_SIZE", "250");

            // configuring failure on specific environment variable
            Environment.SetEnvironmentVariable(appVarKey, null);

            // act
            Action configureOptions = () => _classUnderTest.ConfigureOptions(_serviceCollection);

            // assert
            configureOptions.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void StartupConfigureGoogleClientThrowsWhenGoogleCredentialsAreMissing()
        {
            // arrange
            // setting these to avoid early failure:
            Environment.SetEnvironmentVariable("CHARGES_BATCH_YEARS", "2022;2023");
            Environment.SetEnvironmentVariable("BATCH_SIZE", "250");

            // configuring failure on Google Credentials
            Environment.SetEnvironmentVariable("GOOGLE_API_KEY", null);

            // act
            Action configureGoogleClient = () => _classUnderTest.ConfigureGoogleClient(_serviceCollection);

            // assert
            configureGoogleClient.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("DB_HOST")]
        [InlineData("DB_NAME")]
        [InlineData("DB_USER")]
        [InlineData("DB_PASSWORD")]
        public void StartupConfigureDatabaseContextThrowsWhenEitherOfTheDatabaseEnvVarsIsMissing(string appVarKey)
        {
            // arrange
            // setting these to avoid early failure:
            Environment.SetEnvironmentVariable("CHARGES_BATCH_YEARS", "2022;2023");
            Environment.SetEnvironmentVariable("BATCH_SIZE", "250");
            Environment.SetEnvironmentVariable("DB_HOST", "url_to_host");
            Environment.SetEnvironmentVariable("DB_NAME", "my_db_name");
            Environment.SetEnvironmentVariable("DB_USER", "my_db_user");
            Environment.SetEnvironmentVariable("DB_PASSWORD", "secret_password");

            // configuring failure on specific environment variable
            Environment.SetEnvironmentVariable(appVarKey, null);

            // act
            Action configureDbContext = () => _classUnderTest.ConfigureDatabaseContext(_serviceCollection);

            // assert
            configureDbContext.Should().Throw<ArgumentNullException>();
        }
    }
}
