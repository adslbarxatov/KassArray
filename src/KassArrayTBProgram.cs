extern alias KassArrayDB;

using System;
using System.Threading;
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
			if (!RDGenerics.IsAppInstanceUnique (false, KassArrayDB::RD_AAOW.ProgramDescription.KassArrayTBAlias))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Программа " + ProgramDescription.AssemblyMainName + KassArrayDB::RD_AAOW.ProgramDescription.KassArrayTBAlias +
					" уже запущена." + RDLocale.RNRN + "Закройте запущенный экземпляр и повторите попытку");

				try
					{
					EventWaitHandle ewh =
						EventWaitHandle.OpenExisting (KassArrayDB::RD_AAOW.ProgramDescription.AssemblyMainName +
						KassArrayDB::RD_AAOW.ProgramDescription.KassArrayTBAlias);
					ewh.Set ();
					}
				catch { }

				return;
				}

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

			// Отдельная обработка расширений, в обход ответа ShowAbout из верхнего приложения
			const string ls = KassArrayDB::RD_AAOW.ProgramDescription.KassArrayTBAlias + RDAboutForm.LastShownVersionKey;
			if (RDGenerics.GetAppRegistryValue (ls) != ProgramDescription.AssemblyVersion)
				{
				RDGenerics.RegisterFileAssociations (true);
				RDGenerics.SetAppRegistryValue (ls, ProgramDescription.AssemblyVersion);
				}

			// Запуск
			if (args.Length > 0)
				Application.Run (new KassArrayTBForm (args[0]));
			else
				Application.Run (new KassArrayTBForm (""));
			}
		}
	}
