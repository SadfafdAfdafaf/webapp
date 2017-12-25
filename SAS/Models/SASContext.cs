using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SAS.Models
{
    public class SASContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public SASContext() : base("name=SASContext")
        {
        }

        public System.Data.Entity.DbSet<SAS.Models.Token> Tokens { get; set; }

        public System.Data.Entity.DbSet<SAS.Models.Code> Codes { get; set; }

        public System.Data.Entity.DbSet<SAS.Models.User> Users { get; set; }
        public System.Data.Entity.DbSet<SAS.Models.Owners> Owners { get; set; }
    
    }
}
