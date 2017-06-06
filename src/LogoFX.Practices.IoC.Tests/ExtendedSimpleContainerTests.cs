using FluentAssertions;
using Xunit;

namespace LogoFX.Practices.IoC.Tests
{
    public class ExtendedSimpleContainerTests
    {
        [Fact]
        public void
            GivenDependencyHasOneNamedParameter_WhenDependencyIsRegisteredAndDependencyIsResolvedWithParameter_ThenResolvedDependencyValueIsCorrect
            ()
        {
            var container = new ExtendedSimpleContainer();
            container.RegisterPerRequest(typeof(ITestNamedParameterDependency), null,
                typeof(TestNamedParameterDependency));
            const string model = "5";
            var dependency = container.GetInstance(typeof(ITestNamedParameterDependency), null,
                new IParameter[] {new NamedParameter("model", model)}) as ITestNamedParameterDependency;

            var actualModel = dependency.Model;
            actualModel.Should().Be(model);
        }

        [Fact]
        public void
            GivenDependencyHasOneTypedParameter_WhenDependencyIsRegisteredAndDependencyIsResolvedWithParameter_ThenResolvedDependencyValueIsCorrect
            ()
        {
            var container = new ExtendedSimpleContainer();
            container.RegisterPerRequest(typeof(ITestTypedParameterDependency), null,
                typeof(TestTypedParameterDependency));
            const int val = 6;
            var dependency = container.GetInstance(typeof(ITestTypedParameterDependency), null,
                new IParameter[] {new TypedParameter(typeof(int), val)}) as ITestTypedParameterDependency;

            var actualValue = dependency.Value;
            actualValue.Should().Be(val);
        }

        [Fact]
        public void
            GivenThereAreMultipleSameTypedDependencies_WhenDependencyIsRegisteredByHandlerAndDependencyIsRegisteredByHandler_ThenResolutionOfDependenciesCollectionIsCorrect
            ()
        {
            var modules = new ITestModule[] {new TestModule {Name = "1"}, new TestModule {Name = "2"}};

            var container = new ExtendedSimpleContainer();
            container.RegisterHandler(typeof(ITestModule), null, (c, r) => modules[0]);
            container.RegisterHandler(typeof(ITestModule), null, (c, r) => modules[1]);
            var actualModules = container.GetAllInstances(typeof(ITestModule));

            actualModules.ShouldAllBeEquivalentTo(modules);
        }

        [Fact]
        public void
            GivenThereIsLifetimeProvider_WhenDependencyIsRegisteredPerLifetimeAndLifetimeProviderBecomesNull_TheDependencyBecomesNull
            ()
        {
            var container = new ExtendedSimpleContainer();
            container.RegisterPerLifetime(() => LifetimeProvider.Current, typeof(ITestModule), null,
                typeof(TestModule));
            var @object = string.Empty;
            LifetimeProvider.Current = @object;
            container.GetInstance(typeof(ITestModule), null);
            LifetimeProvider.Current = null;

            var dependency = container.GetInstance(typeof(ITestModule), null);
            dependency.Should().BeNull();
        }

        [Fact]
        public void
            GivenDependencyHasOneNamedParameter_WhenDependencyIsRegisteredByHandlerAsSingleton_ThenSeveralResolutionOfDependenciesAreSame()
        {
            int counter = 0;

            var container = new ExtendedSimpleContainer();
            container.RegisterSingleton<ITestModule>((c, r) => new TestModule {Name = (++counter).ToString()});

            var module = container.GetInstance(typeof(ITestModule), null);
            var actualModule = container.GetInstance(typeof(ITestModule), null);

            actualModule.Should().BeSameAs(module);
        }
    }
}