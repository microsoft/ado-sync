// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.GhSync.Tests;

using Octokit;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
using System.Collections.Generic;

internal static class MockServiceExtensions
{
    public static IServiceCollection AddMock<T>(this IServiceCollection services, Action<Mock<T>>? configure = null)
    where T: class
    {
        var mock = new Mock<T>();
        configure?.Invoke(mock);
        services.AddSingleton<T>(mock.Object);
        return services;
    }
}

public class MockStartup
{
    private Issue testIssue = new Issue();
    private WorkItem testWorkItem = new()
    {
        Url = "https://mock.visualstudio.com",
        Id = 12345,
        Links = new()
    };
    private string? nullStr = null;
    private readonly Lazy<IServiceProvider> services;
    public IServiceProvider Services => services.Value;

    public MockStartup()
    {
        this.services = new Lazy<IServiceProvider>(() =>
        {
            var serviceCollection = new ServiceCollection();
            this.ConfigureServices(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        });
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMock<IOptions>(mock =>
        {
            // GetToken()
            mock
                .Setup(arg => arg.GetToken(It.Is<string>(varName =>
                    varName == "bad-token"
                )))
                .Returns("");
            mock
                .Setup(arg => arg.GetToken(It.Is<string>(varName =>
                    varName == "unauthorized-token"
                )))
                .Returns("unauthorized-token-value");
            mock
                .Setup(arg => arg.GetToken(It.Is<string>(varName =>
                    varName == "valid-token"
                )))
                .Returns("valid-token-value");

            // GetOrgProfile()
            mock
                .Setup(arg => arg.GetOrgProfile(
                    It.Is<GitHubClient>(ghClient =>
                        ghClient.Credentials.Password == "invalid-token-value"
                    ), 
                    It.IsAny<string>()
                ))
                .Returns(Task.FromResult<Organization?>(null));
            mock
                .Setup(arg => arg.GetOrgProfile(
                    It.Is<GitHubClient>(ghClient =>
                        ghClient.Credentials.Password == "valid-token-value"
                    ), 
                    It.IsAny<string>()
                ))
                .Returns(Task.FromResult<Organization?>(new Organization()));

            // GetAdoConnection()
            mock
                .Setup(arg => arg.GetVssConnection(
                    It.Is<string>(adoToken =>
                        adoToken == ""
                    )
                ))
                .Throws<NullReferenceException>();
        });
        services.AddMock<IAdo>(mock =>
        {
            // UpdateFromIssue()
            mock
                .Setup(arg => arg.UpdateFromIssue(It.IsAny<WorkItem>(), It.IsAny<Issue?>()))
                .Returns(Task.FromResult(new WorkItem()));

            // GetAdoWorkItem()
            mock
                .Setup(arg => arg.GetAdoWorkItem(It.Is<Issue>(issue => 
                    issue.Title == "PullGitHubIssueDryRunWorksWhenWorkItemExists" ||
                    issue.Title == "PullGitHubIssueDryRunAllowExistingWhenWorkItemExists"
                )))
                .Returns(
                    Task.FromResult<WorkItem?>(testWorkItem)
                );
            mock
                .Setup(arg => arg.GetAdoWorkItem(It.Is<Issue>(issue => 
                    issue.Title == "PullGitHubIssueDryRunWorksWhenItemDoesNotExist" ||
                    issue.Title == "PullGitHubIssueDryRunAllowExistingWhenWorkItemDoesNotExist" ||
                    issue.Title == "PullGitHubIssueWhenWorkItemDoesNotExist"
                )))
                .Returns(
                    Task.FromResult<WorkItem?>(null)
                );
        });
        services.AddMock<IGitHub>(

        );
        services.AddMock<ISynchronizer>(mock =>
        {
            // PullWorkItemFromIssue()
            mock
                .Setup(arg => arg.PullWorkItemFromIssue(It.Is<Issue>(issue =>
                    issue.Title == "PullGitHubIssueWhenWorkItemDoesNotExist"
                )))
                .Returns(
                    Task.FromResult<WorkItem>(testWorkItem)
                );

            // UpdateState()
            mock
                .Setup(arg => arg.UpdateState(It.IsAny<WorkItem>(), It.Is<Issue>(issue =>
                    issue.Title == "PullGitHubIssueWhenWorkItemDoesNotExist"
                )))
                .Returns(
                    Task.FromResult<WorkItem>(testWorkItem)
                );
        }
        );
    }

    private Issue newIssue(string title = "") =>
        new Issue(
            "",
            "",
            "",
            "",
            number: 123456,
            ItemState.Open,
            title: title,
            body: "",
            closedBy: null,
            user: null,
            labels: new List<Label>().AsReadOnly(),
            assignee: null,
            assignees: new List<User>().AsReadOnly(),
            milestone: null,
            comments: 12,
            pullRequest: null,
            closedAt: null,
            createdAt: DateTimeOffset.Now,
            updatedAt: DateTimeOffset.Now,
            id: 1234567,
            nodeId: "",
            locked: false,
            repository: new Repository(),
            reactions: null
        );
}
