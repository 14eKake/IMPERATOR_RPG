using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
