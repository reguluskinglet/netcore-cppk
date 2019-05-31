using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NLog;
using TabletLocker.Db;
using TabletLocker.Model;

namespace TabletLocker
{
    /// <summary>
    /// Логика взаимодействия для AdminLoginPage.xaml
    /// </summary>
    public partial class AdminLoginPage : Page
    {
        private readonly PageContainer _container;
        private readonly IUserRepository _userRepository = new UserRepository();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public AdminLoginPage(PageContainer cont)
        {
            InitializeComponent();

            _container = cont;

            var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimeMark.Text = DateTime.Now.ToString("HH:mm");
                DateMark.Text = DateTime.Now.ToString("dd.MM.yyyy");
            }, Dispatcher);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _container.BtnBack_Click();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = LoginInput.Text.Trim();
            var password = PasswordInput.Password.Trim();

            if (login.Length > 0 && password.Length > 0)
            {
                User admin = null;
                try
                {
                    admin = _userRepository.FindAdminByLogin(login, password);
                }
                catch (Exception ex)
                {
                    //log? todo
                    _logger.Error(ex, "FindAdminByLogin error");
                }

                if (admin == null)
                {
                    _logger.Error($"admin section login failed, login was [{login}]");
                    //error window
                    LoginInput.Text = "";
                    PasswordInput.Text = "";
                    ShowErrorMessage("ошибка", "не удалось войти");
                }
                else
                {
                    _logger.Debug($"admin section login success, login was [{login}]");
                    //change state
                    _container.CurrentUser = admin;
                    _container.StateAdminList();
                }
            }
        }

        private void ShowErrorMessage(string title, string msg)
        {
            AdminCanvasBlur.Radius = 20;
            MsgBoxTitle.Text = title;
            MsgBoxMessage.Text = msg;
            MsgBox.Visibility = Visibility.Visible;
        }

        private void Button_Click_Msgbox_Return(object sender, RoutedEventArgs e)
        {
            //AdminCanvasBlur.Radius = 0;
            //MsgBox.Visibility = Visibility.Hidden;
            _container.StateMainMenu();
        }
    }
}
