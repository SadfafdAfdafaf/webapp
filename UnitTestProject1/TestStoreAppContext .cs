using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;

namespace UnitTestProject1
{
    public class TestStoreAppContext : IWorkerContext
    {
        public TestStoreAppContext()
        {
            this.Workers = new TestProductDbSet();
        }

        public DbSet<Worker> Workers { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(Worker item) { }
        public void Dispose() { }
    }

    public class TestStoreAppContext2 : IWorkerContext
    {
        public TestStoreAppContext2()
        {
            this.Workers = new TestProductDbSet();
        }

        public DbSet<Worker> Workers { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();      
        }

        public void MarkAsModified(Worker item) { }
        public void Dispose() { }
    }
}
