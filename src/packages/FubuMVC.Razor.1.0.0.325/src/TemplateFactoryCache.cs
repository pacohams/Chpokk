﻿using System;
using System.Collections.Generic;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using FubuCore;
using FubuCore.Util;
using FubuMVC.Core.View;
using FubuMVC.Core.View.Model;
using FubuMVC.Razor.Core;
using FubuMVC.Razor.RazorModel;
using FubuMVC.Razor.Rendering;

namespace FubuMVC.Razor
{
    public interface ITemplateFactory
    {
        IFubuRazorView GetView(ViewDescriptor<IRazorTemplate> descriptor);
    }

    public class TemplateFactoryCache : ITemplateFactory
    {
        private readonly CommonViewNamespaces _commonViewNamespaces;
        private readonly RazorEngineSettings _razorEngineSettings;
        private readonly ITemplateCompiler _templateCompiler;
        private readonly IRazorTemplateGenerator _templateGenerator;
        private readonly Cache<string, long> _lastModifiedCache;
        private readonly IDictionary<string, Type> _cache;

        public TemplateFactoryCache(CommonViewNamespaces commonViewNamespaces, 
            RazorEngineSettings razorEngineSettings, ITemplateCompiler templateCompiler, IRazorTemplateGenerator templateGenerator)
        {
            _commonViewNamespaces = commonViewNamespaces;
            _razorEngineSettings = razorEngineSettings;
            _templateCompiler = templateCompiler;
            _templateGenerator = templateGenerator;
            _cache = new Dictionary<string, Type>();
            _lastModifiedCache = new Cache<string, long>(name => name.LastModified());
        }

        public IFubuRazorView GetView(ViewDescriptor<IRazorTemplate> descriptor)
        {
            Type viewType;
            var filePath = descriptor.Template.FilePath;
            _cache.TryGetValue(filePath, out viewType);
            var lastModified = filePath.LastModified();
            if (viewType == null || (_lastModifiedCache[filePath] != lastModified))
            {
                viewType = getViewType(descriptor);
                lock (_cache)
                {
                    _cache[filePath] = viewType;
                    _lastModifiedCache[filePath] = lastModified;
                }
            }
            return Activator.CreateInstance(viewType).As<IFubuRazorView>();
        }

        private Type getViewType(ViewDescriptor<IRazorTemplate> descriptor)
        {
            var className = ParserHelpers.SanitizeClassName(descriptor.ViewPath);
            var baseTemplateType = _razorEngineSettings.BaseTemplateType;
            var generatedClassContext = new GeneratedClassContext("Execute", "Write", "WriteLiteral", null, null,
                                                                  className, "DefineSection");
            var codeLanguage = RazorCodeLanguageFactory.Create(descriptor.Template.FilePath.FileExtension());
            var host = new RazorEngineHost(codeLanguage)
            {
                DefaultBaseClass = baseTemplateType.FullName,
                DefaultNamespace = "FubuMVC.Razor.GeneratedTemplates",
                GeneratedClassContext = generatedClassContext
            };
            host.NamespaceImports.UnionWith(_commonViewNamespaces.Namespaces);

            var results = _templateGenerator.GenerateCode(descriptor, className, host);

            return _templateCompiler.Compile(className, results.GeneratedCode, host);
        }
    }
}