using System;

namespace CardTowers_StateDelta_Generator.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateStateDelta : Attribute
    {
        // See the attribute guidelines at 
        //  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/attributes
        public GenerateStateDelta()
        {
        }
    }
}


