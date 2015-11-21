// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Matchers;

namespace Stylize.EngineTests
{
    [TestClass]
    public class MalformedDocumentMatcherTests
    {
        static readonly IDocumentMatcher Matcher = new MalformedDocumentMatcher();

        static async Task<bool> IsMatchAsync(string documentText)
        {
            using (var builder = new WorkspaceBuilder())
            {
                return await Matcher.IsMatchAsync(builder.AddDocument(documentText));
            }
        }

        [TestMethod]
        public async Task MatchingMalformedDocumentTest()
        {
            Assert.IsTrue(
                await IsMatchAsync("class Sample { void Test() { x++; } }"),
                "Expected malformed document match");
        }

        [TestMethod]
        public async Task MismatchedValidDocumentTest()
        {
            Assert.IsFalse(
                await IsMatchAsync("class Sample { void Test() { int i; } }"),
                "Expected valid document mismatch");
        }
    }
}
