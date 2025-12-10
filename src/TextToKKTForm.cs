extern alias KassArrayDB;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class TextToKKTForm: Form
		{
		// Дескрипторы информационных классов
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;

		// Дескриптор иконки в трее
		private NotifyIcon ni = new NotifyIcon ();

		// Список панелей главного окна
		private List<Panel> tabs = [];
		private List<int> availableTabs = [];

		// Сообщение о применимости ФН
		private string fnLifeMessage = "";

		// Число режимов преобразования
		private uint encodingModesCount;

		// Пересчитанная ширина логотипа для руководств пользователя
		private int manualLogoWidth = 1200;

		/// <summary>
		/// Ключ командной строки, используемый при автозапуске для скрытия главного окна приложения
		/// </summary>
		public const string HideWindowKey = "-h";
		private bool hideWindow = false;
		private bool closeWindowOnError = false;
		private bool closeWindowOnRequest = false;

		private EventWaitHandle ewhTB, ewhFN;
		private bool ewhTBIsActive = true;
		private bool ewhFNIsActive = true;

		#region Главный интерфейс

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		/// <param name="Flags">Флаги запуска приложения</param>
		public TextToKKTForm (string Flags)
			{
			// Инициализация
			InitializeComponent ();
			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			kb = new KassArrayDB::RD_AAOW.KnowledgeBase ();
			hideWindow = (Flags == HideWindowKey);

			try
				{
				ewhTB = new EventWaitHandle (false, EventResetMode.AutoReset,
					KassArrayDB::RD_AAOW.ProgramDescription.AssemblyMainName + "TB");
				}
			catch
				{
				ewhTBIsActive = false;
				}
			try
				{
				ewhFN = new EventWaitHandle (false, EventResetMode.AutoReset,
					KassArrayDB::RD_AAOW.ProgramDescription.AssemblyMainName + "FN");
				}
			catch
				{
				ewhFNIsActive = false;
				}

			if (!RDGenerics.CheckLibrariesVersions (ProgramDescription.AssemblyLibraries, true))
				{
				closeWindowOnError = true;
				return;
				}

			// Сборка структуры страниц
			int pIdx = 0;
			tabs.Add (GuidesPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Руководства");

			tabs.Add (ErrorsPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Коды ошибок");

			tabs.Add (TagsPanel);
			if (AppSettings.EnableExtendedMode)
				{
				availableTabs.Add (pIdx);
				HeadersList.Items.Add ("Теги ФФД");
				}
			pIdx++;

			tabs.Add (FNPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Срок жизни ФН");

			tabs.Add (KKTPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("ЗН и РНМ");

			tabs.Add (OFDPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Настройки ОФД");

			tabs.Add (TermsPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Словарь терминов");

			tabs.Add (CodesPanel);
			if (AppSettings.EnableExtendedMode)
				{
				availableTabs.Add (pIdx);
				HeadersList.Items.Add ("Коды символов");
				}
			pIdx++;

			tabs.Add (ConnectorsPanel);
			if (AppSettings.EnableExtendedMode)
				{
				availableTabs.Add (pIdx);
				HeadersList.Items.Add ("Разъёмы");
				}
			pIdx++;

			tabs.Add (BarcodesPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Штрих-коды");

			tabs.Add (NConvPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Системы счисления");

			tabs.Add (UConvPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Символы Unicode");

			tabs.Add (HConvPanel);
			availableTabs.Add (pIdx++);
			HeadersList.Items.Add ("Двоичные данные");

			tabs.Add (LowLevelPanel);
			if (AppSettings.EnableExtendedMode)
				{
				availableTabs.Add (pIdx);
				HeadersList.Items.Add ("Нижний уровень");
				}
			pIdx++;

			tabs.Add (OthersPanel);
			availableTabs.Add (pIdx);
			HeadersList.Items.Add ("Прочее");

			try
				{
				HeadersList.SelectedIndex = (int)AppSettings.CurrentTab;
				}
			catch
				{
				HeadersList.SelectedIndex = 0;
				}

			// Настройка контролов
			OverrideCloseButton.Checked = AppSettings.OverrideCloseButton;

			KKTListForCodes.Items.AddRange (kb.CodeTables.GetKKTTypeNames ().ToArray ());
			KKTListForCodes.SelectedIndex = 0;

			KKTListForErrors.Items.AddRange (kb.Errors.GetKKTTypeNames ().ToArray ());
			KKTListForErrors.SelectedIndex = 0;

			OFDNamesList.Items.Add ("Неизвестный ОФД");
			OFDNamesList.Items.AddRange (kb.Ofd.GetOFDNames (false).ToArray ());
			OFDNamesList.SelectedIndex = 0;

			CableType.Items.AddRange (kb.Plugs.GetCablesNames ().ToArray ());
			CableType.SelectedIndex = 0;

			this.Text = ProgramDescription.AssemblyVisibleName;

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);

			if (!RDGenerics.AppHasAccessRights (false, false))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);

			TopFlag.Checked = AppSettings.TopMost;
			FNReader.Visible = FNFromFNReader.Visible = OFDFromFNReader.Visible =
				RNMFromFNReader.Visible = !RDGenerics.StartedFromMSStore && AppSettings.EnableExtendedMode; // Уровень 2

			try
				{
				KKTListForErrors.SelectedIndex = (int)AppSettings.KKTForErrors;
				}
			catch
				{
				KKTListForErrors.SelectedIndex = 0;
				}

			ErrorFindButton_Click (ErrorFindNextButton, null);

			FNLifeEvFlags = (KassArrayDB::RD_AAOW.FNLifeFlags)AppSettings.FNLifeEvFlags;
			FNLifeStartDate.Value = DateTime.Now;
			FNLife_Search (ErrorFindNextButton, null);

			RNMUserINN.Text = AppSettings.UserINN;
			RNMValue.Text = AppSettings.RNMKKT;

			RNMSerial_Search (ErrorFindNextButton, null);
			RNMINNUpdate (null, null);

			OFDINN.Text = AppSettings.OFDINN;
			OFDNalogSite.Text = KassArrayDB::RD_AAOW.OFD.FNSSite;
			OFDDNSNameK.Text = KassArrayDB::RD_AAOW.OFD.OKPSite;
			OFDIPK.Text = KassArrayDB::RD_AAOW.OFD.OKPIP;
			OFDPortK.Text = KassArrayDB::RD_AAOW.OFD.OKPPort;
			CDNSite.Text = KassArrayDB::RD_AAOW.OFD.CDNSite;
			LoadOFDParameters ();

			TermSearch_Click (LLFindNextButton, null);

			try
				{
				KKTListForCodes.SelectedIndex = (int)AppSettings.KKTForCodes;
				}
			catch { }
			TextToConvert.Text = AppSettings.CodesText;
			TextToConvert.Items.AddRange (AppSettings.CodesOftenTexts);

			KKTListForManuals.Items.AddRange (kb.UserGuides.GetKKTList ());
			KKTListForManuals.SelectedIndex = (int)AppSettings.KKTForManuals;
			UserManualFlags = (KassArrayDB::RD_AAOW.UserGuidesFlags)AppSettings.UserGuidesFlags;
			UserManualsTipLabel.Text = KassArrayDB::RD_AAOW.UserGuides.UserManualsTip;

			if (AppSettings.EnableExtendedMode) // Уровень 1
				OperationsListForManuals.Items.AddRange (KassArrayDB::RD_AAOW.UserGuides.OperationTypes (false));
			else
				OperationsListForManuals.Items.AddRange (KassArrayDB::RD_AAOW.UserGuides.OperationTypes (true));

			try
				{
				OperationsListForManuals.SelectedIndex = (int)AppSettings.OperationForManuals;
				}
			catch
				{
				OperationsListForManuals.SelectedIndex = 0;
				}

			BarcodeData.MaxLength = (int)KassArrayDB::RD_AAOW.BarCodes.MaxSupportedDataLength;
			BarcodeData.Text = AppSettings.BarcodeData;
			BarcodesConvertToEN_Click (null, null);

			CableType.SelectedIndex = (int)AppSettings.CableType;

			TLV_ObligationBase.Text = KassArrayDB::RD_AAOW.TLVTags.ObligationBase;
			TLVButton_Click (TLVFindNextButton, null);

			EncodingCombo.Items.AddRange (KassArrayDB::RD_AAOW.DataConvertors.AvailableEncodings);
			encodingModesCount = (uint)EncodingCombo.Items.Count / 2;
			EncodingCombo.SelectedIndex = (int)AppSettings.EncodingForConvertor;

			ConvertHexField.Text = AppSettings.ConversionHex;
			ConvertTextField.Text = AppSettings.ConversionText;

			// Блокировка расширенных функций при необходимости
			RNMGenerate.Visible = PrintFullUserManual.Visible = AppSettings.EnableExtendedMode;
			AddManualLogo.Visible = ManualLogo.Visible = AppSettings.EnableExtendedMode &&
				!RDGenerics.StartedFromMSStore;

			if (AppSettings.EnableExtendedMode) // Уровень 2
				{
				RNMTip.Text = KassArrayDB::RD_AAOW.KKTSupport.RNMTip;
				}
			else
				{
				RNMTip.Visible = false;
				RNMLabel.Text = "Укажите регистрационный номер для проверки:";
				FNReader.Enabled = false;
				}

			ConvNumber.Text = AppSettings.ConversionNumber;
			ConvNumber_TextChanged (null, null);

			ConvCode.Text = AppSettings.ConversionCode;
			ConvCode_TextChanged (null, null);

			LowLevelProtocol.Items.AddRange (kb.LLCommands.GetProtocolsNames ().ToArray ());
			LowLevelProtocol.SelectedIndex = 0;
			LowLevelProtocol_SelectedIndexChanged (null, null);
			LowLevelCommand.SelectedIndex = (int)AppSettings.LowLevelCode;

			// Настройка иконки в трее
			ni.Icon = KassArrayResources.KassArrayTray;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenuStrip = new ContextMenuStrip ();
			ni.ContextMenuStrip.ShowImageMargin = false;

			ni.ContextMenuStrip.Items.Add ("Работа с &ФН", null, CallFNReader);
			ni.ContextMenuStrip.Items.Add ("Заявления для ФНС", null, CallTemplateBuilder);
			ni.ContextMenuStrip.Items[0].Enabled = ni.ContextMenuStrip.Items[1].Enabled =
				!RDGenerics.StartedFromMSStore && AppSettings.EnableExtendedMode;
			ni.ContextMenuStrip.Items.Add (RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit), null,
				CloseService);

			ni.MouseDown += ReturnWindow;

			// Настройка расширенного режима
			ExtendedMode.Checked = AppSettings.EnableExtendedMode;
			ExtendedMode.CheckedChanged += ExtendedMode_CheckedChanged;
			}

		private void TextToKKTForm_Shown (object sender, EventArgs e)
			{
			if (hideWindow)
				this.Hide ();
			if (closeWindowOnError)
				this.Close ();
			}

		// Метод обновляет состояние "поверх всех окон"
		private void TMSet (bool State)
			{
			if (State)
				this.TopMost = TopFlag.Checked;
			else
				this.TopMost = false;
			}

		// Включение / выключение режима сервис-инженера
		private void ExtendedMode_CheckedChanged (object sender, EventArgs e)
			{
			TMSet (false);

			if (ExtendedMode.Checked)
				{
				RDInterface.MessageBox (RDMessageFlags.Error | RDMessageFlags.LockSmallSize,
					AppSettings.ExtendedModeMessage);
				AppSettings.EnableExtendedMode = true;
				}
			else
				{
				RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					AppSettings.NoExtendedModeMessage);
				AppSettings.EnableExtendedMode = false;
				}

			TMSet (true);
			}

		// Возврат окна приложения
		private void ReturnWindow (object sender, MouseEventArgs e)
			{
			if (e.Button != MouseButtons.Left)
				return;

			if (this.Visible)
				{
				BExit_Click (null, null);
				}
			else
				{
				this.Show ();
				this.TopMost = true;

				TMSet (true);
				this.WindowState = FormWindowState.Normal;
				}
			}

		// Завершение работы
		private void CloseService (object sender, EventArgs e)
			{
			closeWindowOnRequest = true;
			this.Close ();
			}

		private void BExit_Click (object sender, EventArgs e)
			{
			SaveAppSettings ();
			this.Hide ();
			}

		private void TextToKKMForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Контроль
			if (closeWindowOnError)
				return;

			if (AppSettings.OverrideCloseButton && !closeWindowOnRequest)
				{
				e.Cancel = true;
				BExit_Click (null, null);
				return;
				}

			// Сохранение параметров
			SaveAppSettings ();

			// Завершение
			ni.Visible = false;
			}

		// Сохранение настроек приложения
		private void SaveAppSettings ()
			{
			RDGenerics.SaveWindowDimensions (this);

			AppSettings.TopMost = TopFlag.Checked;
			AppSettings.CurrentTab = (uint)HeadersList.SelectedIndex;

			AppSettings.KKTForErrors = (uint)KKTListForErrors.SelectedIndex;
			AppSettings.FNLifeEvFlags = (uint)FNLifeEvFlags;

			AppSettings.UserINN = RNMUserINN.Text;
			AppSettings.RNMKKT = RNMValue.Text;

			AppSettings.OFDINN = OFDINN.Text;

			AppSettings.KKTForCodes = (uint)KKTListForCodes.SelectedIndex;
			AppSettings.CodesText = TextToConvert.Text;

			List<string> cots = [];
			for (int i = 0; i < TextToConvert.Items.Count; i++)
				cots.Add (TextToConvert.Items[i].ToString ());
			AppSettings.CodesOftenTexts = cots.ToArray ();

			AppSettings.BarcodeData = BarcodeData.Text;
			AppSettings.CableType = (uint)CableType.SelectedIndex;

			AppSettings.KKTForManuals = (uint)KKTListForManuals.SelectedIndex;
			AppSettings.OperationForManuals = (uint)OperationsListForManuals.SelectedIndex;
			AppSettings.UserGuidesFlags = (uint)UserManualFlags;

			AppSettings.ConversionNumber = ConvNumber.Text;
			AppSettings.ConversionCode = ConvCode.Text;
			AppSettings.EncodingForConvertor = (uint)EncodingCombo.SelectedIndex;
			AppSettings.ConversionHex = ConvertHexField.Text;
			AppSettings.ConversionText = ConvertTextField.Text;

			AppSettings.LowLevelProtocol = (uint)LowLevelProtocol.SelectedIndex;
			AppSettings.LowLevelCode = (uint)LowLevelCommand.SelectedIndex;
			}

		// Отображение справки
		private void BHelp_Clicked (object sender, EventArgs e)
			{
			TMSet (false);
			RDInterface.ShowAbout (false);
			TMSet (true);
			}

		// Вызов библиотеки FNReader
		private void FNReader_Click (object sender, EventArgs e)
			{
			ni.ContextMenuStrip.Show (FNReader, Point.Empty);
			}

		private void CallFNReader (object sender, EventArgs e)
			{
			CallSideTool (true);
			}

		private void CallTemplateBuilder (object sender, EventArgs e)
			{
			CallSideTool (false);
			}

		private void CallSideTool (bool FN)
			{
			// Отправка "сообщения" окну модуля работы с ФН
			bool problem;
			if (FN)
				problem = ewhFNIsActive ? ewhFN.WaitOne (100) : true;
			else
				problem = ewhTBIsActive ? ewhTB.WaitOne (100) : true;

			if (!problem)
				{
				if (FN)
					ewhFN.Set ();
				else
					ewhTB.Set ();
				}

			// Контроль на завершение предыдущих процессов
			bool res;
			string proc = ProgramDescription.AssemblyMainName + (FN ? "FN" : "TB");
			if (!problem)
				{
				// Тихо
				res = RDGenerics.KillAllProcesses (proc, false, true);
				}
			else
				{
				// С предупреждением
				TMSet (false);
				res = RDGenerics.KillAllProcesses (proc, true, false);
				TMSet (true);
				}

			// Отмена дальнейших действий, если процесс не был завершён / уже был запущен
			if (!res)
				return;

			// Нормальный запуск модуля работы с ФН
			BExit_Click (null, null);
			string fnExe = RDGenerics.AppStartupPath + proc + ".exe";
			if (!RDGenerics.CheckLibrariesExistence (fnExe, true))
				return;

			RDGenerics.RunURL (fnExe);
			}

		// Переключение состояния "поверх всех окон"
		private void TopFlag_CheckedChanged (object sender, EventArgs e)
			{
			TMSet (true);
			}

		// Получение данных от FNReader
		private void GetFromFNReader_Click (object sender, EventArgs e)
			{
			// Контроль
			string status = "";
			try
				{
				status = File.ReadAllText (KassArrayDB::RD_AAOW.KKTSupport.CreateStatusFileName (null),
					RDGenerics.GetEncoding (RDEncodings.UTF8));
				}
			catch { }

			if (string.IsNullOrWhiteSpace (status))
				{
				TMSet (false);
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Статус ФН ещё не запрашивался или содержит не все требуемые поля", 2000);
				TMSet (true);

				return;
				}

			// Разбор
			string buttonName = ((Button)sender).Name;

			// Раздел параметров ОФД
			switch (buttonName)
				{
				case "OFDFromFNReader":
					OFDINN.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.OFDINN, true);
					if (!string.IsNullOrWhiteSpace (OFDINN.Text))
						LoadOFDParameters ();

					break;

				// Раздел срока жизни ФН
				case "FNFromFNReader":
					AppSettings.FNSerial = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.FNSerialNumber, true);
					if (string.IsNullOrWhiteSpace (AppSettings.FNSerial))
						return;

					if (!string.IsNullOrWhiteSpace (KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.GenericTaxFlag, false)))
						GenericTaxFlag.Checked = true;
					else
						OtherTaxFlag.Checked = true;

					if (string.IsNullOrWhiteSpace (KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.ServiceFlag, false)))
						GoodsFlag.Checked = true;
					else
						ServicesFlag.Checked = true;

					AutonomousFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.AutonomousFlag, false));

					ExciseFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.ExciseFlag, false));

					FFD12Flag.Checked = (KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.FFDVersion, false) == "1.2");

					AgentsFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.AgentFlag, false));

					MarkGoodsFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.MarkingFlag, false));
					PawnInsuranceFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.PawnFlag, false)) ||
						!string.IsNullOrWhiteSpace (KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.InsuranceFlag, false));

					GamblingLotteryFlag.Checked = !string.IsNullOrWhiteSpace
						(KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.GamblingFlag, false)) ||
						!string.IsNullOrWhiteSpace (KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.LotteryFlag, false));
					// Дату и сезонный режим не запрашиваем

					// Триггер поиска
					FNLife_Search (ErrorFindNextButton, null);

					break;

				// Раздел контроля ЗН ККТ, РНМ и ИНН
				case "RNMFromFNReader":
					AppSettings.KKTSerial = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.KKTSerialNumber, true);
					if (string.IsNullOrWhiteSpace (AppSettings.KKTSerial))
						return;

					RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.RegistrationNumber, false);
					if (string.IsNullOrWhiteSpace (RNMValue.Text))
						return;

					RNMUserINN.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
						(KassArrayDB::RD_AAOW.RegTags.UserINN, false);
					if (string.IsNullOrWhiteSpace (RNMUserINN.Text))
						return;

					// Триггер поиска
					RNMSerial_Search (ErrorFindNextButton, null);

					break;
				}
			}

		// Включение / выключение отдельной кнопки сворачивания
		private void OverrideCloseButton_CheckedChanged (object sender, EventArgs e)
			{
			AppSettings.OverrideCloseButton = OverrideCloseButton.Checked;
			}

		// Выбор текущей страницы
		private void HeadersList_SelectedIndexChanged (object sender, EventArgs e)
			{
			int idx = HeadersList.SelectedIndex;
			if (idx < 0)
				return;

			for (int i = 0; i < tabs.Count; i++)
				tabs[i].Visible = (i == availableTabs[idx]);
			}

		#endregion

		#region Коды символов ККТ

		// Изменение текста и его кодировка
		private void TextToConvert_TextChanged (object sender, EventArgs e)
			{
			ResultText.Text = kb.CodeTables.DecodeText ((uint)KKTListForCodes.SelectedIndex, TextToConvert.Text);
			ErrorLabel.Visible = ResultText.Text.Contains (KassArrayDB::RD_AAOW.KKTCodes.EmptyCode);

			LengthLabel.Text = "Длина: " + TextToConvert.Text.Length;
			DescriptionLabel.Text = kb.CodeTables.GetKKTTypeDescription ((uint)KKTListForCodes.SelectedIndex);

			TextToConvertCenter.Enabled = (TextToConvert.Text.Length > 0) &&
				(TextToConvert.Text.Length <= kb.CodeTables.GetKKTStringLength ((uint)KKTListForCodes.SelectedIndex));
			}

		// Выбор ККТ
		private void KKTListForCodes_SelectedIndexChanged (object sender, EventArgs e)
			{
			TextToConvert_TextChanged (null, null);
			}

		// Очистка полей
		private void TextToConvertClear_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDMessageButtons res = RDInterface.MessageBox (RDMessageFlags.Question |
				RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
				"Вы хотите удалить все сохранённые строки или только текущую?", "Текущую", "Все",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel));
			TMSet (true);

			if (res == RDMessageButtons.ButtonTwo)
				TextToConvert.Items.Clear ();
			else if (res == RDMessageButtons.ButtonThree)
				return;

			TextToConvert.Text = "";
			}

		// Запоминание текста из поля ввода
		private void TextToConvertRemember_Click (object sender, EventArgs e)
			{
			TextToConvert.Items.Add (TextToConvert.Text);
			}

		// Центрирование текста на поле ввода ККТ
		private void TextToConvertCenter_Click (object sender, EventArgs e)
			{
			string text = TextToConvert.Text.Trim ();
			uint total = kb.CodeTables.GetKKTStringLength ((uint)KKTListForCodes.SelectedIndex);

			if (text.Length % 2 != 0)
				if (text.Contains (' '))
					text = text.Insert (text.IndexOf (' '), " ");

			TextToConvert.Text = text.PadLeft ((int)(total + text.Length) / 2, ' ');
			}

		#endregion

		#region Коды ошибок ККТ

		// Выбор ошибки
		private void ErrorCodesList_SelectedIndexChanged (object sender, EventArgs e)
			{
			ErrorFindButton_Click (ErrorFindNextButton, null);
			}

		// Поиск по тексту ошибки
		private void ErrorFindButton_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.ErrorCode, "Введите код ошибки или фрагмент её текста", AppSettings.ErrorCodeMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.ErrorCode = search[0];
			ErrorText.Text = kb.Errors.FindNext ((uint)KKTListForErrors.SelectedIndex, search[0], search[1] == "I");
			}

		#endregion

		#region Срок жизни ФН

		// Поиск номера ФН
		private void FNLife_Search (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.FNSerial, "Введите заводской номер ФН или название модели",
				AppSettings.FNSerialMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			// Подмена названия сигнатурой ЗН
			string sig = kb.FNNumbers.FindSignatureByName (search[0]);
			if (!string.IsNullOrWhiteSpace (sig))
				search[0] = sig;

			AppSettings.FNSerial = search[0];

			// Поиск
			if (!string.IsNullOrWhiteSpace (search[0]))
				FNLifeName.Text = kb.FNNumbers.GetFNName (search[0]);
			else
				FNLifeName.Text = "(модель ФН не задана)";

			// Определение длины ключа
			if (!kb.FNNumbers.IsFNKnown (search[0]))
				{
				FNLife36.Enabled = FNLife13.Enabled = true;
				}
			else
				{
				if (kb.FNNumbers.IsNotFor36Months (search[0]))
					{
					FNLife13.Enabled = true;
					FNLife13.Checked = true;
					FNLife36.Enabled = false;
					}
				else
					{
					FNLife36.Enabled = true;
					FNLife36.Checked = true;
					FNLife13.Enabled = false;
					}
				}

			// Принудительное изменение
			FNLifeStartDate_ValueChanged (null, null);
			}

		// Сброс номера ФН
		private void FNLife_Reset (object sender, EventArgs e)
			{
			AppSettings.FNSerial = "";
			FNLife_Search (ErrorFindNextButton, null);
			}

		// Изменение параметров, влияющих на срок жизни ФН
		private void FNLifeStartDate_ValueChanged (object sender, EventArgs e)
			{
			KassArrayDB::RD_AAOW.FNLifeResult res =
				KassArrayDB::RD_AAOW.KKTSupport.GetFNLifeEndDate (FNLifeStartDate.Value, FNLifeEvFlags);

			FNLifeDate.Text = res.DeadLine;
			switch (res.Status)
				{
				case KassArrayDB::RD_AAOW.FNLifeStatus.Inacceptable:
					FNLifeDate.ForeColor = FNLifeStatus.ForeColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);
					fnLifeMessage = KassArrayDB::RD_AAOW.FNSerial.FNIsNotAcceptableMessage;
					break;

				case KassArrayDB::RD_AAOW.FNLifeStatus.Unwelcome:
					FNLifeDate.ForeColor = FNLifeStatus.ForeColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.WarningText);
					fnLifeMessage = KassArrayDB::RD_AAOW.FNSerial.FNIsNotRecommendedMessage;
					break;

				case KassArrayDB::RD_AAOW.FNLifeStatus.Acceptable:
				default:
					FNLifeDate.ForeColor = FNLifeStatus.ForeColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessText);
					fnLifeMessage = KassArrayDB::RD_AAOW.FNSerial.FNIsAcceptableMessage;
					break;

				case KassArrayDB::RD_AAOW.FNLifeStatus.StronglyUnwelcome:
					FNLifeDate.ForeColor = FNLifeStatus.ForeColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionText);
					fnLifeMessage = KassArrayDB::RD_AAOW.FNSerial.FNIsStronglyUnwelcomeMessage;
					break;
				}

			if (kb.FNNumbers.IsFNKnown (AppSettings.FNSerial))
				{
				if (!kb.FNNumbers.IsFNAllowed (AppSettings.FNSerial))
					{
					FNLifeDate.ForeColor = FNLifeStatus.ForeColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);

					fnLifeMessage += (RDLocale.RN + KassArrayDB::RD_AAOW.FNSerial.FNIsNotAllowedMessage);
					FNLifeName.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					}
				else
					{
					FNLifeName.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					}
				}
			else
				{
				FNLifeName.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				}
			}

		// Копирование срока действия ФН
		private void FNLifeDate_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDGenerics.SendToClipboard (FNLifeDate.Text, true);
			TMSet (true);
			}

		// Отображение сообщения о применимости ФН
		private void FNLifeStatus_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDInterface.MessageBox (RDMessageFlags.Information | RDMessageFlags.NoSound, fnLifeMessage);
			TMSet (true);
			}

		// Статистика по базе ЗН ККТ
		private void FNLifeStats_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDInterface.MessageBox (RDMessageFlags.Information | RDMessageFlags.NoSound,
				kb.FNNumbers.RegistryStats);
			TMSet (true);
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для расчёта срока жизни ФН
		/// </summary>
		private KassArrayDB::RD_AAOW.FNLifeFlags FNLifeEvFlags
			{
			get
				{
				KassArrayDB::RD_AAOW.FNLifeFlags flags = 0;

				if (FNLife13.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.FN15;
				if (kb.FNNumbers.IsFNExactly13 (AppSettings.FNSerial))
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.FNExactly13;
				if (GenericTaxFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.GenericTax;
				if (GoodsFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.Goods;
				if (SeasonFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.Season;
				if (AgentsFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.Agents;
				if (ExciseFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.Excise;
				if (AutonomousFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.Autonomous;
				if (FFD12Flag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.FFD12;
				if (GamblingLotteryFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.GamblingAndLotteries;
				if (PawnInsuranceFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.PawnsAndInsurance;
				if (MarkGoodsFlag.Checked)
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.MarkGoods;

				if (!kb.FNNumbers.IsFNKnown (AppSettings.FNSerial) ||
					kb.FNNumbers.IsFNCompletelySupportingFFD12 (AppSettings.FNSerial))
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.FFD12FullSupport;
				if (!kb.FNNumbers.IsFNKnown (AppSettings.FNSerial) ||
					kb.FNNumbers.IsFNCompatibleWithFFD12 (AppSettings.FNSerial))
					flags |= KassArrayDB::RD_AAOW.FNLifeFlags.FFD12Compatible;
				// Признак распознанного ЗН ФН

				return flags;
				}
			set
				{
				FNLife13.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.FN15);
				FNLife36.Checked = !FNLife13.Checked;
				// .Exactly13 – вспомогательный нехранимый флаг
				GenericTaxFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.GenericTax);
				OtherTaxFlag.Checked = !GenericTaxFlag.Checked;
				GoodsFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.Goods);
				ServicesFlag.Checked = !GoodsFlag.Checked;
				SeasonFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.Season);
				AgentsFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.Agents);
				ExciseFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.Excise);
				AutonomousFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.Autonomous);
				FFD12Flag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.FFD12);
				GamblingLotteryFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.GamblingAndLotteries);
				PawnInsuranceFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.PawnsAndInsurance);
				MarkGoodsFlag.Checked = value.HasFlag (KassArrayDB::RD_AAOW.FNLifeFlags.MarkGoods);
				// .MarkFN – вспомогательный нехранимый флаг
				}
			}

		#endregion

		#region РНМ и ЗН

		// Поиск модели ККТ
		private void RNMSerial_Search (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.KKTSerial, "Введите заводской номер ККТ или название модели",
				kb.KKTNumbers.MaxSerialNumberLength);
			TMSet (true);

			if (search[1] == "C")
				return;
			AppSettings.KKTSerial = search[0];

			// Подмена названия сигнатурой ЗН
			string sig = kb.KKTNumbers.FindSignatureByName (search[0], search[1] == "I");
			if (!string.IsNullOrWhiteSpace (sig))
				search[0] = sig;

			// Заводской номер ККТ
			if (!string.IsNullOrWhiteSpace (search[0]))
				RNMSerialResult.Text = "Модель ККТ: " + kb.KKTNumbers.GetKKTModel (search[0]);
			else
				RNMSerialResult.Text = "(модель ККТ не найдена)";

			// Остальные поля
			RNMRNMUpdate (null, null);
			}

		// Обновление полей ИНН и РНМ
		private void RNMINNUpdate (object sender, EventArgs e)
			{
			// ИНН пользователя
			RegionLabel.Text = "";
			int checkINN = KassArrayDB::RD_AAOW.KKTSupport.CheckINN (RNMUserINN.Text);
			if (checkINN < 0)
				RNMUserINN.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
			else if (checkINN == 0)
				RNMUserINN.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
			else
				RNMUserINN.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.WarningMessage);
			// Не ошибка

			RegionLabel.Text = kb.KKTNumbers.GetRegionName (RNMUserINN.Text);

			RNMRNMUpdate (null, null);
			}

		private void RNMRNMUpdate (object sender, EventArgs e)
			{
			// РН
			if (RNMValue.Text.Length < 10)
				RNMValue.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
			else if (KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text, AppSettings.KKTSerial,
				RNMValue.Text.Substring (0, 10)) == RNMValue.Text)
				RNMValue.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
			else
				RNMValue.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
			}

		// Генерация регистрационного номера
		private void RNMGenerate_Click (object sender, EventArgs e)
			{
			if (RNMValue.Text.Length < 1)
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					AppSettings.KKTSerial, "0");
			else if (RNMValue.Text.Length < 10)
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					AppSettings.KKTSerial, RNMValue.Text);
			else
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					AppSettings.KKTSerial, RNMValue.Text.Substring (0, 10));

			TMSet (false);
			if (string.IsNullOrWhiteSpace (RNMValue.Text))
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Убедитесь, что заводской номер ККТ и ИНН заполнены корректно", 2000);
			TMSet (true);
			}

		// Статистика по базе ЗН ККТ
		private void RNMStats_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDInterface.MessageBox (RDMessageFlags.Information | RDMessageFlags.NoSound,
				kb.KKTNumbers.RegistryStats);
			TMSet (true);
			}

		// Информация о модели ККТ
		private void RNMInfo_Click (object sender, EventArgs e)
			{
			TMSet (false);

			if (!kb.KKTNumbers.LastKKTSearchResult)
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Модель ККТ не найдена", 1000);
			else
				RDInterface.MessageBox (RDMessageFlags.Information | RDMessageFlags.NoSound,
					kb.KKTNumbers.GetKKTDescription ());

			TMSet (true);
			}

		#endregion

		#region ОФД

		// Выбор имени ОФД
		private void OFDNamesList_SelectedIndexChanged (object sender, EventArgs e)
			{
			string s = kb.Ofd.GetOFDINNByName (OFDNamesList.Text);
			OFDINN.Text = string.IsNullOrWhiteSpace (s) ? "" : s;
			LoadOFDParameters ();
			}

		// Выбор ОФД
		private void LoadOFDParameters ()
			{
			List<string> parameters = kb.Ofd.GetOFDParameters (OFDINN.Text.Contains ("0000000000") ?
				"" : OFDINN.Text);

			OFDNamesList.Text = parameters[1];

			OFDDNSName.Text = parameters[2];
			OFDIP.Text = parameters[3];
			OFDPort.Text = parameters[4];
			OFDEmail.Text = parameters[5];
			OFDSite.Text = parameters[6];

			OFDDNSNameM.Text = parameters[7];
			OFDIPM.Text = parameters[8];
			OFDPortM.Text = parameters[9];

			// Обработка аннулированных ОФД
			// Enabled нужен потому, что Visible на стадии загрузки окна принимает значение false,
			// несмотря на то, что в коде инициализации имеет значение true (фактически не виден)
			OFDDisabledLabel.Enabled = OFDDisabledLabel.Visible = !string.IsNullOrWhiteSpace (parameters[10]);
			OFDIP.Enabled = OFDDNSName.Enabled = OFDPort.Enabled =
				OFDIPM.Enabled = OFDDNSNameM.Enabled = OFDPortM.Enabled = !OFDDisabledLabel.Enabled;
			if (OFDDisabledLabel.Enabled)
				OFDDisabledLabel.Text = parameters[10];
			}

		// Копирование в буфер обмена
		private void SendButtonTextToClipboard (object sender, EventArgs e)
			{
			TMSet (false);
			RDGenerics.SendToClipboard (((Button)sender).Text, true);
			TMSet (true);
			}

		private void OFDNameCopy_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDGenerics.SendToClipboard (OFDNamesList.Text.Replace ('«', '\"').Replace ('»', '\"'), true);
			TMSet (true);
			}

		private void OFDINNCopy_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDGenerics.SendToClipboard (OFDINN.Text, true);
			TMSet (true);
			}

		// Поиск по названию ОФД
		private void OFDFindButton_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.OFDSearch, "Введите ИНН ОФД или фрагмент его названия",
				AppSettings.OFDSearchMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.OFDSearch = search[0];
			int idx = kb.Ofd.FindNext (search[0], search[1] == "I");
			if (idx < 0)
				OFDNamesList.SelectedIndex = 0;
			else
				OFDNamesList.SelectedIndex = idx + 1;
			}

		#endregion

		#region Словарь терминов

		// Поиск по тексту ошибки
		private void TermSearch_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.DictionarySearch, "Введите термин или его сокращение",
				AppSettings.DictionarySearchMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.DictionarySearch = search[0];
			string res = kb.Dictionary.FindNext (search[0], search[1] == "I");

			int idx = res.IndexOf ('\x1');
			if (idx < 0)
				{
				LLLabel.Text = res;
				LLDescription.Text = "";
				}
			else
				{
				string[] values = res.Split (['\x1']);
				LLLabel.Text = values[0];
				LLDescription.Text = values[1];
				}
			}

		#endregion

		#region Руководства пользователя

		// Выбор модели аппарата
		private void KKTListForManuals_SelectedIndexChanged (object sender, EventArgs e)
			{
			if (OperationsListForManuals.SelectedIndex < 0)
				return;
			byte idx = (byte)OperationsListForManuals.SelectedIndex;

			AddToPrint.Checked = ((AppSettings.UserGuidesSectionsState & (1u << idx)) != 0);

			UMOperationText.Text = kb.UserGuides.GetGuide ((uint)KKTListForManuals.SelectedIndex,
				(KassArrayDB::RD_AAOW.UserGuidesTypes)idx, UserManualFlags);
			}

		// Печать инструкции
		private void PrintUserManual_Click (object sender, EventArgs e)
			{
			// Сборка задания на печать
			KassArrayDB::RD_AAOW.UserGuidesFlags flags = UserManualFlags;
			if (((Button)sender).Name == PrintUserManualForCashier.Name)
				flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.GuideForCashier;

			string text = KassArrayDB::RD_AAOW.KKTSupport.BuildUserGuide (kb.UserGuides,
				(uint)KKTListForManuals.SelectedIndex, AppSettings.UserGuidesSectionsState, flags);

			// Печать
			TMSet (false);
			string s = KassArrayDB::RD_AAOW.KKTSupport.PrintText (text, KassArrayDB::RD_AAOW.PrinterTypes.ManualA4,
				KassArrayDB::RD_AAOW.PrinterFlags.None);

			if (!string.IsNullOrWhiteSpace (s))
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Не удалось распечатать документ" + RDLocale.RN + s);
			else
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					"Задание отправлено на печать", 700);

			TMSet (true);
			}

		// Выбор компонентов инструкции
		private void AddToPrint_CheckedChanged (object sender, EventArgs e)
			{
			uint sections = AppSettings.UserGuidesSectionsState;
			uint idx = (1u << (byte)OperationsListForManuals.SelectedIndex);

			if (AddToPrint.Checked)
				sections |= idx;
			else
				sections &= ~idx;

			AppSettings.UserGuidesSectionsState = sections;
			}

		// Выбор лого для руководства пользователя
		private void SetUserManualLogo (object sender, EventArgs e)
			{
			// Попытка загрузки исходного изображения
			if (OFDialog.ShowDialog () != DialogResult.OK)
				return;

			Bitmap b;
			try
				{
				b = (Bitmap)Image.FromFile (OFDialog.FileName);
				}
			catch
				{
				TMSet (false);
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					"Выбранный файл недоступен или не является поддерживаемым файлом изображения");
				TMSet (true);

				return;
				}

			// Преобразование и сохранение
			Bitmap b2 = new Bitmap (b, manualLogoWidth, manualLogoWidth * b.Height / b.Width);
			b2.SetResolution (1000, 1000);
			b.Dispose ();

			try
				{
				b2.Save (RDGenerics.AppStartupPath + KassArrayDB::RD_AAOW.KKTSupport.ManualLogoFileName,
					System.Drawing.Imaging.ImageFormat.Png);
				}
			catch
				{
				TMSet (false);
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					KassArrayDB::RD_AAOW.KKTSupport.ManualLogoFileName));
				TMSet (true);

				b2.Dispose ();
				return;
				}
			b2.Dispose ();

			// Успешно
			TMSet (false);
			RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText,
				"Логотип успешно добавлен", 1000);
			TMSet (true);

			AddManualLogo.Checked = true;
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для руководства пользователя
		/// </summary>
		private KassArrayDB::RD_AAOW.UserGuidesFlags UserManualFlags
			{
			get
				{
				KassArrayDB::RD_AAOW.UserGuidesFlags flags = 0;

				if (MoreThanOneItemPerDocument.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.MoreThanOneItemPerDocument;
				if (ProductBaseContainsPrices.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.ProductBaseContainsPrices;
				if (CashiersHavePasswords.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.CashiersHavePasswords;
				if (BaseContainsServices.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.ProductBaseContainsServices;
				if (DocumentsContainMarks.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.DocumentsContainMarks;
				if (BaseContainsSingleItem.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.BaseContainsSingleItem;
				if (AddManualLogo.Checked)
					flags |= KassArrayDB::RD_AAOW.UserGuidesFlags.AddManualLogo;

				return flags;
				}
			set
				{
				MoreThanOneItemPerDocument.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.MoreThanOneItemPerDocument);
				ProductBaseContainsPrices.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.ProductBaseContainsPrices);
				CashiersHavePasswords.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.CashiersHavePasswords);
				BaseContainsServices.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.ProductBaseContainsServices);
				DocumentsContainMarks.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.DocumentsContainMarks);
				BaseContainsSingleItem.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.BaseContainsSingleItem);
				AddManualLogo.Checked =
					value.HasFlag (KassArrayDB::RD_AAOW.UserGuidesFlags.AddManualLogo);
				}
			}

		#endregion

		#region TLV-теги

		// Поиск TLV-тега
		private void TLVButton_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.TLVData, "Введите номер TLV-тега или фрагмент его описания",
				AppSettings.TLVDataMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.TLVData = search[0];

			if (kb.Tags.FindTag (search[0]))
				{
				TLVDescription.Text = kb.Tags.LastDescription;
				TLVType.Text = kb.Tags.LastType;

				if (!string.IsNullOrWhiteSpace (kb.Tags.LastValuesSet))
					TLVValues.Text = kb.Tags.LastValuesSet;
				else
					TLVValues.Text = "";

				TLVValues.Text += kb.Tags.LastObligation;
				}
			else
				{
				TLVDescription.Text = TLVType.Text = TLVValues.Text = "(не найдено)";
				}
			}

		// Ссылка на приказ-обоснование обязательности тегов
		private void TLVObligationBase_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDMessageButtons b = RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.CenterText,
				"Скопировать основание или перейти к его тексту?",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Copy),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_GoTo),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel));

			switch (b)
				{
				case RDMessageButtons.ButtonOne:
					RDGenerics.SendToClipboard (TLV_ObligationBase.Text, true);
					break;

				case RDMessageButtons.ButtonTwo:
					RDGenerics.RunURL (KassArrayDB::RD_AAOW.TLVTags.ObligationBaseLink);
					break;
				}

			TMSet (true);
			}

		// Выбор родительского тега для перехода
		private void TLVFindFromParents_Click (object sender, EventArgs e)
			{
			// Контроль
			string[] parents = kb.Tags.LastParents;
			if (parents.Length < 1)
				{
				TMSet (false);
				RDInterface.MessageBox (RDMessageFlags.CenterText | RDMessageFlags.Warning,
					"Тег входит в состав документа напрямую" + RDLocale.RN +
					"(не является частью других тегов)");
				TMSet (true);

				return;
				}

			ContextMenuStrip cms = new ContextMenuStrip ();
			cms.ShowImageMargin = false;
			for (int i = 0; i < parents.Length; i++)
				cms.Items.Add (parents[i], null, TLVFindFromParentsMenu_Click);

			cms.Show (TLVFindFromParents, new Point (0, TLVFindFromParents.Height));
			}

		private void TLVFindFromParentsMenu_Click (object sender, EventArgs e)
			{
			AppSettings.TLVData = ((ToolStripMenuItem)sender).Text;
			TLVButton_Click (TLVFindNextButton, null);
			}

		#endregion

		#region Штрих-коды

		// Преобразование из русской раскладки
		private void BarcodesConvertToEN_Click (object sender, EventArgs e)
			{
			BarcodeData.Text = KassArrayDB::RD_AAOW.BarCodes.ConvertFromRussianKeyboard (BarcodeData.Text);
			BarcodeDescription.Text = kb.Barcodes.GetBarcodeDescription (BarcodeData.Text);
			}

		private void BarcodeData_KeyUp (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				BarcodesConvertToEN_Click (sender, null);
			}

		// Сброс текста
		private void BarcodeClear_Click (object sender, EventArgs e)
			{
			BarcodeData.Text = "";
			BarcodesConvertToEN_Click (sender, null);
			}

		#endregion

		#region Распайки кабелей

		// Выбор типа кабеля
		private void CableType_SelectedIndexChanged (object sender, EventArgs e)
			{
			uint i = (uint)CableType.SelectedIndex;

			CableLeftSide.Text = "Со стороны " + kb.Plugs.GetCableConnector (i, false);
			CableLeftPins.Text = kb.Plugs.GetCableConnectorPins (i, false);

			CableRightSide.Text = "Со стороны " + kb.Plugs.GetCableConnector (i, true);
			CableRightPins.Text = kb.Plugs.GetCableConnectorPins (i, true);

			CableLeftDescription.Text = kb.Plugs.GetCableConnectorDescription (i, false) + RDLocale.RNRN +
				kb.Plugs.GetCableConnectorDescription (i, true);
			}

		// Поиск по тексту ошибки
		private void ConnFind_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.CableSearch, "Введите тип или назначение распиновки",
				AppSettings.CableSearchMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.CableSearch = search[0];

			int idx = kb.Plugs.FindNext (search[0], search[1] == "I");
			if (idx < 0)
				{
				CableLeftSide.Text = "(описание не найдено)";
				CableLeftPins.Text = CableRightPins.Text = CableLeftDescription.Text = CableRightSide.Text = "";
				}
			else
				{
				CableType.SelectedIndex = idx;
				}
			}

		#endregion

		#region Конверторы

		// Преобразование систем счисления
		private void ConvNumber_TextChanged (object sender, EventArgs e)
			{
			ConvNumberResult.Text = KassArrayDB::RD_AAOW.DataConvertors.GetNumberDescription (ConvNumber.Text);
			}

		private void ConvNumberAdd_Click (object sender, EventArgs e)
			{
			// Извлечение значения с защитой
			bool plus = ((Button)sender).Text.Contains ('+');
			double res = KassArrayDB::RD_AAOW.DataConvertors.GetNumber (ConvNumber.Text);
			if (double.IsNaN (res))
				res = 0.0;

			// Обновление и возврат
			if (plus)
				{
				if (res < KassArrayDB::RD_AAOW.DataConvertors.MaxValue)
					res += 1.0;
				else
					res = KassArrayDB::RD_AAOW.DataConvertors.MaxValue;
				}
			else
				{
				if (res > 0.0)
					res -= 1.0;
				else
					res = 0.0;
				}

			ConvNumber.Text = ((uint)res).ToString ();
			}

		private void ConvNumber_KeyDown (object sender, KeyEventArgs e)
			{
			switch (e.KeyCode)
				{
				case Keys.Up:
					ConvNumberAdd_Click (ConvNumberInc, null);
					break;

				case Keys.Down:
					ConvNumberAdd_Click (ConvNumberDec, null);
					break;
				}
			}

		private void ConvNumberClearButton_Click (object sender, EventArgs e)
			{
			ConvNumber.Text = "";
			}

		// Преобразование символов Unicode
		private void ConvCode_TextChanged (object sender, EventArgs e)
			{
			string[] res = KassArrayDB::RD_AAOW.DataConvertors.GetSymbolDescription (ConvCode.Text, 0, kb.Unicodes);
			ConvCodeSymbol.Text = res[0];
			ConvCodeResult.Text = res[1];
			}

		private void ConvCodeAdd_Click (object sender, EventArgs e)
			{
			bool plus = ((Button)sender).Name == ConvCodeInc.Name;
			string[] res = KassArrayDB::RD_AAOW.DataConvertors.GetSymbolDescription (ConvCode.Text,
				(short)(plus ? 1 : -1), kb.Unicodes);
			ConvCode.Text = res[2];
			}

		private void ConvCode_KeyDown (object sender, KeyEventArgs e)
			{
			switch (e.KeyCode)
				{
				case Keys.Up:
					ConvCodeAdd_Click (ConvCodeInc, null);
					break;

				case Keys.Down:
					ConvCodeAdd_Click (ConvCodeDec, null);
					break;
				}
			}

		private void ConvCodeClearButton_Click (object sender, EventArgs e)
			{
			ConvCode.Text = "";
			}

		private void ConvCodeCopy_Click (object sender, EventArgs e)
			{
			TMSet (false);
			RDGenerics.SendToClipboard (ConvCodeSymbol.Text, true);
			TMSet (true);
			}

		// Поиск диапазона символов Unicode
		private void ConvCodeFind_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.ConversionCodeSearch, "Введите название или часть названия блока символов Unicode",
				AppSettings.ConversionCodeSearchMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.ConversionCodeSearch = search[0];
			ulong left = kb.Unicodes.FindRange (search[0]);
			if (left != 0)
				{
				string[] res = KassArrayDB::RD_AAOW.DataConvertors.GetSymbolDescription ("\x0",
					(long)left, kb.Unicodes);
				ConvCode.Text = res[2];
				}
			}

		// Преобразование hex-данных в текст
		private void ConvertHexToText_Click (object sender, EventArgs e)
			{
			ConvertTextField.Text = KassArrayDB::RD_AAOW.DataConvertors.ConvertHexToText (ConvertHexField.Text,
				(KassArrayDB::RD_AAOW.RDEncodings)(EncodingCombo.SelectedIndex % encodingModesCount),
				EncodingCombo.SelectedIndex >= encodingModesCount);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			ConvertHexField.Text = KassArrayDB::RD_AAOW.DataConvertors.ConvertTextToHex (ConvertTextField.Text,
				(KassArrayDB::RD_AAOW.RDEncodings)(EncodingCombo.SelectedIndex % encodingModesCount),
				EncodingCombo.SelectedIndex >= encodingModesCount);
			}

		// Очистка вкладки преобразования данных
		private void ClearConvertText_Click (object sender, EventArgs e)
			{
			ConvertTextField.Text = "";
			ConvertHexField.Text = "";
			}

		#endregion

		#region Команды нижнего уровня

		// Выбор протокола
		private void LowLevelProtocol_SelectedIndexChanged (object sender, EventArgs e)
			{
			LowLevelCommand.Items.Clear ();
			LowLevelCommand.Items.AddRange (kb.LLCommands.GetCommandsList ((uint)LowLevelProtocol.SelectedIndex).ToArray ());
			LowLevelCommand.SelectedIndex = 0;
			}

		// Выбор команды
		private void LowLevelCommand_SelectedIndexChanged (object sender, EventArgs e)
			{
			LowLevelCommandCode.Text = kb.LLCommands.GetCommand ((uint)LowLevelProtocol.SelectedIndex,
				(uint)LowLevelCommand.SelectedIndex, false);
			LowLevelCommandDescr.Text = kb.LLCommands.GetCommand ((uint)LowLevelProtocol.SelectedIndex,
				(uint)LowLevelCommand.SelectedIndex, true);
			}

		// Поиск команды
		// Поиск по тексту ошибки
		private void LowLevelSearch_Click (object sender, EventArgs e)
			{
			// Определение запроса
			TMSet (false);
			string[] search = KassArrayDB::RD_AAOW.KKTSupport.ObtainSearchCriteria (((Button)sender).Name,
				AppSettings.LowLevelSearch, "Введите описание или фрагмент описания команды",
				AppSettings.LowLevelSearchMaxLength);
			TMSet (true);

			if (search[1] == "C")
				return;

			AppSettings.LowLevelSearch = search[0];
			int idx = kb.LLCommands.FindNext (AppSettings.LowLevelProtocol, search[0], search[1] == "I");
			if (idx < 0)
				{
				LowLevelCommandDescr.Text = LowLevelCommandCode.Text = "(описание не найдено)";
				return;
				}

			LowLevelCommand.SelectedIndex = idx;
			}

		#endregion
		}
	}
