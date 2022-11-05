using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает точку входа приложения
	/// </summary>
	public static class TextToKKTProgram
		{
		// Дополнительные методы
		[DllImport ("user32.dll")]
		private static extern IntPtr FindWindow (string lpClassName, String lpWindowName);
		[DllImport ("user32.dll")]
		private static extern int SendMessage (IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		/// <summary>
		/// Главная точка входа для приложения
		/// </summary>
		[STAThread]
		public static void Main (string[] args)
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			// Язык интерфейса и контроль XPR
			if (!Localization.IsXPRClassAcceptable)
				return;

			// Проверка запуска единственной копии (особая обработка)
			bool result;
			Mutex instance = new Mutex (true, ProgramDescription.AssemblyTitle, out result);
			if (!result)
				{
				// Сохранение пути к вызываемому файлу и инициирование его обработки в уже запущенном приложении
				if (args.Length > 0)
					{
					ConfigAccessor.NextDumpPath = args[0];
					IntPtr ptr = FindWindow (null, ProgramDescription.AssemblyVisibleName);
					SendMessage (ptr, ConfigAccessor.NextDumpPathMsg, IntPtr.Zero, IntPtr.Zero);
					}
				else
					{
					MessageBox.Show ("Программа " + ProgramDescription.AssemblyMainName + " уже запущена",
						ProgramDescription.AssemblyTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}

				return;
				}

			// Отображение справки и запроса на принятие Политики
			if (!ProgramDescription.AcceptEULA ())
				return;
			ProgramDescription.ShowAbout (true);

			// Запуск
			if (args.Length > 0)
				Application.Run (new TextToKKTForm (args[0]));
			else
				Application.Run (new TextToKKTForm (""));
			}
		}
	}
