using System;
using System.Collections.Generic;
using System.Text;

namespace JustABackup.Models.Authentication
{
    public class AuthenticationSessionDetailModel : BaseViewModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Provider { get; set; }
    }
}
