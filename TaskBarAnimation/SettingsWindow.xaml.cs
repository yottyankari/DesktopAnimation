using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DonchanOverlay
{
    public partial class SettingsWindow : Window
    {
        public bool GoGoEnabled { get; private set; }
        public bool Is1P { get; private set; }
        public int SelectedCharacterId { get; private set; }
        public int SelectedFps { get; private set; }

        List<CharacterEntry> characters;
        MainWindow main;

        public SettingsWindow(bool currentGoGo, bool currentIs1P,
                              List<CharacterEntry> characters,
                              int currentCharacterId,
                              int currentFps)
        {
            InitializeComponent();

            this.characters = characters;
            main = (MainWindow)Application.Current.MainWindow;

            GoGoCheck.IsChecked = currentGoGo;
            SideSelect.SelectedIndex = currentIs1P ? 0 : 1;

            // ★ 1P / 2P でスライダー初期値を切り替え
            if (currentIs1P)
            {
                XSlider.Value = main.OffsetX_1P;
                YSlider.Value = main.OffsetY_1P;
            }
            else
            {
                XSlider.Value = main.OffsetX_2P;
                YSlider.Value = main.OffsetY_2P;
            }

            // ★ FPS 初期値
            foreach (ComboBoxItem item in FpsSelect.Items)
            {
                if ((string)item.Tag == currentFps.ToString())
                {
                    FpsSelect.SelectedItem = item;
                    break;
                }
            }

            CharacterSelect.Items.Clear();
            int indexToSelect = 0;

            for (int i = 0; i < characters.Count; i++)
            {
                var c = characters[i];
                var item = new ComboBoxItem
                {
                    Content = c.name,
                    Tag = c.id
                };
                CharacterSelect.Items.Add(item);

                if (c.id == currentCharacterId)
                    indexToSelect = i;
            }

            if (CharacterSelect.Items.Count > 0)
                CharacterSelect.SelectedIndex = indexToSelect;
        }

        // ★ スライダー → 1P / 2P のオフセットに保存
        private void XSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (main == null) return;

            if (main.Is1P)
                main.OffsetX_1P = e.NewValue;
            else
                main.OffsetX_2P = e.NewValue;

            main.ApplyPosition();
        }

        private void YSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (main == null) return;

            if (main.Is1P)
                main.OffsetY_1P = e.NewValue;
            else
                main.OffsetY_2P = e.NewValue;

            main.ApplyPosition();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            GoGoEnabled = GoGoCheck.IsChecked == true;
            Is1P = SideSelect.SelectedIndex == 0;

            if (CharacterSelect.SelectedItem is ComboBoxItem item &&
                item.Tag is int id)
            {
                SelectedCharacterId = id;
            }
            else
            {
                SelectedCharacterId = 0;
            }

            // ★ FPS 保存
            if (FpsSelect.SelectedItem is ComboBoxItem fpsItem)
                SelectedFps = int.Parse((string)fpsItem.Tag);
            else
                SelectedFps = 60;

            this.DialogResult = true;
            this.Close();
        }
    }
}