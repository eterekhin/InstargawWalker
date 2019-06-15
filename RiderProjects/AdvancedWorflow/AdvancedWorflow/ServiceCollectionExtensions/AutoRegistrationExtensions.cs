using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedWorflow.Workflow.WorkflowInterfaces;
using Force;
using Microsoft.Extensions.DependencyInjection;
using AuthProject.WorkflowTest;

namespace AuthProject.ServiceCollectionExtensions
{
    public static class AutoRegistrationExtensions
    {
        private static IEnumerable<Assembly> Assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        private static readonly Type[] BaseTypes =
        {
            typeof(IHandler<,>),
            typeof(IHandler<>),
            typeof(IAsyncHandler<,>),
            typeof(IAsyncHandler<>),
            typeof(ICanAsyncRollBack<>)
        };

        public static IServiceCollection AutoRegistration(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .Scan(x => x.FromAssemblies(Assemblies)
                    .AddClasses(xx => xx.AssignableToAny(BaseTypes))
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithScopedLifetime());
        }
    }
}