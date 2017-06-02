using System;
using System.IO;

namespace Platibus.UnitTests
{
    internal class FileUtil
    {
        public static string NewTempTestPath()
        {
            var now = DateTime.Now;
            var datePath = now.ToString("yyyy-MM-dd");
            var timePath = now.ToString("HH-mm-ss");
            var randomPath = new Random().Next(short.MaxValue).ToString("x4");
            var path = Path.Combine(Path.GetTempPath(), "Platibus", "UnitTests", datePath, timePath, randomPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
