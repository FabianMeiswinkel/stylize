// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine;
using Stylize.Engine.Configuration;
using Stylize.Engine.Matchers;
using Stylize.Engine.Rules;

namespace Stylize.EngineTests
{
    [TestClass]
    public class StylizeEngineTests
    {
        const string JsonConfiguration = @"
{
  ""exclusionMatchers"":{
    ""TestMatcher"":{
      ""boolean"":false
    }
  },
  
  ""exportAssemblies"":[
    ""Stylize.EngineTests.dll""
  ],

  ""globalOptions"":{
    ""newLineText"":""\n""
  },

  ""rules"":{
    ""TestRule1"":{
      ""strings"":[""one"", ""two""],
      ""boolean"":true
    },
    ""TestRule2"":{}
  }
}";

        const string NewLineText = "\n";
        const bool TestMatcherBooleanOption = false;
        const bool TestRuleBooleanOption = true;
        static readonly IReadOnlyList<string> TestRuleStringsOption = new[] { "one", "two" };

        static Func<string, bool> isMatch;
        static Action<Type, string> onComponentExecuted;

        [TestInitialize]
        public void DelegateSetup()
        {
            isMatch = name => false;
            onComponentExecuted = delegate { };
        }

        [TestMethod]
        public async Task StylizeEngineBasicScenarioTest()
        {
            using (var workspaceBuilder = new WorkspaceBuilder())
            {
                // For this test we will have three C# documents.
                string[] cSharpDocumentNames = new[]
                {
                    workspaceBuilder.AddClass("Class1").Name,
                    workspaceBuilder.AddClass("Class2").Name,
                    workspaceBuilder.AddClass("Class3").Name
                };

                // The exclusion matcher will match (exclude) the second document.
                isMatch = name => name.Equals(cSharpDocumentNames[1]);

                // Let's setup the onComponentExecuted delegate to capture executions
                var executions = new List<Tuple<Type, string>>();
                onComponentExecuted = (componentType, documentName) =>
                    executions.Add(Tuple.Create(componentType, documentName));

                var configParser = JsonConfigurationParser.FromString(JsonConfiguration);
                using (var engine = new StylizeEngine(configParser))
                {
                    await engine.RunAsync(workspaceBuilder.BuildWorkspace());
                }

                // Finally, let's assert on the executions.  In particular, we expect that the test matcher should
                // execute on all documents.  However, the test rules (TestRule2 then TestRule1 per ordering) should
                // only execute on the first and third C# documents because the matcher excluded the second.
                var expectedExecutions = new List<Tuple<Type, string>>
                {
                    Tuple.Create(typeof(TestMatcher), cSharpDocumentNames[0]),
                    Tuple.Create(typeof(TestRule2), cSharpDocumentNames[0]),
                    Tuple.Create(typeof(TestRule1), cSharpDocumentNames[0]),

                    Tuple.Create(typeof(TestMatcher), cSharpDocumentNames[1]),

                    Tuple.Create(typeof(TestMatcher), cSharpDocumentNames[2]),
                    Tuple.Create(typeof(TestRule2), cSharpDocumentNames[2]),
                    Tuple.Create(typeof(TestRule1), cSharpDocumentNames[2])
                };

                executions.Should().Equal(expectedExecutions, "Actual executions did not match expectations");
            }
        }

        [ExportDocumentMatcher(Name)]
        class TestMatcher : IDocumentMatcher
        {
            const string Name = "TestMatcher";

            [ExportStylizeOption]
            static Option<bool> BooleanOption { get; } = new Option<bool>(Name, "boolean");

            public Task<bool> IsMatchAsync(Document document)
            {
                document.GetOption(BooleanOption).Should().Be(
                    TestMatcherBooleanOption, "Unexpected boolean option value");

                onComponentExecuted(typeof(TestMatcher), document.Name);

                return Task.FromResult(isMatch(document.Name));
            }
        }

        [ExportStyleRule(Name, LanguageNames.CSharp)]
        [OrderStyleRule(After = TestRule2.Name)]
        [OrderStyleRule(Before = "NonExistantRule")]
        class TestRule1 : IStyleRule
        {
            public const string Name = "TestRule1";

            [ExportStylizeOption]
            static Option<bool> BooleanOption { get; } = new Option<bool>(Name, "boolean");

            [ExportStylizeOption]
            static Option<IReadOnlyList<string>> StringsOption { get; } =
                new Option<IReadOnlyList<string>>(Name, "strings", defaultValue: new string[0]);

            public Task<Solution> EnforceAsync(Document document)
            {
                document.GetOption(BooleanOption).Should().Be(
                    TestRuleBooleanOption, "Unexpected boolean option value");
                document.GetOption(StringsOption).ShouldBeEquivalentTo(
                    TestRuleStringsOption, "Unexpected strings option value");

                onComponentExecuted(typeof(TestRule1), document.Name);

                return Task.FromResult(document.Project.Solution);
            }
        }

        [ExportStyleRule(Name, LanguageNames.CSharp)]
        [OrderStyleRule(Before = TestRule1.Name)]
        class TestRule2 : IStyleRule
        {
            public const string Name = "TestRule2";

            public Task<Solution> EnforceAsync(Document document)
            {
                document.GetOption(GlobalOptions.NewLineText).Should().Be(
                    NewLineText, "Unexpected newLineText global option value");

                onComponentExecuted(typeof(TestRule2), document.Name);

                return Task.FromResult(document.Project.Solution);
            }
        }
    }
}
