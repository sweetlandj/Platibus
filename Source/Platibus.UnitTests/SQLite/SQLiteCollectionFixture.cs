using System;
using System.IO;
using NUnit.Framework;

namespace Platibus.UnitTests.SQLite
{
    [SetUpFixture]
    public class SQLiteCollectionFixture
    {
        public static SQLiteCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new SQLiteCollectionFixture();
        }

        private readonly DirectoryInfo _baseDirectory;

        public DirectoryInfo BaseDirectory { get { return _baseDirectory; } }

        public SQLiteCollectionFixture()
        {
            _baseDirectory = GetTempDirectory();
        }

        protected DirectoryInfo GetTempDirectory()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "Platibus.UnitTests", DateTime.Now.ToString("yyyyMMddHHmmss"));
            var tempDir = new DirectoryInfo(tempPath);
            if (!tempDir.Exists)
            {
                tempDir.Create();
            }
            return tempDir;
        }
    }
}
