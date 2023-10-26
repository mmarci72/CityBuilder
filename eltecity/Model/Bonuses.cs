using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELTECity.Model
{
	public class Bonuses
	{
		public List<Double> PoliceBonus;
		public List<Double> StadiumBonus;
		public List<Double> ForestBonus;
		public List<(Industrial ind, Double bonus)> IndustrialBonus;
		public List<(Forest forest, Boolean canSee)> Forests;

		public Bonuses()
		{
			PoliceBonus = new List<Double>();
			StadiumBonus = new List<Double>();
			ForestBonus = new List<Double>();
			Forests = new List<(Forest forest, Boolean canSee)>();
			IndustrialBonus = new List<(Industrial ind, Double bonus)>();

			PoliceBonus.Add(0.0);
			StadiumBonus.Add(0.0);
			ForestBonus.Add(0.0);
		}
	};
}
