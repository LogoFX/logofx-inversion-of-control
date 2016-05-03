using System;
using Solid.Practices.Modularity;

namespace LogoFX.Practices.IoC
{
    /// <summary>
    /// Composition module that allows registering dependencies bound to 
    /// an external lifetime scope provider.
    /// </summary>
    public interface ISimpleCompositionModule : ICompositionModule
    {
        /// <summary>
        /// Registers dependencies.
        /// </summary>
        /// <param name="container">The ioc container.</param>
        /// <param name="lifetimeScopeProvider">The lifetime scope provider.</param>
        void RegisterModule(SimpleContainer container, Func<object> lifetimeScopeProvider);
    }
}
