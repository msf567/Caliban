
RequestExecutionLevel admin
Name  "CALIBAN"
# define name of installer
OutFile "caliban-setup.exe"
 
# define installation directory
InstallDir $DESKTOP\CALIBAN
 
ShowInstDetails show

CompletedText "Installation completed! CALIBAN.exe placed on desktop. Enjoy! - C"

# start default section
Section

    # set the installation directory as the destination for the following actions
    SetOutPath $INSTDIR
	
	File CALIBAN.exe
	File Note.exe
	File Caliban.Core.dll
	File Colorful.Console.dll
	File CLIGL.dll
	File CSCore.dll
	File EventHook.dll
	File NAudio.dll
	File Newtonsoft.Json.dll
	File Mono.Cecil.dll
	File Treasures.dll
	File demoClue.wav
	File town_dusk_short.wav

    # create a shortcut named "new shortcut" in the start menu programs directory
    # point the new shortcut at the program uninstaller
    CreateShortCut "$SMPROGRAMS\CALIBAN.lnk" "$INSTDIR\CALIBAN.exe"
	CreateShortCut "$DESKTOP\CALIBAN.lnk" "$INSTDIR\CALIBAN.exe"

	
	MessageBox MB_OK "Installation completed! CALIBAN.exe placed on desktop. Enjoy! -C"

SectionEnd