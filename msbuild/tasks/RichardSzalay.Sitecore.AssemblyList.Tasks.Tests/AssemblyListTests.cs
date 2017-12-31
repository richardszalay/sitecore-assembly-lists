using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks.Tests
{
    public class AssemblyListTests
    {
        const string TestListFilename = @"c:\test-list.txt";

        [Fact]
        public void Parse_returns_parsed_assemblyinfo()
        {
            var entries = TestParse(@"Filename,FileVersion,Version
Test1.dll,1.2.3.4,4.3.2.1
Test2.dll,5.6.7.8,8.7.6.5");

            AssertInfo(entries[0].AssemblyInfo, "Test1.dll", new Version(1, 2, 3, 4), new Version(4, 3, 2, 1));
            AssertInfo(entries[1].AssemblyInfo, "Test2.dll", new Version(5, 6, 7, 8), new Version(8, 7, 6, 5));
        }

        [Fact]
        public void Parse_includes_source_data()
        {
            var entries = TestParse(@"Filename,FileVersion,Version
Test1.dll,1.2.3.4,4.3.2.1
Test2.dll,5.6.7.8,8.7.6.5");

            AssertEntry(entries[0], TestListFilename, 2);
            AssertEntry(entries[1], TestListFilename, 3);
        }

        [Fact]
        public void Parse_normalizes_version_numbers()
        {
            var entries = TestParse(@"Filename,FileVersion,Version
Test1.dll,1.2.3.04,4.3.2.01
Test2.dll,5.6.7,8.7.6");

            AssertInfo(entries[0].AssemblyInfo, "Test1.dll", new Version(1, 2, 3, 4), new Version(4, 3, 2, 1));
            AssertInfo(entries[1].AssemblyInfo, "Test2.dll", new Version(5, 6, 7, 0), new Version(8, 7, 6, 0));
        }

        [Fact]
        public void Throws_attributed_exception_for_invalid_lines()
        {
            var ex = Assert.Throws<AssemblyListParseException>(() => TestParse(@"Filename,FileVersion,Version
Test1.dll,1.2.3.04"));

            Assert.Equal(TestListFilename, ex.SourceFile);
            Assert.Equal(2, ex.SourceLine);
        }

        [Fact]
        public void Throws_attributed_exception_for_invalid_versions()
        {
            var ex = Assert.Throws<AssemblyListParseException>(() => TestParse(@"Filename,FileVersion,Version
Test1.dll,a.b.c.d"));

            Assert.Equal(TestListFilename, ex.SourceFile);
            Assert.Equal(2, ex.SourceLine);
        }

        private void AssertEntry(AssemblyListEntry assemblyListEntry, string sourceFile, int sourceLine)
        {
            Assert.Equal(sourceFile, assemblyListEntry.SourceFile);
            Assert.Equal(sourceLine, assemblyListEntry.SourceLine);
        }

        static void AssertInfo(AssemblyInfo assemblyInfo, string filename, Version fileVersion, Version assemblyVersion)
        {
            Assert.Equal(filename, assemblyInfo.Filename);
            Assert.Equal(fileVersion, assemblyInfo.FileVersion);
            Assert.Equal(assemblyVersion, assemblyInfo.AssemblyVersion);
        }

        IList<AssemblyListEntry> TestParse(string input)
        {
            using (var reader = new StringReader(input))
            {
                return AssemblyList.Parse(reader, TestListFilename).ToList();
            }
        }
    }
}
