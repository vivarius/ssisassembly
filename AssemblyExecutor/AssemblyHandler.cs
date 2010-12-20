using System;
using System.Reflection;

namespace SSISExecuteAssemblyTask100
{
    [Serializable]
    class AssemblyHandler : MarshalByRefObject
    {
        #region Private Fields

        private Assembly _assembly;
        private MethodInfo _methodInfo;

        #endregion

        #region Overrided Methods
        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease"/> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime"/> property.
        /// </returns>
        /// <exception cref="T:System.Security.SecurityException">
        /// The immediate caller does not have infrastructure permission.
        /// </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure"/>
        /// </PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Load an assembly from the specified string path
        /// </summary>
        /// <param name="path">the Path to the assembly</param>
        public void LoadAssembly(string path)
        {
            _assembly = Assembly.Load(AssemblyName.GetAssemblyName(path));
        }

        /// <summary>
        /// Execute a Method from a specified assembly
        /// </summary>
        /// <param name="typeName">Type as string</param>
        /// <param name="methodName">Name of the method to execute</param>
        /// <param name="parameters">An array of parameters</param>
        /// <returns></returns>
        public object ExecuteMethod(string typeName, string methodName, params object[] parameters)
        {
            object retObject;

            try
            {
                Type type = _assembly.GetType(typeName);

                var paramTypes = new Type[parameters.Length];

                int index = 0;
                foreach (var parameter in parameters)
                {
                    paramTypes[index] = parameter.GetType();
                    index++;
                }

                _methodInfo = type.GetMethod(methodName, paramTypes);

                if (_methodInfo.IsStatic)
                {
                    retObject = _methodInfo.Invoke(null, parameters);
                }
                else
                {
                    var instanceObject = _assembly.CreateInstance(type.ToString());
                    retObject = _methodInfo.Invoke(instanceObject, parameters);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message + " " + exception.StackTrace);
            }

            return retObject;
        }

        #endregion
    }
}