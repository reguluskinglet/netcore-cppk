using System.Windows;
using System.Windows.Navigation;

namespace WpfMultiScreens.Windows
{
    /// <summary>
    /// Логика взаимодействия для ScreenWindow.xaml
    /// </summary>
    public partial class ScreenWindow : Window
    {
        public ScreenWindow()
        {
            InitializeComponent();
        }

        void frame_Navigated(object sender, NavigationEventArgs e)
        {
            MainFrame.NavigationService.RemoveBackEntry();
        }
    }
}
