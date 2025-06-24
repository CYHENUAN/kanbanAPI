using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class WorkOrder_Cell
{
    public int Id { get; set; }

    public long? WorkOrderId { get; set; }

    public int? CellId { get; set; }

    public virtual Cell? Cell { get; set; }

    public virtual WorkOrder? WorkOrder { get; set; }
}
