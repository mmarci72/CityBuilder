using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public interface IField
    {
        #region Properties

        public Boolean Connected { get; set; }
        public Int32 Price { get; set; }
        public (Int32 x, Int32 y) Location { get; set; }
        public (Int32 x, Int32 y) Size { get; }

        /// <summary>
        /// Should only be used in Update methods of the City class!
        /// </summary>
        public Boolean UpdatedDailyAlready { get; set; }
        
        /// <summary>
        /// Should only be used in Update methods of the City class!
        /// </summary>
        public Boolean UpdatedWeeklyAlready { get; set; }

        /// <summary>
        /// Should only be used in Update methods of the City class!
        /// </summary>
        public Boolean UpdatedMonthlyAlready { get; set; }

        /// <summary>
        /// Should only be used in Update methods of the City class!
        /// </summary>
        public Boolean UpdatedYearlyAlready { get; set; }


        #endregion

        #region Public methods

        public IField MakeField();

        public Int32 UpdateDaily(City city);
        public Int32 UpdateWeekly(City city);
        public Int32 UpdateMonthly(City city);
        public Int32 UpdateYearly(City city);
        public Uri? GetPictureSource();

        #endregion
    }
}
