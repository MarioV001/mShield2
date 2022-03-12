using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace mShield2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Color SelectColor = Color.FromRgb(208, 127, 40);//Orange Color For Selection
        private static readonly Color DefaultColor = Color.FromRgb(45, 57,78);//Default Color For Selection
        public static readonly Color GreenLabelColor = Color.FromRgb(15, 155, 75);//Default Color For Green Label
        public static readonly Color RedLabelColor = Color.FromRgb(228, 25, 25);//Default Color For Green Label
        private List<string> ErroMSGGroup = new List<string>();

        DispatcherTimer mShieldTimer = new DispatcherTimer();
        public static AppsRootJ? myDeserializedClass;
        public static TempProcesess? TempProcessClass = new TempProcesess();//declaring the Temp variables static so we can use them in all forms


        /// <summary>
        /// End of global variables
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainShiledWindow_Initialized(object sender, EventArgs e)
        {

        }
        private void MainShiledWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SlideUpdateTImer.Focus();
            SlideUpdateTImer.Value = Properties.Settings.Default.mShieldUpdateTimer;
            //check for DB signing
            if (Properties.Settings.Default.DBSelected == "null")
            {
                DataSelectionScreen OPenFrm = new DataSelectionScreen();
                OPenFrm.ShowDialog();
            }
            //Check Admin
            if (IsAdministrator() == true)
            {
                IsAdminLabel.Content = "True";
                IsAdminLabel.Foreground = new SolidColorBrush(GreenLabelColor);
                
            }
            else
            {
                IsAdminLabel.Content = "False";
                IsAdminLabel.Foreground = new SolidColorBrush(RedLabelColor);
            }
            //Connect to DB
            AddErrorMSG("Warning: Could Not Connect To ExDevelopers Server!");
            //load Json
            LoadAppsDatabaseFile();
        }

        public void LoadAppsDatabaseFile()
        {
            if (File.Exists(Properties.Settings.Default.DBSelected) == true) myDeserializedClass = JsonSerializer.Deserialize<AppsRootJ>(File.ReadAllText(Properties.Settings.Default.DBSelected));
            else AddErrorMSG("Cound Not Find Database FIle: " + Properties.Settings.Default.DBSelected);
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider CurentSLider = (Slider)sender;
            if(CurentSLider.IsMouseDirectlyOver || CurentSLider.IsMouseOver || CurentSLider.IsFocused)
            {
                UpdateTimeLabel.Content = "mShiled Update: " + e.NewValue + "ms";
                Properties.Settings.Default.mShieldUpdateTimer = e.NewValue;
                mShieldTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
            }
            
        }
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            Border myBoarder = (Border)sender;
            myBoarder.BorderBrush = new SolidColorBrush(SelectColor);
        }

        private void StartTraceBTN_MouseLeave(object sender, MouseEventArgs e)
        {
            Border myBoarder = (Border)sender;
            myBoarder.BorderBrush = new SolidColorBrush(DefaultColor);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Application.Current.Windows[Login_Screen.GetWindowOpenID("LoginScreen")].Show();
                this.Hide();
                
            }
        }

        private void MenuItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            AutoStartMainShield.IsChecked = Properties.Settings.Default.AutoStartMshieldMain;
        }

        private void MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoStartMshieldMain = AutoStartMainShield.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void AutoStartMainShield_Click(object sender, RoutedEventArgs e)
        {
            AutoStartMainShield.IsChecked = !AutoStartMainShield.IsChecked;
        }

        private void Border_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                Shield_List OpenFrm = new Shield_List();
                OpenFrm.Show();
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)//DB Selection COntext Menu
        {
            DataSelectionScreen OpenFrm = new DataSelectionScreen();
            OpenFrm.ShowDialog();
            LoadAppsDatabaseFile();
        }

        private void StartmShield()
        {
            mShieldTimer.Tick -= mShieldTimer_Tick!;
            mShieldTimer.Tick += mShieldTimer_Tick!;
            mShieldTimer.Interval = TimeSpan.FromMilliseconds(mShieldSlider.Value);
            mShieldTimer.Start();
            mShieldEnabledLabel.Foreground = new SolidColorBrush(GreenLabelColor);
            mShieldEnabledLabel.Content = "Enabled";
        }
        private void StopMShield()
        {
            mShieldTimer.Stop();
            mShieldEnabledLabel.Foreground = new SolidColorBrush(RedLabelColor);
            mShieldEnabledLabel.Content = "Disabled";
        }
        public static bool IsProcAccepted = false;
        private void mShieldTimer_Tick(object sender,EventArgs e)
        {

            IsProcAccepted = false;
           Process[] localAll = Process.GetProcesses();

            if (myDeserializedClass == null)
            {
                StopMShield();
                mShieldSlider.Value = 0;
                AddErrorMSG("Database file could now be loaded!");
                return;
            }
            
            foreach (Process Proc in localAll)
            {
                if (Proc.Id < 5)//prevents looking into System Processes
                {
                    continue;
                }
                bool IsFound = false;
                foreach (var item in myDeserializedClass.StoredProcesess.ToArray())
                {
                    if (Proc.ProcessName == item.Name)
                    {
                        if(item.AllowedToRUn=="False") KillProcess(Proc);
                        IsFound = true;
                        break;
                    }
                }
                if (IsFound == false && DetecotrSlider.Value == 1)//not found add new item to DB ?
                {
                    TempProcessClass!.Name = Proc.ProcessName;
                    TempProcessClass!.Responding = Proc.Responding.ToString();
                    TempProcessClass.ID = Proc.Id.ToString();

                    string ProcDescription = "",startPath="",CMDparams="",OGFilename="";
                    try
                    {
                        ProcDescription = Proc.MainModule!.FileVersionInfo.FileDescription!;
                        startPath = Proc.MainModule!.FileName!;
                        OGFilename = Proc.MainModule!.FileVersionInfo.OriginalFilename!;
                        CMDparams = Shield_List.GetCommandLine(Proc).Replace(startPath, "").Replace(@"""", "");
                    }
                    catch (Exception ex)
                    {
                        ProcDescription = ex.Message;
                        startPath = ex.Message;
                        OGFilename = ex.Message;
                        CMDparams = ex.Message;
                    }
                    TempProcessClass!.Description = ProcDescription;
                    TempProcessClass.StartPath = startPath;
                    TempProcessClass.OGName = OGFilename;
                    TempProcessClass.CommandLineParams = CMDparams;

                    ExPopUp NewWin = new ExPopUp();
                    NewWin.ShowDialog();

                    if (IsProcAccepted == false) KillProcess(Proc);
                    myDeserializedClass.StoredProcesess.Add(new StoredProcesess { Name = Proc.ProcessName,OGName= OGFilename, AllowedToRUn = IsProcAccepted.ToString(), 
                        Description = ProcDescription ,StartPath= startPath,CommandLineParams= CMDparams
                    });
                    //clear the TEMP
                    TempProcessClass.ID = "";
                    TempProcessClass.Name = "";
                    TempProcessClass.OGName = "";
                    TempProcessClass.AllowedToRUn = "";
                    TempProcessClass.Description = "";
                    TempProcessClass.StartPath = "";
                    TempProcessClass.CommandLineParams = "";
                }
                
            }
            
        }

        public void KillProcess(Process ProcessToKill,bool LocalKill=true)
        {
            
            if (LocalKill == true)
            {
                try
                {
                    ProcessToKill.Kill();
                    IsKilledSucsess.Content = "True";
                    IsKilledSucsess.Foreground = new SolidColorBrush(GreenLabelColor);
                }
                catch 
                {
                    IsKilledSucsess.Content = "False";
                    IsKilledSucsess.Foreground= new SolidColorBrush(RedLabelColor);
                }
            }
            else {
                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo()
                {
                    FileName = "CMD.exe",
                    Arguments ="/C taskkill /im "+ ProcessToKill.ProcessName + " /f",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Verb = "runas"
                }; //specify paramaters and make it hidden
                proc.StartInfo = info;
                proc.Start();
            }
            if (ProcessToKill.ProcessName == LastProcessBlocked.Content.ToString()!.Replace("Last Process Blocked: ", "")) ProcKilledXtimes.Content = Convert.ToInt32(ProcKilledXtimes.Content) + 1;
            else ProcKilledXtimes.Content = "0";
            LastProcessBlocked.Content = "Last Process Blocked: " + ProcessToKill.ProcessName;
            ProcTimeKilled.Content = DateTime.Now.ToString("hh:mm:ss");
            //PopUp
            Login_Screen.IsWindowAllreadyOpen("BlockAPPPopUp", true);
            BlockAppPopUp NewWinBlock = new BlockAppPopUp();
            NewWinBlock.NameProcesslabel.Content = "Name: " + ProcessToKill.ProcessName;
            NewWinBlock.KilledProcLabel.Content = "Killed: " + IsKilledSucsess.Content;
            NewWinBlock.xTimesLabel.Content = "X-Times: " + ProcKilledXtimes.Content;
            NewWinBlock.Topmost = true;
            NewWinBlock.Show();
        }
        private void SaveDBsettings()
        {
            if (myDeserializedClass != null)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string JsonOut = JsonSerializer.Serialize(new { StoredProcesess = myDeserializedClass!.StoredProcesess }, options);
                File.WriteAllText(Properties.Settings.Default.DBSelected, JsonOut);
            }
            
        }
        private void MainShiledWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (mShieldTimer.IsEnabled == true)
            {
                mShieldTimer.Stop();
            }
            SaveDBsettings();
            //
            Properties.Settings.Default.Save();
            Application.Current.Shutdown();
        }

        private void mShieldSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mShieldSlider.Value == 1) StartmShield();//Enabled
            else StopMShield();//Disable
        }

        private void DetecotrSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(DetecotrSlider.Value==0)
            {
                DetectorEnabledLabel.Foreground= new SolidColorBrush(RedLabelColor);
                DetectorEnabledLabel.Content = "Disabled";
            }
            else
            {
                DetectorEnabledLabel.Foreground = new SolidColorBrush(GreenLabelColor);
                DetectorEnabledLabel.Content = "Enabled";
            }
        }

        private void ResetBlockedStatus_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton== MouseButtonState.Pressed)
            {
                LastProcessBlocked.Content = "Last Process Blocked:";
                ProcTimeKilled.Content = "00:00:00";
                ProcKilledXtimes.Content = "0";
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveDBsettings();
        }
        public  void AddErrorMSG(string Message)
        {
            ErroMSGGroup.Add(Message);
            ErrorMSGLabel.Content = Message;
            ErrorMSGLabel.Name = "ERR_" + (ErroMSGGroup.Count -1);
            ErrorMessageBox.Visibility = Visibility.Visible;
        }

        private void WarnImageBoxClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                int Index = Convert.ToInt32(ErrorMSGLabel.Name.Replace("ERR_", ""));
                if (Index > 0)
                {
                    ErroMSGGroup.RemoveAt(Index);

                    ErrorMSGLabel.Content = ErroMSGGroup[Index - 1];
                    ErrorMSGLabel.Name = "ERR_" + (Index - 1);
                }
                else
                {
                    ErroMSGGroup.Clear();
                    ErrorMessageBox.Visibility = Visibility.Hidden;
                }
            }
        }
    }
    public class StoredProcesess
    {
        public string Name { get; set; } = string.Empty;
        public string OGName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AllowedToRUn { get; set; } = string.Empty;
        public string StartPath { get; set; } = string.Empty;
        public string CommandLineParams { get; set; } = string.Empty;
    }
    public class AppsRootJ
    {
        public List<StoredProcesess> StoredProcesess { get; set; } = default!;
    }
    public class TempProcesess
    {
        public string ID = "";
        public string Name  = "";
        public string OGName  = "";
        public string Description= "";
        public string Responding = "";
        public string AllowedToRUn = "";
        public string StartPath = "";
        public string CommandLineParams = "";
    }
}
