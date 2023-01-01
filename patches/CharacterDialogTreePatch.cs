using System;
using System.IO;
using HarmonyLib;
using OWML.Common;
using SbekuMod.utils;
using UnityEngine;

namespace SbekuMod.patches
{
    [HarmonyPatch]
    public class CharacterDialogTreePatch
    {

        private static readonly string BASE_PATH = "assets/dialogues";
        private static readonly string DIALOG_DUMP_SETTING = "Effettua dump degli alberi di dialogo";
        private static readonly string[] VALID_LANGUAGES = {"ITA"};

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterDialogueTree), nameof(CharacterDialogueTree.LoadXml))]
        public static void CharacterDialogTree_LoadXml_Prefix(CharacterDialogueTree __instance)
        {
            if (Array.IndexOf(VALID_LANGUAGES, SbekuMod.CurrentLanguage) == -1) return;

            string filePath = Path.Combine(UtilityHelper.GetProjectBasePath(), BASE_PATH, __instance._xmlCharacterDialogueAsset.name + ".xml");
            SbekuMod.Instance.ModHelper.Console.WriteLine("Loading Dialogue Tree: " + filePath);

            TextAsset originalTextAsset = __instance._xmlCharacterDialogueAsset;
            try
            {
                if (File.Exists(filePath))
                {
                    var xmlOverride = File.ReadAllText(filePath);
                    TextAsset xmlAsset = new(xmlOverride)
                    {
                        name = __instance._xmlCharacterDialogueAsset.name
                    };

                    __instance._xmlCharacterDialogueAsset = xmlAsset;
                }else
                {
                    if (SbekuMod.Instance.ModHelper.Config.GetSettingsValue<bool>(DIALOG_DUMP_SETTING))
                    {
                        if (!Directory.Exists(BASE_PATH)) Directory.CreateDirectory(Path.Combine(UtilityHelper.GetProjectBasePath(), BASE_PATH));

                        File.WriteAllText(filePath, __instance._xmlCharacterDialogueAsset.text);
                        __instance._xmlCharacterDialogueAsset = originalTextAsset;
                    }
                }
            }
            catch (Exception e)
            {
                SbekuMod.Instance.ModHelper.Console.WriteLine("Unable to load dialogue file: `" + filePath + "`. Falling back to default logic", MessageType.Error);
                SbekuMod.Instance.ModHelper.Console.WriteLine(e.StackTrace, MessageType.Error);
                __instance._xmlCharacterDialogueAsset = originalTextAsset;
            }
        }

    }
}
