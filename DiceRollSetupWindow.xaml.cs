using Newtonsoft.Json;
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
    /// Logique d'interaction pour DiceRollSetupWindow.xaml
    /// </summary>
    /// 
    public enum Difficulty
    {
        TrèsFacile, Facile, Normal, Difficile, TrèsDifficile, Héroïque
    }
    public partial class DiceRollSetupWindow : Window
    {
        private int skillLevel;
        private int characterAttribute;
        private MainWindow mainWindow;
        private Server server;

        public DiceRollSetupWindow(int skillLevel, int characterAttribute, MainWindow main, Server server)
        {
            InitializeComponent();
            this.skillLevel = skillLevel;
            this.characterAttribute = characterAttribute;
            this.mainWindow = main;
            this.server = server;  // Assurez-vous que l'instance du serveur est correctement assignée
        }

        private void ValidateDiceRoll_Click(object sender, RoutedEventArgs e)
        {
            Difficulty difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), ((ComboBoxItem)DifficultyBox.SelectedItem)?.Content.ToString());

            int bonusDice;
            if (!int.TryParse(BonusDiceBox.Text, out bonusDice))
            {
                MessageBox.Show("Invalid input for bonus dice. Please enter a valid number.");
                return;
            }

            int totalDice = skillLevel + characterAttribute + bonusDice;
            RollAndDisplayDice(totalDice, difficulty);
            this.Close();
        }

        public void RollAndDisplayDice(int totalDice, Difficulty difficulty)
        {
            Random random = new Random();
            StringBuilder resultBuilder = new StringBuilder();
            int successThreshold = SuccessThresholds.GetThreshold(skillLevel, difficulty);
            int successCount = 0;

            for (int i = 0; i < totalDice; i++)
            {
                int roll = random.Next(1, 11);  // Lancer un D10
                bool isSuccess = roll >= successThreshold;
                string color = isSuccess ? "green" : "red";

                if(isSuccess) { successCount++; }
                // Ajouter le jet avec la couleur appropriée
                resultBuilder.Append($"<run color=\"{color}\">{roll}</run> ");

                if (roll == 10)
                {
                    successCount++;
                }
                else if (roll == 1)
                {
                    successCount--;
                }
            }

            string coloredMessage = resultBuilder.ToString().Trim();
            string messageToSend = $"DICE_RESULT:{mainWindow.Username} a lancé les dés: {coloredMessage}|SuccessCount:{successCount}";
            mainWindow.SendMessageToServerOrChat(messageToSend);
        }

    }

    public class SuccessThresholds
    {
        private static readonly int[,] thresholds = new int[,] 
        {
        {6, 7, 8, 9, 10, 10},  // Niveau compétence 0
        {5, 6, 7, 8, 9, 10},   // Niveau compétence 1
        {4, 5, 6, 7, 8, 9},    // Niveau compétence 2
        {3, 4, 5, 6, 7, 8},    // Niveau compétence 3
        {2, 3, 4, 5, 6, 7},    // Niveau compétence 4
        {2, 2, 3, 4, 5, 6}     // Niveau compétence 5
        };

        public static int GetThreshold(int skillLevel, Difficulty difficulty)
        {
            return thresholds[skillLevel, (int)difficulty];
        }
    }
}
