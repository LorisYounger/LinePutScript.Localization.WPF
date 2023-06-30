﻿using LinePutScript.Dictionary;
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
        /// 讲未翻译的文本导出成LPS格式,可以直接储存
        /// </summary>
        public static string StoreTranslationListToLPS()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string notrans in LocalizeCore.StoreTranslationList)
                sb.AppendLine($"{notrans}#{notrans}:|");
            return sb.ToString();
        }
        /// <summary>
        /// 当前本地化语言
        /// </summary>
        public static string CurrentCulture
        {
            get => currentCulture; set
            {
                if (localizations.TryGetValue(value, out var lps))
                {
                    currentLPS = lps;
                    currentCulture = value;
                }
                else
                {
                    currentCulture = "null";
                    currentLPS = null;
                }
                BindingNotify.Notify();
            }
        }

        private static LPS_D? currentLPS;
        private static string currentCulture = "null";
        private static Dictionary<string, LPS_D> localizations = new Dictionary<string, LPS_D>();

        /// <summary>
        /// 手动代码实现的本地化语言
        /// </summary>
        public static Func<string, string?>? TranslateFunc { get; set; }

        /// <summary>
        /// 当前有的本地化语言
        /// </summary>
        public static string[] AvailableCultures => localizations.Keys.ToArray();

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
                currentLPS = null;
                BindingNotify.Notify();
                return;
            }
            if (localizations.TryGetValue(culture.Name, out LPS_D? lps))
            {
                currentCulture = culture.Name;
                currentLPS = lps;
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
            if (localizations.TryGetValue(culture, out LPS_D? lps))
            {
                currentCulture = culture;
                currentLPS = lps;
            }
            else
            {
                currentCulture = "null";
                currentLPS = null;
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
            if (currentLPS != null && currentLPS.Assemblage.TryGetValue(key, out var line))
                return line;
            if (StoreTranslation)
                StoreTranslationList.Add(key);
            if (TranslateFunc != null)
            {
                string? str = TranslateFunc(key);
                if (str != null && currentLPS != null)
                {
                    currentLPS.Add(new Line(key, str));
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
        public static string Translate(string key) => Find(key)?.GetString() ?? key;
        /// <summary>
        /// 翻译文本
        /// </summary>
        /// <param name="key">翻译内容</param>
        /// <param name="replace">替换内容 {0}{1}..</param>
        /// <returns>翻译后的文本</returns>
        public static string Translate(string key, params string[] replace)
        {
            var str = Translate(key);
            for (int i = 0; i < replace.Length; i++)
            {
                str = str.Replace($"{{{i}}}", replace[i]);
            }
            return str;
        }
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
        public static void AddCulture(string culture, ILPS file)
        {
            if (localizations.TryGetValue(culture, out var lps))
            {
                lps.AddRange(file);
            }
            else
            if (file is LPS_D lpsd)
            {
                localizations.Add(culture, lpsd);
            }
            else
            {
                localizations.Add(culture, new LPS_D(file));
            }
        }
        /// <summary>
        /// 添加本地化语言
        /// </summary>
        /// <param name="culture">区域性文本</param>
        /// <param name="line">单行</param>
        public static void AddCulture(string culture, ILine line)
        {
            if (localizations.TryGetValue(culture, out var lps))
            {
                lps.Add(line);
            }
            else
            {
                localizations.Add(culture, new LPS_D(line));
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
            public event PropertyChangedEventHandler? PropertyChanged;
            public void Notify()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            }
            public string CurrentCulture => currentCulture;
            public Dictionary<string, LPS_D> Localizations => localizations;
        }
        /// <summary>
        /// 绑定通知更改 PropertyChanged
        /// </summary>
        public static NotifyChanged BindingNotify { get; } = new NotifyChanged();
    }
}