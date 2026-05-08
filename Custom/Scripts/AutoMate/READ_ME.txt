This plugin suite allows you to automatically load both session plugins and Person atom plugins.

For example (and this is the default configuration):
You can have all Male Person atoms automatically load with ImprovedPoV and Explosion Limiter plugins applied to him.
You can have all Female Person atoms automtically load with Spankings, Kiss, and Explosion Limiter plugins applied to her.
You can have all scenes with Only One Female Person Atom additionally load with E-Motion and Easy Moan plugins applied to her.


1) Choose what session plugins you want to load (optional, only if you are an advanced user and often add session plugins to VAM)

Choose Session Plugins you want to load by manually editing the list at the top of AutoMate/Load_Session_Plugins.cs 

By default it loads:
a)  Auto_Load_Person_Plugins.cslist - This automatically loads selected plugins on Person atoms.
b)  A modified clock plugin, which shows both your VAM version and the time in the main VAM UI.


2) Add the ADD_ME_TO_ATOM_IN_DEFAULT_JSON_SCENE.cslist to any atom in your default.json scene

This will make session plugins, in addition to the required Auto_Load_Person_Plugin, automatically load each time you open VAM.

If you don't yet have a default.json in Saves/scene, copy the Default.json scene
from: Scripts/AutoMate/VaM Default Scene/Default.json
to: your VaM/Saves/scene folder.


3) To change which plugins are automatically loaded on Person atoms:

THROUGH THE UI:
a) Open the VAM Menu and check Edit Mode. Press the left most "Open Main UI" button and press the Session Plugins tab.

b) Press Open Custom UI... in the Auto_Load_Person_Plugins area to open the Auto Load Person Plugins interface.

c) Make sure there is at least one person in your scene, and no more than one person of each gender (at most 1 male, 1 female in the scene).

d) Add whatever plugins you want to the female and/or male characters, then press the buttons like "Scan Current Female Settings & Save".
This saves the current set of loaded plugins on that person atom to be the default for all Female (or Male) person atoms.

e) You can load separate plugins for females depending on if they are alone in the scene, or there are multiple person atoms of any gender.
Press Scan Current Fem Solo Settings & Save to set the current set of plugins on the female person to be the default loaded if she is alone in the scene.

MANUALLY:
If you prefer to edit the source code and manually specify paths for the plugins:

a) Open the AutoMate\SESSION_PLUGINS\src\Auto_Load_Person_Plugins.cs file

b) At the top in SetDefaultSettings() enter the absolute paths of the plugins you want to load.

c) Follow the "THROUGH THE UI" steps to open the Auto_Load_Person_Plugins UI..., then press Reset to Default & Save SETTINGS.json.
This implements what you set in code as the default to load.
