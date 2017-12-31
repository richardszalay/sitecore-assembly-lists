using System;
using System.IO;
using Xunit;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks.Tests
{
    public class AssemblyInfoTests
    {
        readonly string fixtureAssembliesBasePath;

        public AssemblyInfoTests()
        {
            var thisAssemblyPath = new Uri(this.GetType().Assembly.GetName().CodeBase).LocalPath;

            const string RelativePathToFixtureAssemblies = @"..\..\..\..\..\..\fixtures";

            fixtureAssembliesBasePath = Path.Combine(thisAssemblyPath, RelativePathToFixtureAssemblies);
        }

        [Fact]
        public void FromAssembly_withFileVersion_returnsFileVersion()
        {
            Test(@"acceptance\Test1.dll", true, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(1, 0, 0, 0),
                FileVersion = new Version(1, 2, 3, 4)
            });
        }

        [Fact]
        public void FromAssembly_withFileVersion_canLoadMoreThanOneMatchingAssembly()
        {
            Test(@"acceptance\Test1.dll", true, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(1, 0, 0, 0),
                FileVersion = new Version(1, 2, 3, 4)
            });

            Test(@"acceptance-alt\Test1.dll", true, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(2, 0, 0, 0),
                FileVersion = new Version(2, 2, 3, 4)
            });
        }

        [Fact]
        public void FromAssembly_withFileVersion_normalizesVersions()
        {
            Test(@"acceptance\Test3.dll", true, new AssemblyInfo
            {
                Filename = "Test3.dll",
                AssemblyVersion = new Version(3, 0, 1, 0),
                FileVersion = new Version(1, 2, 3, 4)
            });

            Test(@"acceptance\Test4.dll", true, new AssemblyInfo
            {
                Filename = "Test4.dll",
                AssemblyVersion = new Version(4, 0, 0, 1),
                FileVersion = new Version(1, 2, 3, 0)
            });
        }

        [Fact]
        public void FromAssembly_withoutFileVersion_loadsAssemblyVersion()
        {
            Test(@"acceptance\Test1.dll", false, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(1, 0, 0, 0)
            });
        }

        [Fact]
        public void FromAssembly_withoutFileVersion_canLoadMoreThanOneMatchingAssembly()
        {
            Test(@"acceptance\Test1.dll", false, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(1, 0, 0, 0)
            });

            Test(@"acceptance-alt\Test1.dll", false, new AssemblyInfo
            {
                Filename = "Test1.dll",
                AssemblyVersion = new Version(2, 0, 0, 0)
            });
        }

        private void Test(string relativeAssemblyPath, bool includeFileVersion, AssemblyInfo expectedAssemblyInfo)
        {
            string fullAssemblyPath = Path.Combine(fixtureAssembliesBasePath, relativeAssemblyPath);

            var actualAssemblyInfo = AssemblyInfo.FromAssembly(fullAssemblyPath, includeFileVersion);

            Assert.Equal(expectedAssemblyInfo.Filename, actualAssemblyInfo.Filename);
            Assert.Equal(expectedAssemblyInfo.AssemblyVersion, actualAssemblyInfo.AssemblyVersion);

            if (includeFileVersion)
            {
                Assert.Equal(expectedAssemblyInfo.FileVersion, actualAssemblyInfo.FileVersion);
            }
            else
            {
                Assert.Null(expectedAssemblyInfo.FileVersion);
            }
        }
    }
}
