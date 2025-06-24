using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class MSL
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int ExposureTimeInMinutes { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
