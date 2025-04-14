// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposePublisher();

var db = builder.AddSqlServer("db")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("TodoApp")
    ;

builder.AddProject<Projects.SparkTodo_API>("sparktodo-api")
    .WithReference(db)
    .WaitFor(db)
    ;

builder.Build().Run();
