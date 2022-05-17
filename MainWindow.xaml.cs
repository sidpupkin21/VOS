using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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

namespace VOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new MainWindowViewModel();
            }
            Loaded += OnLoaded;
            SizeChanged += (sender, args) => Centralize();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Centralize();
        }

        public void Centralize()
        {
            double primaryScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double primaryScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            var desktop = System.Windows.SystemParameters.WorkArea;
            // Left = (primaryScreenWidth - ActualHeight); topright
            Left = desktop.Right - Width;
            Top = desktop.Bottom - Height;
            //Left = (primaryScreenHeight - ActualHeight);
            //Left = (primaryScreenWidth - ActualWidth - 100);
            //Top = 0;
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((MainWindowViewModel)DataContext).IsRunning = !((MainWindowViewModel)DataContext).IsRunning;
            ((MainWindowViewModel)DataContext).SetMessage("");
            ((MainWindowViewModel)DataContext).SpeakText("");
        }
    }
}
