// Copyright (c) 2015.  See LICENSE in the repository root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Stylize.Engine.Repositories
{
    public interface ISourceRepository
    {
        Task CheckOutAsync(Document document);

        Task<bool> HasPendingChangeAsync(Document document);
    }
}
