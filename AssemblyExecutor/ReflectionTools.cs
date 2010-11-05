using System;
using System.IO;
using System.Reflection;

namespace SSISExecuteAssemblyTask100.SSIS
{
    public static class ReflectionTools
    {
        internal static object CreateInstanceFromAssemblyQualifiedName(string typeName)
        {
            Type type = Type.GetType(typeName);
            return type == null
                    ? (null)
                    : Assembly.GetAssembly(type).CreateInstance(type.ToString());
        }

        internal static object CreateInstance(Type type)
        {
            return Assembly.GetAssembly(type).CreateInstance(type.ToString());
        }

        internal static object CreateInstance(Assembly assembly, Type type)
        {
            return assembly.CreateInstance(type.ToString());
        }

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

        internal static Type GetTypeFromName(Assembly assembly, string typeName)
        {
            Type type = Type.GetType(typeName);
            return type ?? assembly.GetType(typeName);
        }

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

        internal static Stream GetEmbeddedResource(string assemblyName, string resourceName)
        {
            return Assembly.Load(assemblyName).GetManifestResourceStream(resourceName);
        }
    }
}