// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SparkTodo_API>("sparktodo-api");

builder.Build().Run();
