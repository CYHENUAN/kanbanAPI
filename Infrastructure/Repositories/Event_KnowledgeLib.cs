using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class Event_KnowledgeLib
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public long KnowledgeLibId { get; set; }

    public DateTime CreationDateTime { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual EventKnowledgeLibrary KnowledgeLib { get; set; } = null!;
}
