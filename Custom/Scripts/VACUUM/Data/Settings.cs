using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Morphology.Data
{
    /// <summary>
    /// Setting Class that gets Serialized to the XML-Setting File. Add any new Settings as a Property here
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        public string Folder
        {
            get => _folder;
            set
            {
                var oldvalue = _folder;
                _folder = value;
                if (value != oldvalue)
                {
                    IsDirty = true;
                }
                OnPropertyChanged();
            }
        }
        public List<geesp0t.VACUUM.ProtectedFolder> ProtectedFolders
        {
            get => _protectedFolders;
            set
            {
                var oldvalue = _protectedFolders;
                _protectedFolders = value;
                if (value != oldvalue)
                {
                    IsDirty = true;
                }
                OnPropertyChanged();
            }
        }

        private bool _isDirty;
        [XmlIgnore]
        public bool IsDirty
        {
            get { return _isDirty; }
            set
            {
                _isDirty = value;
                if (_isDirty)
                {
                    SettingHandler<Settings>.CurrentInstance.Save();
                }
                OnPropertyChanged();
            }
        }
        private string _folder;
        private List<geesp0t.VACUUM.ProtectedFolder> _protectedFolders = new List<geesp0t.VACUUM.ProtectedFolder>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
