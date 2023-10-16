using System.Text.RegularExpressions;

namespace Library
{
    public class EmbeddedResource
    {
        private static Regex AssemblyNameRegex = new Regex(@"^(.*)\.Resources\..*$");
        public static string GetAsString(string path)
        {
            var assemblyName = AssemblyNameRegex.Match(path).Groups[1].Value;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly == null)
                throw new ArgumentException($"Could not find embedded resource assembly '{assemblyName}'.  The embedded resource should be in a top level folder called 'Resources'.  The assembly is derived from the resource path which is '{path}'.");

            using var resourceStream = assembly.GetManifestResourceStream(path);
            if (resourceStream == null)
            {
                return string.Empty;
            }
            using var streamReader = new StreamReader(resourceStream);
            return streamReader.ReadToEnd();
        }
    }
}
