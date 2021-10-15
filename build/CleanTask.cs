using Cake.Common.IO;
using Cake.Core.Diagnostics;
using Cake.Frosting;

namespace Build
{
    [TaskName("Clean")]
    public sealed class CleanTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            context.Log.Information("Cleaning ../src/**/bin" + context.Config);
            context.CleanDirectories("../src/**/bin" + context.Config);
            //var files = context.GetFiles("../src/");
        }
    }

}