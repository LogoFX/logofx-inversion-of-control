﻿using NUnit.Framework;

namespace LogoFX.Practices.IoC.Tests
{   
    [TestFixture]
    class ExtendedSimpleContainerTests
    {
        [Test]
        public void
            GivenDependencyHasOneNamedParameter_WhenDependencyIsRegisteredAndDependencyIsResolvedWithParameter_ThenResolvedDependencyValueIsCorrect
            ()
        {
            var container = new ExtendedSimpleContainer();
            container.RegisterPerRequest(typeof(ITestNamedParameterDependency), null, typeof(TestNamedParameterDependency));
            const string model = "5";
            var dependency = container.GetInstance(typeof (ITestNamedParameterDependency), null,
                new IParameter[] {new NamedParameter("model", model)}) as ITestNamedParameterDependency;

            var actualModel = dependency.Model;
            Assert.AreEqual(model, actualModel);
        }

        [Test]
        public void
            GivenDependencyHasOneTypedParameter_WhenDependencyIsRegisteredAndDependencyIsResolvedWithParameter_ThenResolvedDependencyValueIsCorrect
            ()
        {
            var container = new ExtendedSimpleContainer();
            container.RegisterPerRequest(typeof(ITestTypedParameterDependency), null, typeof(TestTypedParameterDependency));
            const int val = 6;
            var dependency = container.GetInstance(typeof(ITestTypedParameterDependency), null,
                new IParameter[] { new TypedParameter(typeof(int), val) }) as ITestTypedParameterDependency;

            var actualValue = dependency.Value;
            Assert.AreEqual(val, actualValue);
        }

        [Test]
        public void
            GivenThereAreMultipleSameTypedDependencies_WhenDependencyIsRegisteredByHandlerAndDependencyIsRegisteredByHandler_ThenResolutionOfDependenciesCollectionIsCorrect
            ()
        {
            var modules = new ITestModule[] {new TestModule {Name = "1"}, new TestModule {Name = "2"}};

            var container = new ExtendedSimpleContainer();
            container.RegisterHandler(typeof(ITestModule), null,(c,r) => modules[0]);
            container.RegisterHandler(typeof(ITestModule), null, (c, r) => modules[1]);
            var actualModules = container.GetAllInstances(typeof (ITestModule));

            CollectionAssert.AreEqual(modules, actualModules);
        }
        
        [Test]
        public void
            GivenThereIsLifetimeProvider_WhenDependencyIsRegisteredPerLifetimeAndLifetimeProviderBecomesNull_TheDependencyBecomesNull
            ()
        {
            var @object = string.Empty;
            LifetimeProvider.Current = @object;
            
            var container = new ExtendedSimpleContainer();
            container.RegisterPerLifetime(() => LifetimeProvider.Current, typeof(ITestModule), null, typeof(TestModule));

            container.GetInstance(typeof (ITestModule), null);            
            LifetimeProvider.Current = null;
            var dependency = container.GetInstance(typeof (ITestModule), null);
            Assert.IsNull(dependency);
        }
    }
}
