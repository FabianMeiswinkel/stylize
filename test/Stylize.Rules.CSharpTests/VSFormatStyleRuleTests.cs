// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stylize.Engine.Rules;
using Stylize.Rules.CSharp;

namespace Stylize.Rules.CSharpTests
{
    [TestClass]
    public class VSFormatStyleRuleTests
    {
        const string FormattedText = @"
using System;

namespace Test
{
    class Sample
    {
        public void Work<T>(T value)
        {
            uint castValue = (uint)((object)value);
            try
            {
                foreach (char c in ""test"") { Console.WriteLine(c); }
            }
            catch (Exception)
            {
            }
        }
    }
}
";

        static readonly IStyleRule Rule = new VSFormatStyleRule();

        const string UnformattedText = @"
using System;

namespace Test
{
	class Sample
	{
		public void Work < T > ( T value )
		{
			uint castValue = ( uint ) ( (object)   value );
			try
			{
				foreach( char c in ""test"" ){Console.WriteLine( c );}
			}
			catch (	Exception	)
			{
			}
		}
	}
}
";

        [TestMethod]
        public async Task ApplyVisualStudioFormattingTest()
        {
            await Assertions.AssertEnforcementAsync(Rule, UnformattedText, FormattedText);
        }

        [TestMethod]
        public async Task UnchangedAlreadyFormattedTest()
        {
            await Assertions.AssertNoEnforcementAsync(Rule, FormattedText);
        }
    }
}
