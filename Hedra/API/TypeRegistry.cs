using System;

namespace Hedra.API
{
    public abstract class TypeRegistry : Registry<string, Type>
    {
        protected abstract Type RegistryType { get; }

        protected override void MeetsRequirements(string Name, Type ClassType)
        {
            if (!ClassType.IsSubclassOf(RegistryType) || ClassType.IsAbstract)
                throw new ArgumentOutOfRangeException(
                    $"{ClassType.FullName} needs to inherit from {RegistryType.FullName} or be concrete.");
        }
    }
}