namespace LogoFX.Practices.IoC.Specs
{
    internal class TestNamedParameterDependency : ITestNamedParameterDependency
    {
        public TestNamedParameterDependency(string model)
        {
            Model = model;
        }

        public string Model { get; private set; }
    }
}