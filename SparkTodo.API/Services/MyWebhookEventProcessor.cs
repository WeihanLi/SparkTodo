// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Octokit.Webhooks;
using Octokit.Webhooks.Events;

namespace SparkTodo.API.Services;

public sealed class MyWebhookEventProcessor(ILogger<MyWebhookEventProcessor> logger) : WebhookEventProcessor
{
    protected override ValueTask ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent, CancellationToken cancellationToken = default)
    {
        var (repoName, commitId, commitMsg) = (pushEvent.Repository?.FullName, pushEvent.HeadCommit?.Id, pushEvent.HeadCommit?.Message);
        var (name, email) = (pushEvent.Pusher.Name, pushEvent.Pusher.Email);
        if (string.IsNullOrEmpty(commitMsg)
            || commitMsg.IndexOf("skip-ci", StringComparison.OrdinalIgnoreCase) > -1
            || commitMsg.IndexOf("skip-cd", StringComparison.OrdinalIgnoreCase) > -1
            || commitMsg.IndexOf("cd-skip", StringComparison.OrdinalIgnoreCase) > -1
           )
        {
            return ValueTask.CompletedTask;
        }

        logger.LogInformation("Push event received {RepoName} {CommitId} {CommitMsg} {PushByName} {PushByEmail}",
            repoName, commitId, commitMsg, name, email);
        // process push event

        return base.ProcessPushWebhookAsync(headers, pushEvent);
    }
}
