using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac.Integration.WebApi;
using Autofac;
using System.Web.Http;
using System.Web.Hosting;
using System.IO;
using System.Configuration;

namespace FamilyTreeExplorer
{

    public class AutofacConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterDependancies(Autofac.ContainerBuilder builder)
        {
            builder = builder ?? new ContainerBuilder();


            // Register the Web API controllers.
            builder.RegisterApiControllers(typeof(AutofacConfig).Assembly);

            var geocahceDirectory = ConfigurationManager.AppSettings["GeoCache"] ?? @".\App_Data\geocache.json";
            if (!Path.IsPathRooted(geocahceDirectory))
            {
                geocahceDirectory = Path.Combine(HttpRuntime.AppDomainAppPath, geocahceDirectory);
                geocahceDirectory = Path.GetFullPath(geocahceDirectory);
            }
            builder.RegisterInstance(new Services.Geocoder(geocahceDirectory));


            var GedcomFile = ConfigurationManager.AppSettings["GedcomFile"] ?? @".\App_Data\Data.ged";
            if (!Path.IsPathRooted(GedcomFile))
            {
                GedcomFile = Path.Combine(HttpRuntime.AppDomainAppPath, GedcomFile);
                GedcomFile = Path.GetFullPath(GedcomFile);
            }

            builder.RegisterInstance(new Gedcom.Net.GedcomDocument(Gedcom.Net.FileDom.FileDocument.Load(GedcomFile)));

            builder.RegisterType<Services.DataStore>();


            // Build the container.
            var container = builder.Build();

            // Create the depenedency resolver.
            var resolver = new AutofacWebApiDependencyResolver(container);

            // Configure Web API with the dependency resolver.
            GlobalConfiguration.Configuration.DependencyResolver = resolver;

        }
    }
}