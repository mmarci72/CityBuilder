using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public enum Density
    {
        Low = 1, Medium = 5, High = 25
    }
    public abstract class Zone : IField
    {
        #region Fields

        protected List<Person> _population = new List<Person>();
        protected Density _density = Density.Low;
        protected Uri[,]? _pictureSource;

        #endregion

        #region Properties

        public Int32 Price { get; set; }
        public Boolean Connected { get; set; }
        public (int x, int y) Location { get; set; }
        public (int x, int y) Size { get; } = (1, 1);
        public Density Density { get => _density; set { _density = value; } }
        public Double ForestBonus { get; set; }
        public Double StadiumBonus { get; set; }
        public Double PoliceBonus { get; set; }

        public List<Person> Population
        {
            get => _population;
            set => _population = value;
        }

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

        public abstract IField MakeField();

        public Int32 PopulationCount()
        {
            return _population.Count;
        }

        /// <summary>
        /// Az adott zónaépület legalább 65% kihasználtságú-e
        /// </summary>
        /// <returns>true, ha kihasználtság >= 65%, különben false</returns>
        public Boolean HighlyUtilized()
        { return UtilizationRate() >= 0.65; }

        public Boolean IsFull()
        { return UtilizationRate() == 1.0; }

        public abstract Double HappinessRate(City city);

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
        /// Updates every person's funds
        /// </summary>
        /// <returns>Tax deducted from people</returns>
        public virtual Int32 UpdateMonthly(City city)
        {
            Int32 profit = 0;
            if (Connected)
            {
                for (Int32 i = 0; i < _population.Count(); i++)
                {
                    profit += (Int32)(_population[i].UpdateMonthly(city) * city.TaxPercentage);
                }
                city.CurrentYearIncome += profit;
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

        public Uri GetPictureSource()
        {
            if (_pictureSource != null)
            {
                switch (_density)
                {
                    case Density.Low:
                        if (HighlyUtilized())
                        {
                            return _pictureSource[0, 1];
                        }
                        return _pictureSource[0, 0];
                    case Density.Medium:
                        if (HighlyUtilized())
                        {
                            return _pictureSource[1, 1];
                        }
                        return _pictureSource[1, 0];
                    case Density.High:
                        if (HighlyUtilized())
                        {
                            return _pictureSource[2, 1];
                        }
                        return _pictureSource[2, 0];
                    default:
                        return _pictureSource[0, 0];
                }
            }
            return new Uri("");
        }

        public Int32 Upgrade()
        {
            switch (_density)
            {
                case Density.Low:
                    _density = Density.Medium;
                    break;
                case Density.Medium:
                    _density = Density.High;
                    break;
                case Density.High:
                    return 0;
                default:
                    break;
            }
            return -(Int32)_density * 100;
        }


        #endregion

        #region Private methods

        /// <summary>
        /// Calculates the max population from the density of the zone
        /// </summary>
        /// <returns>6 * density multiplier</returns>
        protected Int32 MaxPopulation()
        { return 6 * (Int32)_density; }

        /// <summary>
        /// Calculates the zone's utilization rate
        /// </summary>
        /// <returns>population count / max population</returns>
        protected Double UtilizationRate()
        { return (Double)_population.Count / MaxPopulation(); }

        #endregion
    }
}
