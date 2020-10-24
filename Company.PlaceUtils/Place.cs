using System;
using System.Collections.Generic;
using System.Text;

namespace Company.PlaceUtils
{
    public class Place
    {
        public double Cord_X { get; set; }
        public double Cord_Y { get; set; }
        public int ID { get; set; }
        public int Pupulation { get; set; }
        public string Name { get; set; }
        public string County { get; set; }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
