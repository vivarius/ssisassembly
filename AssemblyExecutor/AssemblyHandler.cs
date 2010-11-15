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