using System;
using System.Collections.Generic;

namespace DataLayerObject.Models
{
    public partial class Account
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public byte? Role { get; set; }

        public virtual Employee Employee { get; set; } = null!;
    }
}
