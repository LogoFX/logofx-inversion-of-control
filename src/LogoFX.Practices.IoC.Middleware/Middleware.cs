using System;
using System.Linq;
using LogoFX.Bootstrapping;
using LogoFX.Client.Bootstrapping.Adapters.SimpleContainer;
using Solid.Practices.Middleware;

namespace LogoFX.Practices.IoC
{
    /// <summary>
    /// Middleware that registers simple composition modules.
    /// </summary>    
    public class RegisterSimpleCompositionModulesMiddleware :
        IMiddleware<IBootstrapperWithContainer<ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>>
    {
        private readonly Func<object> _lifetimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterSimpleCompositionModulesMiddleware"/> class.
        /// </summary>
        /// <param name="lifetimeProvider">The lifetime provider.</param>
        public RegisterSimpleCompositionModulesMiddleware(Func<object> lifetimeProvider)
        {
            _lifetimeProvider = lifetimeProvider;
        }

        /// <summary>
        /// Applies the middleware on the specified object.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <returns></returns>
        public IBootstrapperWithContainer<ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            Apply(IBootstrapperWithContainer<ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> @object)
        {
            foreach (var module in @object.Modules.OfType<ISimpleCompositionModule>())
            {
                module.RegisterModule(@object.Container, _lifetimeProvider);
            }
            return @object;
        }
    }

    /// <summary>
    /// Bootstrapper extensions.
    /// </summary>
    public static class BootstrapperExtensions
    {
        /// <summary>
        /// Uses middleware which registers simple composition modules.
        /// </summary>        
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="lifetimeScopeProvider">The lifetime scope provider.</param>
        public static IBootstrapperWithContainer<ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            UseSimpleCompositionModules(
            this IBootstrapperWithContainer<ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> bootstrapper,
            Func<object> lifetimeScopeProvider)
        {
            return
                bootstrapper.Use(
                    new RegisterSimpleCompositionModulesMiddleware(
                        lifetimeScopeProvider));
        }
    }
}
