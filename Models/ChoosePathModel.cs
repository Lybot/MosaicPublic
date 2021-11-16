using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Commands;
using Prism.Mvvm;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MozaikaApp.Models
{
    class ChoosePathModel:BindableBase
    {
        private readonly string _foregroundViewImages = "ForegroundViewImages";
        private readonly string _foregroundPrintImages = "ForegroundPrintImages";
        private readonly string _foregroundImage = "SavedForegroundImage";
        private readonly string _sizeViewImage = "ViewImages";
        private readonly string _sizePrintImage = "PrintImages";
        private readonly string _cuttedImages = "CutImages";
        private readonly string _overlaidImages = "OverlaidImages";
        private readonly string _serverInfo = "ServerInfo";

        public StringCollection HotFolders
        {
            get => Settings.Default.HotFolders;
            set
            {
                Settings.Default.HotFolders = value;
                Settings.Default.Save();
            }
        }
        public ObservableCollection<HotFolder> HotFoldersList { get; set; }
        public string WorkFolder
        {
            get => Settings.Default.WorkFolder;
            set
            {
                if (!string.IsNullOrEmpty(Settings.Default.WorkFolder))
                {
                    try
                    {
                        var directory = new DirectoryInfo(Settings.Default.WorkFolder);
                        var subDir = directory.GetDirectories();
                        foreach (var dir in subDir)
                        {
                            if (dir.Name == _foregroundViewImages || dir.Name == _foregroundPrintImages || dir.Name == _foregroundImage || dir.Name == _sizeViewImage || dir.Name == _sizePrintImage || dir.Name == _cuttedImages|| dir.Name == _overlaidImages|| dir.Name == _serverInfo)
                                dir.Delete(true);
                        }
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
                Settings.Default.WorkFolder = value;
                Settings.Default.Save();
                RaisePropertyChanged($"WorkFolder");
                RaisePropertyChanged();
                var newDir = new DirectoryInfo(value);
                newDir.CreateSubdirectory(_foregroundViewImages);
                newDir.CreateSubdirectory(_foregroundPrintImages);
                newDir.CreateSubdirectory(_foregroundImage);
                newDir.CreateSubdirectory(_sizeViewImage);
                newDir.CreateSubdirectory(_sizePrintImage);
                newDir.CreateSubdirectory(_cuttedImages);
                newDir.CreateSubdirectory(_overlaidImages);
                newDir.CreateSubdirectory(_serverInfo);
            }
        }
        public void ChooseWorkFolder()
        {
            var dialog = new FolderBrowserDialog {RootFolder = Environment.SpecialFolder.MyComputer};
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    try
                    {
                        WorkFolder = dialog.SelectedPath;
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            }
        }
        public void ChooseForegroundPath()
        {
            if (string.IsNullOrEmpty(Settings.Default.WorkFolder))
            {
                MessageBox.Show(Functions.FindStringResource("PictureError"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Settings.Default.ForegroundPath = dialog.FileName;
                RaisePropertyChanged($"ForegroundPath");
                Settings.Default.Save();
            }
        }
        public void DeleteFolder(string fld)
        {
            HotFolders.Remove(fld);
            var item = HotFoldersList.FirstOrDefault(find => fld == find.FolderPath);
            HotFoldersList.Remove(item);
            RaisePropertyChanged($"HotFoldersList");
        }

        public string AddPath(string path)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            return path;
        }

        public void OnClosing()
        {
            HotFolders = new StringCollection();
            foreach (var hotFolder in HotFoldersList)
            {
                if (!string.IsNullOrEmpty(hotFolder.FolderPath))
                    HotFolders.Add(hotFolder.FolderPath);
            }
        }
        public void AddListElement()
        {
            var firstFolder = new HotFolder() { DeletePath = new DelegateCommand<string>(DeleteFolder), FolderPath = "" };
            firstFolder.AddPath = new DelegateCommand(() =>
            {
                firstFolder.FolderPath = AddPath(firstFolder.FolderPath);
                RaisePropertyChanged($"FolderPath");
                OnClosing();
            });
            HotFoldersList.Add(firstFolder);
            RaisePropertyChanged($"HotFoldersList");
            RaisePropertyChanged($"HotFolders");
        }
        public ChoosePathModel()
        {
            HotFoldersList = new ObservableCollection<HotFolder>();
            if (HotFolders==null)
                HotFolders = new StringCollection();
            foreach (var folder in HotFolders)
            {
                var hotFolder = new HotFolder()
                    {DeletePath = new DelegateCommand<string>(DeleteFolder), FolderPath = folder};
                hotFolder.AddPath = new DelegateCommand(() =>
                {
                    hotFolder.FolderPath = AddPath(hotFolder.FolderPath);
                    RaisePropertyChanged($"FolderPath");
                });
                HotFoldersList.Add(hotFolder);
            }
            if (HotFoldersList.Count == 0)
            {
                AddListElement();
            }
        }
    }
}
