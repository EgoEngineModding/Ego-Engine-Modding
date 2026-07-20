using Avalonia.Controls;
using Avalonia.Media;

namespace EgoEngineLibrary.Frontend.Dialogs.MessageBox;

public partial class MessageBoxView : UserControl
{
    public MessageBoxView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not MessageBoxViewModel dataContext)
        {
            return;
        }
        
        Image.IsVisible = true;
        switch (dataContext.Icon)
        {
            case MessageBoxImage.None:
                Image.Data = null;
                Image.IsVisible = false;
                break;
            case MessageBoxImage.Error:
                Image.Data = this.FindResource(null, "ErrorCircleRegular") as Geometry;
                Image.Foreground = Brushes.Firebrick;
                break;
            case MessageBoxImage.Question:
                Image.Data = this.FindResource(null, "QuestionCircleRegular") as Geometry;
                Image.Foreground = Brushes.SteelBlue;
                break;
            case MessageBoxImage.Exclamation:
                Image.Data = this.FindResource(null, "WarningRegular") as Geometry;
                Image.Foreground = Brushes.Goldenrod;
                break;
            case MessageBoxImage.Asterisk:
                Image.Data = this.FindResource(null, "InfoRegular") as Geometry;
                Image.Foreground = Brushes.SteelBlue;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataContext.Icon), dataContext.Icon, null);
        }
    }
}