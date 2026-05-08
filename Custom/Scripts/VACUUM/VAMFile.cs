using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Xsl;
using Morphology.Data;
using Newtonsoft.Json.Linq;
using Path = System.IO.Path;
using System.IO.Packaging;
using System.IO.Compression;
using System.Diagnostics;


//0.9
//Initial Release

//0.9.2
//Protect folders with /Protected/ in the path 
//Single copy only of protected files/folders in the log file (had multiple copies)
//Easy Clean asks if you want to delete unused files from Saves
//VACUUM explains deletion of screenshots from saves.
//Rename testes height morph in VAM 1.19 only, as VAM 1.19 breaks the name of this morph.
//VAM allows for a scene to have a thumbnail with the same name but different case, such as default.json and Default.jpg. VACUUM now (for example) doesn't delete Default.jpg if it finds a default.json scene.
//Fixed crash if you cleaned again after finishing cleaning (without restarting VACUUM).
//Log, but don't alert, if some path can't be access when iterating through files.
//Log files with paths that are too long
//Log files that include \Custom\ in their paths but are in the Saves folder (these are probably meant to be moved to Custom, but we let the user handle that manually if they wish.

//0.9.3
//Added additional warning text, and display path to potential files to delete, when deleting unused assets (not duplicates, but single unused assets such as assetbundles or audio files that are not in any scene)
//Fixed crash if you chose to not use Easy Clean, then chose to analyze source files, then chose to not allow source files to be changed.
//Fixed crash if you cleaned again, without analyzing source files on the first clean, after finishing cleaning (without restarting VACUUM).


//0.9.4 (UNRELEASED)
//Added scanning for missing and unused Morphs
//Fixed issue where scene files wouldn't be changed to point to files in VAR resources, even if the regular files were missing, unless VACUUM had just deleted the resources as dupliate files.
//Fixed issue where VACUUM crashed if it processed malformed JSON clothing or hair code.
//Add deletion of unused images from Clothing and Hair folders.
//Fixed issue where clothing and hair from VAR files was considered missing if used in scenes (scenes would show clothing and hair from var files as missing clothing and hair).
//Fixed issue where links to VAR file clothing and hair wouldn't be used to fix legacy scenes missing those clothing and hair.
//Fixed issue where VACUUM formatted paths to files within VAR packages were not being understood by VaM. Now formatting of paths to new VAR files does not include AddonPackages or the .var extension. 
//Fixed issue with AlphakiniPantySim, where sim versions were missing textures and styles. In VaM Alphakini is non-standard and uses AlphakiniPantyStyle instead of AlphakiniPantySimStyle.
//Removed writing to log files when VACUUM decides not to catalog an item. It was cluttering the log and was more for development purposes.


//BEFORE RELEASING THIS VERSION MOVE THE DIALOG PROMPT FOR REPLACING SIM CLOTHES RIGHT AFTER THE EASY MATE PROMPT, IF EASY MATE IS CHOSEN???

//CURRENTLY MOST UNUSED AND USED CLOTHING ARE BASED ON IF THEY ARE USED BY PRESETS, NOT USED BY SCENES, FIX THIS!


//PERHAPS FIX THIS OR STOP REPLACING SIM CLOTHES AUTOMATICALLY IN EASY CLEAN?
//WHAT IF SOMETHING HAPPENS TO HAVE SIM SIM IN IT AND IT SHOULD BE, ETC.
/*updatedLine = updatedLine.Replace("SimSimSimSimSimSim", "SimSim");
updatedLine = updatedLine.Replace("SimSimSimSimSim", "SimSim");
updatedLine = updatedLine.Replace("SimSimSimSim", "SimSim");
updatedLine = updatedLine.Replace("SimSimSim", "SimSim");
updatedLine = updatedLine.Replace("SimSimStyle", "SimStyle");
updatedLine = updatedLine.Replace("SimSimMaterial", "SimMaterial");
updatedLine = updatedLine.Replace("SimSimWrapControl", "SimWrapControl");
updatedLine = updatedLine.Replace("SimSimMain", "SimMain");
updatedLine = updatedLine.Replace("Sim Sim", "Sim");*/


//POSSIBLE IMPROVEMENTS
//REPLACE THE FILE SELECTION DIALOG BOX WITH A BETTER VERSION WHERE WE CAN PASTE PATHS.

//IF A SCENE IS MISSING A THUMBNAIL, BUT THERE IS A SIMILARLY NAMED (WHAT DOES THAT MEAN) THUMBNAIL IN THE SAME FOLDER, COPY THAT
//OTHER THUMBNAIL AND RENAME THE COPY TO MATCH THE SCENE, SO WE GET A THUMBNAIL

//ALSO MAKE A LIST OF CLOTHING AND SCENES WITH MISSING THUMBNAIL, AND WHAT TO DO ABOUT IT?  

//HS_KITCHEN HAS TEXTURES WITH LONG FILE NAMES, THE COOKING UTENSILS OVER THE STOVE PNG FILE HAS A LONG NAME AND DOESN'T LOAD
//TEXTURES WITH VERY LONG PATH NAMES DON'T LOAD
//COULD RENAME IMAGE FILES IF OVER A CERTAIN LENGTH IN FILE NAME OR ABSOLUTE PATH, PERHAPS CROP THE IMAGE NAME DOWN
//AND CHECK TO MAKE SURE THE CROPPED VERSION DOESN'T MATCH ANY OTHER KNOWN FILE NAME
namespace geesp0t.VACUUM
{
    class VAMFile
    {
        //REALLY THIS SHOULD BE TWO CLASSES, ONE MANAGER CLASS, AND THEN THE SUBCLASS
        //IF I HAVE TIME I'LL SEPARATE IT OUT AT SOME POINT

        public static MainWindow mainWindow = null;

        public static string VAM_FOLDER = "";
        public static string CUSTOM_FOLDER = "";
        public static string SAVES_FOLDER = "";
        public static string SCRIPTS_FOLDER = "";
        public static string PACKAGES_FOLDER = "";
        public static string LOG_FOLDER = "";
        public static string LOG_FOLDER_LAST = "";
        public static string MORPHS_FOLDER = "";

        public const int PATH_LENGTH_TOO_LONG = 230;
        public const string IMAGE_TYPE = "Image";
        public const string AUDIO_TYPE = "Audio";
        public const string ASSETBUNDLE_TYPE = "Assetbundle";
        public const string VAM_TYPE = "VAM";
        public const string VAM_STYLE_TYPE = "VAM Style";
        public const string SCRIPT_TYPE = "Script";
        public const string VAC_TYPE = "VAC";

        public const string MORPH_EXTENSION = "vmi";
        public const string MORPH_BINARY_EXTENSIONS = "vmb";
        
        private static bool readyToDeepClean = false;
        public static bool easyClean = false;

        private static Stopwatch stopWatch = new Stopwatch();
        private static bool stopWatchRunning = false;
        private static bool working = false;

        private static bool isVAM19 = false;


        public class ZippedFile
        {
            public string data = "";
            public long length = 0;
        }

        public static List<string> varFilePaths = new List<string>();
        public static Dictionary<string, Dictionary<string, ZippedFile>> varFileContents = new Dictionary<string, Dictionary<string, ZippedFile>>();
        public static string varFileLog = "";
        public static Dictionary<string, string> legacyPathToFirstVARReference = new Dictionary<string, string>();
        public static Dictionary<string, string> findVARAbsolutePath = new Dictionary<string, string>();

        public static List<VAMFile> vamFileList = new List<VAMFile>();
        public static List<string> vamFileListPaths = new List<string>();
        public static List<string> resourceFilePaths = new List<string>();

        public static List<string> builtInMorphs = new List<string>();
        public static Dictionary<string, List<string>> morphIDToFiles = new Dictionary<string, List<string>>();
        public static List<string> existingMorphPaths = new List<string>();
        public static List<string> existingMorphs = new List<string>();
        public static List<string> desiredMorphs = new List<string>();
        public static Dictionary<string, List<string>> filesReferenceMorph = new Dictionary<string, List<string>>();

        public static List<string> moveToCustomFolders = new List<string>();
        public static List<string> longPathFiles = new List<string>();

        public static List<string> neverDeleteFiles = new List<string>(); //these are files that if found anywhere shouldn't be deleted
        public static List<string> neverDeleteFilePaths = new List<string>(); //absolute paths of files not to delete
        public static List<string> neverDeleteFolders = new List<string>(); //files in these folders shouldn't be deleted
        public static List<string> neverDeleteExtensions = new List<string>(); //files with these extensions shouldn't be deleted
        public static List<string> filesInProtectedFolders = new List<string>();

        public static Dictionary<string, List<string>> assetTypes = new Dictionary<string, List<string>>();
        public static List<string> assetExtensions = new List<string>();
        public static List<string> duplicateAssetTypesToCheck = new List<string>();
        public static List<string> duplicateAssetExtensionsToCheck = new List<string>();
        public static List<VAMFile> stylesMissingClothingHairVAM = new List<VAMFile>();
        public static Dictionary<string, string> replaceClothingIDs = new Dictionary<string, string>(); //for replacing regular clothes with simclothes

        public static List<string> assetFolders = new List<string>();
        public static List<string> savefileExtensions = new List<string>();
        public static List<string> saveFolders = new List<string>();

        public static Dictionary<string, List<VAMFile>> filesOfType = new Dictionary<string, List<VAMFile>>();

        public static List<string> sourceFiles = new List<string>();
        public static List<string> sourceFilesInCustom = new List<string>();
        public static List<string> sourceFilesInSaves = new List<string>();
        public static Dictionary<string, List<string>> savefileAssets = new Dictionary<string, List<string>>();
        public static List<string> requiredAssets = new List<string>();
        public static List<string> requiredUIDs = new List<string>();
        public static Dictionary<string, List<string>> requiredUIDFiles = new Dictionary<string, List<string>>();
        public static Dictionary<string, List<string>> requiredUIDFilesFromPath = new Dictionary<string, List<string>>();
        public static List<string> builtinUIDs = new List<string>();
        public static List<string> assetUIDs = new List<string>();

        public static Dictionary<string, string> oldToNewAssets = new Dictionary<string, string>();
        public static Dictionary<string, string> oldToNewAssetsIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public static List<string> duplicateNameFilesToDelete = new List<string>(); //only delete these if they are not used in scene files
        public static List<string> duplicatePathFilesToDelete = new List<string>(); //only delete these if they are not used in scene files

        public static Dictionary<string, string> morphReplace = new Dictionary<string, string>();
        public static Dictionary<string, string> morphReplace19 = new Dictionary<string, string>();

        public static List<string> thumbnailAssets = new List<string>();

        public static string static_error_log = ""; //check this at the end

        private static bool reviewEachChange = true;

        private static bool safeDeletePrompt = true;

        private static string changeLog = "";

        private static string vacLog = "";

        private static long totalDeletedFileSize = 0;

        //easy clean is not allowed to re-optimize the same thing twice
        private static bool easyCleanOptimizedVAC = false;
        private static bool easyCleanOptimizedVAR = false;
        private static bool easyCleanOptimizedIDS = false;

        private static bool cleanOneScene = false;


        bool in_var = false;
        string legacy_path_equivalent_from_var = "";
        ZippedFile zipped_file = null;
        string path_to_var = "";
        string path_in_var = "";
        string path = "";
        string folder = "";
        string file_name = "";
        string file_name_without_extension = "";
        string extension = "";
        string extension_category = "";
        bool organized_by_id = false;
        string uid = "";
        bool uses_other_id = false;
        string other_id = "";
        string other_id_file = "";
        bool in_custom_folder = false;
        bool in_saves_folder = false;
        bool in_scripts_folder = false;
        bool in_assets_folder = false;
        bool in_person_textures_folder = false;
        bool in_male_clothing_folder = false;
        bool in_female_clothing_folder = false;
        bool in_male_hair_folder = false;
        bool in_female_hair_folder = false;
        bool is_clothing_hair = false;
        bool is_clothing_hair_style = false;
        bool is_image = false;
        bool is_audio = false;
        bool is_assetbundle = false;
        bool is_script = false;
        bool is_vac = false;
        bool is_morph = false;
        bool is_never_delete_file = false; //mark true if in never delete folder
        bool is_in_never_delete_folder = false;
        string error_log = ""; //check this at the end!
        bool error = false; //don't process files with errors

        //usage when calling Add for a file in a VAR   Add(myvar_withoutextension:/[relativevarpath]); //e.g. NoStage3.Hair_Long_Wavy_1_Bangs.1:/Custom/Hair/Female/NoStage3/Long Wavy 1 Bangs/Long Wavy 1.vam
        public void Add(string absolute_path, ZippedFile zipped_file_data = null)
        {
            zipped_file = zipped_file_data;

            if (absolute_path.Contains(":/"))
            {
                in_var = true;
                string[] path_sections = absolute_path.Split(new[] { ":/" }, StringSplitOptions.None);
                if (path_sections.Length != 2)
                {
                    error = true;
                    error_log += "\nError splitting path for VAR package: " + absolute_path;
                }
                else
                {
                    path_to_var = path_sections[0]; //the absolute path to the var file itself
                    path_in_var = path_sections[1];
                    legacy_path_equivalent_from_var = MakeVaMAbsolutePath(VAM_FOLDER, "", path_in_var);
                    path = GetCorrectedLinkToFileInVarPackage(absolute_path); //files in var packages are always relative
                }
            }
            else
            {
                path = absolute_path;
            }

            vamFileListPaths.Add(path);

            //when we decide if it's a script, etc., check the converted absolute legacy path
            if (in_var)
            {
                absolute_path = legacy_path_equivalent_from_var;
            }

            file_name = Path.GetFileName(absolute_path);
            folder = Path.GetDirectoryName(absolute_path);
            file_name_without_extension = Path.GetFileNameWithoutExtension(absolute_path);
            extension = GetFileExtension(absolute_path);
            
            //where am I?
            if (absolute_path.StartsWith(CUSTOM_FOLDER + "\\"))
            {
                in_custom_folder = true;
                if (absolute_path.StartsWith(SCRIPTS_FOLDER + "\\") || absolute_path.Contains("\\Scripts\\"))
                {
                    //add all scripts folders to never delete
                    neverDeleteFolders.Add(Path.GetDirectoryName(absolute_path));
                    in_scripts_folder = true;
                }
                else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Assets\\"))
                {
                    in_assets_folder = true;
                }
                else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Atom\\Person\\Textures\\"))
                {
                    in_person_textures_folder = true;
                }
            }
            else if (absolute_path.StartsWith(SAVES_FOLDER + "\\"))
            {
                in_saves_folder = true;
            }
            else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Clothing\\Male\\"))
            {
                in_male_clothing_folder = true;
            }
            else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Clothing\\Female\\"))
            {
                in_female_clothing_folder = true;
            }
            else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Hair\\Male\\"))
            {
                in_male_hair_folder = true;
            }
            else if (absolute_path.StartsWith(VAM_FOLDER + "\\Custom\\Hair\\Female\\"))
            {
                in_female_hair_folder = true;
            }

            if (!in_var)
            {
                //protected files and folders
                if (absolute_path.Contains("\\Protected\\"))
                {
                    neverDeleteFolders.Add(Path.GetDirectoryName(absolute_path));
                }

                if (absolute_path.Length >= PATH_LENGTH_TOO_LONG)
                {
                    longPathFiles.Add(absolute_path);
                }

                //never delete?
                foreach (string neverDeleteFileName in neverDeleteFiles)
                {
                    if (absolute_path.EndsWith(neverDeleteFileName))
                    {
                        is_never_delete_file = true;
                        neverDeleteFilePaths.Add(absolute_path);
                    }
                }
                foreach (string neverDeleteExtension in neverDeleteExtensions)
                {
                    if (absolute_path.EndsWith("." + neverDeleteExtension))
                    {
                        is_never_delete_file = true;
                        neverDeleteFilePaths.Add(absolute_path);
                    }
                }

                //assets in certain folders aren't checked for duplicates, they should be kept
                foreach (string neverDeleteFolder in neverDeleteFolders)
                {
                    if (absolute_path.StartsWith(neverDeleteFolder))
                    {
                        is_in_never_delete_folder = true;
                    }
                }

                if (is_never_delete_file)
                {
                    filesInProtectedFolders.Add(absolute_path);
                }
            }

            //what am I?
            
            if (extension == MORPH_EXTENSION) is_morph = true;

            foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
            {
                foreach (string categorizedExtension in assetType.Value)
                {
                    if (extension == categorizedExtension)
                    {
                        extension_category = assetType.Key;
                        filesOfType[extension_category].Add(this);

                        switch (extension_category)
                        {
                            case IMAGE_TYPE:
                                resourceFilePaths.Add(path);
                                is_image = true;
                                break;
                            case AUDIO_TYPE:
                                resourceFilePaths.Add(path);
                                is_audio = true;
                                break;
                            case ASSETBUNDLE_TYPE:
                                resourceFilePaths.Add(path);
                                is_assetbundle = true;
                                break;
                            case VAM_TYPE:
                                is_clothing_hair = true;
                                break;
                            case VAM_STYLE_TYPE:
                                is_clothing_hair_style = true;
                                break;
                            case SCRIPT_TYPE:
                                is_script = true;
                                break;
                            case VAC_TYPE:
                                is_vac = true;
                                break;
                            default:
                                error_log += "\nCouldn't categorize file extension: " + path;
                                break;
                        }
                    }
                }
            }

            if (is_clothing_hair)
            {
                uid = GetVAMFileUID(this);
                if (uid == "")
                {
                    error_log += "\nCouldn't find UID in file: " + path;
                }
                else
                {
                    organized_by_id = true;
                }
            }
        }

        public static void ResetEverything()
        {
            readyToDeepClean = false;
            easyClean = false;

            stopWatch = new Stopwatch();
            stopWatchRunning = false;
            working = false;

            isVAM19 = false;
            varFilePaths = new List<string>();
            varFileContents = new Dictionary<string, Dictionary<string, ZippedFile>>();
            varFileLog = "";
            legacyPathToFirstVARReference = new Dictionary<string, string>();

            vamFileList = new List<VAMFile>();
            vamFileListPaths = new List<string>();
            resourceFilePaths = new List<string>();

            moveToCustomFolders = new List<string>();
            longPathFiles = new List<string>();

            neverDeleteFiles = new List<string>(); //these are files that if found anywhere shouldn't be deleted
            neverDeleteFilePaths = new List<string>(); //absolute paths of files not to delete
            neverDeleteFolders = new List<string>(); //files in these folders shouldn't be deleted
            neverDeleteExtensions = new List<string>(); //files with these extensions shouldn't be deleted
            filesInProtectedFolders = new List<string>();

            assetTypes = new Dictionary<string, List<string>>();
            assetExtensions = new List<string>();
            duplicateAssetTypesToCheck = new List<string>();
            duplicateAssetExtensionsToCheck = new List<string>();
            stylesMissingClothingHairVAM = new List<VAMFile>();
            replaceClothingIDs = new Dictionary<string, string>(); //for replacing regular clothes with simclothes

            assetFolders = new List<string>();
            savefileExtensions = new List<string>();
            saveFolders = new List<string>();

            filesOfType = new Dictionary<string, List<VAMFile>>();

            sourceFiles = new List<string>();
            sourceFilesInCustom = new List<string>();
            sourceFilesInSaves = new List<string>();
            savefileAssets = new Dictionary<string, List<string>>();
            requiredAssets = new List<string>();
            requiredUIDs = new List<string>();
            requiredUIDFiles = new Dictionary<string, List<string>>();
            requiredUIDFilesFromPath = new Dictionary<string, List<string>>();
            builtinUIDs = new List<string>();
            assetUIDs = new List<string>();

            oldToNewAssets = new Dictionary<string, string>();
            oldToNewAssetsIgnoreCase = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            duplicateNameFilesToDelete = new List<string>(); //only delete these if they are not used in scene files
            duplicatePathFilesToDelete = new List<string>(); //only delete these if they are not used in scene files

            morphReplace = new Dictionary<string, string>();
            morphReplace19 = new Dictionary<string, string>();

            thumbnailAssets = new List<string>();

            static_error_log = ""; //check this at the end

            reviewEachChange = true;

            safeDeletePrompt = true;

            changeLog = "";

            vacLog = "";

            totalDeletedFileSize = 0;

            //easy clean is not allowed to re-optimize the same thing twice
            easyCleanOptimizedVAC = false;
            easyCleanOptimizedVAR = false;
            easyCleanOptimizedIDS = false;

            cleanOneScene = false;
        }

        private static void BuildFileLists()
        {
            SetWindowTitle("VACUUM - Scanning VAM Folder and Categorizing VAM Files");

            BuildSourceFilesList();
            BuildvamFileListList();

            ResetWindowTitle();
        }

        private static void SetWindowTitle(string title)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                mainWindow.Title = "VACUUM - " + title;
            });
        }

        private static void ResetWindowTitle()
        {
            SetWindowTitle(VAM_FOLDER);
        }

        private static void BuildSourceFilesList()
        {
            cleanOneScene = false;
            changeLog = "";
            totalDeletedFileSize = 0;
            sourceFilesInCustom.Clear();
            sourceFilesInSaves.Clear();
            sourceFiles.Clear();
            moveToCustomFolders.Clear();
            longPathFiles.Clear();
            desiredMorphs.Clear();
            existingMorphs.Clear();
            existingMorphPaths.Clear();
            morphIDToFiles.Clear();
            filesReferenceMorph.Clear();


            filesOfType.Clear();
            foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
            {
                filesOfType.Add(assetType.Key, new List<VAMFile>());
            }

            if (easyClean || MessageBox.Show("IF YOU PRESS \"Yes\" to Easy Clean, VACUUM WILL NOT PROMPT YOU BEFORE DELETING FILES!" +
                "\n\nPress \"No\" to manually choose what to clean / remove / change." +
                "\n\n\nIf you press \"Yes\" EASY CLEAN WILL:" +
                "\n\nDelete old legacy files from Custom (if you have new VAR Packages containing files at the same virtual paths)." +
                "\n\nExtract all VAC files, then delete them. VAC files are extracted each time you open a VAC scene in VAM. VAC Files are not needed once they are extracted, are slightly slower to load, and take up a lot of extra space." +
                "\n\nDelete Clothing / Hair files with duplicate UIDs." +
                "\n\nReplace JSON save file references to old Custom files, so they reference VAR Package files instead." +
                "\n\nReplace JSON file references to missing resources, so they reference existing resources with the same name if found." +
                "\n\nDelete Resources in your Saves folder which are not used in any Save file (optional, you will be prompted at the end)." +
                "\n\nDelete Duplicates of resources, replacing references in JSON save files to point to the same single copy of each resource." +
                "\n\nReplace Regular Clothes with Identical Sim Clothes (where available) to allow you to touch and move clothes using Clothing -> Undress All." +
                "\n\n\nEASY CLEAN WILL NOT:\n(you have to manual clean to do these things)" +
                "\nDelete Unused Morphs." +
                "\nDelete Unused Resources (Images, Audio) which are not used in any scenes." +
                "\nDelete Unused Assetbundles (which are often unused if you don't add CustomUnityAssets to your scenes)." +
                "\nDelete Unused Clothing." +
                "\nDelete Unused Hair." +
                "\n\n\nWould you like to Easy Clean your VAM folder?\n(Easy Clean may take an hour or more to complete)"
                , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                easyClean = true;
                easyCleanOptimizedVAC = false;
                easyCleanOptimizedVAR = false;
                easyCleanOptimizedIDS = false;
            }

            List<string> tempList;
            foreach (string fileExtension in savefileExtensions)
            {
                SetWindowTitle("Building list of " + fileExtension + " files");

                tempList = GetVAMFilePaths(CUSTOM_FOLDER, fileExtension);
                sourceFilesInCustom.AddRange(tempList);
                sourceFiles.AddRange(tempList);

                tempList = GetVAMFilePaths(SAVES_FOLDER, fileExtension);
                sourceFilesInSaves.AddRange(tempList);
                sourceFiles.AddRange(tempList);
            }
        }

        private static void BuildvamFileListList()
        {
            resourceFilePaths.Clear();
            vamFileListPaths.Clear();
            vamFileList.Clear();
            varFilePaths.Clear();
            varFileContents.Clear();
            filesInProtectedFolders.Clear();
            legacyPathToFirstVARReference.Clear();
            findVARAbsolutePath.Clear();
            thumbnailAssets.Clear();

            //SCAN VAR FILES, ADD CONTENTS INSIDE TO vamFileList.
            //ADD THESE FIRST, SO THEY ARE SIMPLY FOUND FIRST WHEN LOOKING FOR DUPLIATES
            string varFilePathLog = "";
            if (Directory.Exists(PACKAGES_FOLDER))
            {
                varFilePaths.AddRange(GetVAMFilePaths(PACKAGES_FOLDER, "var"));
            }
            varFileLog = "";
            int curFileNumber = 0;
            int totalFileNumber = varFilePaths.Count;
            foreach (string varFilePath in varFilePaths)
            {
                curFileNumber++;
                SetWindowTitle("(" + curFileNumber + "/" + totalFileNumber + ") Processing VAR Package - " + varFilePath);

                string relativePathInVAM = GetCorrectedLinkToVarPackage(varFilePath);

                if (findVARAbsolutePath.ContainsKey(relativePathInVAM))
                {
                    static_error_log += "\nYOU HAVE TWO VAR PACKAGES WITH THE SAME NAME (" + relativePathInVAM + ") IN DIFFERENT FOLDERS WITHIN AddonPackages, THIS IS NOT ALLOWED AND WILL CAUSE ERRORS.";
                    varFilePathLog += "\n\nYOU HAVE TWO VAR PACKAGES WITH THE SAME NAME (" + relativePathInVAM + ") IN DIFFERENT FOLDERS WITHIN AddonPackages, THIS IS NOT ALLOWED AND WILL CAUSE ERRORS. " + varFilePath + " is a duplicate.\n\n";
                } else
                {
                    findVARAbsolutePath.Add(relativePathInVAM, varFilePath);
                }

                varFileLog += "\n\nVAR PACKAGE: " + varFilePath + "\n" + Path.GetFileName(varFilePath) + "VaM Internal Path: " + varFilePath + "\n" + Path.GetFileName(varFilePath) + " Contents:\n";

                varFileContents.Add(varFilePath, ReadZipFileContents(varFilePath));

                foreach (KeyValuePair<string, ZippedFile> zippedFile in varFileContents[varFilePath])
                {
                    VAMFile newVAMFile = new VAMFile();
                    newVAMFile.Add(varFilePath + ":/" + zippedFile.Key, zippedFile.Value); //sets newVAMFile.path to the corrected VaM readable path (only relative paths are ok for vam var files)
                    vamFileList.Add(newVAMFile);
                    if (!legacyPathToFirstVARReference.ContainsKey(newVAMFile.legacy_path_equivalent_from_var))
                    {
                        legacyPathToFirstVARReference.Add(newVAMFile.legacy_path_equivalent_from_var, newVAMFile.path);
                    }

                    //if the file is a morph, then process it
                }
            }


            //log contents of all packages
            SetWindowTitle("Writing log files");
            foreach (KeyValuePair<string, string> varFilePathLogEntry in findVARAbsolutePath)
            {
                varFilePathLog += "VAR PACKAGE " + varFilePathLogEntry.Value + " is referenced in VAM as: " + varFilePathLogEntry.Key + "\n";
            }
            WriteLogFile("AddonPackage VAR file absolute and relative paths.txt", varFilePathLog);
            WriteLogFile("All_VAR_Package_Contents.txt", varFileLog);


            //ALL OTHER FILES NOT IN VAR
            foreach (string folder in assetFolders)
            {
                foreach (string fileExtension in assetExtensions)
                {
                    SetWindowTitle("Finding files of type " + fileExtension + " in folder " + folder);
                    List<string> newFiles = GetVAMFilePaths(folder, fileExtension);
                    foreach (string newFilePath in newFiles)
                    {
                        VAMFile newVAMFile = new VAMFile();
                        newVAMFile.Add(newFilePath);
                        vamFileList.Add(newVAMFile);
                    }
                }
            }

            //VAC FILES ARE ZIP FILES. IN THE ZIP FILE, THE FIRST json FILE FOUND IS OPENED
            //VAC FILES EXTRACT AND OVERWRITE ALL EXISTING FILES WITH MATCHING PATHS IN A FOLDER EACH TIME THEY ARE OPENED
            //IF THERE ARE EXISTING FILES THAT DON'T MATCH VAC FILE PATHS, THE EXISTING FILES AREN'T DELETED
            //IN A SINGLE COMPARISON WITH THE SAME VAC FILE, OPENING THE VAC TOOK 19 SECONDS EACH TIME, WHEREAS OPENING THE RESULTING JSON
            //FILE DIRECTLY TOOK 17 SECONDS. SO IT'S ALSO FASTER TO AVOID VAC FILES
            if (filesOfType[VAC_TYPE].Count > 0)
            {
                //calculate current size of VAC files, that is how much space we will save
                long totalVACSize = 0;
                List<string> vacFileList = new List<string>();
                foreach (VAMFile vamFile in filesOfType[VAC_TYPE])
                {
                    vacFileList.Add(vamFile.path);
                    if (File.Exists(vamFile.path))
                    {
                        FileInfo fi = new FileInfo(vamFile.path);
                        totalVACSize += fi.Length;
                    }
                }
                vacFileList.Sort();
                WriteLogFile("All_" + VAC_TYPE + "_Files.txt", string.Join("\n", vacFileList.ToArray()));

                if (easyClean && easyCleanOptimizedVAC)
                {
                    AppendDeletedLog("Easy Clean Already Extracted and Deleted VAC Files, Skipping Second Optimization.");
                }
                else
                {
                    if (easyClean || MessageBox.Show("Would you like to extract and delete all VAC files from your Saves folder?" +
                        "\n\n\nYou could save " + GetReadableFileSize(totalVACSize) + "." +
                        "\n\nMany scene files are VAC files. These are zipped scene packages." +
                        "\n\nEach time you click a VAC file to open a scene, VAM unzips the VAC file to a folder with the same name as the VAC file, creating a duplicate of all files in the VAC (or overwriting the existing files if present)." +
                        "\n\nThis slow loading time (due to the time it takes to unzip the vac) and leaves you with duplicate copies of all Saves/scene files, one in the VAC package and the other extracted version." +
                        "\n\nTo save space, and slightly speed up loading scenes, VACUUM can extract, then delete, all VAC files." +
                        "\n\nThis will also let VACUUM fix missing references and other issues in the extracted scene files." +
                        "\n\nTo open scenes which were VAC files, instead of clicking the .vac file, open the folder with the VAC name, then click the thumbnail with the vac name." +
                        "\n\nFor example if you were opening a VAC file in scene/MyFavoriteScene.vac, you would now open scene/MyFavoriteScene/myFavoriteScene.json" +
                        "\n\nYou can see the list of all VAC files in " + LOG_FOLDER + "\\All_" + VAC_TYPE + "_Files.txt" + "."
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        vacLog = "";
                        int curFileNum = 0;
                        int totalFileNum = filesOfType[VAC_TYPE].Count;
                        List<string> extractedVACFiles = new List<string>();
                        foreach (VAMFile vamFile in filesOfType[VAC_TYPE])
                        {
                            curFileNum++;
                            if (vamFile.path.StartsWith(SAVES_FOLDER))
                            {
                                SetWindowTitle("(" + curFileNum + "/" + totalFileNum + ") Extracting VAC File - " + vamFile.path);

                                string vacFolder = Path.GetDirectoryName(vamFile.path) + "\\";  // scene\\
                                string vacNameWithoutExtension = Path.GetFileNameWithoutExtension(vamFile.path); // example
                                string newFolderWithoutFinalBackslash = vacFolder + vacNameWithoutExtension; // scene\\example
                                string newFolder = newFolderWithoutFinalBackslash + "\\"; // scene\\example

                                vacLog += "\n\nExtracting: " + vamFile.path + "\nTo: " + newFolder;

                                //it's possible there is a file with the same name as the folder, so the folder can't be easily created
                                if (File.Exists(newFolderWithoutFinalBackslash))
                                {
                                    newFolder = newFolder + "_vac"; //this better not exist!
                                    vacLog += "\n\nFile existed with same name as desired folder, changing folder name to: " + newFolder;
                                }

                                bool extracted = ExtractAllZipFileContents(vamFile.path, newFolder);

                                if (extracted)
                                {
                                    //if there are no json files, and only one subfolder, go one level deeper, perhaps it was zipped inside a secondary directory
                                    List<string> jsonFiles = Directory.GetFiles(newFolder, "*.json").ToList();
                                    if (jsonFiles.Count == 0)
                                    {
                                        List<string> dirs = Directory.GetDirectories(newFolder).ToList();
                                        if (dirs.Count == 1)
                                        {
                                            newFolder = dirs[0];
                                            jsonFiles = Directory.GetFiles(newFolder, "*.json").ToList();
                                            if (jsonFiles.Count == 1)
                                            {
                                                vacLog += "\nFound .json file inside secondary folder: " + newFolder + " .";
                                            }
                                        }
                                    }


                                    //if there is a thumbnail in the same folder, copy it in
                                    // /scene/example.vac
                                    string thumbnailName = vacNameWithoutExtension + ".jpg"; // example.jpg
                                    string thumbnailPath = vacFolder + thumbnailName; // scene\\example.jpg
                                    string newThumbnailPath = newFolder + thumbnailName; // scene\\example\\example.jpg
                                    string newScenePath = newFolder + vacNameWithoutExtension + ".json";
                                    if (File.Exists(thumbnailPath) && Directory.Exists(newFolder) && !File.Exists(newThumbnailPath) && thumbnailPath.StartsWith(VAM_FOLDER) && newThumbnailPath.StartsWith(VAM_FOLDER))
                                    {
                                        File.Move(thumbnailPath, newThumbnailPath);
                                    }

                                    //if there is only one .json file in the new folder, make sure it matches the name of the vac / thumbnail
                                    if (jsonFiles.Contains(newScenePath))
                                    {
                                        //we may have more than one, but one of them matches
                                        extractedVACFiles.Add(vamFile.path);
                                    }
                                    else if (jsonFiles.Count == 0)
                                    {
                                        vacLog += "\nWILL NOT DELETE THIS VAC FILE because there are no .json files in the extracted vac folder: " + newFolder;
                                    }
                                    else if (jsonFiles.Count == 1)
                                    {
                                        extractedVACFiles.Add(vamFile.path);
                                        //make sure the json file matches the folder / thumbnail name
                                        if (jsonFiles[0] != newScenePath && jsonFiles[0].StartsWith(VAM_FOLDER) && newScenePath.StartsWith(VAM_FOLDER))
                                        {
                                            File.Move(jsonFiles[0], newScenePath);
                                            vacLog += "\nChanged .json file name to match thumbnail and folder name.\nWas: " + jsonFiles[0] + "\nNow: " + newScenePath + " .";
                                        }
                                    }
                                    else
                                    {
                                        extractedVACFiles.Add(vamFile.path);
                                        vacLog += "\nTHE EXTRACTED SCENE MAY NOT HAVE A GOOD THUMBNAIL. More than one .json files found in the vac (and none match the vac file name).";
                                    }
                                }
                                else
                                {
                                    vacLog += "\nFAILED TO EXTRACT ZIP FILE: " + vamFile.path;
                                }
                            }
                        }

                        ResetSafeDeletePrompt();
                        bool deletedFiles = false;
                        foreach (string path in extractedVACFiles)
                        {
                            if (SafeDelete(path, "Extracted and Deleted VAC File") < 0) break;
                            deletedFiles = true;
                        }
                        ResetSafeDeletePrompt();

                        WriteLogFile("_CHANGE_LOG_VAC_EXTRACTION_ERRORS.txt", vacLog);
                        vacLog = "";

                        if (deletedFiles)
                        {
                            if (!easyClean) MessageBox.Show("Finished deleting VAC Files from your Saves folder. VACUUM will now rescan your VAM folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (easyClean) easyCleanOptimizedVAC = true;
                            BuildFileLists();
                            return;
                        }
                    }
                }
            }
            else
            {
                if (!easyClean) MessageBox.Show("You have no VAC Files in Saves.\n\nYou either don't use, or have extracted, all VAC files. This saves space, speeds up loading, and allows VACUUM to process all files.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
            }


            curFileNumber = 0;
            totalFileNumber = sourceFiles.Count;
            foreach (string filePath in sourceFiles)
            {
                curFileNumber++;
                SetWindowTitle("(" + curFileNumber + "/" + totalFileNumber + ") Finding Source File Thumbnails");
                //keep possible thumbnail assets that match any source files
                string possibleThumbnail = Path.GetDirectoryName(filePath) + "\\" + Path.GetFileNameWithoutExtension(filePath) + ".jpg";
                if (File.Exists(possibleThumbnail)) thumbnailAssets.Add(possibleThumbnail); //File.Exists is case insensitive

            }

            //log contents of all packages
            SetWindowTitle("Writing log files");

            //folders are dynamically added to neverDeleteFolders when adding files to vamFiles
            //so add all files before showing the neverDeleteFolders
            //single copy only
            neverDeleteFolders = new List<string>(new HashSet<string>(neverDeleteFolders));
            string neverDeleteInfoLog = "";
            neverDeleteFolders.Sort();
            foreach (string neverDeleteFolder in neverDeleteFolders)
            {
                neverDeleteInfoLog += "Protected folder (VACUUM won't delete from this folder): " + neverDeleteFolder + "\n";
            }
            WriteLogFile("All_Protected_Folders.txt", neverDeleteInfoLog);

            //list of clothing ids to change, currently this is clothes to sim clothes
            string replaceClothingIDsLog = "";
            foreach (KeyValuePair<string, string> replaceClothingID in replaceClothingIDs)
            {
                replaceClothingIDsLog += "Would like to replace: " + replaceClothingID.Key + " with: " + replaceClothingID.Value + "\n";
            }
            WriteLogFile("Desired_Clothing_ID_and_Style_Replacements (replace regular clothes with identical Sim clothes).txt", replaceClothingIDsLog);

            ProcessFiles();
        }


        public static void ProcessFiles()
        {
            duplicatePathFilesToDelete.Clear();
            duplicateNameFilesToDelete.Clear();
            oldToNewAssets.Clear();
            oldToNewAssetsIgnoreCase.Clear();
            requiredUIDs.Clear();
            requiredUIDFiles.Clear();
            requiredUIDFilesFromPath.Clear();
            assetUIDs.Clear();

            int curFileNumber = 0;
            int totalFileNumber = filesOfType[VAM_STYLE_TYPE].Count;
            foreach (VAMFile vamFileStyle in filesOfType[VAM_STYLE_TYPE])
            {
                curFileNumber++;
                SetWindowTitle("(" + curFileNumber + "/" + totalFileNumber + ") Processing VAM Styles - " + vamFileStyle.path);

                //can't and don't want to process files with a package, this would only to be to delete unused files from styles, but we won't be deleting files from packages
                if (vamFileStyle.in_var) continue;

                string json = File.ReadAllText(vamFileStyle.path);
                //check vam files in the same folder to see if this is a style of one of them
                foreach (VAMFile vamFileVam in filesOfType[VAM_TYPE])
                {
                    if (vamFileStyle.folder == vamFileVam.folder)
                    {
                        //in the same folder, see if they are linked together (only checking vam matching vaj/vap in the same folder)
                        if (vamFileVam.uid != "")
                        {
                            if (json.Contains(vamFileVam.uid + "Style") || json.Contains(vamFileVam.uid + "Sim"))
                            {
                                //the uid of the vam file is found like this  vamuidStyle  or vamuidSim  in the vaj/vap file
                                if (!requiredUIDs.Contains(vamFileVam.uid)) requiredUIDs.Add(vamFileVam.uid);
                                if (!requiredUIDFiles.ContainsKey(vamFileVam.uid))
                                {
                                    requiredUIDFiles.Add(vamFileVam.uid, new List<string>() { vamFileStyle.path });
                                }
                                else
                                {
                                    if (!requiredUIDFiles[vamFileVam.uid].Contains(vamFileStyle.path))
                                    {
                                        requiredUIDFiles[vamFileVam.uid].Add(vamFileStyle.path);
                                    }
                                }

                                if (!requiredUIDFilesFromPath.ContainsKey(vamFileVam.path))
                                {
                                    requiredUIDFilesFromPath.Add(vamFileVam.path, new List<string>() { vamFileStyle.path });
                                }
                                else
                                {
                                    if (!requiredUIDFilesFromPath[vamFileVam.path].Contains(vamFileStyle.path))
                                    {
                                        requiredUIDFilesFromPath[vamFileVam.path].Add(vamFileStyle.path);
                                    }
                                }
                                vamFileStyle.uses_other_id = true;
                                vamFileStyle.other_id = vamFileVam.uid;
                                vamFileStyle.other_id_file = vamFileVam.path;
                            }
                        }
                    }
                }

                //missing the VAM file?
                if (!vamFileStyle.in_var && !vamFileStyle.uses_other_id)
                {
                    //only vap/vaj in clothing/hair must have a VAM file, other presets are possible that don't use VAM files
                    if (vamFileStyle.path.StartsWith(CUSTOM_FOLDER + "\\Clothing") || vamFileStyle.path.StartsWith(CUSTOM_FOLDER + "\\Hair"))
                    {
                        //could be builtin?
                        bool builtIn = false;
                        if (vamFileStyle.path.Contains("Custom\\Clothing\\Female\\Builtin\\")
                            || vamFileStyle.path.Contains("Custom\\Clothing\\Male\\Builtin\\"))
                        {
                            builtIn = true;
                        }
                        else
                        {
                            string fileName = Path.GetFileName(vamFileStyle.path);
                            foreach (string builtInUID in builtinUIDs)
                            {
                                if (fileName.Contains(builtInUID) || fileName.Contains(builtInUID.Replace(" ", string.Empty)))
                                {
                                    builtIn = true;
                                }
                            }
                        }
                        if (!builtIn) stylesMissingClothingHairVAM.Add(vamFileStyle);
                    }
                }
            }

            SetWindowTitle("Writing Clothing / Hair Logs");

            string stylesMissingClothingHairLog = "";
            foreach (VAMFile vamFile in stylesMissingClothingHairVAM)
            {
                stylesMissingClothingHairLog += "Style: " + vamFile.path + "\nMissing main clothing / hair .vam file from same folder as preset.\n\n";
            }
            WriteLogFile("Missing_Clothing_Hair_vam_File (From Style Or Preset).txt", stylesMissingClothingHairLog);

            //what do we need to do for each
            string existingClothingAndHairLog = "";
            foreach (VAMFile vamFile in vamFileList)
            {
                if (vamFile.organized_by_id)
                {
                    existingClothingAndHairLog += "ID: " + vamFile.uid + "    FILE: " + vamFile.path + "\n";
                    assetUIDs.Add(vamFile.uid);
                }
                AppendErrorLog(vamFile.error_log);
            }
            WriteLogFile("All_Clothing_Hair_UIDs.txt", existingClothingAndHairLog);

            //log list of all assets
            foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
            {
                if (assetType.Key == VAC_TYPE) continue; //we write this log earlier

                List<string> assetsOfCurrentType = new List<string>();
                foreach (VAMFile vamFile in vamFileList)
                {
                    foreach (string testExtension in assetType.Value)
                    {
                        if (vamFile.extension == testExtension)
                        {
                            assetsOfCurrentType.Add(vamFile.path);
                        }
                    }
                }
                SetWindowTitle("Writing log file: " + LOG_FOLDER + "\\All_" + assetType.Key + "_Files.txt");
                assetsOfCurrentType.Sort();
                WriteLogFile("All_" + assetType.Key + "_Files.txt", string.Join("\n", assetsOfCurrentType.ToArray()));
            }


            foreach (string assetType in duplicateAssetTypesToCheck)
            {
                SetWindowTitle("Building list of duplicate " + assetType + " assets with the same name and file size");
                string filesToDeleteLog = "";
                string filePathsToDeleteLog = "";

                Dictionary<string, string> file_names_list = new Dictionary<string, string>();

                foreach (VAMFile vamFile in filesOfType[assetType])
                {
                    if (vamFile.in_var)
                    {
                        //ignore duplicates within VAR packages
                        string key = Path.GetFileName(vamFile.path) + " (size " + vamFile.zipped_file.length + ")";
                        if (!file_names_list.ContainsKey(key))
                        {
                            file_names_list.Add(key, vamFile.path);
                        }
                    }
                }

                //NOW IF WE HAVE DUPLICATES OF WHAT IS IN THE VAR PACKAGES, OR JUST DUPLICATES IN CUSTOM, DELETE THEM
                foreach (VAMFile vamFile in filesOfType[assetType])
                {
                    //not in var, not a thumbnail
                    if (!vamFile.in_var && !IsThumbnail(vamFile.path) && File.Exists(vamFile.path))
                    {
                        //the legacyPathToFirstVARReference is only added to from VAR Packages, so if we have a file that matches, it's matching a VAR Package file in the same virtual path
                        //run this first so on first run we would repath json files to point to VAR packages
                        if (legacyPathToFirstVARReference.ContainsKey(vamFile.path))
                        {
                            if (!oldToNewAssets.ContainsKey(vamFile.path)) oldToNewAssets.Add(vamFile.path, legacyPathToFirstVARReference[vamFile.path]);
                            if (!oldToNewAssetsIgnoreCase.ContainsKey(vamFile.path)) oldToNewAssetsIgnoreCase.Add(vamFile.path, legacyPathToFirstVARReference[vamFile.path]);

                            filePathsToDeleteLog += "Duplicate File With Same Path as VAR Package Resource:\n Duplicate File To Delete: " + vamFile.path + "\n VAR Package Resource: " + legacyPathToFirstVARReference[vamFile.path] + "\n\n";
                            duplicatePathFilesToDelete.Add(vamFile.path);
                        }

                        if (File.Exists(vamFile.path))
                        {
                            FileInfo fi = new FileInfo(vamFile.path);
                            string key = Path.GetFileName(vamFile.path) + " (size " + fi.Length + ")"; //not in MB this is an exact number to use as a file comparison

                            if (file_names_list.ContainsKey(key))
                            {
                                if (!oldToNewAssets.ContainsKey(vamFile.path)) oldToNewAssets.Add(vamFile.path, MakeVaMRelativePath(VAM_FOLDER, file_names_list[key]));
                                if (!oldToNewAssetsIgnoreCase.ContainsKey(vamFile.path)) oldToNewAssetsIgnoreCase.Add(vamFile.path, MakeVaMRelativePath(VAM_FOLDER, file_names_list[key]));

                                filesToDeleteLog += key + " bytes\n Duplicate File: " + vamFile.path + "\n Keep This File: " + file_names_list[key] + "\n\n";
                                duplicateNameFilesToDelete.Add(vamFile.path);
                            }
                            else
                            {
                                file_names_list.Add(key, vamFile.path);
                            }
                        }
                    }
                }

                WriteLogFile("Duplicate_" + assetType + "_Files.txt", filesToDeleteLog);
                WriteLogFile("Duplicate_Path_of_VAR_" + assetType + "_Files.txt", filePathsToDeleteLog);
                WriteLogFile("All_Protected_Files.txt", string.Join("\n", neverDeleteFilePaths.ToArray())); //WRITE THIS AGAIN LATER IN CASE ADDED TO DURING JSON SAVES PROCESSING

                vamFileListPaths.Sort();
                WriteLogFile("All_Resources.txt", string.Join("\n", vamFileListPaths.ToArray()));

                SetWindowTitle("Finished building list of duplicate " + assetType + " assets.");
            }

            string oldToNewAssetsLog = "";
            foreach (KeyValuePair<string, string> morphReplacement in morphReplace)
            {
                oldToNewAssetsLog += "Want to change JSON save file references of morph: " + morphReplacement.Key + "\nTo morph: " + morphReplacement.Value + "\n\n";
            }
            foreach (KeyValuePair<string, string> morphReplacement in morphReplace19)
            {
                oldToNewAssetsLog += "Want to change (in VAM 1.19 and above only) JSON save file references of morph: " + morphReplacement.Key + "\nTo morph: " + morphReplacement.Value + "\n\n";
            }
            foreach (KeyValuePair<string, string> oldToNewAsset in oldToNewAssets)
            {
                oldToNewAssetsLog += "Want to change JSON save file references of: " + MakeVaMRelativePath(VAM_FOLDER, oldToNewAsset.Key) + "\nTo file with same name and size: " + oldToNewAsset.Value + "\n\n";
            }
            WriteLogFile("Desired_JSON_Save_File_Replacements (point to correct resources).txt", oldToNewAssetsLog);

            if (moveToCustomFolders.Count > 0) { 
                moveToCustomFolders = new List<string>(new HashSet<string>(moveToCustomFolders));
                moveToCustomFolders.Sort();
                WriteLogFile("Check_Save_Folders_Which_You_May_Want_To_Move_To_Custom.txt",
                    "These folders have \\Custom\\ in their paths but are in the Saves folder. You may want to manually move them to the appropriate Custom folders.\n" +
                    string.Join("\n", moveToCustomFolders));
            }

            if (longPathFiles.Count > 0) { 
                longPathFiles = new List<string>(new HashSet<string>(longPathFiles));
                longPathFiles.Sort();
                WriteLogFile("Check_Files_With_Very_Long_Paths.txt",
                    "These files have paths that are so long VAM may not be able to open them. " +
                    "\nPaths longer than around " + PATH_LENGTH_TOO_LONG.ToString() + " characters may cause issues." +
                    "\nYou may want to move your VAM folder to create a shorter path for all VAM files (e.g. move a VAM folder with a long path of D:\\Some Folder\\Another Folder\\Another Folder\\VAM into D:\\VAM).\n\n" +
                    string.Join("\n", longPathFiles));
            }

            if (easyClean && easyCleanOptimizedVAR)
            {
                AppendDeletedLog("Easy Clean Already Deleted Duplicates of VAR Package Contents, Skipping Second Optimization.");
            }
            else if (duplicatePathFilesToDelete.Count > 0)
            {
                if (easyClean || MessageBox.Show("Would you like to review and optionally delete old files that are replaced by VAR Package files?" +
                    "\n\n\nVAR Packages contain new and improved versions of legacy files (legacy files are stored in Custom or Saves instead of in VAR pacakges).\n\nVAR Packages contain virtual paths to Custom/Saves so it's possible to check if there are old files at the absolute path." +
                    "\n\nIF YOU DELETE OLDER VERSIONS OF FILES NOW REPLACED BY VAR FILES, DO A DEEP CLEAN (which you will be prompted to do anyway) AND ANALYZE YOUR SOURCE FILES TO REPLACE SAVE FILE REFERENCES TO OLD FILES WITH NEW REFERENCES TO VAR PACKAGE FILES!" +
                    "\n\nLists of all duplicate files are in the files:\n" + LOG_FOLDER + "\\\nDuplicate_Path_of_VAR_Assetbundle_Files.txt\nDuplicate_Path_of_VAR_Audio_Files.txt\nDuplicate_Path_of_VAR_Image_Files.txt", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    bool deletedFiles = false;
                    ResetSafeDeletePrompt();
                    foreach (string path in duplicatePathFilesToDelete)
                    {
                        if (SafeDelete(path, "Replaced by VAR Package") < 0) break;
                        deletedFiles = true;
                    }
                    ResetSafeDeletePrompt();
                    if (deletedFiles)
                    {
                        if (!easyClean) MessageBox.Show("Finished processing duplicate files that match VAR package files. VACUUM will now rescan your VAM folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (easyClean) easyCleanOptimizedVAR = true;
                        BuildFileLists();
                        return;
                    }
                }
            }
            else
            {
                if (!easyClean) MessageBox.Show("You have no files in Custom that have duplicate paths as files within VAR packages.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            AppendErrorLog(static_error_log);
            static_error_log = "";
            ResetWindowTitle();

            DeleteDuplicateUIDs();
        }

        public static bool IsThumbnail(string path)
        {
            //thumbnails match scenes even with bad case ("Default.jpg" shows for "default.json" scene)
            return thumbnailAssets.FindIndex(x => x.Equals(path, StringComparison.OrdinalIgnoreCase)) >= 0; //-1 means not found
        }

        public static int DeleteAssociatedSettingsFiles(string path)
        {
            if (path.EndsWith(".vam"))
            {
                //check for vaj and vap files that should be deleted
                if (requiredUIDFilesFromPath.ContainsKey(path) && requiredUIDFilesFromPath[path].Count > 0)
                {
                    foreach (string fileToDelete in requiredUIDFilesFromPath[path])
                    {
                        //no infinite recursion
                        if (!fileToDelete.EndsWith(".vam"))
                        {
                            if (SafeDelete(fileToDelete, "This Style / Preset was for a Deleted Duplicate or Unused Clothing / Hair File.") < 0) return -1;
                        }
                    }
                }
            }
            return 0;
        }

        public static void WriteLogFile(string path, string contents)
        {
            path = LOG_FOLDER + "\\" + path;

            //backup the existing file if its not empty
            if (File.Exists(path))
            {
                FileInfo fi = new FileInfo(path);
                if (fi.Length > 0)
                {
                    string newFilePath = LOG_FOLDER_LAST + "\\last_not_empty_" + Path.GetFileName(path);
                    if (newFilePath.StartsWith(VAM_FOLDER))
                    {
                        if (File.Exists(newFilePath)) File.Delete(newFilePath);
                        File.Move(path, newFilePath);
                    }
                }
            }

            File.WriteAllText(path, contents);
        }
        public static void AppendLogFile(string path, string contents)
        {
            if (contents != "")
            {
                path = LOG_FOLDER + "\\" + path;
                File.AppendAllText(path, DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + ": " + contents + "\n");
            }
        }

        private static List<string> GetClothingAndHairFolders()
        {
            List<string> foldersToCheck = new List<string>();
            foldersToCheck.Clear();
            foldersToCheck.Add(VAM_FOLDER + "\\Custom\\Clothing\\Female");
            foldersToCheck.Add(VAM_FOLDER + "\\Custom\\Clothing\\Male");
            foldersToCheck.Add(VAM_FOLDER + "\\Custom\\Hair\\Female");
            foldersToCheck.Add(VAM_FOLDER + "\\Custom\\Hair\\Male");

            return foldersToCheck;
        }

        public static void DeleteDuplicateUIDs()
        {
            SetWindowTitle("Checking for Duplicate Clothing and Hair UIDs");
            List<string> foldersToCheck = GetClothingAndHairFolders();

            string filesToDeleteLog = "";
            List<string> filesToDelete = new List<string>();

            //it's ok to have a duplicate uid if one is female and one is male, so check each folder separately
            foreach (string folderName in foldersToCheck)
            {
                //do we have duplicate files with the same id? (name and size don't matter)
                //FIRST CHECK VAR PACKAGES
                Dictionary<string, string> uid_list = new Dictionary<string, string>();
                foreach (VAMFile vamFile in filesOfType[VAM_TYPE])
                {
                    if (vamFile.in_var)
                    {
                        if (vamFile.legacy_path_equivalent_from_var.StartsWith(folderName))
                        {
                            //ignore duplicates within
                            if (!uid_list.ContainsKey(vamFile.uid))
                            {
                                uid_list.Add(vamFile.uid, vamFile.path);
                            }
                        }
                    }
                }

                //NOW IF WE HAVE DUPLICATES OF WHAT IS IN THE VAR PACKAGES, OR JUST DUPLICATES IN CUSTOM, DELETE THEM
                foreach (VAMFile vamFile in filesOfType[VAM_TYPE])
                {
                    //not in var
                    if (!vamFile.in_var)
                    {
                        if (vamFile.path.StartsWith(folderName))
                        {
                            if (uid_list.ContainsKey(vamFile.uid))
                            {
                                filesToDeleteLog += "UID: " + vamFile.uid + "\n Duplicate File To Delete: " + vamFile.path + "\n Keeping This File: " + uid_list[vamFile.uid] + "\n\n";
                                filesToDelete.Add(vamFile.path);
                            }
                            else
                            {
                                uid_list.Add(vamFile.uid, vamFile.path);
                            }
                        }
                    }
                }
            }

            WriteLogFile("Duplicate_Clothing_Hair_UIDs.txt", filesToDeleteLog);

            if (easyClean && easyCleanOptimizedIDS)
            {
                AppendDeletedLog("Easy Clean Already Deleted Duplicate Hair and Clothing UID based Files, Skipping Second Optimization.");
            }
            else if (filesToDelete.Count > 0)
            {
                if (easyClean || MessageBox.Show("Would you like to review dupliate Clothing and Hair UIDs for possible deletion?" +
                    "\n\n\nThese are clothing / hair items from VAR packages or in your custom folder, which serve the same purpose" +
                    " (male clothing for example) but have conflicting / duplicate UIDs." +
                    "\n\nA list of all duplicates is at:\n" + LOG_FOLDER + "\\Duplicate_Clothing_Hair_UIDs.txt", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    bool deletedFiles = false;
                    ResetSafeDeletePrompt();
                    foreach (string path in filesToDelete)
                    {
                        if (SafeDelete(path, "Deleted Duplicate Clothing / Hair UID File") < 0) break;
                        deletedFiles = true;
                    }
                    ResetSafeDeletePrompt();
                    if (!easyClean) MessageBox.Show("Finished processing files with duplicate UIDs.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (deletedFiles)
                    {
                        if (!easyClean) MessageBox.Show("Finished deleting duplicate clothing / hair files. VACUUM will now rescan your VAM folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (easyClean) easyCleanOptimizedIDS = true;
                        BuildFileLists();
                        return;
                    }
                }
            }
            else
            {
                if (!easyClean) MessageBox.Show("You have no duplicate Clothing / Hair UIDs.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SetWindowTitle("Building List of Morphs");
            existingMorphPaths = GetVAMFilePaths(MORPHS_FOLDER, MORPH_EXTENSION);
            //SetWindowTitle("Writing log file: " + LOG_FOLDER + "\\All_Morph_Files.txt");
            //WriteLogFile("All_Morphs_Files.txt", string.Join("\n", existingMorphPaths.ToArray()));

            int curFileNumber = 0;
            int totalFileNumber = existingMorphPaths.Count;
            string morphIDLog = "";

            //morphs in packages
            foreach (VAMFile vamFile in vamFileList)
            {
                if (vamFile.in_var && vamFile.is_morph)
                {
                    try
                    {
                        string json = vamFile.zipped_file.data; 
                        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                        string morphID = jsonObj["displayName"]; //matches "name" in .json file morph list

                        if (morphID != null && morphID != "")
                        {
                            existingMorphs.Add(morphID);
                            if (morphIDToFiles.ContainsKey(morphID))
                            {
                                morphIDToFiles[morphID].Add(vamFile.path);
                                string errorMessage = "Duplicate morph ids: " + morphID + " found in " + vamFile.path + " and " + morphIDToFiles[morphID][0];
                                morphIDLog += "\n" + errorMessage + "\n\n";
                            }
                            else
                            {
                                morphIDToFiles.Add(morphID, new List<string>() { vamFile.path });
                                morphIDLog += "Morph Display Name: " + morphID + " - FILE: " + vamFile.path + "\n";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = "Error parsing morph: " + vamFile.path + " Message: " + ex.Message;
                        static_error_log += "\n" + errorMessage;
                        morphIDLog += "\n\n" + errorMessage + "\n\n";
                    }
                }
            }

            //morphs in custom
            foreach (string morphPath in existingMorphPaths)
            {
                curFileNumber++;
                SetWindowTitle("(" + curFileNumber + "/" + totalFileNumber + ") Scanning Morph - " + morphPath);
                try { 
                    string json = File.ReadAllText(morphPath);
                    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

                    string morphID = jsonObj["displayName"]; //matches "name" in .json file morph list

                    //used to help determine what to match to, as there were both id and displayName fields that often were the same
                    /*string morphID2 = jsonObj["id"];
                    if (morphID != null && morphID != "" && morphID2 != null && morphID2 != "")
                    {
                        if (morphID != morphID2)
                            ShowErrorDialog(morphID + " != " + morphID2 + " in file " + morphPath);
                    }*/

                    if (morphID != null && morphID != "")
                    {
                        existingMorphs.Add(morphID);
                        if (morphIDToFiles.ContainsKey(morphID))
                        {
                            morphIDToFiles[morphID].Add(morphPath);
                            string errorMessage = "Duplicate morph ids: " + morphID + " found in " + morphPath + " and " + morphIDToFiles[morphID][0];
                            morphIDLog += "\n" + errorMessage + "\n\n";
                        } else {
                            morphIDToFiles.Add(morphID, new List<string>() { morphPath });
                            morphIDLog += "Morph Display Name: " + morphID + " - FILE: " + morphPath + "\n";
                        }
                    }
                } 
                catch (Exception ex)
                {
                    string errorMessage = "Error parsing morph: " + morphPath + " Message: " + ex.Message;
                    static_error_log += "\n" + errorMessage;
                    morphIDLog += "\n\n" + errorMessage + "\n\n";
                }
            }

            SetWindowTitle("Writing log file: " + LOG_FOLDER + "\\All_Morph_File_IDs.txt");
            WriteLogFile("All_Morph_File_IDs.txt", morphIDLog);

            AppendErrorLog(static_error_log);
            static_error_log = "";
            ResetWindowTitle();

            readyToDeepClean = true;

            ProcessSourceFiles();
        }

        public static void ProcessSourceFiles()
        {
            if (!stopWatchRunning) stopWatch.Restart();
            stopWatchRunning = true;

            if (!readyToDeepClean)
            {
                BuildFileLists();
                return;
            }


            if (!easyClean)
            {
                if (MessageBox.Show("Deep Clean\n\nWould you like to analyze all source files (.json, .cs, etc.)?" +
                    "\n\n\nAfter analysis you will have various options to clean your VAM folder, including the option to modify json save files to point to new VAR Package resources, and the option to delete unnessary files (duplicate files, where the copy isn't used in any scene, for example)." +
                    "\n\nThis will take a while, check VACUUM's top title bar for progress indicators."
                    , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                {
                    return;
                }
            }

            savefileAssets.Clear();
            requiredAssets.Clear();


            Dictionary<string, List<string>> missingAssets = new Dictionary<string, List<string>>();

            bool updateJSONFiles = false;
            bool autoUpdateJSONFiles = false;
            bool replaceClothes = false;
            if (easyClean)
            {
                updateJSONFiles = true;
                autoUpdateJSONFiles = true;
                replaceClothes = false;

                if (MessageBox.Show("Would you like to replace Regular Clothes with Identical Sim Clothes (where available) to allow you to touch and move clothes using Clothing -> Undress All?" +
                    "\n\n\nThis changes JSON Save files to use Sim versions of clothing if a Sim version is available of the same clothing." +
                    "\n\n\nIt's possible, but unlikely, that these replacements may not contain the configuration (customized look) of those clothes." +
                    "\n\nA list of clothes to replace is shown in the _VACUUM_LOGS\\Desired_Clothing_ID_Replacements (replace regular clothes with identical Sim clothes).txt log file."
                    , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    //pressed yes
                    replaceClothes = true;
                }
            }
            else
            {
                if (MessageBox.Show("Would you like to save space by allowing VACUUM to modify JSON Save files to use new VAR Package assets (or a single copy of duplicate assets)?" +
                "\n\n\nDuring this process VACUUM can modify Save files to point to the same copy of an asset. This allows VACUUM to update JSON files in Saves to point to updated assets in VAR Packages." +
                "\n\nIf you press \"Yes\" clothing and hair styles and settings will also be scanned and fixed if they point to textures that don't exist (but that you have elsewhere in your VAM folder)."
                , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    updateJSONFiles = true;

                    if (MessageBox.Show("Would you like to replace Regular Clothes with Identical Sim Clothes (where available) to allow you to touch and move clothes using Clothing -> Undress All?" +
                        "\n\n\nThis changes JSON Save files to use Sim versions of clothing if a Sim version is available of the same clothing." +
                        "\n\nA list of clothes to replace is shown in the _VACUUM_LOGS\\Desired_Clothing_ID_Replacements (replace regular clothes with identical Sim clothes).txt log file."
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        //pressed yes
                        replaceClothes = true;
                    }

                    if (MessageBox.Show("Would you like to be prompted for each file before a source files (JSON, VAJ or VAP) is modified?" +
                        "\n\n\nIf you press \"Yes\" VACUUM will pause during analysis of the source files to PROMPT YOU FOR EACH MODIFICATION (but you will have the option to change your mind and auto-update all when prompted)." +
                        "\n\nIf you press \"No\" VACUUM will UPDATE ALL SOURCE FILES without further prompting." +
                        "\n\nA list of all changes will be stored in the _VACUUM_LOGS\\_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt file." +
                        "\n\nThe desired changes (to be scanned for in each source file) are available to review in the Desired_JSON_Save_File_Replacements (point to correct resources).txt log file."
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        //pressed no
                        autoUpdateJSONFiles = true;
                    }
                }
            }


            //FOR EACH DUPLICATE, THE FIRST ITEM FOUND (WHICH WILL BE IN CUSTOM IF THERE IS ONE) WILL BE THE NEW REAL ITEM
            //NOW IF A SAVE FILE HAS THE ASSET, EDIT THE JSON (ONLY EDIT JSON FILES) TO CHANGE ITS LOCATION

            /* if (MessageBox.Show("You may have duplicate copies of identical image / audio / asset files. " +
                "Would you like to scan and update scenes and looks (all .json files) to point to the same " +
                "copy of identical files (so duplicates can be removed)?\n\nA file is considered identical if it has the same name and same size. " +
                "If you press \"Yes\" you will have the option to review each individual json file change before it's made.", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)*/


            List<string> extensionSearchStrings = new List<string>();
            foreach (string duplicateAssetExtensionToCheck in duplicateAssetExtensionsToCheck)
            {
                extensionSearchStrings.Add("." + duplicateAssetExtensionToCheck + '"');
            }

            //for script files and tif files we don't delete them, but still search for them and replace references if there are missing files at that url, but same name files somewhere else
            List<string> neverDeleteSearchStrings = new List<string>();
            //special check for plugins, as we can at least notice if we have a missing plugin and try to repath to the existing plugin
            foreach (string scriptType in assetTypes[SCRIPT_TYPE])
            {
                neverDeleteSearchStrings.Add("." + scriptType + '"');
            }
            //at this point neverDeleteSearchStrings should only contain script file extensions!!!
            extensionSearchStrings.AddRange(neverDeleteSearchStrings);  //we are also searching from scripts files

            foreach (string vamClothingHairType in assetTypes[VAM_TYPE])
            {
                extensionSearchStrings.Add("." + vamClothingHairType + '"'); //also we replace vam clothing / hair with new clothing / hair from var files
            }

            foreach (string extension in neverDeleteExtensions)
            {
                neverDeleteSearchStrings.Add("." + extension + '"'); //we find pointers to files of type tif, but we never delete these kinds of files
            }


            int numFiles = sourceFiles.Count();
            int curFile = 0;
            string tempFile = LOG_FOLDER + "\\" + "VACUUM_temp_delete_me.json";
            foreach (string filePath in sourceFiles)
            {
                curFile++;

                SetWindowTitle("Scanning (" + curFile + "/" + numFiles + ") - " + filePath);
                if (!File.Exists(filePath))
                {
                    continue;
                }

                bool changedLine = false;
                bool changed = false;

                int lineNumber = 0;

                string extension = GetFileExtension(filePath);
                bool isJSONSaveFile = filePath.StartsWith(SAVES_FOLDER + "\\") && extension == "json";
                bool isClothingHairStyle = extension == "vap" || extension == "vaj";
                bool editThisFile = isJSONSaveFile || isClothingHairStyle;

                StreamWriter newFile = null;
                if (editThisFile && updateJSONFiles)
                {
                    //make the new file we may use
                    newFile = File.AppendText(tempFile);
                }

                bool replaceClothingID = false;
                string sourceClothingIDString = "";
                string targetClothingIDString = "";

                string[] lines = File.ReadAllLines(filePath);
                //only scan json files (that exist) for clothing, hair and morphs
                if (filePath.EndsWith(".json"))
                {
                    string json = string.Join("\r\n", lines);
                    string morphListJSON = "";
                    var match = Regex.Match("", "");

                    //MORPHS
                    try
                    {
                        int morphsPosition = json.IndexOf("\"morphs\"");
                        if (morphsPosition > 0)
                        {
                            int morphListStart = json.IndexOf('[', morphsPosition);
                            morphListJSON = json.Substring(morphListStart);

                            // Find the next closing bracket that had some sort of white-space in front of it.
                            // This avoids clipping the json array too early like in cases of brackets in names.
                            // Thanks, "[Alter3go]". :p
                            match = Regex.Match(morphListJSON, @"\s+]");
                            morphListJSON = morphListJSON.Substring(0, match.Index + match.Length);

                            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(morphListJSON);
                            foreach (dynamic morph in jsonObj)
                            {
                                string name = morph["name"]; //by displayName
                                desiredMorphs.Add(name);
                                if (!filesReferenceMorph.ContainsKey(name))
                                {
                                    filesReferenceMorph[name] = new List<string>();
                                }
                                filesReferenceMorph[name].Add(filePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendErrorLog("Couldn't parse morphs. Probably bad JSON in file: " + filePath + " Message: " + ex.Message);
                    }

                    //CLOTHING AND HAIR
                    // Instead of parsing the whole scene, cut out just the clothes and hair lists to speed up evaluation.
                    //"clothing"  "hair"
                    List<string> clothingHairKeys = new List<string>();
                    clothingHairKeys.Add("\"hair\" : [");
                    clothingHairKeys.Add("\"clothing\" : [");

                    string keyValue = "";

                    int charCount = json.Length;

                    foreach (string key in clothingHairKeys)
                    {
                        int keyIndex = json.IndexOf(key);
                        while (keyIndex >= 0)
                        {
                            int keyValueStart = json.IndexOf('[', keyIndex);

                            if (keyValueStart > 0)
                            {

                                int bracketCount = 1;
                                for (int i = keyValueStart + 1; i < charCount; i++)
                                {
                                    if (json[i] == '[')
                                    {
                                        bracketCount++;
                                    }
                                    else if (json[i] == ']')
                                    {
                                        bracketCount--;
                                    }

                                    //to think through, imagine the string "[]"
                                    //keyValueStart would be 0, and when breaking, i would be 1, in which case we need Substring(0, 2)
                                    if (bracketCount <= 0)
                                    {
                                        keyValue = json.Substring(keyValueStart, (i - keyValueStart) + 1);
                                        break;
                                    }
                                }

                                try { 
                                    dynamic jsonObjs = Newtonsoft.Json.JsonConvert.DeserializeObject(keyValue);
                                    foreach (dynamic jsonObj in jsonObjs)
                                    {
                                        string uid = jsonObj["id"];
                                        if (uid != null && uid != "")
                                        {
                                            if (uid.Contains("/"))
                                            {
                                                //this is an absolute path to the uid (which is ok in VAM 1.19 and above I guess?)
                                                uid = MakeVaMAbsolutePath(VAM_FOLDER, Path.GetDirectoryName(filePath), uid); //this should also convert relative paths to addonpackages
                                            }

                                            if (replaceClothes && replaceClothingIDs.ContainsKey(uid) && key.Contains("clothing"))
                                            {
                                                //mark this file as wanting to replace regular with sim ids
                                                replaceClothingID = true;
                                            }

                                            if (!requiredUIDs.Contains(uid)) requiredUIDs.Add(uid);

                                            if (!requiredUIDFiles.ContainsKey(uid))
                                            {
                                                requiredUIDFiles.Add(uid, new List<string>() { filePath });
                                            }
                                            else
                                            {
                                                if (!requiredUIDFiles[uid].Contains(filePath))
                                                {
                                                    requiredUIDFiles[uid].Add(filePath);
                                                }
                                            }

                                        }
                                    }
                                    keyIndex = json.IndexOf(key, keyIndex + 1);
                                }
                                catch (Exception ex)
                                {
                                    AppendErrorLog("Couldn't parse clothing or hair. Probably bad JSON in file: " + filePath + " Message: " + ex.Message);
                                    //ShowErrorDialog("Couldn't parse clothing or hair. Probably bad JSON in file:\n" + filePath + "\n\n" + ex.Message);
                                }
                            }
                        }
                    }
                }


                foreach (string line in lines)
                {
                    lineNumber++;
                    changedLine = false;

                    //to speed things up, first check if the line even contains a quote
                    if (line.Contains('"'))
                    {
                        //so, for each line with a quote
                        bool isMorphReplace = false;
                        bool isClothingReplace = false;
                        string originalString = "";
                        string newString = "";

                        //is it just a morph that needs replacing?
                        //what is the last quoted value on this line, may be a morph
                        if (isJSONSaveFile && updateJSONFiles)
                        {
                            int endQuoteMorph = line.LastIndexOf('"');
                            if (endQuoteMorph > 2) //just some value greater than 1, really the index will have to be much more in order for there to be a morph
                            {
                                int startQuoteMorph = line.LastIndexOf('"', endQuoteMorph - 1);
                                if (startQuoteMorph > 2)
                                {
                                    string quotedText = line.Substring(startQuoteMorph + 1, (endQuoteMorph - startQuoteMorph) - 1);
                                    if (morphReplace.ContainsKey(quotedText))
                                    {
                                        //replace this morph
                                        isMorphReplace = true;
                                        originalString = '"' + quotedText + '"';
                                        newString = '"' + morphReplace[quotedText] + '"';
                                    }
                                    else if (isVAM19 && morphReplace19.ContainsKey(quotedText))
                                    {
                                        //replace this morph
                                        isMorphReplace = true;
                                        originalString = '"' + quotedText + '"';
                                        newString = '"' + morphReplace19[quotedText] + '"';

                                    }
                                    else if (replaceClothes && replaceClothingID)
                                    {
                                        //search for both the clothing id to replace, plus any associated styles
                                        //also replacing things like SummerGirlsShortsMaterial and SummerGirlsShortsItemControl with SummerGirlsShortsSimMaterial and SummerGirlsShortsSimItemControl
                                        //so don't search for end quote, replace SummerGirlShorts* with SummerGirlsShortsSim*
                                        foreach (KeyValuePair<string, string> replaceClothing in replaceClothingIDs)
                                        {
                                            sourceClothingIDString = "\"id\" : \"" + replaceClothing.Key;
                                            if (line.Contains(sourceClothingIDString))
                                            {
                                                //special case
                                                //vam is strange with alphakini, it doesn't use Sim in things like AlphakiniPantySimMaterial but does have AlphakiniPantySim
                                                //AlphakiniPantySim is never a start for something like AlphakiniPantySimStyle, but there is an AlphaKiniPantySim
                                                if (line.Contains("\"id\" : \"AlphakiniPantySim\"") || line.Contains("\"id\" : \"AlphakiniBraSim\""))
                                                {
                                                    //skip these they are good
                                                } else { 
                                                    isClothingReplace = true;
                                                    originalString = sourceClothingIDString;
                                                    newString = "\"id\" : \"" + replaceClothing.Value;
                                                }
                                            }
                                        }
                                    }

                                    if ((isMorphReplace || isClothingReplace) && originalString != "" && newString != "" && (originalString != newString || line.Contains("SimSim") || line.Contains("Sim Sim")))
                                    {
                                        string originalLine = line;
                                        string updatedLine = line.Replace(originalString, newString);

                                        if (isClothingReplace && replaceClothes)
                                        {
                                            //DON'T ALLOW FOR MULTI-SIM, FIXING ERRORS CAUSED BY PRIOR VERSION OF VACUUM
                                            //REMOVE THIS AT SOME POINT?  OR WE NEED AT LEAST THE BASICS TO STOP FROM CREATING MORE SimSim VERSIONS
                                            updatedLine = updatedLine.Replace("SimSimSimSimSimSim", "SimSim");
                                            updatedLine = updatedLine.Replace("SimSimSimSimSim", "SimSim");
                                            updatedLine = updatedLine.Replace("SimSimSimSim", "SimSim");
                                            updatedLine = updatedLine.Replace("SimSimSim", "SimSim");
                                            updatedLine = updatedLine.Replace("SimSimStyle", "SimStyle");
                                            updatedLine = updatedLine.Replace("SimSimMaterial", "SimMaterial");
                                            updatedLine = updatedLine.Replace("SimSimWrapControl", "SimWrapControl");
                                            updatedLine = updatedLine.Replace("SimSimMain", "SimMain");
                                            updatedLine = updatedLine.Replace("Sim Sim Sim Sim", "Sim");
                                            updatedLine = updatedLine.Replace("Sim Sim Sim", "Sim");
                                            updatedLine = updatedLine.Replace("Sim Sim", "Sim");
                                        }

                                        if (updatedLine != originalLine)
                                        {
                                            bool doChange = true;

                                            if (!autoUpdateJSONFiles)
                                            {
                                                doChange = false;
                                                MessageBoxResult messageResult = MessageBox.Show("Do you want to replace:\n\n" + originalString + "\n\nwith\n\n" + newString + "\n\nIn line " + lineNumber + " of file:\n" + filePath + "?\n\nPRESS CANCEL TO STOP PROMPTING FOR EACH FILE.\nAFTER PRESSING \"Cancel\" YOU CAN PRESS \"Yes\" TO CHANGE ALL, OR \"No\" TO CHANGE NONE.", "VACUUM", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                                                if (messageResult == MessageBoxResult.Cancel)
                                                {

                                                    if (MessageBox.Show("Press \"No\" to stop processing this list of files (no further changes).\n\nPress \"Yes\" to process all files without prompting (change all).",
                                                        "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                                                    {
                                                        doChange = true;
                                                        autoUpdateJSONFiles = true;
                                                    }
                                                    else
                                                    {
                                                        updateJSONFiles = false;
                                                    }
                                                }
                                                else if (messageResult == MessageBoxResult.Yes)
                                                {
                                                    doChange = true;
                                                }
                                            }

                                            if (doChange)
                                            {
                                                if (isClothingReplace)
                                                {
                                                    AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                        "\nREPLACING CLOTHING WITH SIM VERSION OF SAME CLOTHING SO UNDRESS WORKS AND YOU CAN INTERACT WITH THE CLOTHING." +
                                                        "\nReplaced: " + originalString + " with " + newString + " in " + originalLine +
                                                        "\nUpdated line " + lineNumber + " text now reads: " + updatedLine + "\n\n");
                                                }
                                                else if (isMorphReplace)
                                                {
                                                    AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                        "\nREPLACING KNOWN BAD MORPH NAME WITH CORRECT CURRENT MORPH NAME." +
                                                        "\nReplaced: " + originalString + " with " + newString +
                                                        "\nUpdated line " + lineNumber + " text now reads: " + updatedLine + "\n\n");
                                                }
                                                newFile.WriteLine(updatedLine);
                                                changedLine = true;
                                                changed = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //IF WE REPLACED A MORPH, STOP PROCESSING!  TO SAVE TIME / NOT WASTE RESOURCES
                        //IF WE ABSOLUTE MATCHED A MORPH, WE DIDN'T END IN A FILE EXTENSION AND WON'T FIND ONE
                        //JUST MAKE SURE THE morphReplace DOESN'T CONTAIN STRANGE THINGS LIKE .JPG AS A MORPH NAME
                        if (!isMorphReplace && !isClothingReplace)
                        {
                            string lineLower = line.ToLower();
                            foreach (string searchString in extensionSearchStrings)
                            {
                                if (lineLower.Contains(searchString))
                                {
                                    int index = line.ToLower().IndexOf(searchString);
                                    int endQuote = line.IndexOf('"', index);
                                    int startQuote = line.LastIndexOf('"', index);
                                    string rawPath = line.Substring(startQuote + 1, (endQuote - startQuote) - 1);

                                    string assetPath = MakeVaMAbsolutePath(VAM_FOLDER, Path.GetDirectoryName(filePath), rawPath);

                                    if (assetPath == "") continue;

                                    //do we have that asset?
                                    //we store this even if we aren't sure it's a url reference, so we don't delete files that may be used
                                    bool assetFound = vamFileListPaths.Contains(assetPath); //search all found files

                                    bool updatingLegacyURLPath = false;

                                    //are we a json file in the saves folder? and we are working on a line that looks like it contains a url/reference
                                    if (editThisFile && updateJSONFiles)
                                    {
                                        //some things are quoted that aren't assets, but do contain extensions
                                        //check lines that have quotes that end like   tex"  or  url"   or Url"
                                        //ARE THERE OTHER POSSIBLE LINES WHICH SHOULD BE CHANGED?
                                        bool isURL = false;
                                        if (searchString.Contains(".vam\""))
                                        {
                                            isURL = line.Contains("\"id\"");
                                        }
                                        else
                                        {
                                            isURL = lineLower.Contains("tex\"") || lineLower.Contains("texture\"") || lineLower.Contains("url\"") || lineLower.Contains("bumpmap\"") || lineLower.Contains("\"url") || line.Contains("LUT\"") || line.Contains("\"plugin#");
                                        }
                                        if (!isURL)
                                        {
                                            //let's log these just in case
                                            if (line.Contains("\"displayName\"") || line.Contains("\"audioClip\""))
                                            {
                                                //these should be ignored
                                                continue;
                                            }
                                            else
                                            {
                                                //THESE ARE ANNOYING AND MAKE IT HARD TO FIND SINGLE CHANGES
                                                //BETTER TO ONLY MARK WHAT IS ACTUALLY CHANGED
                                                /*AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                    "\nDIDN'T CHANGE AS VACUUM WASN'T SURE THIS WAS A URL." +
                                                    "\nDidn't Replace: " + '"' + rawPath + '"' + " on line number: " + lineNumber + " (existing line text: " + line + ")\n\n");*/
                                            }
                                        }
                                        else
                                        {

                                            //shouldn't be a thumbnail but checking
                                            if (!IsThumbnail(assetPath))
                                            {

                                                //if we are missing the asset, let's look more broadly to all files of the same name
                                                string newRelativePath = "";
                                                bool fileOfSameName = false;
                                                bool foundNewPath = false;
                                                bool neverDeleteFile = neverDeleteSearchStrings.Contains(searchString);

                                                //script files can only be found in filesOfType
                                                if (neverDeleteFile)
                                                {
                                                    //and the only thing we do with them is replace the link if it links to a file that doesn't exist
                                                    if (!assetFound)
                                                    {
                                                        VAMFile vamFileMatch = filesOfType[SCRIPT_TYPE].FirstOrDefault(element => element.path.EndsWith(Path.GetFileName(assetPath)));
                                                        if (vamFileMatch != null)
                                                        {
                                                            if (File.Exists(vamFileMatch.path) || vamFileMatch.in_var)
                                                            {
                                                                //match is an absolute path, need the relative path
                                                                newRelativePath = MakeVaMRelativePath(VAM_FOLDER, vamFileMatch.path);
                                                                fileOfSameName = true;
                                                                foundNewPath = true;
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (oldToNewAssets.ContainsKey(assetPath))
                                                {
                                                    //old to new assets maps an absolute path of the old asset, to a relative path of the new asset
                                                    newRelativePath = oldToNewAssets[assetPath];
                                                    foundNewPath = true;
                                                }
                                                else if (oldToNewAssetsIgnoreCase.ContainsKey(assetPath))
                                                {
                                                    //check to see if another file with the same path but different upper/lower case is found in oldToNewAssets
                                                    newRelativePath = oldToNewAssetsIgnoreCase[assetPath];
                                                    foundNewPath = true;
                                                }
                                                else if (vamFileListPaths.Contains(assetPath, StringComparer.OrdinalIgnoreCase))
                                                {
                                                    //check to see if another file with the same path but different upper/lower case is found in the file system 
                                                    string match = vamFileListPaths.FirstOrDefault(element => element.Equals(assetPath, StringComparison.CurrentCultureIgnoreCase));
                                                    if (match != "" && match != null)
                                                    {
                                                        if (File.Exists(match) || match.Contains(":/"))
                                                        {
                                                            //match is an absolute path, need the relative path
                                                            newRelativePath = MakeVaMRelativePath(VAM_FOLDER, match);
                                                            foundNewPath = true;
                                                        }
                                                    }
                                                }

                                                //same name asset in files to replace
                                                if (!foundNewPath && !assetFound && !neverDeleteFile)  //don't do this if we aren't supposed to change to a new file
                                                {
                                                    string oldToNewAssetsKey = "";
                                                    string assetName = Path.GetFileName(assetPath);
                                                    //find another key that contains an asset of the same name?
                                                    oldToNewAssetsKey = oldToNewAssets.Keys.Where(currentKey => currentKey.Contains(assetName)).FirstOrDefault();
                                                    if (oldToNewAssetsKey != "" && oldToNewAssetsKey != null)
                                                    {
                                                        newRelativePath = oldToNewAssets[oldToNewAssetsKey];
                                                        fileOfSameName = true;
                                                        foundNewPath = true;
                                                    }
                                                }

                                                //if we didn't find a better file, and we did find the existing asset, and it's a url value, update the location if needed
                                                if (!foundNewPath && assetFound)
                                                {
                                                    if (rawPath.StartsWith(CUSTOM_FOLDER) || rawPath.StartsWith(SAVES_FOLDER) || rawPath.StartsWith(PACKAGES_FOLDER) || rawPath.StartsWith("./") || rawPath.IndexOf("/") < 0)
                                                    {
                                                        //starts with custom, or saves, or known folder, or specific relative path, or doesn't have a subfolder like texfolder/texfile.jpg
                                                        //leave it as it is
                                                    }
                                                    else
                                                    {
                                                        //let's fix this up
                                                        //it contains a urlValue and we want to make sure it paths relative to Custom, Saves, Textures, etc.
                                                        newRelativePath = MakeVaMRelativePath(VAM_FOLDER, assetPath);
                                                        foundNewPath = true;
                                                        updatingLegacyURLPath = true;
                                                    }
                                                }

                                                //if we haven't found the original asset, it doesn't matter if its mapped to a new asset, find anything with the same name
                                                if (!foundNewPath && !assetFound)
                                                {
                                                    //check to see if another file with the same name and case is found in the file system 
                                                    string match = vamFileListPaths.FirstOrDefault(element => element.EndsWith(Path.GetFileName(assetPath)));
                                                    if (match != "" && match != null)
                                                    {
                                                        if (File.Exists(match) || match.Contains(":/"))
                                                        {
                                                            //match is an absolute path, need the relative path
                                                            newRelativePath = MakeVaMRelativePath(VAM_FOLDER, match);
                                                            fileOfSameName = true;
                                                            foundNewPath = true;
                                                        }
                                                    }

                                                    if (!foundNewPath)
                                                    {
                                                        //maybe the same file but case insensitive?
                                                        match = vamFileListPaths.FirstOrDefault(element => element.EndsWith(Path.GetFileName(assetPath), StringComparison.CurrentCultureIgnoreCase));
                                                        if (match != "" && match != null)
                                                        {
                                                            if (File.Exists(match) || match.Contains(":/"))
                                                            {
                                                                //match is an absolute path, need the relative path
                                                                newRelativePath = MakeVaMRelativePath(VAM_FOLDER, match);
                                                                fileOfSameName = true;
                                                                foundNewPath = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (newRelativePath != "" && newRelativePath != rawPath)
                                                {
                                                    bool doChange = true;

                                                    string updatedLine = line.Replace('"' + rawPath + '"', '"' + newRelativePath + '"');

                                                    if (!autoUpdateJSONFiles)
                                                    {
                                                        doChange = false;
                                                        MessageBoxResult messageResult = MessageBox.Show("Do you want to replace:\n\n\"" + rawPath + "\"\n\nwith\n\n\"" + newRelativePath + "\"\n\nIn line " + lineNumber + " of file:\n" + filePath + "?\n\nPRESS CANCEL TO STOP PROMPTING FOR EACH FILE.\nAFTER PRESSING \"Cancel\" YOU CAN PRESS \"Yes\" TO CHANGE ALL, OR \"No\" TO CHANGE NONE.", "VACUUM", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                                                        if (messageResult == MessageBoxResult.Cancel)
                                                        {

                                                            if (MessageBox.Show("Press \"No\" to stop processing this list of files (no further changes).\n\nPress \"Yes\" to process all files without prompting (change all).",
                                                                "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                                                            {
                                                                doChange = true;
                                                                autoUpdateJSONFiles = true;
                                                            }
                                                            else
                                                            {
                                                                updateJSONFiles = false;
                                                            }
                                                        }
                                                        else if (messageResult == MessageBoxResult.Yes)
                                                        {
                                                            doChange = true;
                                                        }
                                                    }

                                                    if (doChange)
                                                    {
                                                        if (fileOfSameName)
                                                        {
                                                            AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                                "\nREPLACING MISSING FILE WITH ANOTHER FILE OF THE SAME NAME\nTHIS FILE MAY BE DIFFERENT, BUT PROBABLY WORTH REPLACING THIS REFERENCE AS THE OTHERWISE THERE WILL BE A MISSING DEPENDENCY IN A JSON SAVES OR CLOTHING STYLE FILE ." +
                                                                "\nReplaced: " + '"' + rawPath + "\" with \"" + newRelativePath + '"' +
                                                                "\nUpdated line " + lineNumber + " text now reads: " + updatedLine + "\n\n");
                                                        }
                                                        else if (updatingLegacyURLPath)
                                                        {
                                                            AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                                "\nURL paths should be relative to a known root folder, such as Saves or Custom. URL paths can fail if they are in relation to the containing .json file folder (texfolder/mytex.jpg can fail)." +
                                                                "\nReplaced: " + '"' + rawPath + "\" with \"" + newRelativePath + '"' +
                                                                "\nUpdated line " + lineNumber + " text now reads: " + updatedLine + "\n\n");
                                                        }
                                                        else
                                                        {
                                                            AppendLogFile("_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt", "File: " + filePath +
                                                                "\nReplaced: " + '"' + rawPath + "\" with \"" + newRelativePath + '"' +
                                                                "\nUpdated line " + lineNumber + " text now reads: " + updatedLine + "\n\n");
                                                        }
                                                        newFile.WriteLine(updatedLine);
                                                        changedLine = true;
                                                        changed = true;
                                                        assetFound = true; //I guess now we are pointing to a VAR file resource or other duplicate that is found
                                                        assetPath = MakeVaMAbsolutePath(VAM_FOLDER, Path.GetDirectoryName(filePath), newRelativePath); //must update the found asset, so that we don't end up ignoring files we should be deleting
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //if the quoted line is displayName, this isn't a real asset!
                                    if (!line.Contains("\"displayName\"") && !line.Contains("\"audioClip\""))
                                    {
                                        //for now we only consider an asset missing if we could update it to fix it
                                        if (!assetFound && editThisFile)
                                        {
                                            //json files in Custom are often used by scripts, cs files often contains partial paths
                                            if (missingAssets.ContainsKey(filePath))
                                            {
                                                missingAssets[filePath].Add(assetPath);
                                            }
                                            else
                                            {
                                                List<string> newList = new List<string>() { assetPath };
                                                missingAssets.Add(filePath, newList);
                                            }
                                        }

                                        //if we changed the reference, we also changed assetPath and are now adding the new asset
                                        requiredAssets.Add(assetPath);

                                        if (savefileAssets.ContainsKey(filePath))
                                        {
                                            savefileAssets[filePath].Add(assetPath);
                                        }
                                        else
                                        {
                                            List<string> newList = new List<string>() { assetPath };
                                            savefileAssets.Add(filePath, newList);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (editThisFile && updateJSONFiles && !changedLine)
                    {
                        if (!changedLine) newFile.WriteLine(line);
                    }
                }

                if (newFile != null)
                {
                    newFile.Close();
                }

                if (changed)
                {
                    safeDeletePrompt = false; //already prompting for each change, and if not changed, won't delete
                    if (File.Exists(filePath) && filePath.StartsWith(VAM_FOLDER) && File.Exists(tempFile))
                    {
                        File.Delete(filePath);
                        File.Move(tempFile, filePath);
                    }
                }
                else
                {
                    //delete the temp file
                    if (File.Exists(tempFile) && tempFile.StartsWith(VAM_FOLDER))
                    {
                        File.Delete(tempFile);
                    }
                }
            }
            //only unique values, by converting to hashset and back
            requiredAssets = new List<string>(new HashSet<string>(requiredAssets));

            //duplicates shouldn't include thumbnails
            duplicateNameFilesToDelete = duplicateNameFilesToDelete.Except(thumbnailAssets, StringComparer.OrdinalIgnoreCase).ToList();

            //duplicates shoudn't include assets that are referenced
            duplicateNameFilesToDelete = duplicateNameFilesToDelete.Except(requiredAssets).ToList();

            //duplicates shouldn't include files we aren't supposed to delete
            duplicateNameFilesToDelete = RemoveNeverDeleteFiles(duplicateNameFilesToDelete);

            //single copy of neverDeleteFiles
            neverDeleteFilePaths = new List<string>(new HashSet<string>(neverDeleteFilePaths));

            SetWindowTitle("Writing log files");
            thumbnailAssets.Sort();
            requiredAssets.Sort();
            resourceFilePaths.Sort();
            neverDeleteFilePaths.Sort();
            WriteLogFile("All_JSON_File_Thumbnails.txt", string.Join("\n", thumbnailAssets.ToArray()));
            WriteLogFile("Used_Resources_List.txt", string.Join("\n", requiredAssets.ToArray()));
            WriteLogFile("All_Managed_Resources (images audio and assetbundles).txt", string.Join("\n", resourceFilePaths.ToArray()));
            WriteLogFile("All_Duplicate_Unused_Non_VAR_Resources.txt", string.Join("\n", duplicateNameFilesToDelete.ToArray()));
            WriteLogFile("All_Protected_Files.txt", string.Join("\n", neverDeleteFilePaths.ToArray())); //WRITE THIS EARLIER IN CASE WE WANT TO REVIEW PROTECTED FILES BEFORE A DEEP CLEAN

            string requiredUIDFilesLog = "";
            foreach (KeyValuePair<string, List<string>> requiredUIDFile in requiredUIDFiles)
            {
                requiredUIDFilesLog += "Clothing / Hair UID: " + requiredUIDFile.Key + "\nUsed by Styles / Presets:\n" + string.Join("\n", requiredUIDFile.Value.ToArray()) + "\n\n";
            }
            WriteLogFile("Used_Clothing_and_Hair_UIDs.txt", requiredUIDFilesLog);

            string missingAssetsLog = "";
            foreach (KeyValuePair<string, List<string>> missingAsset in missingAssets)
            {
                missingAssetsLog += missingAsset.Key + "\nREFERENCES MISSING ASSETS:\n" + string.Join("\n", missingAsset.Value.ToArray()) + "\n\n";
            }
            WriteLogFile("Missing_Resources.txt", missingAssetsLog);

            SetWindowTitle("Writing File Dependencies on other Files log.");
            string saveFileAssetsLog = "";
            foreach (KeyValuePair<string, List<string>> saveFileAsset in savefileAssets)
            {
                saveFileAssetsLog += saveFileAsset.Key + "\nREFERENCES ASSETS:\n" + string.Join("\n", saveFileAsset.Value.ToArray()) + "\n\n";
            }
            WriteLogFile("Used_Resources_For_Each_Save_File (Sorted by JSON Scene, etc).txt", saveFileAssetsLog);


            SetWindowTitle("Sorting Morphs");
            existingMorphs = existingMorphs.Distinct().ToList();
            desiredMorphs = desiredMorphs.Distinct().ToList();
            List<string> unusedMorphs = existingMorphs.Except(desiredMorphs).ToList();
            List<string> missingMorphs = desiredMorphs.Except(existingMorphs).ToList();
            missingMorphs = missingMorphs.Except(builtInMorphs).ToList();

            desiredMorphs.Sort();
            unusedMorphs.Sort();
            WriteLogFile("Unused_Morphs.txt", string.Join("\n", unusedMorphs.ToArray()));
            WriteLogFile("Used_Morphs.txt", string.Join("\n", desiredMorphs.ToArray()));

            string missingMorphsLog = "Missing Morphs is a work in progress. VACUUM looks for morphs in VAR Packages and your Custom/Atom/Person/Morphs folder, but not in your Saves folder. Therefore, morphs marked as missing may actually be located in your Saves folder.\n\n";
            int curFileNumber = 0;
            int totalFileNumber = missingMorphs.Count;
            foreach (string missingMorph in missingMorphs)
            {
                curFileNumber++;
                SetWindowTitle("(" + curFileNumber + "/" + totalFileNumber + ") Logging Missing Morph - " + missingMorph);
                if (filesReferenceMorph.ContainsKey(missingMorph))
                {
                    missingMorphsLog += "MISSING Morph ID: " + missingMorph + "\nUsed by:\n" + string.Join("\n", filesReferenceMorph[missingMorph].ToArray()) + "\n\n";
                }
            }
            WriteLogFile("Missing_Morphs.txt", missingMorphsLog);

            List<string> unusedMorphFiles = new List<string>();
            foreach (string unusedMorph in unusedMorphs)
            {
                if (!unusedMorphs.Contains(":/")) { 
                    if (morphIDToFiles.ContainsKey(unusedMorph))
                    {
                        foreach (string morphPath in morphIDToFiles[unusedMorph])
                        {
                            if (!morphPath.Contains("TenStrip") && !morphPath.Contains("AshAuryn"))
                            {
                                unusedMorphFiles.Add(morphPath);
                            }
                        }
                    }
                }
            }
            unusedMorphFiles = unusedMorphFiles.Distinct().ToList();
            unusedMorphFiles.Sort();
            WriteLogFile("Unused_Morph_Files.txt", string.Join("\n", unusedMorphFiles.ToArray()));

            SetWindowTitle("Sorting and Logging Unused Assets");
            List<string> unusedAssets = resourceFilePaths.Except(requiredAssets).ToList();
            unusedAssets = unusedAssets.Except(thumbnailAssets, StringComparer.OrdinalIgnoreCase).ToList();
            unusedAssets = RemoveNeverDeleteFiles(unusedAssets);

            Dictionary<string, List<string>> unusedAssetsByType = new Dictionary<string, List<string>>();
            Dictionary<string, long> unusedAssetSize = new Dictionary<string, long>();
            foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
            {
                unusedAssetsByType.Add(assetType.Key, new List<string>());
                unusedAssetSize.Add(assetType.Key, 0);
            }

            //unused assets in the save folder should be deleted
            List<string> unusedAssetsInSaves = new List<string>();

            //unused image assets in the Clothing and Hair folders should be deleted
            List<string> unusedImagesInClothingAndHair = new List<string>();
            string custom_clothing_folder = CUSTOM_FOLDER + "\\Clothing";
            string custom_hair_folder = CUSTOM_FOLDER + "\\Hair";

            //all other unused assets by category
            foreach (string unusedAsset in unusedAssets)
            {
                if (unusedAsset.StartsWith(SAVES_FOLDER + "\\"))
                {
                    unusedAssetsInSaves.Add(unusedAsset);
                }

                string extension = GetFileExtension(unusedAsset);
                foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
                {
                    foreach (string categorizedExtension in assetType.Value)
                    {
                        if (assetType.Key == IMAGE_TYPE)
                        {
                            if (unusedAsset.StartsWith(custom_clothing_folder) || unusedAsset.StartsWith(custom_hair_folder))
                            {
                                unusedImagesInClothingAndHair.Add(unusedAsset);
                            }
                        }
                        if (extension == categorizedExtension)
                        {
                            unusedAssetsByType[assetType.Key].Add(unusedAsset);
                        }
                    }
                }
            }


            unusedImagesInClothingAndHair = new List<string>(new HashSet<string>(unusedImagesInClothingAndHair));


            SetWindowTitle("Calculating Size of Unused Assets");
            FileInfo fi;

            long unusedAssetsInSavesSize = 0;
            foreach (string unusedFile in unusedAssetsInSaves)
            {
                if (File.Exists(unusedFile)) { 
                    fi = new FileInfo(unusedFile);
                    unusedAssetsInSavesSize += fi.Length;
                }
            }

            long unusedImagesInClothingAndHairSize = 0;
            foreach (string unusedFile in unusedImagesInClothingAndHair)
            {
                if (File.Exists(unusedFile))
                {
                    fi = new FileInfo(unusedFile);
                    unusedImagesInClothingAndHairSize += fi.Length;
                }
            }

            foreach (KeyValuePair<string, List<string>> unusedAssetType in unusedAssetsByType)
            {
                if (duplicateAssetTypesToCheck.Contains(unusedAssetType.Key))
                {
                    long totalSize = 0;
                    foreach (string unusedFile in unusedAssetType.Value)
                    {
                        if (File.Exists(unusedFile))
                        {
                            fi = new FileInfo(unusedFile);
                            totalSize += fi.Length;
                        }
                    }
                    unusedAssetSize[unusedAssetType.Key] = totalSize;
                    WriteLogFile("Unused_" + unusedAssetType.Key + "_Resources.txt", string.Join("\n", unusedAssetType.Value.ToArray()));
                }
            }

            SetWindowTitle("Logging Unused Assets");
            WriteLogFile("Unused_Managed_Resources_In_Saves.txt", string.Join("\n", unusedAssetsInSaves.ToArray()));
            WriteLogFile("Unused_Images_In_Clothing_and_Hair_Folders.txt", string.Join("\n", unusedImagesInClothingAndHair.ToArray()));


            SetWindowTitle("Sorting Unused Clothing and Hair");

            //CURRENTLY MOST UNUSED AND USED CLOTHING ARE BASED ON IF THEY ARE USED BY PRESETS, NOT USED BY SCENES, FIX THIS!
            //EVENTUALLY EXPAND THIS TO BE ABLE TO DELETE UNUSED ITEMS
            List<string> clothesUsedByScenes = new List<string>();
            foreach (string requiredAssetCheck in requiredAssets)
            {
                if (requiredAssetCheck.EndsWith(".vam"))
                {
                    clothesUsedByScenes.Add(requiredAssetCheck);
                }
            }
            clothesUsedByScenes = clothesUsedByScenes.Distinct().ToList();
            clothesUsedByScenes.Sort();
            WriteLogFile("Used_by_Scene_Clothing_and_Hair.txt", string.Join("\n", clothesUsedByScenes.ToArray()));

            //only unique values, by converting to hashset and back
            requiredUIDs = new List<string>(new HashSet<string>(requiredUIDs));
            List<string> unusedUIDs = assetUIDs.Except(requiredUIDs).ToList();

            List<string> missingUIDs = requiredUIDs.Except(assetUIDs).ToList();
            missingUIDs = new List<string>(new HashSet<string>(missingUIDs));

            List<string> newMissingUIds = new List<string>();
            foreach (string missingUID in missingUIDs)
            {
                //can reference an exact file
                bool missing = true;
                if (File.Exists(missingUID))
                {
                    //don't add to the missing list
                    missing = false;
                } else if (missingUID.Contains(":/") && VARPackageFileExists(missingUID))
                {
                    missing = false;
                }
                if (missing) newMissingUIds.Add(missingUID);
            }
            missingUIDs = newMissingUIds;

            //we are not missing the builtin clothing
            missingUIDs = missingUIDs.Except(builtinUIDs).ToList(); //THIS IS AN INCOMPLETE AND CHANGING LIST, BUT REMOVING SOME OF IT WILL MAKE IT EASIER TO WADE THROUGH THE MISSING FILES

            SetWindowTitle("Logging Unused Clothing and Hair");
            requiredAssets.Sort();
            unusedUIDs.Sort();
            missingUIDs.Sort();
            WriteLogFile("Unused_Clothing_and_Hair_UIDs.txt", string.Join("\n", unusedUIDs));
            //WriteLogFile("Missing_or_Builtin_Clothing_and_Hair.txt", string.Join("\n", missingUIDs));

            string missingUIDFilesLog = "";
            missingUIDFilesLog += "It's possible that if an item is built in to VAM it would show up here (most built in items are automatically removed).\n\n";
            foreach (KeyValuePair<string, List<string>> requiredUIDFile in requiredUIDFiles)
            {
               if (missingUIDs.Contains(requiredUIDFile.Key))
                {
                    missingUIDFilesLog += "MISSING Clothing / Hair UID: " + requiredUIDFile.Key + "\nUsed by:\n" + string.Join("\n", requiredUIDFile.Value.ToArray()) + "\n\n";
                }                
            }
            WriteLogFile("Missing_Clothing_and_Hair.txt", missingUIDFilesLog);
            
            foreach (string assetType in duplicateAssetTypesToCheck)
            {
                List<string> duplicateAssets = new List<string>();
                foreach (string fileName in duplicateNameFilesToDelete)
                {
                    foreach (string extension in assetTypes[assetType])
                    {
                        if (fileName.EndsWith(extension))
                        {
                            duplicateAssets.Add(fileName);
                        }
                    }
                }
                string infoText = "This is a list of unused duplicate " + assetType + " files.\n\nAll of these are both unused, and match another (either used or unused) file with the same name and size.\n\nTo find the path of the identical file that will be kept, search the Duplicate_" + assetType + "_Files.txt file for the file name.";
                WriteLogFile("Unused_Duplicate_" + assetType + "_Files.txt", infoText + "\n\n" + string.Join("\n", duplicateAssets.ToArray()));

                if (duplicateAssets.Count > 0)
                {
                    if (easyClean || MessageBox.Show("Would you like to review UNUSED dupliate " + assetType + " files for possible deletion?" +
                        "\n\n\nThese are " + assetType + " files that are not used in any scene, appearance, hairstyle, etc, and are also one of two or more copies of identical files in your VAM folder." +
                        "\n\nA list of all duplicates is at:\n" + LOG_FOLDER + "\\Unused_Duplicate_" + assetType + "_Files.txt", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        ResetSafeDeletePrompt();
                        foreach (string path in duplicateAssets)
                        {
                            if (SafeDelete(path, "Unused Duplicate " + assetType + " File") < 0) break;
                        }
                        ResetSafeDeletePrompt();
                        if (!easyClean) MessageBox.Show("Finished processing files with duplicate UIDs.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    if (!easyClean) MessageBox.Show("You have no unused duplicate " + assetType + " files.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            SetWindowTitle("Reviewing Unused Files");

            //DELETE UNUSED FILES
            if (easyClean || MessageBox.Show("Would you like to review unused files for possible deletion?" +
                "\n\n\nIf you would like to reduce the size of your VAM folder you can delete assets that are not used in any scene, appearance, preset, etc." +
                "\n\nFiles are never deleted from VAR Packages or Custom/Scripts." +
                "\n\nFor example, many users don't manually add CustomUnityAssets to scenes, and therefore don't use Assetbundles. So for these users, Assetbundles which are not used in any existing scenes are probably not very important and may be deleted to save space."
                , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                if (unusedAssetsInSaves.Count > 0)
                {
                    stopWatch.Stop();
                    if (MessageBox.Show(
                        "Would you like to delete unused resources from your Saves folder with total size " + GetReadableFileSize(unusedAssetsInSavesSize) + "?" +
                        "\n\n\nYou have unused resources (with any of these extensions: " + string.Join(", ", duplicateAssetExtensionsToCheck) + ") in your VaM/Saves folder." +
                        "\n\nThese are probably safe to delete, as files that are not used in Scenes should be found in the VaM/AddonPackages or VaM/Custom folders." +
                        "\n\nIf you saved screenshots into your Saves folder, these will be deleted." +
                        "\n\nCheck the list of files delete in " + LOG_FOLDER + "\\Unused_Managed_Resources_In_Saves.txt"
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        ResetSafeDeletePrompt();
                        foreach (string path in unusedAssetsInSaves)
                        {
                            if (SafeDelete(path, "Unused Resource from Saves Folder") < 0) break;
                        }
                        ResetSafeDeletePrompt();
                        if (!easyClean) MessageBox.Show("Finished processing unused asset files in Saves folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    stopWatch.Start();
                }
                else
                {
                    if (!easyClean) MessageBox.Show("You have no unused assets in your VaM Saves folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                }


                if (unusedImagesInClothingAndHair.Count > 0)
                {
                    stopWatch.Stop();
                    if (MessageBox.Show(
                        "Would you like to delete unused images from your Custom/Clothing and Custom/Hair folders (with total size " + GetReadableFileSize(unusedImagesInClothingAndHairSize) + ")?" +
                        "\n\n\nYou have unused image files, that are in your Clothing or Hair folders, but are not used by any Clothing or Hair." +
                        "\n\nThese are probably safe to delete, as you will probably not use files that are in your Clothing and Hair folders but aren't used by any Clothing or Hair presets." +
                        "\n\nIf you plan to manually load textures into clothing and hair (you create clothing / hair), you should press No to save these files." +
                        "\n\nCheck the list of files delete in " + LOG_FOLDER + "\\Unused_Images_In_Clothing_and_Hair_Folders.txt"
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        ResetSafeDeletePrompt();
                        foreach (string path in unusedImagesInClothingAndHair)
                        {
                            if (SafeDelete(path, "Unused Image from Clothing or Hair folder.") < 0) break;
                        }
                        ResetSafeDeletePrompt();
                        if (!easyClean) MessageBox.Show("Finished processing unused image files from Clothing and Hair folders.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    stopWatch.Start();
                }
                else
                {
                    if (!easyClean) MessageBox.Show("You have no unused images in your VaM Clothing or Hair folders.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (!easyClean && unusedMorphFiles.Count > 0)
                {
                    stopWatch.Stop();
                    if (MessageBox.Show(
                        "Would you like to delete " + unusedMorphFiles.Count + " unused morphs from your Custom/Atom/Person/Morphs folder?" +
                        "\n\n\nUnless you are a content creator, you probably won't use morphs that are not already used in any scene / appearance." +
                        "\n\nSome plugins may require morphs that are seen by VACUUM as unused, so it's possible by deleing all unused morphs a plugin will throw an error and you would need to download and install that plugin again. Common morphs used by plugins, including all AshAuryn and TenStrip morphs, will not be deleted." +
                        "\n\nCheck the list of files delete in " + LOG_FOLDER + "\\Unused_Morph_Files.txt"
                        , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        ResetSafeDeletePrompt();
                        foreach (string path in unusedMorphFiles)
                        {
                            if (SafeDelete(path, "Unused Morph") < 0) break;
                            if (SafeDelete(path.Replace(".vmi", ".vmb"), "Unused Morph") < 0) break;
                            if (SafeDelete(path.Replace(".vmi", ".dsf"), "Unused Morph") < 0) break;
                        }
                        ResetSafeDeletePrompt();
                        if (!easyClean) MessageBox.Show("Finished processing unused morph files.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    stopWatch.Start();
                }
                else
                {
                    if (!easyClean) MessageBox.Show("You have no unused morphs in your Custom/Atom/Person/Morphs folder.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (!easyClean)
                {
                    foreach (KeyValuePair<string, List<string>> unusedAssetType in unusedAssetsByType)
                    {
                        if (duplicateAssetTypesToCheck.Contains(unusedAssetType.Key))
                        {
                            if (unusedAssetType.Value.Count > 0)
                            {
                                if (MessageBox.Show("BE CAREFUL! PRESSING \"Yes\" WILL DELETE " + unusedAssetType.Key.ToUpper() + " FILES THAT ARE NOT CURRENTLY IN SCENES / LOOKS / REFERENCED IN OTHER FILES!" +
                                "\n\nWould you like to delete unused " + unusedAssetType.Key + " files with total size " + GetReadableFileSize(unusedAssetSize[unusedAssetType.Key]) + "?" +
                                "\n\n\nPRESS \"No\" UNLESS YOU KNOW WHAT YOU ARE DOING! ALTHOUGH THESE FILES AREN'T CURRENTLY USED IN SCENES, IF YOU DELETE THEM YOU WILL HAVE FEWER " + unusedAssetType.Key.ToUpper() + " ITEMS TO USE / ADD TO SCENES IN VAM." +
                                "\n\n\n" + unusedAssetType.Key + " files from AddonPackages (VAR) won't be deleted." +
                                "\n\nREVIEW THE LIST OF FILES TO BE DELETED BEFORE YOU PRESS \"Yes\" in: " + LOG_FOLDER + "\\Unused_" + unusedAssetType.Key + "_Resources.txt"
                                 , "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                                {
                                    ResetSafeDeletePrompt();
                                    foreach (string path in unusedAssetType.Value)
                                    {
                                        if (SafeDelete(path, "Unused " + unusedAssetType.Key + " File") < 0) break;
                                    }
                                    ResetSafeDeletePrompt();
                                    MessageBox.Show("Finished processing unused " + unusedAssetType.Key + " files.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("You have no unused " + unusedAssetType.Key + " files.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }


            AppendErrorLog(static_error_log);
            static_error_log = "";
            ResetWindowTitle();


            stopWatch.Stop();
            stopWatchRunning = false;
            TimeSpan timeSpan = stopWatch.Elapsed;
            string cleaningTime = string.Format("{0} hours {1} minutes {2} seconds",
                     (int)timeSpan.TotalHours,
                     timeSpan.Minutes,
                     timeSpan.Seconds);


            WriteLogFile("_SUMMARY (size reduction and time elapsed).txt", "Deleted files with a total size of " + GetReadableFileSize(totalDeletedFileSize) +
                ".\n\nCleaning took: " + cleaningTime + " (may include time with dialog boxes displayed)." +
                "\n\nFinished: " + DateTime.Now.ToString("dd MMMM yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture));

            MessageBox.Show("Finished cleaning your VAM Folder." +
                "\n\nCleaning took: " + cleaningTime + " (may include time with dialog boxes displayed)." +
                "\n\n\nEdits made to JSON files were added to the _CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS file located at:\n\n" + LOG_FOLDER + "\\_CHANGE_LOG_JSON_AND_STYLE_FILE_EDITS (append log).txt" +
                "\n\n\nDeleted files (total of " + GetReadableFileSize(totalDeletedFileSize) + ") and other changes were added to the _CHANGE_LOG_DELETED_FILES file located at:\n\n" + LOG_FOLDER + "\\_CHANGE_LOG_DELETED_FILES (append log).txt"
                , "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        static void AppendErrorLog(string text)
        {
            AppendLogFile("_ERROR_LOG (append log).txt", text);
        }
        static void AppendDeletedLog(string text)
        {
            AppendLogFile("_CHANGE_LOG_DELETED_FILES (append log).txt", text);
        }

        private static string GetFileExtension(string path)
        {
            try
            {
                if (path == null || path == "")
                {
                    return "";
                }
                return Path.GetExtension(path).Substring(1);
            }
            catch (Exception ex)
            {
                static_error_log += "\nFailed to get extension. Error: " + ex.Message + ". Path: " + path;
                return "";
            }
        }

        private static List<string> RemoveNeverDeleteFiles(List<string> sourceList)
        {
            List<string> processedList = new List<string>(sourceList);
            foreach (string neverDeleteFileName in neverDeleteFiles)
            {
                processedList.RemoveAll(x => x.EndsWith(neverDeleteFileName));
            }

            //assets in certain folders aren't unused, they should be kept
            foreach (string neverDeleteFolder in neverDeleteFolders)
            {
                processedList.RemoveAll(x => x.StartsWith(neverDeleteFolder));
            }

            //assets of certain extensions should never be deleted
            foreach (string neverDeleteExtension in neverDeleteExtensions)
            {
                processedList.RemoveAll(x => x.EndsWith("." + neverDeleteExtension));
            }

            return processedList;
        }

        private static void ResetSafeDeletePrompt()
        {
            safeDeletePrompt = true;
        }

        private static int SafeDelete(string path, string explanation)
        {
            if (File.Exists(path) && path.StartsWith(VAM_FOLDER))
            {
                FileInfo fi;
                fi = new FileInfo(path);
                long deletedFileSize = fi.Length;

                if (safeDeletePrompt && !easyClean)
                {
                    MessageBoxResult messageResult = MessageBox.Show("Do you want to delete file " + path + "?\n\nPRESS CANCEL TO STOP PROMPTING FOR EACH FILE. AFTER PRESSING \"Cancel\" YOU CAN PRESS \"Yes\" TO DELETE ALL, OR \"No\" TO DELETE NONE.", "VACUUM", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                    if (messageResult == MessageBoxResult.Cancel)
                    {
                        if (MessageBox.Show("Press \"No\" to stop processing this list of files (no further deletions).\n\nPress \"Yes\" to process all files without prompting (delete all).",
                                            "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            safeDeletePrompt = false;
                            File.Delete(path);
                            AppendDeletedLog("\nDeleted: " + path + "\nSize: " + GetReadableFileSize(deletedFileSize) + " - Reason: " + explanation + "\n");
                            totalDeletedFileSize += deletedFileSize;
                            //if it's a vam file, first check to see if we should delete associated files
                            //this may come back and call safe delete, but not with a .vam file
                            if (path.EndsWith(".vam"))
                            {
                                if (DeleteAssociatedSettingsFiles(path) < 0) return -1;
                            }
                            return 1;
                        }
                        else
                        {
                            //if we return -1 stop processing!
                            return -1;
                        }
                    }
                    else if (messageResult == MessageBoxResult.Yes)
                    {
                        File.Delete(path);
                        AppendDeletedLog("\nDeleted: " + path + "\nSize: " + GetReadableFileSize(deletedFileSize) + " - Reason: " + explanation + "\n");
                        totalDeletedFileSize += deletedFileSize;
                        //if it's a vam file, first check to see if we should delete associated files
                        //this may come back and call safe delete, but not with a .vam file
                        if (path.EndsWith(".vam"))
                        {
                            if (DeleteAssociatedSettingsFiles(path) < 0) return -1;
                        }
                        return 1;
                    }
                }
                else
                {
                    File.Delete(path);
                    AppendDeletedLog("\nDeleted: " + path + "\nSize: " + GetReadableFileSize(deletedFileSize) + " - Reason: " + explanation + "\n");
                    totalDeletedFileSize += deletedFileSize;
                    //if it's a vam file, first check to see if we should delete associated files
                    //this may come back and call safe delete, but not with a .vam file
                    if (path.EndsWith(".vam"))
                    {
                        if (DeleteAssociatedSettingsFiles(path) < 0) return -1;
                    }
                    return 1;
                }
            }
            return 0;
        }
        public static async Task InitTask(MainWindow theMainWindow, string rootFolder)
        {
            stopWatch.Restart();
            stopWatchRunning = true;
            working = true;
            ResetEverything();
            await Task.Run(() => Init(theMainWindow, rootFolder));
            working = false;
        }
        public static async Task DeleteDuplicateUIDsTask()
        {
            stopWatch.Restart();
            stopWatchRunning = true;
            working = true;
            ResetEverything();
            await Task.Run(() => DeleteDuplicateUIDs());
            working = false;
        }
        public static async Task ProcessSourceFilesTask()
        {
            stopWatch.Restart();
            stopWatchRunning = true;
            working = true;
            ResetEverything();
            await Task.Run(() => ProcessSourceFiles());
            working = false;
        }

        public static bool Working()
        {
            return working;
        }

        public static void Init(MainWindow theMainWindow, string rootFolder)
        {
            mainWindow = theMainWindow;

            VAM_FOLDER = rootFolder;
            CUSTOM_FOLDER = VAM_FOLDER + "\\Custom";
            SAVES_FOLDER = VAM_FOLDER + "\\Saves";
            SCRIPTS_FOLDER = VAM_FOLDER + "\\Custom\\Scripts";
            PACKAGES_FOLDER = VAM_FOLDER + "\\AddonPackages";
            LOG_FOLDER = VAM_FOLDER + "\\_VACUUM_LOGS";
            LOG_FOLDER_LAST = VAM_FOLDER + "\\_VACUUM_LOGS\\_LOG_HISTORY";
            MORPHS_FOLDER = VAM_FOLDER + "\\Custom\\Atom\\Person\\Morphs";

            Directory.CreateDirectory(LOG_FOLDER);
            Directory.CreateDirectory(LOG_FOLDER_LAST);

            //set settings for VAMFile
            neverDeleteFiles.Clear();
            neverDeleteFiles.Add("PoolGarden.scene"); //for the EasyMate tutorial, we ask people to use an asset that isn't in any scene, so leave it there
                                                      //more files will be added to this list dynamically, files with these names won't be deleted from any folder

            //don't delete files from the scripts folder
            neverDeleteFolders.Clear();
            neverDeleteFolders.Add(PACKAGES_FOLDER);
            neverDeleteFolders.Add(SCRIPTS_FOLDER);
            neverDeleteFolders.Add(VAM_FOLDER + "\\Custom\\Assets\\Audio");
            neverDeleteFolders.Add(VAM_FOLDER + "\\Custom\\Assets\\Audio\\E-Motion");
            neverDeleteFolders.Add(VAM_FOLDER + "\\Custom\\Assets\\Audio\\RT_LipSync");
            neverDeleteFolders.Add(VAM_FOLDER + "\\Custom\\Audio\\Dollmaster Personas");

            //never delete extensions are only checked to see if the file is in the right path, and only if missing do we try to find a replacement file from our found assets
            //scripts and such are already never deleted, ony image, audio, or assetbundle extensions we don't want deleted need to be mentioned here
            //don't delete tif files
            //tif files are a bit worrysome as two files can have identical size (and identical names, like faceN.tif) but different contents,
            //we won't delete these ever, or change file paths for them, but we will search for them in case one is used but not found at that
            //location, or fix location pointers to full relative paths from custom/saves
            neverDeleteExtensions.Clear();
            neverDeleteExtensions.Add("tif");

            //same list, but grouped
            assetTypes.Clear();
            assetTypes.Add(IMAGE_TYPE, new List<string>() { "jpg", "jpeg", "tif", "png" });
            assetTypes.Add(AUDIO_TYPE, new List<string>() { "mp3", "ogg", "wav" });
            assetTypes.Add(ASSETBUNDLE_TYPE, new List<string>() { "assetbundle", "scene" });
            assetTypes.Add(VAM_TYPE, new List<string>() { "vam" });
            assetTypes.Add(VAM_STYLE_TYPE, new List<string>() { "vap", "vaj" });
            assetTypes.Add(SCRIPT_TYPE, new List<string>() { "cs", "cslist" });
            assetTypes.Add(VAC_TYPE, new List<string>() { "vac" });

            duplicateAssetTypesToCheck = new List<string>() { IMAGE_TYPE, AUDIO_TYPE, ASSETBUNDLE_TYPE }; 

            assetExtensions.Clear();
            foreach (KeyValuePair<string, List<string>> assetType in assetTypes)
            {
                foreach (string extension in assetType.Value)
                {
                    assetExtensions.Add(extension);

                    //list of only image, audio, and assetbundles, as these work differently in terms of duplication checks, as they are not ID based
                    if (duplicateAssetTypesToCheck.Contains(assetType.Key))
                    {
                        duplicateAssetExtensionsToCheck.Add(extension);
                    }
                }
            }


            //IMPORTANT TO add custom first as it makes it easy to choose from the custom by selecting the first found instance of a file
            assetFolders.Clear();
            assetFolders.Add(CUSTOM_FOLDER);
            assetFolders.Add(SAVES_FOLDER);

            savefileExtensions.Clear();
            savefileExtensions.Add("json");
            savefileExtensions.Add("cs");
            savefileExtensions.Add("vaj");
            savefileExtensions.Add("vap");

            //for all versions of vam, although I think this became a problem only in 1.18, so we may break 1.17 if people are still using it
            morphReplace.Add("CheekBones  Width Outer", "Cheek Bones Width Outer");

            //only for vam 1.19 and above
            isVAM19 = Directory.Exists(PACKAGES_FOLDER);
            morphReplace19.Add("Testes Height", "Testes_Height");     //vam 1.19 renamed what used to be "testes height" in 1.18 to testes_height   
            morphReplace19.Add("GA-Izarra", "Izarra Head"); //I THINK THIS IS RIGHT, SEEMED OK IN APPEARANCES I CHECKED, BOTH ARE BUILT-IN

            saveFolders.Clear();
            saveFolders.Add(CUSTOM_FOLDER);
            saveFolders.Add(SAVES_FOLDER);

            replaceClothingIDs.Clear();
            replaceClothingIDs.Add("SummerGirl Shorts", "SummerGirl Shorts Sim");
            replaceClothingIDs.Add("SummerGirl Shirt", "SummerGirl Shirt Sim");
            replaceClothingIDs.Add("Heat Up Panty", "Heat Up Panty Sim");
            replaceClothingIDs.Add("Heat Up Skirt", "Heat Up Skirt Sim");
            replaceClothingIDs.Add("Alphakini Panty", "Alphakini Panty Sim");
            replaceClothingIDs.Add("Alphakini Bra", "Alphakini Bra Sim");
            replaceClothingIDs.Add("Alphakini Panty Sim", "Alphakini Panty Sim"); //THESE DON'T CHANGE, BUT IF WE FIND STYLES WE NEED TO FIX THEM AS THEY ARE NONSTANDARD AND VACCUMM MESSED THEM UP
            replaceClothingIDs.Add("Alphakini Bra Sim", "Alphakini Bra Sim");
            replaceClothingIDs.Add("Heatwave Shirt", "Heatwave Shirt Sim");
            replaceClothingIDs.Add("DATM Jumper", "DATM Jumper Sim");
            replaceClothingIDs.Add("DATM Pants", "DATM Pants Sim");
            //replaceClothingIDs.Add("Stockings", "Stockings Sim"); //can't replace stockings as will mess up stocking styles, can improve this later to check for end quotes, but for now skip stockings
            replaceClothingIDs.Add("Seductress Dress", "Seductress Dress Sim");
            replaceClothingIDs.Add("Silky PJ Top", "Silky PJ Top Sim");
            replaceClothingIDs.Add("Silky PJ Bottom", "Silky PJ Bottom Sim");
            replaceClothingIDs.Add("SummerGirlShorts", "SummerGirlShortsSim");
            replaceClothingIDs.Add("SummerGirlShirt", "SummerGirlShirtSim");
            replaceClothingIDs.Add("HeatUpPanty", "HeatUpPantySim");
            replaceClothingIDs.Add("HeatUpSkirt", "HeatUpSkirtSim");
            //replaceClothingIDs.Add("AlphakiniPanty", "AlphakiniPantySim"); //THESE ARE WRONG, FOR WHATEVER REASON, SIM VERSION DON'T CONTAIN SIM
            //replaceClothingIDs.Add("AlphakiniBra", "AlphakiniBraSim");
            replaceClothingIDs.Add("AlphakiniPantySim", "AlphakiniPanty"); //THESE ARE WRONG, FOR WHATEVER REASON, SIM VERSION DON'T CONTAIN SIM
            replaceClothingIDs.Add("AlphakiniBraSim", "AlphakiniBra"); //EXCEPT THE ONE TIME WHERE IT IS EXACT
            replaceClothingIDs.Add("HeatwaveShirt", "HeatwaveShirtSim");
            replaceClothingIDs.Add("DATM_Jumper", "DATM_JumperSim");
            replaceClothingIDs.Add("DATM_Pants", "DATM_PantsSim");
            //replaceClothingIDs.Add("Stockings", "StockingsSim");
            replaceClothingIDs.Add("SeductressDress", "SeductressDressSim");
            replaceClothingIDs.Add("SilkyPjsCamisole", "SilkyPjsCamisoleSim");
            replaceClothingIDs.Add("SilkyPjsShorts", "SilkyPjsShortsSim");
            //replace SummerGirlShorts with SummerGirlsShortsSim for example in:
            /*{
                "id" : "SummerGirlShortsSimMaterial", 
               "Gloss" : "6.515841", 
               "Specular Color" : {
                    "h" : "0.5330379", 
                  "s" : "0", 
                  "v" : "1"
               }
            }, 
            {
                "id" : "SummerGirlShortsMaterial", 
               "Gloss Texture Offset" : "-0.4967158", 
               "Diffuse Color" : {
                    "h" : "0", 
                  "s" : "0.8069631", 
                  "v" : "0.8366281"
               }
            }, */

            builtinUIDs.Clear();
            builtinUIDs.Add("Ultra Cat Suit");
            builtinUIDs.Add("Errands Shoes");
            builtinUIDs.Add("ST Sneakers");
            builtinUIDs.Add("Avery Top");
            builtinUIDs.Add("SummerGirl Shorts");
            builtinUIDs.Add("SummerGirl Shirt");
            builtinUIDs.Add("Avery Skirt");
            builtinUIDs.Add("Red Rabbit Dress");
            builtinUIDs.Add("Miss Kringle Dress");
            builtinUIDs.Add("Little Flirt Tank Top");
            builtinUIDs.Add("Little Flirt Skirt");
            builtinUIDs.Add("Heat Up Cap");
            builtinUIDs.Add("Heat Up Top");
            builtinUIDs.Add("Heat Up Panty");
            builtinUIDs.Add("Heat Up Skirt");
            builtinUIDs.Add("Alphakini Panty");
            builtinUIDs.Add("Alphakini Bra");
            builtinUIDs.Add("Casual Denim Top");
            builtinUIDs.Add("Casual Denim Jeans");
            builtinUIDs.Add("Casual Denim Shoes");
            builtinUIDs.Add("Heatwave Hat");
            builtinUIDs.Add("Heatwave Sunglasses");
            builtinUIDs.Add("Heatwave Shirt");
            builtinUIDs.Add("Swim Wear 2 Top");
            builtinUIDs.Add("Swim Wear 2 Bottom");
            builtinUIDs.Add("Black Light Gloves");
            builtinUIDs.Add("Black Light Top");
            builtinUIDs.Add("Black Light Pants");
            builtinUIDs.Add("Santa Baby Hat");
            builtinUIDs.Add("Santa Baby Dress");
            builtinUIDs.Add("Harli Heels");
            builtinUIDs.Add("DATM Jumper");
            builtinUIDs.Add("DATM Pants");
            builtinUIDs.Add("Molten Upperarms");
            builtinUIDs.Add("Molten Neckpiece");
            builtinUIDs.Add("Molten Forearms");
            builtinUIDs.Add("Molten Shins");
            builtinUIDs.Add("Molten Thighs");
            builtinUIDs.Add("Molten Bodysuit");
            builtinUIDs.Add("Skull Queen Pauldrons");
            builtinUIDs.Add("Skull Queen Top");
            builtinUIDs.Add("Skull Queen Panties");
            builtinUIDs.Add("Enchantress Sleeves");
            builtinUIDs.Add("Enchantress Skirt");
            builtinUIDs.Add("Enchantress Top");
            builtinUIDs.Add("Enchantress Panty");
            builtinUIDs.Add("Glasses");
            builtinUIDs.Add("Stockings");
            builtinUIDs.Add("Seductress Dress");
            builtinUIDs.Add("Silky PJ Top");
            builtinUIDs.Add("Silky PJ Bottom");
            builtinUIDs.Add("IC Bustier");
            builtinUIDs.Add("IC Bra");
            builtinUIDs.Add("IC Bra Full");
            builtinUIDs.Add("IC Shorts");
            builtinUIDs.Add("Thong");
            builtinUIDs.Add("Swim Trunks");
            builtinUIDs.Add("Tank Top");
            builtinUIDs.Add("Fancy Necklace");
            builtinUIDs.Add("Fancy Mask");
            builtinUIDs.Add("Fancy Bra");
            builtinUIDs.Add("Fancy Panties");
            builtinUIDs.Add("Simple Underwear");
            builtinUIDs.Add("Simple Underwear Shorts");
            builtinUIDs.Add("Simple Top");
            builtinUIDs.Add("Simple Shorts");
            builtinUIDs.Add("Shorts");
            builtinUIDs.Add("SummerGirl Shorts Sim");
            builtinUIDs.Add("SummerGirl Shirt Sim");
            builtinUIDs.Add("Heat Up Panty Sim");
            builtinUIDs.Add("Heat Up Skirt Sim");
            builtinUIDs.Add("Alphakini Panty Sim");
            builtinUIDs.Add("Alphakini Bra Sim");
            builtinUIDs.Add("Heatwave Shirt Sim");
            builtinUIDs.Add("DATM Jumper Sim");
            builtinUIDs.Add("DATM Pants Sim");
            builtinUIDs.Add("Stockings Sim");
            builtinUIDs.Add("Seductress Dress Sim");
            builtinUIDs.Add("Silky PJ Top Sim");
            builtinUIDs.Add("Silky PJ Bottom Sim");
            builtinUIDs.Add("Simple Underwear Sim");
            builtinUIDs.Add("Simple Top Sim");
            builtinUIDs.Add("Simple Underwear Shorts Sim");
            builtinUIDs.Add("Jeans");
            builtinUIDs.Add("Vest");
            builtinUIDs.Add("AI Shoes");
            builtinUIDs.Add("AI Pants");
            builtinUIDs.Add("AI Pants Down");
            builtinUIDs.Add("AI Shirt");
            builtinUIDs.Add("PonytailV3");
            builtinUIDs.Add("Ponytail");
            builtinUIDs.Add("TShirt");
            builtinUIDs.Add("Garden Party Dress");
            builtinUIDs.Add("Shoes");
            builtinUIDs.Add("NP Shirt");
            builtinUIDs.Add("AI Jacket");
            builtinUIDs.Add("Outpost Boots");
            builtinUIDs.Add("NP Pants");
            builtinUIDs.Add("NP Shoes");
            builtinUIDs.Add("Swim Trunks Bikini");
            builtinUIDs.Add("Clothing Creator");
            builtinUIDs.Add("Tyler Hair");
            builtinUIDs.Add("Soleil Hair");
            builtinUIDs.Add("SimV2 Hair Male");
            builtinUIDs.Add("SimV2 Hair");
            builtinUIDs.Add("Sim Hair 2");
            builtinUIDs.Add("Sim Hair");
            builtinUIDs.Add("Krayon Hair");
            builtinUIDs.Add("Scott Hair");
            builtinUIDs.Add("Chace Hair");
            builtinUIDs.Add("Fancy Hair");
            builtinUIDs.Add("No Hair");
            builtinUIDs.Add("Leslie Hair");
            builtinUIDs.Add("Omri Hair");
            builtinUIDs.Add("Flirty Hair");

            //THESE ARE MORPH IDs from VAM, use Custom/Scripts/Easy Mate/src/OutputBuiltinMorphsForVACUUM to generate this list, then replace all " with " in the list and format the start and end with { } to make this list well formatted:
            builtInMorphs = new List<string>() { "Adjust Back", "Adjust Front", "Incline", "Incline 2", "InOut", "Internal Thickness", "Move Back", "Move Front", "Offset", "Offset2", "OpenLarge", "OpenMedium", "OpenSmall", "OpenXXL", "Pucker 1", "Pucker 2", "Small", "Forearms Size", "Shoulders Size", "Shoulders Width", "Shoulders Width B", "Upper Arms Size", "Wrist Size", "Back Definition", "Lats Size", "Lower Back Dimple", "Sacral Dimples", "Androgynous", "Body Scale", "Body Size", "Body Tone", "Bodybuilder Details", "Bodybuilder Size", "Darius 6 Body", "Emaciated", "Fitness Details", "Fitness Size", "Gianni 6 Body", "Heavy", "Height", "Julian Body (JaR)", "Lee 6 Body", "Lee Body (JaR)", "Lower Body Length", "Michael 6 Body", "MVR_G2Female", "Portly", "Scott 6 Body", "Stocky", "Upper Body Length", "Upper Torso Length", "Areolae Depth", "Areolae Diameter", "Nipples", "Nipples Depth", "Nipples Diameter", "Nipples Large", "Nipples Size", "Collarbone Detail", "Costal Angle Arched", "Costal Angle Pointed", "Pectorals Cleavage", "Pectorals Diameter", "Pectorals Heavy", "Pectorals Height", "Pectorals Height Outer", "Pectorals Sag", "Pectorals Under Curve", "Ribcage Size", "Ribs Definition", "Ribs Width", "Scapula Depth", "Scapula Size", "Sternum Depth", "Sternum Height", "Sternum Width", "Torso Fitness", "Feet Arch", "Feet Thickness", "Foot Length", "Foot Length Left", "Foot Length Right", "Frsk_Incline", "Frsk_Left_Right", "PBMFS01", "PBMFS02", "PBMFS03", "PBMFS04", "PBMFS05", "Defined Head", "Foreskin Fold Adjust", "Gl_Height", "Gl_Width", "Glans_Fan", "Glans_Inflate", "Glans_Mushroom", "Glans_Pointed", "Glans_Tall", "Glans_Wide", "Glans-Shaft_Split", "Rim Thickness", "large nuts", "Sc_Pendulous", "Sc_Small", "Scrotum Pendulous", "Scrotum Taper", "Testes Thick", "Testes_Height", "Testes_ShaftSkin", "Testicle Height Left", "Testicle Height Right", "Base Left_Right", "Base Up_Down", "Curve Left_Right", "Curve Up_Down", "Curved", "Penis Length", "Penis Width", "Perineum_Inflate", "Shaft_Balloon", "Shaft_Base Thick", "Shaft_CurveDown-Up", "Shaft_CurveLeft-Right", "Shaft_Spongiosum", "Shaft_Spongiosum-Wide", "Genital Ribbed", "Jake Genital (JaR)", "Jeff Genital (JaR)", "Jimpei Genital (JaR)", "Johnny Genital (JaR)", "Julian Genital (JaR)", "Lee Genital (JaR)", "Seth Genital (JaR)", "Shane Genital (JaR)", "Hands Thickness", "Knuckles Size", "Nails Length", "Brow Define", "Brow Define Left", "Brow Define Right", "Brow Depth", "Brow Depth Left", "Brow Depth Right", "Brow Height", "Brow Inner Height", "Brow Inner Width", "Brow Outer Depth", "Brow Outer Height", "Brow Outer Shift", "Brow Outer Width", "Brow Shape Inner", "Brow Shape Middle", "Brow Shape Outer", "Brow Width", "Brow Width Left", "Brow Width Right", "Brows Arch", "Brows Size", "Cheek Bone Define ", "Cheek Bones Arch ", "Cheek Bones Height", "Cheek Bones Low", "Cheek Bones Round", "Cheek Bones Shape 1", "Cheek Bones Size", "Cheek Bones Size Left", "Cheek Bones Size Right", "Cheek Bones Upper Depth", "Cheek Bones Width", "Cheek Bones Width Middle ", "Cheek Bones Width Outer", "Cheek Bones Width Upper", "Cheek Jowl", "Cheek Lower Depth", "Cheek Lower Width", "Cheeks Define", "Cheeks Depth", "Cheeks Depth Left", "Cheeks Depth Middle", "Cheeks Depth Right", "Cheeks Depth Upper", "Cheeks Dimple Crease", "Cheeks Dimple Crease Left", "Cheeks Dimple Crease Right", "Cheeks Flat", "Cheeks Height", "Cheeks Inner Puffy", "Cheeks Sink", "Cheeks Sink Left", "Cheeks Sink Lower", "Cheeks Sink Right", "Cheeks Upper Crease", "Laugh Lines", "Chin Cleft", "Chin Crease", "Chin Crease B", "Chin Crease Smooth", "Chin Depth", "Chin Height", "Chin Round", "Chin Square", "Chin Width", "Chin Width 2", "Chin Width Left", "Chin Width Right", "Earlobes Attached", "Earlobes Length", "Earlobes Size", "Ears Angle", "Ears Angle Left", "Ears Angle Right", "Ears Angle Upper", "Ears Depth", "Ears Elf", "Ears Elf Long", "Ears Height", "Ears Height Left", "Ears Height Right", "Ears Size", "Ears Size Left", "Ears Size Right", "Crows Feet", "Eye Fold", "Eye Shape Bottom", "Eye Shape Upper", "Eyeballs Depth", "Eyelashes Curl", "Eyelashes Hide Layer 1", "Eyelashes Hide Layer 2", "Eyelashes Length", "Eyelashes Top Point", "Eyelid Upper Inner Shape", "Eyelid Upper Outer Shape", "Eyelids Bottom Define", "Eyelids Bottom In Height", "Eyelids Bottom Out Height", "Eyelids Fold Down", "Eyelids Heavy", "Eyelids Lower Height", "Eyelids Lower Puffy", "Eyelids Shape 1", "Eyelids Shape 2", "Eyelids Smooth", "Eyelids Top In Height", "Eyelids Top Out Height", "Eyelids Upper Height", "Eyelids Upper Height Left", "Eyelids Upper Height Right", "Eyes Almond Inner", "Eyes Almond Inner Left", "Eyes Almond Inner Right", "Eyes Almond Outer", "Eyes Almond Outer Left", "Eyes Almond Outer Right", "Eyes Angle", "Eyes Angle Left", "Eyes Angle Right", "Eyes Bags", "Eyes Cornea Bulge", "Eyes Depth", "Eyes Depth Left", "Eyes Depth Right", "Eyes Height", "Eyes Height Inner", "Eyes Height Left", "Eyes Height Outer", "Eyes Height Right", "Eyes Height Upper", "Eyes Inner  Shape", "Eyes Inner Corner Height", "Eyes Inner Corner Width", "Eyes Inner Depth", "Eyes Iris Correction", "Eyes Iris Size", "Eyes Lower Shape", "Eyes Outer Shape", "Eyes Puffy Lower", "Eyes Puffy Outer", "Eyes Puffy Shift", "Eyes Puffy Upper", "Eyes Round", "Eyes Round Lower", "Eyes Round Upper", "Eyes Shift Lower", "Eyes Shift Upper", "Eyes Size", "Eyes Size Left", "Eyes Size Right", "Eyes Slant Outer ", "Eyes Upper Shape", "Eyes Width", "Eyes Width Left", "Eyes Width Right", "Eyes Wrinkle", "Iris Height", "Iris Placement", "Iris Placement L", "Lacrimals Pinch", "Lacrimals Size", "Pupils Dilate", "Pupils Slit", "Face Angle", "Face Center Depth", "Face Depth", "Face Depth Lower ", "Face Flat", "Face Flat Left", "Face Flat Right", "Face Heart", "Face Heart Left", "Face Heart Right", "Face Height", "Face Height 2", "Face Height Upper ", "Face Lower Thin", "Face Round", "Face Round Left", "Face Round Right", "Face Square", "Face Square Left", "Face Square Right", "Face Young", "Jaw Angle", "Jaw Chin Shape", "Jaw Corner Height", "Jaw Corner Width", "Jaw Corner Width Left", "Jaw Corner Width Right", "Jaw Curve", "Jaw Define", "Jaw Height", "Jaw Height Left", "Jaw Height Right", "Jaw Line Depth", "Jaw Round", "Jaw Size", "Jaw Size Left", "Jaw Size Right", "Jaw Square", "Teeth Bottom Size", "Teeth Bucked", "Teeth Gap", "Teeth Irregular", "Teeth Lower Jaw Move Forward-Back", "Teeth Lower Jaw Move Side-Side", "Teeth Lower Jaw Move Up-Down", "Teeth Lower Jaw Size", "Teeth Lower Jaw Size Depth", "Teeth Lower Jaw Size Height", "Teeth Lower Jaw Size Width", "Teeth Top Size", "Teeth Upper Jaw Move Forward-Back", "Teeth Upper Jaw Move Side-Side", "Teeth Upper Jaw Move Up-Down", "Teeth Upper Jaw Size", "Teeth Upper Jaw Size Depth", "Teeth Upper Jaw Size Height", "Teeth Upper Jaw Size Width", "Tongue Thickness", "Tongue Tip Thickness", "Lip Lower Depth", "Lip Lower Size", "Lip Lower Width", "Lip Top Peak", "Lip Upper (ren)", "Lip Upper Curve", "Lip Upper Curves", "Lip Upper Depth", "Lip Upper Puffy", "Lip Upper Size", "Lip Upper Thick", "Lips Bottom Full", "Lips Bottom Shape ", "Lips Bottom Small", "Lips Bow Height", "Lips Bow Shape", "Lips Center Angle", "Lips Depth", "Lips Edge Define", "Lips Heart", "Lips Square", "Lips Thin", "Lips Top Full", "LIps Top Width", "Lips Upper Curves Corner", "Lips Upper Curves Round", "Lower Mouth Puffy", "Mouth Corner Depth", "Mouth Corner Height", "Mouth Corner Width", "Mouth Curves", "Mouth Curves  Arch", "Mouth Curves Arch Corner", "Mouth Curves Center Width", "Mouth Curves Corner", "Mouth Depth", "Mouth Height", "Mouth Side Crease", "Mouth Size", "Mouth Width", "Outer Mouth Shape V", "Uvula Size", "Mack Nose", "Nose A", "Nose Bridge Curve", "Nose Bridge Depth", "Nose Bridge Height", "Nose Bridge Height 2", "Nose Bridge Lower Width ", "Nose Bridge Middle Depth", "Nose Bridge Middle Width", "Nose Bridge Root Width", "Nose Bridge Skew", "Nose Bridge Slope", "Nose Bridge Width", "Nose Bump", "Nose Depth", "Nose Flesh Size", "Nose Flesh Size Left", "Nose Flesh Size Right", "Nose G", "Nose Height", "Nose Pinch", "Nose Ridge", "Nose Ridge Width", "Nose Septum Depth", "Nose Septum Height", "Nose Septum Shape", "Nose Septum Width", "Nose Side-Side", "Nose Size", "Nose Skew", "Nose Tilt", "Nose Tip Bottom Shape", "Nose Tip Depth", "Nose Tip Height", "Nose Tip Round", "Nose Tip Square", "Nose Tip Upper Shape ", "Nose Tip Width", "Nose Tip Width Lower", "Nose Tip Width Upper", "Nose Twist", "Nose V", "Nose Width", "Nostrils Bottom Rotate", "Nostrils Bottom Shape", "Nostrils Depth", "Nostrils Flare (ren)", "Nostrils Flesh Size", "Nostrils Height", "Nostrils Height Left", "Nostrils Height Right", "Nostrils Inner Height", "Nostrils Rotation", "Nostrils Shape Bottom", "Nostrils Shape Height", "Nostrils Shape Top", "Nostrils Shape Top Angle", "Nostrils Thin", "Nostrils Width", "Nostrils Width Left", "Nostrils Width Lower", "Nostrils Width Right", "Philtrum Angle", "Philtrum Depth", "Philtrum Width", "Cranium Height", "Cranium Size", "Cranium Slope", "Cranium Width", "Forehead Define", "Forehead Flat", "Forehead Round", "Forehead Slope", "ForeHead Top Width", "Forehead Width", "Forehead Wrinkle", "Forehead Wrinkle Left", "Forehead Wrinkle Right", "Head Length", "Head Scale", "Head Width", "Temples Define", "Temples Define Left", "Temples Define Right", "Darius 6 Head", "Gianni 6 Head", "Hector Head", "Julian Head (JaR)", "Lee 6 Head", "Lee Head (JaR)", "Michael 6 Head", "Ryze", "SC_Taric", "Scott 6 Head", "Down Bulge", "Genital Bulge 1", "Genital Bulge 2", "Glute Crease", "Glute Crease Left", "Glute Crease Right", "Glutes Lower Depth", "Glutes Lower Width", "Glutes Round", "Glutes Size", "Glutes Upper Depth", "Glutes Width", "Hip Bone Crest", "Hip Bone Size", "Hip Size", "Hips High", "Hips Round", "Iliac Line", "More Space AG2", "MVR_PenisBulge", "Pubic_Thin-Thick", "Round Bulge", "Tight Bulge 1", "Tight Bulge 2", "Calves Size", "Knee Bones Size", "Legs Length", "Shins Size", "Thigh Bone Size", "Thighs Size", "Thighs Tone", "Adams Apple", "Neck Length", "Neck Size", "Neck Width", "Traps Size", "Linea Alba Depth", "Love Handles", "Navel", "Navel Depth", "Navel Hollow", "Navel Horizontal", "Navel Out", "Navel Size", "Navel Vertical", "Rectus Outer Detail", "Rectus Width", "Stomach Depth", "Stomach Lower Depth", "Stomach Shape", "Stomach Soften", "Waist Width", "Shoulders Shrug", "Left Big Toe Bend", "Left Index Toe Bend", "Left Mid Toe Bend", "Left Pinky Toe Bend", "Left Ring Toe Bend", "Left Toes Fan Down", "Left Toes Spread", "Right Big Toe Bend", "Right Index Toe Bend", "Right Mid Toe Bend", "Right Pinky Toe Bend", "Right Ring Toe Bend", "Right Toes Fan Down", "Right Toes Spread", "Flaccid / Erect", "Flaccid / Erect Adjust", "Glans Left/Right", "Glans Twist", "Glans Up/Down", "Penis Base Left / Right", "Penis Base Up / Down", "Penis Twist", "Scrotum Back / Forward", "Scrotum Left / Right", "Scrotum Twist", "Urethra Open", "Left Fingers Fist", "Left Fingers Grasp", "Left Fingers In-Out", "Left Fingers Squeeze", "Left Fingers Straighten", "Left Index Finger Bend", "Left Mid Finger Bend", "Left Pinky Finger Bend", "Left Ring Finger Bend", "Left Thumb Bend", "Left Thumb Fist", "Left Thumb Grasp", "Left Thumb In-Out", "Left Thumb Straighten", "Left Hand Chop", "Left Hand Fist", "Left Hand Grasp", "Left Hand Spread", "Left Hand Straighten", "Right Fingers Fist", "Right Fingers Grasp", "Right Fingers In-Out", "Right Fingers Squeeze", "Right Fingers Straighten", "Right Index Finger Bend", "Right Mid Finger Bend", "Right Pinky Finger Bend", "Right Ring Finger Bend", "Right Thumb Bend", "Right Thumb Fist", "Right Thumb Grasp", "Right Thumb In-Out", "Right Thumb Straighten", "Right Hand Chop", "Right Hand Fist", "Right Hand Grasp", "Right Hand Spread", "Right Hand Straighten", "Brow Down", "Brow Down Left", "Brow Down Right", "Brow Inner Down", "Brow Inner Down Left", "Brow Inner Down Right", "Brow Inner Up", "Brow Inner Up Left", "Brow Inner Up Right", "Brow Outer Down", "Brow Outer Down Left", "Brow Outer Down Right", "Brow Outer Up", "Brow Outer Up Left", "Brow Outer Up Right", "Brow Squeeze", "Brow Up", "Brow Up Left", "Brow Up Right", "Cheek Crease", "Cheek Crease Left", "Cheek Crease Right", "Cheek Eye Flex", "Cheek Eye Flex Left", "Cheek Eye Flex Right", "Cheek Flex", "Cheek Flex Left", "Cheek Flex Right", "Cheeks Balloon", "Cheeks Balloon Pucker", "Afraid", "Angry", "Concentrate", "Confused", "Contempt", "Desire", "Disgust", "Excitement", "Fear", "Flirting", "Flirting Feminine Left", "Flirting Feminine Right", "Flirting Masculine Left", "Flirting Masculine Right", "Frown", "Glare", "Happy", "Pain", "Sad", "Scream", "Shock", "Smile Full Face", "Smile Open Full Face", "Snarl Left", "Snarl Right", "Surprise", "Eyes Closed", "Eyes Closed (REN)", "Eyes Closed Left", "Eyes Closed Right", "Eyes Open", "Eyes Squint", "Eyes Squint Left", "Eyes Squint Right", "Lip Bottom Down", "Lip Bottom Down Left", "Lip Bottom Down Right", "Lip Bottom In", "Lip Bottom In Left", "Lip Bottom In Right", "Lip Bottom Out", "Lip Bottom Out Left", "Lip Bottom Out Right", "Lip Bottom Up", "Lip Bottom Up Left", "Lip Bottom Up Right", "Lip Top Down", "Lip Top Down Left", "Lip Top Down Right", "Lip Top Up", "Lip Top Up Left", "Lip Top Up Right", "Lips Part", "Lips Part Center", "Lips Pucker", "Lips Pucker Wide", "Tongue Bend Tip", "Tongue Center Dip", "Tongue Curl", "Tongue In-Out", "Tongue Length", "Tongue Narrow-Wide", "Tongue Raise-Lower", "Tongue Roll 1", "Tongue Roll 2", "Tongue Side-Side", "Tongue Twist", "Tongue Up-Down", "Lips Pucker (REN)", "Mouth Frown", "Mouth Narrow", "Mouth Narrow Left", "Mouth Narrow Right", "Mouth Open", "Mouth Open Wide", "Mouth Open Wide 2", "Mouth Open Wide 3", "Mouth Side-Side", "Mouth Side-Side Left", "Mouth Side-Side Right", "Mouth Smile", "Mouth Smile Open", "Mouth Smile Simple", "Mouth Smile Simple Left", "Mouth Smile Simple Right", "Nose Wrinkle", "Nostrils Flare", "EH", "ER", "F", "IH", "IY", "K", "L", "M", "OW", "S", "SH", "T", "TH", "UW", "W", "604-Anus-Shape-Adj.Low", "605-Anus-Shape-Adj.Top", "606-Anus-Horizontal-Adj.", "607-Anus-Vertical-Adj.", "608-Anus-In.Out-Adj.", "609-Anus-In.Out-SmallArea", "610-Anus-In.Out-WideArea", "612-Anus-OuterArea-Relax", "621-Anus-Rotation-a", "622-Anus-Rotation-b", "623-Anus-Rotation-c", "626-Anus-Upper-Adj.c", "627-Anus-Upper-Adj.d", "641-Anus-OuterLine-Adj.a", "642-Anus-OuterLine-Adj.b", "Shoulder Width", "Shoulder Width (B)", "Back Definition .", "BackLessCurve", "Aiko 6 Body", "Beau Body", "Carmen Body", "Daisy Body", "Gia Body", "Heather Body", "Holly Body", "Iris Body", "Kori Body", "Lilith Body", "Lily Body", "Lorraine Body", "Lucille Body", "Lyric Body", "Mei Lin 6 Body", "Monique 6 Body", "Olympia Body", "Poppy Body", "Rose Body", "Stephanie 6 Body", "Teen Josie Body", "Victoria 4 Body", "Victoria 6 Body", "Violet Body", "Areola Puffy Edge", "Areola Size", "Areola Size X", "Areola Size Y", "Areolae Perk", "Breast Centered", "Breast Diameter", "Breast Height", "Breast Height Lower", "Breast Height Upper", "Breast Large", "Breast Pointed", "Breast Round", "Breast Sag1", "Breast Sag2", "Breast Small", "Breast Top Curve1", "Breast Top Curve2", "Breast Zero", "Breasts Cleavage", "Breasts Diameter", "Breasts Downward Slope", "Breasts Gone", "Breasts Heavy", "Breasts Implants", "Breasts Implants Left", "Breasts Implants Right", "Breasts Natural", "Breasts Natural Left", "Breasts Natural Right", "Breasts Perk Side", "Breasts Shape 02", "Breasts Shape 03", "Breasts Shape 04", "Breasts Shape 05", "Breasts Shape 06", "Breasts Shape 07", "Breasts Shape 08", "Breasts Size", "Breasts Small", "Breasts Under Curve", "Breasts Upward Slope", "BreastsCrease", "BreastsFlatten", "BreastsShape1", "BreastsShape2", "BreastsShape3", "Nipple Diameter", "Nipple Size", "Nipples Apply", "Centre Gap Narrow", "Chest Height", "ChestSeparateBreasts", "ChestShape", "ChestSmoothCenter", "ChestUnderBreast", "ChestUp", "ChestUpperNarrow", "CollarBoneIn", "CollarBoneStraight", "RibCageDefine", "101-Cervix-Vagina-Inner-Base", "102-Cervix-Open.Close", "103-Vag.Tube-Back-Adj.", "104-Vag.Tube-Front-Adj.", "201-Clit-Hood-Shorter", "202-Clit-Hood-In.Out", "203-Clit-Diameter", "204-Clit-In.Out", "205-Clit-Tip-Fwd.Bkwd", "206-ClitTo-MonsPubisArea-Adj.", "631-Anus-Vagina-Bridge-Adj.a", "632-Anus-Vagina-Bridge-Adj.b", "633-Anus-Vagina-Bridge-Adj.c", "634-Anus-Vagina-Bridge-Adj.d", "635-Anus-Vagina-Bridge-Adj.e", "636-Anus-Vagina-Bridge-Adj.f", "637-Anus-Vagina-Bridge-Adj.g", "702-Genital-Area-Rotation", "703-Genital-Area-Move-In.Out", "704-MonsPubis-In.Out-a", "Clitoris-erection", "Clitoris-exposure", "Clitoris-size", "Genitals-extreme expansion", "Labia majora-relaxation", "Labia majora-spread-LLow", "Labia majora-spread-LMid", "Labia majora-spread-LUp", "Labia majora-spread-RLow", "Labia majora-spread-RMid", "Labia majora-spread-RUp", "Labia minora-exstrophy", "Labia minora-relaxation", "Labia minora-size", "Labia minora-spread-LLow", "Labia minora-spread-LMid", "Labia minora-spread-LUp", "Labia minora-spread-RLow", "Labia minora-spread-RMid", "Labia minora-spread-RUp", "Labia minora-style1", "Labia minora-style2", "Labia minora-style3", "Labia minora-style4", "Labia minora-thickness", "Vagina-closing", "Vagina-expansion", "Claws2", "Lorraine Nails", "BrowAreaSmooth", "BrowAreaSmooth2", "Cheek Squish Left", "Cheek Squish Right", "CheekBones1", "CheekBonesDefine", "CheekBonesHigh", "CheekBonesLarge", "CheekBonesMore", "CheekHollows", "CheekInnerPuff", "Cheeks Puffy Lower ", "CheekShape3", "CheekShapeRounder", "CheekShapeRounder2", "CheeksOut", "CheeksPuffLower", "CheeksShape1", "CheeksSmooth", "Jennifer Cheeks", "ChinCleft", "ChinOut", "ChinOutRound", "ChinShape", "ChinWider", "Ears  Angle Upper", "FairyEars1", "FairyEars2", "FairyEars3", "Eyelid Upper Shape  Outer", "EyeOutUpDown", "Eyes Height Bottom", "EyeShapeLarger", "EyesShape1", "EyesShape2", "EyesShape4", "LacrimalYoung", "Pupils Dialate", "Teen Josie Lashes", "FaceAllWider", "FaceExotic", "FaceFae1", "FaceFae2", "FaceFemale4", "FaceMidProportion", "FaceModel", "FaceModel2", "FaceRoundFlat1", "FaceRoundFlat2", "FaceShape1", "FaceShape2", "FaceShape3", "FaceShapeAnime", "FaceShapeFemale", "FaceShapeFlat2", "FaceShapeHeart2", "FaceShapeOval", "FaceShapeRound", "FaceShapeRound2", "FaceShapeSquare", "FaceShapeThin", "FemaleProportion1", "FemaleProportion2", "FemaleProportion3", "FemaleProportion4", "LowerFaceSmoothRound", "Young1", "Young2", "Young3", "Young4", "Young5", "JawCornerForward", "JawEarForward", "JawlineRecede", "JawlineReduce", "Vampire Fangs", "Tongue Forked", "Tongue Ridge Bottom", "Lips Corners Pinch", "Lips Upper Curves  Round", "Mouth Marionette lines", "MouthCurves1", "MouthCurves2", "MouthLarger2", "MouthLarger3", "MouthLowLipDefine", "MouthLowLipLg", "MouthLowLipLg2", "MouthLowLipShape", "MouthPouty", "MouthProportion", "MouthRotateUp", "MouthShape1", "MouthShape10", "MouthShape11", "MouthShape12", "MouthShape2", "MouthShape3", "MouthShape4", "MouthShape5", "MouthShape6", "MouthShape7", "MouthShape8", "MouthShape9", "MouthSmaller1", "Upper Lip Curve", "Nose Bridge Middlle Depth", "Nose Tip Square Bottom", "NoseAreaSmooth", "NoseBeak", "NoseFlat", "NoseFlat2", "NoseFlat3", "NoseFlatter", "NoseMouthRatio", "NoseShape10", "NoseShape11", "NoseShape2", "NoseShape4", "NoseShape8", "NoseShape9", "NoseTurnedUp", "Adrianna", "Aiko 6 Head", "Ana Sculpt 2", "Aneta", "Beau Head", "Candy", "Carmen Face", "Danika Head", "Destiny Head", "Eisa", "Gia Head", "Izarra Head", "Kori Head", "Lilith Head", "Lorraine Head", "Lucille Head", "Lyric Head", "MadelineHead", "Maria (VAM)", "Mei Lin 6 Head", "Mia (VAM)", "Monique 6 Head", "Neomi", "Olympia Head", "Parisa", "SC_Alessa", "SC_AlessaEars", "SC_Norma", "SI_Nyssa", "Stephanie 6 Head", "Sumiko Head", "Tara Face", "Tara Face B", "Teen Josie Head", "VianneHead", "Victoria 4 Head", "Victoria 6 Head", "GlutesBig", "GluteShape1", "GluteShape2", "GluteShape3", "GlutesSmall", "HipSmall", "HipSmall2", "HipSmooth", "Pubic Area Size", "Pubic Area Width", "PubicAreaOut", "LegShape1", "LegShape2", "ShinsStraighten", "Thigh Upper Gap", "ThighStraight", "NeckBackIn", "NeckDefine1", "NeckFemaleCombo", "NeckShape1", "ThroatBaseIn", "AbdomenCreaseSmooth", "AbdomenSmooth", "Abs Definition", "Abs Muscle", "AbsUpperLess", "BellyShape1", "BellyShape2", "BellyShape3", "Stomach Shape 1", "Waist Narrow Lower", "Waist Narrow Upper", "WaistHipDefine", "WaistLower", "WaistNarrow", "WaistNarrow2", "Breasts Flatten Left", "Breasts Flatten Right", "Breasts Hang Forward", "Breasts Hang Forward Left", "Breasts Hang Forward Right", "Left Toes Small Curl", "Right Toes Small Curl", "Jaw In-Out", "Jaw Side-Side", "Lips Close", "Mouth Corner Up-Down", "Thin", "Frsk_Globe", "Frsk_Length", "Frsk_Long_Point", "Frsk_Scale_Point", "Frsk_Up_Point", "Gl_Big", "Gl_Frenulum", "Gl_Incline", "Gl_Length", "Gl_Rim", "Gl_Short", "Gl_Slope", "Gl_Small", "Gl_Urethra_Open", "Gl_Urethra_Scale", "Sc_Adhered", "Sc_Castrate", "Sc_Define", "Sc_Depth", "Sc_Flag", "Sc_Front_Back", "Sc_Front_Bulge", "Sc_Globe", "Sc_Left_Right", "Sc_Pointed", "Sc_Smooth_Back", "Sc_Twist", "Sc_Width", "Erect", "Sh_C_Spongiosum", "Sh_Flat_Top", "Sh_Girth", "Sh_Girth_Base", "Sh_Girth_XL", "Sh_Height", "Sh_Irregular", "Sh_Length", "Sh_Massive", "Sh_Taper", "Shaft Wrinkles", "Cheeks Inner Height", "Eyelid Lower Crease", "Eyelid Lower Shape", "Eyes Puffy Upper Center ", "Face Middle Thin", "Face Size", "Face Upper Width", "Jaw Corner Shape", "Lips Upper Bow", "Lips Upper Center Depth", "Nose Bridge Lower Depth", "Nostrils Define Upper", "Nostrils Inner Width", "Nostrils Shape Middle ", "Septum Width Upper", "Forehead Height", "AA", "(REN) Thin", "Maria Body (VAM)", "Pear Figure", "Voluptuous", "Areola Depth", "Breasts Shape 01", "Nipple Length", "Armpit Curve", "Center Gap Depth", "Center Gap Height", "Center Gap Smooth", "Center Gap UpDown", "Centre Gap Wide", "Chest Smoother", "CollarBoneDefine", "CollarNotchStrength", "RibCageWidth", "Gen_Innie", "Cornea Depth", "Eye Shape Bottom M", "Eye Upper Shape M", "EyeAreaSmooth", "EyeAreaThin", "Eyelids Upper Height A", "EyeShape5", "EyeShape6", "EyesShape3", "LacrimalStretch", "FaceMidProportion2", "FaceMidProportion3", "FaceShapeAfrican", "FaceShapeFlat1", "FaceShapeHeart", "FaceSmoothAll", "FaceSmoothLower", "JawHeart", "CurvyLips", "LipLoPuffy", "LipsPuffOut", "LipUpPuffy", "Mouth Mousey", "Mouth Side Crease B", "Mouth_UpLipDefine", "MouthArea_InOut", "MouthCenterXScale", "MouthCornerShape", "MouthLarger1", "MouthOuterXScale", "SmileMuscles", "UpLipAreaShape", "Upper Lip Puffy", "EyesNoseWidth", "NarrowNostrils", "NoseRidgeReduce", "NoseShape1", "NoseShape12", "NoseShape3", "NoseShape5", "NoseShape6", "NoseShape7", "NoseTip1", "NoseTip2", "Nostrils Bottom Shape 2", "Faerie Head", "Greta", "GlutesUp", "HipShape1", "HipSoftenCurves", "HipThighYoung", "ThighShape", "NeckThin", "BellySideSmooth", "Pregnant", "WaistBellyInOut", "WaistHourglass", "Breasts Flatten" };
            //these were somehow not found but were present in EasyMate, may cause them to show as not missing if they really are
            builtInMorphs.Add("Fantasy_04");
            builtInMorphs.Add("PunkGirl");
            builtInMorphs.Add("Jowls 1");
            builtInMorphs.Add("Scarlett 2");
            /*
             USE DISPLAYNAME EVERYWHERE, TO REFERENCE MORPHS, EXCEPT IN THE .CS FILE WE USE WITHIN VAM TO GET BUILT-IN MORPHS, THOSE WE BUILD A LIST OF IDS
              
            FOR BUILT-IN MORPHS WE ASK VAM TO ITERATE THROUGH ALL MORPHS AND OUTPUT THE ID, IT MUST BE THE ID, NOT THE DISPLAY NAME, EVEN THOUGH IT MATCHES THE "NAME" FIELD IN .JSON FILES
            "bulitInMorphUIDs" : ..... "Julian Body (JaR)",
                  { 
                     "uid" : "Julian Body (JaR)", //this matches builtInMorphUIDs, not display names
                     "name" : "Julian Body (JaR)", //this matches builtInMorphUIDs, not display names
                     "value" : "-0.1178645", 
                     "min" : "-1", 
                     "max" : "2"
                  }, 


            //FOR NOT BUILT IN MORPHS, MATCH THE MORPH NAME IN THE .JSON FILE TO THE MORPH FILE'S displayName
            Touchy Booty Shake
                  { 
                     "name" : "Carrie Body", 
                     "value" : "0.131866"
                  }, 

            Custom/Atom/Person/Morphs/female/FBMCarrie.vmi
            { 
   "id" : "FBMCarrie", 
   "displayName" : "Carrie Body", 

*/
            BuildFileLists();
        }

        public static string GetReadableFileSize(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static List<string> GetVAMFilePaths(string dir, string extension)
        {
            List<string> dsfFiles = new List<string>();
            try
            {
                dsfFiles.AddRange(Directory.GetFiles(dir, "*." + extension));

                if (dir.StartsWith(SAVES_FOLDER) && dir.Contains("\\Custom\\"))
                {
                    moveToCustomFolders.Add(dir);
                }

                foreach (string sub in Directory.GetDirectories(dir))
                {
                    dsfFiles.AddRange(GetVAMFilePaths(sub, extension));
                }
            }
            catch (Exception ex)
            {
                static_error_log += "\nError getting VAM File Paths: " + ex.Message;
                //MessageBox.Show("Error getting VAM File Paths: " + ex.Message);
            }

            return dsfFiles;
        }

        public static string GetVAMFileUID(VAMFile vamFile)
        {
            string json = "";
            if (vamFile.in_var)
            {
                //must have text
                if (vamFile.zipped_file.data != "")
                {
                    json = vamFile.zipped_file.data;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (!File.Exists(vamFile.path)) return "";
                json = File.ReadAllText(vamFile.path);
            }

            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);

            string uid = "";
            if (jsonObj["uid"] != null)
            {
                uid = jsonObj["uid"];
            }

            return uid;
        }

        public static bool ExtractAllZipFileContents(string zipFile, string toFolder, bool overwrite = true)
        {
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(toFolder);

                if (File.Exists(zipFile))
                {
                    if (!overwrite)
                    {
                        ZipFile.ExtractToDirectory(zipFile, toFolder);
                        vacLog += "\nEXTRACTED ALL FILES.";
                        return true;
                    }

                    string destinationDirectoryFullPath = di.FullName;

                    using (ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry file in zip.Entries)
                        {
                            string fileName = file.FullName.Replace("/", "\\");
                            string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, fileName));

                            if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                            {
                                vacLog += "\nTrying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability";
                                return false;
                            }

                            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                            bool inError = false;
                            if (fileName.EndsWith("\\"))
                            {
                                //it's a folder
                            }
                            else
                            {
                                //if a file doesn't succeed, keep going
                                try
                                {
                                    file.ExtractToFile(completeFileName, true);
                                }
                                catch (Exception ex)
                                {
                                    vacLog += "\nVAR package .zip file error in file " + zipFile + ":\n" + ex.Message;
                                }
                            }
                            if (inError) return false;
                        }
                        vacLog += "\nEXTRACTED ALL FILES.";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                vacLog += "\nVAR package .zip file error in file " + zipFile + ":\n" + ex.Message;
                return false;
            }
            return false;
        }

        public static Dictionary<string, ZippedFile> ReadZipFileContents(string zipFile)
        {
            Dictionary<string, ZippedFile> fileContents = new Dictionary<string, ZippedFile>();

            try
            {
                if (!File.Exists(zipFile))
                {
                    ShowErrorDialog("VAR package not found: " + zipFile);
                    return fileContents;
                }

                using (ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {

                        string relativePath = entry.FullName;

                        varFileLog += relativePath + "\n";

                        string testAbsolutePath = MakeVaMAbsolutePath(VAM_FOLDER, "", relativePath);

                        //should start with our vam root and not end with a backslash (or its a folder)
                        if (testAbsolutePath.StartsWith(VAM_FOLDER) && !testAbsolutePath.EndsWith("\\"))
                        {
                            ZippedFile zippedFile = new ZippedFile();
                            if (relativePath.EndsWith("vam") || relativePath.EndsWith("vap") || relativePath.EndsWith("vaj") || relativePath.EndsWith(MORPH_EXTENSION))
                            {
                                using (StreamReader s = new StreamReader(entry.Open()))
                                {
                                    // stream with the file
                                    zippedFile.data = s.ReadToEnd();
                                }
                            }
                            zippedFile.length = entry.Length;
                            fileContents.Add(relativePath, zippedFile);
                        }
                        else if (!testAbsolutePath.StartsWith(VAM_FOLDER))
                        {
                            static_error_log += "\nFound file in VAR Package " + Path.GetFileName(relativePath) + " where VACUUM derived an incorrect absolute path of: " + testAbsolutePath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorDialog("VAR package .zip file error in file " + zipFile + ":\n\n" + ex.Message);
            }

            return fileContents;
        }

        public static void ShowErrorDialog(string message)
        {
            MessageBox.Show(message, "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public static int EndIndexOf(string source, string value)
        {
            int index = source.IndexOf(value);
            if (index >= 0)
            {
                index += value.Length;
            }

            return index;
        }

        public static string GetCorrectedLinkToFileInVarPackage(string absoluteOrRelativePath)
        {
            if (absoluteOrRelativePath.Contains(":/"))
            {
                //still it may be a var file absolute path
                string[] path_sections = absoluteOrRelativePath.Split(new[] { ":/" }, StringSplitOptions.None);
                if (path_sections.Length == 2)
                {
                    return GetCorrectedLinkToFileInVarPackage(path_sections[0], path_sections[1]);
                }
            }
            static_error_log += "\nNot a VAR path: " + absoluteOrRelativePath;
            return "";
        }

        public static string GetCorrectedLinkToVarPackage(string pathToVar)
        {
            //THERE ARE ONLY RELATIVE PATHS TO FILES IN VAR FILES, ABSOLUTE PATHS TO VAR FILES DON'T MAKE SENSE TO VAM

            //must be in this format
            //NoStage3.Hair_Long_Wavy_1_Bangs.1:/Custom/Hair/Female/NoStage3/Long Wavy 1 Bangs/Long Wavy 1.vam

            //an absolute path to a var file doesn't work anymore THIS DOESN'T WORK:
            //AddonPackages/NoStage3.Hair_Long_Wavy_1_Bangs.1.var:/Custom/Hair/Female/NoStage3/Long Wavy 1 Bangs/Long Wavy 1.vam

            string varExtension = ".var";
            string originalPath = pathToVar;
            bool adjustedPath = false;

            if (pathToVar.EndsWith(varExtension))
            {
                pathToVar = pathToVar.Substring(0, pathToVar.Length - varExtension.Length);
                adjustedPath = true;
            }

            if (pathToVar.Contains("/"))
            {
                if (pathToVar.LastIndexOf("/") + 1 >= pathToVar.Length) return "";
                pathToVar = pathToVar.Substring(pathToVar.LastIndexOf("/") + 1);
                adjustedPath = true;
            }

            if (pathToVar.Contains("\\"))
            {
                if (pathToVar.LastIndexOf("\\") + 1 >= pathToVar.Length) return "";
                pathToVar = pathToVar.Substring(pathToVar.LastIndexOf("\\") + 1);
                adjustedPath = true;
            }

            if (adjustedPath)
            {
                //static_error_log += "\nAbsolute path to VAR converted to VAM path. Path: " + originalPath + " corrected to: " + pathToVar;
            }

            return pathToVar;
        }

        public static string GetCorrectedLinkToFileInVarPackage(string pathToVar, string pathInVar)
        {
            return GetCorrectedLinkToVarPackage(pathToVar) + ":/" + pathInVar;
        }

        public static bool VARPackageFileExists(string absolutePath)
        {
            if (absolutePath.Contains(":/"))
            {
                //still it may be a var file absolute path
                string[] path_sections = absolutePath.Split(new[] { ":/" }, StringSplitOptions.None);
                if (path_sections.Length == 2)
                {
                    string varPackageRelativePath = GetCorrectedLinkToVarPackage(path_sections[0]);

                    if (findVARAbsolutePath.ContainsKey(varPackageRelativePath))
                    {
                        string absolutePathToVar = findVARAbsolutePath[varPackageRelativePath];
                        if (varFileContents.ContainsKey(absolutePathToVar))
                        {
                            if (varFileContents[absolutePathToVar].ContainsKey(path_sections[1]))
                            {
                                return true;
                            }
                        }
                    } else
                    {
                        static_error_log += "\nCan't find realtive path of VAR file: " + varPackageRelativePath + " referenced in: " + absolutePath + " (probably missing a required VAR package).";
                    }
                }
            }
            return false;
        }

        public static string MakeVaMRelativePath(string rootFolder, string absolutePath)
        {
            //passing in the absolute path of a var file with a relative path in the absolute path?
            if (absolutePath.Contains(":/"))
            {
                return GetCorrectedLinkToFileInVarPackage(absolutePath);
            }

            string relativePath = absolutePath.Replace(rootFolder, "");
            relativePath = relativePath.Replace("\\", "/");

            //could be one or maybe maybe two forward slashes at the beginning
            relativePath = relativePath.TrimStart('/');

            return relativePath;
        }

        public static string MakeVaMAbsolutePath(string rootDirectory, string activeDirectory, string path)
        {
            //remove the end quote
            path = path.TrimEnd('"');

            if (activeDirectory == "")
            {
                activeDirectory = rootDirectory;
            }

            string absolutePath = "";

            string relativeFolder = "./";
            string customFolder = "Custom/";
            string savesFolder = "Saves/";
            string assetsFolder = "Assets/";
            string fileFolder = "file://";
            string texturesFolder = "Textures/";

            string[] path_sections = path.Split(new[] { ":/" }, StringSplitOptions.None);

            //if we are in a .cs file, then we can't know the absolute path, mark it so that any copies of this file should be saved
            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {
                //don't add these
            }
            else if (path.Contains(":/") && path_sections.Length == 2)
            {
                return GetCorrectedLinkToFileInVarPackage(path);
            }
            else if (path.StartsWith(relativeFolder))
            {
                absolutePath = activeDirectory + "\\" + path.Substring(EndIndexOf(path, relativeFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith(customFolder))
            {
                absolutePath = rootDirectory + "\\" + customFolder.Replace("/", "\\") + path.Substring(EndIndexOf(path, customFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith(savesFolder))
            {
                absolutePath = rootDirectory + "\\" + savesFolder.Replace("/", "\\") + path.Substring(EndIndexOf(path, savesFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith(assetsFolder))
            {
                absolutePath = rootDirectory + "\\" + assetsFolder.Replace("/", "\\") + path.Substring(EndIndexOf(path, assetsFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith(fileFolder))
            {
                absolutePath = rootDirectory + path.Substring(EndIndexOf(path, fileFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith(texturesFolder))
            {
                //unusual case
                //"Textures/AFVR models/Zarra_v1.jpg"
                //E:\VaM\Custom\Atom\Person\Textures\AFVR models
                absolutePath = rootDirectory + "\\Custom\\Atom\\Person\\Textures\\" + path.Substring(EndIndexOf(path, texturesFolder)).Replace("/", "\\");
            }
            else if (path.StartsWith("/"))
            {
                absolutePath = rootDirectory + "\\" + path.Replace("/", "\\");
            }
            else
            {
                //if the relative path was texture.png we get  [current folder]\texture.png   if it was   tex/texture.png  we get [current folder]\tex\texture.png
                absolutePath = activeDirectory + "\\" + path.Replace("/", "\\");
            }

            //occasionaly saw double backslashes, replace them
            absolutePath = absolutePath.Replace("\\\\", "\\");

            //we could get something like this
            //[vam]\Custom\Assets\Environments\Skybox_Pack_01\..\..\..\assets\Mofme\cubemap\abandoned_warehouse\0006.png
            int whileCount = 0;
            int relativePathIndex = absolutePath.IndexOf("\\..\\");
            if (relativePathIndex > 0)
            {
                static_error_log += "\nAttempting to improve relative part of strange absolute path: " + absolutePath;
                while (relativePathIndex > 0)
                {
                    whileCount++;
                    bool successful_change = false;
                    //we will remove that, plus remove everything to the left of it till the \, unless that would take us out of custom or saves
                    int startOfPriorDirectoryToRemove = absolutePath.LastIndexOf("\\", relativePathIndex - 1);
                    if (startOfPriorDirectoryToRemove >= 0)
                    {
                        //example s\Skybox_Pack_01\..\.  relativePathIndex=16   startOfPrior=1   desire to remove from 2 , total of 18 characters
                        string newString = absolutePath.Remove(startOfPriorDirectoryToRemove + 1, (relativePathIndex + 4) - (startOfPriorDirectoryToRemove + 1));
                        if (newString.StartsWith(CUSTOM_FOLDER) || newString.StartsWith(SAVES_FOLDER))
                        {
                            successful_change = true;
                            absolutePath = newString;
                        }
                    }

                    //each time it removes something so we don't get stuck in a while loop
                    if (!successful_change) break;
                    if (whileCount > 100) break; //just in case...

                    relativePathIndex = absolutePath.IndexOf("\\..\\");
                }
                static_error_log += "\nImproved path: " + absolutePath;
            }

            return absolutePath;
        }
    }
}
