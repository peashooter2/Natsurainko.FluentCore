﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nrk.FluentCore.GameManagement;

public enum MinecraftVersionType
{
    Release,
    Snapshot,
    OldBeta,
    OldAlpha
}

public record struct MinecraftVersion(MinecraftVersionType Type, string Version)
{
    public static MinecraftVersion Unknown => new(MinecraftVersionType.Release, "Unknown");
}
