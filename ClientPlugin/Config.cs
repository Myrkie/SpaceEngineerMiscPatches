using ClientPlugin.Settings;
using ClientPlugin.Settings.Elements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClientPlugin.Pulsar_Patches;
using ClientPlugin.Settings.Tools;
using VRage.Input;


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
        
        [Separator("Custom Hotkeys")]
        
        [Keybind(description: "Console Keybind.")]
        public Binding ConsoleKeyBind
        {
            get;
            set => SetField(ref field, value);
        } = new(MyKeys.OemTilde, false, true);

        [Checkbox(description: "Open console on game start.")]
        public bool AutoOpen
        {
            get;
            set => SetField(ref field, value);
        } = false;

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