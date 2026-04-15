using Amazon;
using Aspire.Hosting.LocalStack.Container;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var gateway = builder.AddProject<Projects.Api_Gateway>("api-gateway");

var awsConfig = builder.AddAWSSDKConfig()
    .WithProfile("default")
    .WithRegion(RegionEndpoint.EUCentral1);

var localstack = builder
    .AddLocalStack("creditapp-localstack", awsConfig: awsConfig, configureContainer: container =>
    {
        container.Lifetime = ContainerLifetime.Session;
        container.DebugLevel = 1;
        container.LogLevel = LocalStackLogLevel.Debug;
        container.Port = 4566;
        container.AdditionalEnvironmentVariables
            .Add("DEBUG", "1");
        container.AdditionalEnvironmentVariables
            .Add("SNS_CERT_URL_HOST", "sns.eu-central-1.amazonaws.com");
    });

var awsResources = builder
    .AddAWSCloudFormationTemplate("resources", "CloudFormation/creditapp-template-sns.yaml", "creditapp")
    .WithReference(awsConfig)
    .WaitFor(localstack!);

for (var i = 0; i < 3; i++)
{
    var service = builder.AddProject<Projects.CreditApp_Api>($"credit-app-{i}", launchProfileName: null)
        .WithReference(redis)
        .WaitFor(redis)
        .WithReference(awsResources)
        .WithEnvironment("Settings__MessageBroker", "SNS")
        .WaitFor(awsResources)
        .WithHttpsEndpoint(port: 8000 + i);

    gateway.WithReference(service).WaitFor(service);
}

builder.AddProject<Projects.Client_Wasm>("client-wasm")
    .WaitFor(gateway);

var minio = builder.AddMinioContainer("creditapp-minio");

var sink = builder.AddProject<Projects.Service_FileStorage>("service-filestorage")
    .WithReference(awsResources)
    .WithEnvironment("Settings__MessageBroker", "SNS")
    .WithEnvironment("AWS__Resources__SNSUrl", "http://host.docker.internal:5122/api/sns")
    .WithEnvironment("AWS__Resources__MinioBucketName", "creditapp-bucket")
    .WithReference(minio)
    .WaitFor(minio)
    .WaitFor(awsResources);

builder.UseLocalStack(localstack);

builder.Build().Run();
