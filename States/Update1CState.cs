using System.IO.Compression;
using System.Text.RegularExpressions;

using static PathConstants;
using static CommonUtils;
using static InputUtils;

public class Update1CState : IStateExecuter
{
	private string? foundStarterPath;
	private string? IBConnectionString;
	private string? baseName;
	private string? baseLogin;
	private string? basePassword;
	private string? foundArchivePath;
	private string? tmpltsPath; 

	private string? platformPath;

	private static string updateDirName = "sancity";
	private string? updatesDir = Path.Combine(desktop, updateDirName);

	private void UpdatePlatformPath()
	{
		platformPath = VersionUtils.FindCurrentMaxVersionPath() + @"\bin\1cv8.exe";
	}

	private static string GetCurrentDate()
	{
		return DateTime.Now.ToString("dd.MM.yyyy");
	}

	private static string GetBackupName(string str)
	{
		return str.Trim().Replace(" ", "_").Replace("-", "_") + "_" + GetCurrentDate() + ".dt";
	}

    public void Execute()
    {
		UpdatePlatformPath();

		if (!File.Exists(platformPath))
		{
			Log.Error("Не найдена платформа на компьютере!");
			Pause();
			return;
		}

		ShowPCDrivesInfo();

		Pause();

		SetState("Ввод информации для работы ПО", "Ввод информации");

		var possibleBases = BasesFileParser.GetBases();

		string[] possibleBasesOnlyNames = new string[possibleBases.Keys.Count];
		string[] possibleBasesOnlyNamesForList = new string[possibleBases.Keys.Count];

		int index = 0;
		int maxStrLength = 0;
		foreach (var basePair in possibleBases) {
			if (maxStrLength < basePair.Key.Length) maxStrLength = basePair.Key.Length;
		}
		foreach (var basePair in possibleBases) {
			possibleBasesOnlyNames[index] = basePair.Key;

			possibleBasesOnlyNamesForList[index] = basePair.Key;
			for (int i = 0; i < maxStrLength - basePair.Key.Length; i++) possibleBasesOnlyNamesForList[index] += " ";
			possibleBasesOnlyNamesForList[index] += "\t: " + basePair.Value;
			index++;
		}

		int choice = UserHaveToChooseBetween(
			"Выберите базу, с которой хотите работать",
			possibleBasesOnlyNamesForList
		);
		
		
		IBConnectionString = possibleBases.GetValueOrDefault(possibleBasesOnlyNames[choice - 1]);

		IBConnectionString = IBConnectionString.Replace(@"""", @"""""");
		IBConnectionString = $"\"{IBConnectionString}\"";

		baseName = possibleBasesOnlyNames[choice - 1]; 

		if (AskYOrN($"Название бэкапа базы: {GetBackupName(baseName)}. Вы можете сменить название базы. Вы хотите?"))
		{
			Log.WriteLine("Введите другое желаемое название базы:");
			baseName = KernelInput.ReadLine();
		}

		Log.Write("Логин ИБ (если нужен):");
		baseLogin = KernelInput.ReadLine();
		baseLogin = baseLogin.Trim();

		basePassword = "";
		if (!IsEmpty(baseLogin))
		{
			Log.Write("Пароль ИБ (если нужен):");
			basePassword = KernelInput.ReadLine();
		}

		
		foreach (string path in cArchivePossiblePaths)
		{
			if (Directory.Exists(path))
			{
				foundArchivePath = path;
				break;
			}
		}

		
		if (IsEmpty(foundArchivePath))
		{
			Log.Warn("Не был найден архив 1C");

			foundArchivePath = ControlUserForCorrectInput(
				"Введите путь к архиву баз 1С (будет создан, если его нет). Пример: C:\\архив 1С ИЛИ C архив 1С ",
				"Неверный формат введенного пути!",
				s =>
				{
					if (s.Contains('\\') && Directory.Exists(s)) return true;
					string[] tokens = s.Split(" ");

					string drive = $"{tokens[0]}:\\";
	
					if (!Directory.Exists(drive)) return false;

					string folder = "";
					for (int i = 1; i < tokens.Length; i++) folder += tokens[i];

					return true;
				}
			);

			if (!Directory.Exists(foundArchivePath))
			{
				string[] tokens = foundArchivePath.Split(" ");

				string drive = $"{tokens[0]}:\\";

				string folder = "";
				for (int i = 1; i < tokens.Length; i++) folder += tokens[i];

				foundArchivePath = drive + folder;
			}

			if (!Directory.Exists(foundArchivePath))
			{
				Directory.CreateDirectory(foundArchivePath);
			}
		}

		Log.SkipLine();
		Log.Success($"Найден архив 1С {foundArchivePath}");
		Log.SkipLine();

		tmpltsPath = Path.Combine(userProfile, "AppData", "Roaming", "1C", "1cv8", "tmplts");
		if (!Directory.Exists(tmpltsPath))
		{
			tmpltsPath = TemplatesFilePathParser.GetTemplatesPath();

			if (!Directory.Exists(tmpltsPath) || IsEmpty(tmpltsPath))
			{
				Log.Warn($"tmplts нет по пути {tmpltsPath}.");
				Log.SkipLine();
				tmpltsPath = ControlUserForCorrectInput(
					$"Вам нужно найти, где находятся шаблоны конфигураций и вставить полный путь.",
					"По пути ничего не найдено!",
					s =>
					{
						return Directory.Exists(s);
					}
				);
			}

		}
		
		bool userWantsAnotherUpdateDir = AskYOrN($"Папка, куда вы будете помещать обновления: {updatesDir}.\nХотите сменить директорию? (например, если обновляете не одну конфигурацию)\n");
		
		string manualUpdatesDir = "";
		if (userWantsAnotherUpdateDir)
		{
			manualUpdatesDir = ControlUserForCorrectInput(
			$"Название папки на рабочем столе вместо {updateDirName}, если обновляете больше 1 базы:",
			"Неправильный формат пути!",
			s =>
			{
				if (s.Contains('/')) return false;

				return true;
			}
			);

			manualUpdatesDir = Path.Combine(desktop, manualUpdatesDir);
			updateDirName = manualUpdatesDir;
		}


		if (!IsEmpty(manualUpdatesDir))
		{
			updatesDir = manualUpdatesDir;
		}


		if (!Directory.Exists(updatesDir))
		{
			Directory.CreateDirectory(updatesDir);
		}

		/**if (AskYOrN("Завершить работу пользователей? Код разблокировки: КодРазрешения"))
		{
			ProcessUtils.RunProcess(platformPath, 
							arguments:$"{ArgumentUtils.FormArgumentStringEnterprise(IBConnectionString, baseLogin, basePassword)}");

		}*/

		Log.WriteLine("Строка запуска (для проверки):");
		Log.WriteLine(string.Format(@"""{0}"" {1}", platformPath, 
			ArgumentUtils.FormArgumentStringDesigner(IBConnectionString, baseLogin, basePassword)));
		Log.SkipLine();
		Pause();

		Log.SkipLine();
		Log.WriteLine("Запустите конфигуратор проверьте в нем версию платформы и конфигурации");
		Log.WriteLine($"а после загрузите, что необходимо, в папку без распаковки");
		Log.SkipLine();

		string updateFilePath = Path.Combine(updatesDir, "update.txt");
		if (File.Exists(updateFilePath))
			File.Delete(updateFilePath);

		ProcessUtils.RunProcessNoWait("https://releases.1c.ru/total", "", useShellExecute: true);
		Log.WriteLine("Далее начнется выгрузка ИБ, а после - обновление базы");
		Log.Write("Нажмите enter, если папка готова и все обновления находятся в ней");
		Console.ReadLine();

		string[] rarFiles = Directory.GetFiles(updatesDir, "*.rar");
		bool hasRar = rarFiles.Length > 0;

		if (hasRar)
		{
			SetState("Установка платформы 1с", "Установка...");

			string setupRar = rarFiles[0];

			Log.WriteLine($"Путь к 1с: {rarFiles[0]}");

			if (!ArchiverUtils.CanUnZipRar())
			{
				Log.Error("7z и WinRar не были найдены! Распакуйте платформу сами.\nДолжно получиться \\upd1c\\1c_setup\\");
				Pause();
			}
			
			string unZipDir = Path.Combine(updatesDir, "1c_setup");
			if (!Directory.Exists(unZipDir))
				Directory.CreateDirectory(unZipDir);

			Log.WriteLine($"Распаковка... {rarFiles[0]}");
			ArchiverUtils.UnZip(setupRar, unZipDir, true);

			string setupExe = Path.Combine(unZipDir, "setup.exe");
			if (File.Exists(setupExe))
			{
				// Пытаемся с тихой установкой
				int exitCode = ProcessUtils.RunProcess(setupExe, "/S /l:ru");
				if (exitCode != 0)
				{
					ProcessUtils.RunProcess(setupExe, "/l:ru");
				}
			}

			UpdatePlatformPath();
		}

		SetState("Выгрузка информационной базы", "Выгрузка...");

		Log.WriteLine("Выгружаю информационную базу...");
		string dtFilePath = Path.Combine(foundArchivePath, GetBackupName(baseName));

		if (!File.Exists(dtFilePath)) ProcessUtils.RunProcess(platformPath, 
										arguments:$"{ArgumentUtils.FormArgumentStringDesigner(IBConnectionString, baseLogin, basePassword)} /DumpIB \"{dtFilePath}\"");

		Log.Success($"Выгрузка закончена: {dtFilePath}");

		Pause();

		SetState("Распаковка обновлений из папки", "Распаковка...");

		string configurationName = null;
		string configurationVersion = null;
		int fileCounter = 0;

		Log.WriteLine($"Текущая папка: {Directory.GetCurrentDirectory()}");
		Log.WriteLine("Поиск архивов ZIP...");

		// Ожидание, пока появятся zip-файлы
		while (Directory.GetFiles(updatesDir, "*.zip").Length == 0)
		{
			Log.Error($"Архивы обновлений конфигурации .zip не найдены в {updatesDir}. Необходимо их туда поместить");
			Pause();
		}

		string[] zipFiles = Directory.GetFiles(updatesDir, "*.zip");
		Array.Sort(zipFiles);

		foreach (string zipFile in zipFiles)
		{
			fileCounter++;
			string zipFileName = Path.GetFileName(zipFile).Split('.')[0];

			Log.WriteLine($"[{fileCounter}] Найден архив {zipFileName}");

			string folder = Path.Combine(updatesDir, zipFileName);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);

				try
				{
					ZipFile.ExtractToDirectory(zipFile, folder);
				}
				catch
				{
					Log.Error("Не удалось распаковать через библиотеку ZipFile. Использую fallback.");
					
					if (!ArchiverUtils.CanUnZipZip())
					{
						Log.Error("В системе не найден tar.exe и 7zip. Установите их прямо сейчас, если не хотите неожиданного поведения!");
					}

					ArchiverUtils.UnZip(zipFile, folder, true);
				}
			}

			string setupExe = Path.Combine(folder, "setup.exe");
			if (File.Exists(setupExe))
			{
				Log.WriteLine($"[{fileCounter}] Запуск установки...");
				ProcessUtils.RunProcess(setupExe, "/S");
			}
			else
			{
				Log.WriteLine($"[{fileCounter}] Файл setup.exe не найден в папке {folder}");
			}

			// Чтение ReadMe.txt (первая строка — имя конфигурации)
			string readmePath = Path.Combine(folder, "ReadMe.txt");
			if (File.Exists(readmePath) && IsEmpty(configurationName))
			{
				string[] lines = File.ReadAllLines(readmePath);
				if (lines.Length > 0)
					configurationName = lines[0];
			}

			// Чтение VerInfo.txt
			string verInfoPath = Path.Combine(folder, "VerInfo.txt");
			if (File.Exists(verInfoPath))
			{
				string content = File.ReadAllText(verInfoPath).Trim();
				configurationVersion = content;
			}
		}

		Log.SkipLine(3);

		SetState("Установка обновлений из папки", "Установка...");

		int currentUpdate = 1;
		
		foreach (string zipFile in zipFiles)
		{
			string zipFileName = Path.GetFileName(zipFile).Split('.')[0];

			string folder = Path.Combine(updatesDir, zipFileName);

			string efdFile = Path.Combine(folder, "1cv8.efd");

			string confUpdatePath = VersionUtils.GetConfUpdatePath(tmpltsPath, zipFileName);

			if (IsEmpty(confUpdatePath))
			{
				string updatePath = ControlUserForCorrectInput(
					$"Папка с шаблонами конфигураций открылась. Укажите полный путь к папке с установленным релизом:",
					"По пути не найдено файла обновления .cfu!",
					s =>
					{
						return Directory.Exists(s) && File.Exists(Path.Combine(s, "1cv8.cfu"));
					}
				);
			}
/**
			string mftPath = Path.Combine(confUpdatePath, "1cv8.mft");
			if (File.Exists(mftPath))
			{
				string[] lines = File.ReadAllLines(mftPath);
				if (lines.Length > 0)
					
			}*/

			string cfuFile = Path.Combine(confUpdatePath, "1cv8.cfu");


			ProcessUtils.RunProcess(platformPath, 
				arguments:$"{ArgumentUtils.FormArgumentStringDesigner(IBConnectionString, baseLogin, basePassword)} /UpdateCfg \"{cfuFile}\" /UpdateDBCfg");

			Log.Success(string.Format("Успешно установлена конфигурация {0}", currentUpdate));
		}
		
		ProcessUtils.RunProcessNoWait(platformPath, 
			ArgumentUtils.FormArgumentStringEnterprise(IBConnectionString, baseLogin, basePassword));

		Log.Success("Все обновления установлены");

		if (!IsEmpty(configurationName) || fileCounter > 0)
		{
			string resultText = string.Format("Обновлена «{0}» базы «{1}» до v.{2}",
				configurationName ?? "?", baseName, configurationVersion ?? "?");
			File.AppendAllText(updateFilePath, resultText + Environment.NewLine);
			File.AppendAllText(updateFilePath, $"Количество поставленных релизов: {fileCounter}" + Environment.NewLine);
			if (hasRar)
			{
				string updateText = $"Обновлена платформа до {VersionUtils.GetPlatformVersionFromPath(platformPath)}";
				File.AppendAllText(updateFilePath, updateText + Environment.NewLine);
			}
		}

		Pause();
    }

	public static void ShowPCDrivesInfo()
	{
		Log.WriteLine("Информация о дисках на ПК. Сделайте вывод перед бэкапом.");
		Log.SkipLine();

		DriveInfo[] allDrives = DriveInfo.GetDrives();

		foreach (DriveInfo d in allDrives)
        {
            if (d.IsReady == true)
            {
				long TotalSize = d.TotalSize / 1024 / 1024 / 1024;
				long FreeSize = d.TotalFreeSpace / 1024 / 1024 / 1024;
				int FreeSizePercent = (int)(100 * (float)FreeSize / (float)TotalSize);

                Log.Write($"\t{d.Name}  {TotalSize - FreeSize} / {TotalSize} GB | Свободное место: {FreeSizePercent}%\n");
				Log.SkipLine();
            }
        }
	}
}