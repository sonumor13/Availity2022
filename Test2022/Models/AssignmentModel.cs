using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test2022.Models
{
    public class AssignmentModel
    {
        public HttpPostedFileBase file { get; set; }

        public string FirsLastName { get; set; }
        public string NPINumber { get; set; }
        public string BusinesAddress { get; set; }
        public string TelephoneNumber { get; set; }
        public string EmailAddress { get; set; }

    }
}