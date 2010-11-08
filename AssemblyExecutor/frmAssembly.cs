﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Controls;

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
        private Variables Variables
        {
            get { return _taskHost.Variables; }
        }

        private Connections Connections
        {
            get { return _connections; }
        }

        #endregion

        #region ctor

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

                txConfigurationFile.Text = _taskHost.Properties[NamedStringMembers.CONFIGURATION_FILE].GetValue(_taskHost).ToString();
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
            using (var openFileDialog = new OpenFileDialog
            {
                Filter = @"*.config|*.config",
                InitialDirectory = Path.GetDirectoryName(_connections[cmbConnection.Text].ConnectionString)
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txConfigurationFile.Text = openFileDialog.FileName;
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
            _taskHost.Properties[NamedStringMembers.CONFIGURATION_FILE].SetValue(_taskHost, Convert.ToString(txConfigurationFile.Text, System.Globalization.CultureInfo.InvariantCulture));

            var stringBuilder = new StringBuilder();

            foreach (DataGridViewRow row in grdParameters.Rows)
            {
                stringBuilder.Append(row.Cells[0].Value + "|" + row.Cells[2].Value + "|" + row.Cells[1].Value + ";");
            }

            _taskHost.Properties[NamedStringMembers.MAPPING_PARAMS].SetValue(_taskHost, stringBuilder.ToString());
            _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].SetValue(_taskHost, cmbBoxReturnVariable.Text);
        }

        #endregion

        #region Methods

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
            foreach (MethodInfo method in from type in _assembly.GetTypes()
                                          where @namespace == type.Namespace && type.Name == classes
                                          select type.GetMethods()
                                              into methodName
                                              from method in methodName
                                              where method.ReflectedType.IsPublic && !method.ReflectedType.IsGenericTypeDefinition && method.ReflectedType.IsVisible
                                              select method)
            {
                cmbMethod.Items.Add(new ComboBoxObjectComboItem(method, method.Name));
            }


            cmbMethod.DisplayMember = "DisplayMember";
            cmbMethod.ValueMember = "ValueMemeber";
            Cursor = Cursors.Arrow;
        }

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
            cmbBoxReturnVariable.DataSource = LoadOutPutVariables(methodInfo.ReturnParameter, ref selectedText).Items;
            cmbBoxReturnVariable.Text = selectedText;

            Cursor = Cursors.Arrow;
        }

        private DataGridViewComboBoxCell LoadVariables(ParameterInfo parameterInfo)
        {
            var comboBoxCell = new DataGridViewComboBoxCell();

            foreach (Variable variable in Variables)
            {
                if (parameterInfo.ParameterType.IsByRef && variable.DataType == TypeCode.Object
                 || Type.GetTypeCode(Type.GetType(parameterInfo.ParameterType.FullName)) == variable.DataType)
                {
                    comboBoxCell.Items.Add(variable.Name);
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

        private ComboBox LoadOutPutVariables(ParameterInfo parameterInfo, ref string selectedText)
        {
            var comboBox = new ComboBox();

            foreach (Variable variable in
                Variables.Cast<Variable>().Where(variable => Type.GetTypeCode(Type.GetType(parameterInfo.ParameterType.FullName)) == variable.DataType))
            {
                comboBox.Items.Add(variable.Name);
            }

            if (isFirstLoad && _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE] != null && _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].GetValue(_taskHost) != null)
            {
                selectedText = _taskHost.Properties[NamedStringMembers.OUTPUT_VARIABLE].GetValue(_taskHost).ToString();
                isFirstLoad = false;
            }

            return comboBox;
        }

        //Load defined connection
        private void LoadFileConnections()
        {
            foreach (var connection in Connections)
            {
                cmbConnection.Items.Add(connection.Name);
            }
        }

        #endregion
    }
}