using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace BandCenter.Uno
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        protected CoreDispatcher Dispatcher => CoreApplication.MainView.Dispatcher;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> _propertyBackingDictionary = new Dictionary<string, object>();

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            object value;
            if (_propertyBackingDictionary.TryGetValue(propertyName, out value))
            {
                return (T)value;
            }

            return default(T);
        }

        protected bool Set<T>(T newValue, string extraPropsToRaise = "", [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            if (EqualityComparer<T>.Default.Equals(newValue, Get<T>(propertyName))) return false;

            _propertyBackingDictionary[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            if (!string.IsNullOrEmpty(extraPropsToRaise))
            {
                var props = extraPropsToRaise.Split(',');
                foreach (var prop in props)
                {
                    OnPropertyChanged(prop);
                }
            }
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {

            DispatchAsync(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }

        protected async Task DispatchAsync(DispatchedHandler callback)
        {
            // As WASM is currently single-threaded, and Dispatcher.HasThreadAccess always returns false for broader compatibility  reasons
            // the following code ensures the app always directly invokes the callback on WASM.
            var hasThreadAccess = Dispatcher.HasThreadAccess;

            if (hasThreadAccess)
            {
                callback.Invoke();
            }
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, callback);
            }
        }
    }
}
