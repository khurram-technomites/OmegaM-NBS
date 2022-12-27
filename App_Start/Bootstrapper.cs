using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using NowBuySell.Data.Infrastructure;
using NowBuySell.Data.Repositories;
using NowBuySell.Service;
using NowBuySell.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace NowBuySell.Web
{
	public class Bootstrapper
	{

		public static void Run()
		{
			SetAutofacContainer();
			//Configure AutoMapper
			//AutoMapperConfiguration.Configure();

		}

		private static void SetAutofacContainer()
		{
			var builder = new ContainerBuilder();



			// You can register controllers all at once using assembly scanning...
			builder.RegisterControllers(typeof(MvcApplication).Assembly);

			var config = GlobalConfiguration.Configuration;
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			//builder.RegisterControllers(Assembly.GetExecutingAssembly());
			builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
			builder.RegisterType<DbFactory>().As<IDbFactory>().InstancePerRequest();

			// Repositories
			builder.RegisterAssemblyTypes(typeof(UserRepository).Assembly)
				.Where(t => t.Name.EndsWith("Repository"))
				.AsImplementedInterfaces().InstancePerRequest();
			// Services
			builder.RegisterAssemblyTypes(typeof(UserService).Assembly)
			   .Where(t => t.Name.EndsWith("Service"))
			   .AsImplementedInterfaces().InstancePerRequest();

			builder.RegisterType<Mail>().As<IMail>().InstancePerRequest();

			IContainer container = builder.Build();


			DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
			config.DependencyResolver = new AutofacWebApiDependencyResolver((IContainer)container);
		}
	}
}