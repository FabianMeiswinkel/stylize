// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Stylize.Repositories.Tfs
{
    // TODO: Offline implementation
    class CachedWorkspace
    {
        readonly HashSet<string> changedFilePaths;
        readonly Workspace workspace;

        public CachedWorkspace(string solutionFilePath)
        {
            WorkspaceInfo workspaceInfo = Workstation.Current.GetLocalWorkspaceInfo(solutionFilePath);
            var server = new TfsTeamProjectCollection(workspaceInfo.ServerUri);
            this.workspace = workspaceInfo.GetWorkspace(server);

            this.changedFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (PendingChange pendingChange in this.workspace.GetPendingChangesEnumerable())
            {
                if (!String.IsNullOrEmpty(pendingChange.LocalItem) &&
                    (pendingChange.IsAdd || pendingChange.IsEdit))
                {
                    this.changedFilePaths.Add(Path.GetFullPath(pendingChange.LocalItem));
                }
            }
        }

        public void CheckOutFile(string filePath)
        {
            this.workspace.PendEdit(filePath);
        }

        public bool FileHasPendingChange(string filePath)
        {
            return this.changedFilePaths.Contains(Path.GetFullPath(filePath));
        }
    }
}
