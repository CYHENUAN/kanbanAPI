using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class ReplenishLocation_Station
{
    public int Id { get; set; }

    public int ReplenishLocationId { get; set; }

    public int StationId { get; set; }

    public virtual Location ReplenishLocation { get; set; } = null!;

    public virtual Station Station { get; set; } = null!;
}
