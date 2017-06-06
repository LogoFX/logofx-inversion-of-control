using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LogoFX.Practices.IoC
{
    /// <summary>
    ///  A simple IoC container.
    /// </summary>
    public class SimpleContainer : IDisposable
    {
        #region Nested Types

        private class ContainerEntry : List<Func<SimpleContainer, IParameter[], object>>
        {
            public string Key { get; set; }
            public Type Service { get; set; }
        }

        private class FactoryFactory<T>
        {
            public Func<T> Create(SimpleContainer container)
            {
                return () => (T) container.GetInstance(typeof(T), null);
            }
        }

        #endregion

        #region Fields

        private static readonly TypeInfo s_delegateType = typeof(Delegate).GetTypeInfo();
        private static readonly TypeInfo s_enumerableType = typeof(IEnumerable).GetTypeInfo();
        private readonly List<ContainerEntry> _entries;

        #endregion

        #region Constructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "SimpleContainer" /> class.
        /// </summary>
        public SimpleContainer()
        {
            _entries = new List<ContainerEntry>();
        }

        private SimpleContainer(IEnumerable<ContainerEntry> entries)
        {
            _entries = new List<ContainerEntry>(entries);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers the service per lifetime of an external object.
        /// </summary>
        /// <param name="lifeTime">The lifetime function of an external object.</param>
        /// <param name="service">The service.</param>
        /// <param name="key">The key.</param>
        /// <param name="implementation">The implementation.</param>
        public void RegisterPerLifetime(Func<object> lifeTime, Type service, string key, Type implementation)
        {
            WeakReference wr = null;
            object singleton = null;
            RegisterHandlerInternal(service, key,
                (container, args) =>
                {
                    GC.Collect();
                    if (LifetimeIsDead(wr))
                    {
                        return null;
                    }
                    if (wr == null || !wr.IsAlive || singleton == null)
                    {
                        wr = new WeakReference(lifeTime());
                        singleton = BuildInstance(implementation);
                    }
                    return singleton;
                });
        }

        /// <summary>
        /// Registers the instance of dependency
        /// </summary>
        /// <param name="service">Type of dependency</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="implementation">The instance of dependency</param>
        public void RegisterInstance(Type service, string key, object implementation)
        {
            RegisterHandlerInternal(service, key, (container, args) => implementation);
        }

        /// <summary>
        /// Registers the instance of dependency
        /// </summary>
        /// <typeparam name="TService">Type of dependency</typeparam>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="implementation">The instance of dependency</param>
        public void RegisterInstance<TService>(string key, object implementation)
        {
            RegisterHandlerInternal(typeof(TService), key, (container, args) => implementation);
        }

        /// <summary>
        /// Registers the dependency so that a new instance is created on every request.
        /// </summary>
        /// <param name="service">Type of dependency declaration</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="implementation">Type of dependency implementation</param>
        public void RegisterPerRequest(Type service, string key, Type implementation)
        {
            RegisterHandlerInternal(service, key, (container, args) => container.BuildInstance(implementation, args));
        }

        /// <summary>
        /// Registers the dependency so that a new instance is created on every request.
        /// </summary>
        /// <typeparam name="TService">Type of dependency declaration</typeparam>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="implementation">Type of dependency implementation</param>
        public void RegisterPerRequest<TService>(string key, Type implementation)
        {
            RegisterHandlerInternal(typeof(TService), key, (container, args) => container.BuildInstance(implementation, args));
        }

        /// <summary>
        /// Registers dependency as singleton
        /// </summary>
        /// <param name="service">Type of dependency declaration</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="implementation">Type of dependency implementation</param>
        public void RegisterSingleton(Type service, string key, Type implementation)
        {
            RegisterSingletonInternal(service, key, (container, args) => container.BuildInstance(implementation));
        }

        /// <summary>
        /// Registers a custom handler for resolving dependencies from the container as singleton.
        /// </summary>
        /// <param name="service">Type of dependency declaration</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="handler">Resolution handler</param>
        public void RegisterSingleton(Type service, string key, Func<SimpleContainer, IParameter[], object> handler)
        {
            RegisterSingletonInternal(service, key, handler);
        }

        /// <summary>
        /// Registers a custom handler for resolving dependencies from the container as singleton.
        /// </summary>
        /// <typeparam name="TService">Type of dependency declaration</typeparam>
        /// <param name="handler">Resolution handler</param>
        public void RegisterSingleton<TService>(Func<SimpleContainer, IParameter[], object> handler)
        {
            RegisterSingletonInternal(typeof(TService), null, handler);
        }

        /// <summary>
        /// Registers a custom handler for resolving dependencies from the container.
        /// </summary>
        /// <param name="service">Type of dependency declaration</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="handler">Resolution handler</param>
        public void RegisterHandler(Type service, string key, Func<SimpleContainer, IParameter[], object> handler)
        {
            RegisterHandlerInternal(service, key, handler);
        }

        /// <summary>
        /// Registers a custom handler for resolving dependencies from the container.
        /// </summary>
        /// <typeparam name="TService">Type of dependency declaration</typeparam>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="handler">Resolution handler</param>
        public void RegisterHandler<TService>(string key, Func<SimpleContainer, IParameter[], object> handler)
        {
            RegisterHandlerInternal(typeof(TService), key, handler);
        }

        /// <summary>
        /// Determines if a handler for the dependency/key has previously been registered.
        /// </summary>
        /// <param name="service">Type of dependency</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <returns>True if a handler is registered, false otherwise.</returns>
        public bool HasHandler(Type service, string key = null)
        {
            return GetEntry(service, key) != null;
        }

        /// <summary>
        /// Resolves the dependency by its type/key and optional parameters
        /// </summary>
        /// <param name="service">Type of dependency</param>
        /// <param name="key">Optional dependency key, provide null if not needed</param>
        /// <param name="parameters">optional dynamically injected resolution parameters</param>
        /// <returns>The instance of dependency, or null if it cannot be resolved</returns>
        public object GetInstance(Type service, string key = null, params IParameter[] parameters)
        {
            var entry = GetEntry(service, key);
            if (entry != null)
            {
                //last registration always wins - this is a convention amongst ioc containers
                return entry.Last()(this, parameters);
            }
            var serviceTypeInfo = service.GetTypeInfo();
            if (s_delegateType.IsAssignableFrom(serviceTypeInfo))
            {
                var typeToCreate = serviceTypeInfo.GenericTypeArguments[0];
                var factoryFactoryType = typeof(FactoryFactory<>).MakeGenericType(typeToCreate);
                var factoryFactoryHost = Activator.CreateInstance(factoryFactoryType);
                var factoryFactoryMethod = factoryFactoryType.GetTypeInfo().GetDeclaredMethod("Create");
                return factoryFactoryMethod.Invoke(factoryFactoryHost, new object[] {this});
            }

            if (s_enumerableType.IsAssignableFrom(serviceTypeInfo))
            {
                var listType = serviceTypeInfo.GenericTypeArguments[0];
                var instances = GetAllInstances(listType).ToList();
                var array = Array.CreateInstance(listType, instances.Count);

                for (var i = 0; i < array.Length; i++)
                {
                    array.SetValue(instances[i], i);
                }

                return array;
            }

            return null;
        }

        /// <summary>
        /// Resolves all instances of given dependency type
        /// </summary>
        /// <param name="service">Type of dependency</param>
        /// <returns>All the instances or an empty enumerable if none can be resolved.</returns>
        public IEnumerable<object> GetAllInstances(Type service)
        {
            var entry = GetEntry(service, null);
            return entry != null ? entry.Select(x => x(this, null)) : new object[0];
        }

        /// <summary>
        /// Pushes dependencies into an existing instance based on interface properties with setters.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void BuildUp(object instance)
        {
            var injectables = instance.GetType().GetTypeInfo()
                .DeclaredProperties
                .Where(property => property.CanRead && property.CanWrite &&
                                   property.PropertyType.GetTypeInfo().IsInterface);

            foreach (var propertyInfo in injectables)
            {
                var injection = GetAllInstances(propertyInfo.PropertyType).ToArray();
                if (injection.Any())
                {
                    propertyInfo.SetValue(instance, injection.First(), null);
                }
            }
        }

        /// <summary>
        /// Creates a child simple container.
        /// </summary>
        /// <returns>A new container.</returns>
        public SimpleContainer CreateChildContainer()
        {
            return new SimpleContainer(_entries);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a new instance is created.
        /// </summary>
        public event Action<object> Activated = delegate { };

        #endregion

        #region Protected

        /// <summary>
        ///  Actually does the work of creating the instance and satisfying its constructor dependencies.
        /// </summary>
        /// <param name = "type">The type.</param>
        /// <param name="parameters">dynamically injected parameters</param>
        /// <returns></returns>
        protected object BuildInstance(Type type, params IParameter[] parameters)
        {
            var args = DetermineConstructorArgs(type, parameters);
            return ActivateInstance(type, args);
        }

        /// <summary>
        ///   Creates an instance of the type with the specified constructor arguments.
        /// </summary>
        /// <param name = "type">The type.</param>
        /// <param name = "args">The constructor args.</param>
        /// <returns>The created instance.</returns>
        protected virtual object ActivateInstance(Type type, object[] args)
        {
            var instance = args.Length > 0 ? Activator.CreateInstance(type, args) : Activator.CreateInstance(type);
            Activated(instance);
            return instance;
        }

        #endregion

        #region Private Members

        private void RegisterHandlerInternal(Type service, string key,
            Func<SimpleContainer, IParameter[], object> handler)
        {
            GetOrCreateEntry(service, key).Add(handler);
        }

        private void RegisterSingletonInternal(Type service, string key,
            Func<SimpleContainer, IParameter[], object> handler)
        {
            object singleton = null;
            RegisterHandlerInternal(service, key,
                (container, args) => singleton ?? (singleton = handler(container, args)));
        }

        private static bool LifetimeIsDead(WeakReference wr)
        {
            return wr != null && wr.IsAlive == false;
        }

        private ContainerEntry GetOrCreateEntry(Type service, string key)
        {
            var entry = GetEntry(service, key);
            if (entry == null)
            {
                entry = new ContainerEntry {Service = service, Key = key};
                _entries.Add(entry);
            }

            return entry;
        }

        private ContainerEntry GetEntry(Type service, string key)
        {
            if (service == null)
            {
                return _entries.FirstOrDefault(x => x.Key == key);
            }

            if (key == null)
            {
                return _entries.FirstOrDefault(x => x.Service == service && x.Key == null) ??
                       _entries.FirstOrDefault(x => x.Service == service);
            }

            return _entries.FirstOrDefault(x => x.Service == service && x.Key == key);
        }

        private object[] DetermineConstructorArgs(Type implementation, params IParameter[] parameters)
        {
            var args = new List<object>();
            var constructor = SelectEligibleConstructor(implementation);

            if (constructor != null)
                args.AddRange(constructor.GetParameters().Select(info => CreateArgument(info, parameters)));

            return args.ToArray();
        }

        private object CreateArgument(ParameterInfo info, IParameter[] parameters)
        {
            if (parameters != null)
            {
                IParameter match = parameters.OfType<NamedParameter>()
                    .FirstOrDefault(t => t.ParameterName == info.Name);
                if (match != null)
                {
                    return match.ParameterValue;
                }
                match = parameters.OfType<TypedParameter>().FirstOrDefault(t => t.ParameterType == info.ParameterType);
                if (match != null)
                {
                    return match.ParameterValue;
                }
            }
            return GetInstance(info.ParameterType, null);
        }

        private static ConstructorInfo SelectEligibleConstructor(Type type)
        {
            return (from c in type.GetTypeInfo().DeclaredConstructors
                orderby c.GetParameters().Length descending
                select c).FirstOrDefault();
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {

        }
    }
}