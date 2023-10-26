using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ELTECity.Model
{
	public class City
	{
		private class WorkplaceWithDist
		{
			public ((Commercial? comm, int dist) commercial, (Industrial? ind, int dist) industrial) Workplace
			{
				get;
				set;
			}

			public WorkplaceWithDist()
			{
				Workplace = ((null, 0), (null, 0));
			}
		};

		#region Fields

		private const Int32 Radius = 15;
		private Bonuses?[,] _bonuses;
		private Double _taxBonus = 0;
		private Double _fundsBonus = 0.2;
		private Double _workplaceRateBonus = 0.1;

		private const Int32 StartFund = 2000;
		private const Double DefaultTaxPercentage = 0.5;
		private const Double DegreeLimit = 0.3;
		private const Int32 DebtForGameOver = -3000;

		private static readonly Int32 mapSize = 50;

		private IField?[,] _fields;
		private (Int32 x, Int32 y) _highway;
		private Int32 _funds;
		private DateTime _dateTime;
		private Double _taxPercentage;
		private String _name = String.Empty;
		private (Int32 commercial, Int32 industrial) _workerCount = (0, 0);
		private List<Residential> _residentialFields = new();
		private List<Workplace> _workplaceFields = new();
		private Int32 _gameOverTimer = 0;

		public List<(String desc, Int32 money)> _expense_income = new();

		#endregion

		#region Properties

		public Int32 PopulationSize => PopulationCount();

		public Int32 MapSize => mapSize;

		public String Name => _name;

		public IField?[,] Fields
		{
			get => _fields;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Field cannot be null " + nameof(value));
				}

				_fields = value;
			}
		}

		public (Int32 x, Int32 y) Highway
		{
			get { return _highway; }
		}

		public Bonuses?[,] Bonuses
		{
			get => _bonuses;
			set => _bonuses = value;
		}

		public Double TaxBonus => _taxBonus;
		public Double FundsBonus => _fundsBonus;
		public Double WorkplaceRateBonus => _workplaceRateBonus;

		public Int32 GuaranteedPopulation { get; set; } = 100;

		public Double TaxPercentage
		{
			get => _taxPercentage;
			set
			{
				if (value > 1.0 || value < 0.0)
					throw new ArgumentException("The tax cannot be higher than 100 or lower than 0");
				_taxPercentage = value;

				if (_taxPercentage <= 0.15)
					_taxBonus = 0.2;
				else if (_taxPercentage <= 0.25)
					_taxBonus = 0.15;
				else if (_taxPercentage <= 0.35)
					_taxBonus = 0.1;
				else if (_taxPercentage <= 0.45)
					_taxBonus = 0.05;
				else
					_taxBonus = 0;
			}
		}

		public DateTime DateTime
		{
			get => _dateTime;
			set
			{
				if (_dateTime.AddDays(1) != value)
				{
					throw new ArgumentException("Date can only be incremented by one day at a time");
				}

				_dateTime = value;
			}
		}

		public Int32 Funds
		{
			get => _funds;
			set { _funds = value; }
		}

		public IField? this[int x, int y]
		{
			get { return IndexOutOfRange(x, y) ? null : _fields[x, y]; }
			set { _fields[x, y] = value; }
		}

		public Int32 LastYearIncome { get; set; }
		public Int32 LastYearExpense { get; set; }
		public Int32 LastYearProfit { get; set; }
		public Int32 CurrentYearIncome { get; set; }
		public Int32 CurrentYearExpense { get; set; }
		public Int32 UpkeepCost { get; set; }
		public Int32 Pensions { get; set; }

		public Int32 LowDegreeNumber { get; set; } = 0;
		public Int32 MiddleDegreeNumber { get; set; } = 0;
		public Int32 HighDegreeNumber { get; set; } = 0;

		public Int32 NumberOfRetired { get; set; } = 0;

		#endregion

		#region Events

		public event EventHandler<(int, int)>? DemolishEvent;
		public event EventHandler<(int, int)>? ConnectedChangedEvent;
		public event EventHandler? GameOverEvent;
		public event EventHandler? ExpenseIncomeChangedEvent;
		public event EventHandler<(int, int)>? UtilizationChangedEvent;

		#endregion

		#region Constructor

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">The name of the city</param>
		/// <param name="withForest">Generate forests on map or not</param>
		public City(String name, Boolean withForest)
		{
			_highway = (0, 2);
			_bonuses = new Bonuses[mapSize, mapSize];
			_fields = new IField[mapSize, mapSize];
			NewGame(name, withForest);
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Happiness of the entire population
		/// </summary>
		/// <returns> Happiness of the city in percentage </returns>
		public Double HappinessRate()
		{
			Double sum = 0;
			Double count = 0;
			foreach (IField? field in _fields)
			{
				if (field is Zone zone)
				{
					if (zone.Connected && zone.Population.Any())
					{
						sum += zone?.HappinessRate(this) ?? 0;
						count++;
					}
				}
			}

			if (count == 0)
			{
				return 0;
			}

			return (sum / count);
		}

		/// <summary>
		/// Limit of school degrees
		/// </summary>
		/// <returns></returns>
		public Double MiddleDegreeLimit()
		{
			return DegreeLimit * 2;
		}

		/// <summary>
		/// Limit of university degrees
		/// </summary>
		/// <returns></returns>
		public Double HighDegreeLimit()
		{
			return DegreeLimit;
		}

		public void AddExpenseIncome(string ms, Int32 money)
		{
			_expense_income.Insert(0, (ms, money));
			if (_expense_income.Count() > 100)
			{
				_expense_income.RemoveAt(100);
			}
		}

		#endregion

		#region Public game methods

		/// <summary>
		/// Initializes the required fields after starting a new game
		/// </summary>
		/// <param name="name">The name of the city</param>
		/// <exception cref="ArgumentException">Thrown when we didn't pass a proper name for the city</exception>
		public void NewGame(String name, Boolean withForest)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Name cannot be empty or null");
			}

			for (int i = 0; i < mapSize; i++)
			{
				for (int j = 0; j < mapSize; j++)
				{
					_bonuses[i, j] = new Bonuses();
				}
			}

			_dateTime = new DateTime(1984, 4, 20);
			_funds = StartFund;
			_taxPercentage = DefaultTaxPercentage;
			_name = name;

			for (int i = 0; i < mapSize; i++)
			{
				for (int j = 0; j < mapSize; j++)
				{
					_fields[i, j] = null;
				}
			}

			_fields[_highway.x, _highway.y] = new Road
			{
				Connected = true,
				Location = (_highway.x, _highway.y)
			};

			if (withForest)
			{
				GenerateForests();
			}
		}

		/// <summary>
		/// Increments the date by one day and updates the fields accordingly
		/// </summary>
		public void AdvanceDate()
		{
			_dateTime = _dateTime.AddDays(1);
			UpdateDaily();
		}

		/// <summary>
		/// Builds the building to (x, y)
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="building">Building type</param>
		/// <returns>Building was successful or not</returns>
		public Boolean Build(Int32 x, Int32 y, IField building)
		{
			if (!CanBuildingBePlaced(x, y, building))
			{
				return false;
			}

			PlaceBuilding(x, y, building);

			bool canBuild = building is Forest;

			if (building is Road)
			{
				UpdateConnected();
				for (Int32 i = 0; i < building.Size.x; i++)
				{
					for (Int32 j = 0; j < building.Size.y; j++)
					{
						if (_fields[x + i, y + j]!.Connected)
						{
							canBuild = true;
						}
					}
				}
			}

			// If it's any other building we search for an adjacent connected road
			else
			{
				for (Int32 i = 0; i < building.Size.x; i++)
				{
					for (Int32 j = 0; j < building.Size.y; j++)
					{
						if (CheckAdjacentRoad((x + i), (y + j)))
						{
							canBuild = true;
						}
					}
				}
			}

			if (canBuild)
			{
				if (building is Residential)
				{
					_residentialFields.Add((Residential) building);
				}
				else if (building is Workplace)
				{
					_workplaceFields.Add((Workplace) building);
				}

				// subtracts the price from funds
				_funds -= building.Price;
				CurrentYearExpense += building.Price;
				AddExpenseIncome("Building cost: ", -building.Price);
				ExpenseIncomeChangedEvent?.Invoke(this, EventArgs.Empty);
				ApplyBonuses(x, y, Radius, building);

				return true;
			}

			// otherwise the building's placement is illegal
			for (Int32 i = 0; i < building.Size.x; i++)
			{
				for (Int32 j = 0; j < building.Size.y; j++)
				{
					_fields[x + i, y + j]!.Connected = false;
				}
			}

			for (int i = 0; i < _bonuses[building.Location.x, building.Location.y]!.Forests.Count(); i++)
			{
				ApplyForestBonus(_bonuses[building.Location.x, building.Location.y]!.Forests[i].forest.Location.x,
					_bonuses[building.Location.x, building.Location.y]!.Forests[i].forest.Location.y,
					_bonuses[building.Location.x, building.Location.y]!.Forests[i].forest);
			}

			return true;
		}

		/// <summary>
		/// Check if there's any connected road adjacent to the field being checked
		/// </summary>
		/// <param name="x">The x coordinate of the field</param>
		/// <param name="y"> The y coordinate of the field</param>
		/// <returns>Whether there's any connected road adjacent to the field</returns>
		private bool CheckAdjacentRoad(int x, int y)
		{
			if (!IndexOutOfRange(x + 1, y) && Fields[x + 1, y] != null)
			{
				if (Fields[x + 1, y] is Road && Fields[x + 1, y]!.Connected == true)
				{
					return true;
				}
			}

			if (!IndexOutOfRange(x - 1, y) && Fields[x - 1, y] != null)
			{
				if (Fields[x - 1, y] is Road && Fields[x - 1, y]!.Connected == true)
				{
					return true;
				}
			}

			if (!IndexOutOfRange(x, y + 1) && Fields[x, y + 1] != null)
			{
				if (Fields[x, y + 1] is Road && Fields[x, y + 1]!.Connected == true)
				{
					return true;
				}
			}

			if (!IndexOutOfRange(x, y - 1) && Fields[x, y - 1] != null)
			{
				if (Fields[x, y - 1] is Road && Fields[x, y - 1]!.Connected == true)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Demolishes the building on (x, y)
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>Demolition was successful or not</returns>
		public Boolean Demolish(Int32 x, Int32 y)
		{
			if (_fields[x, y] == null || IndexOutOfRange(x, y))
			{
				return false;
			}

			if ((x, y) == _highway)
			{
				return false;
			}

			int buildingX = _fields[x, y]!.Location.x;
			int buildingY = _fields[x, y]!.Location.y;

			if (_fields[x, y]!.Connected)
			{
				DemolishSideEffects(x, y);
				_funds += (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);
				CurrentYearIncome += (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);
				AddExpenseIncome("Demolish connected building: ", (int) Math.Round(_fields[x, y]!.Price * 0.3, 0));
			}
			else
			{
				_funds += (int) Math.Round(_fields[x, y]!.Price * 0.5, 0);
				CurrentYearIncome += (int) Math.Round(_fields[x, y]!.Price * 0.5, 0);
				AddExpenseIncome("Demolish unconnected building: ", (int) Math.Round(_fields[x, y]!.Price * 0.5, 0));
			}


			if (_fields[x, y] is Road)
			{
				DemolishRoad(x, y);
			}
			else if (_fields[x, y] is Workplace workplace)
			{
				_funds -= (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);
				CurrentYearExpense += (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);

				DemolishSameBuildingRecursive(x, y, workplace);
				_workplaceFields.Remove(workplace);

				WorkplaceRelocate(workplace);
			}
			else if (_fields[x, y] is Residential)
			{
				_funds -= (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);
				CurrentYearExpense += (int) Math.Round(_fields[x, y]!.Price * 0.3, 0);

				Residential? residentialBuilding = _fields[x, y] as Residential;

				ResidentialRelocate(residentialBuilding);

				_residentialFields.Remove(residentialBuilding!);
				DemolishSameBuildingRecursive(x, y, residentialBuilding);
			}
			else
			{
				IField? building = _fields[x, y];
				DemolishSameBuildingRecursive(x, y, building);
			}

			return true;
		}

		/// <summary>
		/// Employs a person based on the distance to the workplace and the ratio of people working in industrial and commercial zones
		/// </summary>
		/// <param name="homeX">X coordinate of the person's home</param>
		/// <param name="homeY">Y coordinate of the person's home</param>
		/// <param name="person">The person to search a workplace for</param>
		/// <returns>Whether it found a workplace or not</returns>
		public bool SearchWorkplace(int homeX, int homeY, Person person)
		{
			PriorityQueue<Road?, int> queue = new PriorityQueue<Road?, int>();
			Dictionary<Road, (int, Road? prevRoad)> roads = new Dictionary<Road, (int, Road? prevRoad)>();

			if (!IndexOutOfRange(homeX + 1, homeY) && _fields[homeX + 1, homeY] is Road)
			{
				queue.Enqueue(_fields[homeX + 1, homeY] as Road, 1);
				roads.Add((_fields[homeX + 1, homeY] as Road)!, (1, null));
			}

			if (!IndexOutOfRange(homeX - 1, homeY) && _fields[homeX - 1, homeY] is Road)
			{
				queue.Enqueue(_fields[homeX - 1, homeY] as Road, 1);
				roads.Add((_fields[homeX - 1, homeY] as Road)!, (1, null));
			}

			if (!IndexOutOfRange(homeX, homeY + 1) && _fields[homeX, homeY + 1] is Road)
			{
				queue.Enqueue(_fields[homeX, homeY + 1] as Road, 1);
				roads.Add((_fields[homeX, homeY + 1] as Road)!, (1, null));
			}

			if (!IndexOutOfRange(homeX, homeY - 1) && _fields[homeX, homeY - 1] is Road)
			{
				queue.Enqueue(_fields[homeX, homeY - 1] as Road, 1);
				roads.Add((_fields[homeX, homeY - 1] as Road)!, (1, null));
			}

			WorkplaceWithDist? closestWorkplaces = new WorkplaceWithDist();

			SearchWorkplaceDijkstra(queue, roads, new HashSet<Road>(), ref closestWorkplaces);

			if (closestWorkplaces != null)
			{
				if (closestWorkplaces.Workplace.industrial.ind == null &&
				    closestWorkplaces.Workplace.commercial.comm == null)
				{
					return false;
				}
			}
			else
			{
				return false;
			}

			if (closestWorkplaces.Workplace.industrial.ind != null &&
			    closestWorkplaces.Workplace.commercial.comm == null)
			{
				_workerCount.industrial++;
				person.DistanceToWorkplace = closestWorkplaces.Workplace.industrial.dist;
				closestWorkplaces.Workplace.industrial.ind.Employ(person, this);
			}
			else if (closestWorkplaces.Workplace.industrial.ind == null &&
			         closestWorkplaces.Workplace.commercial.comm != null)
			{
				_workerCount.commercial++;
				person.DistanceToWorkplace = closestWorkplaces.Workplace.commercial.dist;
				closestWorkplaces.Workplace.commercial.comm.Employ(person, this);
			}
			else
			{
				EmployByRatio(closestWorkplaces, person);
			}

			person.ApplyDistanceHappiness();
			return true;
		}

		/// <summary>
		/// Updates the fields' Connected property via the recursive private UpdateConnected() method
		/// </summary>
		public void UpdateConnected()
		{
			// resets all property before setting them true again
			for (Int32 i = 0; i < _fields.GetLength(0); i++)
			{
				for (Int32 j = 0; j < _fields.GetLength(1); j++)
				{
					if (_fields[i, j] != null && !(_fields[i, j] is Forest))
					{
						_fields[i, j]!.Connected = false;
						ConnectedChangedEvent?.Invoke(this, (i, j));
					}

					if (_bonuses[i,j] != null && _bonuses[i, j]!.Forests.Count() != 0)
					{
						var forests = _bonuses[i, j]!.Forests;
						_bonuses[i, j] = new Bonuses();

						_bonuses[i, j]!.Forests = forests;
					}
					else
					{
						_bonuses[i, j] = new Bonuses();
					}
				}
			}

			_fields[_highway.x, _highway.y]!.Connected = true;
			ConnectedChangedEvent?.Invoke(this, (_highway.x, _highway.y));
			// calls the recursion for the starting point (a.k.a. the highway) first
			UpdateConnectedRecursive(_highway.x, _highway.y);
		}

		public Boolean Upgrade(Int32 x, Int32 y)
		{
			if (_fields[x, y] is Zone)
			{
				Int32 price = (_fields[x, y] as Zone)!.Upgrade();
				_funds += price;
				CurrentYearExpense += Math.Abs(price);
				if (price != 0)
				{
					AddExpenseIncome("Zone upgrade: ", price);
				}

				ExpenseIncomeChangedEvent?.Invoke(this, EventArgs.Empty);
				return price != 0;
			}

			return false;
		}

		/// <summary>
		/// Apply bonuses on different fields when you build
		/// </summary>
		public void ApplyBonuses(int x, int y, int radius, IField field)
		{
			if (field is Police || field is Industrial)
			{
				ApplyPoliceAndIndustrialBonus(radius, field);
			}
			else if (field is Residential || field is Commercial)
			{
				if (field is Residential)
				{
					(field as Residential)!.IndustrialDistBonus = _bonuses[x, y]!.IndustrialBonus.Count() != 0
						? _bonuses[x, y]!.IndustrialBonus[0].bonus
						: 0.2;
					(field as Residential)!.PoliceBonus = _bonuses[x, y]!.PoliceBonus[0];
					(field as Residential)!.StadiumBonus = _bonuses[x, y]!.StadiumBonus[0];
					for (int z = 0; z < _bonuses[field.Location.x, field.Location.y]!.Forests.Count(); z++)
					{
						if (_bonuses[field.Location.x, field.Location.y]!.Forests[z].canSee == true)
						{
							(_fields[x, y] as Zone)!.ForestBonus = _bonuses[x, y]!.Forests[z].forest.Age > 10
								? 0.1
								: _bonuses[x, y]!.Forests[z].forest.Age * 0.01;
							break;
						}
					}
				}
				else
				{
					(field as Zone)!.PoliceBonus = _bonuses[x, y]!.PoliceBonus[0];
					(field as Zone)!.StadiumBonus = _bonuses[x, y]!.StadiumBonus[0];
					for (int z = 0; z < _bonuses[field.Location.x, field.Location.y]!.Forests.Count(); z++)
					{
						if (_bonuses[field.Location.x, field.Location.y]!.Forests[z].canSee == true)
						{
							(_fields[x, y] as Zone)!.ForestBonus = _bonuses[x, y]!.Forests[z].forest.Age > 10
								? 0.1
								: _bonuses[x, y]!.Forests[z].forest.Age * 0.01;
							break;
						}
					}
				}
			}
			else if (field is Stadium)
			{
				ApplyStadiumBonus(radius, field);
			}
			else if (field is Forest)
			{
				ApplyForestBonus(x, y, (field as Forest)!);
			}

			if (field != null && field is not Road && field is not Forest)
			{
				for (int i = 0; i < _bonuses[field.Location.x, field.Location.y]!.Forests.Count(); i++)
				{
					ApplyForestBonus(_bonuses[field.Location.x, field.Location.y]!.Forests[i].forest.Location.x,
						_bonuses[field.Location.x, field.Location.y]!.Forests[i].forest.Location.y,
						_bonuses[field.Location.x, field.Location.y]!.Forests[i].forest);
				}
			}
		}


		public void ApplyForestBonus(int x, int y, Forest f)
		{
			int x1 = x;
			int y1 = y;
			int radius = 3;
			for (Int32 i = -radius; i <= radius; i++)
			{
				for (Int32 j = -radius; j <= radius; j++)
				{
					bool canApply = true;
					int startx0 = (x + i);
					int starty0 = (y + j);
					int x0 = (x + i);
					int y0 = (y + j);
					if (!IndexOutOfRange(x + i, y + j))
					{
						int dx = Math.Abs(x1 - x0);
						int sx = x0 < x1 ? 1 : -1;

						int dy = -Math.Abs(y1 - y0);
						int sy = y0 < y1 ? 1 : -1;

						int e = dx + dy;
						int e2 = 0;

						while (x0 != x1 || y0 != y1)
						{
							if ((x0 != startx0 || y0 != starty0) && _fields[x0, y0] != null &&
							    _fields[x0, y0] is not Road)
							{
								canApply = false;
								break;
							}

							e2 = 2 * e;
							if (e2 >= dy)
							{
								if (x0 == x1)
								{
									break;
								}

								e += dy;
								x0 += sx;
							}

							if (e2 <= dx)
							{
								if (y0 == y1)
								{
									break;
								}

								e += dx;
								y0 += sy;
							}
						}

						bool canAddForest = true;
						bool bonusChange = true;
						if (_bonuses[x + i, y + j] == null || _bonuses[startx0, starty0] == null)
						{
							return;
						}

						if (canApply)
						{
							for (int a = 0; a < _bonuses[x + i, y + j]!.Forests.Count(); a++)
							{
								if (_bonuses[x + i, y + j]!.Forests[a].forest != f &&
								    _bonuses[x + i, y + j]!.Forests[a].canSee == true)
								{
									bonusChange = false;
								}

								if (_bonuses[x + i, y + j]!.Forests[a].forest == f &&
								    _bonuses[x + i, y + j]!.Forests[a].canSee == false)
								{
									_bonuses[x + i, y + j]!.Forests[a] = (f, true);
									if (bonusChange && _fields[x + i, y + j] is Zone)
									{
										(_fields[x + i, y + j] as Zone)!.ForestBonus = f.Age > 10 ? 0.1 : f.Age * 0.01;
									}

									canAddForest = false;
									break;
								}

								if (_bonuses[x + i, y + j]!.Forests[a].forest == f)
								{
									canAddForest = false;
									break;
								}
							}

							if (canAddForest)
							{
								_bonuses[startx0, starty0]!.Forests.Add((f, canApply));
							}
						}
						else
						{
							for (int a = 0; a < _bonuses[x + i, y + j]!.Forests.Count(); a++)
							{
								if (_bonuses[x + i, y + j]!.Forests[a].forest != f &&
								    _bonuses[x + i, y + j]!.Forests[a].canSee == true)
								{
									bonusChange = false;
								}

								if (_bonuses[x + i, y + j]!.Forests[a].forest == f &&
								    _bonuses[x + i, y + j]!.Forests[a].canSee == true)
								{
									_bonuses[x + i, y + j]!.Forests[a] = (f, false);
									canAddForest = false;
									break;
								}

								if (_bonuses[x + i, y + j]!.Forests[a].forest == f)
								{
									canAddForest = false;
									break;
								}
							}

							if (bonusChange)
							{
								if (_fields[x + i, y + j] != null && _fields[x + i, y + j] is Zone)
								{
									(_fields[x + i, y + j] as Zone)!.ForestBonus = 0.0;
								}
							}

							if (canAddForest)
							{
								_bonuses[startx0, starty0]!.Forests.Add((f, canApply));
							}
						}
					}
				}
			}
		}

		public void RemoveForest(int x, int y, Forest f)
		{
			int radius = 3;
			for (Int32 i = -radius; i <= radius; i++)
			{
				for (Int32 j = -radius; j <= radius; j++)
				{
					bool bonusChange = true;
					int age = 0;
					if (!IndexOutOfRange(x + i, y + j))
					{
						for (int a = 0; a < _bonuses[x + i, y + j]!.Forests.Count(); a++)
						{
							if (_bonuses[x + i, y + j]!.Forests[a].forest != f &&
							    _bonuses[x + i, y + j]!.Forests[a].canSee == true && bonusChange)
							{
								bonusChange = false;
								age = _bonuses[x + i, y + j]!.Forests[a].forest.Age;
							}

							if (_bonuses[x + i, y + j]!.Forests[a].forest == f &&
							    _bonuses[x + i, y + j]!.Forests[a].canSee == false)
							{
								_bonuses[x + i, y + j]!.Forests.Remove((f, false));
							}
							else if (_bonuses[x + i, y + j]!.Forests[a].forest == f &&
							         _bonuses[x + i, y + j]!.Forests[a].canSee == true)
							{
								_bonuses[x + i, y + j]!.Forests.Remove((f, true));
							}
						}

						if (bonusChange && _fields[x + i, y + j] is Zone)
						{
							(_fields[x + i, y + j] as Zone)!.ForestBonus = age > 10 ? 0.1 : age * 0.01;
						}
						else if (_fields[x + i, y + j] is Zone)
						{
							(_fields[x + i, y + j] as Zone)!.ForestBonus = 0;
						}
					}
				}
			}
		}

		/// <summary>
		/// Removes bonuses on different fields when you demolish
		/// </summary>
		public void RemoveBonuses(int x, int y, int radius, IField? field)
		{
			if (field == null)
			{
				return;
			}

			x = field.Location.x;
			y = field.Location.y;
			if (field is Police || field is Industrial)
			{
				Int32 _firstBoundary = 4;
				Int32 _secondBoundary = 8;
				Int32 _thirdBoundary = 10;
				Int32 _fourthBoundary = 12;
				Int32 _fifthBoundary = 15;
				for (Int32 i = -radius; i < radius; i++)
				{
					for (Int32 j = -radius; j < radius; j++)
					{
						if (!IndexOutOfRange(x + i, y + j) && _bonuses[x + i, y + j] != null)
						{
							if (i >= -_firstBoundary && i <= _firstBoundary && j >= -_firstBoundary &&
							    j <= _firstBoundary)
							{
								if (field is Police)
								{
									_bonuses[x + i, y + j]!.PoliceBonus.Remove(0.2);
								}
								else
								{
									_bonuses[x + i, y + j]!.IndustrialBonus.Remove(((field as Industrial)!, 0));
								}
							}
							else if (i >= -_secondBoundary && i <= _secondBoundary && j >= -_secondBoundary &&
							         j <= _secondBoundary)
							{
								if (field is Police)
								{
									_bonuses[x + i, y + j]!.PoliceBonus.Remove(0.16);
								}
								else
								{
									_bonuses[x + i, y + j]!.IndustrialBonus.Remove(((field as Industrial)!, 0.04));
								}
							}
							else if (i >= -_thirdBoundary && i <= _thirdBoundary && j >= -_thirdBoundary &&
							         j <= _thirdBoundary)
							{
								if (field is Police)
								{
									_bonuses[x + i, y + j]!.PoliceBonus.Remove(0.12);
								}
								else
								{
									_bonuses[x + i, y + j]!.IndustrialBonus.Remove(((field as Industrial)!, 0.08));
								}
							}
							else if (i >= -_fourthBoundary && i <= _fourthBoundary && j >= -_fourthBoundary &&
							         j <= _fourthBoundary)
							{
								if (field is Police)
								{
									_bonuses[x + i, y + j]!.PoliceBonus.Remove(0.08);
								}
								else
								{
									_bonuses[x + i, y + j]!.IndustrialBonus.Remove(((field as Industrial)!, 0.12));
								}
							}
							else if (i >= -_fifthBoundary && i <= _fifthBoundary && j >= -_fifthBoundary &&
							         j <= _fifthBoundary)
							{
								if (field is Police)
								{
									_bonuses[x + i, y + j]!.PoliceBonus.Remove(0.04);
								}
								else
								{
									_bonuses[x + i, y + j]!.IndustrialBonus.Remove(((field as Industrial)!, 0.16));
								}
							}

							if (field is Police && _fields[x + i, y + j] is Zone)
							{
								(_fields[x + i, y + j] as Zone)!.PoliceBonus = _bonuses[x + i, y + j]!.PoliceBonus[0];
							}
							else if (field is Industrial && _fields[x + i, y + j] is Residential)
							{
								(_fields[x + i, y + j] as Residential)!.IndustrialDistBonus =
									_bonuses[x + i, y + j]!.IndustrialBonus.Count() != 0
										? _bonuses[x + i, y + j]!.IndustrialBonus[0].bonus
										: 0.2;
							}
						}
					}
				}
			}
			else if (field is Stadium)
			{
				Int32 _firstBoundary = 4;
				Int32 _secondBoundary = 8;
				Int32 _thirdBoundary = 10;
				Int32 _fourthBoundary = 12;
				Int32 _fifthBoundary = 15;
				Int32 _plusXBoundary = (((Stadium) field).Size.Item1 - 1);
				Int32 _plusYBoundary = (((Stadium) field).Size.Item2 - 1);
				for (Int32 i = -radius; i < (radius + _plusXBoundary); i++)
				{
					for (Int32 j = -radius; j < (radius + _plusYBoundary); j++)
					{
						if (!IndexOutOfRange(x + i, y + j) && _bonuses[x + i, y + j] != null)
						{
							if (i >= -_firstBoundary && i <= (_firstBoundary + _plusXBoundary) &&
							    j >= -_firstBoundary && j <= (_firstBoundary + _plusYBoundary))
							{
								_bonuses[x + i, y + j]!.StadiumBonus.Remove(0.2);
							}
							else if (i >= -_secondBoundary && i <= (_secondBoundary + _plusXBoundary) &&
							         j >= -_secondBoundary && j <= (_secondBoundary + _plusYBoundary))
							{
								_bonuses[x + i, y + j]!.StadiumBonus.Remove(0.16);
							}
							else if (i >= -_thirdBoundary && i <= (_thirdBoundary + _plusXBoundary) &&
							         j >= -_thirdBoundary && j <= (_thirdBoundary + _plusYBoundary))
							{
								_bonuses[x + i, y + j]!.StadiumBonus.Remove(0.12);
							}
							else if (i >= -_fourthBoundary && i <= (_fourthBoundary + _plusXBoundary) &&
							         j >= -_fourthBoundary && j <= (_fourthBoundary + _plusYBoundary))
							{
								_bonuses[x + i, y + j]!.StadiumBonus.Remove(0.08);
							}
							else if (i >= -_fifthBoundary && i <= (_fifthBoundary + _plusXBoundary) &&
							         j >= -_fifthBoundary && j <= (_fifthBoundary + _plusYBoundary))
							{
								_bonuses[x + i, y + j]!.StadiumBonus.Remove(0.04);
							}

							if (_fields[x + i, y + j] is Zone)
							{
								(_fields[x + i, y + j] as Zone)!.StadiumBonus = _bonuses[x + i, y + j]!.StadiumBonus[0];
							}
						}
					}
				}
			}
		}

		public void UtilizationChanged(Zone zone)
		{
			UtilizationChangedEvent?.Invoke(this, (zone.Location.x, zone.Location.y));
		}

		public Boolean IndexOutOfRange(Int32 x, Int32 y)
		{
			return !(x >= 0 && y >= 0 && x < _fields.GetLength(0) && y < _fields.GetLength(1));
		}

		#endregion

		#region Private methods

		private Int32 PopulationCount()
		{
			Int32 count = 0;
			foreach (IField? field in _fields)
			{
				if (field is Residential)
				{
					count += ((Residential) field).PopulationCount();
				}
			}

			return count;
		}

		private void GenerateForests()
		{
			Random rnd = new Random();
			int numberOfForests = rnd.Next(5, 16);

			for (int i = 0; i < numberOfForests; i++)
			{
				int forestY = 0;
				int forestX = 0;
				do
				{
					forestX = rnd.Next(0, MapSize);
					forestY = rnd.Next(0, MapSize);
				} while (_fields[forestX, forestY] != null || (forestX == _highway.x && forestY == _highway.y));

				_fields[forestX, forestY] = new Forest(10)
				{
					Connected = true,
					Location = (forestX, forestY)
				};

				ApplyForestBonus(forestX, forestY, (_fields[forestX, forestY] as Forest)!);
			}
		}

		private bool CanBuildingBePlaced(Int32 x, Int32 y, IField building)
		{
			for (Int32 i = 0; i < building.Size.x; i++)
			{
				for (Int32 j = 0; j < building.Size.y; j++)
				{
					// fields must be in range and fields must be unbuilt
					if (IndexOutOfRange(x + i, y + j) || _fields[x + i, y + j] != null)
					{
						return false;
					}
				}
			}

			return true;
		}

		private void PlaceBuilding(Int32 x, Int32 y, IField building)
		{
			for (Int32 i = 0; i < building.Size.x; i++)
			{
				for (Int32 j = 0; j < building.Size.y; j++)
				{
					_fields[x + i, y + j] = building;
					_fields[x + i, y + j]!.Connected = true;
					_fields[x + i, y + j]!.Location = (x, y);
				}
			}
		}

		private void DemolishRoad(int x, int y)
		{
			_fields[x, y] = null;
			DemolishEvent?.Invoke(this, (x, y));
			UpdateConnected();

			for (int i = 0; i < _residentialFields.Count; i++)
			{
				Residential residential = _residentialFields[i];
				if (residential.Connected == false)
				{
					if (_fields[residential.Location.x, residential.Location.y] != null)
					{
						_funds -= (int) Math.Round(_fields[residential.Location.x, residential.Location.y]!.Price * 0.3,
							0);
					}

					ResidentialRelocate(residential);
				}
			}

			for (int i = 0; i < _workplaceFields.Count; i++)
			{
				Workplace workplace = _workplaceFields[i];

				if (workplace.Connected == false)
				{
					if (_fields[workplace.Location.x, workplace.Location.y] != null)
					{
						_funds -= (int) Math.Round(_fields[workplace.Location.x, workplace.Location.y]!.Price * 0.3, 0);
						CurrentYearExpense +=
							(int) Math.Round(_fields[workplace.Location.x, workplace.Location.y]!.Price * 0.3, 0);
					}

					WorkplaceRelocate(workplace);
				}
			}
		}

		private void WorkplaceRelocate(Workplace workplace)
		{
			int x = workplace.Location.x;
			int y = workplace.Location.y;

			List<Person> population = workplace!.Population;
			int populationCount = population.Count;
			if (workplace is Industrial)
			{
				_workerCount.industrial -= population.Count;
			}
			else
			{
				_workerCount.commercial -= population.Count;
			}

			for (int i = 0; i < populationCount; i++)
			{
				Person person = population[0];
				if (person.Home != null)
				{
					workplace.Fire(person, this);
					if (!SearchWorkplace(person.Home.Location.x, person.Home.Location.y, person))
					{
						person.Home.Population.Remove(person);
					}
					else
					{
						person.RelocationHappiness = -0.24;
					}
				}
			}
		}

		private void ResidentialRelocate(Residential? residentialBuilding)
		{
			if (residentialBuilding == null)
			{
				return;
			}

			List<Person> population = residentialBuilding!.Population;
			int populationSize = population.Count;
			bool homeFound = false;
			for (int i = 0; i < populationSize; i++)
			{
				Person person = residentialBuilding.Population[0];
				for (int j = 0; j < _residentialFields.Count; j++)
				{
					Residential residential = _residentialFields[j];
					if (!residential.IsFull() && residential.Connected)
					{
						residentialBuilding.Population.Remove(person);
						if (person.Workplace != null)
						{
							person.Workplace.Fire(person, this);
						}

						if (SearchWorkplace(residential.Location.x, residential.Location.y, person))
						{
							residential.Population.Add(person);
							person.Home = residential;
							homeFound = true;
							person.RelocationHappiness = -0.24;
						}

						break;
					}
				}

				if (!homeFound)
				{
					person.Home = null;
					residentialBuilding.Population.Remove(person);
					person.Workplace?.Fire(person, this);
				}

				homeFound = false;
			}
		}

		private void EmployByRatio(WorkplaceWithDist closestWorkplaces, Person person)
		{
			double ratio;
			if (_workerCount.industrial < _workerCount.commercial)
				ratio = (double) _workerCount.industrial /
				        _workerCount.commercial;
			else
				ratio = (double) _workerCount.commercial /
				        _workerCount.industrial;

			Workplace? closestWorkplace =
				closestWorkplaces.Workplace.commercial.dist < closestWorkplaces.Workplace.industrial.dist
					? closestWorkplaces.Workplace.commercial.comm
					: closestWorkplaces.Workplace.industrial.ind;

			if (closestWorkplace != null && closestWorkplaces.Workplace.industrial.ind != null &&
			    closestWorkplaces.Workplace.commercial.comm != null)
			{
				if ((_workerCount.industrial == 0 && _workerCount.commercial == 0) ||
				    (ratio > 0.8 && (_workerCount.industrial != 0 || _workerCount.commercial != 0)))
				{
					if (closestWorkplace is Industrial)
					{
						_workerCount.industrial++;
						person.DistanceToWorkplace = closestWorkplaces.Workplace.industrial.dist;
					}
					else
					{
						_workerCount.commercial++;
						person.DistanceToWorkplace = closestWorkplaces.Workplace.commercial.dist;
					}

					closestWorkplace.Employ(person, this);
				}
				else
				{
					if (_workerCount.industrial < _workerCount.commercial)
					{
						_workerCount.industrial++;
						closestWorkplaces.Workplace.industrial.ind.Employ(person, this);
						person.DistanceToWorkplace = closestWorkplaces.Workplace.industrial.dist;
					}
					else
					{
						_workerCount.commercial++;
						closestWorkplaces.Workplace.commercial.comm.Employ(person, this);
						person.DistanceToWorkplace = closestWorkplaces.Workplace.commercial.dist;
					}
				}
			}
		}

		#endregion

		#region Private game methods

		/// <summary>
		/// Applies the appropriate side effects
		/// </summary>
		/// <param name="x">The demolished building's x coordinate</param>
		/// <param name="y">The demolished building's y coordinate</param>
		private void DemolishSideEffects(int x, int y)
		{
			RemoveBonuses(x, y, Radius, _fields[x, y]!);
			var field = _fields[x, y];
			if (field is null)
			{
				return;
			}

			if (field is not Forest)
			{
				for (int i = 0; i < _bonuses[field.Location.x, field.Location.y]!.Forests.Count(); i++)
				{
					ApplyForestBonus(x, y, _bonuses[field.Location.x, field.Location.y]!.Forests[i].forest);
				}
			}
			else if (field is Forest)
			{
				RemoveForest(x, y, (field as Forest)!);
			}
		}

		/// <summary>
		/// Demolishes every side of the desired building
		/// </summary>
		/// <param name="x">X coordinate of the building</param>
		/// <param name="y">Y coordinate of the building</param>
		/// <param name="building">The building that needs to be demolished</param>
		private void DemolishSameBuildingRecursive(int x, int y, IField? building)
		{
			if (building == null) return;
			int sizeX = building.Size.x;
			int sizeY = building.Size.y;
			_fields[x, y] = null;
			DemolishEvent?.Invoke(this, (x, y));
			if (!IndexOutOfRange(x + 1, y) && _fields[x + 1, y] == building)
			{
				DemolishSameBuildingRecursive(x + 1, y, building);
			}

			if (!IndexOutOfRange(x - 1, y) && _fields[x - 1, y] == building)
			{
				DemolishSameBuildingRecursive(x - 1, y, building);
			}

			if (!IndexOutOfRange(x, y + 1) && _fields[x, y + 1] == building)
			{
				DemolishSameBuildingRecursive(x, y + 1, building);
			}

			if (!IndexOutOfRange(x, y - 1) && _fields[x, y - 1] == building)
			{
				DemolishSameBuildingRecursive(x, y - 1, building);
			}
		}

		/// <summary>
		/// Calls the ConnectedChangedEvent for every field of the building
		/// </summary>
		/// <param name="x">X coordinate of the building</param>
		/// <param name="y">Y coordinate of the building</param>
		/// <param name="building">The building which needs to be checked</param>
		private void CheckConnectedSameBuilding(int x, int y, IField? building)
		{
			List<(int x, int y)> connected = new List<(int x, int y)>();
			CheckConnectedSameBuildingRecursive(x, y, connected, building);
		}

		private void CheckConnectedSameBuildingRecursive(int x, int y, List<(int x, int y)> isChecked, IField? building)
		{
			if (building == null) return;
			int sizeX = building.Size.x;
			int sizeY = building.Size.y;
			ConnectedChangedEvent?.Invoke(this, (x, y));
			isChecked.Add((x, y));
			if (!IndexOutOfRange(x + 1, y) && _fields[x + 1, y] == building && !isChecked.Contains((x + 1, y)))
			{
				CheckConnectedSameBuildingRecursive(x + 1, y, isChecked, building);
			}

			if (!IndexOutOfRange(x - 1, y) && _fields[x - 1, y] == building && !isChecked.Contains((x - 1, y)))
			{
				CheckConnectedSameBuildingRecursive(x - 1, y, isChecked, building);
			}

			if (!IndexOutOfRange(x, y + 1) && _fields[x, y + 1] == building && !isChecked.Contains((x, y + 1)))
			{
				CheckConnectedSameBuildingRecursive(x, y + 1, isChecked, building);
			}

			if (!IndexOutOfRange(x, y - 1) && _fields[x, y - 1] == building && !isChecked.Contains((x, y - 1)))
			{
				CheckConnectedSameBuildingRecursive(x, y - 1, isChecked, building);
			}
		}

		/// <summary>
		/// Sets the Connected property to true for every adjacent field and calls itself on every adjacent road
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		private void UpdateConnectedRecursive(Int32 x, Int32 y)
		{
			// if field is not out of index
			if (!IndexOutOfRange(x, y))
			{
				// checks the upper field first
				if (!IndexOutOfRange(x - 1, y) && x - 1 >= 0 && _fields[x - 1, y] != null &&
				    !_fields[x - 1, y]!.Connected)
				{
					_fields[x - 1, y]!.Connected = true;
					CheckConnectedSameBuilding(x - 1, y, _fields[x - 1, y]);
					ApplyBonuses(x - 1, y, Radius, _fields[x - 1, y]!);
					if (_fields[x - 1, y] is Road)
					{
						UpdateConnectedRecursive(x - 1, y);
					}
				}

				// checks the lower field second
				if (!IndexOutOfRange(x + 1, y) && x + 1 <= _fields.GetLength(0) && _fields[x + 1, y] != null &&
				    !_fields[x + 1, y]!.Connected)
				{
					_fields[x + 1, y]!.Connected = true;
					CheckConnectedSameBuilding(x + 1, y, _fields[x + 1, y]);
					ApplyBonuses(x + 1, y, Radius, _fields[x + 1, y]!);
					if (_fields[x + 1, y] is Road)
					{
						UpdateConnectedRecursive(x + 1, y);
					}
				}

				// checks the left field third
				if (!IndexOutOfRange(x, y - 1) && y - 1 >= 0 && _fields[x, y - 1] != null &&
				    !_fields[x, y - 1]!.Connected)
				{
					_fields[x, y - 1]!.Connected = true;
					CheckConnectedSameBuilding(x, y - 1, _fields[x, y - 1]);
					ApplyBonuses(x, y - 1, Radius, _fields[x, y - 1]!);
					if (_fields[x, y - 1] is Road)
					{
						UpdateConnectedRecursive(x, y - 1);
					}
				}

				// checks the right field last
				if (!IndexOutOfRange(x, y + 1) && y + 1 <= _fields.GetLength(1) && _fields[x, y + 1] != null &&
				    !_fields[x, y + 1]!.Connected)
				{
					_fields[x, y + 1]!.Connected = true;
					CheckConnectedSameBuilding(x, y + 1, _fields[x, y + 1]);
					ApplyBonuses(x, y + 1, Radius, _fields[x, y + 1]!);
					if (_fields[x, y + 1] is Road)
					{
						UpdateConnectedRecursive(x, y + 1);
					}
				}
			}
		}

		/// <summary>
		/// Updates what needs to be every day
		/// and also calls UpdateWeekly() and/or UpdateMonthly() when needed
		/// </summary>
		private void UpdateDaily()
		{
			// if first day of week
			if (_dateTime.DayOfWeek == DayOfWeek.Monday)
			{
				UpdateWeekly();
			}

			// if first day of the month
			if (_dateTime == new DateTime(_dateTime.Year, _dateTime.Month, 1))
			{
				UpdateMonthly();
			}

			foreach (IField? field in _fields)
			{
				_funds += field?.UpdateDaily(this) ?? 0;
			}

			// unchecks every UpdatedAlready property before returning
			foreach (IField? field in _fields)
			{
				if (field != null)
					field.UpdatedDailyAlready = false;
			}
		}

		/// <summary>
		/// Updates what needs to be every week
		/// </summary>
		private void UpdateWeekly()
		{
			foreach (IField? field in _fields)
			{
				_funds += field?.UpdateWeekly(this) ?? 0;
			}

			// unchecks every UpdatedAlready property before returning
			foreach (IField? field in _fields)
			{
				if (field != null)
					field.UpdatedWeeklyAlready = false;
			}
		}

		/// <summary>
		/// Updates what needs to be every month and also calls UpdateYearly() when needed
		/// </summary>
		private void UpdateMonthly()
		{
			int zoneprofit = 0;
			int sumofzoneprofit = 0;
			if (Funds < 0)
			{
				if ((_fundsBonus - 0.02) < 0)
				{
					_fundsBonus = 0;
				}
				else
				{
					_fundsBonus -= 0.02;
				}
			}
			else if (Funds > 0)
			{
				if ((_fundsBonus + 0.02) > 0.2)
				{
					_fundsBonus = 0.2;
				}
				else
				{
					_fundsBonus += 0.02;
				}
			}

			if (TaxPercentage <= 0.15)
			{
				_taxBonus = 0.2;
			}
			else if (TaxPercentage <= 0.25)
			{
				_taxBonus = 0.15;
			}
			else if (TaxPercentage <= 0.35)
			{
				_taxBonus = 0.10;
			}
			else if (TaxPercentage <= 0.45)
			{
				_taxBonus = 0.05;
			}
			else if (TaxPercentage <= 100)
			{
				_taxBonus = 0;
			}

			// if first day of the year
			if (_dateTime.DayOfYear == 1)
			{
				UpdateYearly();
			}

			foreach (IField? field in _fields)
			{
				zoneprofit = field?.UpdateMonthly(this) ?? 0;
				_funds += zoneprofit;
				if (field is Zone)
				{
					sumofzoneprofit += zoneprofit;
				}
			}

			if (sumofzoneprofit != 0)
			{
				AddExpenseIncome("Income of zones: ", sumofzoneprofit);
			}

			if (UpkeepCost != 0)
			{
				AddExpenseIncome("Upkeep costs: ", -UpkeepCost);
				UpkeepCost = 0;
			}

			if (Pensions != 0)
			{
				AddExpenseIncome("Pensions: ", Pensions);
				Pensions = 0;
			}

			ExpenseIncomeChangedEvent?.Invoke(this, EventArgs.Empty);

			// unchecks every UpdatedAlready property before returning
			foreach (IField? field in _fields)
			{
				if (field != null)
					field.UpdatedMonthlyAlready = false;
			}

			if (_funds < DebtForGameOver)
			{
				_gameOverTimer++;
			}
			else
			{
				_gameOverTimer = 0;
			}

			if (_gameOverTimer == 12)
			{
				GameOverEvent?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Updates what needs to be every year
		/// </summary>
		private void UpdateYearly()
		{
			HighDegreeNumber = 0;
			MiddleDegreeNumber = 0;
			LowDegreeNumber = 0;
			NumberOfRetired = 0;
			// education institutions and workplaces get priority
			// to fire retired/graduate people first,
			foreach (IField? field in _fields)
			{
				if (field is Education || field is Workplace || field is Forest)
				{
					_funds += field?.UpdateYearly(this) ?? 0;
				}

				if (field != null && (field is Forest))
				{
					ApplyForestBonus(field.Location.x, field.Location.y, (field as Forest)!);
				}
			}

			// then enroll/employ people from Residential zones
			// and update other fields
			foreach (IField? field in _fields)
			{
				if (!(field is Education) && !(field is Workplace) && !(field is Forest))
				{
					_funds += field?.UpdateYearly(this) ?? 0;
				}

				if (field is Zone zone && field != null)
				{
					GetForestBonus(zone);
				}
			}

			// unchecks every UpdatedAlready property before returning
			foreach (IField? field in _fields)
			{
				if (field != null)
					field.UpdatedYearlyAlready = false;
			}

			LastYearExpense = CurrentYearExpense;
			CurrentYearExpense = 0;
			UpkeepCost = 0;
			Pensions = 0;
			LastYearIncome = CurrentYearIncome;
			CurrentYearIncome = 0;
			LastYearProfit = LastYearIncome - LastYearExpense;
		}


		/// <summary>
		/// Helper function for 'SearchWorkplaceDijkstra'
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="closestWorkplaces"></param>
		/// <param name="queue"></param>
		/// <param name="roads"></param>
		private void CheckAdjacentFieldsDijkstra(List<Road?> routes, WorkplaceWithDist? closestWorkplaces,
			PriorityQueue<Road?, int> queue, Dictionary<Road, (int distance, Road? prevRoad)> roads)
		{
			Int32 x = queue.Peek()!.Location.x + 1;
			Int32 y = queue.Peek()!.Location.y;
			if (!IndexOutOfRange(x, y) && _fields[x, y] is Road)
			{
				routes.Add(_fields[x, y] as Road);
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Commercial)
			{
				Commercial? commercial = _fields[x, y] as Commercial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces?.Workplace.commercial.comm == null ||
				     closestWorkplaces.Workplace.commercial.dist > roads[queue.Peek()!].distance + 1) &&
				    !commercial!.IsFull() && commercial.Connected)
				{
					(Commercial comm, int dist) newComm = (commercial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces!.Workplace = (newComm, closestWorkplaces.Workplace.industrial);
				}
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Industrial)
			{
				Industrial? industrial = _fields[x, y] as Industrial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces?.Workplace.industrial.ind == null ||
				     closestWorkplaces.Workplace.industrial.dist > roads[queue.Peek()!].distance + 1) &&
				    !industrial!.IsFull() && industrial.Connected)
				{
					(Industrial ind, int dist) newInd = (industrial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces!.Workplace = (closestWorkplaces.Workplace.commercial, newInd);
				}
			}

			x = queue.Peek()!.Location.x - 1;
			y = queue.Peek()!.Location.y;
			if (!IndexOutOfRange(x, y) && _fields[x, y] is Road)
			{
				routes.Add(_fields[x, y] as Road);
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Commercial)
			{
				Commercial? commercial = _fields[x, y] as Commercial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.commercial.comm == null ||
				     closestWorkplaces.Workplace.commercial.dist > roads[queue.Peek()!].distance + 1) &&
				    !commercial!.IsFull() && commercial.Connected)
				{
					(Commercial comm, int dist) newComm = (commercial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (newComm, closestWorkplaces.Workplace.industrial);
				}
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Industrial)
			{
				Industrial? industrial = _fields[x, y] as Industrial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.industrial.ind == null || closestWorkplaces.Workplace.industrial.dist >
					    roads[queue.Peek()!].distance + 1) && !industrial!.IsFull() && industrial.Connected)
				{
					(Industrial ind, int dist) newInd = (industrial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (closestWorkplaces.Workplace.commercial, newInd);
				}
			}

			x = queue.Peek()!.Location.x;
			y = queue.Peek()!.Location.y + 1;
			if (!IndexOutOfRange(x, y) && _fields[x, y] is Road)
			{
				routes.Add(_fields[x, y] as Road);
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Commercial)
			{
				Commercial? commercial = _fields[x, y] as Commercial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.commercial.comm == null ||
				     closestWorkplaces.Workplace.commercial.dist > roads[queue.Peek()!].distance + 1) &&
				    !commercial!.IsFull() && commercial.Connected)
				{
					(Commercial comm, int dist) newComm = (commercial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (newComm, closestWorkplaces.Workplace.industrial);
				}
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Industrial)
			{
				Industrial? industrial = _fields[x, y] as Industrial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.industrial.ind == null || closestWorkplaces.Workplace.industrial.dist >
					    roads[queue.Peek()!].distance + 1) && !industrial!.IsFull() && industrial.Connected)
				{
					(Industrial ind, int dist) newInd = (industrial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (closestWorkplaces.Workplace.commercial, newInd);
				}
			}

			x = queue.Peek()!.Location.x;
			y = queue.Peek()!.Location.y - 1;
			if (!IndexOutOfRange(x, y) && _fields[x, y] is Road)
			{
				routes.Add(_fields[x, y] as Road);
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Commercial)
			{
				Commercial? commercial = _fields[x, y] as Commercial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.commercial.comm == null ||
				     closestWorkplaces.Workplace.commercial.dist > roads[queue.Peek()!].distance + 1) &&
				    !commercial!.IsFull() && commercial.Connected)
				{
					(Commercial comm, int dist) newComm = (commercial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (newComm, closestWorkplaces.Workplace.industrial);
				}
			}
			else if (!IndexOutOfRange(x, y) && _fields[x, y] is Industrial)
			{
				Industrial? industrial = _fields[x, y] as Industrial;
				if (queue.Peek() != null && closestWorkplaces != null &&
				    (closestWorkplaces.Workplace.industrial.ind == null || closestWorkplaces.Workplace.industrial.dist >
					    roads[queue.Peek()!].distance + 1) && !industrial!.IsFull() && industrial.Connected)
				{
					(Industrial ind, int dist) newInd = (industrial, roads[queue.Peek()!].distance + 1);
					closestWorkplaces.Workplace = (closestWorkplaces.Workplace.commercial, newInd);
				}
			}
		}


		/// <summary>
		/// Gets the closest industrial and closest commercial field relative to a specified residential zone.
		/// </summary>
		/// <param name="queue"></param>
		/// <param name="roads"></param>
		/// <param name="visited"></param>
		/// <param name="closestWorkplaces"></param>
		private void SearchWorkplaceDijkstra(PriorityQueue<Road?, int> queue,
			Dictionary<Road, (int distance, Road? prevRoad)> roads, HashSet<Road> visited,
			ref WorkplaceWithDist? closestWorkplaces)
		{
			if (queue.Count == 0)
			{
				return;
			}

			var routes = new List<Road?>();

			CheckAdjacentFieldsDijkstra(routes, closestWorkplaces, queue, roads);

			foreach (var route in routes)
			{
				if (route != null && visited.Contains(route))
				{
					continue;
				}

				if (queue.Peek() != null && route != null && !roads.ContainsKey(route))
				{
					roads.Add(route, (roads[queue.Peek()!].distance + 1, queue.Peek()));
				}

				int travelDistance = roads[queue.Peek()!].distance + 1;

				if (route != null && travelDistance < roads[route].distance)
				{
					(int distance, Road prevRoad) newRoad = (travelDistance, queue.Peek()!);
					roads[route] = newRoad;
				}

				queue.Enqueue(route, travelDistance);
			}

			visited.Add(queue.Peek()!);
			queue.Dequeue();

			SearchWorkplaceDijkstra(queue, roads, visited, ref closestWorkplaces);
		}

		private void GetForestBonus(IField field)
		{
			int x = field.Location.x;
			int y = field.Location.y;
			for (int z = 0; z < _bonuses[x, y]!.Forests.Count(); z++)
			{
				if (_bonuses[x, y]!.Forests[z].canSee == true)
				{
					(_fields[x, y] as Zone)!.ForestBonus = _bonuses[x, y]!.Forests[z].forest.Age > 10
						? 0.1
						: _bonuses[x, y]!.Forests[z].forest.Age * 0.01;
					break;
				}
			}
		}

		private void ApplyPoliceAndIndustrialBonus(int radius, IField field)
		{
			int x = field.Location.x;
			int y = field.Location.y;
			Int32 _firstBoundary = 4;
			Int32 _secondBoundary = 8;
			Int32 _thirdBoundary = 10;
			Int32 _fourthBoundary = 12;
			Int32 _fifthBoundary = 15;
			for (Int32 i = -radius; i < radius; i++)
			{
				for (Int32 j = -radius; j < radius; j++)
				{
					if (!IndexOutOfRange(x + i, y + j))
					{
						if (i >= -_firstBoundary && i <= _firstBoundary && j >= -_firstBoundary && j <= _firstBoundary)
						{
							AddPoliceAndIndustrialBonus(field, x + i, y + j, 0.2, 0);
						}
						else if (i >= -_secondBoundary && i <= _secondBoundary && j >= -_secondBoundary &&
						         j <= _secondBoundary)
						{
							AddPoliceAndIndustrialBonus(field, x + i, y + j, 0.16, 0.04);
						}
						else if (i >= -_thirdBoundary && i <= _thirdBoundary && j >= -_thirdBoundary &&
						         j <= _thirdBoundary)
						{
							AddPoliceAndIndustrialBonus(field, x + i, y + j, 0.12, 0.08);
						}
						else if (i >= -_fourthBoundary && i <= _fourthBoundary && j >= -_fourthBoundary &&
						         j <= _fourthBoundary)
						{
							AddPoliceAndIndustrialBonus(field, x + i, y + j, 0.08, 0.12);
						}
						else if (i >= -_fifthBoundary && i <= _fifthBoundary && j >= -_fifthBoundary &&
						         j <= _fifthBoundary)
						{
							AddPoliceAndIndustrialBonus(field, x + i, y + j, 0.04, 0.16);
						}

						if (field is Police && _fields[x + i, y + j] is Zone)
						{
							(_fields[x + i, y + j] as Zone)!.PoliceBonus = _bonuses[x + i, y + j]!.PoliceBonus[0];
						}
						else if (field is Industrial)
						{
							(field as Zone)!.PoliceBonus = _bonuses[x + i, y + j]!.PoliceBonus[0];
							(field as Zone)!.StadiumBonus = _bonuses[x + i, y + j]!.StadiumBonus[0];

							for (int z = 0; z < _bonuses[field.Location.x, field.Location.y]!.Forests.Count(); z++)
							{
								if (_bonuses[field.Location.x, field.Location.y]!.Forests[z].canSee == true)
								{
									(_fields[x, y] as Zone)!.ForestBonus = _bonuses[x, y]!.Forests[z].forest.Age > 10
										? 0.1
										: _bonuses[x, y]!.Forests[z].forest.Age * 0.01;
									break;
								}
							}

							if (_fields[x + i, y + j] is Residential)
							{
								(_fields[x + i, y + j] as Residential)!.IndustrialDistBonus =
									_bonuses[x + i, y + j]!.IndustrialBonus.Count() != 0
										? _bonuses[x + i, y + j]!.IndustrialBonus[0].bonus
										: 0.2;
							}
						}
					}
				}
			}
		}

		private void AddPoliceAndIndustrialBonus(IField field, int indexX, int indexY, double policeBonus,
			double industrialBonus)
		{
			if (field == null || _bonuses[indexX, indexY] == null)
			{
				return;
			}

			if (field is Police)
			{
				_bonuses[indexX, indexY]!.PoliceBonus.Add(policeBonus);
				_bonuses[indexX, indexY]!.PoliceBonus.Sort((x, y) => y.CompareTo(x));
			}
			else if (field is Industrial)
			{
				_bonuses[indexX, indexY]!.IndustrialBonus.Add((field as Industrial, industrialBonus)!);
				_bonuses[indexX, indexY]!.IndustrialBonus.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));
			}
		}

		private void ApplyStadiumBonus(int radius, IField field)
		{
			if (!(field is Stadium))
			{
				return;
			}

			int x = field.Location.x;
			int y = field.Location.y;
			Int32 _firstBoundary = 4;
			Int32 _secondBoundary = 8;
			Int32 _thirdBoundary = 10;
			Int32 _fourthBoundary = 12;
			Int32 _fifthBoundary = 15;
			Int32 _plusXBoundary = (field as Stadium)!.Size.Item1 - 1;
			Int32 _plusYBoundary = (field as Stadium)!.Size.Item2 - 1;
			for (Int32 i = -radius; i < (radius + _plusXBoundary); i++)
			{
				for (Int32 j = -radius; j < (radius + _plusYBoundary); j++)
				{
					if (!IndexOutOfRange(x + i, y + j))
					{
						if (i >= -_firstBoundary && i <= (_firstBoundary + _plusXBoundary) && j >= -_firstBoundary &&
						    j <= (_firstBoundary + _plusYBoundary))
						{
							AddStadiumBonus(field, x + i, y + j, 0.2);
						}
						else if (i >= -_secondBoundary && i <= (_secondBoundary + _plusXBoundary) &&
						         j >= -_secondBoundary && j <= (_secondBoundary + _plusYBoundary))
						{
							AddStadiumBonus(field, x + i, y + j, 0.16);
						}
						else if (i >= -_thirdBoundary && i <= (_thirdBoundary + _plusXBoundary) &&
						         j >= -_thirdBoundary && j <= (_thirdBoundary + _plusYBoundary))
						{
							AddStadiumBonus(field, x + i, y + j, 0.12);
						}
						else if (i >= -_fourthBoundary && i <= (_fourthBoundary + _plusXBoundary) &&
						         j >= -_fourthBoundary && j <= (_fourthBoundary + _plusYBoundary))
						{
							AddStadiumBonus(field, x + i, y + j, 0.08);
						}
						else if (i >= -_fifthBoundary && i <= (_fifthBoundary + _plusXBoundary) &&
						         j >= -_fifthBoundary && j <= (_fifthBoundary + _plusYBoundary))
						{
							AddStadiumBonus(field, x + i, y + j, 0.04d);
						}

						if (_fields[x + i, y + j] is Zone)
						{
							(_fields[x + i, y + j] as Zone)!.StadiumBonus = _bonuses[x + i, y + j]!.StadiumBonus[0];
						}
					}
				}
			}
		}

		private void AddStadiumBonus(IField field, int indexX, int indexY, double stadiumBonus)
		{
			if (field == null || _bonuses[indexX, indexY] == null)
			{
				return;
			}

			_bonuses[indexX, indexY]!.StadiumBonus.Add(stadiumBonus);
			_bonuses[indexX, indexY]!.StadiumBonus.Sort((x, y) => y.CompareTo(x));
		}

		#endregion
	}
}