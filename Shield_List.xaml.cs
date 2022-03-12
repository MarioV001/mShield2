using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace mShield2
{
    /// <summary>
    /// Interaction logic for Shield_List.xaml
    /// </summary>
    public partial class Shield_List : Window
    {
        private BackgroundWorker ProcessListWorker = new BackgroundWorker();
        private BackgroundWorker SavedProcessListWorker = new BackgroundWorker();
        private bool ISWorkerBusy = false;
        public static bool IsTraceLookup=false;
        
        public Shield_List()
        {
            InitializeComponent();
        }

        
        private void Window_Initialized(object sender, EventArgs e)
        {
            TraceProcBoarder.Visibility = Visibility.Hidden;
            ControlButtons.Visibility = Visibility.Hidden;
            // Grid.SetRow(uc, ItemsGrid.RowDefinitions.Count - 1);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsTraceLookup == true)
            {
                ExPopUp exPopUp = new ExPopUp();
                var desktopWorkingArea = SystemParameters.WorkArea;
                this.Width = 920;
                this.Height = 550;
                this.Left = desktopWorkingArea.Right - this.Width;
                this.Top = desktopWorkingArea.Bottom - this.Height - exPopUp.Height;
                //
                LookUpTextBox.Text = MainWindow.TempProcessClass!.ID;
                AddProcesByID(Convert.ToInt32(MainWindow.TempProcessClass!.ID));
            }
            else
            {
                GetAllSAvedProcesses();
                SavedAppsCheck.IsChecked= true;
            }
        }
        private void GetAllSAvedProcesses(string SearchName ="")
        {
            if (ISWorkerBusy == true) return;
            SavedProcessListWorker.Dispose();
            TraceProcBoarder.Visibility = Visibility.Hidden;
            ControlButtons.Visibility = Visibility.Hidden;
            ItemsGrid.RowDefinitions.Clear();
            ItemsGrid.Children.Clear();
            IsTraceLookup = false;

            //start Work
            ISWorkerBusy = true;
            SavedProcessListWorker.WorkerReportsProgress = true;
            SavedProcessListWorker.DoWork -= SavedProcessListWorker_DoWork!;
            SavedProcessListWorker.DoWork += SavedProcessListWorker_DoWork!;
            SavedProcessListWorker.RunWorkerCompleted -= ProcessListWorker_RunWorkerCompleted!;
            SavedProcessListWorker.RunWorkerCompleted += ProcessListWorker_RunWorkerCompleted!;
            SavedProcessListWorker.ProgressChanged -= OnProgressChanged!;
            SavedProcessListWorker.ProgressChanged += OnProgressChanged!;
            SavedProcessListWorker.RunWorkerAsync(argument: SearchName);
        }
        private void SavedProcessListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string SearchName = (string)e.Argument!;   // the 'argument' parameter resurfaces here
            BackgroundWorker worker = (BackgroundWorker)sender;

            if (MainWindow.myDeserializedClass == null) return;

            int count = 0;

            foreach (var item in MainWindow.myDeserializedClass.StoredProcesess.ToArray())
            {
                if (SearchName == "" || SearchName != "" && item.Name.ToLower().Contains(SearchName.ToLower()))
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate ()
                    {
                    //Grid For rach Item
                        Grid DynamicGrid = new Grid();
                        DynamicGrid.Width = 330;
                        DynamicGrid.Height = 90;
                        DynamicGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        DynamicGrid.Background = new SolidColorBrush(Color.FromRgb(25, 90, 150));
                        DynamicGrid.Cursor = Cursors.Hand;
                        DynamicGrid.MouseDown += new MouseButtonEventHandler(ProcessMouseDown);
                        DynamicGrid.Name = "_" + count.ToString();


                    // Create Rows
                    RowDefinition gridRow1 = new RowDefinition(); gridRow1.Height = new GridLength(38);//Name
                    RowDefinition gridRow2 = new RowDefinition(); gridRow2.Height = new GridLength(20);//If is blocked
                    RowDefinition gridRow3 = new RowDefinition(); gridRow3.Height = new GridLength(25);//Description
                    DynamicGrid.RowDefinitions.Add(gridRow1);
                        DynamicGrid.RowDefinitions.Add(gridRow2);
                        DynamicGrid.RowDefinitions.Add(gridRow3);

                    //label For Process [NAME]
                    var newNamelabel = new Label();
                        newNamelabel.Height = 38;
                        newNamelabel.FontSize = 20;
                        newNamelabel.FontWeight = FontWeights.Bold;
                        newNamelabel.Content = item.Name;
                        newNamelabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newNamelabel, 0);
                        DynamicGrid.Children.Add(newNamelabel);

                    //Labe For Proc ID
                    var newIDlabel = new Label();
                        newIDlabel.Height = 35;
                        newIDlabel.FontSize = 10;
                        newIDlabel.FontWeight = FontWeights.Bold;
                        newIDlabel.Content = "(" + count + ")";
                        newIDlabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newNamelabel, 0);
                        newIDlabel.HorizontalAlignment = HorizontalAlignment.Right;
                        DynamicGrid.Children.Add(newIDlabel);

                    //label For Process [IS Blcoked]
                    var newIsBlockedlabel = new Label();
                        newIsBlockedlabel.Height = 24;
                        newIsBlockedlabel.FontSize = 11;
                        newIsBlockedlabel.Content = item.AllowedToRUn;
                        if (item.AllowedToRUn == "True") newIsBlockedlabel.Foreground = new SolidColorBrush(MainWindow.GreenLabelColor);
                        else newIsBlockedlabel.Foreground = new SolidColorBrush(MainWindow.RedLabelColor);
                        Grid.SetRow(newIsBlockedlabel, 1);
                        DynamicGrid.Children.Add(newIsBlockedlabel);

                    //label For Process [DESCRIPTION]
                    var newDescriptionlabel = new Label();
                        newDescriptionlabel.Height = 30;
                        newDescriptionlabel.Content = item.Description;
                        newDescriptionlabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newDescriptionlabel, 2);
                        DynamicGrid.Children.Add(newDescriptionlabel);


                    //Add ALl To main Grid ROW
                    var myDefinition = new RowDefinition();
                        myDefinition.Height = new GridLength(90);
                        DynamicGrid.Margin = new Thickness(5);
                        Grid.SetRow(DynamicGrid, count);
                        ItemsGrid.RowDefinitions.Add(myDefinition);
                        ItemsGrid.Children.Add(DynamicGrid);

                        TotalProc.Content = "Total Proc: " + count;
                    });
                }
                worker.ReportProgress((count * 100) / MainWindow.myDeserializedClass.StoredProcesess.Count());

                count++;
            }


        }
        private void GetAllProcesses(string SearchName="")
        {
            if (ISWorkerBusy == true) return;
            ProcessListWorker.Dispose();
            TraceProcBoarder.Visibility = Visibility.Hidden;
            ControlButtons.Visibility = Visibility.Hidden;
            ItemsGrid.RowDefinitions.Clear();
            ItemsGrid.Children.Clear();
            IsTraceLookup = false;

            //start Work
            ISWorkerBusy = true;
            ProcessListWorker.WorkerReportsProgress = true;
            ProcessListWorker.DoWork -= ProcessListWorker_DoWork!;
            ProcessListWorker.DoWork += ProcessListWorker_DoWork!;
            ProcessListWorker.RunWorkerCompleted -= ProcessListWorker_RunWorkerCompleted!;
            ProcessListWorker.RunWorkerCompleted += ProcessListWorker_RunWorkerCompleted!;
            ProcessListWorker.ProgressChanged -= OnProgressChanged!;
            ProcessListWorker.ProgressChanged += OnProgressChanged!;
            ProcessListWorker.RunWorkerAsync(argument: SearchName);
        }
        private void ProcessListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            Process[] localAll = Process.GetProcesses();
            string SearchName = (string)e.Argument!;   // the 'argument' parameter resurfaces here

            int count = 0,subcount=0;

            foreach (Process Proc in localAll)
            {
                if (Proc.Id < 5)
                {
                    subcount++;
                    continue;
                }
                if (SearchName == "" || SearchName != "" && Proc.ProcessName.ToLower().Contains(SearchName.ToLower()))
                {
                    this.Dispatcher.Invoke(DispatcherPriority.Background, (Action)delegate ()
                    {
                        //Grid For rach Item
                        Grid DynamicGrid = new Grid();
                        DynamicGrid.Width = 330;
                        DynamicGrid.Height = 90;
                        DynamicGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
                        DynamicGrid.Background = new SolidColorBrush(Color.FromRgb(25, 90, 150));
                        DynamicGrid.Cursor = Cursors.Hand;
                        DynamicGrid.MouseDown += new MouseButtonEventHandler(ProcessMouseDown);
                        DynamicGrid.Name = "_" + Proc.Id.ToString();


                        // Create Rows
                        RowDefinition gridRow1 = new RowDefinition(); gridRow1.Height = new GridLength(38);//Name
                        RowDefinition gridRow2 = new RowDefinition(); gridRow2.Height = new GridLength(20);//If is blocked
                        RowDefinition gridRow3 = new RowDefinition(); gridRow3.Height = new GridLength(25);//Description
                        DynamicGrid.RowDefinitions.Add(gridRow1);
                        DynamicGrid.RowDefinitions.Add(gridRow2);
                        DynamicGrid.RowDefinitions.Add(gridRow3);

                        //label For Process [NAME]
                        var newNamelabel = new Label();
                        newNamelabel.Height = 38;
                        newNamelabel.FontSize = 20;
                        newNamelabel.FontWeight = FontWeights.Bold;
                        newNamelabel.Content = Proc.ProcessName;
                        newNamelabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newNamelabel, 0);
                        DynamicGrid.Children.Add(newNamelabel);

                        //Labe For Proc ID
                        var newIDlabel = new Label();
                        newIDlabel.Height = 35;
                        newIDlabel.FontSize = 10;
                        newIDlabel.FontWeight = FontWeights.Bold;
                        newIDlabel.Content = "(" + Proc.Id + ")";
                        newIDlabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newNamelabel, 0);
                        newIDlabel.HorizontalAlignment = HorizontalAlignment.Right;
                        DynamicGrid.Children.Add(newIDlabel);

                        //label For Process [IS Blcoked]
                        var newIsBlockedlabel = new Label();
                        newIsBlockedlabel.Height = 24;
                        newIsBlockedlabel.FontSize = 10;
                        newIsBlockedlabel.Content = "Unknown";
                        newIsBlockedlabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newIsBlockedlabel, 1);
                        DynamicGrid.Children.Add(newIsBlockedlabel);

                        //label For Process [DESCRIPTION]
                        var newDescriptionlabel = new Label();
                        newDescriptionlabel.Height = 30;
                        try
                        {
                            newDescriptionlabel.Content = Proc.MainModule!.FileVersionInfo.FileDescription;
                        }
                        catch (Exception ex)
                        {
                            newDescriptionlabel.Content = ex.Message;
                        }

                        newDescriptionlabel.Foreground = new SolidColorBrush(Colors.White);
                        Grid.SetRow(newDescriptionlabel, 2);
                        DynamicGrid.Children.Add(newDescriptionlabel);


                        //Add ALl To main Grid ROW
                        var myDefinition = new RowDefinition();
                        myDefinition.Height = new GridLength(90);
                        DynamicGrid.Margin = new Thickness(5);
                        Grid.SetRow(DynamicGrid, count);
                        ItemsGrid.RowDefinitions.Add(myDefinition);
                        ItemsGrid.Children.Add(DynamicGrid);

                        TotalProc.Content = "Total Proc: " + count;
                    });
                }
                worker.ReportProgress((count * 100) / (localAll.Count() - subcount));
                count++;
            }
            
            
        }
        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }
        private void ProcessListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ISWorkerBusy = false;
            ProgressBar.Value = 100;
            //update ui once worker complete his work
        }

        Grid OldGridControl=null!;
        private void ProcessMouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid GridControl = (Grid)sender;
            if(OldGridControl != null) OldGridControl.Background = new SolidColorBrush(Color.FromRgb(25, 90, 150));
            //SelectionColorSettings


            GridControl.Background = new SolidColorBrush(Color.FromRgb(22, 140, 120));
            TraceProcBoarder.Visibility = Visibility.Visible;
            ControlButtons.Visibility = Visibility.Visible;
            if (IsTraceLookup == true)
            {
                ProcNameLabel.Content = "Name: " + MainWindow.TempProcessClass!.Name;
                ProcAllowedLabel.Content = "Allowed : Pending";
                ProcRespondingLabel.Content = "Responding: " + MainWindow.TempProcessClass!.Responding;
                ProcDescLabel.Content = "Description: " + MainWindow.TempProcessClass!.Description;
                //Extra Tracing Stats
                StartPathTextBox.Text = MainWindow.TempProcessClass!.StartPath;
                OGFileNameTextBox.Text = MainWindow.TempProcessClass!.OGName;
                ArgumentsTextBox.Text = MainWindow.TempProcessClass!.CommandLineParams;
            }else if (e.ChangedButton == MouseButton.Left)
            {
                if (AllRunningChecker.IsChecked == true)
                {
                    Process SelProcess = Process.GetProcessById(Convert.ToInt32(GridControl.Name.Replace("_", "")));

                    if (SelProcess != null)
                    {
                        ProcNameLabel.Content = "Name: " + SelProcess.ProcessName;
                        ProcAllowedLabel.Content = "Allowed : UNK";
                        ProcRespondingLabel.Content = "Responding: " + SelProcess.Responding;
                        try
                        {
                            ProcDescLabel.Content = "Description: " + SelProcess.MainModule!.FileVersionInfo.FileDescription;
                            //Extra Tracing Stats
                            StartPathTextBox.Text = SelProcess.MainModule!.FileName;
                            OGFileNameTextBox.Text = SelProcess.MainModule!.FileVersionInfo.OriginalFilename;
                            ArgumentsTextBox.Text = GetCommandLine(SelProcess).Replace(StartPathTextBox.Text!, "").Replace(@"""", "");
                        }
                        catch (Exception ex)
                        {
                            ProcDescLabel.Content = "Description: " + ex.Message;
                            //Extra Tracing Stats
                            StartPathTextBox.Text = ex.Message;
                            OGFileNameTextBox.Text = ex.Message;
                            ArgumentsTextBox.Text = ex.Message;
                        }
                    }
                }
                else if (SavedAppsCheck.IsChecked == true)//Save items Item pressed (Load extra info)
                {
                    int SaveID = Convert.ToInt32(GridControl.Name.Replace("_", ""));
                    GridID.Name = "GridID_" + SaveID;
                    ProcNameLabel.Content = "Name: " + MainWindow.myDeserializedClass!.StoredProcesess[SaveID].Name;
                    if (MainWindow.myDeserializedClass!.StoredProcesess[SaveID].AllowedToRUn == "True") ProcAllowedLabel.Foreground = new SolidColorBrush(MainWindow.GreenLabelColor);
                    else ProcAllowedLabel.Foreground = new SolidColorBrush(MainWindow.RedLabelColor);
                    ProcAllowedLabel.Content = "Allowed : "+ MainWindow.myDeserializedClass!.StoredProcesess[SaveID].AllowedToRUn;
                    ProcDescLabel.Content = "Description: " + MainWindow.myDeserializedClass!.StoredProcesess[SaveID].Description;
                    //extra Info
                    StartPathTextBox.Text = MainWindow.myDeserializedClass!.StoredProcesess[SaveID].StartPath;
                    OGFileNameTextBox.Text = MainWindow.myDeserializedClass!.StoredProcesess[SaveID].OGName;
                    ArgumentsTextBox.Text = MainWindow.myDeserializedClass!.StoredProcesess[SaveID].CommandLineParams;
                } 
            }
            OldGridControl = GridControl;
        }

        public static string GetCommandLine(Process process)
        {
            string? cmdLine = null;
            using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}"))
            {
                using (var matchEnum = searcher.Get().GetEnumerator())
                {
                    if (matchEnum.MoveNext()) // Move to the 1st item.
                    {
                        cmdLine = matchEnum.Current["CommandLine"]?.ToString();
                    }
                }
            }
            if (cmdLine == null)
            {
               var dummy = process.MainModule; // Provoke exception.
            }
            return cmdLine!;
        }

        private void AddProcesByID(int ProcID)
        {
            ProcessListWorker.Dispose();
            TraceProcBoarder.Visibility = Visibility.Hidden;
            ControlButtons.Visibility = Visibility.Hidden;
            ItemsGrid.RowDefinitions.Clear();
            ItemsGrid.Children.Clear();

            //Process TraceProc = Process.GetProcessById(ProcID);

            //Grid For rach Item
            Grid DynamicGrid = new Grid();
            DynamicGrid.Width = 330;
            DynamicGrid.Height = 90;
            DynamicGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            DynamicGrid.Background = new SolidColorBrush(Color.FromRgb(25, 90, 150));
            DynamicGrid.Cursor = Cursors.Hand;
            DynamicGrid.MouseDown += new MouseButtonEventHandler(ProcessMouseDown);
            DynamicGrid.Name = "_" + ProcID;


            // Create Rows
            RowDefinition gridRow1 = new RowDefinition(); gridRow1.Height = new GridLength(38);//Name
            RowDefinition gridRow2 = new RowDefinition(); gridRow2.Height = new GridLength(20);//If is blocked
            RowDefinition gridRow3 = new RowDefinition(); gridRow3.Height = new GridLength(25);//Description
            DynamicGrid.RowDefinitions.Add(gridRow1);
            DynamicGrid.RowDefinitions.Add(gridRow2);
            DynamicGrid.RowDefinitions.Add(gridRow3);

            //label For Process [NAME]
            var newNamelabel = new Label();
            newNamelabel.Height = 38;
            newNamelabel.FontSize = 20;
            newNamelabel.FontWeight = FontWeights.Bold;
            newNamelabel.Content = MainWindow.TempProcessClass!.Name;
            newNamelabel.Foreground = new SolidColorBrush(Colors.White);
            Grid.SetRow(newNamelabel, 0);
            DynamicGrid.Children.Add(newNamelabel);

            //Labe For Proc ID
            var newIDlabel = new Label();
            newIDlabel.Height = 35;
            newIDlabel.FontSize = 10;
            newIDlabel.FontWeight = FontWeights.Bold;
            newIDlabel.Content = "(" + ProcID + ")";
            newIDlabel.Foreground = new SolidColorBrush(Colors.White);
            Grid.SetRow(newNamelabel, 0);
            newIDlabel.HorizontalAlignment = HorizontalAlignment.Right;
            DynamicGrid.Children.Add(newIDlabel);

            //label For Process [IS Blcoked]
            var newIsBlockedlabel = new Label();
            newIsBlockedlabel.Height = 24;
            newIsBlockedlabel.FontSize = 10;
            newIsBlockedlabel.Content = "Unknown";
            newIsBlockedlabel.Foreground = new SolidColorBrush(Colors.White);
            Grid.SetRow(newIsBlockedlabel, 1);
            DynamicGrid.Children.Add(newIsBlockedlabel);

            //label For Process [DESCRIPTION]
            var newDescriptionlabel = new Label();
            newDescriptionlabel.Height = 30;
            newDescriptionlabel.Content = MainWindow.TempProcessClass!.Description;

            newDescriptionlabel.Foreground = new SolidColorBrush(Colors.White);
            Grid.SetRow(newDescriptionlabel, 2);
            DynamicGrid.Children.Add(newDescriptionlabel);


            //Add ALl To main Grid ROW
            var myDefinition = new RowDefinition();
            myDefinition.Height = new GridLength(90);
            DynamicGrid.Margin = new Thickness(5);
            Grid.SetRow(DynamicGrid, 0);
            ItemsGrid.RowDefinitions.Add(myDefinition);
            ItemsGrid.Children.Add(DynamicGrid);
            //auto press the item
            ProcessMouseDown(DynamicGrid, null!);
            
            ProgressBar.Value = 100;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllRunningChecker.IsChecked == true)
            {
                GetAllProcesses(LookUpTextBox.Text);
            }
            else if (SavedAppsCheck.IsChecked == true)
            {
                GetAllSAvedProcesses(LookUpTextBox.Text);
            }
            else if (OnlineDBCheck.IsChecked == true)
            {
            }
            else TotalProc.Content = "No Search Type Selected!!";
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Image ?image = sender as Image;
                ContextMenu contextMenu = image!.ContextMenu;
                contextMenu.PlacementTarget = image;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private void SavedAppsCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (SavedAppsCheck.IsChecked == true)
            {
                AllRunningChecker.IsChecked = false;
                OnlineDBCheck.IsChecked = false;
            }
        }

        private void AllRunningChecker_Checked(object sender, RoutedEventArgs e)
        {
            if (AllRunningChecker.IsChecked == true)
            {
                SavedAppsCheck.IsChecked = false;
                OnlineDBCheck.IsChecked = false;
            }
        }

        private void OnlineDBCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (OnlineDBCheck.IsChecked == true)
            {
                SavedAppsCheck.IsChecked = false;
                AllRunningChecker.IsChecked = false;
            }
        }

        

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            IsTraceLookup= false;
        }

        private void AcceptBTN_Click(object sender, RoutedEventArgs e)
        {
            int ID = Convert.ToInt32(GridID.Name.Replace("GridID_", ""));
            MainWindow.myDeserializedClass!.StoredProcesess[ID].AllowedToRUn = "True";
            ProcAllowedLabel.Content = "Allowed : " + MainWindow.myDeserializedClass!.StoredProcesess[ID].AllowedToRUn;
        }

        private void DeclineBTN_Click(object sender, RoutedEventArgs e)
        {
            int ID = Convert.ToInt32(GridID.Name.Replace("GridID_", ""));
            MainWindow.myDeserializedClass!.StoredProcesess[ID].AllowedToRUn = "False";
            ProcAllowedLabel.Content = "Allowed : " + MainWindow.myDeserializedClass!.StoredProcesess[ID].AllowedToRUn;
        }

        private void LookUpTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SearchButton_Click(this, null!);
        }
    }
    
}
