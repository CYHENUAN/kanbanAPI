using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories;

public partial class SolderPrintingMachineRecipe
{
    public int Id { get; set; }

    public string? RecipeName { get; set; }
}
