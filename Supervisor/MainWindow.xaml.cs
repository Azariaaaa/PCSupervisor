using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Supervisor.ViewModels;

namespace Supervisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();
            viewModel.PropertyChanged += MainWindow_PropertyChanged;
            DataContext = viewModel;
            InitializeComponent();
        }
        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = sender as MainWindowViewModel;

            if (e.PropertyName == nameof(MainWindowViewModel.CpuUsage))
            {
                AnimateProgressBar(cpuProgressBar, viewModel.CpuUsage ?? 0);
            }

            if (e.PropertyName == nameof(MainWindowViewModel.RamUsagePercentage))
            {
                AnimateProgressBar(ramProgressBar, viewModel.RamUsagePercentage);
            }
        }

        private void AnimateProgressBar(ProgressBar progressBar, double newValue)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = progressBar.Value,
                To = newValue,
                Duration = new Duration(TimeSpan.FromMilliseconds(350))
            };

            progressBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }
    }
}
