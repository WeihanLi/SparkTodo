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
    private static readonly bool IsInK8sClsuter = KubernetesClientConfiguration.IsInCluster();
    private volatile KubernetesEnvironment _environment = null;
    private readonly AsyncLock _lock = new();

    public async Task<KubernetesEnvironment> GetKubernetesEnvironment()
    {
        if (_environment is null)
        {
            using (await _lock.LockAsync())
            {
                if (_environment is null)
                {
                    var host = Environment.MachineName;
                    if (IsInK8sClsuter)
                    {
                        var config = KubernetesClientConfiguration.InClusterConfig();
                        _environment = new KubernetesEnvironment()
                        {
                            Namespace = config.Namespace,
                            PodName = host,
                        };

                        var client = new Kubernetes(config);
                        var podListResult = await client.ListNamespacedPodAsync(config.Namespace);
                        var podInfo = podListResult.Items.First(x => x.Metadata.Name == host);
                        _environment.PodInfo = podInfo;

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
                                    _environment.DeploymentName = item.Name();
                                    _environment.DeploymentInfo = item;
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
                            if (item.Spec.Selector.Count > 0)
                            {
                                var labelMatch = item.Spec.Selector
                                    .All(p => podInfo.GetLabel(p.Key) == p.Value);
                                if (labelMatch)
                                {
                                    _environment.ServiceName = item.Name();
                                    _environment.ServiceInfo = item;
                                    break;
                                }
                            }
                            else
                            {
                                // TODO: match pod according to Selector.MatchExpressions
                            }
                        }
                    }
                    else
                    {
                        _environment = new KubernetesEnvironment() { PodName = host };
                    }
                }
            }
        }
        return _environment;
    }
}
