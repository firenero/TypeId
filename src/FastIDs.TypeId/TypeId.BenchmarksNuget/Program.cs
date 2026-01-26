using System.Reflection;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

var config = DefaultConfig.Instance
    .WithOptions(ConfigOptions.JoinSummary)
    .HideColumns("Arguments", "RatioSD", "Alloc Ratio")
    .AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByMethod);

string[] typeIdNugetVersions =
[
    "1.3.0-alpha.0.1",
    "1.2.1",
    "1.1.0",
    "1.0.0"
];

for (var i = 0; i < typeIdNugetVersions.Length; i++)
{
    var version = typeIdNugetVersions[i];
    config = config.AddJob(Job.Default.WithMsBuildArguments($"/p:SciVersion={version}").WithId(version).WithBaseline(i == 0));
}

BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(config: config, args: args);
