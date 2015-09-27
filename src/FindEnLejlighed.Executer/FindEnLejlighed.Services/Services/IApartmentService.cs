using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindEnLejlighed.Services.Domain;

namespace FindEnLejlighed.Services.Services
{
    public interface IApartmentService
    {
        List<Apartment> GetApartments();
        void Send(Apartment apartment);
    }
}
