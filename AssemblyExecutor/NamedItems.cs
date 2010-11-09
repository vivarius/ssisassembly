namespace SSISExecuteAssemblyTask100
{
    internal static class NamedStringMembers
    {
        public const string ASSEMBLY_CONNECTOR = "AssemblyConnector";
        public const string ASSEMBLY_PATH = "AssemblyPath";
        public const string ASSEMBLY_NAMESPACE = "AssemblyNamespace";
        public const string ASSEMBLY_CLASS = "AssemblyClass";
        public const string ASSEMBLY_METHOD = "AssemblyMethod";
        public const string MAPPING_PARAMS = "MappingParams";
        public const string OUTPUT_VARIABLE = "OutPutVariable";
        public const string CONFIGURATION_FILE = "ConfigurationFile";
        public const string CONFIGURATION_TYPE = "ConfigurationType";
    }

    internal static class ConfigurationType
    {
        public const string NO_CONFIGURATION = "No Configuration File";
        public const string FILE_CONNECTOR = "File Connector";
        public const string TASK_VARIABLE = "Variable";
    }

    internal static class ParameterDirection
    {
        public const string OUT = "Out";
        public const string IN = "In";
    }
}