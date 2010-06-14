using System;
using System.IO;
using System.Reflection;

namespace SSISAssemblyExecutor
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

        private static Type GetTypeFromName(string TypeName)
        {
            Type type = Type.GetType(TypeName);
            if (type != null)
                return type;

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = ass.GetType(TypeName, false);
                if (type != null)
                    break;
            }
            return type;
        }

        internal static Type GetTypeFromName(Assembly assembly, string TypeName)
        {
            Type type = Type.GetType(TypeName);
            return type ?? assembly.GetType(TypeName, false);
        }

        internal static object CreateInstanceFromTypeName(string typeName, params object[] args)
        {
            object instance = null;

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