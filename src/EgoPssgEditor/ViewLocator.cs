using Avalonia.Controls;
using EgoPssgEditor.ViewModels;
using EgoPssgEditor.Views;

namespace EgoPssgEditor;

/// <inheritdoc/>
public sealed class ViewLocator : EgoEngineLibrary.Frontend.ViewLocator
{
    /// <inheritdoc/>
    public override Control? Build(object? param)
    {
        return param switch
        {
            AddAttributeViewModel => new AddAttributeView(),
            AddElementViewModel => new AddElementView(),
            DuplicateTextureViewModel => new DuplicateTextureView(),
            _ => base.Build(param)
        };
    }
}
