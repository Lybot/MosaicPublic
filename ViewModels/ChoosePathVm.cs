using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MozaikaApp.Models;
using MozaikaApp.Properties;
using Prism.Commands;
using Prism.Mvvm;

namespace MozaikaApp.ViewModels
{
    public class ChoosePathVm:BindableBase
    {
        private ChoosePathModel _model = new ChoosePathModel();
        public string WorkFolder => _model.WorkFolder;

        public ObservableCollection<HotFolder> HotFoldersList => _model.HotFoldersList;
        public bool IsEditable => Activation.IsEditable;
        public DelegateCommand ChooseWorkFolder { get; set; }
        public DelegateCommand ChooseSpectatingFolder { get; set; }
        public DelegateCommand ChooseSecondSpectatingFolder { get; set; }
        public DelegateCommand DefaultFolders { get; set; }
        public DelegateCommand AddListElement { get; set; }
        public DelegateCommand OnClosing { get; set; }
        public ChoosePathVm()
        {
            _model.PropertyChanged += (s, e) => RaisePropertyChanged(e.PropertyName);
            ChooseWorkFolder = new DelegateCommand(_model.ChooseWorkFolder);
            AddListElement = new DelegateCommand(_model.AddListElement);
            OnClosing = new DelegateCommand(_model.OnClosing);
        }
    }
    public class HotFolder:BindableBase
    {
        private string _folderPath;

        public string FolderPath
        {
            get => _folderPath;
            set
            {
                _folderPath = value;
                RaisePropertyChanged($"FolderPath");
            }
        }

        public DelegateCommand<string> DeletePath { get; set; }
        public DelegateCommand AddPath { get; set; }
    }
}
