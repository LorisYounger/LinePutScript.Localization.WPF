using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using LinePutScript.Dictionary;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;

namespace Demo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // 本地化:
            // 开启翻译存储, 储存所有未翻译的文本,可以在 LocalizeCore.StoreTranslationList 中查看所有未翻译过的文本
            LocalizeCore.StoreTranslation = true;
            // 加载所有的本地化语言, 通过 LocalizeCore.AddCulture
            foreach (var path in new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.lps"))
                LocalizeCore.AddCulture(path.Name.Split('.')[0], new LPS_D(File.ReadAllText(path.FullName)));
            // 加载当前用户使用的默认语言
            LocalizeCore.LoadDefaultCulture();

            InitializeComponent();
            
            foreach (var culture in LocalizeCore.AvailableCultures)
                CombSelect.Items.Add(culture);
            CombSelect.SelectedItem = LocalizeCore.CurrentCulture;
        }

        private void transOut_Click(object sender, RoutedEventArgs e)
        {
            txt1.Text = string.Join("\r\n", LocalizeCore.StoreTranslationList);
            txt2.Text = txt1.Text;
            TabMain.SelectedIndex = 1;
            //StringBuilder sb = new StringBuilder();
            //foreach (string notrans in LocalizeCore.StoreTranslationList)
            //    sb.AppendLine($"{notrans}#{notrans}:|");
            //File.WriteAllText(Environment.CurrentDirectory + "\\out.txt", sb.ToString());
            //Process.Start(Environment.CurrentDirectory + "\\out.txt");
        }

        private void Input_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "翻译文本".Translate() + "|*.lps";
            if (ofd.ShowDialog() == true)
            {
                //LocalizeCore.AddCulture(ofd.SafeFileName.Split('.')[0], new LPS_D(File.ReadAllText(ofd.FileName)));
                //LocalizeCore.CurrentCulture = ofd.SafeFileName.Split('.')[0];
                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                foreach (var line in new LPS_D(File.ReadAllText(ofd.FileName)))
                {
                    sb1.AppendLine(line.Name);
                    sb2.AppendLine(line.Info);
                }
                txt1.Text = sb1.ToString();
                txt2.Text = sb2.ToString();
            }
        }

        private void Output_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "翻译文本|*.lps";
            if (sfd.ShowDialog() == true)
            {
                StringBuilder sb = new StringBuilder();
                string[] lines1 = txt1.Text.Replace("\r", "").Split('\n');
                string[] lines2 = txt2.Text.Replace("\r", "").Split('\n');
                if(lines1.Length != lines2.Length)
                {
                    MessageBox.Show(LocalizeCore.Translate("行数不一致"));
                    return;
                }
                for (int i = 0; i < lines1.Length; i++)
                    sb.AppendLine($"{lines1[i]}#{lines2[i]}:|");
                File.WriteAllText(sfd.FileName, sb.ToString());
                Process.Start(sfd.FileName);
            }
        }

        private void SwtichTrans_Click(object sender, RoutedEventArgs e)
        {
            LocalizeCore.LoadCulture((string)CombSelect.SelectedItem);
        }
    }
}
