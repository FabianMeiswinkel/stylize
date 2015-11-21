// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stylize.Engine.Matchers
{
    public interface IDocumentMatcher
    {
        Task<bool> IsMatchAsync(Document document);
    }
}
