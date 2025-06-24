using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class SolderPaste
{
    public int Id { get; set; }

    public int MaterialId { get; set; }

    public int TimesOfRegistToFreezingLocation { get; set; }

    public int TimesOfRegistToUnfreezingLocation { get; set; }

    public int TimesOfRegistToStationLocation { get; set; }

    public int CurrentLocationId { get; set; }
}
