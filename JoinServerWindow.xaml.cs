using System;
using System.Windows;

namespace DiceRoller
{
    /// <summary>
    /// Logique d'interaction pour JoinServerWindow.xaml
    /// </summary>
    public partial class JoinServerWindow : Window
    {
        public JoinServerWindow()
        {
            InitializeComponent();
        }

        private void JoinServer_Click(object sender, RoutedEventArgs e)
        {
            string ip = IpAddressTextBox.Text;
            int port = int.Parse(PortTextBox.Text);
            string username = UsernameTextBox.Text;
            ((App)Application.Current).MainWindow = new MainWindow();
            ((MainWindow)((App)Application.Current).MainWindow).ConnectToServer(ip, port, username);
            Close();
            ((App)Application.Current).MainWindow.Show();
        }
    }
}
