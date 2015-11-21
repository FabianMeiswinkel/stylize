// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class OverQualifiedNameStyleRuleTests
    {
        const string FullyQualifiedText = @"
namespace Test
{
    public class Sample
    {
        public void DoWork()
        {
            System.Collections.IEnumerable guids = this.DoSubWork();
        }

        public System.Collections.Generic.IReadOnlyList<System.Guid> DoSubWork()
        {
            return new[]
            {
                System.Guid.NewGuid(),
                System.Guid.NewGuid()
            };
        }
    }
}
";

        static readonly IStyleRule Rule = new OverQualifiedNameStyleRule();

        const string SimplyQualifiedText = @"using System;
using System.Collections;
using System.Collections.Generic;

namespace Test
{
    public class Sample
    {
        public void DoWork()
        {
            IEnumerable guids = this.DoSubWork();
        }

        public IReadOnlyList<Guid> DoSubWork()
        {
            return new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            };
        }
    }
}
";

        const string UnusedAliasTest = @"
using GuidAlias = System.Guid;

namespace Test
{
    class Sample
    {
        void DoWork()
        {
            System.Guid.NewGuid();
        }
    }
}
";

        const string UsedAliasTest = @"
using GuidAlias = System.Guid;

namespace Test
{
    class Sample
    {
        void DoWork()
        {
            GuidAlias.NewGuid();
        }
    }
}
";

        [TestMethod]
        public async Task FullyQualifiedNamesAreSimplifiedTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, FullyQualifiedText, SimplyQualifiedText);
        }

        [TestMethod]
        public async Task UnchangedAlreadySimplifiedQualifiedNamesTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, SimplyQualifiedText);
        }

        [TestMethod]
        public async Task UnchangedAlreadyUsedAliasTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, UsedAliasTest);
        }

        [TestMethod]
        public async Task UnusedAliasesAreSimplifiedTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, UnusedAliasTest, UsedAliasTest);
        }
    }
}
