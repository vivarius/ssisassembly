using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.DataTransformationServices.Controls;

namespace SSISAssemblyExecutor
{
    public partial class frmAssembly : Form
    {
        #region Private Properties
        private Assembly assembly;
        private TaskHost _taskHost;
        private Connections _connections;
        private Dictionary<string, string> paramsManager = new Dictionary<string, string>();
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

        public frmAssembly(string FilePath)
        {
            InitializeComponent();

            grdParameters.DataError += grdParameters_DataError;

            GetAssemblyInfo(FilePath);
        }  

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


            if (_taskHost.Properties["AssemblyConnector"].GetValue(_taskHost) == null)
                return;

            cmbConnection.SelectedIndexChanged -= cmbConnection_SelectedIndexChanged;
            cmbNamespace.SelectedIndexChanged -= cmbNamespace_SelectedIndexChanged;
            cmbClasses.SelectedIndexChanged -= cmbClasses_SelectedIndexChanged;

            try
            {
                try
                {
                    var mappingParams = _taskHost.Properties["MappingParams"].GetValue(_taskHost).ToString().Split(';');

                    foreach (string mappingParam in mappingParams)
                    {
                        paramsManager.Add(mappingParam.Split('|')[0], mappingParam.Split('|')[1]);
                    }
                }
                catch
                {
                    //it will continue
                }

                cmbConnection.SelectedIndex = cmbConnection.FindString(_taskHost.Properties["AssemblyConnector"].GetValue(_taskHost).ToString());
                GetAssemblyInfo(_taskHost.Properties["AssemblyPath"].GetValue(_taskHost).ToString());
                cmbNamespace.SelectedIndex = cmbNamespace.FindString(_taskHost.Properties["AssemblyNamespace"].GetValue(_taskHost).ToString());
                GetAssemblyClasses(cmbNamespace.Text);
                cmbClasses.SelectedIndex = cmbClasses.FindString(_taskHost.Properties["AssemblyClass"].GetValue(_taskHost).ToString());
                GetAssemblyMethods(cmbNamespace.Text, cmbClasses.Text);
                cmbMethod.SelectedIndex = cmbMethod.FindString(_taskHost.Properties["AssemblyMethod"].GetValue(_taskHost).ToString());
            }
            catch
            {
                //it will continue
            }

            cmbConnection.SelectedIndexChanged += cmbConnection_SelectedIndexChanged;
            cmbNamespace.SelectedIndexChanged += cmbNamespace_SelectedIndexChanged;
            cmbClasses.SelectedIndexChanged += cmbClasses_SelectedIndexChanged;
        }
        #endregion

        #region Events

        void grdParameters_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }

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
            GetMethodsParams((MethodInfo)(((ComboItem)((ComboBox)sender).SelectedItem).BindingValue));
        }

        private void grdParameters_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.ColumnIndex)
            {
                case 2:
                    {
                        var expressionBuilder = ExpressionBuilder.Instantiate(_taskHost.Variables,
                                                                                            _taskHost.VariableDispenser,
                                                                                            Type.GetType((grdParameters.Rows[e.RowIndex].Cells[e.ColumnIndex - 2]).Value.ToString().Split('=')[1].Trim()),
                                                                                            string.Empty);

                        if (expressionBuilder.ShowDialog() == DialogResult.OK)
                        {
                            ((DataGridViewComboBoxCell)grdParameters.Rows[e.RowIndex].Cells[e.ColumnIndex - 1]).Items.Add(expressionBuilder.Expression);
                            ((DataGridViewComboBoxCell)grdParameters.Rows[e.RowIndex].Cells[e.ColumnIndex - 1]).Value = expressionBuilder.Expression;
                        }
                    }
                    break;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _taskHost.Properties["AssemblyConnector"].SetValue(_taskHost, cmbConnection.Text);
            _taskHost.Properties["AssemblyPath"].SetValue(_taskHost, Convert.ToString(_connections[cmbConnection.Text].ConnectionString, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyNamespace"].SetValue(_taskHost, Convert.ToString(cmbNamespace.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyClass"].SetValue(_taskHost, Convert.ToString(cmbClasses.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyMethod"].SetValue(_taskHost, Convert.ToString(cmbMethod.Text, System.Globalization.CultureInfo.InvariantCulture));

            StringBuilder stringBuilder = new StringBuilder();

            foreach (DataGridViewRow row in grdParameters.Rows)
            {
                stringBuilder.Append(row.Cells[0].Value + "|" + row.Cells[1].Value + ";");
            }

            _taskHost.Properties["MappingParams"].SetValue(_taskHost, stringBuilder.ToString());
            _taskHost.Properties["OutPutVariable"].SetValue(_taskHost, cmbBoxReturnVariable.Text);
        }

        #endregion

        #region Methods

        //private string ParseVariable(string Variable)
        //{
        //    //@[System::CreatorComputerName]
        //    var strSecond = Variable.Split("::")[1];
        //    return strSecond.Substring(0, strSecond.Length - 1);
        //}

        private void GetAssemblyInfo(string filePath)
        {
            cmbNamespace.Items.Clear();
            cmbMethod.Items.Clear();
            cmbClasses.Items.Clear();

            try
            {
                assembly = Assembly.LoadFrom(filePath);
                GetAssemblyNamespace();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                assembly.GetTypes().Where(type => !arrayList.Contains(type.Namespace)).Where(type => type.Namespace != null))
            {
                arrayList.Add(type.Namespace);
                cmbNamespace.Items.Add(type.Namespace);
            }


            Cursor = Cursors.Arrow;
        }

        private void GetAssemblyClasses(string Namespace)
        {
            Cursor = Cursors.WaitCursor;

            cmbMethod.Items.Clear();
            cmbClasses.Items.Clear();
            grdParameters.Rows.Clear();

            var arrayList = new ArrayList();
            foreach (Type type in assembly.GetTypes().Where(type => Namespace == type.Namespace && type.IsClass && type.IsPublic && !type.IsAbstract && !type.IsInterface).Where(type => !arrayList.Contains(type.Name)))
            {
                arrayList.Add(type.Name);
                cmbClasses.Items.Add(type.Name);
            }

            Cursor = Cursors.Arrow;
        }

        private void GetAssemblyMethods(string Namespace, string Classes)
        {
            Cursor = Cursors.WaitCursor;

            cmbMethod.Items.Clear();
            grdParameters.Rows.Clear();
            foreach (MethodInfo method in from type in assembly.GetTypes()
                                          where Namespace == type.Namespace && type.Name == Classes
                                          select type.GetMethods()
                                              into methodName
                                              from method in methodName
                                              where method.ReflectedType.IsPublic && !method.ReflectedType.IsGenericTypeDefinition && method.ReflectedType.IsVisible
                                              select method)
            {
                cmbMethod.Items.Add(new ComboItem(method, method.Name));
            }


            cmbMethod.DisplayMember = "DisplayValue";
            cmbMethod.ValueMember = "BindingValue";
            Cursor = Cursors.Arrow;
        }

        private void GetMethodsParams(MethodInfo methodInfo)
        {
            Cursor = Cursors.WaitCursor;
            grdParameters.Rows.Clear();

            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                var row = new DataGridViewRow();

                grdParameters.Rows.Add(row);
                row.Cells["grdColParams"] = new DataGridViewTextBoxCell
                                                {
                                                    Value = parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName,
                                                    Tag = parameterInfo.ParameterType.FullName
                                                };

                row.Cells["grdColVars"] = LoadVariables(parameterInfo);

                row.Cells["grdColExpression"] = new DataGridViewButtonCell();
            }

            string selectedText = string.Empty;
            cmbBoxReturnVariable.DataSource = LoadOutPutVariables(methodInfo.ReturnParameter, ref selectedText).Items;
            cmbBoxReturnVariable.Text = selectedText;

            Cursor = Cursors.Arrow;
        }

        private void LoadFileConnections()
        {
            foreach (var connection in Connections)
            {
                cmbConnection.Items.Add(connection.Name);
            }
        }

        private DataGridViewComboBoxCell LoadVariables(ParameterInfo parameterInfo)
        {
            var comboBoxCell = new DataGridViewComboBoxCell();

            foreach (Variable variable in
                Variables.Cast<Variable>().Where(variable => Type.GetTypeCode(Type.GetType(parameterInfo.ParameterType.FullName)) == variable.DataType))
            {
                comboBoxCell.Items.Add(variable.Name);
            }

            if (isFirstLoad && paramsManager != null && paramsManager.Count > 0)
            {
                if (!comboBoxCell.Items.Contains(paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName]))
                    comboBoxCell.Items.Add(paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName]);

                comboBoxCell.Value = paramsManager[parameterInfo.Name + " = " + parameterInfo.ParameterType.FullName];
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

            if (isFirstLoad && _taskHost.Properties["OutPutVariable"] != null && _taskHost.Properties["OutPutVariable"].GetValue(_taskHost) != null)
            {
                selectedText = _taskHost.Properties["OutPutVariable"].GetValue(_taskHost).ToString();
                isFirstLoad = false;
            }

            return comboBox;
        }

        #endregion
    }
}


//public void LoadPredefinedValues(string Namespace, string Class, string Method)
//{
//    cmbNamespace.SelectedItem = Namespace;
//    cmbClasses.SelectedItem = Class;
//    cmbMethod.SelectedItem = Method;
//}

//private void LoadVariablesLists()
//{
//    foreach (var variable in
//        Variables.Cast<Variable>().Where(variable => variable.DataType == TypeCode.String && !variable.SystemVariable))
//    {
//        cmbNamespaceVariable.Items.Add(variable.Name);
//        cmbClassVariable.Items.Add(variable.Name);
//        cmbMethodVariable.Items.Add(variable.Name);
//    }
//}
//private bool IsParamAppID(ParameterInfo[] parameterInfos)
//        {
//            if (parameterInfos != null)
//                return (parameterInfos.Length == 0)
//                           ? true
//                           : (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(string))
//                                 ? true
//                                 : false;
//            return true;
//        }