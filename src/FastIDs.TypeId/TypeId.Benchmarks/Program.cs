using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance;
// config.AddJob(Job.Default.WithId("Scalar").WithEnvironmentVariable("DOTNET_EnableHWIntrinsic", "0").AsBaseline())
//     .AddJob(Job.Default.WithId("Vector128").WithEnvironmentVariable("DOTNET_EnableAVX512F", "0").WithEnvironmentVariable("DOTNET_EnableAVX2", "0"))
//     .AddJob(Job.Default.WithId("Vector256").WithEnvironmentVariable("DOTNET_EnableAVX512F", "0"))
//     .AddJob(Job.Default.WithId("Vector512"));

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(config: config);