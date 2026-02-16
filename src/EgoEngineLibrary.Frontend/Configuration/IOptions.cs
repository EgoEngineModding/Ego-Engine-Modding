using System.Diagnostics.CodeAnalysis;

namespace EgoEngineLibrary.Frontend.Configuration;

public interface IOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] out TOptions>
    where TOptions : class
{
    TOptions Value { get; }
}
