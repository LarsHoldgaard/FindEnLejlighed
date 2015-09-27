using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindEnLejlighed.Services.Domain;

namespace FindEnLejlighed.Services.Database
{
    public class Context: DbContext
    {
        public Context():base("Context")
        {
        }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<SendMessage> SendMessages { get; set; }
        
    }
}
