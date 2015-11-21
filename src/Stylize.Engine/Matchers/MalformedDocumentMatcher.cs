// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stylize.Engine.Matchers
{
    [ExportDocumentMatcher(Name)]
    public class MalformedDocumentMatcher : IDocumentMatcher
    {
        const string Name = "MalformedDocumentMatcher";

        public async Task<bool> IsMatchAsync(Document document)
        {
            SemanticModel semanticModel = await document.GetSemanticModelAsync();
            return semanticModel.GetDiagnostics().Any(d => d.DefaultSeverity == DiagnosticSeverity.Error);
        }
    }
}
