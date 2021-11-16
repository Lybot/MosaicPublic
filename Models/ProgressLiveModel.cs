using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Emgu.CV.Structure;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using MozaikaApp.Views;
using Newtonsoft.Json;
using Prism.Mvvm;
using Xceed.Wpf.Toolkit.Core.Input;
using Image = System.Drawing.Image;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
namespace MozaikaApp.Models
{
    public class ProgressLiveModel : BindableBase
    {
        private readonly Action _fullScreenAction = () =>
        {
            Thread.Sleep(400);
            Disp.Invoke(FullScreen);
        };
        private Random Random { get; } = new Random();
        private static Queue<int> RandomList { get; set; } = new Queue<int>();
        private readonly List<SecondScreenView> _screenList = new List<SecondScreenView>();
        private List<string> _printPaths = new List<string>();
        private int CountFrequencyLink => Settings.Default.CountFrequencyLink;
        private int _donePhotos;
        private int _needPhotos;
        private bool UseSerialLink => Settings.Default.UseSerialLink;
        private bool SerialLinkQuality => Settings.Default.SerialLinkQuality;
        private bool SerialLinkAddresses => Settings.Default.SerialLinkAddresses;
        private string _stageStatus;
        public string StageStatus
        {
            get => _stageStatus;
            set
            {
                _stageStatus = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"ReadyStatus");
            }
        }

        public string ReadyStatus => StageStatus + DonePhotos + "/" + NeedPhotos;
        public static Dispatcher Disp { get; set; }
        public Visibility VisiblePuzzle { get; set; } = Visibility.Collapsed;
        public Visibility VisibleMenu { get; set; } = Visibility.Visible;
        public Visibility VisibleMenuButtons { get; set; } = Visibility.Hidden;
        public Visibility VisibleLoading { get; set; } = Visibility.Collapsed;
        public KeyModifierCollection KeyModifiers { get; } = new KeyModifierCollection() { KeyModifier.None };
        public ObservableCollection<BaseThing> LiveCanvas { get; set; }
        public FileSystemWatcher PrintWatcher { get; set; }
        public List<FileSystemWatcher> FileWatchers { get; set; } = new List<FileSystemWatcher>();
        public List<string> PrintedImages { get; set; } = new List<string>();
        public List<string> AddedPhoto { get; set; } = new List<string>();
        public List<string> PrintPaths
        {
            get => _printPaths;
            set
            {
                _printPaths = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"PrintStatus");
                RaisePropertyChanged($"ReadyStatus");
            }
        }
        public static int SelectedX { get; set; }
        public static int SelectedY { get; set; }

        public int DonePhotos
        {
            get => _donePhotos;
            set
            {
                _donePhotos = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"ReadyStatus");
            }
        }
        public int NeedPhotos
        {
            get => _needPhotos;
            set
            {
                _needPhotos = value;
                RaisePropertyChanged();
                RaisePropertyChanged($"ReadyStatus");
            }
        }
        public int CountCells => Settings.Default.CountHeight * Settings.Default.CountWidth;
        public int CountFill { get; set; }
        public int FreeCells => CountCells - Added;
        public int Added { get; set; }
        public int PrintCount => Settings.Default.CountPrinterPhotos;
        public int HeightCanvas => SizePrint.Height;
        public int WidthCanvas => SizePrint.Width;
        public Size SizePrint => PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.ViewDpi,
            Settings.Default.CountWidth, Settings.Default.CountHeight);
        public static Queue<Task> QueueTasks { get; set; } = new Queue<Task>();
        public bool IsQueueActive { get; set; }
        public bool UseSeveralPhotos
        {
            get
            {
                if (Settings.Default.UseSeveralPhotos && Settings.Default.CountPrinterPhotos > 0)
                    return true;
                return false;
            }
        }
        public string PauseContent { get; set; } = Functions.FindStringResource("ResetCanvas");
        private event EventHandler CamePhoto;
        public static event EventHandler UpdateTab;
        public static event EventHandler WasStart;
        public static event EventHandler<ImageVm> SecondScreen;
        public static event EventHandler<ImageVm> DeleteImage;
        public static Action FullScreen;
        //private List<SecondScreenView> Screens { get; set; } = new List<SecondScreenView>();
        public void DeleteCell()
        {
            if (SelectedX == 0 || SelectedY == 0)
                return;
            var temp = LiveCanvas.LastOrDefault(cell => Equals(cell.Tag, Tuple.Create(SelectedX, SelectedY)));
            if (temp is ImageVm selectedCell)
            {
                var index = LiveCanvas.IndexOf(selectedCell);
                if (LiveCanvas[index] is ImageVm temp2)
                {
                    temp2.Source = null;
                    RaisePropertyChanged($"LiveCanvas");
                    LiveCanvas.Remove(temp2);
                    DeleteImage?.Invoke(null, temp2);
                    GC.Collect(GC.GetGeneration(temp2), GCCollectionMode.Forced);
                }
                RaisePropertyChanged($"LiveCanvas");
                //var label = LiveCanvas.FirstOrDefault(cell => Equals(cell.Tag, Tuple.Create(SelectedX, SelectedY)));
                //var count = LiveCanvas.IndexOf(label);
                var count = Settings.Default.CountWidth * (temp.Tag.Item1 - 1) +
                            temp.Tag.Item2 - 1;
                LiveCanvas[count].Tag = selectedCell.Tag;
                Added -= 1;
                RandomList.Enqueue(count);
                //RaisePropertyChanged($"LiveCanvas");
                RaisePropertyChanged($"Added");
                RaisePropertyChanged($"FreeCells");

                try
                {
                    File.Delete(Settings.Default.WorkFolder + "\\ViewImages\\" +
                                $"R{selectedCell.Tag.Item1}C{selectedCell.Tag.Item2}.png");
                    File.Delete(Settings.Default.WorkFolder + "\\PrintImages\\" +
                                $"R{selectedCell.Tag.Item1}C{selectedCell.Tag.Item2}.png");
                    File.Delete(Settings.Default.WorkFolder + "\\CutImages\\" +
                                $"R{selectedCell.Tag.Item1}C{selectedCell.Tag.Item2}.png");
                }
                catch (Exception)
                {
                    //ignored
                }
                //GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            }
        }
        public bool ProcessNewPhoto(string filename, string path, bool copied)
        {
            var firstBgr = new Bgra(Settings.Default.FirstStringColor.B, Settings.Default.FirstStringColor.G, Settings.Default.FirstStringColor.R, Settings.Default.FirstStringColor.A);
            var secondBgr = new Bgra(Settings.Default.SecondStringColor.B, Settings.Default.SecondStringColor.G, Settings.Default.SecondStringColor.R, Settings.Default.SecondStringColor.A);
            bool success = PhotoProcessing.ProcessImage(Settings.Default.HeightMm, Settings.Default.WidthMm, Settings.Default.PrintDpi, Settings.Default.ViewDpi, Settings.Default.Alpha, Settings.Default.Beta, Settings.Default.Gamma, Settings.Default.Transparency,
                firstBgr, secondBgr, Settings.Default.TextSize, Settings.Default.ThicknessSize, filename, path, Settings.Default.WorkFolder + "\\CutImages",
                Settings.Default.WorkFolder + "\\ViewImages", Settings.Default.WorkFolder + "\\PrintImages", Settings.Default.WorkFolder + "\\OverlaidImages", Settings.Default.WorkFolder + "\\ForegroundViewImages\\" + filename,
                Settings.Default.WorkFolder + "\\ForegroundPrintImages\\" + filename, copied, Settings.Default.FaceMode, Settings.Default.ProMode);
            return success;
        }
        public void NewPuzzle()
        {
            //DialogResult result = MessageBox.Show(@"Начать новую мозайку? (старые фото будут удалены)", @"Подтвердите действие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //if (result == DialogResult.Yes)
            //{
            VisibleMenu = Visibility.Collapsed;
            VisibleLoading = Visibility.Visible;
            RaisePropertyChanged($"VisibleMenu");
            RaisePropertyChanged($"VisibleLoading");
            while (LiveCanvas.Count == 0)
            {
                var min = (int)Math.Min(Settings.Default.WidthMm, Settings.Default.HeightMm);
                LiveCanvas = Functions.Generate(Settings.Default.ViewDpi, Settings.Default.WidthMm, Settings.Default.HeightMm,
                    Settings.Default.CountWidth,
                    Settings.Default.CountHeight, Math.Max(min / 3, 1), 1);
            }
            Added = 0;
            if (Settings.Default.FillAlgorithm?.Count > 0 && Settings.Default.FillAlgorithm.TrueForAll(value => value <= CountCells))
            {
                RandomList = Functions.GenerateRandomList(CountCells, Settings.Default.FillAlgorithm, Random);
            }
            else
                RandomList = Functions.GenerateRandomList(CountCells, Random);
            RaisePropertyChanged($"LiveCanvas");
            RaisePropertyChanged($"Added");
            RaisePropertyChanged($"FreeCells");
            RaisePropertyChanged($"CountCells");
            Activation.IsEditable = false;
            var task = new Task(delegate { CutForegroundImage(() =>
            {
                VisibleLoading = Visibility.Collapsed;
                VisiblePuzzle = Visibility.Visible;
                VisibleMenuButtons = Visibility.Visible;
                RaisePropertyChanged($"VisiblePuzzle");
                RaisePropertyChanged($"VisibleMenuButtons");
                RaisePropertyChanged($"VisibleLoading");
                //Activation.WasStart = true;
                WasStart?.Invoke("", new EventArgs());
                Task.Run(_fullScreenAction);
                Pause();
                return true;
            }); });
            task.Start();
            //task.GetAwaiter().OnCompleted(delegate
            //{
            //    VisibleLoading = Visibility.Collapsed;
            //    VisiblePuzzle = Visibility.Visible;
            //    VisibleMenuButtons = Visibility.Visible;
            //    RaisePropertyChanged($"VisiblePuzzle");
            //    RaisePropertyChanged($"VisibleMenuButtons");
            //    RaisePropertyChanged($"VisibleLoading");
            //    //Activation.WasStart = true;
            //    WasStart?.Invoke("", new EventArgs());
            //    Task.Run(_fullScreenAction);
            //    Pause();
            //});
            SaveWebSettings();
            //}
            //else if (result == DialogResult.No)
            //{
            //    // какое-то действие при нажатии на НЕТ
            //}
        }
        public void InstagramLoader()
        {
            while (true)
            {
                Thread.Sleep(5000);
                if (!Settings.Default.InstagramPhotoLoading) return;
                var tempString = "{date_local}";
                StringBuilder argument = new StringBuilder();
                argument.Append(Settings.Default.InstagramCommandString + $"--filename-pattern={Settings.Default.HotFolders[0]}\\{tempString}_time ");
                foreach (var hashTag in Settings.Default.InstagramHashTags)
                {
                    argument.Append($"\"{hashTag}\" ");
                }
                var process = new ProcessStartInfo {FileName = Environment.CurrentDirectory+"\\instaloader.exe", Arguments = argument.ToString(), UseShellExecute = false, CreateNoWindow = true};
                Process.Start(process);
                Thread.Sleep(new TimeSpan(0, Settings.Default.InstagramUpdateMinutes, 0));
            }
        }

        public void ResetLiveCanvas()
        {
            VisibleLoading = Visibility.Visible;
            VisibleMenu = Visibility.Collapsed;
            RandomList = new Queue<int>();
            if (Settings.Default.RandomList == null)
                RandomList = Functions.GenerateRandomList(FreeCells, Random);
            else
                foreach (var element in Settings.Default.RandomList)
                {
                    RandomList.Enqueue(element);
                }
            RaisePropertyChanged($"VisibleMenu");
            RaisePropertyChanged($"VisibleLoading");
            var min = (int)Math.Min(Settings.Default.WidthMm, Settings.Default.HeightMm);
            LiveCanvas = Functions.Generate(Settings.Default.ViewDpi, Settings.Default.WidthMm, Settings.Default.HeightMm,
                    Settings.Default.CountWidth,
                    Settings.Default.CountHeight, Math.Max(min / 3, 1), 1);
            GC.SuppressFinalize(LiveCanvas);
            var task = new Task(delegate
            {
                DirectoryInfo dir = new DirectoryInfo(Settings.Default.WorkFolder + "\\ViewImages");
                var files = dir.GetFiles();
                Added = 0;
                foreach (var file in files)
                {
                    try
                    {
                        string fileName = file.Name.Remove(file.Name.Length - 4, 4);
                        int place = fileName.IndexOf('C');
                        int index1 = int.Parse(fileName.Substring(1, place - 1));
                        int index2 = int.Parse(fileName.Substring(place + 1));
                        var label = LiveCanvas.FirstOrDefault(check => Equals(check.Tag, new Tuple<int, int>(index1, index2)));
                        if (label != null)
                            Disp.Invoke(delegate
                            {
                                {
                                    LiveCanvas.Add(new ImageVm()
                                    {
                                        Height = label.Height,
                                        Left = label.Left,
                                        Width = label.Width,
                                        Top = label.Top,
                                        Tag = label.Tag,
                                        Source = Functions.BitmapFromUri(new Uri(file.FullName, UriKind.Absolute))
                                    });
                                    var count = Settings.Default.CountWidth * (index1 - 1) +
                                                index2 - 1;
                                    LiveCanvas[count].Tag = new Tuple<int, int>(0, 0);
                                    Added += 1;
                                }
                            });
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            });
            task.GetAwaiter().OnCompleted(delegate
            {
                VisiblePuzzle = Visibility.Visible;
                VisibleLoading = Visibility.Collapsed;
                VisibleMenuButtons = Visibility.Visible;
                RaisePropertyChanged($"VisibleLoading");
                RaisePropertyChanged($"VisibleMenuButtons");
                RaisePropertyChanged($"VisiblePuzzle");
                RaisePropertyChanged($"Added");
                RaisePropertyChanged($"LiveCanvas");
                RaisePropertyChanged($"FreeCells");
                Task.Run(_fullScreenAction);
            });
            Task.Run(task.Start);
        }
        public void FillCopied()
        {
            if (LiveCanvas == null || LiveCanvas.Count == 0)
                return;
            FileInfo[] files;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Settings.Default.WorkFolder + "\\CutImages");
                files = dir.GetFiles();
                if (files.Length == 0)
                    return;
            }
            catch
            {
                return;
            }
            for (int i = 0; i < CountFill; i++)
            {
                var file = files[Random.Next(files.Length)];
                if (file?.DirectoryName == null)
                    return;
                OnChanged(true, new FileSystemEventArgs(WatcherChangeTypes.Created, file.DirectoryName, file.Name));
            }
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                bool copied;
                try
                {
                    copied = (bool)source;
                }
                catch
                {
                    copied = false;
                }
                if (!AddedPhoto.Contains(e.FullPath) || copied)
                {
                    if (!copied)
                        AddedPhoto.Add(e.FullPath);
                    while (!Functions.IsFileReady(e.FullPath)&&!copied)
                    {
                        Thread.Sleep(50);
                    }
                    var task = new Task(() =>
                    {
                        if (RandomList.Count == 0)
                        {
                            if (Added != CountCells)
                            {
                                var list = new List<int>();
                                for (int j = 0; j < CountCells; j++)
                                {
                                    if (!LiveCanvas[j].Tag.Equals(new Tuple<int, int>(0, 0)))
                                        list.Add(j);
                                }
                                Disp.Invoke(delegate
                                {
                                    RandomList = Functions.GenerateRandomList(list.ToArray(), Random);
                                });
                                if (RandomList.Count == 0)
                                    return;
                            }
                            if (RandomList.Count == 0)
                                return;
                        }
                        var value = RandomList.Dequeue();
                        BaseThing label = LiveCanvas[value];
                        while (label.Tag.Equals(new Tuple<int, int>(0, 0)))
                        {
                            value = RandomList.Dequeue();
                            label = LiveCanvas[value];
                        }
                        string fileName = $"R{label.Tag.Item1}C{label.Tag.Item2}.png";
                        try
                        {
                            var formatLength = e.Name.Length- e.Name.LastIndexOf('.')-1;
                            File.Copy(Settings.Default.WorkFolder + "\\ServerInfo\\" + e.Name.Substring(0, e.Name.Length - formatLength) + "json", Settings.Default.WorkFolder + "\\ServerInfo\\" + fileName.Substring(0, fileName.Length - 3) + "json", true);
                            File.Delete(Settings.Default.WorkFolder + "\\ServerInfo\\" + e.Name.Substring(0, e.Name.Length - formatLength) + "json");
                        }
                        catch
                        {
                            //ignored
                        }
                        var success = ProcessNewPhoto(fileName, e.FullPath, copied);
                        if (success)
                        {
                            Disp.Invoke(delegate
                            {
                                var imageVm = new ImageVm()
                                {
                                    Height = label.Height,
                                    Left = label.Left,
                                    Width = label.Width,
                                    Tag = label.Tag,
                                    Top = label.Top,
                                    Source = Functions.BitmapFromUri(new Uri(
                                        Settings.Default.WorkFolder + "\\ViewImages\\" + fileName, UriKind.Absolute)),
                                    ZIndex = 0
                                };
                                LiveCanvas.Add(imageVm);
                                LiveCanvas[value].Tag = new Tuple<int, int>(0, 0);
                                RaisePropertyChanged($"Source");
                                RaisePropertyChanged($"LiveCanvas");
                                Added += 1;
                                RaisePropertyChanged($"CountCells");
                                RaisePropertyChanged($"Added");
                                RaisePropertyChanged($"LiveCanvas");
                                RaisePropertyChanged($"FreeCells");
                                SecondScreen?.Invoke("", imageVm);
                                if (!UseSerialLink) return;
                                if (Added % CountFrequencyLink == 0)
                                {
                                    SerialLink();
                                }
                            });
                        }
                        else
                        {
                            try
                            {
                                if (File.Exists((Settings.Default.WorkFolder + "\\ViewImages\\" + fileName)))
                                {
                                    var time = 0;
                                    while (!Functions.IsFileReady(
                                        Settings.Default.WorkFolder + "\\ViewImages\\" + fileName))
                                    {
                                        Thread.Sleep(100);
                                        time += 50;
                                        if (time == 1000)
                                            break;
                                    }

                                    File.Delete(Settings.Default.WorkFolder + "\\ViewImages\\" + fileName);
                                }

                                if (File.Exists((Settings.Default.WorkFolder + "\\PrintImages\\" + fileName)))
                                {
                                    var time = 50;
                                    while (!Functions.IsFileReady(
                                        Settings.Default.WorkFolder + "\\PrintImages\\" + fileName))
                                    {
                                        Thread.Sleep(100);
                                        time += 50;
                                        if (time == 1000)
                                            break;
                                    }

                                    File.Delete(Settings.Default.WorkFolder + "\\PrintImages\\" + fileName);
                                }
                            }
                            finally
                            {
                                RandomList.Enqueue(value);
                                MessageBox.Show(Functions.FindStringResource("ProcessError") + $@" {e.Name}", Functions.FindStringResource($"Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    });
                    Disp.Invoke(delegate
                    {
                        QueueTasks.Enqueue(task);
                    });
                    CamePhoto?.Invoke("", new EventArgs());
                }
            }
            catch
            {
                MessageBox.Show(Functions.FindStringResource("ProcessError") + $@" {e.Name}", Functions.FindStringResource($"Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PrintNow()
        {
            if (PrintPaths.Count > 0)
            {
                PrintImage(PrintPaths);
                PrintPaths.Clear();
                RaisePropertyChanged($"PrintStatus");
            }
        }
        public void PrintImage()
        {
            var fileName = Settings.Default.WorkFolder + "\\PrintImages\\" +
                  $"R{SelectedX}C{SelectedY}.png";
            if (UseSeveralPhotos)
            {
                if (PrintPaths.Count < Settings.Default.CountPrinterPhotos)
                {
                    PrintPaths.Add(fileName);
                    if (PrintPaths.Count == Settings.Default.CountPrinterPhotos)
                    {
                        PrintImage(PrintPaths);
                        PrintPaths.Clear();

                    }
                    RaisePropertyChanged($"PrintStatus");
                }
            }
            else
            {
                PrintImage(fileName);
            }
        }
        public void PrintImage(string fileName)
        {
            try
            {
                using var pd = new PrintDocument();
                pd.PrintPage += (o, e) =>
                {
                    {
                        using Stream stream = new MemoryStream();
                        var bitmap = new Bitmap(fileName);
                        bitmap.SetResolution(Settings.Default.PrintDpi, Settings.Default.PrintDpi);
                        bitmap.Save(stream, ImageFormat.Png);
                        var img = Image.FromStream(stream);
                        if (Settings.Default.UseIndents)
                        {
                            e.Graphics.DrawImage(img, new PointF((float)Settings.Default.LeftIndent, (float)Settings.Default.TopIndent));
                        }
                        else
                            e.Graphics.DrawImage(img, new Point(0, 0));
                        bitmap.Dispose();
                        stream.Dispose();
                    }
                };
                var printer = new PrinterSettings { PrinterName = Settings.Default.FirstPrinter };
                pd.PrinterSettings = printer;
                pd.Print();
            }
            catch
            {
                // ignored
            }
        }
        public void PrintImage(List<string> paths)
        {
            try
            {
                using var pd = new PrintDocument();
                pd.PrintPage += (o, e) =>
                {
                    {
                        var i = 0;
                        foreach (var fileName in paths)
                        {
                            using Stream stream = new MemoryStream();
                            var bitmap = new Bitmap(fileName);
                            bitmap.SetResolution(Settings.Default.PrintDpi, Settings.Default.PrintDpi);
                            bitmap.Save(stream, ImageFormat.Png);
                            var img = Image.FromStream(stream);
                            var point = PhotoProcessing.CalculatePixelsPoint(Settings.Default.SeveralPhotosIntends[i].X,
                                Settings.Default.SeveralPhotosIntends[i].Y, 100);
                            e.Graphics.DrawImage(img, point);
                            bitmap.Dispose();
                            stream.Dispose();
                            i++;
                        }
                    }
                };
                var printer = new PrinterSettings { PrinterName = Settings.Default.FirstPrinter };
                pd.PrinterSettings = printer;
                pd.Print();
            }
            catch
            {
                // ignored
            }
        }
        public static void MainWindow_Closing(object sender, EventArgs e)
        {
            var list = new List<int>();
            foreach (var value in RandomList)
            {
                list.Add(value);
            }
            Settings.Default.RandomList = list;
            Settings.Default.Save();
            foreach (var process in Process.GetProcessesByName("instaloader"))
            {
                process?.Kill();
            }
        }

        private void CamePhotoFunction(object sender, EventArgs e)
        {
            if (!IsQueueActive)
            {
                IsQueueActive = true;
                var task = new Task(() =>
                {
                    while (QueueTasks.Count > 0)
                    {
                        try
                        {
                            var queueTask = QueueTasks.Dequeue();
                            queueTask.Start();
                            queueTask.Wait();
                        }
                        catch
                        {
                            //ignored
                        }
                    }
                });
                task.Start();
                task.GetAwaiter().OnCompleted(delegate
                {
                    IsQueueActive = false;
                });
            }
        }

        private void ClearResources(object sender, EventArgs e)
        {
            var list = new List<int>();
            GC.ReRegisterForFinalize(LiveCanvas);
            foreach (var value in RandomList)
            {
                list.Add(value);
            }
            Settings.Default.RandomList = list;
            Settings.Default.Save();
            foreach (var element in LiveCanvas)
            {
                var image = element as ImageVm;
                if (image?.Source != null)
                {
                    image.Source = null;
                }
            }
            //GC.Collect();
        }

        private void PrintChanged(object sender, FileSystemEventArgs e)
        {
            if (!PrintedImages.Contains(e.FullPath))
            {
                PrintedImages.Add(e.FullPath);
                while (!Functions.IsFileReady(e.FullPath))
                {
                    Thread.Sleep(50);
                }
                if (UseSeveralPhotos)
                {
                    if (PrintPaths.Count < Settings.Default.CountPrinterPhotos)
                    {
                        PrintPaths.Add(e.FullPath);
                        if (PrintPaths.Count == Settings.Default.CountPrinterPhotos)
                        {
                            PrintImage(PrintPaths);
                            PrintPaths.Clear();

                        }
                        RaisePropertyChanged($"PrintStatus");
                    }
                }
                else
                    PrintImage(e.FullPath);
            }
        }
        public void CutForegroundImage(Func<bool> onCompleted)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundPrintImages");
                foreach (var file in dir.GetFiles())
                {
                    file.Delete();
                }

                DirectoryInfo dir2 = new DirectoryInfo(Settings.Default.WorkFolder + "\\PrintImages");
                foreach (var file in dir2.GetFiles())
                {
                    file.Delete();
                }

                DirectoryInfo dir3 = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundViewImages");
                foreach (var file in dir3.GetFiles())
                {
                    file.Delete();
                }

                DirectoryInfo dir4 = new DirectoryInfo(Settings.Default.WorkFolder + "\\ViewImages");
                foreach (var file in dir4.GetFiles())
                {
                    file.Delete();
                }

                DirectoryInfo dir5 = new DirectoryInfo(Settings.Default.WorkFolder + "\\CutImages");
                foreach (var file in dir5.GetFiles())
                {
                    file.Delete();
                }
                DirectoryInfo dir6 = new DirectoryInfo(Settings.Default.WorkFolder + "\\OverlaidImages");
                foreach (var file in dir6.GetFiles())
                {
                    file.Delete();
                }
            }
            catch (Exception)
            {
                //ignored
            }

            StageStatus = "Cut print images: ";
            Task<bool> cutPrintImage = new Task<bool>(delegate
            {
                bool resultTask;
                if (Settings.Default.ExperimentalProcess)
                    resultTask = PhotoProcessing.CutUnderImageExperimental(Settings.Default.ForegroundPath,
                        Settings.Default.WorkFolder + "\\ForegroundPrintImages",
                        Settings.Default.WidthMm, Settings.Default.HeightMm,
                        Settings.Default.PrintDpi, Settings.Default.CountWidth, Settings.Default.CountHeight,
                        result => {
                            DonePhotos = result;
                            return true;
                        }, result => {
                            NeedPhotos = result;
                            return true;
                        });
                else resultTask= PhotoProcessing.CutUnderImage(Settings.Default.ForegroundPath,
                    Settings.Default.WorkFolder + "\\" + "SavedForegroundImage" + "\\" + "printImage.png",
                    Settings.Default.WorkFolder + "\\ForegroundPrintImages", Settings.Default.WidthMm, Settings.Default.HeightMm,
                    Settings.Default.PrintDpi, Settings.Default.CountWidth, Settings.Default.CountHeight,
                    result => { 
                        DonePhotos = result;
                        return true;
                    }, result => { 
                        NeedPhotos = result;
                        return true;
                    });
                return resultTask;
            });
            cutPrintImage.ContinueWith(kek =>
            {
                if (!kek.Result)
                {
                    MessageBox.Show($"Error during cut photo. Common problem is big size of mosaic.",
                        Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    VisibleMenu = Visibility.Visible;
                        VisibleLoading = Visibility.Collapsed;
                        RaisePropertyChanged($"VisibleMenu");
                        RaisePropertyChanged($"VisibleLoading");
                        Activation.IsEditable = true;
                    return;
                }
                DonePhotos = 0;
                StageStatus = "Cut view images: ";
                Task cutViewImage = new Task(delegate
                {
                    if (Settings.Default.ExperimentalProcess)
                        PhotoProcessing.CutUnderImageExperimental(Settings.Default.ForegroundPath,
                            Settings.Default.WorkFolder + "\\ForegroundViewImages", Settings.Default.WidthMm, Settings.Default.HeightMm,
                            Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight, (result) => {
                                DonePhotos = result;
                                return true;
                            }, (result) => {
                                NeedPhotos = result;
                                return true;
                            });
                    else
                        PhotoProcessing.CutUnderImage(Settings.Default.ForegroundPath,
                        Settings.Default.WorkFolder + "\\" + "SavedForegroundImage" + "\\" + "viewImage.png",
                        Settings.Default.WorkFolder + "\\ForegroundViewImages", Settings.Default.WidthMm, Settings.Default.HeightMm,
                        Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight, (result) => {
                            DonePhotos = result;
                            return true;
                        }, (result) => {
                            NeedPhotos = result;
                            return true;
                        });
                });
                //ReadyStatus = "Cut view images: " + DonePhotos + "/" + NeedPhotos;
                cutViewImage.Start();
                cutViewImage.GetAwaiter().OnCompleted(delegate
                {
                    GC.Collect();
                    onCompleted();
                });
            });
            cutPrintImage.Start();
            //cutPrintImage.GetAwaiter().OnCompleted(delegate
            //{
            //    DonePhotos = 0;
            //    StageStatus = "Cut view images: ";
            //    Task cutViewImage = new Task(delegate
            //    {
            //        PhotoProcessing.CutUnderImage(Settings.Default.ForegroundPath,
            //            Settings.Default.WorkFolder + "\\" + "SavedForegroundImage" + "\\" + "viewImage.png",
            //            Settings.Default.WorkFolder + "\\ForegroundViewImages", Settings.Default.WidthMm, Settings.Default.HeightMm,
            //            Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight, (result) => {
            //                DonePhotos = result;
            //                return true;
            //            }, (result) => {
            //                NeedPhotos = result;
            //                return true;
            //            });
            //    });
            //    //ReadyStatus = "Cut view images: " + DonePhotos + "/" + NeedPhotos;
            //    cutViewImage.Start();
            //    cutViewImage.GetAwaiter().OnCompleted(delegate
            //    {
            //        GC.Collect();
            //        onCompleted();
            //    });
            //});
            //Task cutViewImage = new Task(delegate
            //{
            //    ReadyStatus = "Cut view images: " + DonePhotos + "/" + NeedPhotos;
            //    PhotoProcessing.CutUnderImage(Settings.Default.ForegroundPath,
            //        Settings.Default.WorkFolder + "\\" + "SavedForegroundImage" + "\\" + "viewImage.png",
            //        Settings.Default.WorkFolder + "\\ForegroundViewImages", Settings.Default.WidthMm, Settings.Default.HeightMm,
            //        Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight, (result) => { 
            //            DonePhotos = result;
            //            return true;
            //        }, (result) => {
            //            NeedPhotos = result;
            //            return true;
            //        });
            //});
            ////ReadyStatus = "Cut view images: " + DonePhotos + "/" + NeedPhotos;
            //cutPrintImage.ContinueWith()
            //cutViewImage.Wait();
            //GC.Collect();
            //if ()
        }
        public void Pause()
        {
            if (Activation.WasStart)
            {
                var list = new List<int>();
                foreach (var value in RandomList)
                {
                    list.Add(value);
                }
                Settings.Default.RandomList = list;
                Settings.Default.Save();
                foreach (var fileWatcher in FileWatchers)
                {
                    fileWatcher.EnableRaisingEvents = false;
                }
                Activation.WasStart = false;
                WasStart?.Invoke("", new EventArgs());
                PauseContent = Functions.FindStringResource("ResetCanvas");
                RaisePropertyChanged($"PauseContent");
                _cancellation?.Cancel();
                foreach (var process in Process.GetProcessesByName("instaloader"))
                {
                    process?.Kill();
                }
            }
            else
            {
                foreach (var fileWatcher in FileWatchers)
                {
                    fileWatcher.EnableRaisingEvents = true;
                }
                Activation.WasStart = true;
                WasStart?.Invoke("", new EventArgs());
                PauseContent = Functions.FindStringResource("Pause");
                RaisePropertyChanged($"PauseContent");
                _cancellation = new CancellationTokenSource();
                _instagramTask = new Task(InstagramLoader,_cancellation.Token);
                _instagramTask.Start();
            }
        }

        private CancellationTokenSource _cancellation;
        private Task _instagramTask;
        public void Stop()
        {
            if (MessageBox.Show(Functions.FindStringResource("StopPuzzle"), Functions.FindStringResource("Confirmation"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                return;
            Unloaded();
            Activation.WasStart = false;
            Activation.IsEditable = true;
            _cancellation?.Cancel();
            foreach (var process in Process.GetProcessesByName("instaloader"))
            {
                process?.Kill();
            }

            new Task(() =>
            {
                try
                {
                    var dir = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundViewImages");
                    foreach (var file in dir.GetFiles())
                    {
                        File.Delete(file.FullName);
                    }
                    var dir2 = new DirectoryInfo(Settings.Default.WorkFolder + "\\ForegroundPrintImages");
                    foreach (var file in dir2.GetFiles())
                    {
                        File.Delete(file.FullName);
                    }
                }
                catch
                {
                    //ignored
                }
            }).Start();
            foreach (var screen in _screenList)
            {
                screen.Close();
            }
            VisiblePuzzle = Visibility.Collapsed;
            VisibleMenu = Visibility.Visible;
            VisibleMenuButtons = Visibility.Hidden;
            RaisePropertyChanged($"VisiblePuzzle");
            RaisePropertyChanged($"VisibleMenu");
            RaisePropertyChanged($"VisibleMenuButtons");
            WasStart?.Invoke("", new EventArgs());
            UpdateTab?.Invoke("", new EventArgs());
        }
        public void Save()
        {
            var list = new List<int>();
            foreach (var value in RandomList)
            {
                list.Add(value);
            }
            Settings.Default.RandomList = list;
            Settings.Default.Save();
            var dialog = new SaveFileDialog() { DefaultExt = ".puzzle", Filter = @"Puzzle (*.puzzle)|*.puzzle", FileName = "Mosaic" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Task.Run(delegate
                {
                    var settings = Functions.SaveSettings();
                    if (File.Exists(Settings.Default.WorkFolder + "\\settings.json"))
                        File.Delete(Settings.Default.WorkFolder + "\\settings.json");
                    File.AppendAllText(Settings.Default.WorkFolder + "\\settings.json", settings);
                    Functions.ZipFolder(Settings.Default.WorkFolder, dialog.FileName);

                });
            }
        }
        public void LoadPuzzle()
        {
            var dialog = new OpenFileDialog { DefaultExt = ".puzzle", Filter = @"Puzzle (*.puzzle)|*.puzzle", FileName = "Mosaic" };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Task.Run(delegate
                {
                    Disp.Invoke(delegate
                    {
                        VisibleLoading = Visibility.Visible;
                        VisibleMenu = Visibility.Collapsed;
                        RaisePropertyChanged($"VisibleLoading");
                        RaisePropertyChanged($"VisibleMenu");
                        RaisePropertyChanged($"LiveCanvas");
                    });
                    Activation.IsEditable = false;
                    Directory.Delete(Settings.Default.WorkFolder, true);
                    var success = Functions.UnzipFolder(dialog.FileName, Settings.Default.WorkFolder);
                    if (success)
                    {
                        string settings = File.ReadAllText(Settings.Default.WorkFolder + "\\settings.json");
                        Functions.LoadSettings(settings);
                        Thread.Sleep(300);
                        Disp.Invoke(delegate
                        {
                            UpdateTab?.Invoke("", new EventArgs());
                        });
                    }
                });
            }
        }
        public void LinkPhoto()
        {
            DialogResult addressResult;
            bool quality = false;
            bool address = false;
            var taskAddress = new Task(() =>
            {
                addressResult = MessageBox.Show(Functions.FindStringResource("UseAddresses"),
                    Functions.FindStringResource("Addresses"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                var result = MessageBox.Show(Functions.FindStringResource($"QualityQuestion"),
                    Functions.FindStringResource($"Quality"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    quality = true;
                if (result == DialogResult.No)
                    quality = false;
                if (addressResult == DialogResult.Yes)
                    address = true;
                if (addressResult == DialogResult.No)
                    address = false;
            });
            taskAddress.Start();
            taskAddress.GetAwaiter().OnCompleted(delegate
            {
                var dialog = new SaveFileDialog { DefaultExt = ".png", Filter = @"Image(*.png)|*.png|All files(*.*)|*.*", FileName = "FullPicture" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var task = new Task(() =>
                    {
                        Size size;
                        string path;
                        if (quality)
                        {
                            size = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.PrintDpi,
                                Settings.Default.CountWidth, Settings.Default.CountHeight);
                            if (address)
                                path = Settings.Default.WorkFolder + "\\PrintImages";
                            else
                                path = Settings.Default.WorkFolder + "\\OverlaidImages";
                        }
                        else
                        {
                            size = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm,
                                Settings.Default.HeightMm, Settings.Default.ViewDpi,
                                Settings.Default.CountWidth, Settings.Default.CountHeight);
                            path = Settings.Default.WorkFolder + "\\ViewImages";
                        }
                        PhotoProcessing.LinkPhoto(path, size.Width / Settings.Default.CountWidth,
                            size.Height / Settings.Default.CountHeight, Settings.Default.CountWidth,
                            Settings.Default.CountHeight, dialog.FileName, address);
                    });
                    task.Start();
                    task.GetAwaiter().OnCompleted(delegate
                    {
                        Task.Run(delegate
                        {
                            MessageBox.Show(Functions.FindStringResource("SuccessExport"),
                                Functions.FindStringResource("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    });
                }
            });
        }
        public void SerialLink()
        {
            var task = new Task(() =>
                    {
                        Size size;
                        string path;
                        if (SerialLinkQuality)
                        {
                            size = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.PrintDpi,
                                Settings.Default.CountWidth, Settings.Default.CountHeight);
                            if (SerialLinkAddresses)
                                path = Settings.Default.WorkFolder + "\\PrintImages";
                            else
                                path = Settings.Default.WorkFolder + "\\OverlaidImages";
                        }
                        else
                        {
                            size = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm,
                                Settings.Default.HeightMm, Settings.Default.ViewDpi,
                                Settings.Default.CountWidth, Settings.Default.CountHeight);
                            path = Settings.Default.WorkFolder + "\\ViewImages";
                        }
                        PhotoProcessing.LinkPhoto(path, size.Width / Settings.Default.CountWidth,
                                size.Height / Settings.Default.CountHeight, Settings.Default.CountWidth,
                                Settings.Default.CountHeight, Settings.Default.SerialLinkFolder + $"\\{Added}.png", SerialLinkAddresses);
                    });
            task.Start();
        }
        public void AddScreen()
        {
            var sources = new List<ImageVm>();
            foreach (var element in LiveCanvas)
            {
                if (element is ImageVm image)
                {
                    sources.Add(image);
                }
            }
            _screenList.Add(new SecondScreenView(sources));
            _screenList.Last().Show();
        }
        public void Unloaded()
        {
            if (FileWatchers != null)
            {
                foreach (var fileWatcher in FileWatchers)
                {
                    if (fileWatcher != null)
                    {
                        fileWatcher.Created -= OnChanged;
                        fileWatcher.Dispose();
                    }
                }
            }
            if (PrintWatcher != null)
            {
                PrintWatcher.Created -= PrintChanged;
                PrintWatcher.Dispose();
            }
        }
        public void SaveWebSettings()
        {
            if (string.IsNullOrEmpty(Settings.Default.ServerFolder))
                return;
            var path = Settings.Default.ServerFolder + "\\ClientApp\\build\\settingsMosaic.json";
            if (!File.Exists(path))
                return;
            var jsonString = File.ReadAllText(path);
            var json = JsonConvert.DeserializeObject<SettingsServer>(jsonString);
            var fullSize = PhotoProcessing.CalculatePixels(Settings.Default.WidthMm, Settings.Default.HeightMm, Settings.Default.ViewDpi, Settings.Default.CountWidth, Settings.Default.CountHeight);
            json.FullHeight = fullSize.Height;
            json.FullWidth = fullSize.Width;
            json.CountWidth = Settings.Default.CountWidth;
            json.CountHeight = Settings.Default.CountHeight;
            jsonString = JsonConvert.SerializeObject(json);
            File.WriteAllText(path, jsonString);
        }
        public ProgressLiveModel()
        {

            MainWindowVm.ClearResources += ClearResources;
            CamePhoto += CamePhotoFunction;
            LiveCanvas = new ObservableCollection<BaseThing>();
            Disp = Dispatcher.CurrentDispatcher;
            var randomList = Settings.Default.RandomList;
            if (randomList != null)
            {
                foreach (var value in randomList)
                {
                    RandomList.Enqueue(value);
                }
            }

            try
            {
                PrintWatcher = new FileSystemWatcher(Settings.Default.WorkFolder + "\\PrintImages")
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*.*g"
                };
                PrintWatcher.Changed += PrintChanged;
                PrintWatcher.EnableRaisingEvents = false;
            }
            catch
            {
                new Thread(() =>
                {
                    MessageBox.Show(Functions.FindStringResource("MonitorError"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }).Start();
            }
            foreach (var hotFolder in Settings.Default.HotFolders)
            {
                if (!string.IsNullOrEmpty(hotFolder))
                {
                    try
                    {
                        var watcher = new FileSystemWatcher
                        {
                            Path = hotFolder,
                            NotifyFilter = NotifyFilters.LastWrite,
                            Filter = "*.*g"
                        };
                        watcher.Changed += OnChanged;
                        watcher.EnableRaisingEvents = false;
                        FileWatchers.Add(watcher);
                    }
                    catch (Exception)
                    {
                        new Thread(() =>
                        {
                            MessageBox.Show(Functions.FindStringResource("MonitorError" + $" {hotFolder}"), Functions.FindStringResource("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }).Start();
                    }
                }
            }
            if (!Activation.IsEditable)
                ResetLiveCanvas();
            //if (UseSeveralPhotos)
            //{
            //    RaisePropertyChanged($"VisibleSeveralPrinting");
            //}
        }
    }
}

