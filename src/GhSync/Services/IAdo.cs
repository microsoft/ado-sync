// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Octokit;

namespace Microsoft.GhSync;

public interface IAdo
{
    Task<VssConnection> GetAdoConnection(string ADOTokenName, VssConnection? adoConnection);
    Task<TResult> WithWorkItemClient<TResult>(Func<WorkItemTrackingHttpClient, Task<TResult>> continuation);

    IAsyncEnumerable<Comment> EnumerateComments(WorkItem workItem);


    Task<WorkItem> UpdateFromIssue(WorkItem workItem, Issue? issue);

    Task<WorkItem?> GetAdoWorkItem(Issue? issue);
}
