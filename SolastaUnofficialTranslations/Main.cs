using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using HarmonyLib;
using I2.Loc;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SolastaUnofficialTranslations
{ 
    struct LanguageEntry
    {
        public string code, text, directory;
    }

    static class Language
    {
        const String IN = "Translations-";
        const String OUT = "Export-";
        const String EXT = ".txt";

        public static readonly LanguageSourceData languageSourceData = LocalizationManager.Sources[0];

        private static readonly List<LanguageEntry> languages = new List<LanguageEntry>();

        private static void Error(string msg) => Main.Error(msg);

        private static void Log(string msg) => Main.Log(msg);

        public static void LoadOfficialLanguages() => languageSourceData.LoadAllLanguages();

        public static void LoadCustomLanguages()
        {
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            DirectoryInfo directoryInfo = new DirectoryInfo($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}");
            DirectoryInfo[] directories = directoryInfo.GetDirectories($"{IN}??");

            foreach (var directory in directories)
            {

                var code = directory.Name.Substring(IN.Length, directory.Name.Length - IN.Length);
                var cultureInfo = cultureInfos.First<CultureInfo>(o => o.Name == code);
                if (cultureInfo == null)
                    Error($"unrecognized language {code} on {directory.Name}");
                else if (LocalizationManager.HasLanguage(cultureInfo.DisplayName))
                    Error($"language {code} from {directory.Name} already in game");
                else
                {
                    languages.Add(new LanguageEntry()
                    {
                        code = code,
                        text = cultureInfo.TextInfo.ToTitleCase(cultureInfo.NativeName),
                        directory = directory.Name
                    });
                    Log($"Language {code} detected.");
                }
            }
        }

        public static IEnumerable<LanguageEntry> GetCustomLanguages() => languages.AsEnumerable<LanguageEntry>();

        public static int Count() => languages.Count;

        public static Dictionary<string, string> GetCorrections(string code)
        {
            Dictionary<string, string> corrections = new Dictionary<string, string> { { @"\n", "\n"} };
            string filename = $@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/Corrections-{code}{EXT}";

            if (File.Exists(filename))
            {
                using (var sr = new StreamReader(filename))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var splitted = line.Split('\t');
                        corrections.Add(splitted[0], splitted[1]);
                    }
                }
            } else
            {
                Log($"Corrections file {filename} not loaded.");
            }

            return corrections;
        }

        public static void LoadCustomTerms()
        {
            // load new language terms
            foreach (var language in languages)
            {
                // add language
                languageSourceData.AddLanguage(language.text, language.code);
                var languageIndex = languageSourceData.GetLanguageIndex(language.text);

                // get corrections
                var corrections = GetCorrections(language.code);

                // add terms
                DirectoryInfo directoryInfo = new DirectoryInfo($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{language.directory}");
                FileInfo[] files = directoryInfo.GetFiles($"*{EXT}");
                foreach (var file in files)
                {
                    string filename = $@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{language.directory}/{file.Name}";
                    using (var sr = new StreamReader(filename))
                    {
                        String line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            var splitted = line.Split(new[] { '\t', ' ' }, 2);
                            var term = splitted[0];
                            var text = splitted[1];
                            var corrected = Regex.Replace(text, string.Join("|", corrections.Keys.Select(k => k.ToString()).ToArray()), m => corrections[m.Value]);
                            languageSourceData.AddTerm(term).Languages[languageIndex] = corrected;
                        }
                    }
                }
            }
        }

        internal static string FixCategory(string category)
        {
            if (category == "MonsterAttack")
                return "MonsterAttacks";
            else if (category == "Environment Effect")
                return "EnvironmentEffect";
            else if (category == "Equipement")
                return "Equipment";
            else
                return category;
        }

        internal static void ExportTerms(int languageIndex, String code)
        {
            System.IO.Directory.CreateDirectory($@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{OUT}{code}");
            foreach (var category in languageSourceData.GetCategories())
            {
                if (!category.Contains(":"))
                {
                    var fixedCategory = FixCategory(category);
                    var outputFilename = $@"{UnityModManager.modsPath}/{typeof(Main).Namespace}/{OUT}{code}/{fixedCategory}{EXT}";
                    using (var sw = new StreamWriter(outputFilename))
                        foreach (var termName in languageSourceData.GetTermsList(category))
                        {
                            var term = languageSourceData.GetTermData(termName);
                            sw.WriteLine($"{term.Term}\t{term.Languages[languageIndex]?.Replace("\n", @"\n")}");
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
                modEntry.OnGUI = OnGUI;

                Language.LoadOfficialLanguages();
                Language.LoadCustomLanguages();
                Language.LoadCustomTerms();

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
            var languageSourceData = Language.languageSourceData;
            foreach(var languageData in languageSourceData.mLanguages)
                if(languageData.IsEnabled() && GUILayout.Button($"Export {languageData.Name}"))
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
                foreach (LanguageEntry language in Language.GetCustomLanguages())
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

                foreach (LanguageEntry language in Language.GetCustomLanguages())
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