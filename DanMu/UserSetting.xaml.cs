using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DanMu
{
    /// <summary>
    /// UserSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UserSetting : Window {

        public delegate void settingChangeDelegate(object sender, EventArgs e);

        public event settingChangeDelegate settingChangeEvent;

        public UserSetting() {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
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
            for (int i = 10; i <= 120; i += 2) {
                comboBoxFontSize.Items.Add(i);
            }
            comboBoxFontSize.SelectedItem = (int)setting.getFontSize();

            if (setting.getFontStyle() == FontStyles.Italic) {
                checkBoxFontStyle.IsChecked = true;
            }
            if (setting.getFontWeight() == FontWeights.Bold) {
                checkBoxFontWeight.IsChecked = true;
            }
            if (setting.getRandomColor()) {
                comboBoxFontColor.IsEnabled = false;
            }
            checkBoxRandomColor.IsChecked = setting.getRandomColor();
            if (setting.getRandomFontFamily()) {
                comboBoxFontFamily.IsEnabled = false;
            }
            checkBoxRandomFontFamily.IsChecked = setting.getRandomFontFamily();

            textBoxNum.Text = setting.getNUM().ToString();
            textBoxDuration.Text = setting.getDURATION().ToString();
            textBoxSpeed.Text = setting.getSPEED().ToString();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e) {
            if (comboBoxFontSize.Text == "" || textBoxNum.Text == "" || textBoxSpeed.Text == "" || textBoxDuration.Text == "") {
                System.Windows.MessageBox.Show("请输入所有设置项。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }
            else {
                try {
                    int fontSize = int.Parse(comboBoxFontSize.Text);
                    if (fontSize <= 0 || fontSize > 320) {
                        System.Windows.MessageBox.Show("字体大小设置超出范围。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                    int num = int.Parse(textBoxNum.Text);
                    if (num <= 0 || num > 1000) {
                        System.Windows.MessageBox.Show("弹幕数量设置超出范围。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                    int duration = int.Parse(textBoxDuration.Text);
                    if (duration <= 0 || duration > 10000) {
                        System.Windows.MessageBox.Show("弹幕获取时间设置超出范围。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                    int speed = int.Parse(textBoxSpeed.Text);
                    if (speed <= 0 || speed > 100) {
                        System.Windows.MessageBox.Show("弹幕移动速度设置超出范围。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return;
                    }
                }
                catch (FormatException formatError) {
                    System.Windows.MessageBox.Show("输入格式错误，请检查。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    return;
                }
                catch (OverflowException overflowError) {
                    System.Windows.MessageBox.Show("值溢出，请检查。", "云弹幕",
                MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    return;
                }
            }

            
            setting.setFontSize((int)comboBoxFontSize.SelectedItem);
            if (checkBoxFontStyle.IsChecked == true) {
                setting.setFontStyle(FontStyles.Italic);
            }
            else {
                setting.setFontStyle(FontStyles.Normal);
            }
            if (checkBoxFontWeight.IsChecked == true) {
                setting.setFontWeight(FontWeights.Bold);
            }
            else {
                setting.setFontWeight(FontWeights.Normal);
            }
            if(checkBoxRandomColor.IsChecked == true) {
                setting.setRandomColor(true);
            }
            else {
                setting.setRandomColor(false);
                string selectedColor = comboBoxFontColor.SelectedItem.ToString();
                selectedColor = selectedColor.Substring("System.Windows.Media.Color ".Length, selectedColor.Length - "System.Windows.Media.Color ".Length);
                setting.setForeground(selectedColor);
            }
            if(checkBoxRandomFontFamily.IsChecked == true) {
                setting.setRandomFontFamily(true);
            }
            else {
                setting.setRandomFontFamily(false);
                setting.setFontFamily(((TextBlock)comboBoxFontFamily.SelectedItem).Text);
            }

            setting.setNUM(int.Parse(textBoxNum.Text));
            setting.setDURATION(int.Parse(textBoxDuration.Text));
            setting.setSPEED(int.Parse(textBoxSpeed.Text));

            settingChangeEvent(this, null);
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e) {
            foreach (TextBlock fontItem in comboBoxFontFamily.Items) {
                if (fontItem.Text == "微软雅黑") {
                    comboBoxFontFamily.SelectedItem = fontItem;
                    break;
                }
            }
            comboBoxFontColor.SelectedIndex = 7;
            comboBoxFontSize.SelectedItem = 36;
            checkBoxFontStyle.IsChecked = false;
            checkBoxFontWeight.IsChecked = false;

            textBoxNum.Text = "20";
            textBoxDuration.Text = "100";
            textBoxSpeed.Text = "2";
        }

        private void checkBoxRandomColor_Checked(object sender, RoutedEventArgs e) {
            comboBoxFontColor.IsEnabled = false;
        }

        private void checkBoxRandomColor_Unchecked(object sender, RoutedEventArgs e) {
            comboBoxFontColor.IsEnabled = true;
        }

        private void checkBoxRandomFontFamily_Checked(object sender, RoutedEventArgs e) {
            comboBoxFontFamily.IsEnabled = false;
        }

        private void checkBoxRandomFontFamily_Unchecked(object sender, RoutedEventArgs e) {
            comboBoxFontFamily.IsEnabled = true;
        }
    }
}
