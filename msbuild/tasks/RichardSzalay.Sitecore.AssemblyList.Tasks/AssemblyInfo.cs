using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks
{
    public class AssemblyInfo
    {
        public string Filename { get; set; }
        public Version FileVersion { get; set; }
        public Version AssemblyVersion { get; set; }

        public static AssemblyInfo FromAssembly(string assemblyFile, bool includeFileVersion)
        {
            if (includeFileVersion)
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

                return new AssemblyInfo
                {
                    Filename = Path.GetFileName(assemblyFile),
                    AssemblyVersion = assembly.GetName().Version,
                    FileVersion = GetAssemblyFileVersion(assembly)
                };
            }

            var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);

            return new AssemblyInfo
            {
                Filename = Path.GetFileName(assemblyFile),
                AssemblyVersion = assemblyName.Version
            };
        }

        private static Version GetAssemblyFileVersion(Assembly assembly)
        {
            var fileVersionString = assembly.CustomAttributes
                .Where(x => x.AttributeType.FullName == "System.Reflection.AssemblyFileVersionAttribute")
                .Select(x => x.ConstructorArguments[0].Value as string)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(fileVersionString))
            {
                return null;
            }

            var fileVersion = Version.Parse(fileVersionString);

            return NormalizeVersion(fileVersion);
        }

        public static Version NormalizeVersion(Version version)
        {
            return new Version(
                version.Major == -1 ? 0 : version.Major,
                version.Minor == -1 ? 0 : version.Minor,
                version.Build == -1 ? 0 : version.Build,
                version.Revision == -1 ? 0 : version.Revision
                );
        }
    }
}
