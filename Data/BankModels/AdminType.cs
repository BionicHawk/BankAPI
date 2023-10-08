using System;
using System.Collections.Generic;

namespace BankAPI.Data.BankModels;

public partial class AdminType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Administrator> Administrators { get; set; } = new List<Administrator>();
}
