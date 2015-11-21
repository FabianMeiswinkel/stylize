// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Matchers;

namespace Stylize.EngineTests
{
    [TestClass]
    public class ProjectNameDocumentMatcherTests
    {
        static readonly IDocumentMatcher Matcher = new ProjectNameDocumentMatcher();

        static async Task<bool> IsMatchAsync(string projectName, string projectNameRegex)
        {
            using (var builder = new WorkspaceBuilder(projectName))
            {
                builder.ApplyOptions(
                    o => o.WithChangedOption(ProjectNameDocumentMatcher.ProjectNameRegex, projectNameRegex));

                return await Matcher.IsMatchAsync(builder.AddClass("Sample"));
            }
        }

        [TestMethod]
        public async Task MatchingProjectNameRegexTest()
        {
            Assert.IsTrue(
                await IsMatchAsync("SampleTests", ".*Test.*"),
                "Expected project name match");
        }

        [TestMethod]
        public async Task MismatchedProjectNameRegexTest()
        {
            Assert.IsFalse(
                await IsMatchAsync("SampleClients", ".*Test.*"),
                "Expected project path mismatch");
        }

        [TestMethod]
        public async Task MissingProjectNameRegexIsNotMatchTest()
        {
            Assert.IsFalse(
                await IsMatchAsync("Sample", projectNameRegex: null),
                "Expected no match for missing project name regex");
        }
    }
}
