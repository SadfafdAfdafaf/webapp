using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Models;
using WebApplication3.Models;
using WebApplication4.Models;

namespace UnitTestProject1
{
    class TestProductDbSet : TestDbSet<Worker>
    {
        public override Task<Worker> FindAsync(params object[] keyValues)
        {
            return Task<Worker>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }

    class TestProductDbSetComp : TestDbSet<companies>
    {
        public override Task<companies> FindAsync(params object[] keyValues)
        {
            return Task<companies>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }

    class TestProductDbSetReg : TestDbSet<personalinf>
    {
        public override Task<personalinf> FindAsync(params object[] keyValues)
        {
            return Task<personalinf>.FromResult(this.SingleOrDefault(product => product.Id == (int)keyValues.Single()));
        }
    }
}
