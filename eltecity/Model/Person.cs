using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
    public enum Degree
    {
        Low = 1, Middle = 2, High = 3
    }

    public class Person
    {
        #region Fields

        private Int64 _id;
        private static Int64 _nextId = 1;

        private Int32 _age = 18;
        private Int32 _deathAge = new Random().Next(65, 100);
        private Degree _degree = Degree.Low;
        private Int32 _taxAfterAge45 = 0;
        private Int32 _baseIncome = 5;

        private Residential? _home;
        private Workplace? _workplace;
        private Education? _education;


        #endregion

        #region Properties

        public Degree Degree { get { return _degree; } set { _degree = value; } }
        public Residential? Home
        {
            get => _home;
            set => _home = value;
        }
        public Education? Education { get { return _education; } set { _education = value; } }
        public Workplace? Workplace { get { return _workplace; } set { _workplace = value; } }

        public Int32 CityLeaveTimer { get; set; }
        public Double WorkplaceDistanceHappiness { get; set; }
        public Double RelocationHappiness { get; set; }
        public int DistanceToWorkplace
        {
            get;
            set;
        }

        public Int32 Age
        {
            get => _age;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a person with default age (18)
        /// </summary>
        public Person()
        {
            _id = _nextId++;
        }

        /// <summary>
        /// Constructs a person with specified age
        /// </summary>
        /// <param name="age">Person's specified starting age</param>
        public Person(Int32 age)
        {
            _id = _nextId++;
            _age = age;
            CityLeaveTimer = 12;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Calculates if person is retired or not
        /// </summary>
        /// <returns>Person is retired or not</returns>
        public Boolean IsRetired()
        { return _age >= 65; }

        public Double ResidentialHappiness(City city)
        {
            double sum = 0;
            if (Home != null && Home.ForestBonus > 0 && city.Bonuses[Home.Location.x, Home.Location.y]!.IndustrialBonus.Count() != 0)
            {
                bool canReduce = false;
                int indLocationX = city.Bonuses[Home.Location.x, Home.Location.y]!.IndustrialBonus[0].ind.Location.x;
                int indLocationY = city.Bonuses[Home.Location.x, Home.Location.y]!.IndustrialBonus[0].ind.Location.y;
                var home = Home.Location;
                if (home.x == indLocationX)
                {
                    for (int i = 0; i < city.Bonuses[home.x, home.y]!.Forests.Count(); i++)
                    {
                        if (city.Bonuses[home.x, home.y]!.Forests[i].canSee == true &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.x == home.x &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.y > home.y &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.y < indLocationY)
                        {
                            canReduce = true;
                            break;
                        }
                    }
                }
                else if (home.y == indLocationY)
                {
                    for (int i = 0; i < city.Bonuses[home.x, home.y]!.Forests.Count(); i++)
                    {
                        if (city.Bonuses[home.x, home.y]!.Forests[i].canSee == true &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.y == home.y &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.x > home.x &&
                            city.Bonuses[home.x, home.y]!.Forests[i].forest.Location.x < indLocationX)
                        {
                            canReduce = true;
                            break;
                        }
                    }
                }
                if (canReduce)
                {
                    sum = (city.TaxBonus + city.FundsBonus + city.WorkplaceRateBonus + WorkplaceDistanceHappiness + ((Home.IndustrialDistBonus + 0.05) > 0.2 ? 0.2 : (Home.IndustrialDistBonus + 0.05))
                    + Home.PoliceBonus + Home.StadiumBonus + Home.ForestBonus + RelocationHappiness);
                }
                else
                {
                    sum = (city.TaxBonus + city.FundsBonus + city.WorkplaceRateBonus + WorkplaceDistanceHappiness + Home.IndustrialDistBonus
                    + Home.PoliceBonus + Home.StadiumBonus + Home.ForestBonus + RelocationHappiness);
                }
            }
            else if (Home != null)
            {
                sum = (city.TaxBonus + city.FundsBonus + city.WorkplaceRateBonus + WorkplaceDistanceHappiness + Home.IndustrialDistBonus
                + Home.PoliceBonus + Home.StadiumBonus + Home.ForestBonus + RelocationHappiness);
            }
            return (sum > 1 ? 1 : sum);
        }

        public Double WorkplaceHappiness(City city)
        {
            Double sum = 0;
            if (Workplace != null)
            {
                sum = city.TaxBonus + city.FundsBonus + city.WorkplaceRateBonus + WorkplaceDistanceHappiness + (Workplace.PoliceBonus * 2)
                    + Workplace.ForestBonus + Workplace.StadiumBonus + RelocationHappiness;
            }
            else
            {
                sum = city.TaxBonus + city.FundsBonus + city.WorkplaceRateBonus + WorkplaceDistanceHappiness + RelocationHappiness;
            }
            return (sum > 1 ? 1 : sum);
        }


        /// <summary>
        /// Calculcates the funds change
        /// (either getting paid and then taxed if working, or getting pension if retired)
        /// </summary>
        /// <returns>Tax (non-negative) or pension (negative) amount</returns>
        public Int32 UpdateMonthly(City city)
        {
            Double tmpHappiness = 0;
            if (Home != null)
            {
                tmpHappiness = WorkplaceDistanceHappiness + RelocationHappiness + Home.ForestBonus + Home.StadiumBonus
                    + Home.PoliceBonus + Home.IndustrialDistBonus + city.FundsBonus + city.TaxBonus + city.WorkplaceRateBonus;
            }
            else
            {
                tmpHappiness = WorkplaceDistanceHappiness + RelocationHappiness + city.FundsBonus + city.TaxBonus + city.WorkplaceRateBonus;
            }

            if (tmpHappiness < 0)
            {
                tmpHappiness = 0;
            }
            else if (tmpHappiness > 1)
            {
                tmpHappiness = 1;
            }

            if (tmpHappiness <= 0.3)
            {
                if ((CityLeaveTimer - 1) > 0)
                {
                    CityLeaveTimer -= 1;
                }
                else
                {
                    if (Home != null)
                    {
                        Home.Population.Remove(this);
                    }
                    _home = null;
                    if (Workplace != null)
                    {
                        Workplace.Fire(this, city);
                    }
                }
            }
            else
            {
	            CityLeaveTimer = 12;
            }

            if (RelocationHappiness < 0)
            {
                RelocationHappiness += 0.01;
            }
            return IsRetired() ? -_taxAfterAge45 : (_workplace != null ? _baseIncome * (Int32)_degree : 0);
        }

        /// <summary>
        /// Updates age and education yearly
        /// </summary>
        public void UpdateYearly(City city)
        {
            if (IsRetired())
            {
                city.NumberOfRetired++;
            }
            // if close to retirement
            if (45 < _age && _age < 65)
            {
                // calculate the last 20 years of income, before retiring
                _taxAfterAge45 += _baseIncome * (Int32)_degree / 20;
            }
            // before doing anything, check if person should have died
            if (++_age >= _deathAge)
            {
                // this resets every field/property,
                // so the new person now can search for education and/or workplace
                Die();
            }
            // not studying, but still could
            if (_education == null && _degree != Degree.High)
            {
                if (_degree == Degree.Low)
                {
                    foreach (IField? field in city.Fields)
                    {
                        if (field is School)
                        {
                            ((School)field).Enroll(this);
                        }
                    }
                }
                if (_degree == Degree.Middle)
                {
                    foreach (IField? field in city.Fields)
                    {
                        if (field is University)
                        {
                            ((University)field).Enroll(this);
                        }
                    }
                }
            }
            switch (_degree)
            {
                case Degree.High: city.HighDegreeNumber++; break;
                case Degree.Middle: city.MiddleDegreeNumber++; break;
                case Degree.Low: city.LowDegreeNumber++; break;
            }
        }
        public void ApplyDistanceHappiness()
        {
            WorkplaceDistanceHappiness = DistanceToWorkplace switch
            {
                < 10 => 0.1,
                < 15 => 0.08,
                < 20 => 0.06,
                < 25 => 0.04,
                < 30 => 0.02,
                _ => 0
            };
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Person dies, by resetting every field/property to default
        /// </summary>
        private void Die()
        {
            _age = 18;
            _deathAge = new Random().Next(65, 100);
            _degree = Degree.Low;
            _education?.Withdraw(this);
            Education = null;
            _taxAfterAge45 = 0;
            RelocationHappiness = 0;
            CityLeaveTimer = 12;
        }

        #endregion
    }
}
