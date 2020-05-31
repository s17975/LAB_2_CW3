using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZAD2_WebApplication.Models
{
    public class Response_StudentAuthentication
    {
        public string IndexNumber { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
