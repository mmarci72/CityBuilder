using System;

namespace ELTECity.Model
{
    public class Residential : Zone
    {
        #region Properties

        public Double IndustrialDistBonus { get; set; }

        #endregion

        #region Constructor

        public Residential()
        {
            IndustrialDistBonus = 0.2;

            string imagePathLow1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_low_low.png");
            string imagePathLow2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_low_high.png");
            string imagePathMedium1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_medium_low.png");
            string imagePathMedium2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_medium_high.png");
            string imagePathHigh1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_high_low.png");
            string imagePathHigh2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_high_high.png");

            _pictureSource = new Uri[,]{
                {
                    new Uri(imagePathLow1),
                    new Uri(imagePathLow2)
                },
                {
                    new Uri(imagePathMedium1),
                    new Uri(imagePathMedium2)
                },
                {
                    new Uri(imagePathHigh1),
                    new Uri(imagePathHigh2)
                }
            };

            Price = 100;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Updates population and calculates taxes and pensions
        /// </summary>
        /// <returns>The sum of taxes and pensions</returns>
        public override Int32 UpdateMonthly(City city)
		{
			Int32 profit = 0;
			if (Connected)
			{
				if (!IsFull())
				{
					// population increase based upon the density
					for (Int32 i = 0; i < (Int32)this.Density; i++)
					{
						Person person = new Person(new Random().Next(18, 61));
						if (city.Bonuses[Location.x, Location.y] != null && city.SearchWorkplace(this.Location.x, this.Location.y, person))
						{
							if (city.GuaranteedPopulation >= 0)
							{
								_population.Add(person);
								person.Home = this;
								city.GuaranteedPopulation--;
                                city.LowDegreeNumber++;
                            }
							else
							{
								double happiness = (city.Bonuses[Location.x, Location.y]!.IndustrialBonus[0].bonus +
													person.WorkplaceDistanceHappiness + city.HappinessRate()) / 1.3;

								double probability = happiness switch
								{
									<= 0.3 => 0.4,
									<= 0.5 => 0.6,
									<= 0.7 => 0.8,
									_ => 1
								};
								double rnd = new Random().NextDouble();
								if (rnd < probability)
								{
									_population.Add(person);
									person.Home = this;
                                    city.LowDegreeNumber++;
                                }
								else if (person.Workplace != null)
								{
									person.Workplace!.Fire(person, city);
								}
							}
						}
					}
					
					city.UtilizationChanged(this);
					
				}
				for (int i = 0; i < _population.Count; i++)
				{
                    Int32 currentprofit = (Int32)(_population[i].UpdateMonthly(city) * city.TaxPercentage);
                    profit += currentprofit;
                    if (currentprofit >= 0)
                    {
                        city.CurrentYearIncome += currentprofit;
                    }
                    else
                    {
                        city.Pensions += currentprofit;
                        city.CurrentYearExpense += currentprofit;
                    }
                }
				UpdatedMonthlyAlready = true;
			}
			return profit;
		}

        public override Int32 UpdateYearly(City city)
        {
            for (int i = 0; i < _population.Count(); i++)
            {
                _population[i].UpdateYearly(city);
            }
            return 0;
        }

        public override Double HappinessRate(City city)
        {
            if (!Connected)
            {
                return 0;
            }
            if (_population.Count() == 0)
            {
                return 0;
            }
            else
            {
                Double sum = 0;
                for (int i = 0; i < _population.Count(); i++)
                {
                    sum += _population[i].ResidentialHappiness(city);
                }

                return (sum / _population.Count());
            }
        }

        public override IField MakeField()
        {
            return new Residential();
        }

        #endregion

    }

}
