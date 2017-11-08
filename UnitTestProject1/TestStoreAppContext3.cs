using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication4.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data;

namespace UnitTestProject1
{
    public class TestStoreAppContextGood2 : IRegionContext
    {
        public TestStoreAppContextGood2()
        {
            this.personalinfs = new TestProductDbSetReg();
        }

        public DbSet<personalinf> personalinfs { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return Task<int>.FromResult(0);
        }

        public void MarkAsModified(personalinf item) { }
        public void Dispose() { }
    }

    public class TestStoreAppContextBad2 : IRegionContext
    {
        public TestStoreAppContextBad2()
        {
            this.personalinfs = new TestProductDbSetReg();
        }

        public DbSet<personalinf> personalinfs { get; set; }

        public Task<int> SaveChangesAsync()
        {
            throw new DbUpdateConcurrencyException();
        }

        public void MarkAsModified(personalinf item) { }
        public void Dispose() { }
    }
}
