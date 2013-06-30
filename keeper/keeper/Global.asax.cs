using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Ansi2Html;
using Autofac;
using Autofac.Integration.Mvc;
using Dot.Processor;
using NLog;
using Proc.Runner;

namespace keeper
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            configureIoC();    

            LogManager.GetLogger("trace").Debug("=== App started");
        }

        protected void Application_End()
        {
            LogManager.GetLogger("trace").Debug("=== App finished");
        }


        private void configureIoC()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.Register(ctx => LogManager.GetLogger("trace"))
                .As<Logger>()
                .InstancePerHttpRequest();

            builder.RegisterType<ProcessRunner>()
                .AsImplementedInterfaces()
                .InstancePerDependency()
                ;

            builder.RegisterType<DotProcessor>()
                .AsImplementedInterfaces()
                .SingleInstance()
                ;

            builder.RegisterType<Ansi2HtmlConverter>()
                .AsImplementedInterfaces()
                .InstancePerDependency()
                ;

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

    }
}