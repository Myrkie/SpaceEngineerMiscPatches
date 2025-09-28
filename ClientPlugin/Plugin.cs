using System;
using System.Reflection;
using ClientPlugin.MiscPatches;
using ClientPlugin.Pulsar_Patches;
using ClientPlugin.Settings;
using ClientPlugin.Settings.Layouts;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using VRage.Plugins;

namespace ClientPlugin
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin
    {
        private static Action<string, NLog.LogLevel> PulsarLog;
        
        public static readonly string PluginName = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(
            Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title;
        public static void WriteToPulsarLog(string logMsg, NLog.LogLevel logLevel)
        {
            PulsarLog?.Invoke($"[{PluginName}] {logMsg}", logLevel);
        }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public static Plugin Instance { get; private set; }
        // ReSharper disable once NotAccessedField.Local
        private SettingsGenerator _settingsGenerator;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            ConsoleManager.Init();
            Instance = this;
            Instance._settingsGenerator = new SettingsGenerator();

            Harmony harmony = new Harmony(PluginName);
            PatchPulsarLogs.Patch(harmony);
            PatchMisc.Patch(harmony);
        }

        public void Dispose()
        {
            Instance = null;
        }

        public void Update()
        {
            ConsoleManager.Update();
        }

        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            Instance._settingsGenerator.SetLayout<Simple>();
            MyGuiSandbox.AddScreen(Instance._settingsGenerator.Dialog);
        }
    }
}