using System;
using System.Windows;

namespace DiceRoller
{
    /// <summary>
    /// Logique d'interaction pour CreateServerWindow.xaml
    /// </summary>
    public partial class CreateServerWindow : Window
    {
        public CreateServerWindow()
        {
            InitializeComponent();
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpAddressTextBox.Text;
            int port = int.Parse(PortTextBox.Text);
            ((App)Application.Current).MainWindow = new MainWindow();
            ((MainWindow)((App)Application.Current).MainWindow).StartServer(ip, port);
            Close();
            ((App)Application.Current).MainWindow.Show();
        }
    }
}
