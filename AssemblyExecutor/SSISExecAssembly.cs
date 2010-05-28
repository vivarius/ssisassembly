using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microsoft.SqlServer.Dts.Runtime;

namespace SSISAssemblyExecutor
{
    [DtsTask(
        DisplayName = "Task 'Execute Assembly'",
        UITypeName = "SSISAssemblyExecutor.SSISExecAssemblyUIInterface" +
        ",SSISAssemblyExecuter100," +
        "Version=1.0.0.18," +
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

        public string AssemblyConnector { get; set; }
        public string AssemblyPath { get; set; }
        public string AssemblyNamespace { get; set; }
        public string AssemblyClass { get; set; }
        public string AssemblyMethod { get; set; }

        #endregion

        #region InitializeTask

        #endregion

        #region Validate

        /// <summary>
        /// Validate local parameter
        /// </summary>
        public override DTSExecResult Validate(Connections connections, VariableDispenser variableDispenser,
                                               IDTSComponentEvents componentEvents, IDTSLogging log)
        {
            Boolean IsBaseValid = true;

            if (base.Validate(connections, variableDispenser, componentEvents, log) != DTSExecResult.Success)
            {
                componentEvents.FireError(0, "SSISExecAssembly", "Base validation failed", "", 0);
                IsBaseValid = false;
            }
            if (string.IsNullOrEmpty(AssemblyPath))
            {
                componentEvents.FireError(0, "SSISExecAssembly", "A file path is required.", "", 0);
                IsBaseValid = false;
            }

            if (IsBaseValid)
            {
                return DTSExecResult.Success;
            }
            else
            {
                return DTSExecResult.Failure;
            }
        }

        #endregion

        #region Execute

        /// <summary>
        /// This method is called at runtime, and does all the "work".
        /// </summary>
        public override DTSExecResult Execute(Connections connections, VariableDispenser variableDispenser,
                                              IDTSComponentEvents componentEvents, IDTSLogging log, object transaction)
        {
            Variables vars = null;
            bool _refire = false;

            //variableDispenser.LockForRead(this.Recordset);
            variableDispenser.GetVariables(ref vars);
            componentEvents.FireInformation(0, "SSISAssembly", "Filename: " + AssemblyPath, string.Empty, 0, ref _refire);
            try
            {
                componentEvents.FireInformation(0, "SSISAssembly", "Starts executing method..." + AssemblyMethod, string.Empty, 0,
                                                ref _refire);
                Assembly assembly = Assembly.LoadFrom(AssemblyPath);

                if (assembly == null)
                    throw new Exception("Assembly instance is NULL");

                var type = ReflectionTools.GetTypeFromName(assembly, string.Format("{0}.{1}", AssemblyNamespace, AssemblyClass));

                if (type == null)
                    throw new Exception("Assembly's type is NULL");

                type.InvokeMember(AssemblyMethod,
                                  BindingFlags.InvokeMethod,
                                  null,
                                  (BindingFlags.Static | BindingFlags.Public),
                                  new object[] { });

                componentEvents.FireInformation(0, "SSISAssemblyTask", "The method was executed.", string.Empty, 0,
                                                ref _refire);
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
        public static string SerializeObject(object Parameter)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string text;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, Parameter);
                text = Convert.ToBase64String(stream.GetBuffer());
                stream.Close();
            }
            return text;
        }

        public static object DeserializeObject(string Parameter)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Byte[] str = Convert.FromBase64String(Parameter);
            object objParam;
            using (var stream = new MemoryStream(str))
            {
                objParam = formatter.Deserialize(stream);
            }
            return objParam;
        }
        #endregion

        #region IDTSComponentPersist Members

        void IDTSComponentPersist.LoadFromXML(System.Xml.XmlElement node, IDTSInfoEvents infoEvents)
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

            }
            catch
            {
                //throw;
            }
        }

        void IDTSComponentPersist.SaveToXML(System.Xml.XmlDocument doc, IDTSInfoEvents infoEvents)
        {
            XmlElement taskElement = doc.CreateElement(string.Empty,
                                                       "SSISExecAssembly", string.Empty);

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


            taskElement.Attributes.Append(assemblyPathAttribute);
            taskElement.Attributes.Append(assemblyConnector);
            taskElement.Attributes.Append(assemblyNamespaceAttribute);
            taskElement.Attributes.Append(assemblyClassAttribute);
            taskElement.Attributes.Append(assemblyMethodAttribute);

            doc.AppendChild(taskElement);
        }

        #endregion
    }
}