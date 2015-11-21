// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class OrderedUsingsStyleRuleTests
    {
        const string DefaultOrderedUsings = @"
// Header Comment Trivia

using Custom.Stuff;
using Custom.Stuff.Inner;
using Custom.StuffExtended;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Test.Stuff;
using static Custom.CustomStatic;
using static System.Convert;
using static System.Math;
using static Test.RandomStatic;
using CustomAlias = Custom.Stuff.AliasType;
using En = System.Collections.Enumerable;
using Ex = System.Exception;

namespace Sample { }
";

        static readonly IStyleRule Rule = new OrderedUsingsStyleRule();

        const string SingleUsing = @"
// Header Comment Trivia

using System;

namespace Sample { }";

        const string SystemFirstOrderedUsings = @"
// Header Comment Trivia

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Custom.Stuff;
using Custom.Stuff.Inner;
using Custom.StuffExtended;
using Test.Stuff;
using static System.Convert;
using static System.Math;
using static Custom.CustomStatic;
using static Test.RandomStatic;
using CustomAlias = Custom.Stuff.AliasType;
using En = System.Collections.Enumerable;
using Ex = System.Exception;

namespace Sample { }
";

        const string UnorderedUsings = @"
// Header Comment Trivia

using System.Linq;
using static Test.RandomStatic;
using System;
using Ex = System.Exception;
using static System.Math;
using System.Collections.Generic;
using Test.Stuff;
using System.Collections;
using En = System.Collections.Enumerable;
using Custom.Stuff.Inner;
using Custom.Stuff;
using Custom.StuffExtended;
using CustomAlias = Custom.Stuff.AliasType;
using static System.Convert;
using static Custom.CustomStatic;

namespace Sample { }
";

        static OptionSet ApplyOptions(OptionSet options, bool systemUsingsFirst)
        {
            return options.WithChangedOption(OrderedUsingsStyleRule.SystemUsingsFirst, value: systemUsingsFirst);
        }

        [TestMethod]
        public async Task SingleUsingOrderedTest()
        {
            await Assertions.AssertNoEnforcementAsync(
                Rule, SingleUsing, o => ApplyOptions(o, systemUsingsFirst: false));
        }

        [TestMethod]
        public async Task SingleUsingOrderedWithSystemFirstTest()
        {
            await Assertions.AssertNoEnforcementAsync(
                Rule, SingleUsing, o => ApplyOptions(o, systemUsingsFirst: true));
        }

        [TestMethod]
        public async Task UsingsAlreadyOrderedTest()
        {
            await Assertions.AssertNoEnforcementAsync(
                Rule, DefaultOrderedUsings, o => ApplyOptions(o, systemUsingsFirst: false));
        }

        [TestMethod]
        public async Task UsingsAlreadyOrderedWithSystemFirstTest()
        {
            await Assertions.AssertNoEnforcementAsync(
                Rule, SystemFirstOrderedUsings, o => ApplyOptions(o, systemUsingsFirst: true));
        }

        [TestMethod]
        public async Task UsingsOrderedTest()
        {
            await Assertions.AssertEnforcementAsync(
                Rule,
                UnorderedUsings,
                DefaultOrderedUsings,
                o => ApplyOptions(o, systemUsingsFirst: false));
        }

        [TestMethod]
        public async Task UsingsOrderedWithSystemFirstTest()
        {
            await Assertions.AssertEnforcementAsync(
                Rule,
                UnorderedUsings,
                SystemFirstOrderedUsings,
                o => ApplyOptions(o, systemUsingsFirst: true));
        }
    }
}
