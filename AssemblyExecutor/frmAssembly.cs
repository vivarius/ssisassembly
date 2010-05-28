using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;

namespace SSISAssemblyExecutor
{
    public partial class frmAssembly : Form
    {
        #region Private Properties
        private Assembly assembly;
        private TaskHost _taskHost;
        private Connections _connections;
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

            cmbConnection.SelectedIndex = cmbConnection.FindString(_taskHost.Properties["AssemblyConnector"].GetValue(_taskHost).ToString());
            GetAssemblyInfo(_taskHost.Properties["AssemblyPath"].GetValue(_taskHost).ToString());

            cmbNamespace.SelectedIndex = cmbNamespace.FindString(_taskHost.Properties["AssemblyNamespace"].GetValue(_taskHost).ToString());

            GetAssemblyClasses(cmbNamespace.Text);
            cmbClasses.SelectedIndex = cmbClasses.FindString(_taskHost.Properties["AssemblyClass"].GetValue(_taskHost).ToString());

            GetAssemblyMethods(cmbNamespace.Text, cmbClasses.Text);
            cmbMethod.SelectedIndex = cmbMethod.FindString(_taskHost.Properties["AssemblyMethod"].GetValue(_taskHost).ToString());

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            _taskHost.Properties["AssemblyConnector"].SetValue(_taskHost, cmbConnection.Text);
            _taskHost.Properties["AssemblyPath"].SetValue(_taskHost, Convert.ToString(_connections[cmbConnection.Text].ConnectionString, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyNamespace"].SetValue(_taskHost, Convert.ToString(cmbNamespace.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyClass"].SetValue(_taskHost, Convert.ToString(cmbClasses.Text, System.Globalization.CultureInfo.InvariantCulture));
            _taskHost.Properties["AssemblyMethod"].SetValue(_taskHost, Convert.ToString(cmbMethod.Text, System.Globalization.CultureInfo.InvariantCulture));
        }

        #endregion

        #region Methods
        public void GetAssemblyInfo(string filePath)
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
            var arrayList = new ArrayList();
            foreach (Type type in assembly.GetTypes().Where(type => !arrayList.Contains(type.Namespace)))
            {
                arrayList.Add(type.Namespace);
            }

            cmbNamespace.DataSource = arrayList;
            Cursor = Cursors.Arrow;
        }

        private void GetAssemblyClasses(string Namespace)
        {
            Cursor = Cursors.WaitCursor;
            var arrayList = new ArrayList();
            foreach (Type type in assembly.GetTypes().Where(type => Namespace == type.Namespace && type.IsClass && type.IsPublic && !type.IsAbstract && !type.IsInterface).Where(type => !arrayList.Contains(type.Name)))
            {
                arrayList.Add(type.Name);
            }

            cmbClasses.DataSource = arrayList;
            Cursor = Cursors.Arrow;
        }

        private void GetAssemblyMethods(string Namespace, string Classes)
        {
            Cursor = Cursors.WaitCursor;
            var arrayList = new ArrayList();
            foreach (MethodInfo[] methodName in
                from type in assembly.GetTypes()
                where Namespace == type.Namespace && type.Name == Classes
                select type.GetMethods())
            {
                foreach (MemberInfo method in ((MemberInfo[])methodName).Where(method => method.ReflectedType.IsPublic && !method.ReflectedType.IsGenericTypeDefinition && method.ReflectedType.IsVisible).Where(method => !arrayList.Contains(method.Name)))
                {
                    arrayList.Add(method.Name);
                }
                break;
            }

            cmbMethod.DataSource = arrayList;
            Cursor = Cursors.Arrow;
        }

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