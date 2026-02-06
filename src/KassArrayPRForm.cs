extern alias KassArrayDB;

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class KassArrayPRForm: Form
		{
		// Переменные и константы
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;
		private bool closeWindowOnError = false;

		private string[] ipSignatures = ["ип", "индивидуальный предприниматель"];

		private ContextMenuStrip addressMenu;

		// Ресивер сообщений на повторное открытие окна
		private EventWaitHandle ewh;

		// Список сохранённых реквизитов
		private KAPRList kl;

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		/// <param name="FileName">Путь к файлу для немедленного открытия. Может быть пустым</param>
		public KassArrayPRForm (string FileName)
			{
			// Инициализация
			InitializeComponent ();
			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			kb = new KassArrayDB::RD_AAOW.KnowledgeBase ();

			if (!RDGenerics.CheckLibrariesVersions (ProgramDescription.AssemblyLibraries, true))
				{
				closeWindowOnError = true;
				return;
				}

			this.Text = RDGenerics.DefaultAssemblyVisibleName;

			FNCloseDateField.MinDate = new DateTime ((int)KAPRSupport.MinimumYear, 1, 1, 0, 0, 0);
			FNCloseDateField.MaxDate = new DateTime ((int)KAPRSupport.MaximumYear, 12, 31, 0, 0, 0);
			FNCloseDateField.Value = FNCloseDateField.MinDate;

			FNOpenDateField.MinDate = new DateTime ((int)KAPRSupport.MinimumYear, 1, 1, 0, 0, 0);
			FNOpenDateField.MaxDate = new DateTime ((int)KAPRSupport.MaximumYear, 12, 31, 0, 0, 0);
			FNOpenDateField.Value = FNOpenDateField.MinDate;

			OFDialog.Title = "Выберите файл заявления";
			SFDialog.Title = "Укажите расположение для файла заявления";
			OFDialog.Filter = SFDialog.Filter = "Файлы заявлений " + ProgramDescription.AssemblyMainName + " для ФНС (*" +
				KAPRSupport.BlankFileExtension + ")|*" + KAPRSupport.BlankFileExtension;

			// Настройка контролов
			KKTModelCombo.Items.Add (KAPRSupport.FillableFieldAlias);
			KKTModelCombo.Items.AddRange (kb.KKTNumbers.EnumerateAvailableModels ());
			KKTModelCombo.SelectedIndex = 0;

			FNModelCombo.Items.Add (KAPRSupport.FillableFieldAlias);
			FNModelCombo.Items.AddRange (kb.FNNumbers.EnumerateAvailableModels ());
			FNModelCombo.SelectedIndex = 0;

			OFDNameCombo.Items.Add (KAPRSupport.FillableFieldAlias);
			OFDNameCombo.Items.AddRange (kb.Ofd.GetOFDNames (true).ToArray ());
			OFDNameCombo.SelectedIndex = 0;

			AddressRegionCodeCombo.Items.Add (KAPRSupport.FillableFieldAlias);
			AddressRegionCodeCombo.Items.AddRange (kb.KKTNumbers.EnumerateAvailableRegions ());
			AddressRegionCodeCombo.SelectedIndex = 0;

			addressMenu = new ContextMenuStrip ();
			addressMenu.ShowImageMargin = false;

			AutomatFlag_CheckedChanged (null, null);

			// В последнюю очередь, т.к. запускает изменение прочих состояний
			BlankTypeCombo.Items.AddRange (KAPRSupport.BlankNames);

			// При загрузке файлов требуется полный список вариантов работы с ОФД
			BlankTypeCombo.SelectedIndex = (int)BlankTypes.RegistrationChange;

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);
			try
				{
				AddressRegionCodeCombo.SelectedIndex = (int)KAPRSupport.RegionIndex;
				}
			catch
				{
				AddressRegionCodeCombo.SelectedIndex = 0;
				}

			// Подключение к прослушиванию системного события вызова окна
			if (!RDGenerics.StartedFromMSStore)
				{
				try
					{
					ewh = EventWaitHandle.OpenExisting (KassArrayDB::RD_AAOW.ProgramDescription.AssemblyMainName +
						KassArrayDB::RD_AAOW.ProgramDescription.KassArrayPRAlias);
					}
				catch { }
				ShowWindowTimer.Enabled = true;
				}

			// Отключение запроса из ФН и сброс цветового выделения
			else
				{
				GetFromFN.Visible = false;
				for (int i = 0; i < MainTabControl.TabPages.Count; i++)
					{
					for (int j = 0; j < MainTabControl.TabPages[i].Controls.Count; j++)
						{
						if ((MainTabControl.TabPages[i].Controls[j].ForeColor.ToArgb () & 0xFFFFFF) == 0xFF)
							MainTabControl.TabPages[i].Controls[j].ForeColor = BlankTypeLabel.ForeColor;
						}
					}
				}

			// Открытие файла, если предусмотрено
			if (!string.IsNullOrWhiteSpace (FileName))
				{
				OFDialog.FileName = FileName;
				OFDialog_FileOk (null, null);
				}
			}

		private void KassArrayPRForm_Shown (object sender, EventArgs e)
			{
			if (closeWindowOnError)
				this.Close ();
			}

		// Закрытие окна
		private void KassArrayPRForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Контроль
			if (closeWindowOnError)
				return;

			if (KAPRSupport.ConfirmExit && (RDInterface.MessageBox (RDMessageFlags.CenterText | RDMessageFlags.Warning,
				"Выйти из программы?" + RDLocale.RN + "Все несохранённые данные будут утеряны",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes), RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) !=
				RDMessageButtons.ButtonOne))
				{
				e.Cancel = true;
				return;
				}

			RDGenerics.SaveWindowDimensions (this);
			KAPRSupport.RegionIndex = (uint)AddressRegionCodeCombo.SelectedIndex;
			}

		private void MClose_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Таймер обратной связи с вызывающим приложением
		private void ShowWindowTimer_Tick (object sender, EventArgs e)
			{
			// Контроль
			if (ewh == null)
				return;

			// Защита от лишних действий
			if (this.Visible && (this.WindowState != FormWindowState.Minimized) || !ewh.WaitOne (100))
				{
				ewh.Reset ();   // Удаление задвоенных вызовов
				return;
				}

			// Отмена повторного обращения
			ewh.Reset ();

			// Запуск
			this.Show ();

			this.TopMost = true;
			this.TopMost = false;
			this.WindowState = FormWindowState.Normal;
			}

		// Отображение справки
		private void BHelp_Clicked (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
			}

		// Поиск пользователя
		private void FindUserButton_Click (object sender, EventArgs e)
			{
			// Попытка получения из сохранённого списка
			if (kl == null)
				kl = new KAPRList ();

			string[] req = kl.FindRequisites (INNField.Text);
			if (req == null)
				{
				// Поиск в ЕГРЮЛ
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					"ИНН пользователя скопирован в буфер", 1000);
				RDGenerics.SendToClipboard (INNField.Text, false);
				RDGenerics.RunURL (KAPRSupport.UserSearchRequest);
				}
			else
				{
				// Подгрузка из списка
				UserNameField.Text = req[0];
				OGRNField.Text = req[1];
				KPPField.Text = req[2];
				PresenterTypeField.Text = req[3];
				PresenterTypeFlag.Checked = (req[4] == "1");
				}
			}

		// Ввод ЗН ККТ
		private void KKTSerialField_TextChanged (object sender, EventArgs e)
			{
			string model = kb.KKTNumbers.GetKKTModel (KKTSerialField.Text);
			model = model.Replace (" (неточно)", "");

			int idx = KKTModelCombo.Items.IndexOf (model);
			if (idx >= 0)
				KKTModelCombo.SelectedIndex = idx;
			}

		// Ввод ЗН ФН
		private void FNSerialField_TextChanged (object sender, EventArgs e)
			{
			string model = kb.FNNumbers.GetFNName (FNSerialField.Text);
			int idx = model.LastIndexOf (',');
			if (idx < 0)
				return;

			model = model.Substring (0, idx);
			idx = FNModelCombo.Items.IndexOf (model);
			if (idx >= 0)
				FNModelCombo.SelectedIndex = idx;
			}

		// Выбор варианта работы с ОФД
		private void OFDVariantCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			OFDVariants variant = (OFDVariants)OFDVariantCombo.SelectedIndex;
			OFDNameLabel.Enabled = OFDNameCombo.Enabled = OFDVariantCombo.Enabled &&
				((variant == OFDVariants.ContinueWithOFD) ||
				(variant == OFDVariants.AddOFD) || (variant == OFDVariants.ChangeOFD));
			if (!OFDNameLabel.Enabled)
				OFDNameCombo.SelectedIndex = 0;
			}

		// Вспомогательные свойства
		private bool IsRegistrationChange
			{
			get
				{
				return (BlankTypeCombo.SelectedIndex == (int)BlankTypes.RegistrationChange);
				}
			}

		private bool IsUnregistration
			{
			get
				{
				return (BlankTypeCombo.SelectedIndex == (int)BlankTypes.Unregistration);
				}
			}

		// Выбор типа заявления
		private void BlankTypeCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			KKTStolenFlag.Enabled = KKTMissingFlag.Enabled = IsUnregistration;
			if (!KKTStolenFlag.Enabled)
				KKTStolenFlag.Checked = KKTMissingFlag.Checked = false;
			FNBrokenFlag.Enabled = IsRegistrationChange || IsUnregistration;
			if (!FNBrokenFlag.Enabled)
				FNBrokenFlag.Checked = false;

			UserNameChangeFlag.Enabled = FNChangeFlag.Enabled =
				KKTRNMLabel.Enabled = KKTRNMField.Enabled =
				AddressPlaceChangeFlag.Enabled = AutomatChangeFlag.Enabled =
				OtherChangeFlag.Enabled = IsRegistrationChange;
			if (!UserNameChangeFlag.Enabled)
				UserNameChangeFlag.Checked = FNChangeFlag.Checked =
				AddressPlaceChangeFlag.Checked = AutomatChangeFlag.Checked =
				OtherChangeFlag.Checked = false;

			FNModelLabel.Enabled = FNModelCombo.Enabled = FNSerialLabel.Enabled = FNSerialField.Enabled = !IsUnregistration;
			if (!FNModelLabel.Enabled)
				FNModelCombo.SelectedIndex = 0;

			int ofdIdx = OFDVariantCombo.SelectedIndex;
			OFDVariantCombo.Items.Clear ();
			if (IsRegistrationChange || IsUnregistration)
				OFDVariantCombo.Items.AddRange (KAPRSupport.RegistrationChangeOFDModes);
			else
				OFDVariantCombo.Items.AddRange (KAPRSupport.RegistrationOFDModes);

			if (IsUnregistration || (ofdIdx >= OFDVariantCombo.Items.Count) || (ofdIdx < 0))
				OFDVariantCombo.SelectedIndex = (int)OFDVariants.ContinueWithOFD;
			else
				OFDVariantCombo.SelectedIndex = ofdIdx;

			OFDVariantLabel.Enabled = OFDVariantCombo.Enabled = !IsUnregistration;
			OFDVariantCombo_SelectedIndexChanged (null, null);

			AddressRegionCodeLabel.Enabled = AddressRegionCodeCombo.Enabled =
				AddressAreaLabel.Enabled = AddressAreaField.Enabled =
				AddressIndexLabel.Enabled = AddressIndexField.Enabled =
				AddressCityLabel.Enabled = AddressCityField.Enabled =
				AddressTownLabel.Enabled = AddressTownField.Enabled =
				AddressStreetLabel.Enabled = AddressStreetField.Enabled =
				AddressHouseLabel.Enabled = AddressHouseField.Enabled =
				AddressBuildingLabel.Enabled = AddressBuildingField.Enabled =
				AddressAppartmentLabel.Enabled = AddressAppartmentField.Enabled =
				PlaceLabel.Enabled = PlaceField.Enabled = FindPostIndex.Enabled = !IsUnregistration;

			LotteryFlag.Enabled = GamblingFlag.Enabled = GamblingExchangeFlag.Enabled =
				BankAgentFlag.Enabled = AgentFlag.Enabled = DeliveryFlag.Enabled =
				MarkFlag.Enabled = ExciseFlag.Enabled = InternetFlag.Enabled =
				BSOFlag.Enabled = AutomatFlag.Enabled = !IsUnregistration;
			if (!LotteryFlag.Enabled)
				LotteryFlag.Checked = GamblingFlag.Checked = GamblingExchangeFlag.Checked =
				BankAgentFlag.Checked = AgentFlag.Checked = DeliveryFlag.Checked =
				MarkFlag.Checked = ExciseFlag.Checked = InternetFlag.Checked =
				BSOFlag.Checked = AutomatFlag.Checked = false;

			UpdateFNOpen ();
			UpdateFNClose ();
			}

		// Изменение автоматического режима
		private void AutomatFlag_CheckedChanged (object sender, EventArgs e)
			{
			AutomatNumberLabel.Enabled = AutomatNumberField.Enabled =
				AutomatAddressIsSame.Enabled = AutomatFlag.Checked;
			if (!AutomatNumberLabel.Enabled)
				{
				AutomatAddressIsSame.Checked = false;
				AutomatNumberField.Text = "";
				}
			}

		// Изменение флагов, влияющих на ввод реквизитов закрытия архива
		private void FNChangeFlag_CheckedChanged (object sender, EventArgs e)
			{
			UpdateFNOpen ();
			UpdateFNClose ();
			}

		private void FNBrokenFlag_CheckedChanged (object sender, EventArgs e)
			{
			UpdateFNClose ();
			}

		private void UpdateFNClose ()
			{
			FNCloseLabel.Enabled = FNCloseFDLabel.Enabled = FNCloseFDField.Enabled =
				FNCloseDateLabel.Enabled = FNCloseDateField.Enabled =
				FNCloseFPDLabel.Enabled = FNCloseFPDField.Enabled =
				(FNChangeFlag.Checked || IsUnregistration) && !FNBrokenFlag.Checked;
			if (!FNCloseLabel.Enabled)
				{
				FNCloseDateField.Value = FNCloseDateField.MinDate;
				FNCloseFDField.Text = FNCloseFPDField.Text = "";
				}
			}

		private void UpdateFNOpen ()
			{
			FNOpenLabel.Enabled = FNOpenFDLabel.Enabled = FNOpenFDField.Enabled =
				FNOpenDateLabel.Enabled = FNOpenDateField.Enabled =
				FNOpenFPDLabel.Enabled = FNOpenFPDField.Enabled = IsRegistrationChange;
			if (!FNOpenLabel.Enabled)
				{
				FNOpenDateField.Value = FNOpenDateField.MinDate;
				FNOpenFDField.Text = FNOpenFPDField.Text = "";
				}
			else if (FNChangeFlag.Checked)
				{
				FNOpenFDLabel.Enabled = FNOpenFDField.Enabled = false;
				FNOpenFDField.Text = "1";
				}
			}

		/// <summary>
		/// Метод формирования заявления
		/// </summary>
		private void CreateBlank_Click (object sender, EventArgs e)
			{
			// Запрос настроек, если указано
			if (KAPRSupport.AskSettingsEveryTime)
				_ = new KassArrayPRSettings ();

			// Сохранение копии, если указано
			if (!string.IsNullOrWhiteSpace (KAPRSupport.BackupsPath))
				{
				string fileName = RDInterface.MessageBox ("Укажите название для резервной копии заявления",
					true, 256, KAPRSupport.GetRecommendedFileName (UserNameField.Text, (BlankTypes)BlankTypeCombo.SelectedIndex));

				if (!string.IsNullOrWhiteSpace (fileName))
					{
					SFDialog.FileName = KAPRSupport.BackupsPath + fileName + KAPRSupport.BlankFileExtension;
					SFDialog_FileOk (null, null);
					}
				}

			// Слив полей
			FlushFields ();

			// Контроль значений
			switch (KAPRSupport.CheckFields (IsRegistrationChange, AddressIndexField.Enabled,
				FNOpenDateField.Enabled && FNCloseDateField.Enabled))
				{
				case -1:
				case -2:
				case -3:
				case -4:
				case -5:
				case -6:
				case -7:
				case -8:
				case -9:
				case -10:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						KAPRSupport.CheckFieldsError);
					return;

				case -11:
					if (RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.LockSmallSize,
						KAPRSupport.CheckFieldsError,
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next),
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel)) != RDMessageButtons.ButtonOne)
						return;

					break;
				}

			// Формирование заявления
			string template = KAPRSupport.BuildTemplate (kb);

			// Сохранение реквизитов, если предусмотрено
			if (KAPRSupport.SaveUserRequisites)
				{
				if (kl == null)
					kl = new KAPRList ();

				kl.AddRequisites (INNField.Text, UserNameField.Text, OGRNField.Text,
					KPPField.Text, PresenterTypeField.Text, PresenterTypeFlag.Checked);
				}

			// Запуск на печать
			string res = KassArrayDB::RD_AAOW.KKTSupport.PrintText (template,
				KassArrayDB::RD_AAOW.PrinterTypes.BlankA4,
				KassArrayDB::RD_AAOW.PrinterFlags.None);
			if (!string.IsNullOrWhiteSpace (res))
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Не удалось распечатать документ" + RDLocale.RN + res);
			else
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					"Задание отправлено на печать", 700);
			}

		// Получение реквизитов из ФН
		private void GetFromFN_Click (object sender, EventArgs e)
			{
			#region Получение реквизитов отчётов

			// Регистрационный номер
			string res = "";
			string masterRN = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.RegistrationNumber, true);
			if (KKTRNMField.Enabled)
				KKTRNMField.Text = masterRN;

			// Закрытие архива
			string fnClose = "";
			string rnClose = "";
			try
				{
				fnClose = File.ReadAllText (KassArrayDB::RD_AAOW.KKTSupport.FNCloseFileName,
					RDGenerics.GetEncoding (RDEncodings.CP1251));
				}
			catch { }

			if (!string.IsNullOrWhiteSpace (fnClose))
				{
				string[] lines = fnClose.Split (['\n'], StringSplitOptions.RemoveEmptyEntries);
				if (lines.Length >= 5)
					{
					rnClose = lines[0];

					if (FNCloseFPDField.Enabled && (masterRN == rnClose))
						{
						int idx = lines[2].IndexOf (':');
						FNCloseFDField.Text = lines[2].Substring (idx + 2);

						idx = lines[3].IndexOf (':');
						try
							{
							FNCloseDateField.Value = DateTime.Parse (lines[3].Substring (idx + 2).Replace (',', ' '));
							}
						catch { }

						idx = lines[4].IndexOf (':');
						FNCloseFPDField.Text = lines[4].Substring (idx + 2);
						}
					}
				}

			if (string.IsNullOrWhiteSpace (rnClose))
				res += "• Не удалось получить реквизиты отчёта о закрытии архива ФН";
			else if (masterRN != rnClose)
				res += "• Реквизиты отчёта о закрытии архива ФН получены, но проигнорированы " +
					"из-за несовпадения регистрационного номера";
			else if (!FNCloseFPDField.Enabled)
				res += "• Реквизиты отчёта о закрытии архива ФН получены, но проигнорированы " +
					"из-за неподходящего типа заявления или снятого флажка «" +
					FNChangeFlag.Text + "»";
			else
				res += "• Получены реквизиты отчёта о закрытии архива ФН";
			res += RDLocale.RNRN;

			// Регистрация или перерегистрация
			string fnOpen = "";
			string rnOpen = "";
			try
				{
				fnOpen = File.ReadAllText (KassArrayDB::RD_AAOW.KKTSupport.FNOpenFileName,
					RDGenerics.GetEncoding (RDEncodings.CP1251));
				}
			catch { }

			bool openIsOlder = false;
			if (!string.IsNullOrWhiteSpace (fnOpen))
				{
				string[] lines = fnOpen.Split (['\n'], StringSplitOptions.RemoveEmptyEntries);
				if (lines.Length >= 5)
					{
					rnOpen = lines[0];

					// Помимо прочего отчёт о (пере)регистрации не должен быть старше отчёта о закрытии
					// более чем на месяц (в норме)
					int idx = lines[3].IndexOf (':');
					DateTime dto = DateTime.Parse (lines[3].Substring (idx + 2).Replace (',', ' '));
					openIsOlder = (FNCloseDateField.Value > dto.AddMonths (1));

					if (FNOpenFPDField.Enabled && (masterRN == rnOpen) && !openIsOlder)
						{
						// Не получать из отчёта это значение, если выбран вариант замены ФН
						if (FNOpenFDField.Enabled)
							{
							idx = lines[2].IndexOf (':');
							FNOpenFDField.Text = lines[2].Substring (idx + 2);
							}

						try
							{
							FNOpenDateField.Value = dto;
							}
						catch { }

						idx = lines[4].IndexOf (':');
						FNOpenFPDField.Text = lines[4].Substring (idx + 2);
						}
					}
				}

			if (string.IsNullOrWhiteSpace (rnOpen))
				res += "• Не удалось получить реквизиты отчёта о фискализации ФН";
			else if (masterRN != rnOpen)
				res += "• Реквизиты отчёта о фискализации ФН получены, но проигнорированы " +
					"из-за несовпадения регистрационного номера";
			else if (!FNOpenFPDField.Enabled)
				res += "• Реквизиты отчёта о фискализации ФН получены, но проигнорированы " +
					"из-за неподходящего типа заявления";
			else if (openIsOlder)
				res += "• Реквизиты отчёта о фискализации ФН получены, но проигнорированы, " +
					"т.к. отчёт старше закрытия архива более чем на месяц";
			else
				res += "• Получены реквизиты отчёта о фискализации ФН";
			res += RDLocale.RNRN;

			#endregion

			#region Получение реквизитов регистрации

			// Наименование пользователя
			UserNameField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.UserName, false);

			string unf = UserNameField.Text.ToLower ();
			bool ip = true;
			if (unf.StartsWith (ipSignatures[0]))
				UserNameField.Text = UserNameField.Text.Substring (ipSignatures[0].Length).Trim ();
			else if (unf.StartsWith (ipSignatures[1]))
				UserNameField.Text = UserNameField.Text.Substring (ipSignatures[1].Length).Trim ();
			else
				ip = false;

			if (ip)
				PresenterTypeField.Text = UserNameField.Text;

			// Остальные реквизиты
			INNField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.UserINN, false);
			KKTSerialField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.KKTSerialNumber, false);

			if (FNSerialField.Enabled)
				FNSerialField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.FNSerialNumber, false);

			string ofdINN = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.OFDINN, false);
			if (ofdINN == "0".PadLeft (12, '0'))
				{
				OFDVariantCombo.SelectedIndex = (int)OFDVariants.ContinueWithoutOFD;
				}
			else
				{
				OFDVariantCombo.SelectedIndex = (int)OFDVariants.ContinueWithOFD;
				string ofd = kb.Ofd.GetOFDByINN (ofdINN, 1);
				if (OFDNameCombo.Items.Contains (ofd))
					OFDNameCombo.Text = ofd;
				}

			string addressFromFN = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.RegistrationAddress, false);
			string[] affn = addressFromFN.Split ([',', '.', ' '], StringSplitOptions.RemoveEmptyEntries);

			addressMenu.Items.Clear ();
			for (int i = 0; i < affn.Length; i++)
				addressMenu.Items.Add (affn[i], null, AddressMenu_Click);

			if (PlaceField.Enabled)
				PlaceField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.RegistrationPlace, false);

			if (ExciseFlag.Enabled)
				ExciseFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.ExciseFlag, false) == "1";
			if (MarkFlag.Enabled)
				MarkFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.MarkingFlag, false) == "1";
			if (LotteryFlag.Enabled)
				LotteryFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.LotteryFlag, false) == "1";
			if (GamblingFlag.Enabled)
				GamblingFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.GamblingFlag, false) == "1";
			if (InternetFlag.Enabled)
				InternetFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.InternetFlag, false) == "1";

			if (AutomatFlag.Enabled)
				{
				AutomatFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.AutomatFlag, false) == "1";
				AutomatNumberField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.AutomatSerial, false);
				}

			if (BSOFlag.Enabled)
				BSOFlag.Checked = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
					(KassArrayDB::RD_AAOW.RegTags.BSOFlag, false) == "1";

			#endregion

			// Завершено
			res += "Обратите внимание:" + RDLocale.RN +
				"• Реквизиты, которые можно получить из ФН, помечены синим цветом. Остальные реквизиты при запросе " +
				"из ФН не меняются (кроме модели ККТ и ФН – приложение попытается их подобрать)" + RDLocale.RN +
				"• После загрузки проверять следует все значения (особенно, если из ФН были получены " +
				"не все данные)" + RDLocale.RN +
				"• Запрос данных из ФН приводит к замене ранее введённых данных в соответствующих полях";
			RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.NoSound, res);
			}

		// Поиск почтового индекса
		private void FindPostIndex_Click (object sender, EventArgs e)
			{
			bool city = !string.IsNullOrWhiteSpace (AddressCityField.Text);
			bool town = !string.IsNullOrWhiteSpace (AddressTownField.Text);

			if (!city && !town)
				{
				RDInterface.MessageBox (RDMessageFlags.CenterText | RDMessageFlags.Warning,
					"Для поиска почтового индекса нужно указать город или населённый пункт");
				return;
				}

			// Населённый пункт – более точная локация
			if (town)
				{
				RDGenerics.RunURL (KAPRSupport.AddressIndexSearchRequest +
					AddressTownField.Text.Replace (' ', '+').ToLower ());
				return;
				}

			RDGenerics.RunURL (KAPRSupport.AddressIndexSearchRequest +
				AddressCityField.Text.Replace (' ', '+').ToLower ());
			}

		// Отображение раздела настроек
		private void MSettings_Click (object sender, EventArgs e)
			{
			_ = new KassArrayPRSettings ();
			}

		// Загрузка данных из файла заявления
		private void MOpen_Click (object sender, EventArgs e)
			{
			if (!string.IsNullOrWhiteSpace (KAPRSupport.BackupsPath))
				OFDialog.InitialDirectory = KAPRSupport.BackupsPath;
			else
				OFDialog.InitialDirectory = "";

			OFDialog.ShowDialog ();
			}

		private void OFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Загрузка
			byte[] data;
			try
				{
				data = File.ReadAllBytes (OFDialog.FileName);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_LoadFailure_Fmt),
					Path.GetFileName (OFDialog.FileName)));
				return;
				}

			// Разбор
			byte[] preamble = RDGenerics.GetEncoding (RDEncodings.UTF8).GetPreamble ();
			string file;
			if (data.StartsWith (preamble))
				file = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data, preamble.Length, data.Length - preamble.Length);
			else
				file = RDGenerics.GetEncoding (RDEncodings.CP1251).GetString (data);

			if (!KAPRSupport.ParseFile (file, kb))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Указанный файл повреждён или не является поддерживаемым файлом заявления");
				return;
				}

			// Загрузка
			LoadFields ();
			}

		private void LoadFields ()
			{
			// 11 + 5 + 2 + 11 + 15 + 6 + 1 = 51
			try
				{
				FNChangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.FNChangeFlag);
				UserNameField.Text = KAPRSupport.GetFieldAsString (KBFFields.UserName);
				INNField.Text = KAPRSupport.GetFieldAsString (KBFFields.INN);
				OGRNField.Text = KAPRSupport.GetFieldAsString (KBFFields.OGRN);
				KPPField.Text = KAPRSupport.GetFieldAsString (KBFFields.KPP);
				PresenterTypeField.Text = KAPRSupport.GetFieldAsString (KBFFields.UserPresenter);
				PresenterTypeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.UserPresenterType);
				UserNameChangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.NameChangeFlag);
				KKTStolenFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.KKTStolenFlag);
				KKTMissingFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.KKTMissingFlag);
				FNBrokenFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.FNBrokenFlag);

				KKTSerialField.Text = KAPRSupport.GetFieldAsString (KBFFields.KKTSerialNumber);
				FNSerialField.Text = KAPRSupport.GetFieldAsString (KBFFields.FNSerialNumber);
				KKTModelCombo.Text = KAPRSupport.GetFieldAsString (KBFFields.KKTModelName);
				FNModelCombo.Text = KAPRSupport.GetFieldAsString (KBFFields.FNModelName);
				OFDNameCombo.Text = KAPRSupport.GetFieldAsString (KBFFields.OFDName);

				OFDVariantCombo.SelectedIndex = (int)KAPRSupport.GetFieldAsUint (KBFFields.OFDVariant);
				KKTRNMField.Text = KAPRSupport.GetFieldAsString (KBFFields.RegistrationNumber);

				AddressRegionCodeCombo.SelectedIndex = (int)KAPRSupport.GetFieldAsUint (KBFFields.AddressRegionCode);
				AddressAreaField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressArea);
				AddressCityField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressCity);
				AddressTownField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressTown);
				AddressIndexField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressIndex);
				AddressStreetField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressStreet);
				AddressHouseField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressHouseNumber);
				AddressBuildingField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressBuildingNumber);
				AddressAppartmentField.Text = KAPRSupport.GetFieldAsString (KBFFields.AddressAppartmentNumber);
				PlaceField.Text = KAPRSupport.GetFieldAsString (KBFFields.Place);
				AddressPlaceChangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.AddressPlaceChangeFlag);

				LotteryFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.LotteryFlag);
				GamblingFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.GamblingFlag);
				GamblingExchangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.GamblingExchangeFlag);
				BSOFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.BSOFlag);
				BankAgentFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.BankPaymentAgentFlag);
				AgentFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.PaymentAgentFlag);
				DeliveryFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.DeliveryFlag);
				ExciseFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.ExciseFlag);
				MarkFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.MarkFlag);
				InternetFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.InternetFlag);
				OtherChangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.OtherChangeFlag);
				AutomatFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.AutomatFlag);   // Должен предшествовать остальным
				AutomatNumberField.Text = KAPRSupport.GetFieldAsString (KBFFields.AutomatNumber);
				AutomatAddressIsSame.Checked = KAPRSupport.GetFieldAsBool (KBFFields.AutomatAddressIsSameFlag);
				AutomatChangeFlag.Checked = KAPRSupport.GetFieldAsBool (KBFFields.AutomatChangeFlag);

				FNCloseFDField.Text = KAPRSupport.GetFieldAsString (KBFFields.FNCloseDocumentNumber);
				FNCloseDateField.Value = KAPRSupport.GetFieldAsDateTime (KBFFields.FNCloseDate);
				FNCloseFPDField.Text = KAPRSupport.GetFieldAsString (KBFFields.FNCloseDocumentSign);
				FNOpenFDField.Text = KAPRSupport.GetFieldAsString (KBFFields.FNOpenDocumentNumber);
				FNOpenDateField.Value = KAPRSupport.GetFieldAsDateTime (KBFFields.FNOpenDate);
				FNOpenFPDField.Text = KAPRSupport.GetFieldAsString (KBFFields.FNOpenDocumentSign);

				// В последнюю очередь, поскольку событие запускает внутренние проверки
				BlankTypeCombo.SelectedIndex = (int)KAPRSupport.GetFieldAsUint (KBFFields.BlankType);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					"Файл заявления повреждён и не может быть загружен полностью. Проверьте поля заявления перед формированием");
				return;
				}
			}

		// Сохранение данных в файл заявления
		private void MSave_Click (object sender, EventArgs e)
			{
			SFDialog.FileName = KAPRSupport.GetRecommendedFileName (UserNameField.Text,
				(BlankTypes)BlankTypeCombo.SelectedIndex);
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Формирование списка
			FlushFields ();

			// Запись
			try
				{
				File.WriteAllText (SFDialog.FileName, KAPRSupport.BuildFile (),
					RDGenerics.GetEncoding (RDEncodings.UTF8));
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					string.Format (RDLocale.GetDefaultText (RDLDefaultTexts.Message_SaveFailure_Fmt),
					Path.GetFileName (SFDialog.FileName)));
				return;
				}
			}

		private void FlushFields ()
			{
			// 12 + 5 + 2 + 11 + 15 + 6 = 51
			KAPRSupport.SetField (KBFFields.BlankType, (uint)BlankTypeCombo.SelectedIndex);
			KAPRSupport.SetField (KBFFields.FNChangeFlag, FNChangeFlag.Checked);
			KAPRSupport.SetField (KBFFields.UserName, UserNameField.Text);
			KAPRSupport.SetField (KBFFields.INN, INNField.Text);
			KAPRSupport.SetField (KBFFields.OGRN, OGRNField.Text);
			KAPRSupport.SetField (KBFFields.KPP, KPPField.Text);
			KAPRSupport.SetField (KBFFields.UserPresenter, PresenterTypeField.Text);
			KAPRSupport.SetField (KBFFields.UserPresenterType, PresenterTypeFlag.Checked);
			KAPRSupport.SetField (KBFFields.NameChangeFlag, UserNameChangeFlag.Checked);
			KAPRSupport.SetField (KBFFields.KKTStolenFlag, KKTStolenFlag.Checked);
			KAPRSupport.SetField (KBFFields.KKTMissingFlag, KKTMissingFlag.Checked);
			KAPRSupport.SetField (KBFFields.FNBrokenFlag, FNBrokenFlag.Checked);

			KAPRSupport.SetField (KBFFields.KKTSerialNumber, KKTSerialField.Text);
			KAPRSupport.SetField (KBFFields.KKTModelName, KKTModelCombo.Text);
			KAPRSupport.SetField (KBFFields.FNSerialNumber, FNSerialField.Text);
			KAPRSupport.SetField (KBFFields.FNModelName, FNModelCombo.Text);
			KAPRSupport.SetField (KBFFields.OFDName, OFDNameCombo.Text);

			KAPRSupport.SetField (KBFFields.OFDVariant, (uint)OFDVariantCombo.SelectedIndex);
			KAPRSupport.SetField (KBFFields.RegistrationNumber, KKTRNMField.Text);

			KAPRSupport.SetField (KBFFields.AddressRegionCode, (uint)AddressRegionCodeCombo.SelectedIndex);
			KAPRSupport.SetField (KBFFields.AddressArea, AddressAreaField.Text);
			KAPRSupport.SetField (KBFFields.AddressCity, AddressCityField.Text);
			KAPRSupport.SetField (KBFFields.AddressTown, AddressTownField.Text);
			KAPRSupport.SetField (KBFFields.AddressIndex, AddressIndexField.Text);
			KAPRSupport.SetField (KBFFields.AddressStreet, AddressStreetField.Text);
			KAPRSupport.SetField (KBFFields.AddressHouseNumber, AddressHouseField.Text);
			KAPRSupport.SetField (KBFFields.AddressBuildingNumber, AddressBuildingField.Text);
			KAPRSupport.SetField (KBFFields.AddressAppartmentNumber, AddressAppartmentField.Text);
			KAPRSupport.SetField (KBFFields.Place, PlaceField.Text);
			KAPRSupport.SetField (KBFFields.AddressPlaceChangeFlag, AddressPlaceChangeFlag.Checked);

			KAPRSupport.SetField (KBFFields.LotteryFlag, LotteryFlag.Checked);
			KAPRSupport.SetField (KBFFields.GamblingFlag, GamblingFlag.Checked);
			KAPRSupport.SetField (KBFFields.GamblingExchangeFlag, GamblingExchangeFlag.Checked);
			KAPRSupport.SetField (KBFFields.BSOFlag, BSOFlag.Checked);
			KAPRSupport.SetField (KBFFields.BankPaymentAgentFlag, BankAgentFlag.Checked);
			KAPRSupport.SetField (KBFFields.PaymentAgentFlag, AgentFlag.Checked);
			KAPRSupport.SetField (KBFFields.DeliveryFlag, DeliveryFlag.Checked);
			KAPRSupport.SetField (KBFFields.ExciseFlag, ExciseFlag.Checked);
			KAPRSupport.SetField (KBFFields.MarkFlag, MarkFlag.Checked);
			KAPRSupport.SetField (KBFFields.InternetFlag, InternetFlag.Checked);
			KAPRSupport.SetField (KBFFields.OtherChangeFlag, OtherChangeFlag.Checked);
			KAPRSupport.SetField (KBFFields.AutomatNumber, AutomatNumberField.Text);
			KAPRSupport.SetField (KBFFields.AutomatAddressIsSameFlag, AutomatAddressIsSame.Checked);
			KAPRSupport.SetField (KBFFields.AutomatChangeFlag, AutomatChangeFlag.Checked);
			KAPRSupport.SetField (KBFFields.AutomatFlag, AutomatFlag.Checked);

			KAPRSupport.SetField (KBFFields.FNCloseDocumentNumber, FNCloseFDField.Text);
			KAPRSupport.SetField (KBFFields.FNCloseDate, FNCloseDateField.Value);
			KAPRSupport.SetField (KBFFields.FNCloseDocumentSign, FNCloseFPDField.Text);
			KAPRSupport.SetField (KBFFields.FNOpenDocumentNumber, FNOpenFDField.Text);
			KAPRSupport.SetField (KBFFields.FNOpenDate, FNOpenDateField.Value);
			KAPRSupport.SetField (KBFFields.FNOpenDocumentSign, FNOpenFPDField.Text);
			}

		// Обработчик контекстного меню адресных полей
		private void AddressField_MouseClick (object sender, MouseEventArgs e)
			{
			if (addressMenu.Items.Count < 1)
				return;
			if (e.Button != MouseButtons.Left)
				return;

			addressField = (TextBox)sender;
			addressMenu.Show (addressField, new Point (0, addressField.Height));
			}
		private TextBox addressField;

		private void AddressMenu_Click (object sender, EventArgs e)
			{
			addressField.Text = ((ToolStripItem)sender).Text;
			}
		}
	}
