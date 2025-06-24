using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class Line
{
    public int Id { get; set; }

    public string? LineCode { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
}
