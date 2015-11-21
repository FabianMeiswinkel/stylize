// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class HeaderCommentStyleRuleTests
    {
        static readonly IStyleRule Rule = new HeaderCommentStyleRule();

        const string Comments = @"// <copyright>
//    Copyright (c) Test, Inc.  All rights reserved.
// </copyright>

";

        const string DocumentWithUsings = @"{0}using System;
using System.Collections.Generic;

namespace Test
{{
    class IntList : List<int> {{ }}
}}";

        const string DocumentWithoutUsings = @"{0}namespace Test
{{
    class TestClass {{ }}
}}";

        const string UnexpectedComments = @"// Comments!

";

        static OptionSet ApplyOptions(OptionSet previousOptions, string comments)
        {
            var commentLines = new List<string>();
            if (!string.IsNullOrEmpty(comments))
            {
                using (var reader = new StringReader(comments))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (String.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        commentLines.Add(line.Replace("// ", String.Empty));
                    }
                }
            }

            return previousOptions.WithChangedOption(HeaderCommentStyleRule.HeaderComments, commentLines);
        }

        [TestMethod]
        public async Task HeaderCommentsAddedTest()
        {
            await TestCommentRuleAsync(initialComments: String.Empty, expectedComments: Comments);
        }

        [TestMethod]
        public async Task HeaderCommentsAlreadyPresentTest()
        {
            await TestCommentRuleAsync(initialComments: Comments, expectedComments: Comments);
        }

        [TestMethod]
        public async Task HeaderCommentsFixedTest()
        {
            await TestCommentRuleAsync(initialComments: UnexpectedComments, expectedComments: Comments);
        }

        [TestMethod]
        public async Task HeaderCommentsNotRequiredTest()
        {
            await TestCommentRuleAsync(initialComments: String.Empty, expectedComments: String.Empty);
        }

        [TestMethod]
        public async Task HeaderCommentsRemovedTest()
        {
            await TestCommentRuleAsync(initialComments: Comments, expectedComments: String.Empty);
        }

        static async Task TestCommentRuleAsync(string initialComments, string expectedComments)
        {
            var documents = new[]
            {
                new { Name = "WithUsings",    Text = DocumentWithUsings },
                new { Name = "WithoutUsings", Text = DocumentWithoutUsings },
            };

            foreach (var document in documents)
            {
                Console.WriteLine($"Testing with document {document.Name}");

                var originalDocumentText = String.Format(document.Text, initialComments);

                if (String.Equals(initialComments, expectedComments))
                {
                    await Assertions.AssertNoEnforcementAsync(
                        Rule, originalDocumentText, options => ApplyOptions(options, expectedComments));
                }
                else
                {
                    await Assertions.AssertEnforcementAsync(
                        Rule,
                        originalDocumentText,
                        String.Format(document.Text, expectedComments),
                        options => ApplyOptions(options, expectedComments));
                }
            }
        }
    }
}
