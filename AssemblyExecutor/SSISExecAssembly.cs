using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using SSISAssemblyExecutor;
using DTSExecResult = Microsoft.SqlServer.Dts.Runtime.DTSExecResult;
using VariableDispenser = Microsoft.SqlServer.Dts.Runtime.VariableDispenser;
using DTSProductLevel = Microsoft.SqlServer.Dts.Runtime.DTSProductLevel;

namespace SSISAssemblyExecuter100.SSIS
{
    [DtsTask(
        DisplayName = "Execute Assembly Task",
        UITypeName = "SSISAssemblyExecutor.SSISExecAssemblyUIInterface" +
        ",SSISAssemblyExecuter100," +
        "Version=1.0.0.74," +
        "Culture=Neutral," +
        "PublicKeyToken=bf357d0d3805f1fe",
        IconResource = "SSISAssemblyExecutor.SSISAssemblyExecutor.ico",
        RequiredProductLevel = DTSProductLevel.None
        )]
    public class SSISExecAssembly : Task, IDTSComponentPersist
    {
        #region Constructor
        public SSISExecAssembly()
        {
        }

        #endregion

        #region Public Properties
        [Category("General"), Description("The connector associated with the task")]
        public string AssemblyConnector { get; set; }
        [Category("General"), Description("The path to the assembly file")]
        public string AssemblyPath { get; set; }
        [Category("General"), Description("Source namespace")]
        public string AssemblyNamespace { get; set; }
        [Category("General"), Description("Source class")]
        public string AssemblyClass { get; set; }
        [Category("General"), Description("Method to execute")]
        public string AssemblyMethod { get; set; }
        [Category("General"), Description("Mapping of the parameters of the method to execute")]
        public string MappingParams { get; set; }
        [Category("General"), Description("Output variable")]
        public string OutPutVariable { get; set; }
        #endregion

        #region Private Properties

        Variables vars = null;

        #endregion

        #region InitializeTask

        #endregion

        #region Validate

        /// <summary>
        /// Validate local parameters
        /// </summary>
        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser,
                                               IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            bool IsBaseValid = true;

            if (base.Validate(connections, variableDispenser, componentEvents, log) != DTSExecResult.Success)
            {
                componentEvents.FireError(0, "SSISExecAssembly", "Base validation failed", "", 0);
                IsBaseValid = false;
            }

            if (!File.Exists(connections[AssemblyConnector].ConnectionString.Trim()))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "Assembly doesn't exists at the specified path", "", 0);
                IsBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyConnector))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "A connector is required.", "", 0);
                IsBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyNamespace))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "A namespace is required.", "", 0);
                IsBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyClass))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "A class is required.", "", 0);
                IsBaseValid = false;
            }

            if (string.IsNullOrEmpty(AssemblyMethod))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "A method to execute is required.", "", 0);
                IsBaseValid = false;
            }

            return IsBaseValid ? DTSExecResult.Success : DTSExecResult.Failure;
        }

        #endregion

        #region Execute

        /// <summary>
        /// This method is a run-time method executed dtsexec.exe
        /// </summary>
        /// <param name="connections"></param>
        /// <param name="variableDispenser"></param>
        /// <param name="componentEvents"></param>
        /// <param name="log"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser,
                                              IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {

            bool refire = false;

            GetNeededVariables(variableDispenser);

            componentEvents.FireInformation(0, "SSISAssembly", "Filename: " + connections[AssemblyConnector].ConnectionString, string.Empty, 0, ref refire);
            try
            {
                //Get the path of the targeted file
                AssemblyPath = connections[AssemblyConnector].ConnectionString;

                //Inform us...
                componentEvents.FireInformation(0, "SSISAssembly", "Starts executing method..." + AssemblyMethod, string.Empty, 0, ref refire);

                //Load assembly from the given path
                Assembly assembly = Assembly.LoadFrom(AssemblyPath);

                if (assembly == null)
                    throw new Exception("Assembly instance is NULL");

                //Get the type -Namespace.Class-
                var type = ReflectionTools.GetTypeFromName(assembly, string.Format("{0}.{1}", AssemblyNamespace, AssemblyClass));

                if (type == null)
                    throw new Exception("Assembly's type is NULL");

                object retValue = null, instanceObject = null;

                //Obtain your method information
                MethodInfo methodInfo = type.GetMethod(AssemblyMethod);

                //Prepare your parameters to be passed as parameters
                object[] paramObject = GetValuedParams(vars, variableDispenser, methodInfo);

                //Check method type; if it's static...
                if (methodInfo.IsStatic)
                {
                    //Invoke static member
                    retValue = type.InvokeMember(AssemblyMethod,
                                                     BindingFlags.InvokeMethod,
                                                     null,
                                                     BindingFlags.Static | BindingFlags.Public,
                                                     paramObject);
                }

                //Check object type: it is a instantiable object (class) //TODO check within a  structure?
                if (type.IsClass && !type.IsAbstract && !type.IsSealed)
                {
                    instanceObject = ReflectionTools.CreateInstance(assembly, type);
                    //invoke instanciated member
                    retValue = methodInfo.Invoke(instanceObject, paramObject);
                }

                //get returned value if it exists or is not null
                if (vars[OutPutVariable] != null && retValue != null)
                {
                    vars[OutPutVariable].Value = retValue;
                }

                // Get REF or OUT values obtained after the execution of the method 
                GetRefValueParams(methodInfo, paramObject);

                //...finally we log the successfull information
                componentEvents.FireInformation(0, "SSISAssemblyTask", "The method was executed successfully.",
                                                string.Empty, 0,
                                                ref refire);
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0,
                                          "SSISAssemblyTask",
                                          string.Format("Problem: {0}", 
                                                        ex.Message),
                                          "",
                                          0);
            }
            finally
            {
                if (vars.Locked)
                {
                    vars.Unlock();
                }
            }

            return base.Execute(connections, variableDispenser, componentEvents, log, transaction);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get REF or OUT values obtained after the execution of the method 
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="paramObject"></param>
        private void GetRefValueParams(MethodInfo methodInfo, object[] paramObject)
        {
            int paramIndex = 0;

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                var mappedParams = MappingParams.Split(';').ToArray();

                foreach (string param in from param in mappedParams
                                         where param.Length > 0
                                         where param.Split('|')[0].Split('=')[0].Trim() == parameterInfo.Name
                                         where param.Split('|')[0].Split('=')[1].Trim().Contains("&")
                                         select param)
                {
                    vars[param.Split('|')[1].Trim()].Value = paramObject[paramIndex];
                    break;
                }

                paramIndex++;
            }
        }

        /// <summary>
        /// This method recupers all needed variable saved in the component property 'MappingParams'
        /// </summary>
        /// <param name="variableDispenser"></param>
        private void GetNeededVariables(VariableDispenser variableDispenser)
        {
            try
            {
                var mappedParams = MappingParams.Split(';');

                for (int index = 0; index < mappedParams.Length - 1; index++)
                {
                    var param = mappedParams[index].Split('|')[1];
                    if (param.Contains("@"))
                    {
                        var regexStr = param.Split('@');

                        foreach (var nexSplitedVal in
                            regexStr.Where(val => val.Trim().Length != 0).Select(strVal => strVal.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries)))
                        {
                            variableDispenser.LockForRead(nexSplitedVal[1].Remove(nexSplitedVal[1].IndexOf(']')));
                        }
                    }
                    else
                        variableDispenser.LockForRead(param);
                }

                if (!string.IsNullOrEmpty(OutPutVariable))
                    variableDispenser.LockForWrite(OutPutVariable);
            }
            catch
            {
                //We will continue...
            }

            variableDispenser.GetVariables(ref vars);
        }

        /// <summary>
        /// This method evaluate expressions like @([System::TaskName] + [System::TaskID]) or any other operation created using 
        /// ExpressionBuilder
        /// </summary>
        /// <param name="mappedParam"></param>
        /// <param name="variableDispenser"></param>
        /// <returns></returns>
        private static object EvaluateExpression(string mappedParam, VariableDispenser variableDispenser)
        {
            object variableObject = null;

            ExpressionEvaluatorClass expressionEvaluatorClass = new ExpressionEvaluatorClass
                                                                    {
                                                                        Expression = mappedParam
                                                                    };

            expressionEvaluatorClass.Evaluate(DtsConvert.GetExtendedInterface(variableDispenser), out variableObject, false);
            return variableObject;
        }

        /// <summary>
        /// Prepares method's parameters that will be executed 
        ///     It will recuperate the direct values
        ///     It will make the interpretation of the expressions
        ///     It will pass NULL value for REF Or OUT params
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="variableDispenser"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private object[] GetValuedParams(Variables vars, VariableDispenser variableDispenser, MethodInfo methodInfo)
        {
            object[] objects = null;

            try
            {
                ParameterInfo[] parameterInfos = methodInfo.GetParameters();

                objects = new object[parameterInfos.Length];

                int paramIndex = 0;
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    var mappedParams = MappingParams.Split(';').ToArray();
                    foreach (string param in
                        mappedParams.Where(param => param.Length > 0).Where(param => param.Split('|')[0].Split('=')[0].Trim() == parameterInfo.Name))
                    {
                        objects[paramIndex] = param.Split('|')[1].Contains("@")
                                                  ? EvaluateExpression(param.Split('|')[1], variableDispenser)
                                                  : (!(parameterInfo.ParameterType.IsByRef || parameterInfo.IsOut))
                                                        ? vars[param.Split('|')[1].Trim()].Value
                                                        : null;

                        paramIndex++;

                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }

            return objects;
        }

        #endregion

        #region IDTSComponentPersist Members

        //Get properties from package
        void IDTSComponentPersist.LoadFromXML(XmlElement node, IDTSInfoEvents infoEvents)
        {
            if (node.Name != "SSISExecAssembly")
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
            }
            catch
            {
                //throw;
            }
        }

        //Save properties to package
        void IDTSComponentPersist.SaveToXML(XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            XmlElement taskElement = doc.CreateElement(string.Empty, "SSISExecAssembly", string.Empty);

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

            taskElement.Attributes.Append(assemblyPathAttribute);
            taskElement.Attributes.Append(assemblyConnector);
            taskElement.Attributes.Append(assemblyNamespaceAttribute);
            taskElement.Attributes.Append(assemblyClassAttribute);
            taskElement.Attributes.Append(assemblyMethodAttribute);
            taskElement.Attributes.Append(mappingParamsAttribute);
            taskElement.Attributes.Append(outPutVariableAttribute);

            doc.AppendChild(taskElement);
        }

        #endregion
    }
}
