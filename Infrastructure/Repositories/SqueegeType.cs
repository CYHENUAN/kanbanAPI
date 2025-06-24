using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class SqueegeType
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Squeege> Squeeges { get; set; } = new List<Squeege>();
}
