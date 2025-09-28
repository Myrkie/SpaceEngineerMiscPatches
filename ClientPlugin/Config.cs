using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClientPlugin.Pulsar_Patches;


namespace ClientPlugin
{
    public class Config : INotifyPropertyChanged
    {
        #region User interface

        // TODO: Settings dialog title
        public readonly string Title = "Misc Patches";

        [Separator("Settings")]
        

        [Button(description: "Toggle stdout Console")]
        public void ToggleConsole()
        {
            ConsoleManager.ToggleConsole();
        }

        #endregion

        #region Property change notification boilerplate

        public static readonly Config Default = new Config();
        public static readonly Config Current = ConfigStorage.Load();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}