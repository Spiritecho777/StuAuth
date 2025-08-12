using StuAuth.Classe;
using StuAuth.Properties;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace StuAuth.Classe
{
    public class Loc : INotifyPropertyChanged
    {
        public static event EventHandler? LanguageChanged;

        private readonly ResourceManager _resourceManager = Resources.ResourceManager;
        private CultureInfo _culture = CultureInfo.CurrentUICulture;

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (_culture != value)
                {
                    _culture = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
                }
            }
        }

        public string this[string key] => _resourceManager.GetString(key, _culture) ?? $"[{key}]";
        public string this[string key, params object[] args]
        {
            get
            {
                string format = _resourceManager.GetString(key, _culture) ?? $"[{key}]";
                return string.Format(format, args);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }


    public class LocExtension : MarkupExtension
    {
        public string Key { get; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var loc = (Loc)Application.Current.Resources["Loc"];

            var binding = new Binding($"[{Key}]")
            {
                Source = loc,
                Mode = BindingMode.OneWay
            };

            // Abonnement au changement de langue
            Loc.LanguageChanged += (s, e) =>
            {
                var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
                if (target?.TargetObject is DependencyObject dObj &&
                    target.TargetProperty is DependencyProperty dp)
                {
                    BindingOperations.SetBinding(dObj, dp, binding);
                }
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}