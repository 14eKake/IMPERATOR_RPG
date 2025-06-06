using System;
using System.Windows;

namespace DiceRoller
{
    /// <summary>
    /// Logique d'interaction pour HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        private void CreateServer_Click(object sender, RoutedEventArgs e)
        {
            new CreateServerWindow().ShowDialog();
        }

        private void JoinServer_Click(object sender, RoutedEventArgs e)
        {
            new JoinServerWindow().ShowDialog();
            Close();
        }
    }
}
