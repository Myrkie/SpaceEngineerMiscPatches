using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.World;
using Sandbox.Graphics;
using SpaceEngineers.Game.Entities.Blocks;
using VRage;
using VRage.Game;
using VRageMath;

namespace ClientPlugin.MiscPatches
{
    public static class PatchMisc
    {
        public static void Patch(Harmony harmony)
        {
            var methodInfoPreviewBox = AccessTools.Method(typeof(MyGridClipboard), "UpdatePreviewBBox");
            var prefixPreviewBox = new HarmonyMethod(typeof(PatchMisc).GetMethod(nameof(UpdatePreviewBBoxPrefix), BindingFlags.Static | BindingFlags.NonPublic));
            

            harmony.Patch(methodInfoPreviewBox, prefix: prefixPreviewBox);
        }
        
        private static bool UpdatePreviewBBoxPrefix(MyGridClipboard __instance)
        {
            try
            {
                var previewGridsField = AccessTools.Field(typeof(MyGridClipboard), "m_previewGrids");
                var previewGrids = previewGridsField.GetValue(__instance) as List<MyCubeGrid>;
                if (previewGrids == null || previewGrids.Count == 0)
                    return true;

                var visibleField = AccessTools.Field(typeof(MyGridClipboard), "m_visible");
                var isVisible = visibleField?.GetValue(__instance) as bool? ?? false;
                if (!isVisible)
                    return true;

                var beingAddedField = AccessTools.Field(typeof(MyGridClipboard), "m_isBeingAdded");
                var isBeingAdded = beingAddedField?.GetValue(__instance) as bool? ?? false;
                if (isBeingAdded)
                    return true;

                var hasPreviewBBoxProp = AccessTools.Property(typeof(MyGridClipboard), "HasPreviewBBox");
                var hasPreviewBBox = hasPreviewBBoxProp?.GetValue(__instance) as bool? ?? false;
                if (!hasPreviewBBox)
                    return true;

                Vector3 characterPos = MySession.Static?.LocalHumanPlayer?.GetPosition() ?? Vector3.Zero;

                Vector3 gridPos = Vector3.Zero;
                int count = 0;
                foreach (var grid in previewGrids)
                {
                    if (grid != null)
                    {
                        gridPos += grid.WorldMatrix.Translation;
                        count++;
                    }
                }
                if (count == 0) return true;
                gridPos /= count;

                var copiedInfoField = AccessTools.Field(typeof(MyGridClipboard), "m_copiedGridsInfo");
                var copiedInfo = copiedInfoField?.GetValue(__instance);
                if (copiedInfo == null) return true;

                var pcuField = copiedInfo.GetType().GetField("PCUs");
                var totalBlocksField = copiedInfo.GetType().GetField("TotalBlocks");

                int pcuValue = (int)(pcuField?.GetValue(copiedInfo) ?? 0);
                int totalBlocksValue = (int)(totalBlocksField?.GetValue(copiedInfo) ?? 0);

                var pcuLabel = MyTexts.Get(MyCommonTexts.Clipboard_TotalPCU);
                var blocksLabel = MyTexts.Get(MyCommonTexts.Clipboard_TotalBlocks);

                
                float distanceMeters = Vector3.Distance(gridPos, characterPos);
                string distanceText = distanceMeters < 1000f
                    ? $"{Math.Ceiling(distanceMeters)} m"
                    : $"{(distanceMeters / 1000f):F1} km";

                int totalReactors = 0;
                int totalBatteries = 0;
                int totalSolarPanels = 0;
                int totalHydrogenEngines = 0;

                foreach (var grid in previewGrids)
                {
                    var (r, b, s, h) = CountPowerBlocks(grid);
                    totalReactors += r;
                    totalBatteries += b;
                    totalSolarPanels += s;
                    totalHydrogenEngines += h;
                }
                
                var displayString = new StringBuilder();
                displayString.Append(pcuLabel).Append(pcuValue);
                displayString.Append("\n");
                displayString.Append(blocksLabel).Append(totalBlocksValue);
                displayString.Append($"\nDistance: {distanceText}");
                
                
                var powerString = new StringBuilder();
                if (totalReactors > 0)
                    powerString.AppendLine($"Reactors {totalReactors}");
                if (totalBatteries > 0)
                    powerString.AppendLine($"Batteries {totalBatteries}");
                if (totalSolarPanels > 0)
                    powerString.AppendLine($"Solar Panels {totalSolarPanels}");
                if (totalHydrogenEngines > 0)
                    powerString.AppendLine($"Hydrogen Engines {totalHydrogenEngines}");
                
                Vector2 basePos = new Vector2(0.51f, 0.51f);
                float lineHeight = 0.7f * 0.035f;
                
                
                MyGuiManager.DrawString(
                    MyFontEnum.Green,
                    displayString.ToString(),
                    basePos,
                    0.7f
                );
                
                if (powerString.Length <= 0) return false;
                var powerstr = $"\nPower Blocks: {powerString.ToString().Trim()}";
                Vector2 secondPos = basePos + new Vector2(0f, lineHeight * 2);
                MyGuiManager.DrawString(
                    MyFontEnum.Green,
                    powerstr,
                    secondPos,
                    0.7f
                );

                return false;
            }
            catch
            {
                return true;
            }
        }
        
        
        private static (int reactors, int batteries, int solarPanels, int hydrogenEngine) CountPowerBlocks(MyCubeGrid grid)
        {
            if (grid == null) return (0, 0, 0, 0);

            int reactorCount = 0;
            int batteryCount = 0;
            int solarCount = 0;
            int hydrogenEngineCount = 0;

            foreach (var reactor in grid.GetFatBlocks<MyReactor>())
                if (reactor.IsFunctional && reactor.Enabled)
                    reactorCount++;

            foreach (var battery in grid.GetFatBlocks<MyBatteryBlock>())
                if (battery.IsFunctional && battery.Enabled)
                    batteryCount++;

            foreach (var solar in grid.GetFatBlocks<MySolarPanel>())
                if (solar.IsFunctional && solar.Enabled)
                    solarCount++;
            
            foreach (var hydrogenEngine in grid.GetFatBlocks<MyHydrogenEngine>())
                if (hydrogenEngine.IsFunctional && hydrogenEngine.Enabled)
                    hydrogenEngineCount++;

            return (reactorCount, batteryCount, solarCount, hydrogenEngineCount);
        }
    }
}