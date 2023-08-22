using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using HfsChargesContainer.Helpers.Interfaces;
using HfsChargesContainer.Options;

namespace HfsChargesContainer.Helpers
{
    public class RuntimeEnvVarsHandler : IRuntimeEnvVarsHandler
    {
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly Dictionary<string, string> _ssmToAppNameLookup;

        public RuntimeEnvVarsHandler(
            string environment,
            IAmazonSimpleSystemsManagement? ssmClient = null,
            Dictionary<string, string>? ssmToAppNameLookup = null
        )
        {
            _ssmClient = ssmClient ?? new AmazonSimpleSystemsManagementClient(RegionEndpoint.EUWest2);
            _ssmToAppNameLookup = ssmToAppNameLookup ?? new EnvVarConfig(environment).EnvVarSSMToAppNameLookup;
        }

        private GetParametersResponse GetSSMParameters(List<string> ssmKeys)
        {
            var getParamsTask = _ssmClient.GetParametersAsync(
                new GetParametersRequest {
                    Names = ssmKeys,
                    WithDecryption = true
                });

            return getParamsTask.Result;
        }

        private string MapVariableSSMKeyToAppName(string ssmName)
        {
            try
            {
                return _ssmToAppNameLookup[ssmName];
            }
            catch
            {
                throw new ArgumentException($"SSM key: {ssmName} has no application-scope name mapping.");
            }
        }

        public void LoadRuntimeEnvironmentVariables()
        {
            var ssmKeys = _ssmToAppNameLookup.Keys.ToList();
            var ssmParameters = GetSSMParameters(ssmKeys);

            if (ssmParameters.InvalidParameters.Any())
                throw new ParameterNotFoundException(
                    $"The following parameters were not found: {string.Join(", ", ssmParameters.InvalidParameters)}."
                );

            ssmParameters.Parameters.ForEach(p => {
                var variableSSMName = p.Name;
                var variableValue = p.Value;

                var variableAppName = MapVariableSSMKeyToAppName(variableSSMName);
                Environment.SetEnvironmentVariable(variableAppName, variableValue);
            });
        }
    }
}
