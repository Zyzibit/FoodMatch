var builder = DistributedApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.EnvironmentName == "Development";

// PostgreSQL Database
var postgresBuilder = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithHostPort(5433)
    .WithLifetime(ContainerLifetime.Persistent);

if (isDevelopment)
{
    postgresBuilder.WithPgAdmin();
}

var postgres = postgresBuilder;
var foodmatchDb = postgres.AddDatabase("foodmatch");

// Redis Cache
var redisBuilder = builder.AddRedis("redis")
    .WithDataVolume()
    .WithHostPort(6379)
    .WithLifetime(ContainerLifetime.Persistent);

if (isDevelopment)
{
    redisBuilder.WithRedisCommander();
}

var redis = redisBuilder;

// Backend API - używa profilu "https" z launchSettings.json
var backend = builder.AddProject<Projects.inzynierka>("backend", launchProfileName: "https")
    .WithReference(foodmatchDb)
    .WithReference(redis);

builder.AddNpmApp("frontend", "../frontend", isDevelopment ? "dev" : "build")
    .WithHttpEndpoint(port: 5173, isProxied: false)
    .WithEnvironment("VITE_API_BASE_URL", backend.GetEndpoint("https"));

builder.Build().Run();
