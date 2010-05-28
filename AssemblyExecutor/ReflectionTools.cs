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
            if (type == null)
                return (null);
            Assembly asm = Assembly.GetAssembly(type);
            object obj = asm.CreateInstance(type.ToString());
            return (obj);
        }

        internal static object CreateInstance(Type type)
        {
            Assembly asm = Assembly.GetAssembly(type);
            object obj = asm.CreateInstance(type.ToString());
            return (obj);
        }

        internal static object CreateInstance(Assembly assembly, Type type)
        {
            object obj = assembly.CreateInstance(type.ToString());
            return (obj);
        }

        internal static Type GetTypeFromName(string TypeName)
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
            if (type != null)
                return type;

            type = assembly.GetType(TypeName, false);

            return type;
        }

        internal static object CreateInstanceFromTypeName(string typeName, params object[] args)
        {
            object instance = null;
            Type type = null;

            try
            {
                type = GetTypeFromName(typeName);
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
            Assembly asm = Assembly.Load(assemblyName);
            return (asm.GetManifestResourceStream(resourceName));
        }
    }
}