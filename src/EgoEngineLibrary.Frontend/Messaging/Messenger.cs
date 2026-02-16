using CommunityToolkit.Mvvm.Messaging;

namespace EgoEngineLibrary.Frontend.Messaging;

public static class Messenger
{
    public static IMessenger Default { get; set; } = WeakReferenceMessenger.Default;
}
