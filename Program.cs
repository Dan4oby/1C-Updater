using System.Diagnostics;
using System.IO.Compression;
using System.Text; 
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using static PathConstants;
using static CommonUtils;


class Program {
	static Program() {

		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		SetTitleStatus("");
		//Console.BackgroundColor = ConsoleColor.DarkCyan;
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
	}

	static void Main(string[] args) {

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

		Log.WriteLine("Полностью вставьте строку подключения к информационной базе (снизу): ");
		string IBConnectionString = KernelInput.ReadLine();
		IBConnectionString.Trim();
		IBConnectionString = IBConnectionString.Replace(@"""", @"""""");
		IBConnectionString = $"\"{IBConnectionString}\"";

		// Запуск экранной клавиатуры
		ProcessUtils.RunProcessNoWait("osk.exe", useShellExecute:true);

		Log.WriteLine("Название базы в 1C: ");
		string baseName = KernelInput.ReadLine();
		baseName.Trim();
		baseName.Replace(" ", "_");
		baseName.Replace("-", "_");

		Log.Write("Логин к базе (если нужен):");
		string baseLogin = KernelInput.ReadLine();
		baseLogin.Trim();

		string basePassword = "";
		if (!string.IsNullOrEmpty(baseLogin))
		{
			Log.Write("Пароль от базы (если нужен):");
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

		while (string.IsNullOrEmpty(foundArchivePath))
		{
			Log.WriteLine("Введите путь к архиву баз 1С (будет создан, если нет): ");
			foundArchivePath = KernelInput.ReadLine();
			if (!Directory.Exists(foundArchivePath))
			{
				Directory.CreateDirectory(foundArchivePath);
			}
		}

		Log.Success($"Найден архив баз 1С {foundArchivePath}");

		string tmpltsPath = Path.Combine(userProfile, "AppData", "Roaming", "1C", "1cv8", "tmplts", "1c");
		if (!Directory.Exists(tmpltsPath))
		{
			Log.WriteLine($"tmplts нет по пути {tmpltsPath}.");
			Log.WriteLine("Вам нужно найти, где находятся темплейты и вставить полный путь");
			Log.WriteLine("Введите путь к шаблонам: ");
			tmpltsPath = KernelInput.ReadLine();
		}

		Log.SkipLine();
		Log.WriteLine("Запустите конфигуратор проверьте в нем версию платформы и конфигурации");
		Log.WriteLine("а после загрузите, что необходимо, в папку /upd1c на рабочем столе без распаковки");
		Log.SkipLine();
		string argumentString = ArgumentUtils.FormArgumentString(ArgumentUtils.ONEC.DESIGNER, IBConnectionString, baseLogin, basePassword);
		Log.WriteLine("Строка запуска (для проверки):");
		Log.WriteLine(string.Format(@"""{0}"" {1}", foundStarterPath, argumentString));
		
		string platformVersionPath = VersionUtils.FindCurrentMaxVersionPath();
		platformVersionPath += @"\bin\1cv8.exe";

		Log.SkipLine();
		Log.WriteLine(platformVersionPath);
		Log.SkipLine();

		Log.Write("Название папки на рабочем столе вместо upd1c, если обновляете больше 1 базы: ");
		string manualUpdatesDir = KernelInput.ReadLine();

		string updatesDir;
		if (string.IsNullOrEmpty(manualUpdatesDir))
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

		Directory.SetCurrentDirectory(updatesDir);

		string updateFilePath = Path.Combine(updatesDir, "update.txt");
		if (File.Exists(updateFilePath))
			File.Delete(updateFilePath);

		Log.WriteLine("Нажмите enter, если папка готова и все обновления находятся в ней");
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

		string currentDate = DateTime.Now.ToString("dd.MM.yyyy");
		Log.WriteLine("Выгружаю информационную базу...");
		string dtFilePath = Path.Combine(foundArchivePath, $"{baseName}_{currentDate}.dt");

		ProcessUtils.RunProcess(platformVersionPath, arguments:$"{argumentString} /DumpIB \"{dtFilePath}\"");

		// ============================================
		// РАСПАКОВКА ОБНОВЛЕНИЙ КОНФИГУРАЦИИ
		// ============================================

		SetState("Распаковка обновлений из папки", "Распаковка...");

		string configurationName = null;
		string configurationVersion = null;
		int fileCounter = 0;

		Log.WriteLine($"Текущая папка: {Directory.GetCurrentDirectory()}");
		Log.WriteLine("Поиск архивов ZIP...");

		// Ожидание, пока появятся zip-файлы
		while (Directory.GetFiles(updatesDir, "*.zip").Length == 0)
		{
			Log.Error($"Архивы обновлений конфигурации .zip не найдены в {updatesDir}");
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
			if (File.Exists(readmePath) && string.IsNullOrEmpty(configurationName))
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

		if (!string.IsNullOrEmpty(configurationName) || fileCounter > 0)
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