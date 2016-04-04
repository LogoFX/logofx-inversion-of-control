using System;
using System.Linq;
using LogoFX.Client.Bootstrapping;
using LogoFX.Client.Bootstrapping.Adapters.SimpleContainer;
using Solid.Practices.Middleware;

namespace LogoFX.Practices.IoC
{
    /// <summary>
    /// Middleware that registers simple composition modules.
    /// </summary>
    /// <typeparam name="TRootViewModel">The type of the root view model.</typeparam>    
    public class RegisterSimpleCompositionModulesMiddleware<TRootViewModel> :
        IMiddleware<IBootstrapperWithContainer<TRootViewModel, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>>
    {
        private readonly Func<object> _lifetimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterSimpleCompositionModulesMiddleware{TRootViewModel}"/> class.
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
        public IBootstrapperWithContainer<TRootViewModel, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            Apply(IBootstrapperWithContainer<TRootViewModel, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> @object)
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
        /// Uses the view model factory which is based on LogoFX Simple Container.
        /// </summary>
        /// <typeparam name="TRootViewModel">The type of the root view model.</typeparam>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="lifetimeScopeProvider">The lifetime scope provider.</param>
        public static IBootstrapperWithContainer<TRootViewModel, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            UseLifetimeScope<TRootViewModel>(
            this IBootstrapperWithContainer<TRootViewModel, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> bootstrapper,
            Func<object> lifetimeScopeProvider)
        {
            return
                bootstrapper.Use(
                    new RegisterSimpleCompositionModulesMiddleware
                        <TRootViewModel>(
                        lifetimeScopeProvider));
        }
    }
}
