using HarmonyLib;
using System.Text;
using SbekuMod.utils;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System;

namespace SbekuMod.patches
{
    [HarmonyPatch]
    public class ShipLogEntryCardPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShipLogEntryCard), nameof(ShipLogEntryCard.OnEnterComputer))]
        public static void ShipLogEntryCard_OnEnterComputer_Postfix(ShipLogEntryCard __instance)
        {
            if (ShipLogUtility.LoadedFacts == null || ShipLogUtility.LoadedFacts.Count == 0) return;

            var originalColor = __instance._border.color;

            try
            {
                var shipLogEntryData = ShipLogUtility.GetShipLogEntryData(__instance.GetEntry().GetID());

                if (shipLogEntryData == null || shipLogEntryData.Color == null) return;

                var dataColor = shipLogEntryData.Color;

                var rColor = dataColor.R / 255;
                var gColor = dataColor.G / 255;
                var bColor = dataColor.B / 255;

                var color = new Color(rColor, gColor, bColor);

                __instance._border.color = color;
                __instance._borderColor = color;
                __instance._origBorderColor = color;

                SbekuMod.Instance.ModHelper.Console.WriteLine($"Entry {shipLogEntryData.Id} Color: ({color.r}, {color.g}, {color.b})");

            }catch(Exception e)
            {
                SbekuMod.Instance.ModHelper.Console.WriteLine($"Error Updating Custom Ship Log Entry Color: {e.Message}", OWML.Common.MessageType.Error);

                __instance._border.color = originalColor;
                __instance._borderColor = originalColor;
                __instance._origBorderColor = originalColor;
            }
        }

    }
}
