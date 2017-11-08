using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WebApplication2.Models
{
    public interface IWorkerContext : IDisposable
    {
        DbSet<Worker> Workers { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(Worker item);    
    }
}
