using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using Xunit;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks.Tests
{
    public class ResolveSitecoreAssembliesTests
    {
        readonly string fixtureAssembliesBasePath;

        public ResolveSitecoreAssembliesTests()
        {
            var thisAssemblyPath = new Uri(this.GetType().Assembly.GetName().CodeBase).LocalPath;

            const string RelativePathToFixtureAssemblies = @"..\..\..\..\..\..\fixtures";

            fixtureAssembliesBasePath = Path.GetFullPath(Path.Combine(thisAssemblyPath, RelativePathToFixtureAssemblies));
        }

        [Fact]
        public void whenVerificationDisabled_returnsMatchingAssemblies()
        {
            var buildEngine = new FakeBuildEngine();

            var sut = new ResolveSitecoreAssemblies
            {
                BuildEngine = buildEngine,
                Assemblies = new[]
                {
                    GetFixtureTaskItem(@"acceptance\Test1.dll"),
                    GetFixtureTaskItem(@"acceptance\Test2.dll"),
                    GetFixtureTaskItem(@"dummy\Unrelated1.dll")
                },
                AssemblyLists = new[]
                {
                    GetFixtureTaskItem(@"acceptance-list.txt")
                },
                VerifyAssemblies = false
            };

            var result = sut.Execute();

            Assert.True(result);

            Assert.Equal(2, sut.SitecoreAssemblies.Length);
            AssertFixtureTaskItem(@"acceptance\Test1.dll", sut.SitecoreAssemblies[0]);
            AssertFixtureTaskItem(@"acceptance\Test2.dll", sut.SitecoreAssemblies[1]);
        }

        [Fact]
        public void whenVerificationEnabled_returnsMatchingAssemblies()
        {
            var buildEngine = new FakeBuildEngine();

            var sut = new ResolveSitecoreAssemblies
            {
                BuildEngine = buildEngine,
                Assemblies = new[]
                {
                    GetFixtureTaskItem(@"acceptance\Test1.dll"),
                    GetFixtureTaskItem(@"acceptance\Test2.dll"),
                    GetFixtureTaskItem(@"dummy\Unrelated1.dll")
                },
                AssemblyLists = new[]
                {
                    GetFixtureTaskItem(@"acceptance-list.txt")
                },
                VerifyAssemblies = true
            };

            var result = sut.Execute();

            Assert.True(result);

            Assert.Equal(2, sut.SitecoreAssemblies.Length);
            AssertFixtureTaskItem(@"acceptance\Test1.dll", sut.SitecoreAssemblies[0]);
            AssertFixtureTaskItem(@"acceptance\Test2.dll", sut.SitecoreAssemblies[1]);
        }

        [Fact]
        public void whenVerificationEnabled_logsWarningsForVersionMismatches()
        {
            var buildEngine = new FakeBuildEngine();

            var sut = new ResolveSitecoreAssemblies
            {
                BuildEngine = buildEngine,
                Assemblies = new[]
                {
                    GetFixtureTaskItem(@"acceptance-alt\Test1.dll"),
                    GetFixtureTaskItem(@"acceptance\Test2.dll"),
                    GetFixtureTaskItem(@"dummy\Unrelated1.dll")
                },
                AssemblyLists = new[]
                {
                    GetFixtureTaskItem(@"acceptance-list.txt")
                },
                VerifyAssemblies = true
            };

            var result = sut.Execute();

            Assert.True(result);
            Assert.Single(buildEngine.LoggedWarningEvents);
        }

        private ITaskItem GetFixtureTaskItem(string relativePath)
        {
            string fullPath = Path.Combine(fixtureAssembliesBasePath, relativePath);

            return new TaskItem(fullPath);
        }

        private void AssertFixtureTaskItem(string relativePath, ITaskItem actual)
        {
            string fullPath = Path.Combine(fixtureAssembliesBasePath, relativePath);

            Assert.Equal(fullPath, actual.GetMetadata("FullPath"));
        }
    }
}
