using System.Threading.Tasks;
using Build;
using Cake.Common.IO;
using Cake.Common.IO.Paths;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Tools.DotNetCore.Test;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;

public static class Program
{
    public static int Main(string[] args)
    {
        return new CakeHost()
            .UseContext<BuildContext>()
            .Run(args);
    }
}

public class BuildContext : FrostingContext
{
    public ConvertableFilePath AppTestsCsProj { get; set; }
    public bool Delay { get; set; }
    public string Config { get; set; }
    public BuildContext(ICakeContext context)
        : base(context)
    {
        var testDir = context.Directory("../test");
        var appTestDir = testDir + context.Directory("SolarEV.Tests");
        AppTestsCsProj = appTestDir + context.File("SolarEV.Tests.csproj");

        Config = context.Arguments.GetArgument("Configuration");
    }
}

[TaskName("BuildTask")]
[IsDependentOn(typeof(CleanTask))]
public sealed class BuildTask : FrostingTask<BuildContext>
{
    // Tasks can be asynchronous
    public override void Run(BuildContext context)
    {
        context.DotNetCoreBuild("../SolarEV.sln",
            new DotNetCoreBuildSettings{ Configuration = context.Config });

        context.Log.Information("World");
    }
}

[TaskName("TestTask")]
[IsDependentOn(typeof(BuildTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
    // Tasks can be asynchronous
    public override void Run(BuildContext context)
    {
        var csproj = "../test/SolarEV.Tests/SolarEV.Tests.csproj";
        context.DotNetCoreTest(csproj,
            new DotNetCoreTestSettings { Configuration = context.Config });

        context.Log.Information("World");
    }
}

[TaskName("Default")]
[IsDependentOn(typeof(BuildTask))]
public class DefaultTask : FrostingTask
{
}