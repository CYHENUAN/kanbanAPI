using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class ProcurementType
{
    public int Id { get; set; }

    public string ProcurementCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
}
