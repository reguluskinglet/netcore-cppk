using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace TabletLocker.Utils
{
    public class TouchScreenKeyboard : Window
    {
        #region Property & Variable & Constructor

        public static double WidthTouchKeyboard { get; set; } = 830;
        public static double HeightTouchKeyboard { get; set; } = 290;

        protected static bool ShiftFlag { get; set; }

        private static bool _capsLockFlag;

        protected static bool CapsLockFlag
        {
            get => TouchScreenKeyboard._capsLockFlag;
            set => TouchScreenKeyboard._capsLockFlag = value;
        }

        private static Window _instanceObject;

        private static Brush _previousTextBoxBackgroundBrush = null;
        private static Brush _previousTextBoxBorderBrush = null;
        private static Thickness _previousTextBoxBorderThickness;

        private static Control _currentControl;
        public static string TouchScreenText
        {
            get
            {
                if (_currentControl is WatermarkPasswordBox watermarkPasswordBox)
                {
                    return watermarkPasswordBox.Password;
                } else if (_currentControl is TextBox box)
                {
                    return box.Text;
                }
                else if (_currentControl is PasswordBox passwordBox)
                {
                    return passwordBox.Password;
                }
                else return "";
            }
            set
            {
                if (_currentControl is WatermarkPasswordBox box)
                {
                    box.Password = value;
                } else if (_currentControl is TextBox textBox)
                {
                    textBox.Text = value;
                }
                else if (_currentControl is PasswordBox passwordBox)
                {
                    passwordBox.Password = value;
                }
            }
        }

        public static RoutedUICommand CmdTlide = new RoutedUICommand();
        public static RoutedUICommand Cmd1 = new RoutedUICommand();
        public static RoutedUICommand Cmd2 = new RoutedUICommand();
        public static RoutedUICommand Cmd3 = new RoutedUICommand();
        public static RoutedUICommand Cmd4 = new RoutedUICommand();
        public static RoutedUICommand Cmd5 = new RoutedUICommand();
        public static RoutedUICommand Cmd6 = new RoutedUICommand();
        public static RoutedUICommand Cmd7 = new RoutedUICommand();
        public static RoutedUICommand Cmd8 = new RoutedUICommand();
        public static RoutedUICommand Cmd9 = new RoutedUICommand();
        public static RoutedUICommand Cmd0 = new RoutedUICommand();
        public static RoutedUICommand CmdMinus = new RoutedUICommand();
        public static RoutedUICommand CmdPlus = new RoutedUICommand();
        public static RoutedUICommand CmdBackspace = new RoutedUICommand();

        public static RoutedUICommand CmdTab = new RoutedUICommand();
        public static RoutedUICommand CmdQ = new RoutedUICommand();
        public static RoutedUICommand Cmdw = new RoutedUICommand();
        public static RoutedUICommand CmdE = new RoutedUICommand();
        public static RoutedUICommand CmdR = new RoutedUICommand();
        public static RoutedUICommand CmdT = new RoutedUICommand();
        public static RoutedUICommand CmdY = new RoutedUICommand();
        public static RoutedUICommand CmdU = new RoutedUICommand();
        public static RoutedUICommand CmdI = new RoutedUICommand();
        public static RoutedUICommand CmdO = new RoutedUICommand();
        public static RoutedUICommand CmdP = new RoutedUICommand();
        public static RoutedUICommand CmdOpenCrulyBrace = new RoutedUICommand();
        public static RoutedUICommand CmdEndCrultBrace = new RoutedUICommand();
        public static RoutedUICommand CmdOR = new RoutedUICommand();

        public static RoutedUICommand CmdCapsLock = new RoutedUICommand();
        public static RoutedUICommand CmdA = new RoutedUICommand();
        public static RoutedUICommand CmdS = new RoutedUICommand();
        public static RoutedUICommand CmdD = new RoutedUICommand();
        public static RoutedUICommand CmdF = new RoutedUICommand();
        public static RoutedUICommand CmdG = new RoutedUICommand();
        public static RoutedUICommand CmdH = new RoutedUICommand();
        public static RoutedUICommand CmdJ = new RoutedUICommand();
        public static RoutedUICommand CmdK = new RoutedUICommand();
        public static RoutedUICommand CmdL = new RoutedUICommand();
        public static RoutedUICommand CmdColon = new RoutedUICommand();
        public static RoutedUICommand CmdDoubleInvertedComma = new RoutedUICommand();
        public static RoutedUICommand CmdEnter = new RoutedUICommand();

        public static RoutedUICommand CmdShift = new RoutedUICommand();
        public static RoutedUICommand CmdZ = new RoutedUICommand();
        public static RoutedUICommand CmdX = new RoutedUICommand();
        public static RoutedUICommand CmdC = new RoutedUICommand();
        public static RoutedUICommand CmdV = new RoutedUICommand();
        public static RoutedUICommand CmdB = new RoutedUICommand();
        public static RoutedUICommand CmdN = new RoutedUICommand();
        public static RoutedUICommand CmdM = new RoutedUICommand();
        public static RoutedUICommand CmdGreaterThan = new RoutedUICommand();
        public static RoutedUICommand CmdLessThan = new RoutedUICommand();
        public static RoutedUICommand CmdQuestion = new RoutedUICommand();

        public static RoutedUICommand CmdSpaceBar = new RoutedUICommand();
        public static RoutedUICommand CmdClear = new RoutedUICommand();

        public TouchScreenKeyboard()
        {
            this.Width = WidthTouchKeyboard;
            this.Height = HeightTouchKeyboard;
        }

        static TouchScreenKeyboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TouchScreenKeyboard), new FrameworkPropertyMetadata(typeof(TouchScreenKeyboard)));

            SetCommandBinding();
        }
        #endregion
        #region CommandRelatedCode
        private static void SetCommandBinding()
        {
            CommandBinding CbTlide = new CommandBinding(CmdTlide, RunCommand);
            CommandBinding Cb1 = new CommandBinding(Cmd1, RunCommand);
            CommandBinding Cb2 = new CommandBinding(Cmd2, RunCommand);
            CommandBinding Cb3 = new CommandBinding(Cmd3, RunCommand);
            CommandBinding Cb4 = new CommandBinding(Cmd4, RunCommand);
            CommandBinding Cb5 = new CommandBinding(Cmd5, RunCommand);
            CommandBinding Cb6 = new CommandBinding(Cmd6, RunCommand);
            CommandBinding Cb7 = new CommandBinding(Cmd7, RunCommand);
            CommandBinding Cb8 = new CommandBinding(Cmd8, RunCommand);
            CommandBinding Cb9 = new CommandBinding(Cmd9, RunCommand);
            CommandBinding Cb0 = new CommandBinding(Cmd0, RunCommand);
            CommandBinding CbMinus = new CommandBinding(CmdMinus, RunCommand);
            CommandBinding CbPlus = new CommandBinding(CmdPlus, RunCommand);
            CommandBinding CbBackspace = new CommandBinding(CmdBackspace, RunCommand);

            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbTlide);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb1);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb2);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb3);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb4);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb5);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb6);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb7);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb8);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb9);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cb0);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbMinus);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbPlus);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbBackspace);


            CommandBinding CbTab = new CommandBinding(CmdTab, RunCommand);
            CommandBinding CbQ = new CommandBinding(CmdQ, RunCommand);
            CommandBinding Cbw = new CommandBinding(Cmdw, RunCommand);
            CommandBinding CbE = new CommandBinding(CmdE, RunCommand);
            CommandBinding CbR = new CommandBinding(CmdR, RunCommand);
            CommandBinding CbT = new CommandBinding(CmdT, RunCommand);
            CommandBinding CbY = new CommandBinding(CmdY, RunCommand);
            CommandBinding CbU = new CommandBinding(CmdU, RunCommand);
            CommandBinding CbI = new CommandBinding(CmdI, RunCommand);
            CommandBinding Cbo = new CommandBinding(CmdO, RunCommand);
            CommandBinding CbP = new CommandBinding(CmdP, RunCommand);
            CommandBinding CbOpenCrulyBrace = new CommandBinding(CmdOpenCrulyBrace, RunCommand);
            CommandBinding CbEndCrultBrace = new CommandBinding(CmdEndCrultBrace, RunCommand);
            CommandBinding CbOR = new CommandBinding(CmdOR, RunCommand);

            CommandBinding CbCapsLock = new CommandBinding(CmdCapsLock, RunCommand);
            CommandBinding CbA = new CommandBinding(CmdA, RunCommand);
            CommandBinding CbS = new CommandBinding(CmdS, RunCommand);
            CommandBinding CbD = new CommandBinding(CmdD, RunCommand);
            CommandBinding CbF = new CommandBinding(CmdF, RunCommand);
            CommandBinding CbG = new CommandBinding(CmdG, RunCommand);
            CommandBinding CbH = new CommandBinding(CmdH, RunCommand);
            CommandBinding CbJ = new CommandBinding(CmdJ, RunCommand);
            CommandBinding CbK = new CommandBinding(CmdK, RunCommand);
            CommandBinding CbL = new CommandBinding(CmdL, RunCommand);
            CommandBinding CbColon = new CommandBinding(CmdColon, RunCommand);
            CommandBinding CbDoubleInvertedComma = new CommandBinding(CmdDoubleInvertedComma, RunCommand);
            CommandBinding CbEnter = new CommandBinding(CmdEnter, RunCommand);

            CommandBinding CbShift = new CommandBinding(CmdShift, RunCommand);
            CommandBinding CbZ = new CommandBinding(CmdZ, RunCommand);
            CommandBinding CbX = new CommandBinding(CmdX, RunCommand);
            CommandBinding CbC = new CommandBinding(CmdC, RunCommand);
            CommandBinding CbV = new CommandBinding(CmdV, RunCommand);
            CommandBinding CbB = new CommandBinding(CmdB, RunCommand);
            CommandBinding CbN = new CommandBinding(CmdN, RunCommand);
            CommandBinding CbM = new CommandBinding(CmdM, RunCommand);
            CommandBinding CbGreaterThan = new CommandBinding(CmdGreaterThan, RunCommand);
            CommandBinding CbLessThan = new CommandBinding(CmdLessThan, RunCommand);
            CommandBinding CbQuestion = new CommandBinding(CmdQuestion, RunCommand);



            CommandBinding CbSpaceBar = new CommandBinding(CmdSpaceBar, RunCommand);
            CommandBinding CbClear = new CommandBinding(CmdClear, RunCommand);

            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbTab);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbQ);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cbw);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbE);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbR);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbT);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbY);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbU);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbI);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), Cbo);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbP);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbOpenCrulyBrace);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbEndCrultBrace);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbOR);

            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbCapsLock);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbA);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbS);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbD);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbF);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbG);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbH);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbJ);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbK);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbL);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbColon);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbDoubleInvertedComma);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbEnter);

            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbShift);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbZ);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbX);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbC);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbV);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbB);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbN);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbM);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbGreaterThan);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbLessThan);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbQuestion);



            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbSpaceBar);
            CommandManager.RegisterClassCommandBinding(typeof(TouchScreenKeyboard), CbClear);

        }
        static void RunCommand(object sender, ExecutedRoutedEventArgs e)
        {

            if (e.Command == CmdTlide)  //First Row
            {


                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "`";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "~";
                    ShiftFlag = false;
                }
            }
            else if (e.Command == Cmd1)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "1";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "!";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd2)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "2";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "@";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd3)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "3";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "#";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd4)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "4";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "$";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd5)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "5";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "%";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd6)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "6";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "^";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd7)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "7";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "&";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd8)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "8";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "*";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd9)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "9";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "(";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == Cmd0)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "0";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += ")";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdMinus)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "-";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "_";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdPlus)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "=";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "+";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdBackspace)
            {
                if (!string.IsNullOrEmpty(TouchScreenKeyboard.TouchScreenText))
                {
                    TouchScreenKeyboard.TouchScreenText = TouchScreenKeyboard.TouchScreenText.Substring(0, TouchScreenKeyboard.TouchScreenText.Length - 1);
                }

            }
            else if (e.Command == CmdTab)  //Second Row
            {
                TouchScreenKeyboard.TouchScreenText += "     ";
            }
            else if (e.Command == CmdQ)
            {
                AddKeyBoardINput('Q');
            }
            else if (e.Command == Cmdw)
            {
                AddKeyBoardINput('w');
            }
            else if (e.Command == CmdE)
            {
                AddKeyBoardINput('E');
            }
            else if (e.Command == CmdR)
            {
                AddKeyBoardINput('R');
            }
            else if (e.Command == CmdT)
            {
                AddKeyBoardINput('T');
            }
            else if (e.Command == CmdY)
            {
                AddKeyBoardINput('Y');
            }
            else if (e.Command == CmdU)
            {
                AddKeyBoardINput('U');

            }
            else if (e.Command == CmdI)
            {
                AddKeyBoardINput('I');
            }
            else if (e.Command == CmdO)
            {
                AddKeyBoardINput('O');
            }
            else if (e.Command == CmdP)
            {
                AddKeyBoardINput('P');
            }
            else if (e.Command == CmdOpenCrulyBrace)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "[";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "{";
                    ShiftFlag = false;
                }
            }
            else if (e.Command == CmdEndCrultBrace)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "]";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "}";
                    ShiftFlag = false;
                }
            }
            else if (e.Command == CmdOR)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += @"\";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "|";
                    ShiftFlag = false;
                }
            }
            else if (e.Command == CmdCapsLock)  ///Third ROw
            {

                if (!CapsLockFlag)
                {
                    CapsLockFlag = true;
                }
                else
                {
                    CapsLockFlag = false;

                }
            }
            else if (e.Command == CmdA)
            {
                AddKeyBoardINput('A');
            }
            else if (e.Command == CmdS)
            {
                AddKeyBoardINput('S');
            }
            else if (e.Command == CmdD)
            {
                AddKeyBoardINput('D');
            }
            else if (e.Command == CmdF)
            {
                AddKeyBoardINput('F');
            }
            else if (e.Command == CmdG)
            {
                AddKeyBoardINput('G');
            }
            else if (e.Command == CmdH)
            {
                AddKeyBoardINput('H');
            }
            else if (e.Command == CmdJ)
            {
                AddKeyBoardINput('J');
            }
            else if (e.Command == CmdK)
            {
                AddKeyBoardINput('K');
            }
            else if (e.Command == CmdL)
            {
                AddKeyBoardINput('L');

            }
            else if (e.Command == CmdColon)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += ";";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += ":";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdDoubleInvertedComma)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "'";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += Char.ConvertFromUtf32(34);
                    ShiftFlag = false;
                }


            }
            else if (e.Command == CmdEnter)
            {
                if (_instanceObject != null)
                {
                    _instanceObject.Close();
                    _instanceObject = null;
                }
                _currentControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));


            }
            else if (e.Command == CmdShift) //Fourth Row
            {

                ShiftFlag = true; ;


            }
            else if (e.Command == CmdZ)
            {
                AddKeyBoardINput('Z');

            }
            else if (e.Command == CmdX)
            {
                AddKeyBoardINput('X');

            }
            else if (e.Command == CmdC)
            {
                AddKeyBoardINput('C');

            }
            else if (e.Command == CmdV)
            {
                AddKeyBoardINput('V');

            }
            else if (e.Command == CmdB)
            {
                AddKeyBoardINput('B');

            }
            else if (e.Command == CmdN)
            {
                AddKeyBoardINput('N');

            }
            else if (e.Command == CmdM)
            {
                AddKeyBoardINput('M');

            }
            else if (e.Command == CmdLessThan)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += ",";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "<";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdGreaterThan)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += ".";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += ">";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdQuestion)
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += "/";
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += "?";
                    ShiftFlag = false;
                }

            }
            else if (e.Command == CmdSpaceBar)//Last row
            {

                TouchScreenKeyboard.TouchScreenText += " ";
            }
            else if (e.Command == CmdClear)//Last row
            {

                TouchScreenKeyboard.TouchScreenText = "";
            }
        }
        #endregion
        #region Main Functionality
        private static void AddKeyBoardINput(char input)
        {
            if (CapsLockFlag)
            {
                if (ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += char.ToLower(input).ToString();
                    ShiftFlag = false;

                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += char.ToUpper(input).ToString();
                }
            }
            else
            {
                if (!ShiftFlag)
                {
                    TouchScreenKeyboard.TouchScreenText += char.ToLower(input).ToString();
                }
                else
                {
                    TouchScreenKeyboard.TouchScreenText += char.ToUpper(input).ToString();
                    ShiftFlag = false;
                }
            }
        }


        private static void syncchild()
        {
            if (_currentControl != null && _instanceObject != null)
            {

                var virtualpoint = new Point(0, _currentControl.ActualHeight + 3);
                var Actualpoint = _currentControl.PointToScreen(virtualpoint);

                if (WidthTouchKeyboard + Actualpoint.X > 1280)
                {
                    double difference = WidthTouchKeyboard + 1280;
                    _instanceObject.Left = Actualpoint.X - difference;
                }
                else if (!(Actualpoint.X > 1))
                {
                    _instanceObject.Left = 1;
                }
                else
                {
                    _instanceObject.Left = Actualpoint.X;
                }

                if (HeightTouchKeyboard + Actualpoint.Y > 780)
                {
                    //show on top istead of bottom
                    var virtuaPoinNew = new Point(0, -HeightTouchKeyboard - 3);
                    var actualPointNew = _currentControl.PointToScreen(virtuaPoinNew);
                    _instanceObject.Top = actualPointNew.Y;
                }
                else
                {
                    _instanceObject.Top = Actualpoint.Y;
                }
                
                _instanceObject.Show();
            }
        }

        public static bool GetTouchScreenKeyboard(DependencyObject obj)
        {
            return (bool)obj.GetValue(TouchScreenKeyboardProperty);
        }

        public static void SetTouchScreenKeyboard(DependencyObject obj, bool value)
        {
            obj.SetValue(TouchScreenKeyboardProperty, value);
        }

        public static readonly DependencyProperty TouchScreenKeyboardProperty =
            DependencyProperty.RegisterAttached("TouchScreenKeyboard", typeof(bool), typeof(TouchScreenKeyboard), new UIPropertyMetadata(default(bool), TouchScreenKeyboardPropertyChanged));



        static void TouchScreenKeyboardPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement host = sender as FrameworkElement;
            if (host != null)
            {
                host.GotFocus += new RoutedEventHandler(OnGotFocus);
                host.LostFocus += new RoutedEventHandler(OnLostFocus);
            }

        }



        static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            Control host = sender as Control;

            _previousTextBoxBackgroundBrush = host.Background;
            _previousTextBoxBorderBrush = host.BorderBrush;
            _previousTextBoxBorderThickness = host.BorderThickness;

            //host.Background = Brushes.Yellow;
            //host.BorderBrush = Brushes.Red;
            host.BorderThickness = new Thickness(4);


            _currentControl = host;

            if (_instanceObject == null)
            {
                FrameworkElement ct = host;
                while (true)
                {
                    if (ct is Window window)
                    {
                        window.LocationChanged += new EventHandler(TouchScreenKeyboard_LocationChanged);
                        window.Activated += new EventHandler(TouchScreenKeyboard_Activated);
                        window.Deactivated += new EventHandler(TouchScreenKeyboard_Deactivated);
                        break;
                    }
                    if (ct == null)
                    {
                        break;
                    }
                    ct = (FrameworkElement)ct.Parent;
                }

                _instanceObject = new TouchScreenKeyboard();
                _instanceObject.AllowsTransparency = true;
                _instanceObject.WindowStyle = WindowStyle.None;
                _instanceObject.ShowInTaskbar = false;
                _instanceObject.ShowInTaskbar = false;
                _instanceObject.Topmost = true;

                host.LayoutUpdated += new EventHandler(tb_LayoutUpdated);
            }



        }

        static void TouchScreenKeyboard_Deactivated(object sender, EventArgs e)
        {
            if (_instanceObject != null)
            {
                _instanceObject.Topmost = false;
            }
        }

        static void TouchScreenKeyboard_Activated(object sender, EventArgs e)
        {
            if (_instanceObject != null)
            {
                _instanceObject.Topmost = true;
            }
        }



        static void TouchScreenKeyboard_LocationChanged(object sender, EventArgs e)
        {
            syncchild();
        }

        static void tb_LayoutUpdated(object sender, EventArgs e)
        {
            syncchild();
        }



        static void OnLostFocus(object sender, RoutedEventArgs e)
        {

            Control host = sender as Control;
            host.Background = _previousTextBoxBackgroundBrush;
            host.BorderBrush = _previousTextBoxBorderBrush;
            host.BorderThickness = _previousTextBoxBorderThickness;

            if (_instanceObject != null)
            {
                _instanceObject.Close();
                _instanceObject = null;
            }

        }

        #endregion
    }
}

