using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ZAD2_WebApplication.Models
{
    public class Request_EnrollStudent
    {
        [RegularExpression("s\\d{1,6}")]
        [Required(AllowEmptyStrings=false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string IndexNumber { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string LastName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public DateTime BirthDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Studies { get; set; }
    }
}
