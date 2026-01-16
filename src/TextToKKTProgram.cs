extern alias KassArrayDB;

using System;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает точку входа приложения
	/// </summary>
	public static class TextToKKTProgram
		{
		/// <summary>
		/// Главная точка входа для приложения
		/// </summary>
		[STAThread]
		public static void Main (string[] args)
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);
			RDLocale.InitEncodings ();

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;


			// Контроль прав и целостности
			if (!RDGenerics.AppHasAccessRights (true, false))
				return;

			if (!RDGenerics.StartedFromMSStore &&
				!RDGenerics.CheckLibrariesExistence (ProgramDescription.AssemblyLibraries, true))
				return;

			// Отображение справки и запроса на принятие Политики
			if (!RDInterface.AcceptEULA ())
				return;
			RDInterface.ShowAbout (true);

			// Запуск
			/*if (args.Length > 0)
				Application.Run (new TextToKKTForm (args[0]));
			else
				Application.Run (new TextToKKTForm (""));*/
			if (args.Length > 0)
				{
				if (args[0] == KassArrayDB::RD_AAOW.KKTSupport.HideWindowKey)
					{
					Application.Run (new TextToKKTForm (true));
					return;
					}
				}

			Application.Run (new TextToKKTForm (false));
			}
		}
	}
