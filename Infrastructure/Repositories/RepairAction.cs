using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class RepairAction
{
    public int Id { get; set; }

    public string RepairActionCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PartRepairRecord> PartRepairRecords { get; set; } = new List<PartRepairRecord>();
}
