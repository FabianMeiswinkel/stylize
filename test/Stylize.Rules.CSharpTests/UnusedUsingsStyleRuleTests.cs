// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class UnusedUsingsStyleRuleTests
    {
        const string EmptyUsings = @"
// Header comment

namespace Test
{
    class Sample { }
}
";

        const string FullyUsedUsings = @"
// Header comment

using System;
using static System.Math;
using F2Double = System.Func<double, double>;

namespace Test
{
    class Sample
    {
        public void Work()
        {
            F2Double fun = value => value + 1;
            Console.WriteLine(fun(PI));
        }
    }
}
";

        const string PartiallyUsedUsings = @"
// Header comment

using System;
using System.Linq;
using static System.Math;
using static System.Environment;
using Ex = System.Exception;
using F1Double = System.Func<double>;
using F2Double = System.Func<double, double>;

namespace Test
{
    class Sample
    {
        public void Work()
        {
            F2Double fun = value => value + 1;
            Console.WriteLine(fun(PI));
        }
    }
}
";

        static readonly IStyleRule Rule = new UnusedUsingsStyleRule();

        const string UnusedUsings = @"
// Header comment

using System;
using System.Linq;
using static System.Math;
using static System.Environment;
using Ex = System.Exception;
using F1Double = System.Func<double>;
using F2Double = System.Func<double, double>;

namespace Test
{
    class Sample { }
}
";

        [TestMethod]
        public async Task PartiallyUsedUsingsCleanedTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, PartiallyUsedUsings, FullyUsedUsings);
        }

        [TestMethod]
        public async Task UnchangedEmptyUnusedUsingsTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, EmptyUsings);
        }

        [TestMethod]
        public async Task UnchangedFullyUsedUsingsTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, FullyUsedUsings);
        }

        [TestMethod]
        public async Task UnusedUsingsCleanedTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, UnusedUsings, EmptyUsings);
        }
    }
}
