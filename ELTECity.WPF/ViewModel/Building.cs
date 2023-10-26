using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ELTECity.Model;

namespace ELTECity.WPF.ViewModel
{
    public class Building : ViewModelBase
    {
        #region Fields

        private Boolean _isEnabled;
        private String? _color;
        #endregion

        #region Properties

        public int Number { get; set; }

        public IField? BuildingType { get; set; }

        public String? BuildingName { get; set; }

        public BitmapImage? BuildingPicture { get; set; }

        public Boolean IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public String? Price { get; set; }

        public String? Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand? SelectBuildingCommand { get; set; }

        #endregion
    }
}
