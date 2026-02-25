using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DonchanOverlay
{
    public partial class MainWindow : Window
    {
        // ★ 1P / 2P で別々のオフセット
        public double OffsetX_1P = -450;
        public double OffsetY_1P = 200;

        public double OffsetX_2P = 500;
        public double OffsetY_2P = 200;

        // ★ FPS
        public int CurrentFps = 60;

        DispatcherTimer idleTimer = new DispatcherTimer();
        DispatcherTimer gogoTimer = new DispatcherTimer();

        int idleFrame = 0;
        int gogoFrame = 0;

        bool GoGo = false;
        bool GoGoPlaying = false;

        public bool Is1P = true;
        int CurrentCharacterId = 0;

        List<CharacterEntry> Characters = new();

        List<BitmapImage> idleFrames = new();
        List<BitmapImage> gogoFrames = new();
        List<BitmapImage> gogoStartFrames = new();

        public MainWindow()
        {
            InitializeComponent();

            this.Topmost = true;

            LoadCharacters();
            PreloadAll();

            ApplySide();
            ApplyPosition();
            ApplyFps();

            this.KeyDown += MainWindow_KeyDown;

            StartIdleAnimation();
        }

        // ★ FPS をタイマーに反映
        public void ApplyFps()
        {
            double interval = 1000.0 / CurrentFps;

            idleTimer.Interval = TimeSpan.FromMilliseconds(interval);
            gogoTimer.Interval = TimeSpan.FromMilliseconds(interval);
        }

        // ★ 1P / 2P で別々のオフセットを使う ApplyPosition
        public void ApplyPosition()
        {
            var area = SystemParameters.WorkArea;

            double ox = Is1P ? OffsetX_1P : OffsetX_2P;
            double oy = Is1P ? OffsetY_1P : OffsetY_2P;

            if (Is1P)
                this.Left = area.Left + ox;
            else
                this.Left = area.Right - this.Width + ox;

            this.Top = area.Bottom - this.Height + oy;
        }

        void LoadCharacters()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var cfg = CharacterConfig.Load(baseDir);

            if (cfg.characters.Count == 0)
            {
                Characters = new List<CharacterEntry>
                {
                    new CharacterEntry { id = 0, name = "キャラ0" }
                };
            }
            else
            {
                Characters = cfg.characters;
            }

            CurrentCharacterId = Characters[0].id;
        }

        List<BitmapImage> LoadFrames(string folder)
        {
            List<BitmapImage> list = new();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(baseDir, "Flames", CurrentCharacterId.ToString(), folder);

            if (!Directory.Exists(dir))
                return list;

            var files = Directory.GetFiles(dir, "*.png");

            Array.Sort(files, (a, b) =>
            {
                int na = int.Parse(Path.GetFileNameWithoutExtension(a));
                int nb = int.Parse(Path.GetFileNameWithoutExtension(b));
                return na.CompareTo(nb);
            });

            foreach (var file in files)
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.UriSource = new Uri(file);
                img.EndInit();
                img.Freeze();
                list.Add(img);
            }

            return list;
        }

        void PreloadAll()
        {
            idleFrames = LoadFrames("idle");
            gogoFrames = LoadFrames("gogo");
            gogoStartFrames = LoadFrames("gogoStart");

            idleFrame = 0;
            gogoFrame = 0;
        }

        void IdleTickHandler(object? sender, EventArgs e)
        {
            if (GoGoPlaying)
                return;

            if (GoGo)
            {
                _ = PlayGoGoStart();
                return;
            }

            if (idleFrames.Count == 0)
                return;

            DonImage.Source = idleFrames[idleFrame];
            idleFrame = (idleFrame + 1) % idleFrames.Count;
        }

        void StartIdleAnimation()
        {
            GoGoPlaying = false;

            gogoTimer.Stop();
            idleTimer.Stop();

            idleTimer = new DispatcherTimer();
            ApplyFps();
            idleTimer.Tick += IdleTickHandler;
            idleTimer.Start();
        }

        async Task PlayGoGoStart()
        {
            GoGoPlaying = true;

            idleTimer.Stop();
            idleTimer.Tick -= IdleTickHandler;

            gogoTimer.Stop();

            for (int i = 0; i < gogoStartFrames.Count; i++)
            {
                DonImage.Source = gogoStartFrames[i];
                await Task.Delay((int)(1000.0 / CurrentFps));
            }

            StartGoGoLoop();
        }

        void StartGoGoLoop()
        {
            gogoTimer.Stop();

            gogoTimer = new DispatcherTimer();
            ApplyFps();

            gogoTimer.Tick += (s, e) =>
            {
                if (!GoGo)
                {
                    GoGoPlaying = false;
                    StartIdleAnimation();
                    return;
                }

                if (gogoFrames.Count == 0)
                    return;

                DonImage.Source = gogoFrames[gogoFrame];
                gogoFrame = (gogoFrame + 1) % gogoFrames.Count;
            };

            gogoTimer.Start();
        }

        public void SetGoGo(bool value)
        {
            GoGo = value;

            if (!GoGo)
            {
                GoGoPlaying = false;
                StartIdleAnimation();
            }
        }

        void ApplySide()
        {
            FlipTransform.ScaleX = Is1P ? 1 : -1;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) &&
                Keyboard.IsKeyDown(Key.LeftShift) &&
                e.Key == Key.D)
            {
                var sw = new SettingsWindow(GoGo, Is1P, Characters, CurrentCharacterId, CurrentFps);
                if (sw.ShowDialog() == true)
                {
                    SetGoGo(sw.GoGoEnabled);
                    Is1P = sw.Is1P;
                    CurrentCharacterId = sw.SelectedCharacterId;

                    PreloadAll();

                    ApplySide();
                    ApplyPosition();

                    CurrentFps = sw.SelectedFps;
                    ApplyFps();
                }
            }
        }
    }
}