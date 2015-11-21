// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Matchers;

namespace Stylize.EngineTests
{
    [TestClass]
    public class AttributeDocumentMatcherTests
    {
        static readonly IDocumentMatcher Matcher = new AttributeDocumentMatcher();

        const string GeneratedText = @"
using System.Runtime.CompilerServices;

namespace Test
{
    [CompilerGenerated]
    public class Generated
    {
    }
}
";

        static async Task<bool> IsMatchAsync(string documentText, params string[] matchingAttributes)
        {
            using (var builder = new WorkspaceBuilder())
            {
                builder.ApplyOptions(
                    o => o.WithChangedOption(AttributeDocumentMatcher.AttributeNames, matchingAttributes));

                return await Matcher.IsMatchAsync(builder.AddDocument(documentText));
            }
        }

        [TestMethod]
        public async Task AttributeNameMatchesTest()
        {
            Assert.IsTrue(
                await IsMatchAsync(GeneratedText, "CompilerGeneratedAttribute", "GeneratedCodeAttribute"),
                "Expected attribute symbol name to match");
        }

        [TestMethod]
        public async Task AttributeFullNameMatchesTest()
        {
            Assert.IsTrue(
                await IsMatchAsync(GeneratedText, "System.Runtime.CompilerServices.CompilerGeneratedAttribute"),
                "Expected attribute symbol name to match");
        }

        [TestMethod]
        public async Task AttributeBaseClassDoesNotMatchTest()
        {
            Assert.IsFalse(
                await IsMatchAsync(GeneratedText, "Attribute"),
                "Expected attribute base symbol name to not match");
        }
    }
}
