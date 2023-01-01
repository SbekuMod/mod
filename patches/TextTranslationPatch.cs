using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using HarmonyLib;
using OWML.Common;
using SbekuMod.utils;
using UnityEngine;

namespace SbekuMod.patches
{
    [HarmonyPatch]
    public class TextTranslationPatch
    {
        private static string BASE_PATH = "assets/translations";

        class LoadedTranslation
        {
            public TextAsset asset;
            public bool isResource;
        }

        private static LoadedTranslation GetTranslation(TextTranslation instance)
        {

            var filePath = Path.Combine(UtilityHelper.GetProjectBasePath(), BASE_PATH, TextTranslation.s_langFolder[(int)instance.m_language] + ".xml");
            SbekuMod.Instance.ModHelper.Console.WriteLine("Loading Translation: " + filePath);
            try
            {
                if (File.Exists(filePath))
                {
                    var xmlOverride = File.ReadAllText(filePath);

                    return new LoadedTranslation()
                    {
                        asset = new(xmlOverride),
                        isResource = false
                    };
                }
            }
            catch (Exception e)
            {
                SbekuMod.Instance.ModHelper.Console.WriteLine("Unable to load translation file: `" + filePath + "`. Falling back to default logic", MessageType.Error);
                SbekuMod.Instance.ModHelper.Console.WriteLine(e.StackTrace, MessageType.Error);
            }

            // DEFAULT LOGIC
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Translation");
            stringBuilder.Append(Path.DirectorySeparatorChar);
            stringBuilder.Append(TextTranslation.s_langFolder[(int)instance.m_language]);
            stringBuilder.Append(Path.DirectorySeparatorChar);
            stringBuilder.Append("Translation");

            TextAsset textAsset = Resources.Load<TextAsset>(stringBuilder.ToString());
            File.WriteAllText(filePath, textAsset.text);

            return new LoadedTranslation()
            {
                asset = textAsset,
                isResource = true
            };
        }

        // DEFAULT LOGIC WITH MINOR CHANGES
        private static void LanguageSetup(TextAsset textAsset, bool isResource, TextTranslation __instance, TextTranslation.LanguageChanged ___OnLanguageChanged)
        {
            if (null == textAsset)
            {
                Debug.LogError("Unable to load text translation file for language " + TextTranslation.s_langFolder[(int)__instance.m_language]);
                return;
            }
            string text = OWUtilities.RemoveByteOrderMark(textAsset);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(text);
            XmlNode xmlNode = xmlDocument.SelectSingleNode("TranslationTable_XML");
            XmlNodeList xmlNodeList = xmlNode.SelectNodes("entry");
            TextTranslation.TranslationTable_XML translationTable_XML = new TextTranslation.TranslationTable_XML();
            foreach (object obj in xmlNodeList)
            {
                XmlNode xmlNode2 = (XmlNode)obj;
                translationTable_XML.table.Add(new TextTranslation.TranslationTableEntry(xmlNode2.SelectSingleNode("key").InnerText, xmlNode2.SelectSingleNode("value").InnerText));
            }
            foreach (object obj2 in xmlNode.SelectSingleNode("table_shipLog").SelectNodes("TranslationTableEntry"))
            {
                XmlNode xmlNode3 = (XmlNode)obj2;
                translationTable_XML.table_shipLog.Add(new TextTranslation.TranslationTableEntry(xmlNode3.SelectSingleNode("key").InnerText, xmlNode3.SelectSingleNode("value").InnerText));
            }
            foreach (object obj3 in xmlNode.SelectSingleNode("table_ui").SelectNodes("TranslationTableEntryUI"))
            {
                XmlNode xmlNode4 = (XmlNode)obj3;
                translationTable_XML.table_ui.Add(new TextTranslation.TranslationTableEntryUI(int.Parse(xmlNode4.SelectSingleNode("key").InnerText), xmlNode4.SelectSingleNode("value").InnerText));
            }
            __instance.m_table = new TextTranslation.TranslationTable(translationTable_XML);

            if (isResource) Resources.UnloadAsset(textAsset);
            ___OnLanguageChanged?.Invoke();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.SetLanguage))]
        public static bool TextTranslation_SetLanguage_Prefix(TextTranslation __instance, ref TextTranslation.Language lang, ref TextTranslation.LanguageChanged ___OnLanguageChanged)
        {

            if (lang == TextTranslation.Language.UNKNOWN)
            {
                Debug.LogWarning("Tried to set Language to UNKNOWN; defaulting to English(US)");
                lang = TextTranslation.Language.ENGLISH;
            }
            __instance.m_language = lang;
            __instance.m_table = null;

            LoadedTranslation loadedTranslation = GetTranslation(__instance);

            LanguageSetup(loadedTranslation.asset, loadedTranslation.isResource, __instance, ___OnLanguageChanged);

            // UPDATE LANGUAGE
            SbekuMod.CurrentLanguage = TextTranslation.s_langFolder[(int)__instance.m_language];

            return false; // DO NOT EXECUTE ORIGINAL FUNCTION
        }
    }
}
