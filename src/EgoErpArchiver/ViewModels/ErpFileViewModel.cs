using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoErpArchiver.ViewModels;

public class ErpFileViewModel : ViewModelBase
{
    public string FilePath { get; set; } = string.Empty;
    
    public ErpFile? File { get; set; }

    public ErpFileViewModel()
    {
    }
}
