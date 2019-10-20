using VirtualMachineManager.App.Services;
using VirtualMachineManager.DataAccess.Traces;

namespace VirtualMachineManager.App
{
    class Program
    {
        static void Main()
        {
            /*await new HostBuilder()
                .ConfigureAppConfiguration((configBuilder) =>
                {
                    //configBuilder.
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //services
                })
                .RunConsoleAsync();*/

            var paramsManager = new ParametersManager("Settings.ini");

            using (var dbContext = new TracesDataContextBuilder()
                .WithDbFilePath("data\\traces.db")
                .WithInputTracesPath("input")
                .Build())
            {
            }
        }
    }
}
