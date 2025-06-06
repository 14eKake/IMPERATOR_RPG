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
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                MessageBox.Show("Invalid port number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ((App)Application.Current).MainWindow = new MainWindow();
            ((MainWindow)((App)Application.Current).MainWindow).StartServer(ip, port);
            Close();
            ((App)Application.Current).MainWindow.Show();
        }
    }
}
