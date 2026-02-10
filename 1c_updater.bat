@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: ============================================
:: ИНИЦИАЛИЗАЦИЯ ПЕРЕМЕННЫХ
:: ============================================
set "user=%USERPROFILE%"

set "one_s_lnk=1C Предприятие.lnk"
set "one_s=1cestart.exe"

set "possible_starter_paths[1]=%user%\Desktop\%one_s_lnk%"
set "possible_starter_paths[2]=C:\Program Files\1cv8\common\%one_s%"
set "possible_starter_paths[3]=C:\Program Files (x86)\1cv8\common\%one_s%"
set "possible_starter_paths[4]=%user%\AppData\Microsoft\Windows\Start Menu\Programs\%one_s_lnk%"
set "possible_starter_paths[5]=C:\ProgramData\Microsoft\Windows\Start Menu\Programs\%one_s_lnk%"

set "one_s_archive[0]=C:\Архив 1С"
set "one_s_archive[1]=D:\Архив 1С"
set "one_s_archive[2]=E:\Архив 1С"
set "one_s_archive[3]=C:\Архив 1C"
set "one_s_archive[4]=D:\Архив 1C"
set "one_s_archive[5]=E:\Архив 1C"


:: ============================================
:: ОТОБРАЖЕНИЕ ИНСТРУКЦИЙ
:: ============================================
REM Инструмент автообновления 1С.
REM
REM 1. Введите путь до базы, название базы, путь к директории бэкапов, если она не была найдена
REM 2. Будет запущена платформа в режиме конфигуратор - проверьте её версию и версию конфигурации
REM 3. Выгрузка будет сделана автоматически после закрытия конфигуратора
REM 4. Скачайте с сайта 1с релизы посл. платформу и последовательно версии конфигураций.
REM 5. Положите все скачанное в папку upd1c на рабочем столе (создайте, если нет). 
REM    Версии и платформа должны быть в архивах .zip / .rar в первозданном виде.
REM 6. Автоматически будет обновлена платформа (если есть в папке upd1c), 
REM    а после - последовательно и конфигурации.
REM

:: ============================================
:: ИНФОРМАЦИЯ О ДИСКАХ
:: ============================================
echo.
echo Информация о дисках. Сделайте вывод перед бэкапом:
echo.
start diskmgmt.msc
pause
cls

echo Введите что-нибудь, если обновляете больше одной базы:
set /p "multiple_update="

cls

:: ============================================
:: ПОИСК ПУТЕЙ К ЯРЛЫКУ 1С
:: ============================================
set "found_starter_path="
for /l %%i in (1,1,5) do (
    if exist "!possible_starter_paths[%%i]!" (
        set "found_starter_path=!possible_starter_paths[%%i]!"
        goto :found_starter
    )
)

:found_starter
if defined found_starter_path (
    echo Найден путь к платформе: !found_starter_path!
) else (
    echo 1C не найден ни в одном из путей.
    goto :end
)

"!found_starter_path!"

:: ============================================
:: ВВОД ДАННЫХ ПОЛЬЗОВАТЕЛЕМ
:: ============================================
echo Путь к базе: 
set /p "base_path="
start osk.exe
set /p "base_name=Название базы в 1с:"
set /p "base_login=Логин к базе (если нужен):"
if "!base_login!"=="" (
    set "base_login="
) else (
    set /p "base_password=Пароль от базы (если нужен):"
)

:: ============================================
:: ФОРМИРОВАНИЕ СТРОКИ ПОДКЛЮЧЕНИЯ
:: ============================================
set "connection_string="
if defined base_login (
    set "connection_string=/N "!base_login!""
    if defined base_password (
        set "connection_string=/N "!base_login!" /P "!base_password!""
    )
)
set CONNECT_STR=

:: ============================================
:: ЗАПУСК КОНФИГУРАТОРА ДЛЯ ПРОВЕРКИ
:: ============================================
echo Запустите конфигуратора проверьте в нем версию платформы и конфигурации
echo а после загрузите, что необходимо, в папку /upd1c на рабочем столе без распаковки
echo "!found_starter_path!" DESIGNER /F "!base_path!" "!connection_string!
REM "%found_starter_path%" DESIGNER /F "!base_path!" !connection_string! 

if defined multiple_update (
    echo Версия платформы, которую собираетесь установить, либо версия платформы из справки:
    set /p platform_version=
)

:: ============================================
:: ПОДГОТОВКА К ОБНОВЛЕНИЮ
:: ============================================

echo 

if defined multiple_update (
    echo Название папки на рабочем столе вместо upd1c, если обновляете больше 1 базы
    set /p "manual_updates_dir="
)

if not defined manual_updates_dir (
    set "updates_dir=%user%\Desktop\upd1c"
) else (
    set "updates_dir=%user%\Desktop\%manual_updates_dir%"
)

if not exist "!updates_dir!" (
	mkdir "!updates_dir!"
)

cd /d "!updates_dir!"
del update.txt 2>nul

echo Нажмите enter, если папка готова и все обновления находятся в ней
pause

cls

:: ============================================
:: ПОИСК АРХИВАТОРОВ
:: ============================================
set "7z_path="
if exist "C:\Program Files\7-Zip\7z.exe" (
    set "7z_path=C:\Program Files\7-Zip\7z.exe"
) else if exist "C:\Program Files (x86)\7-Zip\7z.exe" (
    set "7z_path=C:\Program Files (x86)\7-Zip\7z.exe"
)

set "winrar_path="
if exist "C:\Program Files\WinRar\UnRar.exe" (
    set "winrar_path=C:\Program Files\WinRar\UnRar.exe"
) else if exist "C:\Program Files (x86)\WinRar\UnRar.exe" (
    set "winrar_path=C:\Program Files (x86)\WinRar\UnRar.exe"
)

:: ============================================
:: ПРОВЕРКА СУЩЕСТВОВАНИЯ ПЛАТФОРМЫ
:: ============================================
if not exist *.rar (
    goto :launch_command_calculation
)

:: ============================================
:: ПРЕДУПРЕЖДЕНИЕ ЕСЛИ АРХИВАТОРЫ НЕ НАЙДЕНЫ
:: ============================================
if not defined winrar_path (
    if not defined 7z_path (
        echo 7z и WinRar не были найдены! Распакуйте платформу сами.
        echo Должно получиться \upd1c\1c_setup\
        echo Нажмите, чтобы продолжить...
        pause
    )
)

:: ============================================
:: РАСПАКОВКА И УСТАНОВКА ПЛАТФОРМЫ
:: ============================================
if not exist "1c_setup" mkdir "1c_setup"

if defined winrar_path (
    "!winrar_path!" x -y "*.rar" "1c_setup\"
) else if defined 7z_path (
    "!7z_path!" x "*.rar" -o"1c_setup\" -y
)

"1c_setup\setup.exe" /S /l:ru
if !errorlevel! equ 0 (
	echo.
) else (
	"1c_setup\setup.exe" /l:ru
)

echo.
echo Обновлена платформа до %platform_version%>>update.txt

rd /s /q "1c_setup\" 2>nul

echo.
echo Установка новой версии платформы завершена! 
echo.


:: ============================================
:: ВВОД БИНАРНИКА ПЛАТФОРМЫ ПОЛЬЗОВАТЕЛЕМ
:: ============================================

:launch_command_calculation
set "designer_launch_command="!found_starter_path!" DESIGNER /F "!base_path!""
set "enterprise_launch_command="!found_starter_path!" ENTERPRISE /F "!base_path!""
if defined connection_string (
	set "designer_launch_command=!designer_launch_command! !connection_string!"
	set "enterprise_launch_command=!enterprise_launch_command! !connection_string!"
)

if defined multiple_update (
    set "found_platform_path="
	
	echo Версия !platform_version!
    if defined platform_version (
        set "possible_platform_paths[1]=C:\Program Files\1cv8\!platform_version!"
        set "possible_platform_paths[2]=C:\Program Files (x86)\1cv8\!platform_version!"

        for /l %%i in (1,1,5) do (
            if exist "!possible_platform_paths[%%i]!" (
                set "found_platform_path=!possible_platform_paths[%%i]!"
                goto :found_platform
            )
        )
        echo Введенная версия платформы не была найдена
    )
    :input_exe
    echo Не была найдена введенная вами версия. Введите путь к директории последней версии платформы сами: 
    set /p one_s_path=
    if not exist "!one_s_path!" (
        echo Пути !one_s_path! не существует. Попробуйте заново
        goto :input_exe
    )

    :found_platform
    if defined found_platform_path (
        set "one_s_exe_path=!found_platform_path!\bin\1cv8.exe"
    ) else (
        set "one_s_exe_path=!one_s_path!\bin\1cv8.exe"
    )

    set "designer_launch_command="!one_s_exe_path!" DESIGNER /F "!base_path!""
    set "enterprise_launch_command="!one_s_exe_path!" ENTERPRISE /F "!base_path!""
    if defined connection_string (
        set "designer_launch_command=!designer_launch_command! !connection_string!"
        set "enterprise_launch_command=!enterprise_launch_command! !connection_string!"
    )
)
cls
:: ============================================
:: ПОИСК ПУТИ К АРХИВУ БАЗ 1С
:: ============================================

set "found_archive_path="

for /l %%i in (0,1,10) do (
    if exist "!one_s_archive[%%i]!" (
        set "found_archive_path=!one_s_archive[%%i]!"
        goto :archive_found
    )
)

:archive_found
if defined found_archive_path (
    echo Найден архив баз 1С !found_archive_path!
) else (
	echo Введите путь к архиву (будет создан)
    set /p "found_archive_path="
    if not exist "!found_archive_path!" (
        mkdir "!found_archive_path!"
    )
)

:: ============================================
:: ФОРМИРОВАНИЕ ДАТЫ ДЛЯ ИМЕНИ ФАЙЛА
:: ============================================
for /f "tokens=2 delims==." %%a in ('wmic os get localdatetime /value 2^>nul') do set datetime=%%a
if not defined datetime (
    for /f "tokens=1-3 delims=/" %%a in ("%date%") do (
        set "day=%%a"
        set "month=%%b"
        set "year=%%c"
    )
) else (
    set "year=%datetime:~0,4%"
    set "month=%datetime:~4,2%"
    set "day=%datetime:~6,2%"
)
set "date=%day%.%month%.%year%"

:: ============================================
:: ВЫГРУЗКА ИНФОРМАЦИОННОЙ БАЗЫ
:: ============================================
echo Выгружаю информационную базу...
echo !designer_launch_command! /DumpIB "!found_archive_path!\!base_name!_%date%.dt"
start "" /wait !designer_launch_command! /DumpIB "!found_archive_path!\!base_name!_%date%.dt"

if not defined multiple_update (
	timeout /t 5 /nobreak >nul

	echo Ожидание закрытия 1c...
	:wait2
	tasklist /FI "IMAGENAME eq 1cv8.exe" 2>NUL | find /I /N "1cv8.exe" >NUL
	if "%ERRORLEVEL%"=="0" (
		timeout /t 1 /nobreak >nul
		goto :wait2
	)
)

:: ============================================
:: РАСПАКОВКА ОБНОВЛЕНИЙ КОНФИГУРАЦИИ
:: ============================================
set "configuration_name="
set "configuration_version="
set "count=0"

echo Текущая папка: %cd%
echo Поиск архивов ZIP...

:think1
dir *.zip >nul 2>nul
if errorlevel 1 (
    echo Архивы обновлений конфигурации (.zip) не найдены. Попробуйте исправить ошибку
    goto :think1
)

for /f "tokens=*" %%f in ('dir /b /o:d *.zip') do (
    set /a count+=1
    echo [!count!] Найден архив: %%f
    
    set "folder=!count!"
    
    :: Создаем папку, если ее нет
    if not exist "!folder!" (
        mkdir "!folder!"
        
        tar -xf "%%f" -C "!folder!" 2>nul
        if errorlevel 1 (
            echo ВНИМАНИЕ: Не удалось распаковать %%f  tar
			"!7z_path!" x "%%f" -o"!folder!\" -y
			if errorlevel 1 (
				echo ОШИБКА при попытке распаковки 7z. Попробуйте установить 7z. А после продолжайте
				pause
				
				set "7z_path="
				if exist "C:\Program Files\7-Zip\7z.exe" (
					set "7z_path=C:\Program Files\7-Zip\7z.exe"
				) else if exist "C:\Program Files (x86)\7-Zip\7z.exe" (
					set "7z_path=C:\Program Files (x86)\7-Zip\7z.exe"
				)
				"!7z_path!" x "%%f" -o"!folder!\" -y
			)
        )
    )
    
    if exist "!folder!\setup.exe" (
        echo [!count!] Запуск установки...
        "!folder!\setup.exe" /S
    ) else (
        echo [!count!] Файл setup.exe не найден в папке !folder!
    )
    
    :: Читаем информацию из файлов
    if exist "!folder!\ReadMe.txt" (
        for /f "usebackq delims=" %%a in ("!folder!\ReadMe.txt") do (
            if not defined configuration_name set "configuration_name=%%a"
        )
    )
    
    :: Получаем все содержимое VerInfo.txt через пробел, если файл существует
    if exist "!folder!\VerInfo.txt" (
        for /f "usebackq delims=" %%b in ("!folder!\VerInfo.txt") do (
            set "configuration_version=!verinfo_content! %%b"
        )
    )
)

:: ============================================
:: НАСТРОЙКА ПУТИ К ШАБЛОНАМ
:: ============================================
echo.
set "tmplts_path=%user%\AppData\Roaming\1C\1cv8\tmplts\1c"
if not exist "!tmplts_path!" (
    echo tmplts нет по пути !tmplts_path!.
    echo Вам нужно найти, где находятся темплейты и вставить полный путь
	echo Введите путь к шаблонам:
    set /p "tmplts_path=Введите путь к шаблонам: "
)

set "current_update=1"
 
:loop_update
if exist "!current_update!\1cv8.efd" (
    start "" "!tmplts_path!"
    echo Темплейты открылись. Укажите полный путь к папке с установленным релизом:  
    set /p update_path=
    echo.
    
    if not exist "!update_path!\1cv8.cfu" (
        echo У введенного релиза не найден файл cfu
        echo !update_path!\1cv8.cfu
        
        echo Исправьте ошибку самостоятельно, либо досрочно завершите выполнение программы
        
        pause
        set /a current_update-=1
        goto :continue_update
    )
    
    echo Обновляю конфигурацию...
    start "" /wait !designer_launch_command! /UpdateCfg "!update_path!\1cv8.cfu" /UpdateDBCfg

    if not defined multiple_update (
        timeout /t 5 /nobreak >nul

        echo Ожидание закрытия 1c...
        :wait3
        tasklist /FI "IMAGENAME eq 1cv8.exe" 2>NUL | find /I /N "1cv8.exe" >NUL
        if "%ERRORLEVEL%"=="0" (
            timeout /t 1 /nobreak >nul
            goto :wait3
        )
    )
    
    echo Успешно установлена конфигурация !current_update!
    echo.
)

:continue_update
set /a current_update+=1
if !current_update! leq !count! (
    goto :loop_update
)

start "" /wait !enterprise_launch_command!

if not defined multiple_update (
	timeout /t 5 /nobreak >nul

	echo Ожидание закрытия 1c...
	:wait4
	tasklist /FI "IMAGENAME eq 1cv8.exe" 2>NUL | find /I /N "1cv8.exe" >NUL
	if "%ERRORLEVEL%"=="0" (
		timeout /t 1 /nobreak >nul
		goto :wait4
	)
)

echo Все обновления установлены

:: ============================================
:: ЗАПИСЬ ИТОГОВ ОБНОВЛЕНИЯ В ФАЙЛ
:: ============================================
echo Обновлена конфигурация «!configuration_name!» базы «!base_name!» до версии!configuration_version!>>update.txt
echo Количество поставленных релизов: !count!>>update.txt

:end
pause
exit /b

