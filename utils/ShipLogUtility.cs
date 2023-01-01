using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace SbekuMod.utils
{
    public class ShipLogUtility
    {

        public static readonly string BASE_PATH = "assets/shipLog";
        public static readonly string SHIP_LOG_DUMP_SETTING = "Effettua dump registro della nave";
        public static readonly string SHIP_LOG_LOAD_CUSTOM_EVENTS_SETTING = "Carica eventi nel registro della nave";
        public static readonly string[] VALID_LANGUAGES = { "ITA" };

        public static readonly string[] SHIP_LOG_XML_ASSETS = { "Guide", "Curiosities", "Endings" };

        public static readonly Vector2 CARD_STARTING_POINT = new(105, -1556);
        public static readonly float CARD_BOX_WIDTH = 1300f;
        public static readonly Vector2 CARD_SIZE = new(300, 500);

        public static Vector2 NEXT_CARD_STARTING_POINT = CARD_STARTING_POINT;
        public static List<string> LoadedFacts;
        public static List<ShipLogEntryData> LoadedFactEntryData;

        public static void Initialize()
        {
            LoadedFacts = new List<string>();
            LoadedFactEntryData = new List<ShipLogEntryData>();
            NEXT_CARD_STARTING_POINT = CARD_STARTING_POINT;
        }

        public static List<EntryData> ComputeEntryDataForLogs(TextAsset textAsset)
        {
            List<EntryData> entries = new();

            XElement xelement = XDocument.Parse(OWUtilities.RemoveByteOrderMark(textAsset)).Element("AstroObjectEntry");
            string value = xelement.Element("ID").Value;
            foreach (XElement xelement2 in xelement.Elements("Entry"))
            {
                ShipLogEntry shipLogEntry = new ShipLogEntry(value, xelement2, "");

                var cardStartingPoint = NEXT_CARD_STARTING_POINT;
                var cardFinishPoint = NEXT_CARD_STARTING_POINT + new Vector2(CARD_SIZE.x, 0);

                if (cardFinishPoint.x > CARD_STARTING_POINT.x + CARD_BOX_WIDTH)
                {
                    cardStartingPoint = new Vector2(CARD_STARTING_POINT.x, NEXT_CARD_STARTING_POINT.y - CARD_SIZE.y);
                    cardFinishPoint = cardStartingPoint + new Vector2(CARD_SIZE.x, 0);
                }

                var entryData = new EntryData()
                {
                    id = shipLogEntry.GetID(),
                    //sprite = testDataEntry.sprite,
                    //altSprite = testDataEntry.altSprite,
                    cardPosition = cardStartingPoint,
                };

                NEXT_CARD_STARTING_POINT = cardFinishPoint;
                entries.Add(entryData);

                shipLogEntry.GetRumorFacts().ForEach(entries => LoadedFacts.Add(entries.GetID()));
            }

            return entries;
        }

        public static ShipLogEntryData GetShipLogEntryData(string id)
        {
            if (LoadedFactEntryData == null) return null;

            foreach (var entryData in LoadedFactEntryData)
                if (entryData.Id.Equals(id)) return entryData;

            return null;
        }

        public static void LoadShipLogEntryData()
        {
            if (LoadedFactEntryData == null) LoadedFactEntryData = new();

            try
            {
                foreach (var xmlFileName in SHIP_LOG_XML_ASSETS)
                {
                    string filePath = Path.Combine(UtilityHelper.GetProjectBasePath(), BASE_PATH, $"{xmlFileName}.json");

                    if (!File.Exists(filePath)) continue;

                    var json = File.ReadAllText(filePath);
                    ShipLogEntryData[] entryDataArray = JsonConvert.DeserializeObject<ShipLogEntryData[]>(json);

                    foreach (var entry in entryDataArray) LoadedFactEntryData.Add(entry);
                }

                SbekuMod.Instance.ModHelper.Console.WriteLine($"Loaded Ship Log Entry Data: {LoadedFactEntryData.Count}");

            }catch(Exception e)
            {
                SbekuMod.Instance.ModHelper.Console.WriteLine($"Error Loading Custom Ship Log Entry Data: {e.Message}", OWML.Common.MessageType.Error);
                LoadedFactEntryData = new();
            }

        }

        public static void RevealAllFacts()
        {
            var shipLogManager = SbekuMod.FindObjectOfType<ShipLogManager>();
            shipLogManager.RevealAllFacts();
        }

        public static void RevealAllLoadedFacts()
        {
            if (LoadedFacts == null) return;

            var shipLogManager = SbekuMod.FindObjectOfType<ShipLogManager>();

            LoadedFacts.ForEach(entryId => shipLogManager.RevealFact(entryId, saveGame: false));
        }

    }
}
