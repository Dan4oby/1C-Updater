using System.Diagnostics;
using System.IO.Compression;
using System.Text; 
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using static PathConstants;


class Program {
	static Program() {

		Console.OutputEncoding = Encoding.UTF8;
		Console.InputEncoding = Encoding.UTF8;

		SetTitleStatus("");
		//Console.BackgroundColor = ConsoleColor.DarkCyan;
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;

		Log.Clear();
		
	}

	static void Main(string[] args) {

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
		Log.Clear();

		SetTitleStatus("Ввод информации для работы");

		bool isMultipleUpdate = true;

		string foundStarterPath = null;
		foreach (string path in possibleStarterPaths)
		{
			if (File.Exists(path))
			{
				foundStarterPath = path;
				break;
			}
		}

		if (foundStarterPath != null)
		{
			Log.Success(string.Format("Найден путь к платформе: {0}", foundStarterPath));
			Log.SkipLine();
		}
		else
		{
			Log.Error("1C не найден ни в одном из путей.");
			Log.SkipLine();
			Pause();
			return;
		}

		Process.Start(foundStarterPath);


		Log.WriteLine("Полностью вставьте строку подключения к информационной базе (снизу): ");
		string IBConnectionString = KernelInput.ReadLine();
		IBConnectionString.Trim();
		IBConnectionString = IBConnectionString.Replace(@"""", @"""""");
		IBConnectionString = $"\"{IBConnectionString}\"";

		// Запуск экранной клавиатуры
		RunProcessWithShellExecute("osk.exe", "");

		Log.WriteLine("Название базы в 1C: ");
		string baseName = KernelInput.ReadLine();
		baseName.Trim();
		baseName.Replace(" ", "_");
		baseName.Replace("-", "_");

		Log.SkipLine();

		Log.Write("Логин к базе (если нужен):");
		string baseLogin = KernelInput.ReadLine();
		baseLogin.Trim();

		string basePassword = null;
		if (!string.IsNullOrEmpty(baseLogin))
		{
			Log.Write("Пароль от базы (если нужен):");
			basePassword = KernelInput.ReadLine();
		}

		string authString = ArgumentUtils.GetAuthString(baseLogin, basePassword);


		Log.Clear();

		Log.WriteLine("Запустите конфигуратор проверьте в нем версию платформы и конфигурации");
		Log.WriteLine("а после загрузите, что необходимо, в папку /upd1c на рабочем столе без распаковки");
		Log.SkipLine();
		string resultString = ArgumentUtils.FormResultString(ArgumentUtils.ONEC.DESIGNER, IBConnectionString, authString);
		Log.WriteLine(string.Format(@"""{0}"" {1}", foundStarterPath, resultString));



		string basePath = "";
		string platformVersionPath = findCurrentMaxVersionPath();
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

		// Удаление файла update.txt, если существует
		string updateFilePath = Path.Combine(updatesDir, "update.txt");
		if (File.Exists(updateFilePath))
			File.Delete(updateFilePath);

		Log.WriteLine("Нажмите enter, если папка готова и все обновления находятся в ней");
		Console.ReadLine();

		Log.Clear();


		// ============================================
		// ПРОВЕРКА СУЩЕСТВОВАНИЯ ПЛАТФОРМЫ (RAR)
		// ============================================
		string[] rarFiles = Directory.GetFiles(updatesDir, "*.rar");
		bool hasRar = rarFiles.Length > 0;

		// ============================================
		// РАСПАКОВКА И УСТАНОВКА ПЛАТФОРМЫ
		// ============================================
		if (hasRar)
		{
			if (string.IsNullOrEmpty(winrarPath) && string.IsNullOrEmpty(path7Zip))
			{
				Log.WriteLine("7z и WinRar не были найдены! Распакуйте платформу сами.");
				Log.WriteLine("Должно получиться \\upd1c\\1c_setup\\");
				Pause();
			}
				
			string setupDir = Path.Combine(updatesDir, "1c_setup");
			if (!Directory.Exists(setupDir))
				Directory.CreateDirectory(setupDir);

			if (!string.IsNullOrEmpty(winrarPath))
			{
				foreach (string rar in rarFiles)
				{
					ProcessStartInfo psi = new ProcessStartInfo
					{
						FileName = winrarPath,
						Arguments = string.Format(@"x -y ""{0}"" ""{1}\""", rar, setupDir),
						UseShellExecute = false,
						CreateNoWindow = true
					};
					using (Process p = Process.Start(psi))
					{
						p.WaitForExit();
					}
				}
			}
			else if (!string.IsNullOrEmpty(path7Zip))
			{
				foreach (string rar in rarFiles)
				{
					ProcessStartInfo psi = new ProcessStartInfo
					{
						FileName = path7Zip,
						Arguments = string.Format(@"x \""{0}\"" -o\""{1}\\"" -y", rar, setupDir),
						UseShellExecute = false,
						CreateNoWindow = true
					};
					using (Process p = Process.Start(psi))
					{
						p.WaitForExit();
					}
				}
			}

			string setupExe = Path.Combine(setupDir, "setup.exe");
			if (File.Exists(setupExe))
			{
				// Пытаемся с тихой установкой
				int exitCode = Utils.RunProcess(setupExe, "/S /l:ru");
				if (exitCode != 0)
				{
					// Если не получилось, запускаем с обычным интерфейсом
					Utils.RunProcess(setupExe, "/l:ru");
				}

				// Запись в лог
				string updateText = string.Format("Обновлена платформа до {0}", platformVersionPath ?? "неизвестной версии");
				File.AppendAllText(updateFilePath, updateText + Environment.NewLine);
				Console.WriteLine(updateText);
			}

			// Удаление временной папки
			try
			{
				Directory.Delete(setupDir, true);
			}
			catch { /* игнорируем */ }
		}

		// ============================================
		// ФОРМИРОВАНИЕ СТРОК ЗАПУСКА ДИЗАЙНЕРА И ПРЕДПРИЯТИЯ
		// ============================================
		string designerLaunchCommand = string.Format("\"{0}\" DESIGNER /F \"{1}\"", foundStarterPath, basePath);
		string enterpriseLaunchCommand = string.Format("\"{0}\" ENTERPRISE /F \"{1}\"", foundStarterPath, basePath);
		if (!string.IsNullOrEmpty(authString))
		{
			designerLaunchCommand += " " + authString;
			enterpriseLaunchCommand += " " + authString;
		}

		string one_s_exe_path = null;
		if (isMultipleUpdate)
		{
			string foundPlatformPath = null;
			if (!string.IsNullOrEmpty(platformVersionPath))
			{
				List<string> possiblePlatformPaths = new List<string>()
				{
					Path.Combine(programFiles, "1cv8", platformVersionPath),
					Path.Combine(programFilesX86, "1cv8", platformVersionPath)
				};

				foreach (string path in possiblePlatformPaths)
				{
					if (Directory.Exists(path))
					{
						foundPlatformPath = path;
						break;
					}
				}

				if (foundPlatformPath == null)
				{
					Console.WriteLine("Введенная версия платформы не была найдена");
				}
			}

			if (foundPlatformPath == null)
			{
				// Ручной ввод пути
				string one_s_path;
				do
				{
					Console.Write("Не была найдена введенная вами версия. Введите путь к директории последней версии платформы сами: ");
					one_s_path = Console.ReadLine();
				} while (!Directory.Exists(one_s_path));
				one_s_exe_path = Path.Combine(one_s_path, "bin", "1cv8.exe");
			}
			else
			{
				one_s_exe_path = Path.Combine(foundPlatformPath, "bin", "1cv8.exe");
			}

			designerLaunchCommand = string.Format("\"{0}\" DESIGNER /F \"{1}\"", one_s_exe_path, basePath);
			enterpriseLaunchCommand = string.Format("\"{0}\" ENTERPRISE /F \"{1}\"", one_s_exe_path, basePath);
			if (!string.IsNullOrEmpty(authString))
			{
				designerLaunchCommand += " " + authString;
				enterpriseLaunchCommand += " " + authString;
			}
		}

		Console.Clear();

		// ============================================
		// ПОИСК ПУТИ К АРХИВУ БАЗ 1С
		// ============================================
		string foundArchivePath = null;
		foreach (string path in one_s_archive)
		{
			if (Directory.Exists(path))
			{
				foundArchivePath = path;
				break;
			}
		}

		if (foundArchivePath == null)
		{
			Console.Write("Введите путь к архиву (будет создан): ");
			foundArchivePath = Console.ReadLine();
			if (!Directory.Exists(foundArchivePath))
			{
				Directory.CreateDirectory(foundArchivePath);
			}
		}
		else
		{
			Console.WriteLine(string.Format("Найден архив баз 1С {0}", foundArchivePath));
		}

		// ============================================
		// ФОРМИРОВАНИЕ ДАТЫ ДЛЯ ИМЕНИ ФАЙЛА
		// ============================================
		string currentDate = DateTime.Now.ToString("dd.MM.yyyy");

		// ============================================
		// ВЫГРУЗКА ИНФОРМАЦИОННОЙ БАЗЫ
		// ============================================
		Console.WriteLine("Выгружаю информационную базу...");
		string dtFilePath = Path.Combine(foundArchivePath, string.Format("{0}_{1}.dt", baseName, currentDate));
		Console.WriteLine(string.Format("{0} /DumpIB \"{1}\"", designerLaunchCommand, dtFilePath));
		
		// Запуск дизайнера для выгрузки
		string fullCommand = designerLaunchCommand + string.Format(" /DumpIB \"{0}\"", dtFilePath);
		RunCommand(fullCommand);

		if (!isMultipleUpdate)
		{
			Console.WriteLine("Ожидание закрытия 1c...");
			WaitForProcessExit("1cv8");
		}

		// ============================================
		// РАСПАКОВКА ОБНОВЛЕНИЙ КОНФИГУРАЦИИ
		// ============================================
		string configurationName = null;
		string configurationVersion = null;
		int count = 0;

		Console.WriteLine(string.Format("Текущая папка: {0}", Directory.GetCurrentDirectory()));
		Console.WriteLine("Поиск архивов ZIP...");

		// Ожидание, пока появятся zip-файлы
		while (Directory.GetFiles(updatesDir, "*.zip").Length == 0)
		{
			Console.WriteLine("Архивы обновлений конфигурации .zip не найдены. Попробуйте исправить ошибку");
			Console.ReadLine();
		}

		string[] zipFiles = Directory.GetFiles(updatesDir, "*.zip");
		Array.Sort(zipFiles);

		foreach (string zipFile in zipFiles)
		{
			count++;
			Console.WriteLine(string.Format("[{0}] Найден архив {1}", count, Path.GetFileName(zipFile)));

			string folder = Path.Combine(updatesDir, count.ToString());
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);

				// Попытка распаковки через ZipFile
				try
				{
					ZipFile.ExtractToDirectory(zipFile, folder);
				}
				catch
				{
					Console.WriteLine(string.Format("ВНИМАНИЕ не удалось распаковать {0} через ZipFile", Path.GetFileName(zipFile)));
					if (!string.IsNullOrEmpty(sevenZipPath))
					{
						Utils.RunProcess(sevenZipPath, string.Format(@"x \""{0}\"" -o\""{1}\\"" -y", zipFile, folder));
					}
					else
					{
						Console.WriteLine("ОШИБКА при попытке распаковки 7z. Попробуйте установить 7z. А после продолжайте");
						Console.ReadLine();
						// Повторный поиск 7z
						if (File.Exists(sevenZipPath1))
							sevenZipPath = sevenZipPath1;
						else if (File.Exists(sevenZipPath2))
							sevenZipPath = sevenZipPath2;

						if (!string.IsNullOrEmpty(sevenZipPath))
						{
							Utils.RunProcess(sevenZipPath, string.Format(@"x \""{0}\"" -o\""{1}\\"" -y", zipFile, folder));
						}
					}
				}
			}

			string setupExe = Path.Combine(folder, "setup.exe");
			if (File.Exists(setupExe))
			{
				Console.WriteLine(string.Format("[{0}] Запуск установки...", count));
				Utils.RunProcess(setupExe, "/S");
			}
			else
			{
				Console.WriteLine(string.Format("[{0}] Файл setup.exe не найден в папке {1}", count, folder));
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

		// ============================================
		// НАСТРОЙКА ПУТИ К ШАБЛОНАМ
		// ============================================
		Console.WriteLine();
		string tmpltsPath = Path.Combine(userProfile, "AppData", "Roaming", "1C", "1cv8", "tmplts", "1c");
		if (!Directory.Exists(tmpltsPath))
		{
			Console.WriteLine(string.Format("tmplts нет по пути {0}.", tmpltsPath));
			Console.WriteLine("Вам нужно найти, где находятся темплейты и вставить полный путь");
			Console.Write("Введите путь к шаблонам: ");
			tmpltsPath = Console.ReadLine();
		}

		// ============================================
		// ЦИКЛ УСТАНОВКИ ОБНОВЛЕНИЙ КОНФИГУРАЦИИ
		// ============================================
		int currentUpdate = 1;
		while (currentUpdate <= count)
		{
			string updateFolder = Path.Combine(updatesDir, currentUpdate.ToString());
			string efdFile = Path.Combine(updateFolder, "1cv8.efd");
			if (File.Exists(efdFile))
			{
				// Открыть папку с шаблонами в проводнике
				try { Process.Start("explorer.exe", tmpltsPath); } catch { }

				Console.WriteLine("Темплейты открылись. Укажите полный путь к папке с установленным релизом:");
				string updatePath = Console.ReadLine();
				Console.WriteLine();

				string cfuFile = Path.Combine(updatePath, "1cv8.cfu");
				if (!File.Exists(cfuFile))
				{
					Console.WriteLine("У введенного релиза не найден файл cfu");
					Console.WriteLine(cfuFile);
					Console.WriteLine("Исправьте ошибку самостоятельно, либо досрочно завершите выполнение программы");
					Console.ReadLine();
					currentUpdate--;
					goto continue_update;
				}

				Console.WriteLine("Обновляю конфигурацию...");
				// Запуск обновления конфигурации
				string updateCommand = designerLaunchCommand + string.Format(" /UpdateCfg \"{0}\" /UpdateDBCfg", cfuFile);
				RunCommand(updateCommand);

				if (!isMultipleUpdate)
				{
					Console.WriteLine("Ожидание закрытия 1c...");
					WaitForProcessExit("1cv8");
				}

				Console.WriteLine(string.Format("Успешно установлена конфигурация {0}", currentUpdate));
				Console.WriteLine();
			}

		continue_update:
			currentUpdate++;
		}

		// Запуск предприятия
		RunCommand(enterpriseLaunchCommand);

		if (!isMultipleUpdate)
		{
			Console.WriteLine("Ожидание закрытия 1c...");
			WaitForProcessExit("1cv8");
		}

		Console.WriteLine("Все обновления установлены");

		// ============================================
		// ЗАПИСЬ ИТОГОВ ОБНОВЛЕНИЯ В ФАЙЛ
		// ============================================
		if (!string.IsNullOrEmpty(configurationName) || count > 0)
		{
			string resultText = string.Format("Обновлена конфигурация «{0}» базы «{1}» до версии {2}",
				configurationName ?? "?", baseName, configurationVersion ?? "?");
			File.AppendAllText(updateFilePath, resultText + Environment.NewLine);
			File.AppendAllText(updateFilePath, string.Format("Количество поставленных релизов: {0}", count) + Environment.NewLine);
			Console.WriteLine(resultText);
		}

		Console.WriteLine("Нажмите любую клавишу для выхода...");
		Console.ReadKey();
	}

	// ============================================
	// ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
	// ============================================

	/// <summary>
	/// Пауза с ожиданием нажатия Enter
	/// </summary>
	static void Pause()
	{
		Console.WriteLine("Нажмите Enter для продолжения...");
		Console.ReadLine();
	}

	/// <summary>
	/// Запуск процесса и ожидание завершения
	/// </summary>


	/// <summary>
	/// Запуск команды (путь в кавычках + аргументы)
	/// </summary>
	static void RunCommand(string fullCommand)
	{
		try
		{
			fullCommand = fullCommand.Trim();
			string fileName;
			string arguments;

			if (fullCommand.StartsWith("\""))
			{
				int endQuote = fullCommand.IndexOf('\"', 1);
				if (endQuote > 0)
				{
					fileName = fullCommand.Substring(1, endQuote - 1);
					arguments = fullCommand.Substring(endQuote + 1).Trim();
				}
				else
				{
					fileName = fullCommand;
					arguments = "";
				}
			}
			else
			{
				int space = fullCommand.IndexOf(' ');
				if (space > 0)
				{
					fileName = fullCommand.Substring(0, space);
					arguments = fullCommand.Substring(space + 1);
				}
				else
				{
					fileName = fullCommand;
					arguments = "";
				}
			}

			ProcessStartInfo psi = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			using (Process p = Process.Start(psi))
			{
				p.WaitForExit();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(string.Format("Ошибка запуска: {0}", ex.Message));
		}
	}

	/// <summary>
	/// Ожидание завершения всех процессов с указанным именем
	/// </summary>
	static void WaitForProcessExit(string processName)
	{
		while (Process.GetProcessesByName(processName).Length > 0)
		{
			Thread.Sleep(1000);
		}
	}
}