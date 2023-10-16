

// ReSharper disable CheckNamespace
using System.Reflection;

namespace Library
{
    public static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetTypesSafe(this Assembly assembly)
        {
            //Ignore types that have a missing referenced assembly
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types!;
            }

            return types.Where(t => t != null);
        }

        public static IEnumerable<Type> GetAllDerivedTypes<T>(this Assembly assembly) => assembly.GetTypesSafe().Where(t => t.IsAbstract == false && t.IsSubclassOf(typeof(T)));

        public static bool IsCustomType(this Type type) => type.IsPrimitive == false && type.IsEnum == false && type != typeof(string) && type != typeof(decimal);
    }
}
