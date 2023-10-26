using ELTECity.Model;
using ELTECity.WPF.View;
using ELTECity.WPF.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace ELTECity.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        City? _model;
        MainWindow? _mainView;
        GameWindow? _gameView;
        MainViewModel? _viewModel;
        DispatcherTimer? _timer;

        private bool _forceclose = false;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {

            _mainView = new MainWindow();
            _viewModel = new MainViewModel();
            _gameView = new GameWindow();
            _viewModel.NewGameEvent += OnNewGame;
            _viewModel.ExitGameCommand += OnExitGame;

            _timer = new DispatcherTimer(DispatcherPriority.Input);
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += new EventHandler(Timer_Tick);

            _mainView.Show();

            _mainView.DataContext = _viewModel;
        }

        private void OnNewGame(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _model = _viewModel.Model;
                _viewModel.BackToMainMenuEvent += OnBackToMainMenu;

                _viewModel.PauseGame += ViewModel_PauseGame;
                _viewModel.TimeSpeedChange += ViewModel_TimeChange;
                _viewModel.DemolishSideEffect += ViewModelOnDemolishSideEffect;
            }
            if (_gameView != null)
            {
                _gameView.DataContext = _viewModel;
            }
            _mainView?.Close();
            _gameView?.Show();

            _gameView!.Closing += OnCloseGame;

            _model!.GameOverEvent += ModelOnGameOverEvent;
        }

        private void ModelOnGameOverEvent(object? sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
            if ((MessageBox.Show("You have been fired!", "Game Over", MessageBoxButton.OK) == MessageBoxResult.OK))
            {
                InitMainMenu();
            }

        }

        private void ViewModelOnDemolishSideEffect(object? sender, int index)
        {
            if (MessageBox.Show("Demolishing this building will result in side effects." + Environment.NewLine +
                                "Do you want to continue?",
                    "Demolish",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Asterisk) == MessageBoxResult.OK && _viewModel != null)
            {
                _viewModel.ConfirmDemolish(index);
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_model != null)
            {
                _model.AdvanceDate();
            }
            if (_viewModel != null)
            {
                _viewModel.UpdateProperty();
            }
        }

        private void InitMainMenu()
        {
            _mainView = new MainWindow();
            _viewModel = new MainViewModel();

            _forceclose = true;
            _gameView?.Close();
            _forceclose = false;

            _gameView = new GameWindow();
            _viewModel.NewGameEvent += OnNewGame;
            _viewModel.ExitGameCommand += OnExitGame;

            _mainView.Show();

            _mainView.DataContext = _viewModel;
        }


        private void OnBackToMainMenu(object? sender, EventArgs e)
        {

            if (MessageBox.Show("Are you sure you want to go back to the main menu?" + Environment.NewLine +
                                "If you go back all your progress will be lost.",
                    "ELTECity",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Asterisk) == MessageBoxResult.OK
                    && _timer != null)
            {
                _timer.Stop();
                InitMainMenu();
            }
        }

        #region Viewmodel event handlers

        private void ViewModel_TimeChange(object? sender, EventArgs e)
        {
            if (_timer != null)
            {
                if (_viewModel != null && !_viewModel.IsPaused)
                {
                    _viewModel.IsPaused = false;
                    _viewModel.TimeType = "►";
                    _timer.Interval = TimeSpan.FromMilliseconds(2000);
                    _timer.Start();
                }
                else if (_viewModel != null)
                {
                    if (_viewModel.TimeType == "►")
                    {
                        _timer.Interval = TimeSpan.FromMilliseconds(2000);
                    }
                    else if (_viewModel.TimeType == "►►")
                    {
                        _timer.Interval = TimeSpan.FromMilliseconds(1000);
                    }
                    else if (_viewModel.TimeType == "►►►")
                    {
                        _timer.Interval = TimeSpan.FromMilliseconds(100);
                    }
                }
            }
        }

        private void ViewModel_PauseGame(object? sender, EventArgs e)
        {
            if (_viewModel != null && _timer != null && !_viewModel.IsPaused)
            {
                _timer.Stop();
            }
        }

        #endregion

        private void OnExitGame(object? sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?",
                    "ELTECity",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void OnCloseGame(object? sender, CancelEventArgs e)
        {
            if (_forceclose)
            {
                return;
            }
            if (MessageBox.Show("Are you sure you want to quit?",
                    "ELTECity",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk) == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
