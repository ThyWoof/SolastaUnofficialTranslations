# Solasta Portuguese Translation

Although this Mod is finished the translation is a work in progress. I expect help to fine tune it.

# How to Translate

1. Open $SOLASTA_HOME/Mods/SolastaPortugueseTranslation/Translations.json on a text editor
2. Apply your corrections
3. Submit a request to this repo

# How to Compile

1. Install Visual Studio 2019
2. Edit SolastaCustomMerchants.csproj and fix your game folder on line 5
3. Use "InstallDebug" to have it installed directly to your Mods folder

# How to Debug

1. Open Solasta game folder
	* Rename Solasta.exe to Solasta.exe.original
	* Rename UnityPlayer.dll to UnityPlayer.dll.original
	* Add below entries to *Solasta_Data\boot.config*:
```
wait-for-managed-debugger=1
player-connection-debug=1
```
2. Download and install [7zip](https://www.7-zip.org/a/7z1900-x64.exe)
3. Download [Unity Editor 2019.4.19](https://download.unity3d.com/download_unity/ca5b14067cec/Windows64EditorInstaller/UnitySetup64-2019.4.19f1.exe)
4. Open Downloads folder
	* Right-click UnitySetup64-2019.4.1f1.exe, 7Zip -> Extract Here
	* Navigate to Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_development_mono
	* Copy *WindowsPlayer.exe* (and rename to *Solasta.exe*), *UnityPlayer.dll* and *WinPixEventRuntime.dll* to Solasta game folder

You can now attach the Unity Debugger from Visual Studio 2019, Debug -> Attach Unity Debug
