using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Company.PlaceUtils
{


    public static class Places
    {
        //List of places from CSV
        public static List<Place> knownPlaces;

        //List of places user guessed
        public static ObservableCollection<Place> guessedPlaces = new ObservableCollection<Place>();

        public static bool ContainsName(List<Place> placesInput, string name)
        {
            bool output = false;

            foreach (var i in placesInput)
            {
                if (i.Name.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return output;
        }

        public static bool ContainsName(ObservableCollection<Place> placesInput, string name)
        {
            bool output = false;

            foreach (var i in placesInput)
            {
                if (i.Name.ToLower() == name.ToLower())
                {
                    return true;
                }
            }
            return output;
        }



        public static Place getPlace(List<Place> placesInput, string name)
        {
            Place output = new Place();

            foreach (var i in placesInput)
            {
                if (i.Name.ToLower() == name.ToLower())
                {
                    return i;
                }
            }

            return output;
        }

        public static double Map(this double value, double fromSource, double toSource, double fromTarget, double toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

    }
}
