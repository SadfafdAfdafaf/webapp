using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SAS.Models
{
    public class Token
    {
        public int Id { get; set; }     
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? TimeofreleaseAccessToken { get; set; }
        public DateTime? TimeofreleaseRefreshToken { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Code")]
        public int AccessCodeId { get; set; }
        public Code Code { get; set; }

    }

    public class Code
    {
        public int Id { get; set; }

        public string AccessCode { get; set; }

        public DateTime? Timeofrelease { get; set; }

        [ForeignKey("Owners")]
        public int ownerId { get; set; }
        public Owners Owners { get; set; }
    }

    public class Owners
    {
        public int Id { get; set; }

        public int ClienSecret { get; set; }

        public string RedirectUri { get; set; }

        public string Name { get; set; }

    }


    //формат отсылки токенов на гейт
    public class tokenmessage
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }

        public string token_type { get; set; }
    }
}