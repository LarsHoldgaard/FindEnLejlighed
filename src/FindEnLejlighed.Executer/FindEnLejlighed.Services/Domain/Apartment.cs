using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindEnLejlighed.Services.Domain
{
    public class Apartment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DbaId { get; set; }

        public DateTime DateCreated { get; set; }

        public string Link { get; set; }
        public ContactStatus ContactStatus { get; set; }
        public string SquareMeter { get; set; }
        public int Zipcode { get; set; }
        public string TakeOverDate { get; set; }
        public string SellerType { get; set; }
        public string  CityRegion { get; set; }

        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Postcode { get; set; }
        public string minlivingspace { get; set; }
        public string RentalPeriod { get; set; }
        public string Minnumberofrooms { get; set; }

        public string Propertytype { get; set; }
        public string Boligkvm { get; set; }
        public string Deposit { get; set; }
        public string partlyfurnished { get; set; }
        public string Washingmachine { get; set; }
        public string basement { get; set; }


    }
}
