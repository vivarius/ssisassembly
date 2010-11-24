using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;

namespace SSISExecuteAssemblyTask100
{
    class SSISExecuteAssemblyTaskUIInterface : IDtsTaskUI
    {
        #region Private Variables

        private TaskHost _taskHost;
        private Connections _connections;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SSISExecuteAssemblyTaskUIInterface"/> class.
        /// </summary>
        public SSISExecuteAssemblyTaskUIInterface()
        {
        }

        #endregion

        #region IDtsTaskUI Interface

        /// <summary>
        /// Associates a user interface with its task. Called by the client, which is usually the designer application.
        /// </summary>
        /// <param name="taskHost">The <see cref="T:Microsoft.SqlServer.Dts.Runtime.TaskHost"/> of the task.</param>
        /// <param name="serviceProvider">The IServiceProviderhttp://go.microsoft.com/fwlink/?LinkId=33994 interface provided by the designer.</param>
        public void Initialize(TaskHost taskHost, IServiceProvider serviceProvider)
        {
            _taskHost = taskHost;
            IDtsConnectionService cs = serviceProvider.GetService(typeof(IDtsConnectionService)) as IDtsConnectionService;
            _connections = cs.GetConnections();
        }

        /// <summary>
        /// Gets the view.
        /// </summary>
        /// <returns></returns>
        public ContainerControl GetView()
        {
            //Show the property window
            return new frmAssembly(_taskHost, _connections);
        }

        public void Delete(IWin32Window parentWindow)
        {
        }

        public void New(IWin32Window parentWindow)
        {
        }

        #endregion
    }
}