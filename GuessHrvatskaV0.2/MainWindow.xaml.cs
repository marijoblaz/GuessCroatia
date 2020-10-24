using Company.PlaceUtils;
using Csv;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Path = System.IO.Path;
using Point = System.Windows.Point;

//VC
//First build 18.10.2020
//Refactoring 24.10.2020

namespace GuessHrvatskaV0._2
{
    public partial class MainWindow : Window
    {
        //File managment
        private readonly string csvPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DataSet\Croatia.csv");
        private readonly string photoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DataSet\Croatia.png");

        //Labeling the guessed places by placing circle on its positon
        //based on coordinates imported from CSV file
        private List<Point> guessedPlacesPointLoaction = new List<Point>();
        private List<Ellipse> guessedPlacesEllipses = new List<Ellipse>();

        private Thickness maxMinCords = new Thickness(0,0,0,0);

        //seeding random
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            //Init image
            CroatiaImage.Source = new BitmapImage(new Uri(photoPath));
            //Init listView
            listViewGuessedPlaces.ItemsSource = Places.guessedPlaces;
            //Init places
            initPlaces();

            checkWindowSpecs();
        }

        private void checkWindowSpecs()
        {
            //Fix scaling isssues
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiX = (int)dpiXProperty.GetValue(null, null);

            if (dpiX != 96)
            {
                MessageBox.Show("Note: Your windows scaling is not set to 100% this may result in wrong position of cities.","Error!");
            }
        }

        private void initPlaces()
        {
            //Get the records from csv and store them to List knownPlaces, load places
            using (var csv = new CsvLoader(csvPath, ',')) { Places.knownPlaces = csv.GetRecords<Place>(); }

            //Looking for max and min value in object list - forward to Thickness
            maxMinCords.Left = Places.knownPlaces.Min(r => r.Cord_X);
            maxMinCords.Right = Places.knownPlaces.Max(r => r.Cord_X);
            maxMinCords.Top = Places.knownPlaces.Max(r => r.Cord_Y);
            maxMinCords.Bottom = Places.knownPlaces.Min(r => r.Cord_Y);
        }

        private void userInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string userInputText = userInputTextBox.Text;
            if (e.Key == Key.Enter)
            {
                if (Places.ContainsName(Places.knownPlaces, userInputText) && !Places.ContainsName(Places.guessedPlaces, userInputText))
                {
                    //Add to list
                    Places.guessedPlaces.Add(Places.getPlace(Places.knownPlaces, userInputText));
                    ShowUserGueesedStats();
                    PlaceCircleOnMap(Places.getPlace(Places.knownPlaces, userInputText));
                }
                //Clear the textbox
                userInputTextBox.Text = string.Empty;
            }
        }
        private void ShowUserGueesedStats() => GueesdText.Text = "You named " + Places.guessedPlaces.Count() + " cities, with a total population of " + Places.guessedPlaces.Sum(r => r.Pupulation);

        private void listView_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as ListView).SelectedItem;
            if (item != null)
            {
                for (int i = 0; i < Places.guessedPlaces.Count; i++)
                {
                    guessedPlacesEllipses[i].Fill = new SolidColorBrush(Colors.Red);
                    if (Places.guessedPlaces[i].ID.ToString() == item.ToString())
                    {
                        guessedPlacesEllipses[i].Fill = new SolidColorBrush(Colors.Green);
                    }
                } 
            }
        }
        private void PlaceCircleOnMap(Place place)
        {
            //This method gets called everytime user adds a new place
            /*
             * Izracunati rubne tocke u podacima, pronaci max i min kordinata
             * da ih mozemo mapirati na prozor
             * 
             *         Y cord
             *     515 |
             *         |
             *         |
             *         |
             *         |
             *         |
             *     469 L________________ X cord 
             * 
             *         614         660
             *     
             *     Ovako su u csvu koordinate poslozene
             */

            //GetMaxPopulation
            int maxPopulation = Places.knownPlaces.Max(r => r.Pupulation);

            //Create new ellipse
            guessedPlacesEllipses.Add(new Ellipse());
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Fill = new SolidColorBrush(Colors.Red);
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Stroke = new SolidColorBrush(Colors.Black);
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].StrokeThickness = 1;
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Opacity = 0.4;

            //Set size
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Height = Places.Map(place.Pupulation,0,maxPopulation,10,80);
            guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Width = guessedPlacesEllipses[guessedPlacesEllipses.Count - 1].Height;

            //Add new location
            guessedPlacesPointLoaction.Add(new Point(Places.Map(place.Cord_X, maxMinCords.Left, maxMinCords.Right, 0, CroatiaImage.ActualWidth - 10), 
                                                     Places.Map(place.Cord_Y, maxMinCords.Top, maxMinCords.Bottom, 0, CroatiaImage.ActualHeight - 10)));

            //Set location with margins
            guessedPlacesEllipses[guessedPlacesEllipses.Count -1].Margin = getEllipsesCords(guessedPlacesEllipses.Count-1);

            //Add it to canvas
            canvas.Children.Add(guessedPlacesEllipses[guessedPlacesEllipses.Count - 1]);
            
        }

        private Thickness getEllipsesCords(int i)
        {
            return new Thickness((guessedPlacesPointLoaction[i].X + getImagePosition().X) - (guessedPlacesEllipses[i].Width)/2,
                                 (guessedPlacesPointLoaction[i].Y + getImagePosition().Y) - (guessedPlacesEllipses[i].Width)/2, 0, 0);
        }


        private Point getImagePosition()
        {
            var test = GetAbsolutePlacement(CroatiaImage, false);
            return new Point(test.X, test.Y);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            for (int i = 0; i < Places.guessedPlaces.Count; i++)
            {
                guessedPlacesPointLoaction[i] = new Point(Places.Map(Places.guessedPlaces[i].Cord_X, maxMinCords.Left, maxMinCords.Right, 0, CroatiaImage.ActualWidth - 10), 
                                                           Places.Map(Places.guessedPlaces[i].Cord_Y, maxMinCords.Top, maxMinCords.Bottom, 0, CroatiaImage.ActualHeight - 10));
                guessedPlacesEllipses[i].Margin = getEllipsesCords(i);
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear game
            userInputTextBox.IsEnabled = true;
            GueesdText.Text = string.Empty;

            Places.guessedPlaces.Clear();
            guessedPlacesPointLoaction.Clear();
            guessedPlacesEllipses.Clear();
            canvas.Children.Clear();
        }

        private void shareButton_Click(object sender, RoutedEventArgs e)
        {

            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;

            using (Bitmap bmp = new Bitmap((int)screenWidth,
                (int)screenHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    String filename = "ScreenCapture-" + DateTime.Now.ToString("ddMMyyyy-hhmmss") + ".png";
                    string imagePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename);
                    Opacity = .0;
                    g.CopyFromScreen((int)screenLeft, (int)screenTop, 0, 0, bmp.Size);
                    bmp.Save(imagePath);
                    Opacity = 1;
                }

            }

            MessageBox.Show("Screen captured!", "Share",new MessageBoxButton());
        }

        private void gitHubButton_Click(object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start("https://github.com/");

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            Places.guessedPlaces.Add(Places.knownPlaces[random.Next(0, Places.knownPlaces.Count)]);
            listViewGuessedPlaces.ItemsSource = Places.guessedPlaces;
            ShowUserGueesedStats();
            PlaceCircleOnMap(Places.guessedPlaces[Places.guessedPlaces.Count-1]);
        }

        private Rect GetAbsolutePlacement(FrameworkElement element, bool relativeToScreen = false)
        {
            var absolutePos = element.PointToScreen(new Point(0, 0));
            if (relativeToScreen)
            {
                return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
            }
            var posMW = Application.Current.MainWindow.PointToScreen(new Point(0, 0));
            absolutePos = new Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
        }

    }
}
