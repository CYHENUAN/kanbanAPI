﻿using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class MachineConfiguration
{
    public int Id { get; set; }

    public string? StationNumber { get; set; }

    public string? MachineCode { get; set; }

    public string? IPV4Address { get; set; }
}
