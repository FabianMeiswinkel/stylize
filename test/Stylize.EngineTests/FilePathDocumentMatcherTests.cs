// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Matchers;

namespace Stylize.EngineTests
{
    [TestClass]
    public class FilePathDocumentMatcherTests
    {
        static readonly IDocumentMatcher Matcher = new FilePathDocumentMatcher();

        static async Task<bool> IsMatchAsync(string className, string directoryPath, string fileNameRegex)
        {
            using (var builder = new WorkspaceBuilder())
            {
                builder.ApplyOptions(
                    o => o.WithChangedOption(FilePathDocumentMatcher.FilePathRegex, fileNameRegex));

                return await Matcher.IsMatchAsync(builder.AddClass(className, directoryPath));
            }
        }

        [TestMethod]
        public async Task MatchingFileNameDirectoryRegexTest()
        {
            Assert.IsTrue(
                await IsMatchAsync("Sample", "C:\\source\\stuff\\", ".*source\\\\stuff.*"),
                "Expected file path match");
        }

        [TestMethod]
        public async Task MatchingFileNameMultiRegexTest()
        {
            Assert.IsTrue(
                await IsMatchAsync("Sample", "C:\\source\\", ".*Sample\\.cs$"),
                "Expected file path match");
        }

        [TestMethod]
        public async Task MatchingFileNameRegexTest()
        {
            Assert.IsTrue(
                await IsMatchAsync("Sample", "C:\\source\\", ".*Sample\\.cs$|.*Other\\.cs$"),
                "Expected file path match");
        }

        [TestMethod]
        public async Task MismatchedFileNameRegexTest()
        {
            Assert.IsFalse(
                await IsMatchAsync("Sample", "C:\\source\\", ".*Test.*"),
                "Expected file path mismatch");
        }

        [TestMethod]
        public async Task MissingFilePathRegexIsNotMatchTest()
        {
            Assert.IsFalse(
                await IsMatchAsync("Sample", "C:\\source\\", fileNameRegex: null),
                "Expected no match for missing file path regex");
        }
    }
}
