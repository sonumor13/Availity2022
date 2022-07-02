using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test2022.Models
{
    public class EDIInformation
    {
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Version { get; set; }
        public string InsuranceCompany { get; set; }

    }
}