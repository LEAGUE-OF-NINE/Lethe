using UnityEngine.ProBuilder;
using UnityEngine;
using System.IO;
using System;
using Lethe;
using System.IO.Compression;
using System.Linq;
using AssetsTools.NET.Extra;
using AssetsTools.NET;
using SharpCompress.Compressors.Xz;
namespace Lethe.VisualMods;

public class Carra
{
	public static DirectoryInfo tmpAssetFolder;
	public static string[] moddedBundles;
	
	public static void Patch()
	{
		try
		{
			CleanUpAtLaunch();
			// tmp folder for extracting .carraX assets
			tmpAssetFolder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
				$"carra_{DateTime.Now.ToString("yyyy-MM-dd-H-m-ss")}"));
			LetheHooks.LOG.LogInfo($"Patching assets...");
			bool hasCarra = false;

			foreach (var customAppSubfolder in GetCustomAppearanceFolderList())
			{
				foreach (var bundlePath in Directory.GetFiles(customAppSubfolder, "*.carra*", SearchOption.AllDirectories))
				{
					hasCarra = true;
					LetheHooks.LOG.LogInfo($"Processing {bundlePath}...");
					string tmpOutput = Path.Combine(tmpAssetFolder.FullName, new FileInfo(bundlePath).Name);
					using (ZipArchive archive = ZipFile.Open(bundlePath, ZipArchiveMode.Read))
					{
						archive.ExtractToDirectory(tmpOutput);
					}

					AssetsPatch(tmpOutput);
				}
			}

			if (!hasCarra) LetheHooks.LOG.LogInfo("No .carra file found.");
		}
		catch (Exception ex)
		{
			LetheHooks.LOG.LogError($"Exception: {ex}");
		}
	}

	public static void AssetsPatch(string carraTmp)
	{
		foreach (var bnd in Directory.GetDirectories(carraTmp, "*", SearchOption.TopDirectoryOnly))
		{
			DirectoryInfo bundleInfo = Directory.CreateDirectory(bnd);
			string expectedRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"..",
				"LocalLow",
				"Unity",
				"ProjectMoon_LimbusCompany",
				bundleInfo.Name
			);
			if (!Directory.Exists(expectedRoot))
				LetheHooks.LOG.LogInfo($"Can't find {expectedRoot}, skip patching assets...");
			else
			{
				try
				{
					string expectedPath = Directory.GetFiles(expectedRoot, "*__data*", SearchOption.AllDirectories)
						.FirstOrDefault();
					if (String.IsNullOrEmpty(expectedPath)) continue;
					LetheHooks.LOG.LogInfo($"Backing up {expectedPath}...");
					File.Copy(expectedPath, expectedPath.Replace("__data", "__original"), true);
					LetheHooks.LOG.LogInfo($"Patching {expectedPath}...");

					var rnd = new System.Random();
					string randomName = $"tmp_{rnd.Next(1000, 10000)}.bytes";
					File.Copy(expectedPath, Path.Combine(tmpAssetFolder.FullName, randomName), true);
					LetheHooks.LOG.LogInfo($"Initiating asset tools...");
					var manager = new AssetsManager();
					var bundleInst = manager.LoadBundleFile(Path.Combine(tmpAssetFolder.FullName, randomName));
					var assetInst = manager.LoadAssetsFileFromBundle(bundleInst, 0, true);

					var asset = assetInst.file;
					var bundle = bundleInst.file;

					// decompress and patch
					foreach (string rawData in Directory.GetFiles(bundleInfo.FullName, "*", SearchOption.AllDirectories))
					{
						using (var xz = new XZStream(File.OpenRead(rawData)))
						using (Stream toFile = new FileStream(rawData + ".raw_asset", FileMode.Create))
						{
							xz.CopyTo(toFile);
						}

						var bjgbgb = Path.GetFileName(rawData).Split('.');
						long.TryParse(bjgbgb.First(), out long pathID);
						var success = int.TryParse(bjgbgb.Last(), out int treeID);
						AssetFileInfo __new = new();
						if (success)
						{
							var treeInfo = asset.Metadata.TypeTreeTypes[treeID];
							var scriptidx = treeInfo.ScriptTypeIndex;
							var typeID = treeInfo.TypeId;
							Console.WriteLine($"finding treeidx {treeID} scriptidx {scriptidx} for {pathID}");
							__new = AssetFileInfo.Create(asset, pathID, typeID, scriptidx);
							//var __new__basefield = manager.GetBaseField(assetInst, __new); dont rlly need this
							__new.SetNewData(File.ReadAllBytes(rawData + ".raw_asset"));
						}

						var overwrite_exist = asset.GetAssetInfo(pathID);
						if (overwrite_exist != null)
						{
							LetheHooks.LOG.LogInfo($"Overwrting {pathID}");
							overwrite_exist.SetNewData(File.ReadAllBytes(rawData + ".raw_asset"));
						}
						else asset.Metadata.AddAssetInfo(__new);
					}

					//finally pack uncompressed bundle
					LetheHooks.LOG.LogInfo("Writing modded assets to file...");
					bundle.BlockAndDirInfo.DirectoryInfos[0].SetNewData(asset);
					using (AssetsFileWriter writer = new AssetsFileWriter(expectedPath))
						bundle.Write(writer);
				}
				catch (Exception ex)
				{
					LetheHooks.LOG.LogError($"Error: {ex}");
				}
			}
		}
	}
	public static void CleanUpAtLaunch()
	{
		// remove carra_ temp folder from previous launch
		foreach (var tmpCarra in Directory.GetDirectories(Path.GetTempPath(), "carra_*", SearchOption.TopDirectoryOnly))
			Directory.Delete(tmpCarra, true);
	}

	public static void CleanUpAtClose()
	{
		string bundleRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"..",
				"LocalLow",
				"Unity",
				"ProjectMoon_LimbusCompany"
			);
		LetheHooks.LOG.LogInfo($"Cleaning up assets");
		foreach (var og in Directory.GetFiles(bundleRoot, "*__original", SearchOption.AllDirectories))
		{
			LetheHooks.LOG.LogInfo($"Restoring {og} -> {og.Replace("__original", "__data")}");
			File.Copy(og, og.Replace("__original", "__data"), true);
			File.Delete(og);
		}	
	}

	public static string[] GetCustomAppearanceFolderList() => Directory.GetDirectories(LetheMain.pluginPath.FullPath, "*custom_appearance*", SearchOption.AllDirectories);
}

