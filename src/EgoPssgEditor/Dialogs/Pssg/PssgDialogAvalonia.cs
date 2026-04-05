using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using EgoPssgEditor.Views;

namespace EgoPssgEditor.Dialogs.Pssg;

public class PssgDialogAvalonia
{
    public static void Register(Window recipient)
    {
        PssgDialog.Messenger.Register<Window, AddElementMessage>(recipient, AddElementHandler);
        PssgDialog.Messenger.Register<Window, DuplicateTextureMessage>(recipient, DuplicateTextureHandler);
    }

    public static void Unregister(Window recipient)
    {
        PssgDialog.Messenger.Unregister<AddElementMessage>(recipient);
        PssgDialog.Messenger.Unregister<DuplicateTextureMessage>(recipient);
    }

    private static void AddElementHandler(Window recipient, AddElementMessage message)
    {
        message.Reply(Show(recipient));
        return;

        static async Task<string?> Show(Window owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            AddElementWindow win = new();
            var res = await win.ShowDialog<bool>(owner);
            return res ? win.ElementName : null;
        }
    }

    private static void DuplicateTextureHandler(Window recipient, DuplicateTextureMessage message)
    {
        message.Reply(ShowDuplicateTexture(recipient, message.ViewModel));
        return;

        static async Task<bool> ShowDuplicateTexture(Window owner, DuplicateTextureViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(owner);

            DuplicateTextureWindow win = new() { TextureName = viewModel.TextureName };
            var res = await win.ShowDialog<bool>(owner);
            viewModel.TextureName = win.TextureName;
            return res;
        }
    }
}
