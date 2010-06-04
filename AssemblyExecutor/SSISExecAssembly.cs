using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using SSISAssemblyExecutor;
using DTSExecResult = Microsoft.SqlServer.Dts.Runtime.DTSExecResult;
using DTSProductLevel = Microsoft.SqlServer.Dts.Runtime.DTSProductLevel;
using VariableDispenser = Microsoft.SqlServer.Dts.Runtime.VariableDispenser;

namespace SSISAssemblyExecuter100.SSIS
{
    [DtsTask(
        DisplayName = "Execute Assembly Task",
        UITypeName = "SSISAssemblyExecutor.SSISExecAssemblyUIInterface" +
        ",SSISAssemblyExecuter100," +
        "Version=1.0.0.60," +
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
        [Category("General"), Description("The path to the assembly")]
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
                componentEvents.FireError(0, "SSISExecAssembly", "A method path is required.", "", 0);
                IsBaseValid = false;
            }

            return IsBaseValid ? DTSExecResult.Success : DTSExecResult.Failure;
        }

        #endregion

        #region Execute

        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser,
                                              IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {

            bool refire = false;

            GetNeededVariables(variableDispenser);

            componentEvents.FireInformation(0, "SSISAssembly", "Filename: " + connections[AssemblyConnector].ConnectionString, string.Empty, 0, ref refire);
            try
            {
                AssemblyPath = connections[AssemblyConnector].ConnectionString;
                componentEvents.FireInformation(0, "SSISAssembly", "Starts executing method..." + AssemblyMethod, string.Empty, 0,
                                                ref refire);
                Assembly assembly = Assembly.LoadFrom(AssemblyPath);

                if (assembly == null)
                    throw new Exception("Assembly instance is NULL");

                var type = ReflectionTools.GetTypeFromName(assembly, string.Format("{0}.{1}", AssemblyNamespace, AssemblyClass));

                if (type == null)
                    throw new Exception("Assembly's type is NULL");

                if (type.IsClass)
                {
                    ReflectionTools.CreateInstance(assembly, type);
                }

                object retValue = type.InvokeMember(AssemblyMethod,
                                                      BindingFlags.InvokeMethod,
                                                      Type.DefaultBinder,
                                                      (BindingFlags.Static | BindingFlags.Public),
                                                      GetValuedParams(vars, variableDispenser));

                if (vars[OutPutVariable] != null && retValue != null)
                {
                    vars[OutPutVariable].Value = retValue;
                }


                componentEvents.FireInformation(0, "SSISAssemblyTask", "The method was executed.", string.Empty, 0,
                                                ref refire);
            }
            catch (Exception ex)
            {
                componentEvents.FireError(0, "SSISAssemblyTask", ex.ToString(), "", 0);
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

        private void GetNeededVariables(VariableDispenser variableDispenser)
        {
            try
            {   //Regex regex = new Regex(@"^@[\s*[a-zA-Z::\s]+\s*]$");
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

        private object EvaluateExpression(string mappedParam, VariableDispenser variableDispenser)
        {
            object variableObject = null;

            ExpressionEvaluatorClass expressionEvaluatorClass = new ExpressionEvaluatorClass
                                                                    {
                                                                        Expression = mappedParam
                                                                    };

            expressionEvaluatorClass.Evaluate(DtsConvert.GetExtendedInterface(variableDispenser), out variableObject, false);
            return variableObject;
        }

        private object[] GetValuedParams(Variables vars, VariableDispenser variableDispenser)
        {
            var objects = new ArrayList();

            try
            {
                var mappedParams = MappingParams.Split(';');
                foreach (var param in from pStr in mappedParams where pStr.Trim().Length != 0 select pStr.Split('|')[1])
                {
                    objects.Add(!(param.Contains("@"))
                                    ? vars[param].Value
                                    : EvaluateExpression(param, variableDispenser));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }

            return objects.ToArray();
        }

        #endregion

        #region IDTSComponentPersist Members

        void IDTSComponentPersist.LoadFromXML(XmlElement node, IDTSInfoEvents infoEvents)
        {
            if (node.Name != "SSISExecAssembly")
            {
                throw new Exception("Unexpected task element when loading task.");
            }

            try
            {
                AssemblyPath = node.Attributes.GetNamedItem("AssemblyPath").Value;
                AssemblyConnector = node.Attributes.GetNamedItem("AssemblyConnector").Value;
                AssemblyNamespace = node.Attributes.GetNamedItem("AssemblyNamespace").Value;
                AssemblyClass = node.Attributes.GetNamedItem("AssemblyClass").Value;
                AssemblyMethod = node.Attributes.GetNamedItem("AssemblyMethod").Value;
                MappingParams = node.Attributes.GetNamedItem("MappingParams").Value;
                OutPutVariable = node.Attributes.GetNamedItem("OutPutVariable").Value;
            }
            catch
            {
                //throw;
            }
        }

        void IDTSComponentPersist.SaveToXML(XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            XmlElement taskElement = doc.CreateElement(string.Empty, "SSISExecAssembly", string.Empty);

            XmlAttribute assemblyConnector = doc.CreateAttribute(string.Empty, "AssemblyConnector", string.Empty);
            assemblyConnector.Value = AssemblyConnector;

            XmlAttribute assemblyPathAttribute = doc.CreateAttribute(string.Empty, "AssemblyPath", string.Empty);
            assemblyPathAttribute.Value = AssemblyPath;

            XmlAttribute assemblyNamespaceAttribute = doc.CreateAttribute(string.Empty, "AssemblyNamespace", string.Empty);
            assemblyNamespaceAttribute.Value = AssemblyNamespace;

            XmlAttribute assemblyClassAttribute = doc.CreateAttribute(string.Empty, "AssemblyClass", string.Empty);
            assemblyClassAttribute.Value = AssemblyClass;

            XmlAttribute assemblyMethodAttribute = doc.CreateAttribute(string.Empty, "AssemblyMethod", string.Empty);
            assemblyMethodAttribute.Value = AssemblyMethod;

            XmlAttribute mappingParamsAttribute = doc.CreateAttribute(string.Empty, "MappingParams", string.Empty);
            mappingParamsAttribute.Value = MappingParams;

            XmlAttribute outPutVariableAttribute = doc.CreateAttribute(string.Empty, "OutPutVariable", string.Empty);
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