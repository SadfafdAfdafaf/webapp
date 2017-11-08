using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication3.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;

namespace UnitTestProject1
{
    public class TestStoreAppContextGood : ICompaniesContext
    {
        public TestStoreAppContextGood()
        {
            this.companies = new TestProductDbSetComp();
        }

        public DbSet<companies> companies { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(companies item) { }
        public void Dispose() { }
    }

    public class TestStoreAppContextBad : ICompaniesContext
    {
        public TestStoreAppContextBad()
        {
            this.companies = new TestProductDbSetComp();
        }

        public DbSet<companies> companies { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();      
        }

        public void MarkAsModified(companies item) { }
        public void Dispose() { }
    }
}
