using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class MaterialReceivingForm
{
    public int Id { get; set; }

    public int FormId { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public virtual WMSForm Form { get; set; } = null!;

    public virtual ICollection<MaterialReceivingFormItem> MaterialReceivingFormItems { get; set; } = new List<MaterialReceivingFormItem>();
}
