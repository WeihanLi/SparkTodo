// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using k8s;
using k8s.Models;

namespace SparkTodo.API.Services;

public class KubernetesEnvironment
{
    public string Namespace { get; set; }
    public string PodName { get; set; }
    public V1Pod PodInfo { get; set; }
    public PodMetricsList PodMetrics { get; set; }
    public string DeploymentName { get; set; }
    public V1Deployment DeploymentInfo { get; set; }
    public string ServiceName { get; set; }
    public V1Service ServiceInfo { get; set; }
}

public interface IKubernetesService
{
    Task<KubernetesEnvironment> GetKubernetesEnvironment();
}

public class KubernetesService : IKubernetesService
{
    private readonly ILogger<KubernetesService> _logger;
    private static readonly bool IsInK8SCluster = KubernetesClientConfiguration.IsInCluster();
    private volatile KubernetesEnvironment _environment;
    private readonly AsyncLock _lock = new();

    public KubernetesService(ILogger<KubernetesService> logger)
    {
        _logger = logger;
    }

    public async Task<KubernetesEnvironment> GetKubernetesEnvironment()
    {
        if (_environment is null)
        {
            using var locker = await _lock.LockAsync();
            if (_environment is not null)
                return _environment;

            var host = Environment.MachineName;
            if (IsInK8SCluster)
            {
                var config = KubernetesClientConfiguration.InClusterConfig();
                var environment = new KubernetesEnvironment()
                {
                    Namespace = config.Namespace,
                    PodName = host,
                };

                var client = new Kubernetes(config);
                try
                {
                    environment.PodMetrics = await client.GetKubernetesPodsMetricsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetPodMetrics Exception");
                }

                try
                {
                    var podListResult = await client.ListNamespacedPodAsync(config.Namespace);
                    var podInfo = podListResult.Items.First(x => x.Name() == host);
                    environment.PodInfo = podInfo;

                    // deployment
                    var deploymentListResult = await client.ListNamespacedDeploymentAsync(config.Namespace);
                    foreach (var item in deploymentListResult.Items)
                    {
                        if (item.Spec.Selector.MatchLabels.Count > 0)
                        {
                            var labelMatch = item.Spec.Selector.MatchLabels
                                .All(p => podInfo.GetLabel(p.Key) == p.Value);
                            if (labelMatch)
                            {
                                environment.DeploymentName = item.Name();
                                environment.DeploymentInfo = item;
                                break;
                            }
                        }
                        else
                        {
                            // TODO: match pod according to Selector.MatchExpressions
                        }
                    }

                    // service
                    var serviceListResult = await client.ListNamespacedServiceAsync(config.Namespace);
                    foreach (var item in serviceListResult.Items)
                    {
                        Console.WriteLine("Service: {item.Name()}");
                        var labelMatch = item.Spec.Selector
                            .All(p => podInfo.GetLabel(p.Key) == p.Value);
                        if (labelMatch)
                        {
                            environment.ServiceName = item.Name();
                            environment.ServiceInfo = item;
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Get Pod Deployment/Service exception");
                }

                _environment = environment;
            }
            else
            {
                _environment = new KubernetesEnvironment()
                {
                    PodName = host
                };
            }
        }
        return _environment;
    }
}
