using System;
using System.Collections.Generic;
using System.Text;

namespace Anitro.Data_Structures.API_Classes
{
    public class Rating
    {
        public string type { get; set; }

        private string _value;
        public string value 
        {
            get { return _value; }
            set { _value = value; }
        }

        private double _valueAsDouble;
        public double valueAsDouble 
        {
            get { return _valueAsDouble; }
            set 
            {
                _value = value.ToString();
                _valueAsDouble = value;
            }
        }
    }
}
