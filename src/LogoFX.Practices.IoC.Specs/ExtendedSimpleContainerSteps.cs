using System;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace LogoFX.Practices.IoC.Specs
{
    [Binding]
    internal sealed class ExtendedSimpleContainerSteps
    {
        private readonly ContainerScenarioDataStoreBase _scenarioDataStoreBase;

        public ExtendedSimpleContainerSteps(ScenarioContext scenarioContext)
        {
            _scenarioDataStoreBase = new ContainerScenarioDataStoreBase(scenarioContext);
        }

        [When(@"The container is created")]
        public void WhenTheContainerIsCreated()
        {
            var container = new ExtendedSimpleContainer();
            _scenarioDataStoreBase.Container = container;
        }

        [When(@"The dependency with named parameter is registered in transient fashion")]
        public void WhenTheDependencyWithNamedParameterIsRegisteredInTransientFashion()
        {
            var container = _scenarioDataStoreBase.Container;
            container.RegisterPerRequest(typeof(ITestNamedParameterDependency), null,
                typeof(TestNamedParameterDependency));
        }

        [When(@"The dependency with typed parameter is registered in transient fashion")]
        public void WhenTheDependencyWithTypedParameterIsRegisteredInTransientFashion()
        {
            var container = _scenarioDataStoreBase.Container;
            container.RegisterPerRequest(typeof(ITestTypedParameterDependency), null,
                typeof(TestTypedParameterDependency));
        }

        [When(@"The dependency with named parameter is registered in singleton fashion")]
        public void WhenTheDependencyWithNamedParameterIsRegisteredInSingletonFashion()
        {
            var container = _scenarioDataStoreBase.Container;
            container.RegisterSingleton<ITestModule>((c, r) => new TestModule {Name = "1"});
        }

        [When(@"The same type dependency is registered in instance fashion using first instance")]
        public void WhenTheSameTypeDependencyIsRegisteredInInstanceFashionUsingFirstInstance()
        {
            RegisterInstanceDependency((r, v) => r.FirstModule = v, new TestModule {Name = "1"});
        }

        [When(@"The same type dependency is registered in instance fashion using second instance")]
        public void WhenTheSameTypeDependencyIsRegisteredInInstanceFashionUsingSecondInstance()
        {
            RegisterInstanceDependency((r, v) => r.SecondModule = v, new TestModule { Name = "2" });
        }

        private void RegisterInstanceDependency(Action<ContainerScenarioDataStoreBase, ITestModule> valueSetter, ITestModule instance)
        {
            var container = _scenarioDataStoreBase.Container;
            var module = new TestModule { Name = "1" };
            container.RegisterHandler(typeof(ITestModule), null, (c, r) => instance);
            valueSetter(_scenarioDataStoreBase, instance);
        }

        [When(@"The dependency is resolved with value '(.*)' for named parameter")]
        public void WhenTheDependencyIsResolvedWithValueForNamedParameter(string parameter)
        {
            var container = _scenarioDataStoreBase.Container;
            var dependency = container.GetInstance(typeof(ITestNamedParameterDependency), null,
                new IParameter[] { new NamedParameter("model", parameter) }) as ITestNamedParameterDependency;
            _scenarioDataStoreBase.Dependency = dependency;
        }

        [When(@"The dependency is resolved with value (.*) for typed parameter")]
        public void WhenTheDependencyIsResolvedWithValueForTypedParameter(int parameter)
        {
            var container = _scenarioDataStoreBase.Container;
            const int val = 6;
            var dependency = container.GetInstance(typeof(ITestTypedParameterDependency), null,
                new IParameter[] { new TypedParameter(typeof(int), val) }) as ITestTypedParameterDependency;
            _scenarioDataStoreBase.Dependency = dependency;
        }

        [When(@"The collection of dependencies is resolved")]
        public void WhenTheCollectionOfDependenciesIsResolved()
        {
            var container = _scenarioDataStoreBase.Container;
            var dependencies = container.GetAllInstances(typeof(ITestModule)).ToArray();
            _scenarioDataStoreBase.Dependencies = dependencies;
        }

        [Then(@"Actual value of parameter inside the named dependency is '(.*)'")]
        public void ThenActualValueOfParameterInsideTheNamedDependencyIs(string parameter)
        {
            var dependency = _scenarioDataStoreBase.Dependency as ITestNamedParameterDependency;
            var actualModel = dependency.Model;
            actualModel.Should().Be(parameter);
        }

        [Then(@"Actual value of parameter inside the typed dependency is (.*)")]
        public void ThenActualValueOfParameterInsideTheTypedDependencyIs(int parameter)
        {
            var dependency = _scenarioDataStoreBase.Dependency as ITestTypedParameterDependency;
            var actualValue = dependency.Value;
            actualValue.Should().Be(parameter);
        }

        [Then(@"The collection of dependencies is equivalent to the collection of instances")]
        public void ThenTheCollectionOfDependenciesIsEquivalentToTheCollectionOfInstances()
        {
            var dependencies = _scenarioDataStoreBase.Dependencies.OfType<ITestModule>().ToArray();
            var modules = new[]
                {_scenarioDataStoreBase.FirstModule, _scenarioDataStoreBase.SecondModule};
            dependencies.Should().BeSubsetOf(modules);
            modules.Should().BeSubsetOf(dependencies);
        }

        [Then(@"Multiple dependency resolutions yield same value")]
        public void ThenMultipleDependencyResolutionsYieldSameValue()
        {
            var container = _scenarioDataStoreBase.Container;
            var module = container.GetInstance(typeof(ITestModule), null);
            var actualModule = container.GetInstance(typeof(ITestModule), null);

            actualModule.Should().BeSameAs(module);
        }
    }
}
