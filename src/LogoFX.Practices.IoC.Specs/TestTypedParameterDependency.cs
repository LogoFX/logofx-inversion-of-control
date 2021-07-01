namespace LogoFX.Practices.IoC.Specs
{
    internal class TestTypedParameterDependency : ITestTypedParameterDependency
    {
        public TestTypedParameterDependency(int val)
        {
            Value = val;
        }

        public int Value { get; private set; }
    }
}