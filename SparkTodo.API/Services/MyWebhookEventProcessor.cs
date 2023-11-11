// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Octokit.Webhooks;
using Octokit.Webhooks.Events;

namespace SparkTodo.API.Services;

public sealed class MyWebhookEventProcessor(ILogger<MyWebhookEventProcessor> logger) : WebhookEventProcessor
{
    protected override Task ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent)
    {
        var (repoName, commitId) = (pushEvent.Repository?.FullName, pushEvent.HeadCommit?.Id);
        var (name, email) = (pushEvent.Pusher.Name, pushEvent.Pusher.Email);
        logger.LogInformation("Push event received {RepoName} {CommitId} {PushByName} {PushByEmail}",
            repoName, commitId, name, email);
        return base.ProcessPushWebhookAsync(headers, pushEvent);
    }
}
