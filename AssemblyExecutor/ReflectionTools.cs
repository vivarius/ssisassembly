using System;
using System.IO;
using System.Reflection;

namespace SSISExecuteAssemblyTask100
{
    public static class ReflectionTools
    {
        /// <summary>
        /// Creates the name of the instance from assembly qualified.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        internal static object CreateInstanceFromAssemblyQualifiedName(string typeName)
        {
            Type type = Type.GetType(typeName);
            return type == null
                    ? (null)
                    : Assembly.GetAssembly(type).CreateInstance(type.ToString());
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        internal static object CreateInstance(Type type)
        {
            return Assembly.GetAssembly(type).CreateInstance(type.ToString());
        }

        /// <summary>
        /// Creates the instance.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        internal static object CreateInstance(Assembly assembly, Type type)
        {
            return assembly.CreateInstance(type.ToString());
        }

        /// <summary>
        /// Gets the name of the type from.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        private static Type GetTypeFromName(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
                return type;

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(typeName, false);
                if (type != null)
                    break;
            }

            return type;
        }

        /// <summary>
        /// Gets the name of the type from.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        internal static Type GetTypeFromName(Assembly assembly, string typeName)
        {
            Type type = Type.GetType(typeName);
            return type ?? assembly.GetType(typeName);
        }

        /// <summary>
        /// Creates the name of the instance from type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        internal static object CreateInstanceFromTypeName(string typeName, params object[] args)
        {
            object instance;

            try
            {
                Type type = GetTypeFromName(typeName);
                if (type == null)
                    return null;

                instance = Activator.CreateInstance(type, args);
            }
            catch
            {
                return null;
            }
            return instance;
        }

        /// <summary>
        /// Gets the embedded resource.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="resourceName">Name of the resource.</param>
        /// <returns></returns>
        internal static Stream GetEmbeddedResource(string assemblyName, string resourceName)
        {
            return Assembly.Load(assemblyName).GetManifestResourceStream(resourceName);
        }
    }
}