SevenDeskBar
=============================
Desktop toolbar for Windows: shows the content of one or more folders as a toolbar docked to an edge of the screen,
like the toolbars of the Windows XP taskbar.

Project page:   https://sourceforge.net/projects/sevendeskbar/
Downloads:      https://sourceforge.net/projects/sevendeskbar/files/
Source code:    https://sourceforge.net/p/sevendeskbar/code/
Git clone:      git clone https://git.code.sf.net/p/sevendeskbar/code sevendeskbar-code
Build:          dotnet build -c Release   (requires the .NET SDK; output in bin\Release\net48)

Version 1.2.0.1
=============================
Fixed: Visual Studio Code (and other Electron applications) did not start from the toolbar when the toolbar had been launched from a VS Code terminal
	(the inherited ELECTRON_RUN_AS_NODE variable is now removed at startup)

Version 1.2.0.0
=============================
Native high DPI support (per monitor): toolbar, icons, texts and windows are drawn sharp at any zoom level and are redrawn when the zoom changes
Ticket 16: per-folder settings (title, text, large icons) - "Options..." button or double click on a folder in the settings window
Ticket 17: fixed endless redraws after saving the settings (appbar repositioning ping-pong, restart at every save with Aero enabled)
	With auto hide the toolbar no longer reserves screen space: showing/hiding it does not resize the maximized windows any more
Ticket 18: icons always drawn at the same size (shortcuts to folders, broken shortcuts...)
Ticket 19: toolbar on the left/right side no longer covers the Windows taskbar
Ticket 22: drag files or folders on an empty area of the toolbar to create a shortcut in the nearest folder section
	New per-folder option "Align at the end of the toolbar" to place a folder section at the bottom/right of the toolbar
Ticket 24: the right click menu of the icons opens on the same monitor of the toolbar
Ticket 26: the toolbar stays always on top also on Windows 10/11
Changing language in the settings window now recreates the window in the new language (keeping the values not yet saved)
Fixed: an error in a folder prevented the following folders from being loaded
Version 1.1.0.0
=============================
Project converted from Visual Studio 2012 / .NET 3.5 to SDK format / .NET Framework 4.8 (build: dotnet build -c Release)
New settings to choose font and color of the toolbar texts
The toolbar is redrawn automatically when resolution, zoom (DPI) or monitor configuration change
Extra items that do not fit in the toolbar:
	"Pop-out list": the extra items are shown in an ordered grid, as wide as the toolbar (with a configurable number of columns) and scrollable if too long
	"Scroll the toolbar": the toolbar itself scrolls with the mouse wheel or with the arrow buttons; when the toolbar auto-hides the scroll goes back to the top
Fixed crash (timeout 0x800705B4) opening shortcuts to subfolders of network paths (mapped drives or \\server\share)
The toolbar is no longer hidden by "Show desktop" (Win+D)
The toolbar no longer appears in the Alt+Tab list
Fixed English layout of the settings window ("Auto hide after" checkbox under the delay field)
Folder watchers are released when the toolbar is refreshed
Version 1.0.6.6
=============================
Fixed ticket 13: "The file does not exist" regression
	Now file (shortcut or executable) will be always executed. If an error occurs a warning message will be showed
Fixed error if a reorder button in settings window was clicked without any selected folder

Version 1.0.6.5
=============================
Fixed ticket 10: A bad shortcut will cause SevenDeskBar to crash
	Now if a file is not valid or not executable or invalid link's target (Ex. moved or deleted) a warning message will be showed
Fixed ticket 12: Respect hidden files.
	New setting to show hidden files, by default the hidden files will not be showed

Version 1.0.6.1
=============================
Fixed strange behaviour when user enable/deactivate use of Aero interface or when change toolbar position with Aero interface enabled

Version 1.0.6.0
=============================
Add option to enable Aero style interface (only Windows Vista and Seven)

Version 1.0.5.1
=============================
Added functionality to open toolbar's folder with doubleclick on the folder name (if showed)

Version 1.0.5.0
=============================
Added German translation
Added entries in the right click menu to toggle the display of the file name, display the folder name as the title of the toolbar and activate the "Auto Hide" functionality
Added functionality to rearrange the icons in the toolbar.
How to rearrange the icons:
	Select the item in the right click menu "Rearrangment mode"
	Drag the icons by holding down the ALT key.
	After the adjustment of the icons re-select the menu item right click to save the arrangement of icons.
	Be careful though you can also move the icons in the toolbar of the other sections of the folders will still be sorting out between the files in the folder at the usual next startup.
	To delete the data of this sort is a special button on the form settings.
If the order is not set or the data sorting are deleted icons will be arranged in the style of the toolbar of Windows XP: folders first and then the files, both in alphabetical order
====================
			Italiano
====================
Aggiunta traduzione in tedesco
Aggiunte voci nel menù del tasto destro per attivare la visualizzazione del nome del file, mostrare il nome della cartella come titolo della toolbar e attivare la funzionalità di "nascondi automaticamente"
Aggiunta funzionalità per riordinare le icone nella toolbar.
Come riordinare le icone:
	Selezionare la voce nel menù del tasto destro "Modalità ordinamento"
	Trascinare le icone tenendo premuto il tasto ALT.
	Terminato il riordinamento delle icone selezionare nuovamento la voce del menù del tasto destro per salvare l'ordinamento delle icone.
	Attenzione sebbene sia possibile spostare le icone anche nelle sezioni delle toolbar delle altre cartelle l'ordinamento verrà comunque effettuato tra i file appartenenti alla solita cartella al successivo avvio dell'applicazione.
	Per eliminare i dati di ordinamento è presente un apposito pulsante nella maschera delle impostazioni.
Se l'ordinamento non è impostato o i dati d'ordinamento vengono eliminati le icone verranno ordinate come su Windows XP: prima le cartelle e poi i file entrambi seguento l'ordine alfabetico
 
Version 1.0.2.0
=============================
Add Spanish translation
Add show text option. Now user can choose if show only icon or icon and text on toolbar

Version 1.0.1.5
=============================
Add auto hide functionality

Version 1.0.1.0
=============================
Fixed
Language change on the fly, now the new language is correctly applied on all element (controls and context menu)
Added property to set toolbar size.
Added property for choose icon's dimension (Large or small)

Release 1.0.0.5
=============================
New right click menu to refresh toolbar.
Change toolbar position now resize and reorder toolstrip button correctly
Change language on the fly for form and controls, for right click menu not yet implemented
File copy and move work on folder's link works according to old  Windows toolbar function
Application setting migration from previous installation
Add possibility to watch folders included, each change will immediately replicated in toolbars
Drag and drop files/folders over folder's link now will show menu for  copy or move file

Release 1.0.0.2
=============================
Right click over element: now show's default shell context menu
Basic implementation of File copy and move when a file is dropped on a folder element
English translation

Release 1.0.0.0
=============================
First release