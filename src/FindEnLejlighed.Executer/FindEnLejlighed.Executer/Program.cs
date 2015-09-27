using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindEnLejlighed.Services.Domain;
using FindEnLejlighed.Services.Services;

namespace FindEnLejlighed.Executer
{
    class Program
    {
        

        static void Main(string[] args)
        {
           IApartmentService service = new DbaApartmentService();

            var apartment = new Apartment()
            {
                DbaId = 1018746576,
                Link = "http://www.dba.dk/4840-villa-5-vaer/id-1018746576/",
                Postcode = "2100"
            };

            service.Send(apartment);

            //  var apartments = service.GetApartments();
        }
    }
}
