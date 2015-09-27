using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindEnLejlighed.Services.Domain
{
    public class SendMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DbaId { get; set; }


        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }

        public string Name { get; set; }

        public DateTime DateSent { get; set; }
    }
}
