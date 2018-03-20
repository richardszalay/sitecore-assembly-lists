using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using Xunit;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks.Tests
{
    public class LoadDefaultAssemblyListsTests
    {
        readonly string fixtureAssembliesBasePath;

        public LoadDefaultAssemblyListsTests()
        {
            var thisAssemblyPath = new Uri(this.GetType().Assembly.GetName().CodeBase).LocalPath;

            const string RelativePathToFixtureAssemblies = @"..\..\..\..\..\..\fixtures";

            fixtureAssembliesBasePath = Path.GetFullPath(Path.Combine(thisAssemblyPath, RelativePathToFixtureAssemblies));
        }

        [Fact]
        public void whenVerificationDisabled_returnsMatchingAssemblies()
        {
            var buildEngine = new FakeBuildEngine();

            var sut = new LoadDefaultAssemblyLists
            {
                BuildEngine = buildEngine,
                Manifest = GetFixtureTaskItem(@"acceptance-default-lists.txt")
            };

            var result = sut.Execute();

            Assert.True(result);

            var firstDefaultList = Assert.Single(sut.DefaultAssemblyLists);

            AssertFixtureTaskItem(@"acceptance-list.txt", firstDefaultList);
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
