using RDotNet;
using System;

namespace VirtualMachineManager.Prognosing.Algorythms
{
    public static class RGlobalEnvironment
    {
        public static REngine R { get; private set; }

        public static void InitREngineWithForecasing(string packageLibPath)
        {
            R = REngine.GetInstance();

            // Add package lib path
            R.Evaluate($".libPaths(c('{packageLibPath}', .libPaths()))");

            InstallRPackages("foreach", "doParallel", "forecast");

            // Register parallel R cluster and load required packages
            R.Evaluate(
                $"myCluster = makeCluster({Environment.ProcessorCount})\n" +
                $"registerDoParallel(myCluster)\n" +
                @"clusterCall(myCluster, function() {
                    .libPaths(c('" + packageLibPath + @"', .libPaths()))
                    library('forecast')
                })"
                );

        }

        private static void InstallRPackages(params string[] packages)
        {
            foreach (var package in packages)
            {
                R.Evaluate($"if (!require('{package}')) install.packages('{package}', repos='https://cloud.r-project.org/')");
            }
        }
    }
}
