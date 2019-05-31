using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using TabletLocker.Db;
using TabletLocker.Model;

namespace TabletLocker
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly PageContainer _container;
        private ScrollDragger _dragger;

        public MainPage(PageContainer cont)
        {
            InitializeComponent();

            _container = cont;

            var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimeMark.Text = DateTime.Now.ToString("HH:mm");
                DateMark.Text = DateTime.Now.ToString("dd.MM.yyyy");
            }, Dispatcher);

            _dragger = new ScrollDragger(TroubleSvp, TroubleSv);
        }

        private void BtnBarcodeEnter_Click(object sender, RoutedEventArgs e)
        {
            var code = BarcodeInput.Text.Trim();
            BarcodeInput.Text = "";
            _container.BarcodeProcess(code);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _container.BtnBack_Click();
        }

        private void Button_Click_AdminLogin(object sender, RoutedEventArgs e)
        {
            _container.BtnAdmin_Click();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click_Msgbox_Return(object sender, RoutedEventArgs e)
        {
            _container.BtnMainMsgBoxReturn_Click();
        }

        private void BtnTake_Click(object sender, RoutedEventArgs e)
        {
            _container.BtnTake_Click();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            //_container.BtnReturn_Click();
        }

        private void BtnReport_Click(object sender, RoutedEventArgs e)
        {
            _container.BtnReport_Click();
        }

        //trouble report window
        private void BtnTroubleInputSelect_Click(object sender, RoutedEventArgs e)
        {
            //if selected - incident created
            int? troubleId = null;
            foreach (RadioButton radio in TroubleSvp.Children)
            {
                if (radio.IsChecked == true && radio.Name.Contains("_"))
                {
                    troubleId = int.Parse(radio.Name.Split('_')[1]);
                }
            }
            _container.BtnTroubleInputSelect_Click(troubleId);
        }

        private void BtnTroubleInputCancel_Click(object sender, RoutedEventArgs e)
        {
            _container.TroubleInputCancel_Click();
        }

        private void BtnTroubleInputClose_Click(object sender, RoutedEventArgs e)
        {
            _container.TroubleInputCancel_Click();
        }

        private void TroubleSv_ManipulationBoundaryFeedback(object sender, System.Windows.Input.ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }
    }
}
