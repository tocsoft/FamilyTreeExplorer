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

            var file = args[0];

            var context = new LINQ2GEDCOM.GEDCOMContext(file);

            var people = context.Individuals.Select(p => new Person
            {
                Id = p.ID,
                Name = p.Names.FirstOrDefault().GivenName,
                Parents = new[] { p.ParentFamily.HusbandID, p.ParentFamily.WifeID }.Where(x => x!=null).ToArray()
                ,
                Children = p.SpousalFamilies.SelectMany(x => x.Children).Select(x => x.ID).ToArray()
                ,
                Partners = p.SpousalFamilies.SelectMany(s => new[] { s.HusbandID, s.WifeID }.Where(x => x != null)).Where(s => s != p.ID).ToArray()

            });


        }
    }

    public class Person
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string[] Parents { get; set; }

        public string[] Children { get; set; }

        public string[] Partners { get; set; }
    }

    public class Address
    {
        public string Details
        {
            get; set;
        }

        public float Longitude { get; set; }

        public float Latitude { get; set; }
    }
}
