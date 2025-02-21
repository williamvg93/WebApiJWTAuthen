using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiJwt.Entities
{
    public class User
    {
        public int Id {get; set;}
        public string UserName { get; set;} = string.Empty;
        public string PasswordHash {get;set;} = string.Empty;
        public string Role {get; set;} = string.Empty;
        public string? RefreshToken {get; set;}
        public DateTime RefTokenExpTime {get; set;}
    }
}