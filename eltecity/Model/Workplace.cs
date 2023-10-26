using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public abstract class Workplace : Zone
    {
        #region Public methods

        /// <summary>
        /// Employs person, if capacity and connectedness allows to
        /// </summary>
        /// <param name="who">Who to employ</param>
        /// <returns>Could employ or not</returns>
        public Boolean Employ(Person who, City city)
        {
	        var isHighlyUtilized = HighlyUtilized();
            // can't employ is full
            if (IsFull() || !Connected)
            {
                return false;
            }
            _population.Add(who);
            who.Workplace = this;

	        if (isHighlyUtilized != HighlyUtilized())
            {
	            city.UtilizationChanged(this);
            }


            return true;
        }

        /// <summary>
        /// Fires person, if working there already
        /// </summary>
        /// <param name="who">Who to fire</param>
        /// <returns>Person was working there</returns>
        public Boolean Fire(Person who, City city)
        {
	        var isHighlyUtilized = HighlyUtilized();
            // can't remove if not working there
            if (_population.Remove(who))
            {
                who.Workplace = null;
	            if (isHighlyUtilized != HighlyUtilized())
				{
					city.UtilizationChanged(this);
				}
                return true;
            }
            

            return false;
        }

        public override Double HappinessRate(City city)
        {
            if (!Connected)
            {
                return 0;
            }
            if (!_population.Any())
            {
                return 0;
            }

            Double sum = 0;
            for (int i = 0; i < _population.Count(); i++)
            {
                sum += _population[i].WorkplaceHappiness(city);
            }

            return (sum / _population.Count());

        }

        /// <summary>
        /// Calculate the monthly tax amount
        /// </summary>
        /// <returns>Monthly tax amount</returns>
        public override Int32 UpdateMonthly(City city)
        {
	        
            Int32 profit = 0;
            if (Connected)
            {
                for (int i = 0; i < _population.Count(); i++)
                {
                    profit += (Int32)(_population[i].UpdateMonthly(city) * city.TaxPercentage);
                }
                city.CurrentYearIncome += profit;
                UpdatedMonthlyAlready = true;
            }

            
           
            return profit;
        }

        /// <summary>
        /// Fires people who got retired
        /// </summary>
        /// <returns></returns>
        public override Int32 UpdateYearly(City city)
        {
            for (int i = 0; i < _population.Count(); i++)
            {
                if (_population[i].IsRetired())
                {
                    Fire(_population[i], city);
                }
            }
            return 0;
        }


        #endregion
    }
}
