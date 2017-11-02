using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class workermodel
    {
        public int Id { get; set; }
        public string FIO { get; set; }
        public int Company { get; set; }
        public int Cost { get; set; }
        public int RegionOffice { get; set; }

    }

    public class companiesmodel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string CEO { get; set; }
        public int region { get; set; }
    }

    public class personalinfmodel
    {
        public int Id { get; set; }
        public string claster { get; set; }
    }

    public class detailedworkermodel
    {
        public string FIO { get; set; }
        public string Name { get; set; }
        public string CEO { get; set; }
        public int region { get; set; }
        public int Cost { get; set; }
        public int RegionOffice { get; set; }

    }
}