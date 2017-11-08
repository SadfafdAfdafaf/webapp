using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WebApplication3.Models
{
    public interface ICompaniesContext : IDisposable
    {
        DbSet<companies> companies { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(companies item);    
    }
}
