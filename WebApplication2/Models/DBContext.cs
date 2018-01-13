using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
    public class DBContext : DbContext, IWorkerContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public DBContext() : base("name=DBContext")
        {
        }

        public System.Data.Entity.DbSet<WebApplication2.Models.Worker> Workers { get; set; }

        public void MarkAsModified(Worker item)
        {
            Entry(item).State = EntityState.Modified;
        }
    
    }

    public class Fuck2Context : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public Fuck2Context()
            : base("name=Fuck2Context")
        {
        }

        public System.Data.Entity.DbSet<WebApplication2.Models.instatmes> instatmes { get; set; }

    }
}
