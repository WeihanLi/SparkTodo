// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

namespace SparkTodo.Models.Configs;

public sealed class OpenTelemetryConfig
{
    public required string ServiceName { get; set; }
    public required string ServiceVersion { get; set; }
}
