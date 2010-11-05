using System;
using System.Windows.Forms;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Runtime.Design;
using SSISExecuteAssemblyTask100.SSIS;

namespace SSISExecuteAssemblyTask100
{
    class SSISExecAssemblyUIInterface : IDtsTaskUI
    {
        #region Private Variables

        private TaskHost _taskHost;
        private Connections _connections;
        #endregion

        #region Constructor
        public SSISExecAssemblyUIInterface()
        {
        }

        #endregion

        #region IDtsTaskUI Interface

        public void Initialize(TaskHost taskHost, IServiceProvider serviceProvider)
        {
            _taskHost = taskHost;
            IDtsConnectionService cs = serviceProvider.GetService(typeof (IDtsConnectionService)) as IDtsConnectionService;
            _connections = cs.GetConnections();
        }

        /// <summary>
        /// 
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