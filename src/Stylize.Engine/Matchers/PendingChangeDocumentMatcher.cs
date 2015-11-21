// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using Stylize.Engine.Repositories;

namespace Stylize.Engine.Matchers
{
    [ExportDocumentMatcher(Name)]
    public class PendingChangeDocumentMatcher : IDocumentMatcher
    {
        public const string Name = "PendingChangeDocumentMatcher";

        readonly ISourceRepository repository;

        [ImportingConstructor]
        public PendingChangeDocumentMatcher(
            [Import(ExportSourceRepositoryAttribute.CurrentName)] ISourceRepository repository)
        {
            this.repository = repository;
        }

        [ExportStylizeOption]
        public static Option<bool> MatchWhenChanged { get; } = new Option<bool>(Name, "matchWhenChanged");

        public async Task<bool> IsMatchAsync(Document document)
        {
            bool hasChange = await this.repository.HasPendingChangeAsync(document);
            return hasChange == document.GetOption(MatchWhenChanged);
        }
    }
}
