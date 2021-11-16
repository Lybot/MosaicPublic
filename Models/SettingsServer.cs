using MozaikaApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MozaikaApp.Models
{
    public class SettingsServer
    {
        public int CountWidth { get; set; }
        public int CountHeight { get; set; }
        public int FullWidth { get; set; }
        public int FullHeight { get; set; }
        public List<Question> Questions { get; set; }
        public string PreviewText { get; set; }
        public bool ImageVisibility { get; set; }
        public AddButtonInfo AddPhotoButton { get; set; }
        public LinkButtonInfo LinkButton { get; set; }
        public bool ScreenSaver { get; set; }
    }

    public class LinkButtonInfo
    {
        public bool Enabled { get; set; }
        public string Link { get; set; }
        public string Text { get; set; }
    }
    public class AddButtonInfo
    {
        public bool Enabled { get; set; }
        public string Text { get; set; }
    }
}
