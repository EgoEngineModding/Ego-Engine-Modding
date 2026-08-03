using EgoEngineLibrary.Frontend.Dialogs;
using EgoErpArchiver.ViewModels;

namespace EgoErpArchiver.Dialogs.Erp;

public static class ErpDialog
{
    public static Task ShowProgressDialog(ProgressDialogViewModel viewModel)
    {
        return DialogService.Instance.ShowProgressDialog(viewModel);
    }

    public static Task ShowProgressDialog(this IDialogService dialogService, ProgressDialogViewModel viewModel)
    {
        return dialogService.ShowDialogAsync(viewModel);
    }
}
