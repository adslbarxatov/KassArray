extern alias KassArrayDB;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		private ConfigAccessor ca = null;
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;

		// Дескриптор иконки в трее
		private NotifyIcon ni = new NotifyIcon ();

		// Рассчитанный срок жизни ФН
		private string fnLifeResult = "";

		// Ссылки на текущие смещения в списках поиска
		private int lastErrorSearchOffset = 0;
		private int lastOFDSearchOffset = 0;
		private int lastLowLevelSearchOffset = 0;
		private int lastConnSearchOffset = 0;

		// Число режимов преобразования
		private uint encodingModesCount;

		/// <summary>
		/// Ключ командной строки, используемый при автозапуске для скрытия главного окна приложения
		/// </summary>
		public const string HideWindowKey = "-h";
		private bool hideWindow = false;
		private bool closeWindowOnError = false;
		private bool closeWindowOnRequest = false;

		private EventWaitHandle ewh;
		private bool ewhIsActive = true;

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

			ca = new ConfigAccessor ();
			kb = new KassArrayDB::RD_AAOW.KnowledgeBase ();
			hideWindow = (Flags == HideWindowKey);

			try
				{
				ewh = new EventWaitHandle (false, EventResetMode.AutoReset,
					KassArrayDB::ProgramDescription.AssemblyTitle);
				}
			catch
				{
				ewhIsActive = false;
				}

			if (KassArrayDB::ProgramDescription.GetCurrentAssemblyVersion != ProgramDescription.AssemblyVersion)
				{
				RDGenerics.MessageBox (RDMessageTypes.Error_Center,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.MessageFormat_WrongVersion_Fmt),
					ProgramDescription.KassArrayDLLs[0]));

				closeWindowOnError = true;
				return;
				}

			OverrideCloseButton.Checked = ca.OverrideCloseButton;

			// Настройка контролов
			KKTListForCodes.Items.AddRange (kb.CodeTables.GetKKTTypeNames ().ToArray ());
			KKTListForCodes.SelectedIndex = 0;

			KKTListForErrors.Items.AddRange (kb.Errors.GetKKTTypeNames ().ToArray ());
			KKTListForErrors.SelectedIndex = 0;

			LowLevelProtocol.Items.AddRange (kb.LLCommands.GetProtocolsNames ().ToArray ());
			LowLevelProtocol.SelectedIndex = (int)ca.LowLevelProtocol;
			LowLevelProtocol_CheckedChanged (null, null);

			FNLifeStartDate.Value = DateTime.Now;

			OFDNamesList.Items.Add ("Неизвестный ОФД");
			OFDNamesList.Items.AddRange (kb.Ofd.GetOFDNames ().ToArray ());
			OFDNamesList.SelectedIndex = 0;

			CableType.Items.AddRange (kb.Plugs.GetCablesNames ().ToArray ());
			CableType.SelectedIndex = 0;

			this.Text = ProgramDescription.AssemblyVisibleName;

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);

			if (!RDGenerics.AppHasAccessRights (false, true))
				{
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);
				KeepAppState.Checked = KeepAppState.Enabled = false;
				}
			else
				{
				KeepAppState.Checked = ca.KeepApplicationState;
				}

			TopFlag.Checked = ca.TopMost;
			FNReader.Visible = FNFromFNReader.Visible = OFDFromFNReader.Visible =
				RNMFromFNReader.Visible = !RDGenerics.StartedFromMSStore && ca.EnableExtendedMode;	// Уровень 2

			MainTabControl.SelectedIndex = (int)ca.CurrentTab;
			ConvertorsContainer.SelectedIndex = (int)ca.ConvertorTab;

			try
				{
				KKTListForErrors.SelectedIndex = (int)ca.KKTForErrors;
				}
			catch
				{
				KKTListForErrors.SelectedIndex = 0;
				}
			ErrorSearchText.Text = ca.ErrorCode;
			ErrorFindButton_Click (null, null);

			FNLifeSN.Text = ca.FNSerial;
			FNLifeEvFlags = (KassArrayDB::RD_AAOW.FNLifeFlags)ca.FNLifeEvFlags;

			RNMSerial.MaxLength = (int)kb.KKTNumbers.MaxSerialNumberLength;
			RNMSerial.Text = ca.KKTSerial;
			RNMUserINN.Text = ca.UserINN;
			RNMValue.Text = ca.RNMKKT;
			RNMSerial_TextChanged (null, null); // Для протяжки пустых полей

			OFDINN.Text = ca.OFDINN;
			OFDNalogSite.Text = KassArrayDB::RD_AAOW.OFD.FNSSite;
			OFDDNSNameK.Text = KassArrayDB::RD_AAOW.OFD.OKPSite;
			OFDIPK.Text = KassArrayDB::RD_AAOW.OFD.OKPIP;
			OFDPortK.Text = KassArrayDB::RD_AAOW.OFD.OKPPort;
			LoadOFDParameters ();

			LowLevelCommand.SelectedIndex = (int)ca.LowLevelCode;

			try
				{
				KKTListForCodes.SelectedIndex = (int)ca.KKTForCodes;
				}
			catch { }
			TextToConvert.Text = ca.CodesText;
			TextToConvert.Items.AddRange (ca.CodesOftenTexts);

			KKTListForManuals.Items.AddRange (kb.UserGuides.GetKKTList ());
			KKTListForManuals.SelectedIndex = (int)ca.KKTForManuals;
			UserManualFlags = (KassArrayDB::RD_AAOW.UserManualsFlags)ca.UserManualFlags;

			if (ca.EnableExtendedMode)	// Уровень 1
				OperationsListForManuals.Items.AddRange (KassArrayDB::RD_AAOW.UserManuals.OperationTypes);
			else
				OperationsListForManuals.Items.AddRange (KassArrayDB::RD_AAOW.UserManuals.OperationsForCashiers);

			try
				{
				OperationsListForManuals.SelectedIndex = (int)ca.OperationForManuals;
				}
			catch
				{
				OperationsListForManuals.SelectedIndex = 0;
				}

			BarcodeData.MaxLength = (int)KassArrayDB::RD_AAOW.BarCodes.MaxSupportedDataLength;
			BarcodeData.Text = ca.BarcodeData;
			BarcodesConvertToEN_Click (null, null);

			CableType.SelectedIndex = (int)ca.CableType;

			TLV_FFDCombo.Items.Add ("ФФД 1.05");
			TLV_FFDCombo.Items.Add ("ФФД 1.1");
			TLV_FFDCombo.Items.Add ("ФФД 1.2");
			TLV_FFDCombo.SelectedIndex = (int)ca.FFDForTLV;

			TLVFind.Text = ca.TLVData;
			TLV_ObligationBase.Text = KassArrayDB::RD_AAOW.TLVTags.ObligationBase;
			TLVButton_Click (null, null);

			EncodingCombo.Items.AddRange (KassArrayDB::RD_AAOW.DataConvertors.AvailableEncodings);
			encodingModesCount = (uint)EncodingCombo.Items.Count / 2;
			EncodingCombo.SelectedIndex = (int)ca.EncodingForConvertor;

			ConvertHexField.Text = ca.ConversionHex;
			ConvertTextField.Text = ca.ConversionText;

			// Блокировка расширенных функций при необходимости
			RNMGenerate.Visible = LowLevelTab.Enabled = TLVTab.Enabled = ConnectorsTab.Enabled =
				PrintFullUserManual.Visible = ca.EnableExtendedMode;	// Уровень 2
			CodesTab.Enabled = ca.EnableExtendedMode;	// Уровень 1

			RNMTip.Text = "Индикатор ФФД: красный – поддержка не планируется; зелёный – поддерживается; " +
				"жёлтый – планируется; синий – нет сведений" + RDLocale.RN +
				"(на момент релиза этой версии приложения)";
			if (ca.EnableExtendedMode)	// Уровень 2
				{
				RNMTip.Text += (RDLocale.RNRN +
					"Первые 10 цифр являются порядковым номером ККТ в реестре. При генерации " +
					"РНМ их можно указать вручную – остальные будут достроены программой");
				}
			else
				{
				RNMLabel.Text = "Укажите регистрационный номер для проверки:";
				FNReader.Enabled = false;
				}

			ConvNumber.Text = ca.ConversionNumber;
			ConvNumber_TextChanged (null, null);

			ConvCode.Text = ca.ConversionCode;
			ConvCode_TextChanged (null, null);

			// Настройка иконки в трее
			ni.Icon = Properties.TextToKKMResources.KassArrayTray;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();

			ni.ContextMenu.MenuItems.Add (new MenuItem ("Работа с &ФН", FNReader_Click));
			ni.ContextMenu.MenuItems[0].Enabled = !RDGenerics.StartedFromMSStore && ca.EnableExtendedMode;	// Уровень 2
			ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit),
				CloseService));

			ni.MouseDown += ReturnWindow;
			ni.ContextMenu.MenuItems[1].DefaultItem = true;

			// Настройка расширенного режима
			ExtendedMode.Checked = ca.EnableExtendedMode;
			ExtendedMode.CheckedChanged += ExtendedMode_CheckedChanged;
			}

		private void TextToKKTForm_Shown (object sender, EventArgs e)
			{
			if (hideWindow)
				this.Hide ();
			if (closeWindowOnError)
				this.Close ();
			}

		// Включение / выключение режима сервис-инженера
		private void ExtendedMode_CheckedChanged (object sender, EventArgs e)
			{
			if (ExtendedMode.Checked)
				{
				RDGenerics.MessageBox (RDMessageTypes.Error_Left, ConfigAccessor.ExtendedModeMessage);
				ca.EnableExtendedMode = true;
				}
			else
				{
				RDGenerics.MessageBox (RDMessageTypes.Question_Left, ConfigAccessor.NoExtendedModeMessage);
				ca.EnableExtendedMode = false;
				}
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
				if (!TopFlag.Checked)
					this.TopMost = false;
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

			if (ca.OverrideCloseButton && !closeWindowOnRequest)
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

			ca.KeepApplicationState = KeepAppState.Checked;
			ca.TopMost = TopFlag.Checked;
			if (!ca.KeepApplicationState)
				return;

			ca.CurrentTab = (uint)MainTabControl.SelectedIndex;
			ca.ConvertorTab = (uint)ConvertorsContainer.SelectedIndex;

			ca.KKTForErrors = (uint)KKTListForErrors.SelectedIndex;
			ca.ErrorCode = ErrorSearchText.Text;

			ca.FNSerial = FNLifeSN.Text;
			ca.FNLifeEvFlags = (uint)FNLifeEvFlags;

			ca.KKTSerial = RNMSerial.Text;
			ca.UserINN = RNMUserINN.Text;
			ca.RNMKKT = RNMValue.Text;

			ca.OFDINN = OFDINN.Text;

			ca.LowLevelProtocol = (uint)LowLevelProtocol.SelectedIndex;
			ca.LowLevelCode = (uint)LowLevelCommand.SelectedIndex;

			ca.KKTForCodes = (uint)KKTListForCodes.SelectedIndex;
			ca.CodesText = TextToConvert.Text;

			List<string> cots = new List<string> ();
			foreach (object o in TextToConvert.Items)
				cots.Add (o.ToString ());
			ca.CodesOftenTexts = cots.ToArray ();

			ca.BarcodeData = BarcodeData.Text;
			ca.CableType = (uint)CableType.SelectedIndex;
			ca.TLVData = TLVFind.Text;

			ca.KKTForManuals = (uint)KKTListForManuals.SelectedIndex;
			ca.OperationForManuals = (uint)OperationsListForManuals.SelectedIndex;
			ca.UserManualFlags = (uint)UserManualFlags;
			ca.FFDForTLV = (uint)TLV_FFDCombo.SelectedIndex;

			ca.ConversionNumber = ConvNumber.Text;
			ca.ConversionCode = ConvCode.Text;
			ca.EncodingForConvertor = (uint)EncodingCombo.SelectedIndex;
			ca.ConversionHex = ConvertHexField.Text;
			ca.ConversionText = ConvertTextField.Text;
			}

		// Отображение справки
		private void BHelp_Clicked (object sender, EventArgs e)
			{
			this.TopMost = false;
			RDGenerics.ShowAbout (false);
			this.TopMost = TopFlag.Checked;
			}

		// Запрос цвета, соответствующего статусу поддержки
		private Color StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses Status)
			{
			if (Status == KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Planned)
				return Color.FromArgb (255, 255, 200);

			if (Status == KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Supported)
				return Color.FromArgb (200, 255, 200);

			if (Status == KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unsupported)
				return Color.FromArgb (255, 200, 200);

			// Остальные
			return Color.FromArgb (200, 200, 255);
			}

		// Вызов библиотеки FNReader
		private void FNReader_Click (object sender, EventArgs e)
			{
			CallFNReader ();
			}

		private void CallFNReader ()
			{
			// Отправка "сообщения" окну модуля работы с ФН
			bool problem = ewhIsActive ? ewh.WaitOne (100) : true;
			if (!problem)
				ewh.Set ();

			// Контроль на завершение предыдущих процессов
			bool res;
			if (!problem)
				{
				// Тихо
				res = RDGenerics.KillAllProcesses (
					Path.GetFileNameWithoutExtension (ProgramDescription.KassArrayDLLs[1]), false, true);
				}
			else
				{
				// С предупреждением
				this.TopMost = false;
				res = RDGenerics.KillAllProcesses (
					Path.GetFileNameWithoutExtension (ProgramDescription.KassArrayDLLs[1]), true, false);
				this.TopMost = TopFlag.Checked;
				}

			// Отмена дальнейших действий, если процесс не был завершён / уже был запущен
			if (!res)
				return;

			// Нормальный запуск модуля работы с ФН
			RDGenerics.RunURL (RDGenerics.AppStartupPath + ProgramDescription.KassArrayDLLs[1]);
			}

		// Переключение состояния "поверх всех окон"
		private void TopFlag_CheckedChanged (object sender, EventArgs e)
			{
			this.TopMost = TopFlag.Checked;
			}

		// Получение данных от FNReader
		private void GetFromFNReader_Click (object sender, EventArgs e)
			{
			// Контроль
			string status = "";
			try
				{
				status = File.ReadAllText (KassArrayDB::RD_AAOW.KKTSupport.StatusFilePath,
					RDGenerics.GetEncoding (RDEncodings.UTF8));
				}
			catch { }

			if (string.IsNullOrWhiteSpace (status))
				{
				this.TopMost = false;
				RDGenerics.MessageBox (RDMessageTypes.Information_Center,
					"Статус ФН ещё не запрашивался или содержит не все требуемые поля");
				this.TopMost = TopFlag.Checked;

				return;
				}

			// Разбор
			int left, right;
			string buttonName = ((Button)sender).Name;

			// Раздел параметров ОФД
			switch (buttonName)
				{
				case "OFDFromFNReader":
					if (((left = status.LastIndexOf ("ИНН ОФД: ")) >= 0) &&
						((right = status.IndexOf ("\n", left)) >= 0))
						{
						left += 9;
						OFDINN.Text = status.Substring (left, right - left).Trim ();
						LoadOFDParameters ();
						}

					break;

				// Раздел срока жизни ФН
				case "FNFromFNReader":
					if (((left = status.LastIndexOf ("номер ФН: ")) >= 0) &&
						((right = status.IndexOf ("\n", left)) >= 0))
						{
						left += 10;
						FNLifeSN.Text = status.Substring (left, right - left).Trim ();
						}

					if ((left = status.LastIndexOf ("Регистрация")) < 0)
						return;

					if (status.IndexOf ("обложение: ОСН", left) >= 0)
						GenericTaxFlag.Checked = true;
					else
						OtherTaxFlag.Checked = true;

					if ((status.IndexOf ("реализация товаров", left) >= 0))
						GoodsFlag.Checked = true;
					else
						ServicesFlag.Checked = true;

					AutonomousFlag.Checked = (status.IndexOf ("автономная", left) >= 0);
					ExciseFlag.Checked = (status.IndexOf ("а подакцизн", left) >= 0);
					FFD12Flag.Checked = (status.IndexOf ("ФФД: 1.2", left) >= 0);
					AgentsFlag.Checked = (status.IndexOf ("А: признак", left) >= 0);
					MarkGoodsFlag.Checked = (status.IndexOf ("а маркирован", left) >= 0);
					PawnInsuranceFlag.Checked = (status.IndexOf ("ги ломбарда", left) >= 0);
					GamblingLotteryFlag.Checked = (status.IndexOf ("ги страхования", left) >= 0);
					// Дату и сезонный режим не запрашиваем

					break;

				// Раздел контроля ЗН ККТ, РНМ и ИНН
				case "RNMFromFNReader":
					if (((left = status.LastIndexOf ("Заводской номер ККТ: ")) >= 0) &&
						((right = status.IndexOf ("\n", left)) >= 0))
						{
						left += 21;
						RNMSerial.Text = status.Substring (left, right - left).Trim ();
						}
					if (((left = status.LastIndexOf ("Регистрационный номер ККТ: ")) >= 0) &&
						((right = status.IndexOf ("\n", left)) >= 0))
						{
						left += 27;
						RNMValue.Text = status.Substring (left, right - left).Trim ();
						}
					if (((left = status.LastIndexOf ("ИНН пользователя: ")) >= 0) &&
						((right = status.IndexOf ("\n", left)) >= 0))
						{
						left += 18;
						RNMUserINN.Text = status.Substring (left, right - left).Trim ();
						}
					break;
				}
			}

		// Включение / выключение отдельной кнопки сворачивания
		private void OverrideCloseButton_CheckedChanged (object sender, EventArgs e)
			{
			ca.OverrideCloseButton = OverrideCloseButton.Checked;
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
				if (text.Contains (" "))
					text = text.Insert (text.IndexOf (' '), " ");

			TextToConvert.Text = text.PadLeft ((int)(total + text.Length) / 2, ' ');
			}

		#endregion

		#region Коды ошибок ККТ

		// Выбор ошибки
		private void ErrorCodesList_SelectedIndexChanged (object sender, EventArgs e)
			{
			ErrorFindButton_Click (null, null);
			}

		// Поиск по тексту ошибки
		private void ErrorFindButton_Click (object sender, EventArgs e)
			{
			List<string> codes = kb.Errors.GetErrorCodesList ((uint)KKTListForErrors.SelectedIndex);
			string text = ErrorSearchText.Text.ToLower ();

			lastErrorSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				string code = codes[j].ToLower ();
				string res = kb.Errors.GetErrorText ((uint)KKTListForErrors.SelectedIndex, (uint)j);

				if (code.Contains (text) || res.ToLower ().Contains (text) ||
					code.Contains ("?") && text.Contains (code.Replace ("?", "")))
					{
					lastErrorSearchOffset = j;
					ErrorText.Text = codes[j] + ": " + res;
					return;
					}
				}

			// Код не найден
			ErrorText.Text = "(описание ошибки не найдено)";
			}

		private void ErrorSearchText_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				ErrorFindButton_Click (null, null);
			}

		private void ErrorClearButton_Click (object sender, EventArgs e)
			{
			ErrorSearchText.Text = "";
			}

		#endregion

		#region Срок жизни ФН

		// Ввод номера ФН в разделе срока жизни
		private void FNLifeSN_TextChanged (object sender, EventArgs e)
			{
			// Получение описания
			if (!string.IsNullOrWhiteSpace (FNLifeSN.Text))
				FNLifeName.Text = kb.FNNumbers.GetFNName (FNLifeSN.Text);
			else
				FNLifeName.Text = "(введите ЗН ФН)";

			// Определение длины ключа
			if (!kb.FNNumbers.IsFNKnown (FNLifeSN.Text))
				{
				FNLife36.Enabled = FNLife13.Enabled = true;
				}
			else
				{
				if (kb.FNNumbers.IsNotFor36Months (FNLifeSN.Text))
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

		// Изменение параметров, влияющих на срок жизни ФН
		private void FNLifeStartDate_ValueChanged (object sender, EventArgs e)
			{
			string res = KassArrayDB::RD_AAOW.KKTSupport.GetFNLifeEndDate (FNLifeStartDate.Value, FNLifeEvFlags);

			FNLifeResult.Text = "ФН прекратит работу ";
			if (res.StartsWith (KassArrayDB::RD_AAOW.KKTSupport.FNLifeInacceptableSign))
				{
				FNLifeResult.ForeColor = Color.FromArgb (255, 0, 0);
				fnLifeResult = res.Substring (1);
				FNLifeResult.Text += (fnLifeResult + KassArrayDB::RD_AAOW.FNSerial.FNIsNotAcceptableMessage);
				}
			else if (res.StartsWith (KassArrayDB::RD_AAOW.KKTSupport.FNLifeUnwelcomeSign))
				{
				FNLifeResult.ForeColor = Color.FromArgb (255, 128, 0);
				fnLifeResult = res.Substring (1);
				FNLifeResult.Text += (fnLifeResult + KassArrayDB::RD_AAOW.FNSerial.FNIsNotRecommendedMessage);
				}
			else
				{
				FNLifeResult.ForeColor = Color.FromArgb (0, 128, 0);
				fnLifeResult = res;
				FNLifeResult.Text += res;
				}

			if (kb.FNNumbers.IsFNKnown (FNLifeSN.Text))
				{
				if (!kb.FNNumbers.IsFNAllowed (FNLifeSN.Text))
					{
					FNLifeResult.ForeColor = Color.FromArgb (255, 0, 0);

					FNLifeResult.Text += KassArrayDB::RD_AAOW.FNSerial.FNIsNotAllowedMessage;
					FNLifeName.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unsupported);
					}
				else
					{
					FNLifeName.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Supported);
					}
				}
			else
				{
				FNLifeName.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unknown);
				}
			}

		// Копирование срока действия ФН
		private void FNLifeResult_Click (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (fnLifeResult);
			}

		// Очистка полей
		private void FNLifeSNClear_Click (object sender, EventArgs e)
			{
			FNLifeSN.Text = "";
			}

		// Поиск сигнатуры ЗН ФН по части названия
		private void FNFindSN_Click (object sender, EventArgs e)
			{
			string sig = kb.FNNumbers.FindSignatureByName (FNLifeSN.Text);
			if (sig != "")
				FNLifeSN.Text = sig;
			}

		private void FNLifeSN_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				FNFindSN_Click (null, null);
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для расчёта срока жизни ФН
		/// </summary>
		private KassArrayDB::RD_AAOW.FNLifeFlags FNLifeEvFlags
			{
			get
				{
				KassArrayDB::RD_AAOW.FNLifeFlags flags =
					KassArrayDB::RD_AAOW.KKTSupport.SetFlag (0, KassArrayDB::RD_AAOW.FNLifeFlags.FN15, FNLife13.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.FNExactly13, kb.FNNumbers.IsFNExactly13 (FNLifeSN.Text));
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.GenericTax, GenericTaxFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.Goods, GoodsFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.Season, SeasonFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.Agents, AgentsFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.Excise, ExciseFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.Autonomous, AutonomousFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.FFD12, FFD12Flag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.GamblingAndLotteries, GamblingLotteryFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.PawnsAndInsurance, PawnInsuranceFlag.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.MarkGoods, MarkGoodsFlag.Checked);

				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.FNLifeFlags.MarkFN,
					!kb.FNNumbers.IsFNKnown (FNLifeSN.Text) || kb.FNNumbers.IsFNCompatibleWithFFD12 (FNLifeSN.Text));
				// Признак распознанного ЗН ФН

				return flags;
				}
			set
				{
				FNLife13.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.FN15);
				FNLife36.Checked = !FNLife13.Checked;
				// .Exactly13 – вспомогательный нехранимый флаг
				GenericTaxFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.GenericTax);
				OtherTaxFlag.Checked = !GenericTaxFlag.Checked;
				GoodsFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.Goods);
				ServicesFlag.Checked = !GoodsFlag.Checked;
				SeasonFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.Season);
				AgentsFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.Agents);
				ExciseFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.Excise);
				AutonomousFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.Autonomous);
				FFD12Flag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.FFD12);
				GamblingLotteryFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.GamblingAndLotteries);
				PawnInsuranceFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.PawnsAndInsurance);
				MarkGoodsFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.FNLifeFlags.MarkGoods);
				// .MarkFN – вспомогательный нехранимый флаг
				}
			}

		#endregion

		#region РНМ и ЗН

		// Ввод номеров в разделе РН
		private void RNMSerial_TextChanged (object sender, EventArgs e)
			{
			// Заводской номер ККТ
			if (RNMSerial.Text != "")
				{
				RNMSerialResult.Text = kb.KKTNumbers.GetKKTModel (RNMSerial.Text);

				KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses[] statuses =
					kb.KKTNumbers.GetFFDSupportStatus (RNMSerial.Text);
				RNMSupport105.BackColor = StatusToColor (statuses[0]);
				RNMSupport11.BackColor = StatusToColor (statuses[1]);
				RNMSupport12.BackColor = StatusToColor (statuses[2]);
				}
			else
				{
				RNMSerialResult.Text = "(введите ЗН ККТ)";
				RNMSupport105.BackColor = RNMSupport11.BackColor = RNMSupport12.BackColor =
					StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unknown);
				}

			// ИНН пользователя
			RegionLabel.Text = "";
			int checkINN = KassArrayDB::RD_AAOW.KKTSupport.CheckINN (RNMUserINN.Text);
			if (checkINN < 0)
				RNMUserINN.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unknown);
			else if (checkINN == 0)
				RNMUserINN.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Supported);
			else
				RNMUserINN.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Planned);
			// Не ошибка

			RegionLabel.Text = kb.KKTNumbers.GetRegionName (RNMUserINN.Text);

			// РН
			if (RNMValue.Text.Length < 10)
				RNMValue.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unknown);
			else if (KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text, RNMSerial.Text,
				RNMValue.Text.Substring (0, 10)) == RNMValue.Text)
				RNMValue.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Supported);
			else
				RNMValue.BackColor = StatusToColor (KassArrayDB::RD_AAOW.KKTSerial.FFDSupportStatuses.Unsupported);
			}

		// Генерация регистрационного номера
		private void RNMGenerate_Click (object sender, EventArgs e)
			{
			if (RNMValue.Text.Length < 1)
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					RNMSerial.Text, "0");
			else if (RNMValue.Text.Length < 10)
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					RNMSerial.Text, RNMValue.Text);
			else
				RNMValue.Text = KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (RNMUserINN.Text,
					RNMSerial.Text, RNMValue.Text.Substring (0, 10));
			}

		// Очистка полей
		private void RNMSerialClear_Click (object sender, EventArgs e)
			{
			RNMSerial.Text = "";
			RNMUserINN.Text = "";
			RNMValue.Text = "";
			}

		// Поиск сигнатуры ЗН ККТ по части названия
		private void RNMSerialFind_Click (object sender, EventArgs e)
			{
			string sig = kb.KKTNumbers.FindSignatureByName (RNMSerial.Text);
			if (sig != "")
				RNMSerial.Text = sig;
			}

		private void RNMSerial_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				RNMSerialFind_Click (null, null);
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
			List<string> parameters = kb.Ofd.GetOFDParameters (OFDINN.Text.Contains ("0000000000") ? "" : OFDINN.Text);

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
			OFDDisabledLabel.Enabled = OFDDisabledLabel.Visible = !string.IsNullOrWhiteSpace (parameters[10]);
			if (OFDDisabledLabel.Enabled)
				OFDDisabledLabel.Text = parameters[10] + RDLocale.RN;
			}

		// Копирование в буфер обмена
		private void OFDDNSName_Click (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (((Button)sender).Text);
			}

		private void OFDNameCopy_Click (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (OFDNamesList.Text.Replace ('«', '\"').Replace ('»', '\"'));
			}

		private void OFDINNCopy_Click (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (OFDINN.Text);
			}

		// Поиск по названию ОФД
		private void OFDFindButton_Click (object sender, EventArgs e)
			{
			List<string> codes = kb.Ofd.GetOFDNames ();
			codes.AddRange (kb.Ofd.GetOFDINNs ());

			string text = OFDSearchText.Text.ToLower ();

			lastOFDSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastOFDSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastOFDSearchOffset = (i + lastOFDSearchOffset) % codes.Count;
					OFDNamesList.SelectedIndex = lastOFDSearchOffset % (codes.Count / 2) + 1;
					return;
					}
			}

		private void OFDSearchText_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				OFDFindButton_Click (null, null);
			}

		// Очистка полей
		private void OFDINNClear_Click (object sender, EventArgs e)
			{
			OFDSearchText.Text = "";
			}

		#endregion

		#region Команды нижнего уровня

		// Выбор списка команд нижнего уровня
		private void LowLevelProtocol_CheckedChanged (object sender, EventArgs e)
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

		// Поиск по тексту ошибки
		private void LowLevelFindButton_Click (object sender, EventArgs e)
			{
			List<string> codes = kb.LLCommands.GetCommandsList ((uint)LowLevelProtocol.SelectedIndex);
			string text = LowLevelSearchText.Text.ToLower ();

			lastLowLevelSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastLowLevelSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastLowLevelSearchOffset = LowLevelCommand.SelectedIndex =
						(i + lastLowLevelSearchOffset) % codes.Count;
					return;
					}
			}

		private void LowLevelSearchText_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				LowLevelFindButton_Click (null, null);
			}

		private void LowLevelClearButton_Click (object sender, EventArgs e)
			{
			LowLevelSearchText.Text = "";
			}

		#endregion

		#region Руководства пользователя

		// Выбор модели аппарата
		private void KKTListForManuals_SelectedIndexChanged (object sender, EventArgs e)
			{
			byte idx = (byte)OperationsListForManuals.SelectedIndex;

			AddToPrint.Checked = ca.GetUserManualSectionState (idx);
			UMOperationText.Text = kb.UserGuides.GetManual ((uint)KKTListForManuals.SelectedIndex, idx,
				UserManualFlags);
			}

		// Печать инструкции
		private void PrintUserManual_Click (object sender, EventArgs e)
			{
			// Сборка задания на печать
			KassArrayDB::RD_AAOW.UserManualsFlags flags = UserManualFlags;
			if (((Button)sender).Name.Contains ("Cashier"))
				flags |= KassArrayDB::RD_AAOW.UserManualsFlags.GuideForCashier;

			string text = KassArrayDB::RD_AAOW.KKTSupport.BuildUserManual (kb.UserGuides,
				(uint)KKTListForManuals.SelectedIndex, ca.UserManualSections, flags);

			// Печать
			KassArrayDB::RD_AAOW.KKTSupport.PrintText (text, KassArrayDB::RD_AAOW.PrinterTypes.ManualA4);
			}

		// Выбор компонентов инструкции
		private void AddToPrint_CheckedChanged (object sender, EventArgs e)
			{
			ca.SetUserManualSectionState ((byte)OperationsListForManuals.SelectedIndex, AddToPrint.Checked);
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для руководства пользователя
		/// </summary>
		private KassArrayDB::RD_AAOW.UserManualsFlags UserManualFlags
			{
			get
				{
				KassArrayDB::RD_AAOW.UserManualsFlags flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (0,
					KassArrayDB::RD_AAOW.UserManualsFlags.MoreThanOneItemPerDocument,
					MoreThanOneItemPerDocument.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.UserManualsFlags.ProductBaseContainsPrices,
					ProductBaseContainsPrices.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.UserManualsFlags.CashiersHavePasswords,
					CashiersHavePasswords.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.UserManualsFlags.ProductBaseContainsServices,
					BaseContainsServices.Checked);
				flags = KassArrayDB::RD_AAOW.KKTSupport.SetFlag (flags,
					KassArrayDB::RD_AAOW.UserManualsFlags.DocumentsContainMarks,
					DocumentsContainMarks.Checked);

				return flags;
				}
			set
				{
				MoreThanOneItemPerDocument.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.UserManualsFlags.MoreThanOneItemPerDocument);
				ProductBaseContainsPrices.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.UserManualsFlags.ProductBaseContainsPrices);
				CashiersHavePasswords.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.UserManualsFlags.CashiersHavePasswords);
				BaseContainsServices.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.UserManualsFlags.ProductBaseContainsServices);
				DocumentsContainMarks.Checked = KassArrayDB::RD_AAOW.KKTSupport.IsSet (value,
					KassArrayDB::RD_AAOW.UserManualsFlags.DocumentsContainMarks);
				}
			}

		#endregion

		#region TLV-теги

		// Очистка полей
		private void TLVClearButton_Click (object sender, EventArgs e)
			{
			TLVFind.Text = "";
			TLVButton_Click (null, null);
			}

		// Поиск TLV-тега
		private void TLVButton_Click (object sender, EventArgs e)
			{
			if (kb.Tags.FindTag (TLVFind.Text,
				(KassArrayDB::RD_AAOW.TLVTags_FFDVersions)(TLV_FFDCombo.SelectedIndex + 2)))
				{
				TLVDescription.Text = kb.Tags.LastDescription;
				TLVType.Text = kb.Tags.LastType;

				if (!string.IsNullOrWhiteSpace (kb.Tags.LastValuesSet))
					TLVValues.Text = kb.Tags.LastValuesSet + RDLocale.RNRN;
				else
					TLVValues.Text = "";
				TLVValues.Text += kb.Tags.LastObligation;
				}
			else
				{
				TLVDescription.Text = TLVType.Text = TLVValues.Text = "(не найдено)";
				}
			}

		private void TLVFind_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				TLVButton_Click (null, null);
			}

		// Выбор ФФД
		private void TLVFFD_Changed (object sender, EventArgs e)
			{
			if ((TLVDescription.Text.Length >= 4) && (TLVDescription.Text != "(не найдено)") &&
				(TLVDescription.Text.Substring (0, 4) != TLVFind.Text))
				TLVFind.Text = TLVDescription.Text.Substring (0, 4);

			TLVButton_Click (null, null);
			}

		// Ссылка на приказ-обоснование обязательности тегов
		private void TLVObligationBase_Click (object sender, EventArgs e)
			{
			try
				{
				Process.Start (KassArrayDB::RD_AAOW.TLVTags.ObligationBaseLink);
				}
			catch { }
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
		private void ConnFindButton_Click (object sender, EventArgs e)
			{
			List<string> conns = kb.Plugs.GetCablesNames ();
			string text = ConnFind.Text.ToLower ();

			lastConnSearchOffset++;
			for (int i = 0; i < conns.Count; i++)
				{
				int j = (i + lastConnSearchOffset) % conns.Count;
				string conn = conns[j].ToLower ();

				if (conn.Contains (text))
					{
					CableType.SelectedIndex = lastConnSearchOffset = j;
					return;
					}
				}

			// Код не найден
			CableLeftSide.Text = "(описание не найдено)";
			CableLeftPins.Text = CableRightPins.Text = CableLeftDescription.Text = CableRightSide.Text = "";
			}

		private void ConnFind_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				ConnFindButton_Click (null, null);
			}

		private void ConnClearButton_Click (object sender, EventArgs e)
			{
			ConnFind.Text = "";
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
			bool plus = ((Button)sender).Text.Contains ("+");
			double res = KassArrayDB::RD_AAOW.DataConvertors.GetNumber (ConvNumber.Text);

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
			string[] res = KassArrayDB::RD_AAOW.DataConvertors.GetSymbolDescription (ConvCode.Text, 0);
			ConvCodeSymbol.Text = res[0];
			ConvCodeResult.Text = res[1];
			}

		private void ConvCodeAdd_Click (object sender, EventArgs e)
			{
			bool plus = ((Button)sender).Name == ConvCodeInc.Name;
			string[] res = KassArrayDB::RD_AAOW.DataConvertors.GetSymbolDescription (ConvCode.Text,
				(short)(plus ? 1 : -1));
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
		}
	}
