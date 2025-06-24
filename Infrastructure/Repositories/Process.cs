﻿using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class Process
{
    public int Id { get; set; }

    public string ProcessNumber { get; set; } = null!;

    public int MaterialId { get; set; }

    public string? Description { get; set; }

    public DateTime? EditDateTime { get; set; }

    public int? EditorId { get; set; }

    public int State { get; set; }

    public DateTime? ValidBegin { get; set; }

    public DateTime? ValidEnd { get; set; }

    public int? FactoryId { get; set; }

    public int? CreatorId { get; set; }

    public int? Version { get; set; }

    public DateTime? CreationDateTime { get; set; }

    public virtual User? Editor { get; set; }

    public virtual Factory? Factory { get; set; }

    public virtual Material Material { get; set; } = null!;

    public virtual ICollection<Material_Process> Material_Processes { get; set; } = new List<Material_Process>();

    public virtual ICollection<ProcessStep> ProcessSteps { get; set; } = new List<ProcessStep>();

    public virtual ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
