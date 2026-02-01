using Avalonia.Controls;

using CommunityToolkit.Mvvm.Messaging;

using EgoPssgEditor.Views;

namespace EgoPssgEditor.Dialogs.Pssg;

public class PssgDialogAvalonia
{
    public static void Register(Window recipient)
    {
        PssgDialog.Messenger.Register<Window, AddNodeMessage>(recipient, AddNodeHandler);
        PssgDialog.Messenger.Register<Window, AddAttributeMessage>(recipient, AddAttributeHandler);
        PssgDialog.Messenger.Register<Window, DuplicateTextureMessage>(recipient, DuplicateTextureHandler);
    }

    public static void Unregister(Window recipient)
    {
        PssgDialog.Messenger.Unregister<AddNodeMessage>(recipient);
        PssgDialog.Messenger.Unregister<AddAttributeMessage>(recipient);
        PssgDialog.Messenger.Unregister<DuplicateTextureMessage>(recipient);
    }

    private static void AddNodeHandler(Window recipient, AddNodeMessage message)
    {
        message.Reply(ShowAddNode(recipient));
        return;

        static async Task<string?> ShowAddNode(Window owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            AddNodeWindow win = new AddNodeWindow();
            var res = await win.ShowDialog<bool>(owner);
            return res ? win.NodeName : null;
        }
    }

    private static void AddAttributeHandler(Window recipient, AddAttributeMessage message)
    {
        message.Reply(ShowAddAttribute(recipient));
        return;

        static async Task<AddAttributeResponse?> ShowAddAttribute(Window owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            AddAttributeWindow win = new();
            var res = await win.ShowDialog<bool>(owner);
            return res
                ? new AddAttributeResponse { Name = win.AttributeName, Value = win.Value, Type = win.AttributeValueType, }
                : null;
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
