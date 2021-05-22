# Solasta Unofficial Translation

* This Mod allows additional text translations to be easily added to the Solasta cRPG game.
* I expect the community to step in, work as a team, and provide better translations files than the ones I got from Google API.
* Current translations
	- Brazilian Portuguese (pt-BR)
	- Spanish (es)
	- Italian (it)
* You need [Unity Mod Manager 0.23.4](https://www.nexusmods.com/site/mods/21?tab=files) or higher installed in your game folder.

# How to export official languages

1. Press Ctrl-F10 to Open UMM Window
2. Expand Solasta Unofficial Translation options
3. Press the Export #Language# button (text files are exported to Mods/SolastaUnofficialTranslations/Export-#code#)

# How to fix bad translations

* Translation files are stored in $SOLASTA_HOME/Mods/SolastaUnofficialTranslations/Translation-#LANGUAGE-CODE#.txt organized by categories
* Translation files are TAB separated
* Apply your corrections (use the Export function to get the original term from official languages)
* Submit a request to this repo

# How to create new translations

* Install Python 3.*
* Install Python deep_translator library
	- `pip3 install deep_translator`
* Export EN official language
* Run batch translator
	- `py Scripts\translate.py EXPORT_FOLDER_NAME -c LANGUAGE_CODE`

# How to compile

1. Download and install [Unity Mod Manager (UMM)](https://www.nexusmods.com/site/mods/21)
2. Execute UMM, Select Solasta, and Install
3. Download and install [SolastaModApi](https://www.nexusmods.com/solastacrownofthemagister/mods/48) using UMM
4. Create the environment variable *SolastaInstallDir* and point it to your Solasta game home folder
	- tip: search for "edit the system environment variables" on windows search bar
5. Use "Install Release" or "Install Debug" to have the Mod installed directly to your Game Mods folder

Unity Mod Manager and this mod template make use of [Harmony](https://go.microsoft.com/fwlink/?linkid=874338)

# How to debug

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
		* Copy *UnityPlayer.dll* and *WinPixEventRuntime.dll* to clipboard
	* Navigate to the Solasta game folder
		* Rename *UnityPlayer.dll* to *UnityPlayer.dll.original*
		* Paste *UnityPlayer.dll* and *WinPixEventRuntime.dll* from clipboard
5. You can now attach the Unity Debugger from Visual Studio 2019, Debug -> Attach Unity Debug