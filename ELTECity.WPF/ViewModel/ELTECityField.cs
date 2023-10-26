using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ELTECity.WPF.ViewModel
{
    public class ELTECityField : ViewModelBase
    {
        #region Fields

        private Double _happiness;
        private BitmapImage? _tilePicture;
        private Double _rotate;
        private (Int32 x, Int32 y) _scale;
        private (Int32 x, Int32 y) _translate;
        private Boolean _isenabled;
        private String? _mouseOverColor;
        private String? _mouseLeaveColor;

        #endregion

        #region Properties

        public Int32 X { get; set; }

        public Int32 Y { get; set; }

        public Int32 Number { get; set; }

        public BitmapImage? TilePicture
        {
            get => _tilePicture;
            set
            {
                _tilePicture = value;
                OnPropertyChanged();
            }
        }

        public Double Rotate
        {
            get => _rotate;
            set
            {
                _rotate = value;
                _translate = (value == 90 ? (62, 0) : value == 180 ? (62, 62) : value == 270 ? (0, 62) : (0, 0));
                OnPropertyChanged();
                OnPropertyChanged("TranslateX");
                OnPropertyChanged("TranslateY");
            }
        }

        public (Int32 x, Int32 y) Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                _translate = (value.x >= 0 ? 0 : 62, value.y >= 0 ? 0 : 62);
                OnPropertyChanged("ScaleX");
                OnPropertyChanged("ScaleY");
                OnPropertyChanged("TranslateX");
                OnPropertyChanged("TranslateY");
            }
        }

        public Int32 ScaleX
        {
            get => _scale.x;
            set
            {
                _scale.x = value;
                _translate.x = value >= 0 ? 0 : 62;
                OnPropertyChanged();
            }
        }

        public Int32 ScaleY
        {
            get => _scale.y;
            set
            {
                _scale.y = value;
                _translate.y = value >= 0 ? 0 : 62;
                OnPropertyChanged();
            }
        }

        public Int32 TranslateX
        {
            get => _translate.x;
            set
            {
                _translate.x = value;
                OnPropertyChanged();
            }
        }

        public Int32 TranslateY
        {
            get => _translate.y;
            set
            {
                _translate.y = value;
                OnPropertyChanged();
            }
        }

        public String? MouseOverColor
        {
            get => _mouseOverColor;
            set
            {
                _mouseOverColor = value;
                OnPropertyChanged();
            }
        }

        public String? MouseLeaveColor
        {
            get => _mouseLeaveColor;
            set
            {
                _mouseLeaveColor = value;
                OnPropertyChanged();
            }
        }

        public Double Happiness
        {
            get => _happiness;
            set
            {
                _happiness = value;
                OnPropertyChanged();
            }
        }

        public Boolean IsEnabled
        {
            get => _isenabled;
            set
            {
                _isenabled = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand? TileClickCommand { get; set; }

        #endregion
    }
}
