using System;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using System.IO;
using Newtonsoft.Json.Linq;
using I2.Loc;

namespace SolastaPortugueseTranslation
{ 
    public class Main
    {
        private static String LANGUAGE = "Portuguese";

        // [System.Diagnostics.Conditional("DEBUG")]
        public static void Log(string msg)
        {
            if (logger != null) logger.Log(msg);
        }

        public static void Error(Exception ex)
        {
            if (logger != null) logger.Error(ex.ToString());
        }

        public static void Error(string msg)
        {
            if (logger != null) logger.Error(msg);
        }

        public static UnityModManager.ModEntry.ModLogger logger;
        public static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                var harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Error(ex);
                throw;
            }
            return true;
        }

        [HarmonyPatch(typeof(MainMenuScreen), "RuntimeLoaded")]
        internal static class MainMenuScreen_RuntimeLoaded_Patch
        {
            static void Postfix()
            {
                var translations = JArray.Parse(File.ReadAllText(UnityModManager.modsPath + @"/SolastaPortugueseTranslation/Translations.json"));
                var languageSourceData = LocalizationManager.Sources[0];
                languageSourceData.AddLanguage(LANGUAGE);
                var languageIndex = languageSourceData.GetLanguageIndex("English");

                foreach (JObject translation in translations)
                {
                    var key = translation["Key"].ToString();
                    try
                    {
                        languageSourceData.AddTerm(key).Languages[languageIndex] = translation[LANGUAGE].ToString();
                    }
                    catch
                    {
                        Log(key);
                    }
                }     
            }
        }
    }
}