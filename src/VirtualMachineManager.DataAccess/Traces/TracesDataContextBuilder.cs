using System.IO;
using Z.EntityFramework.Extensions;

namespace VirtualMachineManager.DataAccess.Traces
{
    public class TracesDataContextBuilder
    {
        private string _dbPath;
        private string _tracesPath;

        public TracesDataContextBuilder WithDbFilePath(string path)
        {
            _dbPath = path;
            return this;
        }

        public TracesDataContextBuilder WithInputTracesPath(string path)
        {
            _tracesPath = path;
            return this;
        }

        public TracesDataContext Build()
        {
            EntityFrameworkManager.ContextFactory = context => context;
            if (File.Exists(_dbPath))
            {
                return new TracesDataContext(_dbPath);
            }

            var dbContext = new TracesDataContext(_dbPath);
            dbContext.Database.EnsureCreated();
            new TracesParser(dbContext, _tracesPath).ParseData();
            return dbContext;
        }
    }
}
