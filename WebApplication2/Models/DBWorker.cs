using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;


namespace WebApplication2.Models
{
    public class Worker
    {
        public int Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FIO { get; set; }
        public int Company { get; set; }        
        public int Cost { get; set; }
        public int RegionOffice { get; set; }
    }

}