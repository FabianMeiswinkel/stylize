// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class DefaultAccessModifierStyleRuleTests
    {
        const string ClassWrapper = @"
namespace Test
{{
    public class Sample
    {{
        {0}
    }}
}}";

        const string CompilationUnitWrapper = "{0}";

        const string NamespaceWrapper = @"
namespace Test
{{
    {0}
}}";

        static readonly IReadOnlyList<string> NestedDeclarations = new[]
        {
            "const int Field;",
            "static readonly System.Guid Field;",
            "int field;",
            "event System.EventHandler Event;",
            "string Property { get; }",
            "int this[int i] { get { return 0; } }",
            "static Sample operator ~(Sample s) { return new Sample(); }",
            "static implicit operator int(Sample s) { return 0; }",
            "void Work() { }",
            "Sample() { }"
        };

        static readonly IStyleRule Rule = new DefaultAccessModifierStyleRule();

        const string StructWrapper = @"
namespace Test
{{
    public struct Sample
    {{
        {0}
    }}
}}";

        static readonly IReadOnlyList<string> UniversalDeclarations = new[]
        {
            "class Thing { }",
            "struct Thing { }",
            "interface IThing { }",
            "delegate void Work();",
            "enum Value { Zero }"
        };

        static string AddModifier(string modifier, string declaration)
        {
            if (String.IsNullOrEmpty(modifier))
            {
                return declaration;
            }

            return String.Concat(modifier, " ", declaration);
        }

        static OptionSet ApplyOptions(OptionSet options, bool explicitAccessModifier)
        {
            return options.WithChangedOption(
                DefaultAccessModifierStyleRule.ExplicitAccessModifier, explicitAccessModifier);
        }

        [TestMethod]
        public async Task CorrectedNestedAccessModifiersTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Explicit private access modifier in class",
                    Wrapper = ClassWrapper,
                    OriginalModifier = String.Empty,
                    ExpectedModifier = "private",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Explicit private access modifier in struct",
                    Wrapper = StructWrapper,
                    OriginalModifier = String.Empty,
                    ExpectedModifier = "private",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Implicit private access modifier in class",
                    Wrapper = ClassWrapper,
                    OriginalModifier = "private",
                    ExpectedModifier = String.Empty,
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Implicit private access modifier in struct",
                    Wrapper = StructWrapper,
                    OriginalModifier = "private",
                    ExpectedModifier = String.Empty,
                    ExplicitAccessModifier = false
                }
            };

            foreach (var scenario in scenarios)
            {
                foreach (string declaration in UniversalDeclarations.Union(NestedDeclarations))
                {
                    Console.WriteLine("Scenario: \"{0}\", Original Declaration: \"{1}\"", scenario.Name, declaration);
                    await Assertions.AssertEnforcementAsync(
                        Rule,
                        String.Format(scenario.Wrapper, AddModifier(scenario.OriginalModifier, declaration)),
                        String.Format(scenario.Wrapper, AddModifier(scenario.ExpectedModifier, declaration)),
                        o => ApplyOptions(o, scenario.ExplicitAccessModifier));
                }
            }
        }

        [TestMethod]
        public async Task CorrectedOuterAccessModifiersTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Explicit internal access modifier in compilation unit",
                    Wrapper = CompilationUnitWrapper,
                    OriginalModifier = String.Empty,
                    ExpectedModifier = "internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Explicit internal access modifier in namespace",
                    Wrapper = NamespaceWrapper,
                    OriginalModifier = String.Empty,
                    ExpectedModifier = "internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Implicit internal access modifier in compilation unit",
                    Wrapper = CompilationUnitWrapper,
                    OriginalModifier = "internal",
                    ExpectedModifier = String.Empty,
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Implicit internal access modifier in namespace",
                    Wrapper = NamespaceWrapper,
                    OriginalModifier = "internal",
                    ExpectedModifier = String.Empty,
                    ExplicitAccessModifier = false
                }
            };

            foreach (var scenario in scenarios)
            {
                foreach (string declaration in UniversalDeclarations)
                {
                    Console.WriteLine("Scenario: \"{0}\", Original Declaration: \"{1}\"", scenario.Name, declaration);
                    await Assertions.AssertEnforcementAsync(
                        Rule,
                        String.Format(scenario.Wrapper, AddModifier(scenario.OriginalModifier, declaration)),
                        String.Format(scenario.Wrapper, AddModifier(scenario.ExpectedModifier, declaration)),
                        o => ApplyOptions(o, scenario.ExplicitAccessModifier));
                }
            }
        }

        [TestMethod]
        public async Task UnchangedNestedAccessModifiersTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Public access modifier in class",
                    Wrapper = ClassWrapper,
                    Modifier = "public",
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Public access modifier in struct",
                    Wrapper = StructWrapper,
                    Modifier = "public",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Internal access modifier in class",
                    Wrapper = ClassWrapper,
                    Modifier = "internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Internal access modifier in struct",
                    Wrapper = StructWrapper,
                    Modifier = "internal",
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Protected access modifier in class",
                    Wrapper = ClassWrapper,
                    Modifier = "protected",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Protected access modifier in struct",
                    Wrapper = StructWrapper,
                    Modifier = "protected",
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Protected internal access modifier in class",
                    Wrapper = ClassWrapper,
                    Modifier = "protected internal",
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Protected internal access modifier in struct",
                    Wrapper = StructWrapper,
                    Modifier = "protected internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Private access modifier in class",
                    Wrapper = ClassWrapper,
                    Modifier = "private",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Private access modifier in struct",
                    Wrapper = StructWrapper,
                    Modifier = "private",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Default (private) access modifier in compilation unit",
                    Wrapper = ClassWrapper,
                    Modifier = String.Empty,
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Default (private) access modifier in namespace",
                    Wrapper = StructWrapper,
                    Modifier = String.Empty,
                    ExplicitAccessModifier = false
                }
            };

            foreach (var scenario in scenarios)
            {
                foreach (string declaration in UniversalDeclarations.Union(NestedDeclarations))
                {
                    Console.WriteLine("Scenario: \"{0}\", Declaration: \"{1}\"", scenario.Name, declaration);
                    await Assertions.AssertNoEnforcementAsync(
                        Rule,
                        String.Format(scenario.Wrapper, AddModifier(scenario.Modifier, declaration)),
                        o => ApplyOptions(o, scenario.ExplicitAccessModifier));
                }
            }
        }

        [TestMethod]
        public async Task UnchangedOuterAccessModifiersTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Public access modifier in compilation unit",
                    Wrapper = CompilationUnitWrapper,
                    Modifier = "public",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Public access modifier in namespace",
                    Wrapper = NamespaceWrapper,
                    Modifier = "public",
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Internal access modifier in compilation unit",
                    Wrapper = CompilationUnitWrapper,
                    Modifier = "internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Internal access modifier in namespace",
                    Wrapper = NamespaceWrapper,
                    Modifier = "internal",
                    ExplicitAccessModifier = true
                },
                new
                {
                    Name = "Default (internal) access modifier in compilation unit",
                    Wrapper = CompilationUnitWrapper,
                    Modifier = String.Empty,
                    ExplicitAccessModifier = false
                },
                new
                {
                    Name = "Default (internal) access modifier in namespace",
                    Wrapper = NamespaceWrapper,
                    Modifier = String.Empty,
                    ExplicitAccessModifier = false
                }
            };

            foreach (var scenario in scenarios)
            {
                foreach (string declaration in UniversalDeclarations)
                {
                    Console.WriteLine("Scenario: \"{0}\", Declaration: \"{1}\"", scenario.Name, declaration);
                    await Assertions.AssertNoEnforcementAsync(
                        Rule,
                        String.Format(scenario.Wrapper, AddModifier(scenario.Modifier, declaration)),
                        o => ApplyOptions(o, scenario.ExplicitAccessModifier));
                }
            }
        }
    }
}
