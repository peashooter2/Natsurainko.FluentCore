﻿using System;
using System.Threading.Tasks;

namespace Nrk.FluentCore.GameManagement.ModLoaders;

public abstract class ModLoaderInstallerBase : IModLoaderInstaller
{
    #region IModLoaderInstaller Members

    public required string AbsoluteId { get; set; }

    public required MinecraftInstance InheritedFrom { get; set; }

    public event EventHandler<double>? ProgressChanged;

    public abstract Task<InstallResult> ExecuteAsync();

    #endregion

    protected void OnProgressChanged(double progress) => ProgressChanged?.Invoke(this, progress);
}
