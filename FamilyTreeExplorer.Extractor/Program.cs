using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyTreeExplorer.Extractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any() || !File.Exists(args[0]))
            {
                throw new Exception("NO gedcom specified");
            }

            var file = File.OpenText(args[0]);

            var context = new Gedcom.Net.GedcomDocument(file, false);

            file.Dispose();

        }
    }
}
