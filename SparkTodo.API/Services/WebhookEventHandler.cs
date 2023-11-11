// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Octokit.Webhooks;
using Octokit.Webhooks.Events;

namespace SparkTodo.API.Services;

public sealed class MyWebhookEventProcessor(ILogger<MyWebhookEventProcessor> logger) : WebhookEventProcessor
{
    protected override Task ProcessPushWebhookAsync(WebhookHeaders headers, PushEvent pushEvent)
    {
        var (repoName, repoHomepage, commitId) = (pushEvent.Repository?.Name, pushEvent.Repository?.Homepage,
            pushEvent.HeadCommit?.Id);
        var (name, email) = (pushEvent.Pusher.Name, pushEvent.Pusher.Email);
        logger.LogInformation("Push event received {RepoName} {RepoHomepage} {CommitId} {PushByName} {PushByEmail}",
            repoName, repoHomepage, commitId, name, email);
        return base.ProcessPushWebhookAsync(headers, pushEvent);
    }
}
