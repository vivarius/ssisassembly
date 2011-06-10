using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using DTSExecResult = Microsoft.SqlServer.Dts.Runtime.DTSExecResult;
using VariableDispenser = Microsoft.SqlServer.Dts.Runtime.VariableDispenser;
using DTSProductLevel = Microsoft.SqlServer.Dts.Runtime.DTSProductLevel;

namespace SSISExecuteAssemblyTask100
{
    [DtsTask(
        DisplayName = "Execute Assembly Task",
        UITypeName = "SSISExecuteAssemblyTask100.SSISExecuteAssemblyTaskUIInterface" +
        ",SSISExecuteAssemblyTask100," +
        "Version=1.0.1.9," +
        "Culture=Neutral," +
        "PublicKeyToken=99d80f2884c4916d",
        IconResource = "ExecuteAssemblyTask.ico",
        RequiredProductLevel = DTSProductLevel.None,
        TaskContact = "Cosmin VLASIU -> cosmin.vlasiu@gmail.com"
        )]
    public class SSISExecuteAssemblyTask : Task, IDTSComponentPersist
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SSISExecuteAssemblyTask"/> class.
        /// </summary>
        public SSISExecuteAssemblyTask()
        {
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the assembly connector.
        /// </summary>
        /// <value>The assembly connector.</value>
        [Category("Component specific"), Description("The connector associated with the task")]
        public string AssemblyConnector { get; set; }
        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>The assembly path.</value>
        [Category("Component specific"), Description("The path to the assembly file")]
        public string AssemblyPath { get; set; }
        /// <summary>
        /// Gets or sets the assembly namespace.
        /// </summary>
        /// <value>The assembly namespace.</value>
        [Category("Component specific"), Description("Source namespace")]
        public string AssemblyNamespace { get; set; }
        /// <summary>
        /// Gets or sets the assembly class.
        /// </summary>
        /// <value>The assembly class.</value>
        [Category("Component specific"), Description("Source class")]
        public string AssemblyClass { get; set; }
        /// <summary>
        /// Gets or sets the assembly method.
        /// </summary>
        /// <value>The assembly method.</value>
        [Category("Component specific"), Description("Method to execute")]
        public string AssemblyMethod { get; set; }
        /// <summary>
        /// Gets or sets the mapping params.
        /// </summary>
        /// <value>The mapping params.</value>
        [Category("Component specific"), Description("Mapping of the parameters of the method to execute")]
        public string MappingParams { get; set; }
        /// <summary>
        /// Gets or sets the out put variable.
        /// </summary>
        /// <value>The out put variable.</value>
        [Category("Component specific"), Description("Output variable")]
        public string OutPutVariable { get; set; }
        /// <summary>
        /// Gets or sets the configuration file.
        /// </summary>
        /// <value>The configuration file.</value>
        [Category("Component specific"), Description("The path to the configuration file")]
        public string ConfigurationFile { get; set; }
        /// <summary>
        /// Gets or sets the type of the configuration.
        /// </summary>
        /// <value>The type of the configuration.</value>
        [Category("Component specific"), Description("Configuration type")]
        public string ConfigurationType { get; set; }

        #endregion

        #region Private Properties

        Variables _vars;

        #endregion

        #region Validate

        /// <summary>
        /// Validate local parameters
        /// </summary>
        /// <param name="connections">A collection of connections used by the task.</param>
        /// <param name="variableDispenser">A <see cref="T:Microsoft.SqlServer.Dts.Runtime.VariableDispenser"/> object for locking variables.</param>
        /// <param name="componentEvents">An object that implements the <see cref="T:Microsoft.SqlServer.Dts.Runtime.IDTSComponentEvents"/> interface.</param>
        /// <param name="log">An object that implements the <see cref="T:Microsoft.SqlServer.Dts.Runtime.IDTSLogging"/> interface.</param>
        /// <returns>
        /// A value from the <see cref="T:Microsoft.SqlServer.Dts.Runtime.DTSExecResult"/> enumeration.
        /// </returns>
        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            bool isBaseValid = true;

            if (base.Validate(connections, variableDispenser, componentEvents, log) != DTSExecResult.Success)
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "Base validation failed", "", 0);
                isBaseValid = false;
            }

            if (!File.Exists(connections[AssemblyConnector].ConnectionString.Trim()))
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "Assembly doesn't exists at the specified path", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyConnector))
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "A connector is required.", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyNamespace))
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "A namespace is required.", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyClass))
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "A class is required.", "", 0);
                isBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyMethod))
            {
                componentEvents.FireError(0, "SSISExecuteAssemblyTask", "A method to execute is required.", "", 0);
                isBaseValid = false;
            }

            if (ConfigurationType != SSISExecuteAssemblyTask100.ConfigurationType.NO_CONFIGURATION)
            {
                if (string.IsNullOrEmpty(ConfigurationType))
                {
                    componentEvents.FireError(0, "SSISExecuteAssemblyTask", "A method to execute is required.", "", 0);
                    isBaseValid = false;
                }
            }

            return isBaseValid ? DTSExecResult.Success : DTSExecResult.Failure;
        }

        #endregion

        #region Execute

        /// <summary>
        /// Just execute the task
        /// </summary>
        /// <param name="connections">Get the list of all connectors</param>
        /// <param name="variableDispenser">Variables Handler</param>
        /// <param name="componentEvents">Fire & Log Messages and exceptions</param>
        /// <param name="log">Log Messages</param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser, IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            bool refire = false;

            GetNeededVariables(variableDispenser);

            componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Filename: " + connections[AssemblyConnector].ConnectionString, string.Empty, 0, ref refire);

            try
            {
                //Get the path and folder of the targeted file
                AssemblyPath = connections[AssemblyConnector].ConnectionString;

                string privateBinPath = Path.GetDirectoryName(AssemblyPath);

                //Inform us...
                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Starts executing method..." + AssemblyMethod, string.Empty, 0, ref refire);

                var appDomainSetup = new AppDomainSetup
                {
                    PrivateBinPath = privateBinPath,
                    ShadowCopyFiles = "true"
                };

                GetConfigurationFile(variableDispenser, connections, appDomainSetup);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", string.Format("AppDomainSetup created... within ConfigurationFile = {0}, PrivateBinPath = {1}, AssemblyMethod = {2}", appDomainSetup.ConfigurationFile, privateBinPath, AssemblyMethod), string.Empty, 0, ref refire);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Create AppDomain... ", string.Empty, 0, ref refire);

                AppDomain appDomain = AppDomain.CreateDomain("AppDomainSSISAssemblyTask",
                                                             null,
                                                             appDomainSetup);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Create AssemblyHandler... ", string.Empty, 0, ref refire);

                var assemblyLoader = (AssemblyHandler)appDomain.CreateInstanceAndUnwrap(typeof(AssemblyHandler).Assembly.FullName,
                                                                                        typeof(AssemblyHandler).FullName);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "assemblyLoader.LoadAssembly... ", string.Empty, 0, ref refire);

                assemblyLoader.LoadAssembly(AssemblyPath);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Get Parameters's values", string.Empty, 0, ref refire);

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "MappingParams = " + MappingParams, string.Empty, 0, ref refire);

                var varObjects = GetValuedParamsWithoutMethodInfo(_vars, variableDispenser);

                foreach (var varObject in varObjects)
                {
                    componentEvents.FireInformation(0, "SSIS Execute Assembly Task", string.Format("Params obtained {0}", varObject), string.Empty, 0, ref refire);
                }

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Call ExecuteMethod method... ", string.Empty, 0, ref refire);

                object retValue = assemblyLoader.ExecuteMethod(string.Format("{0}.{1}", AssemblyNamespace, AssemblyClass), AssemblyMethod, varObjects.ToArray());

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Get returned value if it exists or is not null... ", string.Empty, 0, ref refire);

                //get returned value if it exists or is not null
                if (retValue != null && string.IsNullOrEmpty(GetVariableFromNamespaceContext(OutPutVariable)) && _vars[GetVariableFromNamespaceContext(OutPutVariable)] != null)
                    _vars[GetVariableFromNamespaceContext(OutPutVariable)].Value = retValue;

                componentEvents.FireInformation(0, "SSIS Execute Assembly Task", "Get REF or OUT values obtained after the execution of the method ... ", string.Empty, 0, ref refire);

                // Get REF or OUT values obtained after the execution of the method 
                GetRefValueParamsWithoutMethodInfo(varObjects.ToArray());

                try
                {
                    AppDomain.Unload(appDomain);
                    componentEvents.FireInformation(0, "SSISAssemblyTask",
                                                   appDomain.IsFinalizingForUnload()
                                                       ? "The domain Is Finalizing For Unload."
                                                       : @"The domain Is Not Finalizing For Unload! Maybe you will catch an ""Invalid access to memory location.""",
                                                   string.Empty, 0,
                                                   ref refire);
                }
                catch (AppDomainUnloadedException appDomainUnloadedException)
                {
                    componentEvents.FireInformation(0, "SSISAssemblyTask",
                                                   string.Format(@"The assembly's AppDomain does not exist. Details: {0}", appDomainUnloadedException.Message),
                                                   string.Empty, 0,
                                                   ref refire);
                }

                //...finally we log the successfull information
                componentEvents.FireInformation(0, "SSISAssemblyTask", "The method was executed successfully.", string.Empty, 0, ref refire);
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0, "SSISAssemblyTask", string.Format("Problem: {0} {1}", ex.Message, ex.StackTrace), "", 0);
            }
            finally
            {

                if (_vars.Locked)
                {
                    _vars.Unlock();
                }
            }

            return base.Execute(connections, variableDispenser, componentEvents, log, transaction);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fill the ConfigurationFile of AppDomain property if a ConnectionFile Property is specified
        /// </summary>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="connections">The connections.</param>
        /// <param name="appDomainSetup">The app domain setup.</param>
        private void GetConfigurationFile(VariableDispenser variableDispenser, Connections connections, AppDomainSetup appDomainSetup)
        {
            if (ConfigurationType != SSISExecuteAssemblyTask100.ConfigurationType.NO_CONFIGURATION)
            {
                string configurationFile = string.Empty;

                if (ConfigurationType == SSISExecuteAssemblyTask100.ConfigurationType.FILE_CONNECTOR)
                    configurationFile = connections[ConfigurationFile].ConnectionString;

                if (ConfigurationType == SSISExecuteAssemblyTask100.ConfigurationType.TASK_VARIABLE)
                    configurationFile = (ConfigurationFile.Contains('@'))
                                            ? EvaluateExpression(ConfigurationFile, variableDispenser).ToString()
                                            : _vars[ConfigurationFile].Value.ToString();

                appDomainSetup.ConfigurationFile = configurationFile;
            }
        }

        /// <summary>
        /// Determines whether [is variable in lock for read or write] [the specified lock for read].
        /// </summary>
        /// <param name="lockForRead">The lock for read.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>
        /// 	<c>true</c> if [is variable in lock for read or write] [the specified lock for read]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsVariableInLockForReadOrWrite(List<string> lockForRead, string variable)
        {
            bool retVal = lockForRead.Contains(variable);

            if (!retVal)
            {
                lockForRead.Add(variable);
            }

            return retVal;
        }

        /// <summary>
        /// Get REF or OUT values obtained after the execution of the method
        /// </summary>
        /// <param name="paramObject">The param object.</param>
        private void GetRefValueParamsWithoutMethodInfo(object[] paramObject)
        {
            int paramIndex = 0;

            string[] mappedParams = MappingParams.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var mappedParam in mappedParams)
            {
                var paramDirection = mappedParam.Split('|')[2];

                if (paramDirection == ParameterDirection.OUT)
                {
                    _vars[mappedParam.Split('|')[1].Trim()].Value = paramObject[paramIndex];
                }

                paramIndex++;
            }
        }

        /// <summary>
        /// Get REF or OUT values obtained after the execution of the method
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <param name="paramObject">The param object.</param>
        private void GetRefValueParams(MethodInfo methodInfo, object[] paramObject)
        {
            int paramIndex = 0;

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                var mappedParams = MappingParams.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                foreach (var param in from param in mappedParams
                                      where param.Length > 0
                                      where param.Split('|')[0].Split('=')[0].Trim() == parameterInfo.Name
                                      where param.Split('|')[0].Split('=')[1].Trim().Contains("&")
                                      select param)
                {
                    _vars[param.Split('|')[1].Trim()].Value = paramObject[paramIndex];
                    break;
                }

                paramIndex++;
            }
        }

        /// <summary>
        /// This method recupers all needed variable saved in the component property 'MappingParams'
        /// </summary>
        /// <param name="variableDispenser">The variable dispenser.</param>
        private void GetNeededVariables(VariableDispenser variableDispenser)
        {

            List<string> lockForRead = new List<string>();

            try
            {
                //Get variables for Method parameter
                var mappedParams = MappingParams.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string mappedParam in mappedParams)
                {
                    var param = mappedParam.Split('|')[1];
                    try
                    {
                        if (param.Contains("@"))
                        {
                            var regexStr = param.Split('@');

                            foreach (var nexSplitedVal in
                                    regexStr.Where(val => val.Trim().Length != 0).Select(strVal => strVal.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)))
                            {
                                if (!IsVariableInLockForReadOrWrite(lockForRead, nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']'))))
                                    variableDispenser.LockForRead(nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']')));
                            }
                        }
                        //else
                        //    variableDispenser.LockForRead(param);
                    }
                    catch
                    {
                    }
                }

                //Get variables for Configuration File
                if (ConfigurationType == SSISExecuteAssemblyTask100.ConfigurationType.TASK_VARIABLE)
                {
                    var param = ConfigurationFile;

                    if (param.Contains("@"))
                    {
                        var regexStr = param.Split('@');

                        foreach (var nexSplitedVal in regexStr.Where(val => val.Trim().Length != 0).Select(strVal => strVal.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            if (!IsVariableInLockForReadOrWrite(lockForRead, nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']'))))
                                variableDispenser.LockForRead(nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']')));
                        }
                    }
                    else
                        variableDispenser.LockForRead(param);
                }

                //Get variable for out put variable // Lock it for "write"
                if (!string.IsNullOrEmpty(OutPutVariable))
                    if (OutPutVariable.Contains("@"))
                    {
                        var regexStr = OutPutVariable.Split('@');

                        foreach (var nexSplitedVal in regexStr.Where(val => val.Trim().Length != 0).Select(strVal => strVal.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            if (!IsVariableInLockForReadOrWrite(lockForRead, nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']'))))
                                variableDispenser.LockForWrite(nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']')));
                        }
                    }

            }
            catch (Exception)
            {
                //We will continue...
            }

            variableDispenser.GetVariables(ref _vars);
        }

        /// <summary>
        /// This method evaluate expressions like @([System::TaskName] + [System::TaskID]) or any other operation created using
        /// ExpressionBuilder
        /// </summary>
        /// <param name="mappedParam">The mapped param.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <returns></returns>
        private static object EvaluateExpression(string mappedParam, VariableDispenser variableDispenser)
        {
            object variableObject = null;

            try
            {
                var expressionEvaluatorClass = new ExpressionEvaluatorClass
                {
                    Expression = mappedParam
                };

                expressionEvaluatorClass.Evaluate(DtsConvert.GetExtendedInterface(variableDispenser), out variableObject, false);
            }
            catch (Exception) // for hardcoded values
            {
                variableObject = mappedParam;
            }

            return variableObject;
        }

        /// <summary>
        /// Gets the variable from namespace context.
        /// </summary>
        /// <param name="ssisVariable">The ssis variable.</param>
        /// <returns></returns>
        private static string GetVariableFromNamespaceContext(string ssisVariable)
        {
            return ssisVariable.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[1].Replace("[", string.Empty).Replace("]", string.Empty).Replace("@", string.Empty);
        }

        /// <summary>
        /// Prepares method's parameters that will be executed
        /// It will recuperate the direct values
        /// It will make the interpretation of the expressions
        /// It will pass NULL value for REF Or OUT params
        /// </summary>
        /// <param name="vars">The vars.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <param name="methodInfo">The method info.</param>
        /// <returns></returns>
        private object[] GetValuedParams(Variables vars, VariableDispenser variableDispenser, MethodInfo methodInfo)
        {
            object[] objects;

            try
            {
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                objects = new object[parameterInfos.Length];

                int paramIndex = 0;

                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    var mappedParams = MappingParams.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    foreach (string param in mappedParams.Where(param => param.Length > 0).Where(param => param.Split('|')[0].Split('=')[0].Trim() == parameterInfo.Name))
                    {
                        objects[paramIndex] = EvaluateExpression(param.Split('|')[1], variableDispenser);
                        //param.Split('|')[1].Contains("@")
                        //                           ? EvaluateExpression(param.Split('|')[1], variableDispenser)
                        //                           : (!(parameterInfo.ParameterType.IsByRef || parameterInfo.IsOut))
                        //                                 ? vars[param.Split('|')[1].Trim()].Value
                        //                                 : null;

                        paramIndex++;

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.StackTrace);
            }

            return objects;
        }

        /// <summary>
        /// Prepares method's parameters that will be executed
        /// It will recuperate the direct values
        /// It will make the interpretation of the expressions
        /// It will pass NULL value for REF Or OUT params
        /// </summary>
        /// <param name="vars">The vars.</param>
        /// <param name="variableDispenser">The variable dispenser.</param>
        /// <returns></returns>
        private List<object> GetValuedParamsWithoutMethodInfo(Variables vars, VariableDispenser variableDispenser)
        {
            var objects = new List<object>();

            try
            {
                string[] mappedParams = MappingParams.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                objects.AddRange(from mappedParam in mappedParams
                                 let paramInfo = mappedParam.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[1]
                                 let paramType = mappedParam.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1]
                                 let paramDirection = mappedParam.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[2]
                                 select Convert.ChangeType(EvaluateExpression(paramInfo.Trim(), variableDispenser)
                                     //select Convert.ChangeType(((paramInfo.Contains("@"))
                                     //                           ? EvaluateExpression(paramInfo.Trim(), variableDispenser)
                                     //                           : (paramDirection == ParameterDirection.IN)
                                     //                                   ? vars[paramInfo.Trim()].Value
                                     //                                   : vars["@" + paramInfo.Trim()].Value)
                                                           , Type.GetType(paramType)));
            }
            catch (Exception exception)
            {
                throw new Exception("Check StackTrace " + exception.Message + " :: " + exception.StackTrace);
            }

            return objects;
        }

        #endregion

        #region IDTSComponentPersist Members

        /// <summary>
        /// Loads component information from XML.
        /// </summary>
        /// <param name="node">The node that contains the information to be loaded.</param>
        /// <param name="infoEvents">An object that implements the <see cref="T:Microsoft.SqlServer.Dts.Runtime.IDTSInfoEvents"/> interface for raising events (errors, warnings, and so on) during persistence.</param>
        void IDTSComponentPersist.LoadFromXML(XmlElement node, IDTSInfoEvents infoEvents)
        {
            if (node.Name != "SSISExecuteAssemblyTask")
            {
                throw new Exception("Unexpected task element when loading task.");
            }

            try
            {
                AssemblyPath = node.Attributes.GetNamedItem(NamedStringMembers.ASSEMBLY_PATH).Value;
                AssemblyConnector = node.Attributes.GetNamedItem(NamedStringMembers.ASSEMBLY_CONNECTOR).Value;
                AssemblyNamespace = node.Attributes.GetNamedItem(NamedStringMembers.ASSEMBLY_NAMESPACE).Value;
                AssemblyClass = node.Attributes.GetNamedItem(NamedStringMembers.ASSEMBLY_CLASS).Value;
                AssemblyMethod = node.Attributes.GetNamedItem(NamedStringMembers.ASSEMBLY_METHOD).Value;
                MappingParams = node.Attributes.GetNamedItem(NamedStringMembers.MAPPING_PARAMS).Value;
                OutPutVariable = node.Attributes.GetNamedItem(NamedStringMembers.OUTPUT_VARIABLE).Value;
                ConfigurationFile = node.Attributes.GetNamedItem(NamedStringMembers.CONFIGURATION_FILE).Value;
                ConfigurationType = node.Attributes.GetNamedItem(NamedStringMembers.CONFIGURATION_TYPE).Value;
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        /// <summary>
        /// Saves a component to XML. Tasks and containers implement this method.
        /// </summary>
        /// <param name="doc">The XML document to which to save the information.</param>
        /// <param name="infoEvents">An object that implements the <see cref="T:Microsoft.SqlServer.Dts.Runtime.IDTSInfoEvents"/> interface for raising events (errors, warnings, and so on) during persistence.</param>
        void IDTSComponentPersist.SaveToXML(XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            XmlElement taskElement = doc.CreateElement(string.Empty, "SSISExecuteAssemblyTask", string.Empty);

            XmlAttribute assemblyConnector = doc.CreateAttribute(string.Empty, NamedStringMembers.ASSEMBLY_CONNECTOR, string.Empty);
            assemblyConnector.Value = AssemblyConnector;

            XmlAttribute assemblyPathAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.ASSEMBLY_PATH, string.Empty);
            assemblyPathAttribute.Value = AssemblyPath;

            XmlAttribute assemblyNamespaceAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.ASSEMBLY_NAMESPACE, string.Empty);
            assemblyNamespaceAttribute.Value = AssemblyNamespace;

            XmlAttribute assemblyClassAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.ASSEMBLY_CLASS, string.Empty);
            assemblyClassAttribute.Value = AssemblyClass;

            XmlAttribute assemblyMethodAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.ASSEMBLY_METHOD, string.Empty);
            assemblyMethodAttribute.Value = AssemblyMethod;

            XmlAttribute mappingParamsAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.MAPPING_PARAMS, string.Empty);
            mappingParamsAttribute.Value = MappingParams;

            XmlAttribute outPutVariableAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.OUTPUT_VARIABLE, string.Empty);
            outPutVariableAttribute.Value = OutPutVariable;

            XmlAttribute configurationFileAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.CONFIGURATION_FILE, string.Empty);
            configurationFileAttribute.Value = ConfigurationFile;

            XmlAttribute configurationTypeAttribute = doc.CreateAttribute(string.Empty, NamedStringMembers.CONFIGURATION_TYPE, string.Empty);
            configurationTypeAttribute.Value = ConfigurationType;

            taskElement.Attributes.Append(assemblyPathAttribute);
            taskElement.Attributes.Append(assemblyConnector);
            taskElement.Attributes.Append(assemblyNamespaceAttribute);
            taskElement.Attributes.Append(assemblyClassAttribute);
            taskElement.Attributes.Append(assemblyMethodAttribute);
            taskElement.Attributes.Append(mappingParamsAttribute);
            taskElement.Attributes.Append(outPutVariableAttribute);
            taskElement.Attributes.Append(configurationFileAttribute);
            taskElement.Attributes.Append(configurationTypeAttribute);

            doc.AppendChild(taskElement);
        }

        #endregion
    }
}
