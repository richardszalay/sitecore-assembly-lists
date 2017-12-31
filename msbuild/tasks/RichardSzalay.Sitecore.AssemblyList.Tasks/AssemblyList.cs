using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks
{
    public class AssemblyList
    {
        public static ICollection<AssemblyListEntry> Parse(string file)
        {
            using (var reader = File.OpenText(file))
            {
                return Parse(reader, file);
            }
        }

        public static ICollection<AssemblyListEntry> Parse(TextReader reader, string sourceFileName)
        {
            SkipHeaderLine(reader);

            return ParseEntries(reader, sourceFileName).ToList();
        }

        static IEnumerable<AssemblyListEntry> ParseEntries(TextReader reader, string sourceFileName)
        {
            string rawEntryLine = reader.ReadLine();
            int lineNumber = 2; // Skip header

            while (rawEntryLine != null)
            {
                AssemblyInfo assemblyInfo;

                try
                {
                    assemblyInfo = ParseEntry(rawEntryLine, sourceFileName, lineNumber);
                }
                catch(Exception ex)
                {
                    throw new AssemblyListParseException("Failed to parse assembly list entry", sourceFileName, lineNumber, ex);
                }

                yield return new AssemblyListEntry
                {
                    AssemblyInfo = assemblyInfo,
                    SourceFile = sourceFileName,
                    SourceLine = lineNumber
                };

                rawEntryLine = reader.ReadLine();
                lineNumber++;
            }
        }

        private static AssemblyInfo ParseEntry(string line, string sourceFile, int sourceLine)
        {
            var parts = line.Split(',');

            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid assembly list entry format");
            }

            string filename = parts[0];
            string fileVersionString = parts[1];
            string assemblyVersionString = parts[2];

            if (!Version.TryParse(fileVersionString, out var fileVersion))
            {
                throw new ArgumentException("Unable to parse file version");
            }

            if (!Version.TryParse(assemblyVersionString, out var assemblyVersion))
            {
                throw new ArgumentException("Unable to parse assembly version");
            }

            return new AssemblyInfo
            {
                Filename = filename,
                FileVersion = AssemblyInfo.NormalizeVersion(fileVersion),
                AssemblyVersion = AssemblyInfo.NormalizeVersion(assemblyVersion)
            };
        }

        static void SkipHeaderLine(TextReader reader)
        {
            reader.ReadLine();
        }
    }

    public class AssemblyListEntry
    {
        public AssemblyInfo AssemblyInfo { get; set; }
        public int SourceLine { get; set; }
        public string SourceFile { get; set; }
    }
}
