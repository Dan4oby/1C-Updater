using System.IO.Compression;
using System.Text.RegularExpressions;

using static PathConstants;
using static CommonUtils;
using static InputUtils;

public class Update1CState : IStateExecuter
{
    public void Execute()
    {
        string foundStarterPath = null;
		foreach (string path in cStarterPossiblePaths)
		{
			if (File.Exists(path))
			{
				foundStarterPath = path;
				break;
			}
		}

		if (foundStarterPath == null)
		{
			Log.Error("1C не найден ни в одном из путей.");
			Pause();
			return;
		}

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

		Pause();

		SetState("Ввод информации для работы ПО", "Ввод информации");

		PlayLookAtMeSound();

		ProcessUtils.RunProcessNoWait(foundStarterPath);

		
		string IBConnectionString = 
		ControlUserForCorrectInput(
			@"Вставьте строку подключения к информационной базе. Пример: File=""C:\Bases\BUH"";",
			"Строка подключения неправильная!", 
			s =>
			{
				string[] possibleConnectionWays =
				{
					"File",
					"srvr"
				};
				string[] tokens = s.Split("=");

				if (tokens.Length < 2) return false;

				string connectionWay = tokens[0];

				if (!possibleConnectionWays.Contains(connectionWay)) return false;

				// todo: все кроме File не понятно, как парсить
				// нужно больше примеров
				if (!connectionWay.Equals("File")) return true;

				string path = tokens[1];

				if (!path.Contains(@"""") || !path.Contains(";")) return false;

				path = path.Replace(@"""", "");
				path = path.Replace(";", "");

				if (!Directory.Exists(path)) return false;

				return true;
			}
		);

		IBConnectionString = IBConnectionString.Trim();
		IBConnectionString = IBConnectionString.Replace(@"""", @"""""");
		IBConnectionString = $"\"{IBConnectionString}\"";

		bool launchOsk = AskYOrN("Включить экранную клавиатуру? ");

		if (launchOsk) ProcessUtils.RunProcessNoWait("osk.exe", useShellExecute:true);

		string currentDate = DateTime.Now.ToString("dd.MM.yyyy");

		Log.WriteLine("Название бэкапа базы (будет взято из пути при пропуске): ");
		string baseName = KernelInput.ReadLine();
		if (IsEmpty(baseName))
		{
			string[] tokens = IBConnectionString.Split('"');
			string path = tokens[3];
			string[] pathTokens = path.Split('\\');
			baseName = pathTokens[pathTokens.Length - 1];
		}
		baseName = baseName.Trim();
		baseName = baseName.Replace(" ", "_");
		baseName = baseName.Replace("-", "_");

		Log.WriteLine($"Название бэкапа: {baseName}_{currentDate}.dt");
		Log.SkipLine();

		Log.Write("Логин ИБ (если нужен):");
		string baseLogin = KernelInput.ReadLine();
		baseLogin = baseLogin.Trim();

		string basePassword = "";
		if (!IsEmpty(baseLogin))
		{
			Log.Write("Пароль ИБ (если нужен):");
			basePassword = KernelInput.ReadLine();
		}

		string foundArchivePath = "";
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

		string tmpltsPath = Path.Combine(userProfile, "AppData", "Roaming", "1C", "1cv8", "tmplts", "1c");
		if (!Directory.Exists(tmpltsPath))
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

		Log.SkipLine();
		Log.WriteLine("Запустите конфигуратор проверьте в нем версию платформы и конфигурации");
		Log.WriteLine("а после загрузите, что необходимо, в папку User\\Desktop\\upd1c без распаковки");
		Log.SkipLine();
		
		string platformVersionPath = VersionUtils.FindCurrentMaxVersionPath();
		platformVersionPath += @"\bin\1cv8.exe";
		string argumentString = ArgumentUtils.FormArgumentString(ArgumentUtils.ONEC.DESIGNER, IBConnectionString, baseLogin, basePassword);
		Log.WriteLine("Строка запуска (для проверки):");
		Log.WriteLine(string.Format(@"""{0}"" {1}", platformVersionPath, argumentString));
		Log.SkipLine();

		string manualUpdatesDir = ControlUserForCorrectInput(
			"Название папки на рабочем столе вместо upd1c, если обновляете больше 1 базы:",
			"Неправильный формат пути!",
			s =>
			{
				if (IsEmpty(s)) return true;

				if (s.Contains('/')) return false;

				return true;
			}
		);

		string updatesDir;
		if (IsEmpty(manualUpdatesDir))
		{
			updatesDir = Path.Combine(desktop, "upd1c");
		}
		else
		{
			updatesDir = Path.Combine(desktop, manualUpdatesDir);
		}

		if (!Directory.Exists(updatesDir))
		{
			Directory.CreateDirectory(updatesDir);
		}

		ProcessUtils.RunProcess("explorer.exe", updatesDir, true, waitForProcess:false); 

		Directory.SetCurrentDirectory(updatesDir);

		string updateFilePath = Path.Combine(updatesDir, "update.txt");
		if (File.Exists(updateFilePath))
			File.Delete(updateFilePath);

		ProcessUtils.RunProcessNoWait("https://releases.1c.ru/total", "", useShellExecute: true);
		Log.WriteLine("Далее начнется выгрузка ИБ, а после обновление базы");
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

				platformVersionPath = VersionUtils.FindCurrentMaxVersionPath();
				platformVersionPath += @"\bin\1cv8.exe";
			}
		}

		SetState("Выгрузка информационной базы", "Выгрузка...");

		Log.WriteLine("Выгружаю информационную базу...");
		string dtFilePath = Path.Combine(foundArchivePath, $"{baseName}_{currentDate}.dt");

		if (!File.Exists(dtFilePath)) ProcessUtils.RunProcess(platformVersionPath, arguments:$"{argumentString} /DumpIB \"{dtFilePath}\"");

		Log.Success($"Выгрузка закончена: {dtFilePath}");
		PlayLookAtMeSound();
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
			Log.WriteLine($"[{fileCounter}] Найден архив {Path.GetFileName(zipFile)}");

			string folder = Path.Combine(updatesDir, fileCounter.ToString());
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

					ArchiverUtils.UnZip(zipFile, folder, false);
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
		
		while (currentUpdate <= fileCounter)
		{
			string updateFolder = Path.Combine(updatesDir, currentUpdate.ToString());
			string efdFile = Path.Combine(updateFolder, "1cv8.efd");
			if (File.Exists(efdFile))
			{
				// Открыть папку с шаблонами в проводнике
				try 
				{ 
					ProcessUtils.RunProcess("explorer.exe", tmpltsPath, true, waitForProcess:false); 
				} catch { }

				PlayLookAtMeSound();

				Log.WriteLine("Темплейты открылись. Укажите полный путь к папке с установленным релизом:");
				string updatePath = KernelInput.ReadLine();

				string cfuFile = Path.Combine(updatePath, "1cv8.cfu");
				if (!File.Exists(cfuFile))
				{
					Log.Error($"У введенного релиза не найден файл {cfuFile}");
					Log.Error("Исправьте ошибку самостоятельно, либо досрочно завершите выполнение программы");
					Pause();

					continue;
				}

				Log.WriteLine("Обновляю конфигурацию...");

				ProcessUtils.RunProcess(platformVersionPath, arguments:$"{argumentString} /UpdateCfg \"{cfuFile}\" /UpdateDBCfg");

				Log.Success(string.Format("Успешно установлена конфигурация {0}", currentUpdate));
			}

			currentUpdate++;
		}

		argumentString = ArgumentUtils.FormArgumentString(ArgumentUtils.ONEC.ENTERPRISE, IBConnectionString, baseLogin, basePassword);
		ProcessUtils.RunProcessNoWait(platformVersionPath, argumentString);

		PlayLookAtMeSound();

		Log.Success("Все обновления установлены");

		if (!IsEmpty(configurationName) || fileCounter > 0)
		{
			string resultText = string.Format("Обновлена «{0}» базы «{1}» до v.{2}",
				configurationName ?? "?", baseName, configurationVersion ?? "?");
			File.AppendAllText(updateFilePath, resultText + Environment.NewLine);
			File.AppendAllText(updateFilePath, $"Количество поставленных релизов: {fileCounter}" + Environment.NewLine);
			if (hasRar)
			{
				string updateText = $"Обновлена платформа до {VersionUtils.GetPlatformVersionFromPath(platformVersionPath)}";
				File.AppendAllText(updateFilePath, updateText + Environment.NewLine);
			}
		}

		Pause();
    }
}