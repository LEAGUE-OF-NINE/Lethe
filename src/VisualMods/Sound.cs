using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Lethe.VisualMods;
public class Sound
{
	public static string soundFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"..","LocalLow","ProjectMoon/LimbusCompany/Assets/Sound/FMODBuilds/Desktop");
	public static string SmallestBank() => Directory.GetFiles(soundFolder, "*.bank*", SearchOption.AllDirectories).OrderBy(f => new FileInfo(f).Length).FirstOrDefault();
	public static void WaitingForValidation()
	{
		string smolBank = SmallestBank();
		LetheHooks.LOG.LogInfo($"Delete {smolBank}");
		File.Delete(smolBank);
		LetheHooks.LOG.LogInfo($"Status file {smolBank}: {File.Exists(smolBank)}");
		while (!File.Exists(smolBank))
			Thread.Sleep(10);
	}

	public static void SoundReplace()
	{
		WaitingForValidation();
		LetheHooks.LOG.LogInfo("Validation complete, replacing sound files...");
		bool hasBank = false;
		foreach (var customAppSubfolder in Carra.GetCustomAppearanceFolderList())
		{
			foreach (var bankPath in Directory.GetFiles(customAppSubfolder, "*.bank*", SearchOption.AllDirectories))
			{
				try
				{
					hasBank = true;
					LetheHooks.LOG.LogInfo($"Replacing {bankPath}");
					string target = Path.Combine(soundFolder, new FileInfo(bankPath).Name);
					File.Copy(target, $"{target}.bak", true);
					File.Copy(bankPath, target, true);
				}catch (Exception ex)
				{
					LetheHooks.LOG.LogError($"Error: {ex}");
				}
					
			}
		}
		if (!hasBank) LetheHooks.LOG.LogInfo("No .bank file found, skip sound replacing process.");
	}

	public static void RestoreSound()
	{
		LetheHooks.LOG.LogInfo("Restoring sound...");
		foreach (var backup in Directory.GetFiles(soundFolder, "*.bank.bak*"))
		{
			File.Copy(backup, backup.Replace(".bak", ""), true);
			File.Delete(backup);
		}
	}
}
