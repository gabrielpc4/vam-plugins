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

namespace geesp0t.VACUUM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private SettingHandler<Settings> _settings;

        //IF WE WANT TO MANUALLY SPECIFY PROTECTED FOLDERS, OR ANYTHING ELSE
        //List<ProtectedFolder> protectedFoldersList = new List<ProtectedFolder>();

        public SettingHandler<Settings> Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();

            Loaded += Start;
            
            //IF WE WANT TO MANUALLY SPECIFY PROTECTED FOLDERS, OR ANYTHING ELSE
            /*
            protectedFoldersList = Settings.LoadedSettings.ProtectedFolders;
            if (protectedFoldersList == null)
            {
                protectedFoldersList = new List<ProtectedFolder>();
            }

            dataGrid1.ItemsSource = protectedFoldersList;
            dataGrid1.CellEditEnding += ProtectedFoldersDataGrid_CellEditEnding;*/
        }

        //IF WE WANT TO MANUALLY SPECIFY PROTECTED FOLDERS, OR ANYTHING ELSE
        /*void ProtectedFoldersDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //var el = e.EditingElement as TextBox;
            //MessageBox.Show(el.Text + "\n\n" + protectedFoldersList[0].Data, "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);

            Settings.LoadedSettings.ProtectedFolders = protectedFoldersList;
        }*/

        void Start(object sender, EventArgs e)
        {

            if (MessageBox.Show("VACUUM deletes unused duplicate files to reduce file size and remove conflicts.\n\nVACUUM improves scenes by fixing references to old or missing files (for example updating old scenes to use new VAR Package resources).\n\nMake a backup copy of your VAM folder before using VACUUM!\n\nVACUUM has strict checks to only modify files within your selected VAM folder.\n\nVACUUM will prompt you before making any file changes (unless you press \"Yes\" to Easy Clean).\n\nVACUUM is provided \"as is\" without warranty of any kind. You are solely responsible for backing up your computer's data, and any damage or loss of data that results from the use of VACUUM.\n\nAll VACUUM source files are provided as part of the VACUUM package.\n\nPress \"Yes\" to agree to be fully responsible for VACUUM use, or \"No\" to close VACUUM now.", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.No)
            {
                Application.Current.MainWindow.Close();
                return;
            }

            if (Settings.LoadedSettings.Folder != null)
            {
                // If a folder was selected in a previous session, 
                // reload the same folder when app starts up.
                if (MessageBox.Show("Would you like to Clean your last selected VAM folder?\n\nPress \"No\" to select a different VAM root folder.\n\nPress \"Yes\" to analyze and clean your VAM folder: " + Settings.LoadedSettings.Folder + ".", "VACUUM", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    LoadVAMFolder();
                } else
                {
                    OpenFolderNow();
                }
            } else
            {

                //IF WE WANT TO MANUALLY SPECIFY PROTECTED FOLDERS, OR ANYTHING ELSE
                /*MessageBox.Show("If you want to protect some folders, enter their absolute paths in the Protected Folders List." +
                    "\n\n\nThen, press \"Select VAM Folder\" to select your VAM root folder and start cleaning it!" +
                    "\n\n\nYour VAM root folder is where you normally launch VAM." +
                    "\n\nYour VAM root folder contains your Custom and Saves folders." +
                    "\n\n\nMost regular users won't need to protect any folders." +
                    "\n\nIf you are a content creator and don't want VACUUM to clean files from one or more development folders, enter those folders in the Protected Folders List." +
                    "\n\nIf you have stored unused files in your Saves folder (for example a screenshots folder, with images that VAM doesn't use, but that you want to see from Windows Explorer), enter those folders in the Protected Folders List."
                    , "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);*/

                MessageBox.Show("Press \"Select VAM Folder\" to select your VAM root folder and start cleaning it!" +
                        "\n\n\nYour VAM root folder is where you normally launch VAM." +
                        "\n\nYour VAM root folder contains your Custom and Saves folders."
                        , "VACUUM", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Settings.LoadedSettings.PropertyChanged += OnViewOptionChanged;
        }

        private void LoadSettings()
        {
            var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.xml");

            // Load settings store from auto-created XML settings file.
            Settings = new SettingHandler<Settings>(new FileInfo(settingsPath));
        }
        //DLL Imports for External Mouse Point Tracking
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        
        private async void LoadVAMFolder()
        {
            string folder = Settings.LoadedSettings.Folder;
            if (folder != null)
            {
                if (Directory.Exists(folder))
                {
                    this.Title = "VACUUM - " + folder;
                    //regions = new Regions(Settings.LoadedSettings, _morph_references);
                    //DataContext = regions;

                    if (!Directory.Exists(folder + "\\Custom\\"))
                    {
                        MessageBox.Show("Selected folder does not contain a \"Custom\" folder. Select your VAM root folder (which should have a Custom and Saves folder in it).\n\nYou selected: " + folder, "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
                        OpenFolderNow();
                    } else if (!Directory.Exists(folder + "\\Saves\\"))
                    {
                        MessageBox.Show("Selected folder does not contain a \"Saves\" folder. Select your VAM root folder (which should have a Custom and Saves folder in it).\n\nYou selected: " + folder, "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
                        OpenFolderNow();
                    } else
                    {
                        if (VAMFile.Working())
                        {
                            MessageBox.Show("VACUUM is currently processing your VAM folder, please wait.\n\nProgress is shown in the VACUUM window title bar.\n\nTo cancel the current task, close and reopen VACUUM.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            await VAMFile.InitTask(this, folder);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Last used VAM root folder doesn't exist: " + folder, "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
                    OpenFolderNow();
                }
            } else
            {
                MessageBox.Show("Please first select your VAM folder by pressing the \"Select VAM Folder\" button.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OpenFolderNow()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = Settings.LoadedSettings.Folder ?? "";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.LoadedSettings.Folder = dialog.SelectedPath;
                    LoadVAMFolder();
                } else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                }
            }
        }
        private void OnOpenFolder(object sender, RoutedEventArgs e)
        {
            if (VAMFile.Working())
            {
                MessageBox.Show("VACUUM is currently processing your VAM folder, please wait.\n\nProgress is shown in the VACUUM window title bar.\n\nTo cancel the current task, close and reopen VACUUM.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                OpenFolderNow();
            }
        }
        private void OnRefresh(object sender, RoutedEventArgs e)
        {
            OnCleanFolder(sender, e);
        }
        private void OnCleanFolder(object sender, RoutedEventArgs e)
        {
            if (VAMFile.Working())
            {
                MessageBox.Show("VACUUM is currently processing your VAM folder, please wait.\n\nProgress is shown in the VACUUM window title bar.\n\nTo cancel the current task, close and reopen VACUUM.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                LoadVAMFolder();
            }
        }


        private async void OnDeleteDupUIDS(object sender, RoutedEventArgs e)
        {
            if (VAMFile.Working())
            {
                MessageBox.Show("VACUUM is currently processing your VAM folder, please wait.\n\nProgress is shown in the VACUUM window title bar.\n\nTo cancel the current task, close and reopen VACUUM.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
            } else { 
                await VAMFile.DeleteDuplicateUIDsTask();
            }
        }

        private async void OnDeepClean(object sender, RoutedEventArgs e)
        {
            if (VAMFile.Working())
            {
                MessageBox.Show("VACUUM is currently processing your VAM folder, please wait.\n\nProgress is shown in the VACUUM window title bar.\n\nTo cancel the current task, close and reopen VACUUM.", "VACUUM", MessageBoxButton.OK, MessageBoxImage.Error);
            } else
            {
                await VAMFile.ProcessSourceFilesTask();
            }
        }

        private void OnDeleteFilesDuplicatedInAddonPackages(object sender, RoutedEventArgs e)
        {
            //DeleteFilesDuplicatedInAddonPackages();
        }

        private void OnDeleteUnusedAssetbundles(object sender, RoutedEventArgs e)
        {
            //Checking for clothing means finding the ID of each vam file, and then if you delete the vam file, also delete the vap files
            //If the ID is Cat Headphone ("uid" : "Cat Headphone") then vap files with Cat HeadphoneStyle should be deleted "storables" : [  {  "id" : "Cat HeadphoneStyle"
            //DeleteFilesDuplicatedInAddonPackages();
            //GetUsedFilesWithExtensions(assetExtensions, assetTypes, assetFolders, savefileExtensions, saveFolders);
        }
        private void OnViewOptionChanged(object sender, PropertyChangedEventArgs args)
        {
            Console.WriteLine("Property " + args.PropertyName + " changed");
            if (args.PropertyName.StartsWith("Show"))
            {
                //LoadVAMFolder();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}