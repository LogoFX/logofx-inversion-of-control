using Attest.Testing.Context.SpecFlow;
using TechTalk.SpecFlow;

namespace LogoFX.Practices.IoC.Specs
{
    internal sealed class ContainerScenarioDataStoreBase : ScenarioDataStoreBase
    {
        public ContainerScenarioDataStoreBase(ScenarioContext scenarioContext) : base(scenarioContext)
        {
        }

        public ExtendedSimpleContainer Container
        {
            get => GetValue<ExtendedSimpleContainer>();
            set => SetValue(value);
        }

        public object Dependency
        {
            get => GetValue<object>();
            set => SetValue(value);
        }

        public object[] Dependencies
        {
            get => GetValue<object[]>();
            set => SetValue(value);
        }

        public ITestModule FirstModule
        {
            get => GetValue<ITestModule>();
            set => SetValue(value);
        }

        public ITestModule SecondModule
        {
            get => GetValue<ITestModule>();
            set => SetValue(value);
        }
    }
}
