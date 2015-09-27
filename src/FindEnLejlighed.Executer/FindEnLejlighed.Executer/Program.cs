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

            var apartments = service.GetApartments();
        }
    }
}
