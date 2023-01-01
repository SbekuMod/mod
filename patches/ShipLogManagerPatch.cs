using HarmonyLib;
using SbekuMod.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace SbekuMod.patches
{
    [HarmonyPatch]
    public class ShipLogManagerPatch
    {


        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.Awake))]
        public static void ShipLogManager_Awake_Prefix(ShipLogManager __instance)
        {
            ShipLogUtility.Initialize();

            if (!SbekuMod.Instance.ModHelper.Config.GetSettingsValue<bool>(ShipLogUtility.SHIP_LOG_LOAD_CUSTOM_EVENTS_SETTING) || Array.IndexOf(ShipLogUtility.VALID_LANGUAGES, SbekuMod.CurrentLanguage) == -1) 
                return;

            var originalShipLogXmlAssets = __instance._shipLogXmlAssets;
            var originalShipLogLibraryEntryData = __instance._shipLogLibrary.entryData;

            try
            {
                var textAssets = new List<TextAsset>(__instance._shipLogXmlAssets);
                var libraryEntryData = new List<EntryData>(__instance._shipLogLibrary.entryData);

                foreach (var xmlFileName in ShipLogUtility.SHIP_LOG_XML_ASSETS)
                {
                    string filePath = Path.Combine(UtilityHelper.GetProjectBasePath(), ShipLogUtility.BASE_PATH, $"{xmlFileName}.xml");

                    if (!File.Exists(filePath)) continue;

                    var xml = File.ReadAllText(filePath);
                    var textAsset = new TextAsset(xml)
                    {
                        name = xmlFileName
                    };

                    textAssets.Add(textAsset);
                    foreach (var entry in ShipLogUtility.ComputeEntryDataForLogs(textAsset))
                    {
                        if (libraryEntryData.FindIndex(existantEntry => existantEntry.id.Equals(entry.id)) == -1)
                            libraryEntryData.Add(entry);
                    }
                   
                    SbekuMod.Instance.ModHelper.Console.WriteLine($"Ship Log file added: {filePath}");
                }

                ShipLogUtility.LoadShipLogEntryData();

                __instance._shipLogXmlAssets = textAssets.ToArray();
                __instance._shipLogLibrary.entryData = libraryEntryData.ToArray();

                SbekuMod.Instance.ModHelper.Console.WriteLine($"Ship Log Library Entry Data Count: {__instance._shipLogLibrary.entryData.Length}");
            }catch(Exception e)
            {
                SbekuMod.Instance.ModHelper.Console.WriteLine($"Error Loading Custom Ship Log Entries: {e.Message}", OWML.Common.MessageType.Error);

                __instance._shipLogXmlAssets = originalShipLogXmlAssets;
                __instance._shipLogLibrary.entryData = originalShipLogLibraryEntryData;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShipLogManager), nameof(ShipLogManager.GenerateEntriesFromXml))]
        public static void ShipLogManager_GenerateEntriesFromXml_Prefix(ref TextAsset xml)
        {
            if (SbekuMod.Instance.ModHelper.Config.GetSettingsValue<bool>(ShipLogUtility.SHIP_LOG_LOAD_CUSTOM_EVENTS_SETTING)) 
                SbekuMod.Instance.ModHelper.Console.WriteLine($"Loading Ship Log: {xml.name}");

            if (!SbekuMod.Instance.ModHelper.Config.GetSettingsValue<bool>(ShipLogUtility.SHIP_LOG_DUMP_SETTING)) return;            

            string filePath = Path.Combine(UtilityHelper.GetProjectBasePath(), ShipLogUtility.BASE_PATH, xml.name + ".xml");

            if(!File.Exists(filePath))
            {
                if (!Directory.Exists(ShipLogUtility.BASE_PATH)) 
                    Directory.CreateDirectory(Path.Combine(UtilityHelper.GetProjectBasePath(), ShipLogUtility.BASE_PATH));

                SbekuMod.Instance.ModHelper.Console.WriteLine($"Dumping file: {filePath}");
                File.WriteAllText(filePath, xml.text);
            }

        }

    }
}
