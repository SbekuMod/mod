using UnityEngine;

namespace SbekuMod.utils
{
    public class ModReloader
    {

        public static void ReloadDialogs()
        {
            SbekuMod.Instance.ModHelper.Console.WriteLine("RELOADING DIALOGS");
            CharacterDialogueTree[] dialogues = GameObject.FindObjectsOfType<CharacterDialogueTree>();

            foreach (CharacterDialogueTree dialogue in dialogues) dialogue.LoadXml();

            
            SbekuMod.Instance.ModHelper.Console.WriteLine("RELOADED");
        }

        public static void ReloadTranslation()
        {
            SbekuMod.Instance.ModHelper.Console.WriteLine("RELOADING LANGUAGE");
            TextTranslation translation = GameObject.FindObjectOfType<TextTranslation>();

            translation.InitializeLanguage();

            SbekuMod.Instance.ModHelper.Console.WriteLine("LANGUAGE RELOADED");
        }

    }
}
