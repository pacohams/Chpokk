using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChpokkWeb.Shared;
using ChpokkWeb.Stuff;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.View;
using FubuMVC.Spark;
using FubuMVC.Spark.SparkModel;
using StructureMap;

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
				//.HomeIs<DummyModel>()
				.HomeIs<InputStuffModel>()
				;

			this.UseSpark();

			Views
				.TryToAttachWithDefaultConventions()
				//.RegisterActionLessViews(
				//token => token.ViewModelType == typeof(DummyModel), chain => {
				//    chain.Route = new RouteDefinition("demo");
				//})
				;
		}
	}

	public class ModellessSparkViewFacility : IViewFacility {
		private readonly ITemplateRegistry _templateRegistry;
		public ModellessSparkViewFacility(ITemplateRegistry templateRegistry) {
			_templateRegistry = templateRegistry;
		}

		public IEnumerable<IViewToken> FindViews(TypePool types, BehaviorGraph graph) {
			var descriptors = from template in _templateRegistry.AllTemplates()
							  where template.Descriptor is ViewDescriptor
							  select template.Descriptor as ViewDescriptor;
			var tokens = from descriptor in descriptors where !descriptor.HasViewModel() select new SparkViewToken(descriptor);
			return tokens.Cast<IViewToken>();

		}
	}
}