using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks
{
    public class ResolveSitecoreAssemblies : Task
    {
        [Required]
        public ITaskItem[] AssemblyLists { get; set; }
        
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        [Required]
        public bool VerifyAssemblies { get; set; }

        [Output]
        public ITaskItem[] SitecoreAssemblies { get; set; }

        public override bool Execute()
        {
            if (AssemblyLists == null || AssemblyLists.Length == 0)
            {
                Log.LogError("No assembly lists were supplied");
                return false;
            }

            if (Assemblies == null || Assemblies.Length == 0)
            {
                return true;
            }

            var outputFilesSet = new HashSet<string>();
            var outputItems = new List<ITaskItem>();

            try
            {
                var assemblyLists = AssemblyLists.Select(al => al.GetMetadata("FullPath")).Select(AssemblyList.Parse).ToList();
                var assemblyLookup = CreateAssemblyLookup(Assemblies);

                foreach (var assemblyList in assemblyLists)
                {
                    foreach (var assemblyListEntry in assemblyList)
                    {
                        if (!assemblyLookup.TryGetValue(assemblyListEntry.AssemblyInfo.Filename, out var matchingAssembly))
                        {
                            // Missing assembly
                            continue;
                        }

                        if (VerifyAssemblies)
                        {
                            var actualAssemblyInfo = AssemblyInfo.FromAssembly(matchingAssembly, true);

                            VerifyAssemblyInfo(assemblyListEntry, actualAssemblyInfo);
                        }

                        if (outputFilesSet.Add(matchingAssembly))
                        {
                            outputItems.Add(new TaskItem(matchingAssembly));
                        }
                    }
                }

                SitecoreAssemblies = outputItems.ToArray();

                return true;
            }
            catch (AssemblyListParseException ex)
            {
                Log.LogErrorFromException(ex, false, true, ex.SourceFile);
                return false;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }

        private void VerifyAssemblyInfo(AssemblyListEntry assemblyListEntry, AssemblyInfo actualAssemblyInfo)
        {
            var expectedAssemblyInfo = assemblyListEntry.AssemblyInfo;

            if (expectedAssemblyInfo.AssemblyVersion != actualAssemblyInfo.AssemblyVersion)
            {
                Log.LogWarning($"Sitecore assembly {expectedAssemblyInfo.Filename} version mismatch. Expected {expectedAssemblyInfo.AssemblyVersion} but found {actualAssemblyInfo.AssemblyVersion}");
                return;
            }

            if (actualAssemblyInfo.FileVersion != null && expectedAssemblyInfo.FileVersion != actualAssemblyInfo.FileVersion)
            {
                Log.LogWarning($"Sitecore assembly {expectedAssemblyInfo.Filename} file version mismatch. Expected {expectedAssemblyInfo.FileVersion} but found {actualAssemblyInfo.FileVersion}");
                return;
            }
        }

        private IDictionary<string, string> CreateAssemblyLookup(ITaskItem[] assemblies)
        {
            return assemblies.ToDictionary(
                item => string.Concat(item.GetMetadata("Filename"), item.GetMetadata("Extension")),
                item => item.GetMetadata("FullPath"),
                StringComparer.OrdinalIgnoreCase
                );
        }
    }
}
