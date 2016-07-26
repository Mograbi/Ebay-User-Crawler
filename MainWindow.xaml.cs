using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EbayCrawlerWPF_2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Program myProgram = new Program();
        Thread pThread;
        Thread nThread;
        private bool validUser = false;
        public MainWindow()
        {
            InitializeComponent();
            App.positiveWriter = new ControlWriter(PositiveOut);
            App.negativeWriter = new ControlWriter(NegativeOut);
            pThread = new Thread(myProgram.searchProductIn);
            nThread = new Thread(myProgram.searchProductIn);
        }

        private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            UserInput.Foreground = Brushes.Black;
            myProgram.User = UserInput.Text;
            
        }

        private void ProductInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            myProgram.Product = ProductInput.Text;
        }

        private void StartSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!Program.CheckForInternetConnection())
            {
                MessageBoxResult result = MessageBox.Show("There is no Internet Connection\nRetry?", 
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(ProductInput.Text) || !validUser)
            {
                return;
            }
            int pages = myProgram.getSumPages();
            ProgressBar.Maximum = pages;
            App.progressBar = new ProgressWrapper(ProgressBar, pages);
            pThread.Start(Program.positive);
            nThread.Start(Program.negative);
            StartSearch.Background = new SolidColorBrush(Colors.Green);
        }

        private void StopSearch_Click(object sender, RoutedEventArgs e)
        {
            if (pThread.IsAlive)
            {
                pThread.Suspend();
            }
            if (nThread.IsAlive)
            {
                nThread.Suspend();
            }
            Brush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDDDDDD"));
            StartSearch.Background = brush;
            //StartSearch.Background = new SolidColorBrush();
        }

        private void UserInput_TouchLeave(object sender, TouchEventArgs e)
        {
            
        }

        private void UserInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (myProgram.User == "" || !myProgram.checkUserID())
            {
                UserInput.Foreground = Brushes.Red;
                return;
            }
            validUser = true;
            UserInput.Foreground = Brushes.Green;
        }
    }
}
