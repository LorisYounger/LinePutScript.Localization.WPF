using LinePutScript.Dictionary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace LinePutScript.Localization.WPF
{
    /// <summary>
    /// 本地化核心
    /// </summary>
    public static class LocalizeCore
    {
        /// <summary>
        /// 开启此项目以自动收集未翻译文本
        /// </summary>
        public static bool StoreTranslation { get; set; } = false;
        /// <summary>
        /// 储存的需要/未翻译的文本
        /// </summary>
        public static HashSet<string> StoreTranslationList = new HashSet<string>();
        /// <summary>
        /// 将未翻译的文本导出成LPS格式,可以直接储存
        /// </summary>
        public static string StoreTranslationListToLPS()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string notrans in StoreTranslationList)
            {
                var str = notrans.Replace("\n", @"\n").Replace("\r", @"\r");
                sb.AppendLine($"{str}#{str}:|");
            }

            return sb.ToString();
        }
        /// <summary>
        /// 将未翻译的文本导出成List string格式, 方便查看
        /// </summary>
        public static List<string> StoreTranslationListToList()
        {
            List<string> tmp = new List<string>();
            foreach (string notrans in StoreTranslationList)
            {
                tmp.Add(notrans.Replace("\n", @"\n").Replace("\r", @"\r"));
            }
            return tmp;
        }
        /// <summary>
        /// 当前本地化语言
        /// </summary>
        public static string CurrentCulture
        {
            get => currentCulture; set
            {
                if (Localizations.TryGetValue(value, out var lps))
                {
                    CurrentLPS = lps;
                    currentCulture = value;
                }
                else
                {
                    currentCulture = "null";
                    CurrentLPS = null;
                }
                BindingNotify.Notify();
            }
        }

        public static LPS_D? CurrentLPS;
        private static string currentCulture = "null";
        public static Dictionary<string, LPS_D> Localizations = new Dictionary<string, LPS_D>();

        /// <summary>
        /// 手动代码实现的本地化语言
        /// </summary>
        public static Func<string, string?>? TranslateFunc { get; set; }

        /// <summary>
        /// 当前有的本地化语言
        /// </summary>
        public static string[] AvailableCultures => Localizations.Keys.ToArray();

        /// <summary>
        /// 加载当前电脑的默认本地化语言
        /// </summary>
        public static void LoadDefaultCulture() => LoadCulture(CultureInfo.CurrentCulture);
        /// <summary>
        /// 加载指定的本地化语言
        /// </summary>
        /// <param name="culture">区域性信息</param>
        public static void LoadCulture(CultureInfo culture)
        {
            if (culture == null || string.IsNullOrWhiteSpace(culture.Name))
            {
                currentCulture = "null";
                CurrentLPS = null;
                BindingNotify.Notify();
                return;
            }
            if (Localizations.TryGetValue(culture.Name, out LPS_D? lps))
            {
                currentCulture = culture.Name;
                CurrentLPS = lps;
                BindingNotify.Notify();
            }
            else
            {
                LoadCulture(culture.Parent);
            }
        }
        /// <summary>
        /// 加载指定的本地化语言
        /// </summary>
        /// <param name="culture">区域性文本</param>
        public static void LoadCulture(string culture)
        {
            if (Localizations.TryGetValue(culture, out LPS_D? lps))
            {
                currentCulture = culture;
                CurrentLPS = lps;
            }
            else
            {
                currentCulture = "null";
                CurrentLPS = null;
            }
            BindingNotify.Notify();

        }
        /// <summary>
        /// 查找指定的数据
        /// </summary>
        /// <param name="key">查找值</param>
        /// <returns>返回的数据</returns>
        public static ISetObject? Find(string key)
        {
            if (CurrentLPS != null && CurrentLPS.Assemblage.TryGetValue(key, out var line))
                return line;
            if (StoreTranslation)
                StoreTranslationList.Add(key);
            if (TranslateFunc != null)
            {
                string? str = TranslateFunc(key);
                if (str != null && CurrentLPS != null)
                {
                    CurrentLPS.Add(new Line(key, str));
                    return new SetObject(str);
                }
            }
            return null;
        }
        /// <summary>
        /// 翻译文本
        /// </summary>
        /// <param name="key">翻译内容</param>
        /// <returns>翻译后的文本</returns>
        public static string Translate(this string key) => Find(key)?.GetString() ?? key;

        /// <summary>
        /// 翻译文本
        /// </summary>
        /// <param name="key">翻译内容</param>
        /// <param name="replace">替换内容 {0:f2}{1}..</param>
        /// <returns>翻译后的文本</returns>
        public static string Translate(this string key, params object[] replace) => string.Format(Translate(key), replace);
        /// <summary>
        /// 获得Double数据
        /// </summary>
        public static double GetDouble(string key, double def = default) => Find(key)?.GetDouble() ?? def;
        /// <summary>
        /// 获得Int数据
        /// </summary>
        public static int GetInt(string key, int def = default) => Find(key)?.GetInteger() ?? def;
        /// <summary>
        /// 获得Bool数据
        /// </summary>
        public static bool GetBool(string key, bool def = default) => Find(key)?.GetBoolean() ?? def;
        /// <summary>
        /// 获得Int64数据
        /// </summary>
        public static long GetInt64(string key, long def = default) => Find(key)?.GetInteger64() ?? def;
        /// <summary>
        /// 添加本地化语言
        /// </summary>
        /// <param name="culture">区域性文本</param>
        /// <param name="file">本地化文件</param>
        public static void AddCulture(string culture, IEnumerable<ILine> file)
        {
            if (Localizations.TryGetValue(culture, out var lps))
            {
                foreach (ILine item in file)
                {
                    item.Name = item.Name.Replace(@"\n", "\n").Replace(@"\r", "\r");
                    item.info = item.info.Replace(@"\n", "\n").Replace(@"\r", "\r");
                    lps.Add(item);
                }
            }
            else
            {
                LPS_D lpsd = new LPS_D();
                foreach (ILine item in file)
                {
                    item.Name = item.Name.Replace(@"\n", "\n").Replace(@"\r", "\r");
                    item.info = item.info.Replace(@"\n", "\n").Replace(@"\r", "\r");
                    lpsd.Add(item);
                }
                Localizations.Add(culture, lpsd);
            }
        }
        /// <summary>
        /// 添加本地化语言
        /// </summary>
        /// <param name="culture">区域性文本</param>
        /// <param name="line">单行</param>
        public static void AddCulture(string culture, ILine line)
        {
            line.Name = line.Name.Replace(@"\n", "\n").Replace(@"\r", "\r");
            line.Info = line.Info.Replace(@"\n", "\n").Replace(@"\r", "\r");
            if (Localizations.TryGetValue(culture, out var lps))
            {
                lps.Add(line);
            }
            else
            {
                Localizations.Add(culture, new LPS_D(line));
            }
        }
        /// <summary>
        /// 添加本地化语言
        /// </summary>
        /// <param name="culture">区域性文本</param>
        /// <param name="key">匹配名称</param>
        /// <param name="value">储存值</param>
        public static void AddCulture(string culture, string key, string value) => AddCulture(culture, new Line(key, value));



        /// <summary>
        /// 通知更改
        /// </summary>
        public class NotifyChanged : INotifyPropertyChanged
        {
            /// <summary>
            /// PropertyChanged
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;
            /// <summary>
            /// 通知更改
            /// </summary>
            public void Notify()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
            /// <summary>
            /// 没啥用的文本,用于检测绑定
            /// </summary>
            public string TmpString { get; set; } = "";
            /// <summary>
            /// 获取隐藏的 Localizations
            /// </summary>
            public Dictionary<string, LPS_D> Localizations => LocalizeCore.Localizations;
        }
        /// <summary>
        /// 绑定通知更改 PropertyChanged
        /// </summary>
        public static NotifyChanged BindingNotify { get; } = new NotifyChanged();
    }
}
