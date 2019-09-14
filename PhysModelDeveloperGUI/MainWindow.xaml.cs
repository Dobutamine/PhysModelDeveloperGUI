using System.Windows;
using System.Windows.Media;

namespace PhysModelDeveloperGUI
{

    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel;


        public MainWindow()
        {
            InitializeComponent();

            // first find the properties of the current screen for adpative purposes
            double screen_x = SystemParameters.PrimaryScreenWidth;
            double screen_y = SystemParameters.PrimaryScreenHeight;
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            double dpi_scale = dpi.DpiScaleX;

            // instatiate the mainwindow viewmodel
            mainWindowViewModel = new MainWindowViewModel(screen_x, screen_y, dpi_scale);

            // set the datacontext
            DataContext = mainWindowViewModel;

            // pass the graphs to the mainviewmodel
            mainWindowViewModel.InitTrendGraph(graphTrends);
            mainWindowViewModel.InitBloodgasGraph(graphBloodgas);
            mainWindowViewModel.InitModelDiagram(graphDiagram);
            mainWindowViewModel.InitPatientMonitor(graphMonitor);
            mainWindowViewModel.InitPVLoop(graphLoop);

        }



    }
}
