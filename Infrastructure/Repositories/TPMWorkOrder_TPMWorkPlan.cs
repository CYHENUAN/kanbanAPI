using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class TPMWorkOrder_TPMWorkPlan
{
    public int Id { get; set; }

    public int TPMWorkOrderId { get; set; }

    public int TPMWorkPlanId { get; set; }

    public DateTime CreationDateTime { get; set; }

    public virtual TPMWorkOrder TPMWorkOrder { get; set; } = null!;

    public virtual TPMWorkPlan TPMWorkPlan { get; set; } = null!;
}
