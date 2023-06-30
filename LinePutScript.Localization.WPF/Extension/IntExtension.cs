using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
#nullable enable
namespace LinePutScript.Localization.WPF
{
    /// <summary>
    /// WPF绑定转换成Int
    /// </summary>
    public class IntExtension : MarkupExtension
    { 
        /// <summary>
        /// 查找值
        /// </summary>
        public string? Key { get; set; }
        /// <summary>
        /// 查找值
        /// </summary>
        public Binding? KeySource { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public int DefValue { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public Binding? DefValueSource { get; set; }


        /// <summary>
        /// WPF绑定转换Int
        /// </summary>
        /// <param name="key">查找值</param>
        public IntExtension(string key)
        {
            Key = key;
        }
        /// <summary>
        /// WPF绑定转换Int
        /// </summary>
        /// <param name="keySource">查找值</param>
        /// <param name="defvalueSource">默认值</param>
        public IntExtension(Binding keySource, Binding defvalueSource)
        {
            KeySource = keySource;
            DefValueSource = defvalueSource;
        }
        /// <summary>
        /// WPF绑定转换Int
        /// </summary>
        /// <param name="keySource">查找值</param>
        /// <param name="defvalue">默认值</param>
        public IntExtension(Binding keySource, int defvalue)
        {
            KeySource = keySource;
            DefValue = defvalue;
        }
        /// <summary>
        /// WPF绑定转换Int
        /// </summary>
        /// <param name="key">查找值</param>
        /// <param name="defvalue">默认值</param>
        public IntExtension(string key, int defvalue)
        {
            Key = key;
            DefValue = defvalue;
        }
        /// <summary>
        /// WPF绑定转换Int
        /// </summary>
        /// <param name="key">查找值</param>
        /// <param name="defvalueSource">默认值</param>
        public IntExtension(string key, Binding defvalueSource)
        {
            Key = key;
            DefValueSource = defvalueSource;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            MultiBinding Binding = new MultiBinding
            {
                Converter = new LocalConverter(Key, DefValue),
                NotifyOnSourceUpdated = true
            };
            Binding.Bindings.Add(new Binding()
            {
                Source = LocalizeCore.BindingNotify,
                Path = new PropertyPath("CurrentCulture")
            });
            if (KeySource != null)
            {
                Binding.Bindings.Add(KeySource);
            }
            if (DefValueSource != null)
            {
                Binding.Bindings.Add(DefValueSource);
            }
            return Binding.ProvideValue(serviceProvider);
        }
        private class LocalConverter : IMultiValueConverter
        {
            /// <summary>
            /// 查找值
            /// </summary>
            public string? Key { get; set; }
            /// <summary>
            /// 替换词
            /// </summary>
            public int DefValue { get; set; }
            /// <summary>
            /// 生成一个WPF绑定转换成字符串, 开发者无需使用这个
            /// </summary>
            public LocalConverter(string? key = null, int defvalue = default)
            {
                Key = key;
                DefValue = defvalue;
            }

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                string? k = null;
                if (Key != null)
                {
                    k = Key;
                }
                else if (values.Length == 2)
                {
                    k = System.Convert.ToString(values[1]);
                }
                if (Key != null && values.Length == 2)
                {
                    DefValue = System.Convert.ToInt32(values[1]);
                }
                else if (values.Length == 3)
                {
                    DefValue = System.Convert.ToInt32(values[2]);
                }
                return LocalizeCore.GetInt(k ?? "", DefValue);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
        }
    }
}
