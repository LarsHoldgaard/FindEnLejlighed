﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindEnLejlighed.Services.Domain
{
    public class Apartment
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public ContactStatus ContactStatus { get; set; }
        public int SquareMeter { get; set; }
        public int Zipcode { get; set; }
        public string TakeOverDate { get; set; }
        public string SellerType { get; set; }
        public string  CityRegion { get; set; }

        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}
