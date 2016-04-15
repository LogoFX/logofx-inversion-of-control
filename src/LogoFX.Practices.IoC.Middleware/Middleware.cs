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
    /// <typeparam name="TRootObject">The type of the root object.</typeparam>    
    public class RegisterSimpleCompositionModulesMiddleware<TRootObject> :
        IMiddleware<IBootstrapperWithContainer<TRootObject, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>>
    {
        private readonly Func<object> _lifetimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterSimpleCompositionModulesMiddleware{TRootObject}"/> class.
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
        public IBootstrapperWithContainer<TRootObject, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            Apply(IBootstrapperWithContainer<TRootObject, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> @object)
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
        /// <typeparam name="TRootObject">The type of the root object.</typeparam>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="lifetimeScopeProvider">The lifetime scope provider.</param>
        public static IBootstrapperWithContainer<TRootObject, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer>
            UseSimpleCompositionModules<TRootObject>(
            this IBootstrapperWithContainer<TRootObject, ExtendedSimpleContainerAdapter, ExtendedSimpleContainer> bootstrapper,
            Func<object> lifetimeScopeProvider)
        {
            return
                bootstrapper.Use(
                    new RegisterSimpleCompositionModulesMiddleware
                        <TRootObject>(
                        lifetimeScopeProvider));
        }
    }
}
