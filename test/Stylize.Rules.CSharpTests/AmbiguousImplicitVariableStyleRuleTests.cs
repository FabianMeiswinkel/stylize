// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class AmbiguousImplicitVariableStyleRuleTests
    {
        const string ForEachDeclarationWrapper = @"
using System;
using System.Linq;

namespace Test
{{
    class Sample
    {{
        void Work()
        {{
            foreach ({0}) {{ }}
        }}
    }}
}}";

        const string GenericMethodDeclarationWrapper = @"
using System;
using System.Linq;

namespace Test
{{
    class Sample<T> where T : {0}
    {{
        T Value {{ get; }}

        void Work()
        {{
            {1}
        }}
    }}
}}";

        const string MethodDeclarationWrapper = @"
using System;
using System.Linq;

namespace Test
{{
    class Sample
    {{
        void Work()
        {{
            {0}
        }}
    }}
}}";

        static readonly IStyleRule Rule = new AmbiguousImplicitVariableStyleRule();

        static OptionSet ApplyForEachOption(OptionSet options, bool value = true)
        {
            return options.WithChangedOption(AmbiguousImplicitVariableStyleRule.EnforceForEachStatements, value);
        }

        [TestMethod]
        public async Task AmbiguousForEachImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Ambiguous IEnumerable",
                    Original = "var number in Enumerable.Range(1, 10)",
                    Expected = "int number in Enumerable.Range(1, 10)"
                },
                new
                {
                    Name = "Type parameter member access",
                    Original = "var pair in new System.Collections.Generic.Dictionary<string, Guid>()",
                    Expected = "System.Collections.Generic.KeyValuePair<string, System.Guid> pair in new System.Collections.Generic.Dictionary<string, Guid>()"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(ForEachDeclarationWrapper, scenario.Original),
                    String.Format(ForEachDeclarationWrapper, scenario.Expected),
                    o => ApplyForEachOption(o));
            }
        }

        [TestMethod]
        public async Task AmbiguousGenericMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Type parameter declaration",
                    Constraint = "class",
                    Original = "var value = this.Value;",
                    Expected = "T value = this.Value;"
                },
                new
                {
                    Name = "Type parameter member access",
                    Constraint = "System.Collections.IEnumerable",
                    Original = "var enumerator = this.Value.GetEnumerator();",
                    Expected = "System.Collections.IEnumerator enumerator = this.Value.GetEnumerator();"
                },
                new
                {
                    Name = "Type parameter as type argument",
                    Constraint = "class",
                    Original = "var values = new[] { this.Value }.ToList();",
                    Expected = "System.Collections.Generic.List<T> values = new[] { this.Value }.ToList();"
                },
                new
                {
                    Name = "Unknown type constraint (implicit)",
                    Constraint = "UnknownType",
                    Original = "var unknown = this.Value;",
                    Expected = "T unknown = this.Value;"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(GenericMethodDeclarationWrapper, scenario.Constraint, scenario.Original),
                    String.Format(GenericMethodDeclarationWrapper, scenario.Constraint, scenario.Expected));
            }
        }

        [TestMethod]
        public async Task AmbiguousMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Generic with type keyword",
                    Original = "var numbers = Enumerable.Range(1, 10).ToList();",
                    Expected = "System.Collections.Generic.List<int> numbers = Enumerable.Range(1, 10).ToList();"
                },
                new
                {
                    Name = "Generic without type keyword",
                    Original = "var guids = Enumerable.Range(1, 10).Select(i => Guid.NewGuid());",
                    Expected = "System.Collections.Generic.IEnumerable<System.Guid> guids = Enumerable.Range(1, 10).Select(i => Guid.NewGuid());"
                },
                new
                {
                    Name = "Implicit generic LINQ expression",
                    Original = "var number = Enumerable.Range(1, 10).First();",
                    Expected = "int number = Enumerable.Range(1, 10).First();"
                },
                new
                {
                    Name = "Nullable variable",
                    Original = "var id = System.Threading.Tasks.Task.CurrentId;",
                    Expected = "int? id = System.Threading.Tasks.Task.CurrentId;"
                },
                new
                {
                    Name = "Value type variable",
                    Original = "var size = Environment.WorkingSet;",
                    Expected = "long size = Environment.WorkingSet;"
                },
                new
                {
                    Name = "Reference type variable",
                    Original = "var version = Environment.Version;",
                    Expected = "System.Version version = Environment.Version;"
                },
                new
                {
                    Name = "Spacing preserved",
                    Original = " var  data  = new[] { 1, 2,  3,   4,    5 };",
                    Expected = " int[]  data  = new[] { 1, 2,  3,   4,    5 };"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(MethodDeclarationWrapper, scenario.Original),
                    String.Format(MethodDeclarationWrapper, scenario.Expected));
            }
        }

        [TestMethod]
        public async Task UnambiguousForEachImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Typed List",
                    Original = "int number in new System.Collections.Generic.List<int>()",
                    Expected = "var number in new System.Collections.Generic.List<int>()"
                },
                new
                {
                    Name = "Type parameter member access",
                    Original = "Guid guid in new System.Collections.ArrayList().Cast<Guid>()",
                    Expected = "var guid in new System.Collections.ArrayList().Cast<Guid>()"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(ForEachDeclarationWrapper, scenario.Original),
                    String.Format(ForEachDeclarationWrapper, scenario.Expected),
                    o => ApplyForEachOption(o));
            }
        }

        [TestMethod]
        public async Task UnambiguousGenericMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Type parameter instantiation",
                    Constraint = "new()",
                    Original = "T value = new T();",
                    Expected = "var value = new T();"
                },
                new
                {
                    Name = "Type parameter extension method",
                    Constraint = "System.Collections.IEnumerable",
                    Original = "byte b = this.Value.Cast<Byte>().First();",
                    Expected = "var b = this.Value.Cast<Byte>().First();"
                },
                new
                {
                    Name = "Type parameter as type argument",
                    Constraint = "class",
                    Original = "T value = new System.Collections.Generic.List<T>().First();",
                    Expected = "var value = new System.Collections.Generic.List<T>().First();"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(GenericMethodDeclarationWrapper, scenario.Constraint, scenario.Original),
                    String.Format(GenericMethodDeclarationWrapper, scenario.Constraint, scenario.Expected));
            }
        }

        [TestMethod]
        public async Task UnambiguousMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Generic with type keyword instantiation",
                    Original = "System.Collections.Generic.List<int> numbers = new System.Collections.Generic.List<int>();",
                    Expected = "var numbers = new System.Collections.Generic.List<int>();"
                },
                new
                {
                    Name = "Generic with mixed type keyword instantiation",
                    Original = "System.Collections.Generic.List<UInt64> numbers = new System.Collections.Generic.List<ulong>();",
                    Expected = "var numbers = new System.Collections.Generic.List<ulong>();"
                },
                new
                {
                    Name = "Generic without type keyword instantiation",
                    Original = "System.Collections.Generic.List<Guid> guids = new System.Collections.Generic.List<Guid>();",
                    Expected = "var guids = new System.Collections.Generic.List<Guid>();"
                },
                new
                {
                    Name = "Explicit generic LINQ expression",
                    Original = "int number = Enumerable.Range(1, 10).First<int>();",
                    Expected = "var number = Enumerable.Range(1, 10).First<int>();"
                },
                new
                {
                    Name = "Nullable shorthand declaration with generic instantiation",
                    Original = "Guid? data = new Nullable<System.Guid>();",
                    Expected = "var data = new Nullable<System.Guid>();"
                },
                new
                {
                    Name = "Nullable generic declaration with shorthand instantiation",
                    Original = "Nullable<byte> data = new byte?();",
                    Expected = "var data = new byte?();"
                },
                new
                {
                    Name = "Nullable declaration with cast",
                    Original = "byte? data = (byte?)5;",
                    Expected = "var data = (byte?)5;"
                },
                new
                {
                    Name = "Literal value type declaration",
                    Original = "int number = 5;",
                    Expected = "var number = 5;"
                },
                new
                {
                    Name = "Literal reference type declaration",
                    Original = "string data = \"asdf\";",
                    Expected = "var data = \"asdf\";"
                },
                new
                {
                    Name = "Reference type instantiation",
                    Original = "Sample sample = new Sample();",
                    Expected = "var sample = new Sample();"
                },
                new
                {
                    Name = "Spacing preserved",
                    Original = " int[]  data  = new int[] { 1, 2,  3,   4,    5 };",
                    Expected = " var  data  = new int[] { 1, 2,  3,   4,    5 };"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertEnforcementAsync(
                    Rule,
                    String.Format(MethodDeclarationWrapper, scenario.Original),
                    String.Format(MethodDeclarationWrapper, scenario.Expected));
            }
        }

        [TestMethod]
        public async Task UnchangedUsingImplicitVariableTest()
        {
            await Assertions.AssertEnforcementAsync(
                Rule,
                "class Sample { void Work() { using (System.IO.MemoryStream s = new System.IO.MemoryStream()) {} } }",
                "class Sample { void Work() { using (var s = new System.IO.MemoryStream()) {} } }");
        }

        [TestMethod]
        public async Task UnchangedAnonymousArrayTest()
        {
            await Assertions.AssertNoEnforcementAsync(
                Rule, "class Sample { void Work() { var values = new[] { new { Id = 1 } } } }");
        }

        [TestMethod]
        public async Task UnchangedDeclarationWithoutInitializationTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, "class Sample { void Work() { int value; } }");
        }

        [TestMethod]
        public async Task UnchangedExplicitConstTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, "class Sample { void Work() { const int val = 1; } }");
        }

        [TestMethod]
        public async Task UnchangedExplicitFieldTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, "class Sample { int value = 1; }");
        }

        [TestMethod]
        public async Task UnchangedForEachImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "ForEach option off",
                    Declaration = "var number in Enumerable.Range(1, 10)",
                    ApplyOptions = new Func<OptionSet, OptionSet>(o => ApplyForEachOption(o, value: false))
                },
                new
                {
                    Name = "Anonymous type",
                    Declaration = "var element in Enumerable.Range(1, 10).Select(i => new { Num = i })",
                    ApplyOptions = new Func<OptionSet, OptionSet>(o => ApplyForEachOption(o))
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertNoEnforcementAsync(
                    Rule,
                    String.Format(ForEachDeclarationWrapper, scenario.Declaration),
                    scenario.ApplyOptions);
            }
        }

        [TestMethod]
        public async Task UnchangedGenericMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Type parameter instantiation",
                    Constraint = "new()",
                    Declaration = "var value = new T();"
                },
                new
                {
                    Name = "Downcast to type constraint",
                    Constraint = "System.Collections.IEnumerable",
                    Declaration = "System.Collections.IEnumerable enumerable = this.Value;"
                },
                new
                {
                    Name = "Unknown type constraint (explicit)",
                    Constraint = "UnknownType",
                    Declaration = "UnknownType unknown = this.Value;"
                },
                new
                {
                    Name = "Unknown member access (explicit)",
                    Constraint = "class",
                    Declaration = "UnknownType unknown = this.Value.Unknown();"
                },
                new
                {
                    Name = "Unknown member access (implicit)",
                    Constraint = "class",
                    Declaration = "var unknown = this.Value.Unknown();"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertNoEnforcementAsync(
                    Rule, String.Format(GenericMethodDeclarationWrapper, scenario.Constraint, scenario.Declaration));
            }
        }

        [TestMethod]
        public async Task UnchangedMethodImplicitVariableTest()
        {
            var scenarios = new[]
            {
                new
                {
                    Name = "Generic with type keyword",
                    Declaration = "System.Collections.Generic.List<int> numbers = Enumerable.Range(1, 10).ToList();"
                },
                new
                {
                    Name = "Generic with type keyword instantiation",
                    Declaration = "var numbers = new System.Collections.Generic.List<ulong>();"
                },
                new
                {
                    Name = "Generic without type keyword instantiation",
                    Declaration = "var guids = new System.Collections.Generic.List<Guid>();"
                },
                new
                {
                    Name = "Anonymous generic type",
                    Declaration = "var anon = Enumerable.Range(1, 5).Select(i => new { Num = i });"
                },
                new
                {
                    Name = "Unknown generic type",
                    Declaration = "var unknown = Enumerable.Range(1, 5).Select(i => UnknownType.GetValue(i));"
                },
                new
                {
                    Name = "Anonymous type",
                    Declaration = "var anon = new { Num = 5 };"
                },
                new
                {
                    Name = "Nullable implicit conversion",
                    Declaration = "int? number = 5;"
                },
                new
                {
                    Name = "Boxing",
                    Declaration = "object data = \"test\";"
                },
                new
                {
                    Name = "Unknown type declaration",
                    Declaration = "var unknown = UnknownType.GetValue();"
                },
                new
                {
                    Name = "Unknown type instantiation",
                    Declaration = "var unknown = new UnknownType();"
                },
                new
                {
                    Name = "Unknown type conversion",
                    Declaration = "UnknownType1 unknown = new UnknownType2();"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertNoEnforcementAsync(
                    Rule, String.Format(MethodDeclarationWrapper, scenario.Declaration));
            }
        }

        [TestMethod]
        public async Task UnchangedUsingWithoutDeclarationTest()
        {
            const string usingMissingDeclarationWrapper =
                "class Sample {{ void Work() {{ System.IDisposable test; using ({0}) {{ }} }} }}";

            var scenarios = new[]
            {
                new
                {
                    Name = "Using identifier only",
                    Declaration = "test"
                },
                new
                {
                    Name = "Using expression only",
                    Declaration = "new System.IO.MemoryStream()"
                }
            };

            foreach (var scenario in scenarios)
            {
                Console.WriteLine("Scenario: {0}", scenario.Name);
                await Assertions.AssertNoEnforcementAsync(
                    Rule,
                    String.Format(usingMissingDeclarationWrapper, scenario.Declaration));
            }
        }
    }
}
