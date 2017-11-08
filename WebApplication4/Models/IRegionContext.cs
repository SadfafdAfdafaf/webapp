using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace WebApplication4.Models
{
    
    public interface IRegionContext : IDisposable
    {
        DbSet<personalinf> personalinfs { get; }
        Task<int> SaveChangesAsync();
        void MarkAsModified(personalinf item);    
    }
}
