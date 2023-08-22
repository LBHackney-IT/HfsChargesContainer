using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using FluentAssertions;
using HfsChargesContainer.Helpers;
using HfsChargesContainer.Helpers.Interfaces;
using Moq;
using Xunit;

namespace HfsChargesContainer.Tests.Helpers
{
    public class RuntimeEnvVarsHandlerTests
    {
        private Dictionary<string, string> _ssmToAppKeyLookup;
        private Mock<IAmazonSimpleSystemsManagement> _ssmClientMock;
        private IRuntimeEnvVarsHandler _classUnderTest;

        public RuntimeEnvVarsHandlerTests()
        {
            _ssmToAppKeyLookup = new ()
            {
                { "/area-of-service/environment/db-host", "DB_HOST" },
                { "/nightly-job/environment/google-api-key", "GOOGLE_API_KEY" },
                { "/area-of-service/environment/db-password", "DB_PASSWORD" },
            };
            _ssmClientMock = new Mock<IAmazonSimpleSystemsManagement>();
            _classUnderTest = new RuntimeEnvVarsHandler("environment", _ssmClientMock.Object, _ssmToAppKeyLookup);
        }

        [Fact]
        public void RuntimeEnvVarsHandlerRequestsTheSSMKeysMatchingOnesFromSSMLookupDictionary()
        {
            // arrange
            var expectedSSMKeys = _ssmToAppKeyLookup.Keys.ToList();

            _ssmClientMock
                .Setup(c => c.GetParametersAsync(
                    It.IsAny<GetParametersRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetParametersResponse());

            // act
            _classUnderTest.LoadRuntimeEnvironmentVariables();

            // assert
            _ssmClientMock.Verify(
                u => u.GetParametersAsync(
                    It.Is<GetParametersRequest>(
                        request => expectedSSMKeys.All(
                            key => request.Names.Contains(key)
                        )),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void RuntimeEnvVarsHandlerSetsTheEnvironment()
        {
            // arrange
            var appKeys = _ssmToAppKeyLookup.Values.ToList();
            var ssmKeys = _ssmToAppKeyLookup.Keys.ToList();

            // Clear the environment
            appKeys.ForEach(ak => Environment.SetEnvironmentVariable(ak, null));

            var expectedSSMParams = ssmKeys.Select(sk => new Parameter() {
                Name = sk,
                Value = sk.Length.ToString()
            }).ToList();

            _ssmClientMock
                .Setup(c => c.GetParametersAsync(
                    It.IsAny<GetParametersRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetParametersResponse() {
                    InvalidParameters = new List<string>(),
                    Parameters = expectedSSMParams
                });

            // act
            _classUnderTest.LoadRuntimeEnvironmentVariables();

            // assert
            var runtimeEnvVarVals = appKeys.Select(ak => Environment.GetEnvironmentVariable(ak));

            runtimeEnvVarVals.Should().NotContainNulls();
        }

        [Fact]
        public void RuntimeEnvVarsHandlerMapsSSMValuesToAppScopedEnvVarKeyNamesCorrectly()
        {
            // arrange
            var appKeys = _ssmToAppKeyLookup.Values.ToList();
            var ssmKeys = _ssmToAppKeyLookup.Keys.ToList();

            // Clear the environment
            appKeys.ForEach(ak => Environment.SetEnvironmentVariable(ak, null));

            var expectedSSMParams = ssmKeys.Select(sk => new Parameter() {
                Name = sk,
                Value = sk.Length.ToString()
            }).ToList();

            _ssmClientMock
                .Setup(c => c.GetParametersAsync(
                    It.IsAny<GetParametersRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetParametersResponse() {
                    InvalidParameters = new List<string>(),
                    Parameters = expectedSSMParams
                });

            var expectedAppKeyToSSMValMappings = new Dictionary<string, string>();
            expectedSSMParams.ForEach(p => expectedAppKeyToSSMValMappings.Add(_ssmToAppKeyLookup[p.Name], p.Value));

            foreach(var pair in expectedAppKeyToSSMValMappings)
            {
                Console.WriteLine($"Key: {pair.Key}; Val={pair.Value}");
            }

            // act
            _classUnderTest.LoadRuntimeEnvironmentVariables();

            // assert
            var runtimeEnvVarKeyVal = appKeys.Select(ak => new
            {
                AppKeyName = ak,
                Value = Environment.GetEnvironmentVariable(ak)
            }).ToList();

            foreach (var runtimeEnvVar in runtimeEnvVarKeyVal)
            {
                var expectedEnvVarValue = expectedAppKeyToSSMValMappings[runtimeEnvVar.AppKeyName];
                runtimeEnvVar.Value.Should().Be(expectedEnvVarValue);
            }
        }

        [Fact]
        public void RuntimeEnvVarsHandlerThrowsWhenMissingSSMKeysAreEncountered()
        {
            // arrange
            var notFoundSSMKeys = new List<string>() {
                "/hfs-nightly/environment/sns-arn",
                "/hackney/environment/some-token"
            };

            var expectedErrorMsg = string.Join(", ", notFoundSSMKeys);

            _ssmClientMock
                .Setup(c => c.GetParametersAsync(
                    It.IsAny<GetParametersRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetParametersResponse() {
                    InvalidParameters = notFoundSSMKeys,
                    Parameters = new List<Parameter>()
                });

            // act
            Action loadRuntimeVars = () => _classUnderTest.LoadRuntimeEnvironmentVariables();

            // assert
           loadRuntimeVars.Should().Throw<ParameterNotFoundException>().WithMessage($"*{expectedErrorMsg}*");
        }
    }
}
