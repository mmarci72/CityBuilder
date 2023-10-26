using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using ELTECity.Model;

namespace ELTECity.WPF.ViewModel;

public enum Speed
{
	Slow = 1,
	Medium = 2,
	Fast = 3
}

public class MainViewModel : ViewModelBase
{
	#region Fields

	private City? _model;
	private Visibility? _buildWindowVisibility = Visibility.Hidden;
	private Visibility? _financialAndPopulation = Visibility.Visible;
	private Visibility? _buildingStatistics = Visibility.Hidden;
	private IField? _selectedBuilding;
	private int _selectedBuildingIndex;
	private bool _buildMode;
	private bool _paused = true;
	private Speed _timeSpeed = Speed.Slow;
	private bool _demolish;
	private IField? _lastSelectedField;

	#endregion

	#region Properties

	public ObservableCollection<ELTECityField>? Fields { get; set; }
	public ObservableCollection<Building>? Buildings { get; set; }

	public int Size
	{
		get
		{
			if (_model != null) return _model.MapSize;
			return 0;
		}
	}

	public ObservableCollection<ExpenseIncome>? ExpenseIncomeList { get; set; }
	public City? Model => _model;

	public string CityName
	{
		get
		{
			if (_model != null) return _model.Name;
			return string.Empty;
		}
	}

	public String Happiness
	{
		get
		{
			if (_model != null)
			{
				return (Math.Round(_model.HappinessRate() * 100, 2)).ToString() + "%";
			}

			return "0$";
		}
	}

	public string Date
	{
		get
		{
			if (_model != null) return _model.DateTime.ToShortDateString();
			return string.Empty;
		}
	}

	public string Funds
	{
		get
		{
			if (_model != null) return "$" + _model.Funds;
			return "$0";
		}
	}

	public double TaxPercentage
	{
		get
		{
			if (_model != null) return _model.TaxPercentage;
			return 0;
		}
		set
		{
			if (_model != null) _model.TaxPercentage = value;
			OnPropertyChanged();
			OnPropertyChanged("Happiness");
		}
	}

	public int PopulationSize
	{
		get
		{
			if (_model != null) return _model.PopulationSize;
			return 0;
		}
	}

	public bool IsPaused

	{
		get => !_paused;
		set
		{
			_paused = value;
			OnPropertyChanged();
		}
	}

	public string TimeType
	{
		get
		{
			switch (_timeSpeed)
			{
				case Speed.Slow:
					return "►";
				case Speed.Medium:
					return "►►";
				case Speed.Fast:
					return "►►►";
				default:
					return "";
			}
		}
		set
		{
			switch (value)
			{
				case "►":
					_timeSpeed = Speed.Slow;
					break;
				case "►►":
					_timeSpeed = Speed.Medium;
					break;
				case "►►►":
					_timeSpeed = Speed.Fast;
					break;
				default:
					break;
			}

			OnPropertyChanged();
		}
	}

	public Visibility? BuildMode_WD
	{
		get => _buildWindowVisibility;
		set
		{
			_buildWindowVisibility = value;
			OnPropertyChanged();
		}
	}

	public Visibility? FinancialAndPopulation_WD
	{
		get => _financialAndPopulation;
		set
		{
			_financialAndPopulation = value;
			OnPropertyChanged();
		}
	}

	public Visibility? BuildingStatistics_WD
	{
		get => _buildingStatistics;
		set
		{
			_buildingStatistics = value;
			OnPropertyChanged();
		}
	}

	public int BuildingLevel
	{
		get
		{
			if (_lastSelectedField is Zone zone)
			{
				var density = zone.Density;
				if (density == Density.Low)
					return 1;
				else if (density == Density.Medium)
					return 2;
				else if (density == Density.High) return 3;
			}

			return 0;
		}
	}

	public int BuildingNumberOfPeople
	{
		get
		{
			if (_lastSelectedField is Zone zone) return zone.PopulationCount();
			return 0;
		}
	}


	public string CurrentIncome
	{
		get
		{
			if (_model != null) return _model.CurrentYearIncome.ToString() + "$";
			return "0$";
		}
	}

	public string CurrentExpense
	{
		get
		{
			if (_model != null) return _model.CurrentYearExpense.ToString() + "$";
			;
			return "0$";
		}
	}

	public String BuildingHappiness
	{
		get
		{
			if (_lastSelectedField is Zone && _model != null)
			{
				return (Math.Round((_lastSelectedField as Zone)!.HappinessRate(_model) * 100, 2)).ToString() + "%";
			}

			return "0";
		}
		set
		{
			BuildingHappiness = value;
			OnPropertyChanged();
		}
	}

	public String CurrentProfit
	{
		get
		{
			if (_model != null) return ((_model.CurrentYearIncome - _model.CurrentYearExpense).ToString() + "$");
			return "0$";
		}
		set { }
	}

	public string LastIncome
	{
		get
		{
			if (_model != null) return _model.LastYearIncome.ToString() + "$";
			return "0$";
		}
	}

	public String LastExpense
	{
		get
		{
			if (_model != null)
			{
				return _model.LastYearExpense.ToString() + "$";
			}

			return "0$";
		}
	}

	public String LastProfit
	{
		get
		{
			if (_model != null) return _model.LastYearProfit.ToString() + "$";
			return "0$";
		}
	}

	public String BuildingLevelUpCost
	{
		get
		{
			if (_lastSelectedField is Zone)
			{
				switch (((Zone)_lastSelectedField).Density)
				{
					case Density.Low:
						return ((Int32) Density.Medium * _lastSelectedField.Price).ToString() + "$";
					case Density.Medium:
						return ((Int32) Density.High * _lastSelectedField.Price).ToString() + "$";
					case Density.High:
						return "0";
					default:
						break;
				}
			}

			return "0";
		}

		set
		{
			BuildingLevelUpCost = value;
			OnPropertyChanged();
		}
	}

	public Int32? LowDegreeNumber => _model?.LowDegreeNumber;
	public Int32? MidDegreeNumber => _model?.MiddleDegreeNumber;
	public Int32? HighDegreeNumber => _model?.HighDegreeNumber;
	public Int32? NumberOfRetired => _model?.NumberOfRetired;

	#endregion

	#region Commands

	public DelegateCommand NewGameCommand { get; set; }
	public DelegateCommand ExitCommand { get; set; }
	public DelegateCommand BuildModeCommand { get; set; }
	public DelegateCommand FinancialAndPopulationCommand { get; set; }
	public DelegateCommand BackToMainMenuCommand { get; set; }
	public DelegateCommand PauseGameCommand { get; set; }
	public DelegateCommand TimeSpeedCommand { get; set; }
	public DelegateCommand LevelUpCommand { get; set; }

	#endregion

	#region Events

	public event EventHandler? NewGameEvent;
	public event EventHandler? BackToMainMenuEvent;
	public event EventHandler? ExitGameCommand;
	public event EventHandler? PauseGame;
	public event EventHandler? TimeSpeedChange;
	public event EventHandler<int>? DemolishSideEffect;

	#endregion

	#region Constructor

	public MainViewModel()
	{
		NewGameCommand = new DelegateCommand(param => OnNewGame((string?) param));
		PauseGameCommand = new DelegateCommand(param => OnPauseGame());
		TimeSpeedCommand = new DelegateCommand(param => OnTimeSpeedChange());
		ExitCommand = new DelegateCommand(_ => OnExitGame());
		BuildModeCommand = new DelegateCommand(param => OnBuildMode());
		FinancialAndPopulationCommand = new DelegateCommand(param => OnFinancialAndPopulation());
		BackToMainMenuCommand = new DelegateCommand(param => OnBackToMainMenu());
		LevelUpCommand = new DelegateCommand(param => OnLevelUp());
	}

	#endregion

	#region Methods

	/// <summary>
	/// Called when we start a new game
	/// </summary>
	/// <param name="name">Name of the city</param>
	private void OnNewGame(string? name)
	{
		//Initializing fields
		_model = new City(name ?? string.Empty, true);

		_model.DemolishEvent += ModelOnDemolishEvent;
		_model.ConnectedChangedEvent += ModelOnConnectedChangedEvent;
		_model.UtilizationChangedEvent += ModelOnUtilizationChangedEvent;
		_model.ExpenseIncomeChangedEvent += ModelOnExpenseIncomeEvent;

		ExpenseIncomeList = new ObservableCollection<ExpenseIncome>();
        Fields = new ObservableCollection<ELTECityField>();
		Buildings = new ObservableCollection<Building>();
		_buildMode = false;

		//Generating the map
		RefreshTable();
		//Generating the available building types
		GenerateBuildingTypes();

		OnPropertyChanged("CityName");
		OnPropertyChanged("Size");
		NewGameEvent?.Invoke(this, EventArgs.Empty);
	}

	public void ConfirmDemolish(int index)
	{
		if (_model != null && Fields != null) _model.Demolish(Fields[index].X, Fields[index].Y);
	}

	/// <summary>
	/// Gets executed when the user clicks a button on the map
	/// </summary>
	/// <param name="index">Index of the tile clicked</param>
	private void OnTileClick(int index)
	{
		if (_model == null || Fields == null)
		{
			return;
		}

		if (_buildMode)
		{
			BuildingStatistics_WD = Visibility.Hidden;
			if (_demolish)
			{
				Demolish(index);
			}
			else if (_selectedBuilding == null)
			{
				ShowStatisticsWindow(index);
			}
			else if (_model.Build(Fields[index].X, Fields[index].Y, _selectedBuilding))
			{
				Build(index);
			}
			else
			{
				BuildingStatistics_WD = Visibility.Hidden;
				ShowStatisticsWindow(index);
			}
		}
		else
		{
			BuildingStatistics_WD = Visibility.Hidden;
			ShowStatisticsWindow(index);
		}
	}

	private void Build(int index)
	{
		if (_selectedBuilding == null || _model == null || Fields == null)
		{
			return;
		}

		if (_selectedBuilding is Road)
		{
			UpdateRoadPicture(index);
			UpdateRoadPicture(index - 1);
			UpdateRoadPicture(index + 1);
			UpdateRoadPicture(index - 50);
			UpdateRoadPicture(index + 50);
		}
		else
		{
			for (var i = 0; i < _selectedBuilding.Size.x; i++)
			{
				for (var j = 0; j < _selectedBuilding.Size.y; j++)
				{
					UpdatePictureForBuilding(index, i, j);
				}
			}
		}

		OnPropertyChanged("Funds");
		OnPropertyChanged("Happiness");

		_selectedBuilding = _selectedBuilding.MakeField();
	}

	private void Demolish(int index)
	{
		if (_model == null || Fields == null) return;

		var building = _model.Fields[Fields[index].X, Fields[index].Y];
		if (building == null || !building.Connected ||
		    (_model.Highway.x == Fields[index].X && _model.Highway.y == Fields[index].Y))
		{
			_model.Demolish(Fields[index].X, Fields[index].Y);
			OnPropertyChanged("Funds");
			OnPropertyChanged("Happiness");
		}
		else
		{
			DemolishSideEffect?.Invoke(this, index);
			OnPropertyChanged("Funds");
			OnPropertyChanged("Happiness");
		}

		if (building is Road)
		{
			UpdateRoadPicture(index);
			UpdateRoadPicture(index - 1);
			UpdateRoadPicture(index + 1);
			UpdateRoadPicture(index - 50);
			UpdateRoadPicture(index + 50);
		}
	}

	private void ShowStatisticsWindow(int index)
	{
		if (_model == null || Fields == null)
		{
			return;
		}

		if (_model.Fields[Fields[index].X, Fields[index].Y] == _lastSelectedField)
		{
			BuildingStatistics_WD = Visibility.Hidden;
			_lastSelectedField = null;
		}
		else if (_model.Fields[Fields[index].X, Fields[index].Y] is Zone)
		{
			_lastSelectedField = _model.Fields[Fields[index].X, Fields[index].Y];
			OnPropertyChanged("BuildingLevel");
			OnPropertyChanged("BuildingLevelUpCost");
			OnPropertyChanged("BuildingNumberOfPeople");
			OnPropertyChanged("BuildingHappiness");
			BuildingStatistics_WD = Visibility.Visible;
		}
		else
		{
			_lastSelectedField = _model.Fields[Fields[index].X, Fields[index].Y];
		}
	}

	private void UpdatePictureForBuilding(int index, int i, int j)
	{
		if (_model == null || Fields == null)
		{
			return;
		}

		if (i == 0)
		{
			if (j == 0)
			{
				Fields[index + i * _model.MapSize + j].TilePicture =
					GetPictureForBuilding(_model.Fields[Fields[index].X,
						Fields[index].Y]);
				Fields[index + i * _model.MapSize + j].Scale = (1, 1);
			}

			if (j == 1)
			{
				Fields[index + i * _model.MapSize + j].TilePicture =
					GetPictureForBuilding(_model.Fields[Fields[index].X,
						Fields[index].Y]);
				Fields[index + i * _model.MapSize + j].Scale = (-1, 1);
			}
		}

		if (i == 1)
		{
			if (j == 0)
			{
				Fields[index + i * _model.MapSize + j].TilePicture =
					GetPictureForBuilding(_model.Fields[Fields[index].X,
						Fields[index].Y]);
				Fields[index + i * _model.MapSize + j].Scale = (1, -1);
			}

			if (j == 1)
			{
				Fields[index + i * _model.MapSize + j].TilePicture =
					GetPictureForBuilding(_model.Fields[Fields[index].X,
						Fields[index].Y]);
				Fields[index + i * _model.MapSize + j].Scale = (-1, -1);
			}
		}

		if (!_model.Fields[Fields[index].X, Fields[index].Y]!.Connected)
			Fields[index + i * _model.MapSize + j].MouseLeaveColor = "Black";
	}

	private void UpdateRoadPicture(int index)
	{
		if (_model != null && Fields != null && !IndexOutOfRange(index) &&
		    _model[Fields[index].X, Fields[index].Y] is Road)
		{
			var adjacentRoads =
				new bool[]
				{
					!(_model.IndexOutOfRange(Fields[index].X - 1, Fields[index].Y) ||
					  _model.Fields[Fields[index].X - 1, Fields[index].Y] is not Road) ||
					(Fields[index].X, Fields[index].Y) == _model.Highway,
					!(_model.IndexOutOfRange(Fields[index].X + 1, Fields[index].Y) ||
					  _model.Fields[Fields[index].X + 1, Fields[index].Y] is not Road),
					!(_model.IndexOutOfRange(Fields[index].X, Fields[index].Y - 1) ||
					  _model.Fields[Fields[index].X, Fields[index].Y - 1] is not Road),
					!(_model.IndexOutOfRange(Fields[index].X, Fields[index].Y + 1) ||
					  _model.Fields[Fields[index].X, Fields[index].Y + 1] is not Road)
				};
			var adjacentRoadsCount = Convert.ToInt32(adjacentRoads[0]) + Convert.ToInt32(adjacentRoads[1]) +
			                         Convert.ToInt32(adjacentRoads[2]) + Convert.ToInt32(adjacentRoads[3]);
			if (adjacentRoadsCount == 0)
			{
				var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
				Fields[index].TilePicture =
					new BitmapImage(new Uri(imagePath));
			}

			if (adjacentRoadsCount == 1)
			{
				if (adjacentRoads[0] || adjacentRoads[1])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 90;
				}

				if (adjacentRoads[2] || adjacentRoads[3])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 0;
				}
			}

			if (adjacentRoadsCount == 2)
			{
				if (adjacentRoads[0] && adjacentRoads[1])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 90;
				}

				if (adjacentRoads[2] && adjacentRoads[3])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 0;
				}

				if (adjacentRoads[0] && adjacentRoads[2])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_curve.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 180;
				}

				if (adjacentRoads[1] && adjacentRoads[2])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_curve.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 90;
				}

				if (adjacentRoads[1] && adjacentRoads[3])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_curve.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 0;
				}

				if (adjacentRoads[0] && adjacentRoads[3])
				{
					var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_curve.png");
					Fields[index].TilePicture =
						new BitmapImage(new Uri(imagePath));
					Fields[index].Rotate = 270;
				}
			}

			if (adjacentRoadsCount == 3)
			{
				var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_half_cross.png");
				Fields[index].TilePicture =
					new BitmapImage(new Uri(imagePath));
				if (!adjacentRoads[0]) Fields[index].Rotate = 0;

				if (!adjacentRoads[1]) Fields[index].Rotate = 180;

				if (!adjacentRoads[2]) Fields[index].Rotate = 270;

				if (!adjacentRoads[3]) Fields[index].Rotate = 90;
			}

			if (adjacentRoadsCount == 4)
			{
				var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_cross.png");
				Fields[index].TilePicture =
					new BitmapImage(new Uri(imagePath));
			}
		}
	}

	/// <summary>
	/// Gets executed when the user selects a building in building mode
	/// </summary>
	/// <param name="index">Index of the clicked building</param>
	private void OnSelectBuilding(int index)
	{
		//if index is 10 we are in demolish mode
		if (index == 10)
		{
			if (_demolish && Buildings != null)
			{
				Buildings[index - 1].Color = "LightGray";
				_demolish = false;
			}
			else
			{
				Buildings![_selectedBuildingIndex].Color = "LightGray";
				_demolish = true;
				Buildings[index - 1].Color = "Yellow";
				_selectedBuildingIndex = index - 1;
				_selectedBuilding = null;
			}
		}
		else
		{
			UpdateSelectedField(index);
		}
	}

	private void UpdateSelectedField(int index)
	{
		if (Buildings == null)
		{
			return;
		}

		if (_demolish)
		{
			Buildings[9].Color = "LightGray";
			_demolish = false;
		}

		if (Buildings[index - 1].BuildingType != null &&
		    Buildings[index - 1].BuildingType!.GetType() == _selectedBuilding?.GetType())
		{
			_selectedBuilding = null;
			Buildings[_selectedBuildingIndex].Color = "LightGray";
		}
		else
		{
			if (_selectedBuilding != null)
				Buildings[_selectedBuildingIndex].Color = "LightGray";

			if (Buildings[index - 1].BuildingType != null)
			{
				_selectedBuilding = Buildings[index - 1].BuildingType!.MakeField();
			}

			Buildings[index - 1].Color = "Yellow";

			_selectedBuildingIndex = index - 1;
		}
	}

	/// <summary>
	/// Updates the properties on the view
	/// </summary>
	public void UpdateProperty()
	{
		OnPropertyChanged("Date");
		OnPropertyChanged("Funds");
		OnPropertyChanged("Happiness");
		OnPropertyChanged("PopulationSize");
		OnPropertyChanged("BuildingLevel");
		OnPropertyChanged("BuildingLevelUpCost");
		OnPropertyChanged("BuildingNumberOfPeople");
		OnPropertyChanged("BuildingHappiness");
		OnPropertyChanged("CurrentIncome");
		OnPropertyChanged("CurrentExpense");
		OnPropertyChanged("CurrentProfit");
		OnPropertyChanged("LastIncome");
		OnPropertyChanged("LastExpense");
		OnPropertyChanged("LastProfit");
		OnPropertyChanged("LowDegreeNumber");
		OnPropertyChanged("MidDegreeNumber");
		OnPropertyChanged("HighDegreeNumber");
		OnPropertyChanged("NumberOfRetired");
	}

	/// <summary>
	/// Invokes the PauseGame event and stops the timer
	/// </summary>
	private void OnPauseGame()
	{
		if (IsPaused /*|| _model.GetGameOver*/) IsPaused = true;
		PauseGame?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Changes the speed of the game
	/// </summary>
	private void OnTimeSpeedChange()
	{
		if ((int) _timeSpeed == 1)
			_timeSpeed = (Speed) 2;
		else if ((int) _timeSpeed == 2)
			_timeSpeed = (Speed) 3;
		else if ((int) _timeSpeed == 3) _timeSpeed = (Speed) 1;
		TimeSpeedChange?.Invoke(this, EventArgs.Empty);
		OnPropertyChanged("TimeType");
	}

	/// <summary>
	/// The Building side menu comes up
	/// </summary>
	private void OnBuildMode()
	{
		if (BuildMode_WD != Visibility.Visible)
		{
			BuildMode_WD = Visibility.Visible;
			FinancialAndPopulation_WD = Visibility.Hidden;
			OnPauseGame();
		}
		else if (BuildMode_WD == Visibility.Visible)
		{
			OnPauseGame();
		}

		_buildMode = true;
	}

	/// <summary>
	/// The Financial and Population side menu comes up
	/// </summary>
	private void OnFinancialAndPopulation()
	{
		if (FinancialAndPopulation_WD != Visibility.Visible)
		{
			FinancialAndPopulation_WD = Visibility.Visible;
			BuildMode_WD = Visibility.Hidden;
			_buildMode = false;
		}
	}

	/// <summary>
	/// The view goes back to the main menu
	/// </summary>
	private void OnBackToMainMenu()
	{
		BackToMainMenuEvent?.Invoke(this, EventArgs.Empty);

		_buildMode = false;
	}

	private void OnLevelUp()
	{
		if (_model != null && _lastSelectedField != null &&
		    _model.Upgrade(_lastSelectedField.Location.x, _lastSelectedField.Location.y) && Fields != null)
		{
			Fields[_lastSelectedField.Location.x * 50 + _lastSelectedField.Location.y].TilePicture =
				new BitmapImage(_lastSelectedField.GetPictureSource());
			OnPropertyChanged("Funds");
			OnPropertyChanged("BuildingLevel");
			OnPropertyChanged("BuildingLevelUpCost");
		}
	}

	/// <summary>
	/// Generate all buildings that a player can build during the game
	/// </summary>
	private void GenerateBuildingTypes()
	{
		if (Buildings != null)
		{
			var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/commercial_low_1.png");
			Buildings.Add(new Building
			{
				Number = 1,
				BuildingType = new Commercial(),
				BuildingName = "Commercial Zone",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Commercial().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/industrial_low_high.png");
			Buildings.Add(new Building
			{
				Number = 2,
				BuildingType = new Industrial(),
				BuildingName = "Industrial Zone",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Industrial().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/residential_low_high.png");
			Buildings.Add(new Building
			{
				Number = 3,
				BuildingType = new Residential(),
				BuildingName = "Residential Zone",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Residential().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/forest.png");
			Buildings.Add(new Building
			{
				Number = 4,
				BuildingType = new Forest(),
				BuildingName = "Forest",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Forest().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/police.png");
			Buildings.Add(new Building
			{
				Number = 5,
				BuildingType = new Police(),
				BuildingName = "Police Station",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Police().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/school.png");
			Buildings.Add(new Building
			{
				Number = 6,
				BuildingType = new School(),
				BuildingName = "High School",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new School().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/university.png");
			Buildings.Add(new Building
			{
				Number = 7,
				BuildingType = new University(),
				BuildingName = "University",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new University().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/stadium.png");
			Buildings.Add(new Building
			{
				Number = 8,
				BuildingType = new Stadium(),
				BuildingName = "Stadium",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Stadium().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/road_line.png");
			Buildings.Add(new Building
			{
				Number = 9,
				BuildingType = new Road(),
				BuildingName = "Road",
				BuildingPicture = new BitmapImage(new Uri(imagePath)),
				IsEnabled = true,
				Price = "Price: $" + new Road().Price,
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
			Buildings.Add(new Building()
			{
				Number = 10,
				BuildingType = null,
				BuildingName = "",
				BuildingPicture = null,
				IsEnabled = true,
				Price = "Demolish",
				Color = "LightGray",
				SelectBuildingCommand = new DelegateCommand(param => OnSelectBuilding((int) param!))
			});
		}
	}

	/// <summary>
	/// Refresh tiles according to data stored in the model
	/// </summary>
	private void RefreshTable()
	{
		if (_model != null)
		{
			var index = 0;
			for (var i = 0; i < _model.MapSize; i++)
			for (var j = 0; j < _model.MapSize; j++)
			{
				double happiness;
				Zone? zone = null;
				if (_model.Fields[i, j] != null) zone = _model.Fields[i, j] as Zone;
				if (zone != null && _model.Fields[i, j] is Zone)
					happiness = zone.HappinessRate(_model);
				else
					happiness = -1.0;
				if (Fields != null)
					Fields.Add(new ELTECityField()
					{
						Number = index,
						X = (i * 100 + j) / 100,
						Y = (i * 100 + j) % 100,
						TilePicture = GetPictureForBuilding(_model.Fields[i, j]),
						Rotate = 0,
						Scale = (1, 1),
						MouseLeaveColor = "Transparent",
						MouseOverColor = "Black",
						Happiness = happiness,
						IsEnabled = true,
						TileClickCommand = new DelegateCommand(param => OnTileClick((int) param!))
					});
				index++;
			}
		}

		OnPropertyChanged("Happiness");
		OnPropertyChanged("Date");
		OnPropertyChanged("PopulationSize");
	}

	private BitmapImage GetPictureForBuilding(IField? field)

	{
		var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/grass.png");
		return new BitmapImage(field?.GetPictureSource() ?? new Uri(imagePath));
	}

	private void OnExitGame()
	{
		ExitGameCommand?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Gets called when a building is demolished
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e">X and Y coordinates of the destroyed building's coordinates</param>
	private void ModelOnDemolishEvent(object? sender, (int x, int y) e)
	{
		if (_model != null && Fields != null)
		{
			Fields[e.x * 50 + e.y].TilePicture = GetPictureForBuilding(_model.Fields[e.x, e.y]);
			Fields[e.x * 50 + e.y].Rotate = 0;
			Fields[e.x * 50 + e.y].Scale = (1, 1);
			Fields[e.x * 50 + e.y].MouseLeaveColor = "Transparent";
		}
	}

	/// <summary>
	/// Gets called when the connected property of a field is changed
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e">X and Y coordinates of the changed building's coordinates</param>
	private void ModelOnConnectedChangedEvent(object? sender, (int x, int y) e)
	{
		if (_model != null)
			// Change the border color of the building based on whether it's connected or not
			// Change the border color of the building based on whether it's connected or not
			Fields![e.x * 50 + e.y].MouseLeaveColor =
				!_model.Fields[e.x, e.y]!.Connected && !(_model.Fields[e.x, e.y] is Forest) ? "Black" : "Transparent";
	}

	/// <summary>
	/// Generate the latest max 100 Expense/Income 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ModelOnExpenseIncomeEvent(object? sender, EventArgs e)
	{
		if (_model!._expense_income.Count == 0)
		{
			return;
		}

		ExpenseIncomeList!.Clear();
		for (int i = 0; i < _model._expense_income.Count; ++i)
		{
			String color = (_model._expense_income[i].money >= 0 ? "Green" : "Red");
			ExpenseIncomeList.Add(new ExpenseIncome
			{
				Description = _model._expense_income[i].desc,
				Money = _model._expense_income[i].money.ToString() + "$",
				Color = color
			});
		}
	}

	private void ModelOnUtilizationChangedEvent(object? sender, (int x, int y) e)
	{
		if (Fields != null && _model != null)
			Fields[e.x * 50 + e.y].TilePicture = GetPictureForBuilding(_model.Fields[e.x, e.y]);
	}

	private bool IndexOutOfRange(int index)
	{
		if (_model != null) return 0 > index || index >= _model.MapSize * _model.MapSize;
		return true;
	}

	#endregion
}