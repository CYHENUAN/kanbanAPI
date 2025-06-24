﻿using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class FailureSlip
{
    public long Id { get; set; }

    public long PartProcessRecordId { get; set; }

    public string Content { get; set; } = null!;

    public virtual PartProcessRecord PartProcessRecord { get; set; } = null!;
}
