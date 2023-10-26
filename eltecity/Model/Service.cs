using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
	public abstract class Service : IField
	{
		#region Fields

        protected (Int32 x, Int32 y) _size = (1, 1);
        protected Int32 _baseProfit = 20;
        protected Uri? _pictureSource;

        #endregion

        #region Properties

        public Int32 Price { get; set; }
        public bool Connected { get; set; }
		public (int x, int y) Location { get; set; }
		public (int x, int y) Size { get => _size; }

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

		public virtual Int32 UpdateDaily(City city)
		{
			if (Connected)
			{
				UpdatedDailyAlready = true;
			}
			return 0;
		}

		public virtual Int32 UpdateWeekly(City city)
		{
			if (Connected)
			{
				UpdatedWeeklyAlready = true;
			}
			return 0;
		}

        /// <summary>
        /// Calculates the monthly upkeep cost of the service building
        /// </summary>
        /// <returns>Monthly upkeep cost of the service building</returns>
        public virtual Int32 UpdateMonthly(City city)
        {
            Int32 profit = 0;
            if (Connected)
            {
                profit = -_baseProfit;
                city.UpkeepCost += _baseProfit;
                city.CurrentYearExpense += _baseProfit;
                UpdatedMonthlyAlready = true;
            }
            return profit;
        }

		public virtual Int32 UpdateYearly(City city)
		{
			if (Connected)
			{
				UpdatedYearlyAlready = true;
			}
			return 0;
		}

        public Uri? GetPictureSource()
        {
            return _pictureSource;
        }

        public abstract IField MakeField();

		#endregion
	}
}
