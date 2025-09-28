using System;
using System.Reflection;
using HarmonyLib;
using NLog;

namespace ClientPlugin.Pulsar_Patches
{
    public static class PatchPulsarLogs
    {
        public static void Patch(Harmony harmony)
        {
            var sharedWriteLine = AccessTools.Method(
                typeof(Pulsar.Shared.LogFile),
                "WriteLine",
                new[] { typeof(string), typeof(LogLevel) }
            );

            var postfixShared = new HarmonyMethod(
                typeof(PatchPulsarLogs).GetMethod(
                    nameof(SharedWriteLinePostfix),
                    BindingFlags.Static | BindingFlags.NonPublic
                )
            );

            harmony.Patch(sharedWriteLine, postfix: postfixShared);

            var compilerWriteLine = AccessTools.Method(
                typeof(Pulsar.Compiler.LogFile),
                "WriteLine",
                new[] { typeof(string), typeof(LogLevel) }
            );

            var postfixCompiler = new HarmonyMethod(
                typeof(PatchPulsarLogs).GetMethod(
                    nameof(CompilerWriteLinePostfix),
                    BindingFlags.Static | BindingFlags.NonPublic
                )
            );

            harmony.Patch(compilerWriteLine, postfix: postfixCompiler);
        }

        private static void SharedWriteLinePostfix(string text, LogLevel level)
        {
            try
            {
                var lvl = level?.ToString().ToUpper() ?? "INFO";
                Console.WriteLine($"[PULSAR:{lvl}] {text}");
            }
            catch (Exception ex)
            {
                Plugin.WriteToPulsarLog($"Failed to patch Shared log {ex}", LogLevel.Error);
            }
        }

        private static void CompilerWriteLinePostfix(string text, LogLevel level)
        {
            try
            {
                var lvl = level?.ToString().ToUpper() ?? "INFO";
                Console.WriteLine($"[PULSAR-COMPILER:{lvl}] {text}");
            }
            catch (Exception ex)
            {
                Plugin.WriteToPulsarLog($"Failed to patch Compiler log {ex}", LogLevel.Error);

            }
        }
    }
}
