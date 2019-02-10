using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Xml.Schema;


namespace DesertGenerator
{
    internal class Program
    {
        private readonly int _desertWidth = 4;
        private readonly int _desertDepth = 2;
        private readonly int _amountOfTreasures = 1;

        private static readonly List<FileStream> HeavyRocks = new List<FileStream>();

        private string[] desertNames = new string[3]
        {
            "sand",
            "hill",
            "dune"
        };

        private static DirectoryInfo _desertRoot = new DirectoryInfo("D:\\Desert");


        private static void ClearDesert()
        {
            foreach (var rock in HeavyRocks)
            {
                rock.Unlock(0, 0);
            }

            _desertRoot.Delete(true);
        }

        private void GenerateDesert()
        {
            ClearDesert();

            Directory.CreateDirectory(_desertRoot.FullName);
        }
    }
}