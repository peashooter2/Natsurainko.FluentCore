﻿using Nrk.FluentCore.Experimental.GameManagement.Dependencies;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Nrk.FluentCore.Experimental.GameManagement.Instances;

public static class MinecraftInstanceExtensions
{
    public static string GetSuitableJavaVersion(this MinecraftInstance instance)
    {
        if (instance is ModifiedMinecraftInstance { HasInheritance: true } inst)
            return inst.GetSuitableJavaVersion();

        JsonNode? majorJavaVersionNode = JsonNodeUtils.ParseFile(instance.ClientJsonPath)["javaVersion"]?["majorVersion"];

        if (majorJavaVersionNode is null)
            return "8";
        else
            return majorJavaVersionNode.GetValue<int>().ToString();
    }

    public static IEnumerable<ModLoaderInfo> GetModLoaders(this MinecraftInstance instance)
    {
        if (instance is ModifiedMinecraftInstance inst)
            return inst.ModLoaders;
        else
            return [];
    }

    public static void Delete(this MinecraftInstance instance)
    {
        string versionDirPath = Path.Combine(instance.MinecraftFolderPath, "versions", instance.VersionFolderName);
        Directory.Delete(versionDirPath, true);
    }

    public static MinecraftClient? GetJarElement(this MinecraftInstance instance)
    {
        string clientJsonPath = instance.ClientJsonPath;
        if (instance is ModifiedMinecraftInstance { HasInheritance: true } inst)
            clientJsonPath = inst.InheritedMinecraftInstance.ClientJsonPath;

        JsonNode? clientArtifactNode = JsonNodeUtils.ParseFile(clientJsonPath)["downloads"]?["client"];

        if (clientArtifactNode is null)
            return null;

        string clientJarPath = instance.ClientJarPath;
        if (instance is ModifiedMinecraftInstance { HasInheritance: true } inst_)
            clientJarPath = inst_.ClientJarPath;

        if (clientJarPath is null)
            return null;

        string? sha1 = clientArtifactNode["sha1"]?.GetValue<string>();
        string? url = clientArtifactNode["url"]?.GetValue<string>();
        int? size = clientArtifactNode["size"]?.GetValue<int>();

        if (sha1 is null || url is null || size is null)
            throw new InvalidDataException("Invalid client info");

        return new MinecraftClient(url)
        {
            MinecraftFolderPath = instance.MinecraftFolderPath,
            ClientId = Path.GetFileNameWithoutExtension(clientJarPath),
            Size = (int)size,
            Sha1 = sha1
        };
    }

    public static GameStorageInfo GetStatistics(this MinecraftInstance instance)
    {
        var (libs, nativeLibs) = instance.GetRequiredLibraries();
        var assetIndex = instance.GetAssetIndex();
        var assets = instance.GetRequiredAssets();

        long size = 0;
        int assetCount = 0;
        int libCount = 0;

        foreach (var lib in libs.Concat(nativeLibs))
        {
            if (File.Exists(lib.FullPath))
                size += new FileInfo(lib.FullPath).Length;
            libCount++;
        }

        foreach (var asset in assets)
        {
            assetCount++;
            if (File.Exists(asset.FullPath))
            {
                size += new FileInfo(asset.FullPath).Length;
            }
        }

        size += new FileInfo(assetIndex.FullPath).Length;

        if (File.Exists(instance.ClientJarPath))
            size += new FileInfo(instance.ClientJarPath).Length;

        size += new FileInfo(instance.ClientJsonPath).Length;

        return new GameStorageInfo
        {
            AssetsCount = assetCount,
            LibrariesCount = libCount,
            ModLoaders = instance.GetModLoaders(),
            TotalSize = size
        };
    }
}

/// <summary>
/// 游戏统计数据
/// </summary>
public record GameStorageInfo
{
    /// <summary>
    /// 依赖库文件数
    /// </summary>
    public int LibrariesCount { get; set; }

    /// <summary>
    /// 依赖材质文件数
    /// </summary>
    public int AssetsCount { get; set; }

    /// <summary>
    /// 共计占用磁盘空间大小
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// 加载器
    /// </summary>
    public IEnumerable<ModLoaderInfo>? ModLoaders { get; set; }
}