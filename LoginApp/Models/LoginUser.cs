using System;
using System.Collections.Generic;

namespace LoginApp.Models
{
    public partial class LoginUser
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
