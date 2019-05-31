using System.Diagnostics;
using System.Windows;
using NLog;
using RzdMonitors.Data.Base;
using RzdMonitors.Data.Enums;

namespace WpfMultiScreens.Pages
{
    /// <summary>
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : IReportPage
    {
        public ScreenType ScreenType { get; set; } = ScreenType.None;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void UpdateData()
        {
            //do nothing
            _logger.Debug("UpdateData()");
        }

        public StartPage()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
