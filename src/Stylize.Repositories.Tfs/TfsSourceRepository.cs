// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Stylize.Engine;
using Stylize.Engine.Repositories;

namespace Stylize.Repositories.Tfs
{
    [ExportSourceRepository(Name)]
    public class TfsSourceRepository : ISourceRepository
    {
        public const string Name = "TfsSourceRepository";

        readonly IDictionary<string, CachedWorkspace> solutionWorkspaceMap =
            new Dictionary<string, CachedWorkspace>(StringComparer.OrdinalIgnoreCase);

        public Task CheckOutAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            this.GetTfsWorkspace(document).CheckOutFile(document.FilePath);

            return Task.CompletedTask;
        }

        CachedWorkspace GetTfsWorkspace(Document document)
        {
            return this.solutionWorkspaceMap.GetOrAdd(
                document.Project.Solution.FilePath,
                solutionFilePath => new CachedWorkspace(solutionFilePath));
        }

        public Task<bool> HasPendingChangeAsync(Document document)
        {
            if (document == null) { throw new ArgumentNullException(nameof(document)); }

            bool hasPendingChange = this.GetTfsWorkspace(document).FileHasPendingChange(document.FilePath);

            return Task.FromResult(hasPendingChange);
        }
    }
}
