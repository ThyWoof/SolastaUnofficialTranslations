using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using I2.Loc;

namespace SolastaUnofficialTranslations
{ 
    struct LanguageEntry
    {
         public string code, text, desc, file;
    }

    class Languages
    {
        private static readonly List<LanguageEntry> languages = new List<LanguageEntry>();
        private static void Log(string msg) => Main.Log(msg);

        private static void Error(string msg) => Main.Error(msg);

        public static void LoadLanguages(string languageFile)
        {
            foreach (JToken language in JArray.Parse(File.ReadAllText(languageFile)))
            {
                LanguageEntry languageEntry;
                try
                {
                    languageEntry = new LanguageEntry()
                    {
                        code = language["Code"].ToString(),
                        text = language["Text"].ToString(),
                        desc = language["Desc"].ToString(),
                        file = language["File"].ToString()
                    };
                }
                catch
                {
                    Error("Invalid language entry:" + language.ToString());
                    continue;
                }
                languages.Add(languageEntry);
            }
        }

        public static IEnumerable<LanguageEntry> GetAll() => languages.AsEnumerable<LanguageEntry>();

        public static int Count() => languages.Count;

        public static void LoadTranslations()
        {
            foreach (var language in languages)
            {
                // add language to localization manager
                var languageSourceData = LocalizationManager.Sources[0];
                languageSourceData.AddLanguage(language.text, language.code);

                var languageIndex = languageSourceData.GetLanguageIndex(language.text);

                // add language translation keys
                languageSourceData.AddTerm("Setting/&TextLanguages" + language.code + "Title").Languages[languageIndex] = language.text;
                languageSourceData.AddTerm("Setting/&TextLanguages" + language.code + "Description").Languages[languageIndex] = language.desc;

                // add translations
                var translations = JArray.Parse(File.ReadAllText(UnityModManager.modsPath + @"/SolastaUnofficialTranslations/" + language.file));
                foreach (JObject translation in translations)
                {
                    String key;
                    try
                    {
                        key = translation["key"].ToString();
                    }
                    catch
                    {
                        Error("key not found: " + translation.ToString());
                        continue;
                    }
                    try
                    {
                        languageSourceData.AddTerm(key).Languages[languageIndex] = translation[language.code].ToString();
                    }
                    catch
                    {
                        Error(language.code + " code not found: " + translation.ToString());
                    }
                }
            }
        }
    }

    public class Main
    {
        [Conditional("DEBUG")]
        internal static void Log(string msg) => Logger.Log(msg);
        internal static void Error(Exception ex) => Logger?.Error(ex.ToString());
        internal static void Error(string msg) => Logger?.Error(msg);
        internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }


        internal static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                Logger = modEntry.Logger;

                ModBeforeDBReady();

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
            internal static void Postfix()
            {
                ModAfterDBReady();
            }
        }

        // ENTRY POINT IF YOU NEED SERVICE LOCATORS ACCESS
        internal static void ModBeforeDBReady()
        {
            Languages.LoadLanguages(UnityModManager.modsPath + @"/SolastaUnofficialTranslations/Languages.json");
            Languages.LoadTranslations();
        }

        // ENTRY POINT IF YOU NEED SAFE DATABASE ACCESS
        internal static void ModAfterDBReady()
        {

        }
    }

    [HarmonyPatch(typeof(SettingDropListItem), "Bind")]
    internal static class SettingDropListItem_Bind_Patch
    {
        public static void Postfix(
            SettingTypeDropListAttribute ___settingTypeDropListAttribute, 
            GuiDropdown ___dropList)
        {
            if (___settingTypeDropListAttribute?.Name == "TextLanguage")
            {
                int top = ___settingTypeDropListAttribute.Items.Count<String>();
                String[] items = new String[top + Languages.Count()];
                Array.Copy(___settingTypeDropListAttribute.Items, items, top);
                ___settingTypeDropListAttribute.Items = items;
                foreach (LanguageEntry language in Languages.GetAll())
                {
                    if (!items.Contains<String>(language.code))
                        items[top++] = language.code;
                    if (!___dropList.options.Any(o => o.text == language.text))
                        ___dropList.options.Add(new GuiDropdown.OptionDataAdvanced 
                        {
                            text = language.text,
                            TooltipContent = "Setting/&TextLanguage" + language.code + "Description"
                        });
                }
            }
        }
    }
}