using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Frontend.Dialogs.File;
using EgoEngineLibrary.Frontend.Dialogs.MessageBox;
using EgoEngineLibrary.Frontend.ViewModels;

namespace EgoJpkArchiver.ViewModels
{
    public sealed partial class MainViewModel : ViewModelBase
    {
        private const string ManifestTxtFileName = "manifest.jpk.txt";
        JpkFile _file;
        string _fileName = "raceload.jpk";

        public override string DisplayName
        {
            get;
            protected set
            {
                field = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<JpkEntry> Entries { get; }

        public MainViewModel()
        {
            DisplayName = $"{Properties.Resources.AppTitleLong} {Properties.Resources.AppVersionShort}";
            Entries = [];
            _file = new JpkFile();
        }

        public void ParseCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                this.ReadJpk(args[1]);
            }
        }

        [RelayCommand]
        private async Task CreateFromFolder()
        {
            try
            {
                var folderOptions = new FolderOpenOptions
                {
                    Title = "Select a folder to create a new jpk:",
                };
                var res = await FileDialog.ShowOpenFolderDialog(folderOptions);
                if (res.Count > 0)
                {
                    CreateJpk(res[0]);
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show($"Failed to create!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Open()
        {
            try
            {
                var openOptions = new FileOpenOptions
                {
                    FileName = _fileName,
                    FileTypeChoices = [FilePickerType.Jpk, FilePickerType.All],
                };
                var res = await FileDialog.ShowOpenFileDialog(openOptions);
                if (res.Count > 0)
                {
                    this.ReadJpk(res[0]);
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show($"Failed to open!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            try
            {
                var saveOptions = new FileSaveOptions
                {
                    FileName = _fileName,
                    FileTypeChoices = [FilePickerType.Jpk, FilePickerType.All],
                };
                var res = await FileDialog.ShowSaveFileDialog(saveOptions);
                if (res is not null)
                {
                    this.WriteJpk(res);
                }
            }
            catch (Exception ex)
            {
                await MessageBox.Show($"Failed to save!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateJpk(string folderPath)
        {
            Entries.Clear();
            _file = new JpkFile();
            var manifestFilePath = Path.Combine(folderPath, ManifestTxtFileName);
            var files = File.Exists(manifestFilePath)
                ? File.ReadAllLines(manifestFilePath).Select(x => Path.Combine(folderPath, x))
                : Directory.EnumerateFiles(folderPath);
            foreach (string f in files)
            {
                var entry = new JpkEntry(_file) { Name = Path.GetFileName(f) };
                using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                entry.Import(fs);
                _file.Entries.Add(entry);
                Entries.Add(entry);
            }

            SetFileName($"{Path.GetFileName(folderPath)}.jpk");
            SetTitle();
        }

        private void ReadJpk(string filePath)
        {
            Entries.Clear();
            _file = new JpkFile();
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _file.Read(fs);
            foreach (var entry in _file.Entries)
            {
                Entries.Add(entry);
            }

            SetFileName(filePath);
            SetTitle();
        }

        private void WriteJpk(string filePath)
        {
            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _file.Write(fs);

            SetFileName(filePath);
            SetTitle();
        }

        private void SetFileName(string filePath)
        {
            _fileName = Path.GetFileName(filePath);
        }

        private void SetTitle()
        {
            DisplayName = $"{Properties.Resources.AppTitleShort} {Properties.Resources.AppVersionShort} - {_fileName}";
        }

        [RelayCommand]
        private async Task Export()
        {
            var folderOptions = new FolderOpenOptions
            {
                Title = "Select a target folder to export the files:",
            };
            var res = await FileDialog.ShowOpenFolderDialog(folderOptions);
            if (res.Count > 0)
            {
                try
                {
                    foreach (JpkEntry entry in this._file.Entries)
                    {
                        using var fs = File.Open(Path.Combine(res[0], entry.Name),
                            FileMode.Create, FileAccess.Write, FileShare.Read);
                        entry.Export(fs);
                    }

                    File.WriteAllLines(Path.Combine(res[0], ManifestTxtFileName),
                        this._file.Entries.Select(x => x.Name));
                }
                catch (Exception ex)
                {
                    await MessageBox.Show($"Failed to export!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task Import()
        {
            var folderOptions = new FolderOpenOptions
            {
                Title = "Select a source folder to import the files:",
            };
            var res = await FileDialog.ShowOpenFolderDialog(folderOptions);
            if (res.Count > 0)
            {
                try
                {
                    // Import
                    Entries.Clear();
                    foreach (string f in Directory.EnumerateFiles(res[0]))
                    {
                        var fileName = Path.GetFileName(f);
                        if (this._file.Contains(fileName))
                        {
                            using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                            this._file[fileName].Import(fs);
                        }
                    }

                    foreach (var entry in  _file.Entries)
                    {
                        Entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    await MessageBox.Show($"Failed to import!{Environment.NewLine}{ex}", Properties.Resources.AppTitleLong,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
