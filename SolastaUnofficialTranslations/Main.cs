using System;
using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using System.IO;
using Newtonsoft.Json.Linq;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;

namespace SolastaUnofficialTranslations
{ 
    public class Main
    {
        private static JArray languages;

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

        static void LoadLanguages()
        {
            languages = JArray.Parse(File.ReadAllText(UnityModManager.modsPath + @"/SolastaUnofficialTranslations/Languages.json"));
            foreach (JObject language in languages)
            {
                String code, text, desc, file;
                try
                {
                    code = language["Code"].ToString();
                    text = language["Text"].ToString();
                    desc = language["Desc"].ToString();
                    file = language["File"].ToString();
                }
                catch
                {
                    Log("skipping language: " + language.ToString());
                    continue;
                }

                // add language to localization manager
                var languageSourceData = LocalizationManager.Sources[0];
                languageSourceData.AddLanguage(text, code);
                var languageIndex = languageSourceData.GetLanguageIndex(text);
                languageSourceData.EnableLanguage(text, true);

                // add language translation keys
                languageSourceData.AddTerm("Setting/&TextLanguage" + code + "Title").Languages[languageIndex] = text;
                languageSourceData.AddTerm("Setting/&TextLanguage" + code + "Description").Languages[languageIndex] = desc;

                // add current language translations
                var translations = JArray.Parse(File.ReadAllText(UnityModManager.modsPath + @"/SolastaUnofficialTranslations/" + file));
                foreach (JObject translation in translations)
                {
                    var key = translation["key"].ToString();
                    try
                    {
                        languageSourceData.AddTerm(key).Languages[languageIndex] = translation[code].ToString();
                    }
                    catch
                    {
                        Log("FAIL: language " + code + " on key: " + key);
                    }
                }
            }
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                logger = modEntry.Logger;
                LoadLanguages();
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

        [HarmonyPatch(typeof(GameManager), "BindServiceSettings")]
        internal static class GameManager_BindServiceSettings_Patch
        {
            public static void Prefix(GameManager __instance, Dictionary<string, string> ___languageByCode)
            {
                String code, text;
                foreach (JObject language in languages)
                {
                    try
                    {
                        code = language["Code"].ToString();
                        text = language["Text"].ToString();
                        ___languageByCode.Add(code, text);
                    }
                    catch
                    {
                        continue;
                    }
                }  
            }
        }

        [HarmonyPatch(typeof(SettingDropListItem), "Bind")]
        internal static class SettingDropListItem_Bind_Patch
        {
            public static void Postfix(
                SettingDropListItem __instance, Setting setting, 
                SettingItem.OnSettingChangedHandler onSettingChanged, 
                SettingTypeDropListAttribute ___settingTypeDropListAttribute, GuiDropdown ___dropList)
            {
                if (__instance == null)
                    return;

                if (___settingTypeDropListAttribute?.Name == "TextLanguage")
                {
                    String code, text;
                    foreach (JObject language in languages)
                    {
                        try
                        {
                            code = language["Code"].ToString();
                            text = language["Text"].ToString();
                        }
                        catch
                        {
                            continue;
                        }

                        if (!___dropList.options.Any(o => o.text == text))
                        {
                            ___dropList.options.Add(new GuiDropdown.OptionDataAdvanced
                                {
                                    text = text,
                                    TooltipContent = "Setting/&TextLanguage" + code + "Description"
                            });

                            int index = 0;
                            String[] items = new String[___settingTypeDropListAttribute.Items.Count<String>() + 1];
                            foreach (var item in ___settingTypeDropListAttribute.Items)
                                items[index++] = item;
                            items[index] = code;
                            ___settingTypeDropListAttribute.Items = items;
                        }
                    }
                }
            }
        }
    }
}