using System;
using System.IO;
using System.Linq;
using System.Threading;
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
		while (!File.Exists(smolBank))
			Thread.Sleep(10);
	}

	public static void SoundReplace()
	{
		if (!CheckExistingBank())
			LetheHooks.LOG.LogInfo("No .bank file found, skip sound replacing process.");
		else
		{
			WaitingForValidation();
			LetheHooks.LOG.LogInfo("Validation complete, replacing sound files...");
			foreach (var customAppSubfolder in Carra.GetCustomAppearanceFolderList())
			{
				foreach (var bankPath in Directory.GetFiles(customAppSubfolder, "*.bank*", SearchOption.AllDirectories))
				{
					try
					{
						LetheHooks.LOG.LogInfo($"Replacing {bankPath}");
						string target = Path.Combine(soundFolder, new FileInfo(bankPath).Name);
						if (!File.Exists(target))
						{
							LetheHooks.LOG.LogInfo($"Can't find {target}, skip replacing...");
							continue;
						}
						File.Copy(target, $"{target}.bak", true);
						File.Copy(bankPath, target, true);
					}
					catch (Exception ex)
					{
						LetheHooks.LOG.LogError($"Error: {ex}");
					}

				}
			}		
		}
	}

	public static bool CheckExistingBank()
	{
		foreach (var customAppSubfolder in Carra.GetCustomAppearanceFolderList())
		{
			if (Directory.GetFiles(customAppSubfolder, "*.bank*", SearchOption.AllDirectories).Length > 0) return true;
		}		
		return false;
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
