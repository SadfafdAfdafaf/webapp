using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace SAS.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string UserPass { get; set; }

        public string UserRole { get; set; }

    }
}