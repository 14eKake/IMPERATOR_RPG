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
using System.IO;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;
using System.Windows.Markup;
namespace DiceRoller
{
    /// <summary>
    /// Logique d'interaction pour CharacterSheet.xaml
    /// </summary>
    public partial class CharacterSheet : Window
    {
        private MainWindow mainWindow;

        public CharacterSheet(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
        }

        private void SaveCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            // Vérifie si tous les champs nécéssaire sont remlis 
            if (!ValidateFields())
            {
                return;
            }

            // Création d'un objet pour stocker les données de la feuille de personnage
            CharacterData characterData = new CharacterData
            {
                //Détail Personnage
                Nom = nom.Text,  // Remplacez `txtNom` par le nom réel de votre TextBox
                Origine = origine.Text,
                Age = age.Text,
                Taille = taille.Text,
                Poids = poids.Text,
                Sexe = (homme.IsChecked ?? false) ? "Homme" : "Femme",

                Carriere1 = Carriere_1.Text,
                Carriere2 = Carriere_2.Text,
                Carriere3 = Carriere_3.Text,

                //Caractéristique Général
                #region compétence
                ///
                /// COMPETENCES
                ///
                // Charactéristique principal
                Corpus = GetSelectedComboBoxValue(corpus),
                Charisma = GetSelectedComboBoxValue(charisma),
                Sensus = GetSelectedComboBoxValue(sensus),
                Spiritus = GetSelectedComboBoxValue(spiritus),


                //CORPUS compétences
                ArmesContondantes = GetSelectedComboBoxValue(Armes_Contondandes),
                ArmesPerforantes = GetSelectedComboBoxValue(Armes_Percantes),
                ArmesTranchantes = GetSelectedComboBoxValue(Armes_tranchantes),
                ArmeJetArc = GetSelectedComboBoxValue(Arme_Jet_Arc),
                ArmeJetFrondes = GetSelectedComboBoxValue(Arme_Jet_Frondes),
                ArmeJet = GetSelectedComboBoxValue(Arme_Jet),
                Athletisme = GetSelectedComboBoxValue(Athletisme),
                Bouclier = GetSelectedComboBoxValue(Bouclier),
                Attelage = GetSelectedComboBoxValue(Attelage),
                Endurance = GetSelectedComboBoxValue(Endurance),
                Equitation = GetSelectedComboBoxValue(Equitation),
                Escalade = GetSelectedComboBoxValue(Escalade),
                Esquive = GetSelectedComboBoxValue(Esquive),
                Natation = GetSelectedComboBoxValue(Natation),
                Pugilat = GetSelectedComboBoxValue(Pugilat),
                Souplesse = GetSelectedComboBoxValue(Souplesse),
                VolALaTire = GetSelectedComboBoxValue(Vol_a_la_tire),

                // CHARISMA compétences
                Commandement = GetSelectedComboBoxValue(Commandement),
                ComedieDance = GetSelectedComboBoxValue(Comedie_dance),
                Dressage = GetSelectedComboBoxValue(Dressage),
                Marchandage = GetSelectedComboBoxValue(Marchandage),
                Persuasion = GetSelectedComboBoxValue(Persuasion),
                Politique = GetSelectedComboBoxValue(Politique),
                Psychologie = GetSelectedComboBoxValue(Psychologie),
                Reseau = GetSelectedComboBoxValue(Reseau),
                Seduction = GetSelectedComboBoxValue(Seduction),
                UsCoutume = GetSelectedComboBoxValue(Us_Coutume),

                // SENSUS compétences
                Bibliotheque = GetSelectedComboBoxValue(Bibliotheque),
                Discretion = GetSelectedComboBoxValue(Discretion),
                Divination = GetSelectedComboBoxValue(Divination),
                Estimation = GetSelectedComboBoxValue(Estimation),
                Fouille = GetSelectedComboBoxValue(Fouille),
                Orientation = GetSelectedComboBoxValue(Orientation),
                Pieges = GetSelectedComboBoxValue(Pieges),
                Pistage = GetSelectedComboBoxValue(Pistage),
                Survie = GetSelectedComboBoxValue(Survie),
                Vigilance = GetSelectedComboBoxValue(Vigilance),

                // SPIRITUS compétences
                Agriculture = GetSelectedComboBoxValue(Agriculture),
                Alchimie = GetSelectedComboBoxValue(Alchimie),
                Artillerie = GetSelectedComboBoxValue(Artillerie),
                Artisanat = GetSelectedComboBoxValue(Artisanat),
                Ecriture = GetSelectedComboBoxValue(Ecriture),
                GestionPatrimoine = GetSelectedComboBoxValue(Gestion_Patrimoine),
                Herboristerie = GetSelectedComboBoxValue(Herboristerie),
                Histoire = GetSelectedComboBoxValue(Histoire),
                Ingenierie = GetSelectedComboBoxValue(Ingenierie),
                Language = GetSelectedComboBoxValue(Language),
                Medecine = GetSelectedComboBoxValue(Medecine),
                Mythologie = GetSelectedComboBoxValue(Mythologie),
                Navigation = GetSelectedComboBoxValue(Navigation),
                Occultisme = GetSelectedComboBoxValue(Occultisme),
                Resistance = GetSelectedComboBoxValue(Resistance),
                Strategie = GetSelectedComboBoxValue(Strategie),
                Theologie = GetSelectedComboBoxValue(Theologie),
                Zoologie = GetSelectedComboBoxValue(Zoologie)

                #endregion compétence
                //Inventaire

                // Ajoutez d'autres propriétés ici
            };
            // Extraire le texte des RichTextBox
            characterData.EquipementsRessourcesText = new TextRange(equipementsRessources.Document.ContentStart, equipementsRessources.Document.ContentEnd).Text;
            characterData.AvantagesDefautsText = new TextRange(avantagesDefauts.Document.ContentStart, avantagesDefauts.Document.ContentEnd).Text;

            // Sérialiser l'objet en JSON
            string json = JsonConvert.SerializeObject(characterData, Formatting.Indented);

            // Demander à l'utilisateur où sauvegarder le fichier
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = "json",
                FileName = "CharacterSheet"
            };

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                if (File.Exists(saveFileDialog.FileName) &&
                    MessageBox.Show("Le fichier existe déjà. Voulez-vous l'écraser?", "Attention", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    return;
                }

                // Écrire le JSON dans le fichier
                File.WriteAllText(saveFileDialog.FileName, json);
            }


        }

        // Permet de récupérer la valeur de mes comboBOX
        private int GetSelectedComboBoxValue(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return int.TryParse(selectedItem.Content.ToString(), out int value) ? value : 0;
            }
            return 0; // Valeur par défaut si rien n'est sélectionné ou en cas d'erreur
        }

        private void LoadCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = "json"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filePath = openFileDialog.FileName;
                string jsonData = File.ReadAllText(filePath);
                CharacterData characterData = JsonConvert.DeserializeObject<CharacterData>(jsonData);

                // Mettre à jour l'interface utilisateur
                nom.Text = characterData.Nom;
                origine.Text = characterData.Origine;
                age.Text = characterData.Age;
                taille.Text = characterData.Taille;
                poids.Text = characterData.Poids;
                homme.IsChecked = characterData.Sexe == "Homme";
                femme.IsChecked = characterData.Sexe == "Femme";

                Carriere_1.Text = characterData.Carriere1;
                Carriere_2.Text = characterData.Carriere2;
                Carriere_3.Text = characterData.Carriere3;

                SetSelectedComboBoxValue(corpus, characterData.Corpus);
                SetSelectedComboBoxValue(charisma, characterData.Charisma);
                SetSelectedComboBoxValue(sensus, characterData.Sensus);
                SetSelectedComboBoxValue(spiritus, characterData.Spiritus);

                // Mettre à jour les compétences
                SetSelectedComboBoxValue(Armes_Contondandes, characterData.ArmesContondantes);
                SetSelectedComboBoxValue(Armes_Percantes, characterData.ArmesPerforantes);
                SetSelectedComboBoxValue(Armes_tranchantes, characterData.ArmesTranchantes);
                SetSelectedComboBoxValue(Arme_Jet_Arc, characterData.ArmeJetArc);
                SetSelectedComboBoxValue(Arme_Jet_Frondes, characterData.ArmeJetFrondes);
                SetSelectedComboBoxValue(Arme_Jet, characterData.ArmeJet);
                SetSelectedComboBoxValue(Athletisme, characterData.Athletisme);
                SetSelectedComboBoxValue(Bouclier, characterData.Bouclier);
                SetSelectedComboBoxValue(Attelage, characterData.Attelage);
                SetSelectedComboBoxValue(Endurance, characterData.Endurance);
                SetSelectedComboBoxValue(Equitation, characterData.Equitation);
                SetSelectedComboBoxValue(Escalade, characterData.Escalade);
                SetSelectedComboBoxValue(Esquive, characterData.Esquive);
                SetSelectedComboBoxValue(Natation, characterData.Natation);
                SetSelectedComboBoxValue(Pugilat, characterData.Pugilat);
                SetSelectedComboBoxValue(Souplesse, characterData.Souplesse);
                SetSelectedComboBoxValue(Vol_a_la_tire, characterData.VolALaTire);

                // CHARISMA compétences
                SetSelectedComboBoxValue(Commandement, characterData.Commandement);
                SetSelectedComboBoxValue(Comedie_dance, characterData.ComedieDance);
                SetSelectedComboBoxValue(Dressage, characterData.Dressage);
                SetSelectedComboBoxValue(Marchandage, characterData.Marchandage);
                SetSelectedComboBoxValue(Persuasion, characterData.Persuasion);
                SetSelectedComboBoxValue(Politique, characterData.Politique);
                SetSelectedComboBoxValue(Psychologie, characterData.Psychologie);
                SetSelectedComboBoxValue(Reseau, characterData.Reseau);
                SetSelectedComboBoxValue(Seduction, characterData.Seduction);
                SetSelectedComboBoxValue(Us_Coutume, characterData.UsCoutume);

                // SENSUS compétences
                SetSelectedComboBoxValue(Bibliotheque, characterData.Bibliotheque);
                SetSelectedComboBoxValue(Discretion, characterData.Discretion);
                SetSelectedComboBoxValue(Divination, characterData.Divination);
                SetSelectedComboBoxValue(Estimation, characterData.Estimation);
                SetSelectedComboBoxValue(Fouille, characterData.Fouille);
                SetSelectedComboBoxValue(Orientation, characterData.Orientation);
                SetSelectedComboBoxValue(Pieges, characterData.Pieges);
                SetSelectedComboBoxValue(Pistage, characterData.Pistage);
                SetSelectedComboBoxValue(Survie, characterData.Survie);
                SetSelectedComboBoxValue(Vigilance, characterData.Vigilance);

                // SPIRITUS compétences
                SetSelectedComboBoxValue(Agriculture, characterData.Agriculture);
                SetSelectedComboBoxValue(Alchimie, characterData.Alchimie);
                SetSelectedComboBoxValue(Artillerie, characterData.Artillerie);
                SetSelectedComboBoxValue(Artisanat, characterData.Artisanat);
                SetSelectedComboBoxValue(Ecriture, characterData.Ecriture);
                SetSelectedComboBoxValue(Gestion_Patrimoine, characterData.GestionPatrimoine);
                SetSelectedComboBoxValue(Herboristerie, characterData.Herboristerie);
                SetSelectedComboBoxValue(Histoire, characterData.Histoire);
                SetSelectedComboBoxValue(Ingenierie, characterData.Ingenierie);
                SetSelectedComboBoxValue(Language, characterData.Language);
                SetSelectedComboBoxValue(Medecine, characterData.Medecine);
                SetSelectedComboBoxValue(Mythologie, characterData.Mythologie);
                SetSelectedComboBoxValue(Navigation, characterData.Navigation);
                SetSelectedComboBoxValue(Occultisme, characterData.Occultisme);
                SetSelectedComboBoxValue(Resistance, characterData.Resistance);
                SetSelectedComboBoxValue(Strategie, characterData.Strategie);
                SetSelectedComboBoxValue(Theologie, characterData.Theologie);
                SetSelectedComboBoxValue(Zoologie, characterData.Zoologie);

                LoadTextIntoRichTextBox(equipementsRessources, characterData.EquipementsRessourcesText);
                LoadTextIntoRichTextBox(avantagesDefauts, characterData.AvantagesDefautsText);

            }
        }

        private void LoadTextIntoRichTextBox(RichTextBox richTextBox, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                richTextBox.Document.Blocks.Clear();
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
        }

        private void SetSelectedComboBoxValue(ComboBox comboBox, int value)
        {
            comboBox.SelectedIndex = value; // Assumant que l'index correspond à la valeur
        }
        private void RollDiceForSkill_Click(object sender, RoutedEventArgs e)
        {
            Button diceButton = sender as Button;
            StackPanel parentPanel = diceButton.Parent as StackPanel;
            ComboBox skillLevelBox = parentPanel.Children.OfType<ComboBox>().FirstOrDefault();

            if (skillLevelBox != null && skillLevelBox.SelectedItem is ComboBoxItem selectedItem)
            {
                int skillLevel = int.Parse(selectedItem.Content.ToString());
                int characterAttribute = GetCharacterAttribute(parentPanel);

                // Passer l'instance correcte de mainWindow et mainWindow.Server à DiceRollSetupWindow
                DiceRollSetupWindow setupWindow = new DiceRollSetupWindow(skillLevel, characterAttribute, mainWindow, mainWindow.Server);
                setupWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un niveau de compétence.");
            }
        }


        private int GetCharacterAttribute(DependencyObject element)
        {
            // Remonter de deux niveaux dans la hiérarchie des StackPanel
            StackPanel grandParentPanel = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(element)) as StackPanel;

            if (grandParentPanel != null)
            {
                // Identifier l'attribut principal basé sur le StackPanel grand-parent
                if (grandParentPanel.Name.Contains("CorpusSkills"))
                {
                    return GetSelectedComboBoxValue(corpus);
                }
                if (grandParentPanel.Name.Contains("CharismaSkills"))
                {
                    return GetSelectedComboBoxValue(charisma);
                }
                if (grandParentPanel.Name.Contains("SensusSkills"))
                {
                    return GetSelectedComboBoxValue(sensus);
                }
                if (grandParentPanel.Name.Contains("SpiritusSkills"))
                {
                    return GetSelectedComboBoxValue(spiritus);
                }
            }
            return 0; // Valeur par défaut si non trouvé
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            // Code pour ouvrir la fenêtre HomeWindow
            HomeWindow homeWindow = new HomeWindow();
            homeWindow.Show();
        }


        private bool ValidateFields()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(nom.Text))
            {
                errors.AppendLine("Le champ 'Nom' est obligatoire.");
            }
            if (string.IsNullOrWhiteSpace(origine.Text))
            {
                errors.AppendLine("Le champ 'Origine' est obligatoire.");
            }
            if (!int.TryParse(age.Text, out _))
            {
                errors.AppendLine("Le champ 'Âge' doit être un nombre valide.");
            }
            if (!float.TryParse(taille.Text, out _))
            {
                errors.AppendLine("Le champ 'Taille' doit être un nombre valide.");
            }
            if (!int.TryParse(poids.Text, out _))
            {
                errors.AppendLine("Le champ 'Poids' doit être un nombre valide.");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Erreur de Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }


    }
    public class CharacterData
    {
        public string Nom { get; set; }
        public string Origine { get; set; }
        public string Age { get; set; }
        public string Taille { get; set; }
        public string Poids { get; set; }
        public string Sexe { get; set; }
        public string Carriere1 { get; set; }
        public string Carriere2 { get; set; }
        public string Carriere3 { get; set; }
        public string EquipementsRessourcesText { get; set; }
        public string AvantagesDefautsText { get; set; }

        public int Corpus { get; set; }
        public int Charisma { get; set; }
        public int Spiritus { get; set; }
        public int Sensus { get; set; }


        // CORPUS compétences
        public int ArmesContondantes { get; set; }
        public int ArmesPerforantes { get; set; }
        public int ArmesTranchantes { get; set; }
        public int ArmeJetArc { get; set; }
        public int ArmeJetFrondes { get; set; }
        public int ArmeJet { get; set; }
        public int Athletisme { get; set; }
        public int Bouclier { get; set; }
        public int Attelage { get; set; }
        public int Endurance { get; set; }
        public int Equitation { get; set; }
        public int Escalade { get; set; }
        public int Esquive { get; set; }
        public int Natation { get; set; }
        public int Pugilat { get; set; }
        public int Souplesse { get; set; }
        public int VolALaTire { get; set; }

        // CHARISMA compétences
        public int Commandement { get; set; }
        public int ComedieDance { get; set; }
        public int Dressage { get; set; }
        public int Marchandage { get; set; }
        public int Persuasion { get; set; }
        public int Politique { get; set; }
        public int Psychologie { get; set; }
        public int Reseau { get; set; }
        public int Seduction { get; set; }
        public int UsCoutume { get; set; }

        // SENSUS compétences
        public int Bibliotheque { get; set; }
        public int Discretion { get; set; }
        public int Divination { get; set; }
        public int Estimation { get; set; }
        public int Fouille { get; set; }
        public int Orientation { get; set; }
        public int Pieges { get; set; }
        public int Pistage { get; set; }
        public int Survie { get; set; }
        public int Vigilance { get; set; }

        // SPIRITUS compétences
        public int Agriculture { get; set; }
        public int Alchimie { get; set; }
        public int Artillerie { get; set; }
        public int Artisanat { get; set; }
        public int Ecriture { get; set; }
        public int GestionPatrimoine { get; set; }
        public int Herboristerie { get; set; }
        public int Histoire { get; set; }
        public int Ingenierie { get; set; }
        public int Language { get; set; }
        public int Medecine { get; set; }
        public int Mythologie { get; set; }
        public int Navigation { get; set; }
        public int Occultisme { get; set; }
        public int Resistance { get; set; }
        public int Strategie { get; set; }
        public int Theologie { get; set; }
        public int Zoologie { get; set; }
    }


}
