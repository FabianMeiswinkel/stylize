// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stylize.Engine.Rules
{
    public interface IStyleRule
    {
        /// <summary>
        /// Enforce this rule upon the specified document and return the resulting solution.  Note that the enforcement
        /// of some rules may result in cascading changes to other documents (e.g., renaming a symbol).  These types of
        /// rules are supported, but per convention the "source" of the change should be located in the specified
        /// document (e.g., the symbol definition).
        /// </summary>
        /// <param name="document">Document to be styled</param>
        /// <returns>Resulting solution</returns>
        Task<Solution> EnforceAsync(Document document);
    }
}
