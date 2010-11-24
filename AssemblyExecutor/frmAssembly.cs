using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Controls;
using Microsoft.SqlServer.Dts.Runtime.Wrapper;
using TaskHost = Microsoft.SqlServer.Dts.Runtime.TaskHost;
using Variable = Microsoft.SqlServer.Dts.Runtime.Variable;
using VariableDispenser = Microsoft.SqlServer.Dts.Runtime.VariableDispenser;

namespace SSISExecuteAssemblyTask100
{
    public partial class frmAssembly : Form
    {
        private enum ParameterDirection
        {
            In, Out
        }

        #region Private Properties
        private Assembly _assembly;
        private readonly TaskHost _taskHost;
        private readonly Connections _connections;
        private readonly Dictionary<string, string> _paramsManager = new Dictionary<string, string>();
        private bool isFirstLoad = false;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        private Variables Variables
        {
            get { return _taskHost.Variables; }
        }

        /// <summary>
        /// Gets the connections.
        /// </summary>
        /// <value>The connections.</value>
        private Connections Connections
        {
            get { return _connections; }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="frmAssembly"/> class.
        /// </summary>
        /// <param name="taskHost">The task host.</param>
        /// <param name="connections">The connections.</param>
        public frmAssembly(TaskHost taskHost, Connections connections)
        {
            InitializeComponent();
            isFirstLoad = true;

            _taskHost = taskHost;
            _connections = connections;

            if (taskHost == null)
            {
                throw new ArgumentNullException("taskHost");
            }

            LoadFileConnections();

            try
            {
                if (_taskHost.Properties[NamedStringMembers.CONFIGURATION_TYPE].GetValue(_taskHost).ToString() != ConfigurationType.NO_CONFIGURATION)
                {
                    if (_taskHost.Properties[NamedStringMembers.CONFIGURATION_TYPE].GetValue(_taskHost).ToString() == ConfigurationType.FILE_CONNECTOR)
                    {
                        LoadConfigFileConnections();
                        optChooseConfigFileConnector.Checked = true;
                    }
                    else
                    {
                        cmbConfigurationFile.Items.AddRange(LoadVariables("System.String").ToArray());
                        optChooseVariable.Checked = true;
                    }

                    cmbConfigurationFile.Text = _taskHost.Properties[NamedStringMembers.CONFIGURATION_FILE].GetValue(_taskHost).ToString();

                    chkConfigFile.Checked = true;
                }
                else
                    chkConfigFile.Checked = false;
            }
            catch
            {
                chkConfigFile.Checked = false;
            }
            finally
            {
                EnableConfigurationControlFile();
            }

            if (_taskHost.Properties[NamedStringMembers.ASSEMBLY_CONNECTOR].GetValue(_taskHost) == null)
                return;

            cmbConnection.SelectedIndexChanged -= cmbConnection_SelectedIndexChanged;
            cmbNamespace.SelectedIndexChanged -= cmbNamespace_SelectedIndexChanged;
            cmbClasses.SelectedIndexChanged -= cmbClasses_SelectedIndexChanged;

            try
            {
                try
                {
                    var mappingParams = _taskHost.Properties[NamedStringMembers.MAPPING_PARAMS].GetValue(_taskHost).ToString().Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string mappingParam in mappingParams)
                    {
                        _paramsManager.Add(mappingParam.Split('|')[0], mappingParam.Split('|')[1]);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }

                cmbConnection.SelectedIndex = cmbConnection.FindString(_taskHost.Properties[NamedStringMembers.ASSEMBLY_CONNECTOR].GetValue(_taskHost).ToString());
                GetAssemblyInfo(_taskHost.Properties[NamedStringMembers.ASSEMBLY_PATH].GetValue(_taskHost).ToString());
                cmbNamespace.SelectedIndex = cmbNamespace.FindString(_taskHost.Properties[NamedStringMembers.ASSEMBLY_NAMESPACE].GetValue(_taskHost).ToString());
                GetAssemblyClasses(cmbNamespace.Text);
                cmbClasses.SelectedIndex = cmbClasses.FindString(_taskHost.Properties[NamedStringMembers.ASSEMBLY_CLASS].GetValue(_taskHost).ToString());
                GetAssemblyMethods(cmbNamespace.Text, cmbClasses.Text);

                SelectTheRightMethod();

                //cmbConfigurationFile.Text = _taskHost.Properties[NamedStringMembers.CONFIGURATION_FILE].GetValue(_taskHost).ToString();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            cmbConnection.SelectedIndexChanged += cmbConnection_SelectedIndexChanged;
            cmbNamespace.SelectedIndexChanged += cmbNamespace_SelectedIndexChanged;
            cmbClasses.SelectedIndexChanged += cmbClasses_SelectedIndexChanged;
        }
        #endregion

        #region Events

        private void cmbConnection_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAssemblyInfo(_connections[cmbConnection.SelectedItem].ConnectionString);
        }

        private void cmbNamespace_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAssemblyClasses(cmbNamespace.Text);
        }

        private void cmbClasses_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetAssemblyMethods(cmbNamespace.Text, cmbClasses.Text);
        }

        private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetMethodsParams((MethodInfo)(((ComboBoxObjectComboItem)((ComboBox)sender).SelectedItem).ValueMemeber));
        }

        private void grdParameters_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 3:
                    {
                        if ((grdParameters.Rows[e.RowIndex].Cells[1]).Value.ToString() != ParameterDirection.In.ToString("g"))
                        {
                            MessageBox.Show(@"You're not allowed to specify an expression for an OUT or REF type parameter", @"Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        using (var expressionBuilder = ExpressionBuilder.Instantiate(_taskHost.Variables, _taskHost.VariableDispenser, Type.GetType((grdParameters.Rows[e.RowIndex].Cells[0]).Value.ToString().Split('=')[1].Trim()), string.Empty))
                        {
                            if (expressionBuilder.ShowDialog() == DialogResult.OK)
                            {
                                ((DataGridViewComboBoxCell)grdParameters.Rows[e.RowIndex].Cells[e.ColumnIndex - 1]).Items.Add(expressionBuilder.Expression);
                                grdParameters.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value = expressionBuilder.Expression;
                            }
                        }
                    }

                    break;
            }
        }

        private void btConfigFile_Click(object sender, EventArgs e)
        {
            if (optChooseVariable.Checked)
                using (var expressionBuilder = ExpressionBuilder.Instantiate(_taskHost.Variables, _taskHost.VariableDispenser, Type.GetType("String"), cmbConfigurationFile.Text))
                {
                    if (expressionBuilder.ShowDialog() == DialogResult.OK)
                    {
                        cmbConfigurationFile.Text = expressionBuilder.Expression;
                    }
                }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //Save the values
            _taskHost.Properties[NamedStringMembers.ASSEMBLY_CONNECTOR].SetValue(_taskHost, cmbConnection.Text);
            _taskHost.Properties[NamedStringMembers.ASSEMBLY_PATH].SetValue(_taskHost, Convert.ToString(_connections[cmbConnection.Text].ConnectionString, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties[NamedStringMembers.ASSEMBLY_NAMESPACE].SetValue(_taskHost, Convert.ToString(cmbNamespace.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties[NamedStringMembers.ASSEMBLY_CLASS].SetValue(_taskHost, Convert.ToString(cmbClasses.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties[NamedStringMembers.ASSEMBLY_METHOD].SetValue(_taskHost, Convert.ToString(cmbMethod.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties[NamedStringMembers.CONFIGURATION_FILE].SetValue(_taskHost, chkConfigFile.Checked
                                                                                                    ? Convert.ToString(cmbConfigurationFile.Text, System.Globalization.CultureInfo.InvariantCulture)
                                                                                                    : string.Empty);
            _taskHost.Properties[NamedStringMembers.CONFIGURATION_TYPE].SetValue(_taskHost, chkConfigFile.Checked
                                                                                                    ? optChooseVariable.Checked
                                                                                                        ? ConfigurationType.TASK_VARIABLE
                                                                                                        : ConfigurationType.FILE_CONNECTOR
                                                                                                     : ConfigurationType.NO_CONFIGURATION);

            var stringBuilder = new StringBuilder();

            foreach (DataGridViewRow row in grdParameters.Rows)
            {
                stringBuilder.Append(row.Cells[0].Value + "|" + row.Cells[2].Value + "|" + row.Cells[1].Value + ";");
            }

            _taskHost.Properties[NamedStringMembers.MAPPING_PARAMS].SetValue(_taskHost, stringBuilder.ToString());
            _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].SetValue(_taskHost, cmbBoxReturnVariable.Text);
        }

        private void optChooseConfigFileConnector_Click(object sender, EventArgs e)
        {
            LoadConfigFileConnections();
        }

        private void optChooseVariable_Click(object sender, EventArgs e)
        {
            LoadVariablesForConfigFile();
        }

        private void chkConfigFile_CheckedChanged(object sender, EventArgs e)
        {
            EnableConfigurationControlFile();
        }
        #endregion

        #region Methods
        #region Assembly's specifics

        /// <summary>
        /// Selects the right method.
        /// </summary>
        private void SelectTheRightMethod()
        {
            int index = 0;

            foreach (var cmbItem in cmbMethod.Items)
            {
                if ((string)((ComboBoxObjectComboItem)cmbItem).DisplayMember == (string)_taskHost.Properties[NamedStringMembers.ASSEMBLY_METHOD].GetValue(_taskHost))
                {
                    Type type = _assembly.GetType(string.Format("{0}.{1}",
                                                                _taskHost.Properties[NamedStringMembers.ASSEMBLY_NAMESPACE].GetValue(_taskHost),
                                                                _taskHost.Properties[NamedStringMembers.ASSEMBLY_CLASS].GetValue(_taskHost)));

                    var paramTypes = new Type[_paramsManager.Count];

                    int counter = 0;
                    foreach (var parameter in _paramsManager)
                        paramTypes[counter++] = Type.GetType(parameter.Key.Split('=')[1].Trim());

                    MethodInfo methodInfo = type.GetMethod((string)((ComboBoxObjectComboItem)cmbItem).DisplayMember, paramTypes);

                    if (methodInfo != null)
                        if (methodInfo == ((ComboBoxObjectComboItem)cmbItem).ValueMemeber)
                        {
                            cmbMethod.SelectedIndex = index;
                            break;
                        }

                }
                index++;
            }
        }

        /// <summary>
        /// Gets the assembly info.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void GetAssemblyInfo(string filePath)
        {
            cmbNamespace.Items.Clear();
            cmbMethod.Items.Clear();
            cmbClasses.Items.Clear();

            try
            {
                _assembly = Assembly.LoadFrom(filePath);
                GetAssemblyNamespace();
            }
            catch (Exception e)
            {
                DialogResult = DialogResult.Ignore;
                MessageBox.Show(e.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Dispose();
                Close();
            }
        }

        /// <summary>
        /// Gets the assembly namespace.
        /// </summary>
        private void GetAssemblyNamespace()
        {
            Cursor = Cursors.WaitCursor;
            cmbNamespace.Items.Clear();
            cmbMethod.Items.Clear();
            cmbClasses.Items.Clear();
            grdParameters.Rows.Clear();

            var arrayList = new ArrayList();

            foreach (Type type in
                _assembly.GetTypes().Where(type => !arrayList.Contains(type.Namespace)).Where(type => type.Namespace != null))
            {
                arrayList.Add(type.Namespace);
                cmbNamespace.Items.Add(type.Namespace);
            }


            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Gets the assembly classes.
        /// </summary>
        /// <param name="namespace">The @namespace.</param>
        private void GetAssemblyClasses(string @namespace)
        {
            Cursor = Cursors.WaitCursor;

            cmbMethod.Items.Clear();
            cmbClasses.Items.Clear();
            grdParameters.Rows.Clear();

            var arrayList = new ArrayList();
            foreach (Type type in _assembly.GetTypes().Where(type => @namespace == type.Namespace && type.IsClass && type.IsPublic && !type.IsAbstract && !type.IsInterface).Where(type => !arrayList.Contains(type.Name)))
            {
                arrayList.Add(type.Name);
                cmbClasses.Items.Add(type.Name);
            }

            Cursor = Cursors.Arrow;
        }

        private void GetAssemblyMethods(string @namespace, string classes)
        {
            Cursor = Cursors.WaitCursor;

            cmbMethod.Items.Clear();
            grdParameters.Rows.Clear();

            //foreach (MethodInfo method in from type in _assembly.GetTypes()
            //                              where @namespace == type.Namespace && type.Name == classes
            //                              select type.GetMethods()
            //                                  into methodName
            //                                  from method in methodName
            //                                  where method.ReflectedType.IsPublic //&& !method.ReflectedType.IsGenericTypeDefinition && method.ReflectedType.IsVisible
            //                                  select method)
            //{
            //      cmbMethod.Items.Add(new ComboBoxObjectComboItem(method, method.Name));
            //}

            Type type = _assembly.GetTypes().First(t => t.Namespace == @namespace && t.Name == classes);

            foreach (var method in type.GetMethods().Where(t => t.IsPublic))
            {

                cmbMethod.Items.Add(new ComboBoxObjectComboItem(method, method.Name));
            }

            cmbMethod.DisplayMember = "DisplayMember";
            cmbMethod.ValueMember = "ValueMemeber";
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Gets the methods params.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        private void GetMethodsParams(MethodInfo methodInfo)
        {
            Cursor = Cursors.WaitCursor;
            grdParameters.Rows.Clear();

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                int index = grdParameters.Rows.Add();

                DataGridViewRow row = grdParameters.Rows[index];

                row.Cells["grdColParams"] = new DataGridViewTextBoxCell
                                                {
                                                    Value = parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName,
                                                    Tag = parameterInfo.ParameterType.FullName
                                                };

                row.Cells["grdColDirection"] = new DataGridViewTextBoxCell
                                                   {
                                                       Value = (!parameterInfo.ParameterType.IsByRef)
                                                                   ? ParameterDirection.In.ToString("g")
                                                                   : ParameterDirection.Out.ToString("g")
                                                   };

                row.Cells["grdColVars"] = LoadVariables(parameterInfo);

                row.Cells["grdColExpression"] = new DataGridViewButtonCell();

            }

            string selectedText = string.Empty;
            cmbBoxReturnVariable.DataSource = LoadVariables(methodInfo.ReturnParameter, ref selectedText).Items;
            cmbBoxReturnVariable.Text = selectedText;

            Cursor = Cursors.Arrow;
        }

        #endregion

        /// <summary>
        /// Enables the configuration control file.
        /// </summary>
        private void EnableConfigurationControlFile()
        {
            if (chkConfigFile.Checked)
            {
                EnbaleConfigurationFileControls(true);
                if (optChooseConfigFileConnector.Checked)
                    LoadConfigFileConnections();
                else
                    LoadVariablesForConfigFile();

            }
            else
            {
                EnbaleConfigurationFileControls(false);
            }
        }

        /// <summary>
        /// Loads the variables for config file.
        /// </summary>
        private void LoadVariablesForConfigFile()
        {
            cmbConfigurationFile.Items.Clear();
            cmbConfigurationFile.Items.AddRange(LoadVariables("System.String").ToArray());
        }

        /// <summary>
        /// Enbales the configuration file controls.
        /// </summary>
        /// <param name="bEnable">if set to <c>true</c> [b enable].</param>
        private void EnbaleConfigurationFileControls(bool bEnable)
        {
            optChooseVariable.Enabled = bEnable;
            optChooseConfigFileConnector.Enabled = bEnable;
            btConfigFileExpression.Enabled = bEnable;
            cmbConfigurationFile.Enabled = bEnable;
        }

        #region Variable Handlers

        /// <summary>
        /// Loads the variables.
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <returns></returns>
        private DataGridViewComboBoxCell LoadVariables(ParameterInfo parameterInfo)
        {
            var comboBoxCell = new DataGridViewComboBoxCell();

            foreach (Variable variable in Variables)
            {
                if (parameterInfo.ParameterType.IsByRef && variable.DataType == TypeCode.Object ||
                    Type.GetTypeCode(Type.GetType(parameterInfo.ParameterType.FullName)) == variable.DataType)
                {
                    comboBoxCell.Items.Add(string.Format("@[{0}::{1}]", variable.Namespace, variable.Name));
                }
            }

            if (isFirstLoad && _paramsManager != null && _paramsManager.Count > 0)
            {
                if (!comboBoxCell.Items.Contains(_paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName]))
                    comboBoxCell.Items.Add(_paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName]);

                comboBoxCell.Value = _paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName];
            }

            return comboBoxCell;
        }

        /// <summary>
        /// Loads the variables.
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <param name="selectedText">The selected text.</param>
        /// <returns></returns>
        private ComboBox LoadVariables(ParameterInfo parameterInfo, ref string selectedText)
        {
            return LoadVariables(parameterInfo.ParameterType.FullName, ref selectedText);
        }

        /// <summary>
        /// Loads the variables.
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <param name="selectedText">The selected text.</param>
        /// <returns></returns>
        private ComboBox LoadVariables(string parameterInfo, ref string selectedText)
        {
            var comboBox = new ComboBox();

            foreach (Variable variable in Variables.Cast<Variable>().Where(variable => Type.GetTypeCode(Type.GetType(parameterInfo)) == variable.DataType))
            {
                comboBox.Items.Add(string.Format("@[{0}::{1}]", variable.Namespace, variable.Name));
            }

            if (isFirstLoad && _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE] != null && _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].GetValue(_taskHost) != null)
            {
                selectedText = _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].GetValue(_taskHost).ToString();
                isFirstLoad = false;
            }

            return comboBox;
        }

        /// <summary>
        /// Loads the variables.
        /// </summary>
        /// <param name="parameterInfo">The parameter info.</param>
        /// <returns></returns>
        private List<string> LoadVariables(string parameterInfo)
        {
            return Variables.Cast<Variable>().Where(variable => Type.GetTypeCode(Type.GetType(parameterInfo)) == variable.DataType).Select(variable => string.Format("@[{0}::{1}]", variable.Namespace, variable.Name)).ToList();
        }

        #endregion

        #region Load File Connections

        /// <summary>
        /// Loads the file connections.
        /// </summary>
        private void LoadFileConnections()
        {
            cmbConnection.Items.Clear();

            foreach (var connection in Connections)
            {
                cmbConnection.Items.Add(connection.Name);
            }
        }

        /// <summary>
        /// Loads the config file connections.
        /// </summary>
        private void LoadConfigFileConnections()
        {
            cmbConfigurationFile.Items.Clear();
            foreach (var connection in Connections)
            {
                cmbConfigurationFile.Items.Add(connection.Name);
            }
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
            object variableObject;

            var expressionEvaluatorClass = new ExpressionEvaluatorClass
            {
                Expression = mappedParam
            };

            expressionEvaluatorClass.Evaluate(DtsConvert.GetExtendedInterface(variableDispenser), out variableObject, false);
            return variableObject;
        }

        #endregion

        #endregion
    }
}