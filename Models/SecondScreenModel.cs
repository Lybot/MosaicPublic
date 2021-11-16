using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using MozaikaApp.Properties;
using MozaikaApp.ViewModels;
using Prism.Mvvm;

namespace MozaikaApp.Models
{
    public class SecondScreenModel : BindableBase
    {
        public int WidthWindow { get; set; } = 600;
        public int HeightWindow { get; set; } = 300;
        public WindowStyle Style { get; set; } = WindowStyle.SingleBorderWindow;
        public WindowState State { get; set; } = WindowState.Normal;
        public List<ImageVm> Sources;
        private Queue<Task> Tasks { get; set; } = new Queue<Task>();
        public bool IsQueueActive { get; set; }
        private event EventHandler CamePhoto;
        public ObservableCollection<BaseThing> LiveCanvas { get; set; }
        public Visibility ButtonVisibility { get; set; } = Visibility.Visible;
        private static Dispatcher _dispatcher;
        private int _milliseconds = 1000;
        private int _fpsAnimation = 60;
        public ImageVm LoadBackgroundImage()
        {
            var path = Settings.Default.BackgroundImagePath;
            if (string.IsNullOrEmpty(path) || !Settings.Default.BackgroundImageVisibility)
                return null;
            var imageVm = new ImageVm { ZIndex = -1 };
            var image = new Bitmap(path);
            var viewDpi = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
            int needWidth = (int)(PhotoProcessing.ConvertMmToInch(Settings.Default.WidthMm) * viewDpi) * Settings.Default.CountWidth;
            int needHeight = (int)(PhotoProcessing.ConvertMmToInch(Settings.Default.HeightMm) * viewDpi) * Settings.Default.CountHeight;
            double needFormat = needWidth / (double)needHeight;
            double screenFormat = WidthWindow / (double)HeightWindow;
            var heightFull = (double)HeightWindow;
            var widthFull = (double)WidthWindow;
            double intend = 0;
            bool sideIntend = false; // true - top intend, false - left intend

            if (needFormat >= screenFormat)
            {
                heightFull = LiveCanvas[0].Height * Settings.Default.CountHeight;
                intend = LiveCanvas[0].Top;
                //heightFull = HeightWindow;
                //heightFull = (int)(heightFull * screenFormat / needFormat / Settings.Default.CountHeight) * Settings.Default.CountHeight;
                sideIntend = true;
            }
            else
            {
                widthFull = LiveCanvas[0].Width * Settings.Default.CountWidth;
                intend = LiveCanvas[0].Left;
                //widthFull = WidthWindow;
                ////widthFull = (int)(widthFull * needFormat / screenFormat / Settings.Default.CountWidth) * Settings.Default.CountWidth;
            }


            if (sideIntend)
            {
                imageVm.Top = intend;
                imageVm.Left = 0;
            }
            else
            {
                imageVm.Left = intend;
                imageVm.Top = 0;
            }

            imageVm.Height = heightFull;
            imageVm.Width = widthFull;
            image = PhotoProcessing.ResizeImage(image, (int)widthFull, (int)heightFull);
            imageVm.Source = Functions.BitmapToImageSource(image, false);
            return imageVm;
        }
        public Task Animation(ImageVm element, PointF startPosition, PointF endPosition, int widthScreen, int heightScreen)
        {
            //new Task(() =>
            //{
            var endWidth = element.Width;
            var endHeight = element.Height;
            var midPosition = new PointF(widthScreen / 3f, heightScreen / 3f);
            float ms = (1f / _fpsAnimation * 1000);
            float ticks = _milliseconds / ms / 3;
            float topTick = (midPosition.Y - startPosition.Y) / ticks;
            float leftTick = (midPosition.X - startPosition.X) / ticks;
            float widthTick = (widthScreen / 3f - (float)element.Width) / ticks;
            float heightTick = (heightScreen / 3f - (float)element.Height) / ticks;
            var currentPosition = startPosition;
            while (Math.Abs(currentPosition.X - midPosition.X) > 1 || Math.Abs(currentPosition.Y - midPosition.Y) > 1)
            {
                if (Math.Abs(currentPosition.X - midPosition.X) > 1)
                    currentPosition.X += leftTick;
                else
                    currentPosition.X = endPosition.X;
                if (Math.Abs(currentPosition.Y - midPosition.Y) > 1)
                    currentPosition.Y += topTick;
                else
                    currentPosition.Y = endPosition.Y;
                if (Math.Abs(element.Height - heightScreen / 3f) > 1)
                    element.Height += heightTick;
                else
                    element.Height = endHeight;
                if (Math.Abs(element.Width - widthScreen / 3f) > 1)
                    element.Width += widthTick;
                else
                    element.Width = endWidth;
                element.Top = currentPosition.Y;
                element.Left = currentPosition.X;
                //_dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate
                //{
                //    if (Math.Abs(currentPosition.Y - endPosition.Y) < 0.1 && Math.Abs(currentPosition.X - endPosition.X) < 0.1)
                //        AnimationEvent?.Invoke(null, new ImageVm() { Top = currentPosition.Y, Width = element.Width, Source = null, Height = element.Height, Left = currentPosition.X });
                //    else
                //        AnimationEvent?.Invoke(null, new ImageVm() { Top = currentPosition.Y, Width = element.Width, Source = element.Source, Height = element.Height, Left = currentPosition.X });
                //}));
                Thread.Sleep((int)ms);
            }
            Thread.Sleep(_milliseconds / 3);
            topTick = (endPosition.Y - currentPosition.Y) / ticks;
            leftTick = (endPosition.X - currentPosition.X) / ticks;
            while (Math.Abs(currentPosition.X - endPosition.X) > 1 || Math.Abs(currentPosition.Y - endPosition.Y) > 1)
            {
                if (Math.Abs(currentPosition.X - endPosition.X) > 1)
                    currentPosition.X += leftTick;
                else
                    currentPosition.X = endPosition.X;
                if (Math.Abs(currentPosition.Y - endPosition.Y) > 1)
                    currentPosition.Y += topTick;
                else
                    currentPosition.Y = endPosition.Y;
                if (Math.Abs(element.Height - endHeight) > 1)
                    element.Height -= heightTick;
                else
                    element.Height = endHeight;
                if (Math.Abs(element.Width - endWidth) > 1)
                    element.Width -= widthTick;
                else
                    element.Width = endWidth;
                element.Left = currentPosition.X;
                element.Top = currentPosition.Y;
                //_dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate
                //{
                //    if (Math.Abs(currentPosition.Y - endPosition.Y) < 0.1 && Math.Abs(currentPosition.X - endPosition.X) < 0.1)
                //        AnimationEvent?.Invoke(null, new ImageVm() { Top = currentPosition.Y, Width = element.Width, Source = null, Height = element.Height, Left = currentPosition.X });
                //    else
                //        AnimationEvent?.Invoke(null, new ImageVm() { Top = currentPosition.Y, Width = element.Width, Source = element.Source, Height = element.Height, Left = currentPosition.X });
                //}));
                Thread.Sleep((int)ms);
            }
            return Task.CompletedTask;
            //}).Start();

        }
        public void StartPuzzle()
        {
            new Task(delegate
            {
                var viewDpi = (int)Graphics.FromHwnd(IntPtr.Zero).DpiX;
                _dispatcher?.Invoke(delegate
                {
                    Style = WindowStyle.None;
                    State = WindowState.Maximized;
                    ButtonVisibility = Visibility.Collapsed;
                    RaisePropertyChanged($"State");
                    RaisePropertyChanged($"Style");
                    RaisePropertyChanged($"ButtonVisibility");
                    LiveCanvas = Functions.Generate(viewDpi, Settings.Default.WidthMm, Settings.Default.HeightMm, WidthWindow, HeightWindow, Settings.Default.CountWidth, Settings.Default.CountHeight, true);
                    LiveCanvas.Add(LoadBackgroundImage());
                    RaisePropertyChanged($"LiveCanvas");
                    RaisePropertyChanged($"HeightWindow");
                    RaisePropertyChanged($"WidthWindow");
                });
                foreach (var source in Sources)
                {
                    var label = LiveCanvas.FirstOrDefault(baseThing => Equals(baseThing.Tag, source.Tag));
                    var task = new Task(delegate
                    {
                        if (label != null)
                        {
                            //var endPosition = new PointF((float)label.Left, (float)label.Top);
                            //var index = LiveCanvas.Count;
                            //var task2= new Task(delegate
                            //{
                            var image = new ImageVm()
                            {
                                Height = label.Height,
                                Left = label.Left,
                                Source = source.Source,
                                Tag = label.Tag,
                                Top = label.Top,
                                Width = label.Width
                            };
                            //await Animation(image, endPosition, WidthWindow, HeightWindow);
                            _dispatcher.Invoke(delegate
                                {
                                    LiveCanvas.Add(image);
                                    RaisePropertyChanged($"LiveCanvas");
                                });
                            //});
                            //Tasks.Enqueue(task2);
                        }
                    });
                    Tasks.Enqueue(task);
                    CamePhoto?.Invoke(null, EventArgs.Empty);
                }
            }).Start();
        }

        public void Close()
        {
            IsQueueActive = false;
            CamePhoto -= ProcessPhoto;
            ProgressLiveModel.SecondScreen -= SecondScreenCamePhoto;
            ProgressLiveModel.DeleteImage -= DeleteImage;
        }
        public SecondScreenModel()
        {
            CamePhoto += ProcessPhoto;
            ProgressLiveModel.SecondScreen += SecondScreenCamePhoto;
            ProgressLiveModel.DeleteImage += DeleteImage;
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        private void DeleteImage(object sender, ImageVm e)
        {
            var image = LiveCanvas.LastOrDefault(thing => Equals(thing.Tag, e.Tag));
            _dispatcher.Invoke(delegate
            {
                LiveCanvas.Remove(image);
                RaisePropertyChanged($"LiveCanvas");
            });
        }

        private void SecondScreenCamePhoto(object sender, ImageVm e)
        {
            var label = LiveCanvas.FirstOrDefault(baseThing => Equals(baseThing.Tag, e.Tag));
            var task = new Task(async delegate
            {
                if (label != null)
                {
                    var startPosition = new PointF(0, 0);
                    var corner = new Random().Next(1, 5);
                    switch (corner)
                    {
                        case 1:
                            startPosition = new PointF(WidthWindow - (float)label.Width, 0);
                            break;
                        case 2:
                            break;
                        case 3:
                            startPosition = new PointF(0, WidthWindow - (float)label.Height);
                            break;
                        case 4:
                            startPosition = new PointF(WidthWindow - (float)label.Width, HeightWindow - (float)label.Height);
                            break;
                    }
                    var endPosition = new PointF((float)label.Left, (float)label.Top);
                    var image = new ImageVm()
                    {
                        Height = label.Height,
                        Left = startPosition.X,
                        Source = e.Source,
                        Tag = label.Tag,
                        Top = startPosition.Y,
                        Width = label.Width
                    };
                    _dispatcher.Invoke(delegate
                    {
                        LiveCanvas.Add(image);
                        RaisePropertyChanged($"LiveCanvas");
                    });
                    await Animation(image, startPosition, endPosition, WidthWindow, HeightWindow);
                    //});
                    //Tasks.Enqueue(task2);
                }
            });
            Tasks.Enqueue(task);
            CamePhoto?.Invoke(null, EventArgs.Empty);
        }

        private void ProcessPhoto(object sender, EventArgs e)
        {
            if (IsQueueActive)
                return;
            IsQueueActive = true;
            var task = new Task(() =>
            {
                while (Tasks.Count > 0 && IsQueueActive)
                {
                    var dequeue = Tasks.Dequeue();
                    dequeue.Start();
                    dequeue.Wait();
                }
            });
            task.Start();
            task.GetAwaiter().OnCompleted(delegate
            {
                IsQueueActive = false;
            });
        }
    }

    //public class AnimateBehaviour : Behavior<UIElement>
    //{
    //    protected override void OnAttached()
    //    {
    //        base.OnAttached();
    //        SecondScreenModel.AnimationEvent += SecondScreenModelAnimationEvent;
    //    }

    //    protected override void OnDetaching()
    //    {
    //        base.OnDetaching();
    //        SecondScreenModel.AnimationEvent -= SecondScreenModelAnimationEvent;
    //    }
    //    private void SecondScreenModelAnimationEvent(object sender, ImageVm e)
    //    {
    //        AssociatedObject.SetValue(Image.SourceProperty, e.Source);
    //        AssociatedObject.SetValue(Canvas.TopProperty, e.Top);
    //        AssociatedObject.SetValue(Canvas.LeftProperty, e.Left);
    //        AssociatedObject.SetValue(FrameworkElement.WidthProperty, e.Width);
    //        AssociatedObject.SetValue(FrameworkElement.HeightProperty, e.Height);
    //    }
    //}
}
