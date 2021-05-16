using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace LogoFX.Practices.IoC.Specs
{
    [Binding]
    internal sealed class ExtendedSimpleContainerSteps
    {
        private readonly ScenarioContext _scenarioContext;

        public ExtendedSimpleContainerSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [When(@"The container is created")]
        public void WhenTheContainerIsCreated()
        {
            var container = new ExtendedSimpleContainer();
            _scenarioContext.Add("container", container);
        }

        [When(@"The dependency with named parameter is registered in transient fashion")]
        public void WhenTheDependencyWithNamedParameterIsRegisteredInTransientFashion()
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            container.RegisterPerRequest(typeof(ITestNamedParameterDependency), null,
                typeof(TestNamedParameterDependency));
        }

        [When(@"The dependency with typed parameter is registered in transient fashion")]
        public void WhenTheDependencyWithTypedParameterIsRegisteredInTransientFashion()
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            container.RegisterPerRequest(typeof(ITestTypedParameterDependency), null,
                typeof(TestTypedParameterDependency));
        }

        [When(@"The dependency with named parameter is registered in singleton fashion")]
        public void WhenTheDependencyWithNamedParameterIsRegisteredInSingletonFashion()
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            container.RegisterSingleton<ITestModule>((c, r) => new TestModule {Name = "1"});
        }

        [When(@"The same type dependency is registered in instance fashion using first instance")]
        public void WhenTheSameTypeDependencyIsRegisteredInInstanceFashionUsingFirstInstance()
        {
            RegisterInstanceDependency("firstModule", new TestModule {Name = "1"});
        }

        [When(@"The same type dependency is registered in instance fashion using second instance")]
        public void WhenTheSameTypeDependencyIsRegisteredInInstanceFashionUsingSecondInstance()
        {
            RegisterInstanceDependency("secondModule", new TestModule { Name = "2" });
        }

        private void RegisterInstanceDependency(string key, ITestModule instance)
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            var module = new TestModule { Name = "1" };
            container.RegisterHandler(typeof(ITestModule), null, (c, r) => instance);
            _scenarioContext.Add(key, instance);
        }

        [When(@"The dependency is resolved with value '(.*)' for named parameter")]
        public void WhenTheDependencyIsResolvedWithValueForNamedParameter(string parameter)
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            var dependency = container.GetInstance(typeof(ITestNamedParameterDependency), null,
                new IParameter[] { new NamedParameter("model", parameter) }) as ITestNamedParameterDependency;
            _scenarioContext.Add("dependency", dependency);
        }

        [When(@"The dependency is resolved with value (.*) for typed parameter")]
        public void WhenTheDependencyIsResolvedWithValueForTypedParameter(int parameter)
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            const int val = 6;
            var dependency = container.GetInstance(typeof(ITestTypedParameterDependency), null,
                new IParameter[] { new TypedParameter(typeof(int), val) }) as ITestTypedParameterDependency;
            _scenarioContext.Add("dependency", dependency);
        }

        [When(@"The collection of dependencies is resolved")]
        public void WhenTheCollectionOfDependenciesIsResolved()
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            var dependencies = container.GetAllInstances(typeof(ITestModule)).ToArray();
            _scenarioContext.Add("dependencies", dependencies);
        }

        [Then(@"Actual value of parameter inside the named dependency is '(.*)'")]
        public void ThenActualValueOfParameterInsideTheNamedDependencyIs(string parameter)
        {
            var dependency = _scenarioContext.Get<ITestNamedParameterDependency>("dependency");
            var actualModel = dependency.Model;
            actualModel.Should().Be(parameter);
        }

        [Then(@"Actual value of parameter inside the typed dependency is (.*)")]
        public void ThenActualValueOfParameterInsideTheTypedDependencyIs(int parameter)
        {
            var dependency = _scenarioContext.Get<ITestTypedParameterDependency>("dependency");
            var actualValue = dependency.Value;
            actualValue.Should().Be(parameter);
        }

        [Then(@"The collection of dependencies is equivalent to the collection of instances")]
        public void ThenTheCollectionOfDependenciesIsEquivalentToTheCollectionOfInstances()
        {
            var dependencies = _scenarioContext.Get<IEnumerable<object>>("dependencies").ToArray();
            var modules = new[]
                {_scenarioContext.Get<ITestModule>("firstModule"), _scenarioContext.Get<ITestModule>("secondModule")};
            dependencies.Should().BeSubsetOf(modules);
            modules.Should().BeSubsetOf(dependencies);
        }

        [Then(@"Multiple dependency resolutions yield same value")]
        public void ThenMultipleDependencyResolutionsYieldSameValue()
        {
            var container = _scenarioContext.Get<ExtendedSimpleContainer>("container");
            var module = container.GetInstance(typeof(ITestModule), null);
            var actualModule = container.GetInstance(typeof(ITestModule), null);

            actualModule.Should().BeSameAs(module);
        }
    }
}
