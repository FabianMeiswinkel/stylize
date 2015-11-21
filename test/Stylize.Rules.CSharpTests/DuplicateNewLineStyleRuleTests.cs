// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class DuplicateNewLineStyleRuleTests
    {
        const string ExpectedText = @"
using System;

using System.Linq;

namespace Test
{

    class Sample

    {

        #region Regions are a bit weird because they are structured trivia (trivia which contains trivia)

        #endregion

        // Method: Work

        public void Work()
        {

            foreach (char c in ""hi"")
            {

                // Comment!

            }

        }

    }

}";

        const string OriginalText = @"
using System;


using System.Linq;


namespace Test
{


    class Sample


    {


        #region Regions are a bit weird because they are structured trivia (trivia which contains trivia)


        #endregion


        // Method: Work


        public void Work()
        {


            foreach (char c in ""hi"")
            {


                // Comment!


            }


        }


    }


}";

        static readonly IStyleRule Rule = new DuplicateNewLineStyleRule();

        [TestMethod]
        public async Task DuplicateNewLinesRemovedTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, OriginalText, ExpectedText);
        }

        [TestMethod]
        public async Task NoDuplicateNewLinesTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, ExpectedText);
        }
    }
}
