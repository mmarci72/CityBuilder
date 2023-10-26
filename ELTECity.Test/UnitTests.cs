using ELTECity.Model;

namespace ELTECity.Test;

[TestClass]
public class UnitTests
{
	#region Fields

	City _model = null!;

	#endregion

	[TestInitialize]
	public void Initialize()
	{
		_model = new City("test", false);
	}

	#region Building

	/// <summary>
	/// Test if buildings are built correctly
	/// </summary>
	[TestMethod]
	public void TestBuildingBuilt()
	{
		_model.Build(1, 1, new Residential());
		_model.Build(1, 2, new Commercial());
		_model.Build(1, 3, new Industrial());
		_model.Build(1, 4, new Forest());
		_model.Build(1, 5, new Road());
		_model.Build(1, 6, new Police());
		_model.Build(2, 1, new School());
		_model.Build(2, 3, new University());
		_model.Build(2, 5, new Stadium());

		Assert.IsTrue(_model[1, 1] is Residential);
		Assert.IsTrue(_model[1, 2] is Commercial);
		Assert.IsTrue(_model[1, 3] is Industrial);
		Assert.IsTrue(_model[1, 4] is Forest);
		Assert.IsTrue(_model[1, 5] is Road);
		Assert.IsTrue(_model[1, 6] is Police);
		Assert.IsTrue(_model[2, 1] is School);
		Assert.IsTrue(_model[2, 2] is School);
		Assert.IsTrue(_model[2, 3] is University);
		Assert.IsTrue(_model[2, 4] is University);
		Assert.IsTrue(_model[3, 3] is University);
		Assert.IsTrue(_model[3, 4] is University);
		Assert.IsTrue(_model[2, 5] is Stadium);
		Assert.IsTrue(_model[2, 6] is Stadium);
		Assert.IsTrue(_model[3, 5] is Stadium);
		Assert.IsTrue(_model[3, 6] is Stadium);
	}


	/// <summary>
	/// Test building on an already built field
	/// </summary>
	[TestMethod]
	public void TestBuildingOnBuiltField()
	{
		_model.Build(1, 1, new Residential());
		_model.Build(1, 2, new Commercial());
		_model.Build(1, 3, new Industrial());
		_model.Build(1, 1, new Commercial());
		Assert.IsTrue(_model[1, 1] is Residential);

		_model.Build(0, 2, new Stadium());
		Assert.IsTrue(_model[0, 2] is Road);
		Assert.IsTrue(_model[1, 2] is Commercial);
		Assert.IsNull(_model[0, 3]);
		Assert.IsTrue(_model[1, 3] is Industrial);

	}

	/// <summary>
	/// Test building outside the bounds of the map
	/// </summary>
	[TestMethod]
	public void TestBuildingOutsideBounds()
	{
		_model.Build(49, 49, new School());
		Assert.IsNull(_model[49, 49]);
		_model.Build(49, 49, new University());
		Assert.IsNull(_model[49, 49]);
		_model.Build(49, 49, new Stadium());
		Assert.IsNull(_model[49, 49]);
	}

	#endregion

	#region Upgrade

	/// <summary>
	/// Test upgrading a residential building
	/// </summary>
	[TestMethod]
	public void TestResidentialUpgrade()
	{
		_model.Build(0, 1, new Residential());
		Int32 previousFunds = _model.Funds;

		_model.Upgrade(0, 1);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.Medium, (_model[0, 1] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from Medium to High
		_model.Upgrade(0, 1);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[0, 1] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from High to High
		_model.Upgrade(0, 1);
		Assert.AreEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[0, 1] as Zone)?.Density);
	}

	/// <summary>
	/// Test upgrading a commercial building
	/// </summary>
	[TestMethod]
	public void TestCommercialUpgrade()
	{
		_model.Build(1, 2, new Commercial());
		Int32 previousFunds = _model.Funds;

		// from Low to Medium
		_model.Upgrade(1, 2);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.Medium, (_model[1, 2] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from Medium to High
		_model.Upgrade(1, 2);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[1, 2] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from High to High
		_model.Upgrade(1, 2);
		Assert.AreEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[1, 2] as Zone)?.Density);
	}

	/// <summary>
	/// Test upgrading a industrial building
	/// </summary>
	[TestMethod]
	public void TestIndustrialUpgrade()
	{
		_model.Build(0, 3, new Industrial());
		Int32 previousFunds = _model.Funds;

		_model.Upgrade(0, 3);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.Medium, (_model[0, 3] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from Medium to High
		_model.Upgrade(0, 3);
		Assert.AreNotEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[0, 3] as Zone)?.Density);
		previousFunds = _model.Funds;

		// from High to High
		_model.Upgrade(0, 3);
		Assert.AreEqual(previousFunds, _model.Funds);
		Assert.AreEqual(Density.High, (_model[0, 3] as Zone)?.Density);
	}

	#endregion

	#region Demolish

	/// <summary>
	/// Test demolishing buildings
	/// </summary>
	[TestMethod]
	public void TestDemolish()
	{
		_model.Build(1, 1, new Residential());
		_model.Build(1, 2, new Commercial());
		_model.Build(1, 3, new Industrial());
		_model.Build(1, 4, new Forest());
		_model.Build(1, 5, new Road());
		_model.Build(1, 6, new Police());
		_model.Build(2, 1, new School());
		_model.Build(2, 3, new University());
		_model.Build(2, 5, new Stadium());

		_model.Demolish(1, 1);
		_model.Demolish(1, 2);
		_model.Demolish(1, 3);
		_model.Demolish(1, 4);
		_model.Demolish(1, 5);
		_model.Demolish(1, 6);
		_model.Demolish(2, 1);
		_model.Demolish(2, 3);
		_model.Demolish(2, 5);

		Assert.IsNull(_model[1, 1]);
		Assert.IsNull(_model[1, 2]);
		Assert.IsNull(_model[1, 3]);
		Assert.IsNull(_model[1, 4]);
		Assert.IsNull(_model[1, 5]);
		Assert.IsNull(_model[1, 6]);
		Assert.IsNull(_model[2, 1]);
		Assert.IsNull(_model[2, 2]);

		Assert.IsNull(_model[2, 3]);
		Assert.IsNull(_model[2, 4]);
		Assert.IsNull(_model[3, 3]);
		Assert.IsNull(_model[3, 4]);

		Assert.IsNull(_model[2, 5]);
		Assert.IsNull(_model[2, 6]);
		Assert.IsNull(_model[3, 5]);
		Assert.IsNull(_model[3, 6]);

	}


	/// <summary>
	/// Test demolishing the highway
	/// </summary>
	[TestMethod]
	public void TestHighwayDemolish()
	{
		_model.Demolish(0, 2);
		Assert.IsTrue(_model[0, 2] is Road);
	}

	[TestMethod]
	public void TestNullDemolish()
	{
		Assert.IsNull(_model.Fields[10,10]);
		_model.Demolish(10, 10);
		Assert.IsNull(_model.Fields[10,10]);
	}

	[TestMethod]
	public void TestMoveOutWhenRoadDemolished()
	{
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());
		_model.Build(3, 2, new Road());
		_model.Build(0, 1, new Residential());
		_model.Build(3, 1, new Industrial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		Assert.IsTrue((_model.Fields[0,1] as Residential)?.Population.Count > 0);
		Assert.IsTrue((_model.Fields[3,1] as Industrial)?.Population.Count > 0);



		_model.Demolish(2, 2);

		Assert.IsNull(_model.Fields[2,2]);
		Assert.IsTrue(_model.Fields[0,1]?.Connected);
		Assert.IsTrue(!_model.Fields[3,1]?.Connected);
		Assert.AreEqual(0,(_model.Fields[0,1] as Residential)?.Population.Count);
		Assert.AreEqual(0,(_model.Fields[3,1] as Industrial)?.Population.Count);

	}

	[TestMethod]
	public void TestRelocationWhenRoadDemolished()
	{
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());
		_model.Build(3, 2, new Road());
		_model.Build(0, 1, new Residential());
		_model.Build(3, 1, new Industrial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}
		_model.Build(0, 3, new Industrial());

		Assert.IsTrue((_model.Fields[0,1] as Residential)?.Population.Count > 0);
		Assert.IsTrue((_model.Fields[3,1] as Industrial)?.Population.Count > 0);
		Assert.AreEqual(0, (_model.Fields[0, 3] as Industrial)?.Population.Count);

		_model.Demolish(2, 2);

		Assert.IsNull(_model.Fields[2,2]);
		Assert.IsTrue(_model.Fields[0,1]?.Connected);
		Assert.IsTrue(!_model.Fields[3,1]?.Connected);
		Assert.IsTrue((_model.Fields[0,1] as Residential)?.Population.Count > 0);
		Assert.AreEqual(0, (_model.Fields[3,1] as Industrial)?.Population.Count);
		Assert.IsTrue((_model.Fields[0,3] as Industrial)?.Population.Count > 0);

		_model.Demolish(0, 1);
		_model.Demolish(3, 1);
		_model.Demolish(0, 3);

		_model.Build(2, 2, new Road());
		_model.Build(0, 1, new Industrial());
		_model.Build(3, 1, new Residential());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		_model.Build(0, 3, new Residential());

		Assert.IsTrue((_model.Fields[0,1] as Industrial)?.Population.Count > 0);
		Assert.IsTrue((_model.Fields[3,1] as Residential)?.Population.Count > 0);
		Assert.IsTrue((_model.Fields[0,3] as Residential)?.Population.Count == 0);

		_model.Demolish(2, 2);

		Assert.IsNull(_model.Fields[2,2]);
		Assert.IsTrue(_model.Fields[0,1]?.Connected);
		Assert.IsTrue(!_model.Fields[3,1]?.Connected);
		Assert.IsTrue((_model.Fields[0,1] as Industrial)?.Population.Count > 0);
		Assert.AreEqual(0,(_model.Fields[3,1] as Residential)?.Population.Count);
		Assert.IsTrue((_model.Fields[0,3] as Residential)?.Population.Count > 0);
	}


	#endregion

	#region Education

	/// <summary>
	/// Test whether people are getting the right degree
	/// </summary>
	[TestMethod]
	public void TestDegree()
	{
		_model.Funds = 1000000;
		_model.Build(0, 3, new Stadium());
		_model.Build(0, 1, new Residential());
		_model.Build(2, 3, new Industrial());
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());
		_model.Build(3, 2, new Road());
		_model.Build(1, 0, new School());

		var school = _model.Fields[1, 0] as School;
		var residential = _model.Fields[0, 1] as Residential;

		for (int i = 0; i < 366; i++)
		{
			_model.AdvanceDate();
		}

		Person? student = residential?.Population.FirstOrDefault(p => p.Education == school);
		Assert.IsTrue(school?.Students.Count > 0);
		Assert.IsNotNull(student);
		Assert.IsTrue(student.Degree == Degree.Low);

		for (int i = 0; i < 4 * 365; i++)
		{
			_model.AdvanceDate();
		}

		Assert.IsTrue(student.Degree == Degree.Middle);

		_model.Build(2, 0, new University());

		var university = _model.Fields[2, 0] as University;

		for (int i = 0; i < 366; i++)
		{
			_model.AdvanceDate();
		}

		Assert.IsTrue(university?.Students.Count > 0);
		Assert.IsTrue(residential?.Population.Any(p => p.Education == university));

		for (int i = 0; i < 4 * 365; i++)
		{
			_model.AdvanceDate();
		}

		Assert.IsTrue(student.Degree == Degree.High);
	}

	#endregion

	#region MoveIn

	[TestMethod]

	public void TestMovingInAfterBuilding()
	{
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Industrial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		Assert.AreEqual(1,(_model.Fields[0, 1] as Residential)?.Population.Count);
		Assert.AreEqual(1,(_model.Fields[0, 3] as Industrial)?.Population.Count);
	}

	[TestMethod]
	public void TestClosestWorkplace()
	{
		
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Industrial());
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());
		_model.Build(2, 1, new Industrial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		Assert.AreEqual(1,(_model.Fields[0, 1] as Residential)?.Population.Count);
		Assert.AreEqual(1,(_model.Fields[0, 3] as Industrial)?.Population.Count);
		Assert.AreEqual(0,(_model.Fields[2,1] as Industrial)?.Population.Count);
	}

	[TestMethod]
	public void TestWorkplaceRatio()
	{
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Industrial());
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());
		_model.Build(2, 1, new Commercial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		
		Assert.AreEqual(1,(_model.Fields[0, 1] as Residential)?.Population.Count);
		Assert.AreEqual(1,(_model.Fields[0, 3] as Industrial)?.Population.Count);
		Assert.AreEqual(0,(_model.Fields[2,1] as Commercial)?.Population.Count);

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		Assert.AreEqual(1,(_model.Fields[2,1] as Commercial)?.Population.Count);

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		Assert.AreEqual(2,(_model.Fields[0, 3] as Industrial)?.Population.Count);

	}

	#endregion

	#region Happiness

	[TestMethod]
	public void TestStadiumHappiness()
	{

		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Commercial());
		_model.Build(1, 2, new Road());

		for (int i = 0; i < 80; i++)
		{
			_model.AdvanceDate();
		}
		double? prevHappiness = (_model.Fields[0, 1] as Residential)?.HappinessRate(_model);


		_model.Build(1, 0, new Stadium());

		Assert.AreEqual(Math.Round((prevHappiness??0)+0.2, 2),  Math.Round(((_model.Fields[0, 1] as Residential)?.HappinessRate(_model) ?? 0), 2));

		_model.Demolish(1, 0);

		Assert.AreEqual(Math.Round((prevHappiness??0), 2),  Math.Round(((_model.Fields[0, 1] as Residential)?.HappinessRate(_model) ?? 0), 2));

	}

	[TestMethod]
	public void TestPoliceBonus()
	{
		
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Commercial());
		_model.Build(1, 2, new Road());

		for (int i = 0; i < 80; i++)
		{
			_model.AdvanceDate();
		}
		double? prevHappiness = (_model.Fields[0, 1] as Residential)?.HappinessRate(_model);


		_model.Build(1, 1, new Police());

		Assert.AreEqual(Math.Round((prevHappiness??0)+0.2, 2),  Math.Round(((_model.Fields[0, 1] as Residential)?.HappinessRate(_model) ?? 0), 2));

		_model.Demolish(1, 1);

		Assert.AreEqual(Math.Round((prevHappiness??0), 2),  Math.Round(((_model.Fields[0, 1] as Residential)?.HappinessRate(_model) ?? 0), 2));
	}

	[TestMethod]
	public void TestHappinessRate()
	{
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Industrial());

		for (int i = 0; i < 80; i++)
		{
			_model.AdvanceDate();
		}

		var happiness = _model.HappinessRate();

		_model.Build(1, 2, new Road());
		_model.Build(1, 0, new Stadium());

		Assert.IsTrue(happiness < _model.HappinessRate());
	}

	[TestMethod]
	public void TestPersonLeavingCity()
	{
		_model.Funds = -2000;
		_model.Build(0, 1, new Residential());
		_model.Build(2, 3, new Industrial());
		_model.Build(1, 2, new Road());
		_model.Build(2, 2, new Road());


		for (int i = 0; i < 80; i++)
		{
			_model.AdvanceDate();
		}

		Person person = (_model.Fields[0, 1] as Residential)!.Population[0];
		_model.Build(0, 3, new Industrial());
		_model.Demolish(1, 2);

		Assert.AreEqual((_model.Fields[0, 1] as Residential), person.Home);

		while(person.Home != null)
		{
            _model.AdvanceDate();
            _model.Build(1, 2, new Road());
            if (person.Workplace != null && person.Workplace.Location.x == 0 && person.Workplace.Location.y == 3)
            {
                _model.Demolish(0, 3);
                _model.Build(0, 3, new Industrial());
            }
            else if (person.Workplace != null && person.Workplace.Location.x == 2 && person.Workplace.Location.y == 3)
            {
                _model.Demolish(2, 3);
                _model.Build(2, 3, new Industrial());
            }
        }
		//for (int i = 0; i < 365; i++)
		//{
			
		//}

		Assert.IsNull(person.Home);

	}

	#endregion

	#region Pension

	[TestMethod]
	public void TestRetired()
	{
		_model.Build(0, 1, new Residential());
		_model.Build(0, 3, new Commercial());

		for (int i = 0; i < 40; i++)
		{
			_model.AdvanceDate();
		}

		int? age = (_model.Fields[0, 1] as Residential)?.Population[0].Age;
		int? prevAge = age;
		Assert.IsTrue(!(_model.Fields[0, 1] as Residential)?.Population[0].IsRetired());
		for (int i = 0; i < 365 * (65-age); i++)
		{
			_model.AdvanceDate();
			if (_model.DateTime.DayOfYear == 1)
			{
				Assert.AreEqual(prevAge+1, (_model.Fields[0, 1] as Residential)?.Population[0].Age);
				prevAge++;
			}
		}

		Assert.IsTrue((_model.Fields[0, 1] as Residential)?.Population[0].IsRetired());

	}

	#endregion

	#region Forest

	[TestMethod]
	public void TestForestGeneration()
	{
		_model = new City("test", true);

		int forestCount = 0;

		for (int i = 0; i < _model.MapSize; i++)
		{
			for (int j = 0; j < _model.MapSize; j++)
			{
				if (_model.Fields[i, j] is Forest)
				{
					forestCount++;
				}
			}
		}

		Assert.IsTrue(forestCount > 0);
	}

	[TestMethod]
	public void TestForestAge()
	{
		_model.Build(0, 1, new Forest());

		Forest? forest = (_model.Fields[0, 1] as Forest);

		Assert.AreEqual(0, forest?.Age);

		for (int i = 0; i < 11*365; i++)
		{
			int? prevAge = forest?.Age;
			_model.AdvanceDate();
			if (_model.DateTime.DayOfYear == 1)
			{
				Assert.AreEqual(prevAge+1, forest?.Age);
			}
		}
	}

	#endregion

	#region Misc

	[TestMethod]
	public void TestHighwayLocation()
	{
		Assert.AreEqual(0,_model.Highway.x);
		Assert.AreEqual(2,_model.Highway.y);
	}

	[TestMethod]
	[ExpectedException(typeof(ArgumentException))]
	public void TestEmptyCityNameException()
	{
		_model.NewGame("", false);
	}

	[TestMethod]
	public void TestGameOver()
	{
		_model.Funds = -1000000;
		bool eventFired = false;	
		_model.GameOverEvent += (sender, args) => eventFired = true;

		for (int i = 0; i < 400; i++)
		{
			_model.AdvanceDate();
		}

		Assert.IsTrue(eventFired);
	}

	#endregion
	
}