using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DanMu
{
    /// <summary>
    /// UserSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UserSetting : Window
    {
        public delegate void settingChangeDelegate(object sender, EventArgs e);

        public event settingChangeDelegate settingChangeEvent;

        public UserSetting() {
            this.ResizeMode = ResizeMode.NoResize;

            InitializeComponent();

            buttonOk.Click += new RoutedEventHandler(buttonOk_Click);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //选择字体 
            comboBoxFontFamily.IsEditable = false;
            comboBoxFontFamily.IsReadOnly = true;
            foreach (FontFamily _f in Fonts.SystemFontFamilies) {
                LanguageSpecificStringDictionary _font = _f.FamilyNames;
                if (_font.ContainsKey(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn"))) {
                    string _fontName = null;
                    if (_font.TryGetValue(System.Windows.Markup.XmlLanguage.GetLanguage("zh-cn"), out _fontName)) {
                        TextBlock fontItem = new TextBlock();
                        fontItem.Text = _fontName;
                        fontItem.FontFamily = new FontFamily(_fontName);
                        fontItem.FontSize = 12;
                        comboBoxFontFamily.Items.Add(fontItem);
                    }
                }
            }
            foreach (TextBlock fontItem in comboBoxFontFamily.Items) {
                if (fontItem.Text == setting.getFontFamily().ToString()) {
                    comboBoxFontFamily.SelectedItem = fontItem;
                    break;
                }
            }
            comboBoxFontColor.SelectedIndex = 7;

            comboBoxFontSize.IsEditable = true;
            comboBoxFontSize.IsReadOnly = false;
            for(int i = 10; i <= 120; i += 2) {
                comboBoxFontSize.Items.Add(i);
            }
            comboBoxFontSize.SelectedItem = (int)setting.getFontSize();

            if (setting.getFontStyle() == FontStyles.Italic) {
                checkBoxFontStyle.IsChecked = true;
            }
            if(setting.getFontWeight() == FontWeights.Bold) {
                checkBoxFontWeight.IsChecked = true;
            }
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            setting.setFontFamily(((TextBlock)comboBoxFontFamily.SelectedItem).Text);
            string selectedColor = comboBoxFontColor.SelectedItem.ToString();
            selectedColor = selectedColor.Substring("System.Windows.Media.Color ".Length, selectedColor.Length - "System.Windows.Media.Color ".Length);
            setting.setForeground(selectedColor);
            setting.setFontSize((int)comboBoxFontSize.SelectedItem);
            if (checkBoxFontStyle.IsChecked == true) {
                setting.setFontStyle(FontStyles.Italic);
            }
            else {
                setting.setFontStyle(FontStyles.Normal);
            }
            if(checkBoxFontWeight.IsChecked == true) {
                setting.setFontWeight(FontWeights.Bold);
            }
            else {
                setting.setFontWeight(FontWeights.Normal);
            }
            settingChangeEvent(this, null);
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
