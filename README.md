# Solasta Unofficial Translation

* This Mod allows additional text translations to be easily added to the Solasta cRPG game.
* I expect the community to step in, work as a team, and provide better translations files than the ones I got from Google API.
* Current translations:
	- Brazilian Portuguese (pt-BR)
	- Spanish (es)

# How to Install

* You need Unity Mod Manager installed on your game folder. Please follow instructions on Nexus or Discord.

	- Current version of Unity Mod Manager injects into the game at a later stage. I already reported this to UMM author. Meanwhile you need to edit your config.xml file inside your GAME_FOLDER/Solasta_data/Managed/UnityModManager. Replace with below:

		```
		<?xml version="1.0" encoding="utf-8"?>
			<Config xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" Name="Solasta">
			<Folder>Solasta</Folder>
			<ModsDirectory>Mods</ModsDirectory>
			<ModInfo>Info.json</ModInfo>
			<GameExe>Solasta.exe</GameExe>
			<EntryPoint>[UnityEngine.UIModule.dll]UnityEngine.Canvas.cctor:Before</EntryPoint>
			<StartingPoint>[Assembly-CSharp.dll]TacticalAdventuresApplication.Update:After</StartingPoint>
		<MinimalManagerVersion>0.22.13</MinimalManagerVersion>
		</Config>
		```
# How to fix bad translations and notify

1. Open $SOLASTA_HOME/Mods/SolastaUnofficialTranslations/Translation-<LANGUAGE-CODE>.json on a text editor
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
