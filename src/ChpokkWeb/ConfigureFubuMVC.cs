using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Bottles;
using Bottles.Diagnostics;
using ChpokkWeb.App_Start;
using ChpokkWeb.Repa;
using ChpokkWeb.Shared;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Assets.Combination;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.View;
using FubuMVC.Spark;
using FubuMVC.Spark.SparkModel;
using Spark;
using StructureMap;
using dotless.Core;
using dotless.Core.Importers;
using dotless.Core.Input;
using dotless.Core.Parser;
using dotless.Core.Stylizers;

namespace ChpokkWeb {
	public class ConfigureFubuMVC : FubuRegistry {
		public ConfigureFubuMVC() {
			// This line turns on the basic diagnostics and request tracing
			IncludeDiagnostics(true);

			// All public methods from concrete classes ending in "Controller"
			// in this assembly are assumed to be action methods
			Actions.IncludeClassesSuffixedWithController();

			// Policies
			Routes
				.IgnoreControllerNamesEntirely()
				.RootAtAssemblyNamespace()
				;

		

			Views
				.TryToAttachWithDefaultConventions()
				.RegisterActionLessViews(
				token => typeof(IDontNeedActionsModel).IsAssignableFrom(token.ViewModelType) || token.ViewModelType.Name.Contains("InputModel"), (chain, token) =>
				        {
				            var url = (token.Name == "DemoView") ? "" : token.Name;
							if (token.ViewModelType == typeof(RepositoryInputModel))
				        	{
				        		url += "/{Name}";
				        	}
				            chain.Route = new RouteDefinition(url); 
				        })
				;

		}

		//internal class AssetPathResolver : IPathResolver {
		//    public string GetFullPath(string path) {
		//        return HttpContext.Current.Server.MapPath(path).Replace(@"\_content\", @"\Content\");
		//    } 
		//}

		public class SparkSettingsActivator : IActivator {
			private readonly ISparkViewEngine _engine;
			public SparkSettingsActivator(ISparkViewEngine engine) {
				_engine = engine;
			}

			public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log) {
				var settings = _engine.Settings.As<SparkSettings>();
				settings.Debug = true;
			}
		}
	}


}