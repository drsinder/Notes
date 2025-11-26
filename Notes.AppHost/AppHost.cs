var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Notes>("notes");

builder.Build().Run();
