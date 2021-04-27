using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using I2.Loc;
using System.Globalization;
using TMPro;

namespace SolastaUnofficialTranslations
{ 
    struct LanguageEntry
    {
        public string code, text, file;
    }

    static class Language
    {
        const String IN = "Translation-";
        const String OUT = "Export-";
        const String EXT = ".txt";

        private static readonly LanguageSourceData languageSourceData = LocalizationManager.Sources[0];

        private static readonly List<LanguageEntry> languages = new List<LanguageEntry>();

        private static void Error(string msg) => Main.Error(msg);

        public static void LoadLanguages()
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            DirectoryInfo directoryInfo = new DirectoryInfo($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}");
            FileInfo[] files = directoryInfo.GetFiles($"{IN}*{EXT}");

            languageSourceData.LoadAllLanguages();

            foreach (var file in files)
            {
                var code = file.Name.Substring(IN.Length, file.Name.Length - IN.Length - EXT.Length);
                var cultureInfo = cultureInfos.First<CultureInfo>(o => o.Name == code);
                if (cultureInfo == null)
                    Error($"unrecognized language {code} on {file.Name}");
                else if (LocalizationManager.HasLanguage(cultureInfo.DisplayName))
                    Error($"language {code} from {file.Name} already in game");
                else
                    languages.Add(new LanguageEntry()
                    {
                        code = code,
                        text = cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName),
                        file = file.Name
                    });
            }
        }

        public static IEnumerable<LanguageEntry> GetAll() => languages.AsEnumerable<LanguageEntry>();

        public static int Count() => languages.Count;

        public static void LoadTerms()
        {
            // load new language terms
            foreach (var language in languages)
            {
                // add language
                languageSourceData.AddLanguage(language.text, language.code);
                var languageIndex = languageSourceData.GetLanguageIndex(language.text);

                // add terms
                var inputFilename = $@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{language.file}";
                using (var sr = new StreamReader(inputFilename))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var splitted = line.Split(new[] { '\t', ' ' }, 2);
                        languageSourceData.AddTerm(splitted[0]).Languages[languageIndex] = splitted[1].Trim();
                    }
                }
            }
        }

        internal static void ExportTerms(int languageIndex, String code)
        {
            var outputFilename = $@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{OUT}{code}{EXT}";
            using (var sw = new StreamWriter(outputFilename))
                foreach (var term in languageSourceData.mTerms)
                    sw.WriteLine($"{term.Term} {term.Languages[languageIndex]?.Replace("\n", @"\n")}");
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
                modEntry.OnGUI = OnGUI;

                Language.LoadLanguages();
                Language.LoadTerms();

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

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            var languageSourceData = LocalizationManager.Sources[0];
            foreach(var languageData in languageSourceData.mLanguages)
                if (GUILayout.Button($"Export {languageData.Name}"))
                {
                    Language.ExportTerms(languageSourceData.GetLanguageIndex(languageData.Name), languageData.Code);
                }
        }
    }

    [HarmonyPatch(typeof(GameManager), "BindServiceSettings")]
    internal static class GameManager_BindServiceSettings_Patch
    {
        public static void Prefix(Dictionary<string, string> ___languageByCode)
        {
            if (___languageByCode != null)
                foreach (LanguageEntry language in Language.GetAll())
                    if (!___languageByCode.ContainsKey(language.code))
                        ___languageByCode.Add(language.code, language.text);
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
                String[] items = new String[top + Language.Count()];
                Array.Copy(___settingTypeDropListAttribute.Items, items, top);
                ___settingTypeDropListAttribute.Items = items;

                foreach (LanguageEntry language in Language.GetAll())
                {
                    items[top++] = language.code;
                    ___dropList.options.Add(new GuiDropdown.OptionDataAdvanced 
                    {
                        text =language.text,
                        TooltipContent = language.text
                    });
                }
            }
        }
    }
}