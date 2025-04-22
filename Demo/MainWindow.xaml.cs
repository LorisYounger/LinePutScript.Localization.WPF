using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LinePutScript;
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

#if !DEBUG
            TabMain.SelectedIndex = 1;
#endif
            //txt1.PreviewMouseWheel += (s, e) =>
            //{
            //    txt2.ScrollToVerticalOffset(txt1.VerticalOffset);
            //};
            //txt2.PreviewMouseWheel += (s, e) =>
            //{
            //    txt1.ScrollToVerticalOffset(txt2.VerticalOffset);
            //};
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
                if (lines1.Length != lines2.Length)
                {
                    MessageBox.Show($"{lines1.Length} != {lines2.Length}", LocalizeCore.Translate("行数不一致"));
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txt1.Text) || string.IsNullOrEmpty(txt2.Text))
                return;
            if (MessageBox.Show(LocalizeCore.Translate("是否关闭?"), LocalizeCore.Translate("是否关闭?"), MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void tbseach_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !string.IsNullOrEmpty(tbseach.Text))
            {
                string[] lines1 = txt1.Text.Replace("\r", "").Split('\n');
                string[] lines2 = txt2.Text.Replace("\r", "").Split('\n');
                if (lines1.Length != lines2.Length)
                {
                    MessageBox.Show($"{lines1.Length} != {lines2.Length}", LocalizeCore.Translate("行数不一致"));
                    return;
                }
                var sts = tbseach.Text.ToLower().Split(' ');
                HashSet<int> searchlist = new HashSet<int>();
                for (int i = 0; i < lines1.Length; i++)
                {
                    bool isok = true;
                    foreach (var st in sts)
                    {
                        if (!lines1[i].ToLower().Contains(st))
                        {
                            isok = false;
                            break;
                        }
                    }
                    if (isok)
                        searchlist.Add(i);
                }
                for (int i = 0; i < lines1.Length; i++)
                {
                    bool isok = true;
                    foreach (var st in sts)
                    {
                        if (!lines2[i].ToLower().Contains(st))
                        {
                            isok = false;
                            break;
                        }
                    }
                    if (isok)
                        searchlist.Add(i);
                }
                StringBuilder sb1 = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                StringBuilder sb3 = new StringBuilder();
                var list = searchlist.OrderBy(x => x);
                foreach (int i in list)
                {
                    sb1.AppendLine(lines1[i]);
                    sb2.AppendLine(lines2[i]);
                    sb3.AppendLine((i + 1).ToString());
                }
                findtxt1.Text = sb1.ToString();
                findtxt2.Text = sb2.ToString();
                fline.Text = sb3.ToString();
            }
        }

        private void linet1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            int maxi = (int)(tb.ActualHeight / 20.36);
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= maxi; i++)
                sb.AppendLine(i.ToString());
            tb.Text = sb.ToString().Trim('\n');
        }

        private void GetAllCSTrans_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Folders|*.*";
            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                var set = new HashSet<string>();
                LoadCS(new FileInfo(ofd.FileName).Directory, set);
                txt1.Text = string.Join("\r\n", set);
                txt2.Text = txt1.Text;
                TabMain.SelectedIndex = 1;
            }
        }
        public static void LoadCS(DirectoryInfo directory, HashSet<string> Set)
        {
            foreach (var fi in directory.GetFiles("*.cs"))
            {
                LoadCS(fi, Set);
            }
            foreach (var di in directory.GetDirectories())
            {
                LoadCS(di, Set);
            }
        }
        public static void LoadCS(FileInfo file, HashSet<string> Set)
        {
            var fs = File.ReadAllLines(file.FullName);
            foreach (var str in fs)
            {
                LoadCS(str, Set);
            }
        }
        public static void LoadCS(string line, HashSet<string> Set)
        {
            if (line.Contains("\".Translate("))
            {
                var ss = Sub.Split(line, "\".Translate(", 1);
                LoadCS(ss[1], Set);
                var ss2 = Sub.Split(new string(ss[0].Reverse().ToArray()), "\"", 1);
                LoadCS(new string(ss2[1].Reverse().ToArray()), Set);
                Set.Add(new string(ss2[0].Reverse().ToArray()));
            }
            else if (line.Contains("Translate(\""))
            {
                // var s = str.Split(new string[] { "Translate(" }, StringSplitOptions.None)[1].Split(')')[0];
                var ss = Sub.Split(line, "Translate(\"", 1);
                LoadCS(ss[0], Set);
                var ss2 = Sub.Split(ss[1], "\"", 1);
                Set.Add(ss2[0]);
                LoadCS(ss2[1], Set);
            }
        }

        private void RemoveFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "翻译文本".Translate() + "|*.lps";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                List<string> set = new List<string>();
                set.AddRange(txt1.Text.Replace("\r", "").Split('\n'));
                //foreach (var str in txt1.Text.Replace("\r", "").Split('\n'))
                //{
                //    set.Add(str);
                //}
                foreach (string fn in ofd.FileNames)
                    foreach (var line in new LPS_D(File.ReadAllText(fn)))
                    {
                        set.Remove(line.Name);
                    }
                txt1.Text = string.Join("\r\n", set);
                txt2.Text = txt1.Text;
            }
        }

        private void ReadLPSFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "LPSFile|*.lps";
            if (ofd.ShowDialog() == true)
            {
                var set = new HashSet<string>();
                foreach (var str in txt1.Text.Replace("\r", "").Split('\n'))
                {
                    set.Add(str);
                }
                foreach (var line in new LpsDocument(File.ReadAllText(ofd.FileName)))
                {
                    set.Add(line["name"].Info);
                    set.Add(line["Name"].Info);
                    set.Add(line["desc"].Info);
                    set.Add(line["text"].Info);
                    set.Add(line["Text"].Info);
                    set.Add(line.Text);
                }
                txt1.Text = string.Join("\r\n", set);
                txt2.Text = txt1.Text;
            }
        }

        private void RemoveReplice_Click(object sender, RoutedEventArgs e)
        {
            var list = txt1.Text.Replace("\r", "").Split('\n').ToList().Distinct().ToList();
            list.Remove("");
            txt1.Text = string.Join("\r\n", list);
            txt2.Text = txt1.Text;
        }

        private void RemoveFromFolder_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Folders|*.*";
            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == true)
            {
                var set = new HashSet<string>();
                RemoveFF(new FileInfo(ofd.FileName).Directory, set);
                txt1.Text = string.Join("\r\n", set);
                txt2.Text = txt1.Text;
                TabMain.SelectedIndex = 1;
            }
        }
        public static void RemoveFF(DirectoryInfo directory, HashSet<string> Set)
        {
            foreach (var fi in directory.GetFiles("*.cs"))
            {
                RemoveFF(fi, Set);
            }
            foreach (var di in directory.GetDirectories())
            {
                RemoveFF(di, Set);
            }
        }
        public static void RemoveFF(FileInfo file, HashSet<string> Set)
        {
            foreach (var line in new LPS_D(File.ReadAllText(file.FullName)))
            {
                Set.Remove(line.Name);
            }
        }
    }
}
