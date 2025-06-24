using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class MaterialOtherMovementForm
{
    public int Id { get; set; }

    public int? FormId { get; set; }

    public virtual WMSForm? Form { get; set; }

    public virtual ICollection<MaterialOtherMovementFormItem> MaterialOtherMovementFormItems { get; set; } = new List<MaterialOtherMovementFormItem>();
}
