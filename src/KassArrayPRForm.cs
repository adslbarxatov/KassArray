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

		/*private string[][] blankNames = [
			["Заявление о первичной регистрации ККТ", "первичная регистрация"],
			["Заявление о перерегистрации ККТ", "перерегистрация"],
			["Заявление о снятии ККТ с учёта", "снятие с учёта"],
			];*/

		private ContextMenuStrip addressMenu;

		// Переменная обмена значениями полей
		private string[] fieldsSet = new string[KAPRSupport.FieldsCount];

		/*// Константы режимов
		private const int continueWithOFD = 0;
		private const int continueWithoutOFD = 1;
		private const int addOFD = 2;
		private const int removeOFD = 3;
		private const int changeOFD = 4;*/

		/*private const int registrationMode = 0;
		private const int registrationChangeMode = 1;
		private const int unregistrationMode = 2;*/

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

			/*OFDVariantCombo.Items.Add ("Продолжить работу с ОФД");
			OFDVariantCombo.Items.Add ("Продолжить работу без ОФД");
			OFDVariantCombo.Items.Add ("Перейти на работу с ОФД");
			OFDVariantCombo.Items.Add ("Перейти на работу без ОФД");
			OFDVariantCombo.Items.Add ("Сменить ОФД");
			OFDVariantCombo.SelectedIndex = 0;*/

			AddressRegionCodeCombo.Items.Add (KAPRSupport.FillableFieldAlias);
			AddressRegionCodeCombo.Items.AddRange (kb.KKTNumbers.EnumerateAvailableRegions ());
			AddressRegionCodeCombo.SelectedIndex = 0;

			addressMenu = new ContextMenuStrip ();
			addressMenu.ShowImageMargin = false;

			AutomatFlag_CheckedChanged (null, null);

			// В последнюю очередь, т.к. запускает изменение прочих состояний
			/*BlankTypeCombo.Items.Add (blankNames[registrationMode][0]);
			BlankTypeCombo.Items.Add (blankNames[registrationChangeMode][0]);
			BlankTypeCombo.Items.Add (blankNames[unregistrationMode][0]);*/
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
			/*OFDNameLabel.Enabled = OFDNameCombo.Enabled = (OFDVariantCombo.SelectedIndex % 2 == 0) &&
				OFDVariantCombo.Enabled;*/

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
			switch (KAPRSupport.CheckFields (fieldsSet, IsRegistrationChange, AddressIndexField.Enabled,
				FNOpenDateField.Enabled && FNCloseDateField.Enabled))
				{
				case -1:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"ИНН пользователя должен быть числом длиной в 10 (организация) или 12 " +
						"(предприниматель) цифр." + KAPRSupport.LeftEmptyAlias);
					return;

				case -2:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"ОГРН пользователя должен быть числом длиной в 13 (организация) или 15 " +
						"(предприниматель) цифр." + KAPRSupport.LeftEmptyAlias);
					return;

				case -3:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"КПП должен быть числом длиной в 9 цифр. Кроме того, он не должен быть указан, " +
						"если пользователь является индивидуальным предпринимателем." + KAPRSupport.LeftEmptyAlias);
					return;

				case -4:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Заводской номер ФН должен быть числом, состоящим из 16 цифр." + KAPRSupport.LeftEmptyAlias);
					return;

				case -5:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Регистрационный номер ККТ должен быть числом, состоящим из 16 цифр." + KAPRSupport.LeftEmptyAlias);
					return;

				case -6:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Почтовый индекс должен быть числом, состоящим из 6 цифр. В случае, если регион РФ " +
						"выбран, это поле является обязательным для заполнения." + KAPRSupport.LeftEmptyAlias);
					return;

				case -7:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Номер фискального документа (ФД) должен быть числом между 1 и 250000 " +
						"(включительно)." + KAPRSupport.LeftEmptyAlias);
					return;

				case -8:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Фискальный признак документа (ФПД) должен быть числом между 1 и " + 0xFFFFFFFF.ToString () +
						" (включительно)." + KAPRSupport.LeftEmptyAlias);
					return;

				case -9:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Не указана ни одна из причин перерегистрации");
					return;

				case -10:
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Указанный регистрационный номер не соответствует ИНН пользователя и ЗН ККТ");
					return;

				case -11:
					if (RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.LockSmallSize,
						"Отчёт о (пере)регистрации старше отчёта о закрытии архива ФН. Если это – ожидаемая " +
						"ситуация, и все реквизиты заполнены корректно, нажмите кнопку «Далее»",
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next),
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel)) != RDMessageButtons.ButtonOne)
						return;

					break;
				}

			// Формирование заявления
			string template = KAPRSupport.BuildTemplate (fieldsSet, kb);

			/*// Контроль ИНН, если указан
			bool checkRNM = IsRegistrationChange;
			if (!string.IsNullOrWhiteSpace (INNField.Text))
				{
				if ((INNField.Text.Length != 10) && (INNField.Text.Length != 12) ||
					!UInt64.TryParse (INNField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"ИНН пользователя должен быть числом длиной в 10 (организация) или 12 " +
						"(предприниматель) цифр." + leftEmpty);
					return;
					}
				}
			else
				{
				checkRNM = false;
				}*/

			/*// Контроль ОГРН, если указан
			if (!string.IsNullOrWhiteSpace (OGRNField.Text))
				{
				if ((OGRNField.Text.Length != 13) && (OGRNField.Text.Length != 15) ||
					!UInt64.TryParse (OGRNField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"ОГРН пользователя должен быть числом длиной в 13 (организация) или 15 " +
						"(предприниматель) цифр." + leftEmpty);
					return;
					}
				}*/

			/*// Контроль КПП, если указан
			if (!string.IsNullOrWhiteSpace (KPPField.Text))
				{
				if ((KPPField.Text.Length != 9) || !UInt64.TryParse (KPPField.Text, out _) ||
					(INNField.Text.Length > 10) || (OGRNField.Text.Length > 13))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"КПП должен быть числом длиной в 9 цифр. Кроме того, он не должен быть указан, " +
						"если пользователь является индивидуальным предпринимателем." + leftEmpty);
					return;
					}
				}*/

			/*// Контроль ЗН ФН, если указан
			if (!string.IsNullOrWhiteSpace (FNSerialField.Text))
				{
				if ((FNSerialField.Text.Length != 16) || !UInt64.TryParse (FNSerialField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Заводской номер ФН должен быть числом, состоящим из 16 цифр." + leftEmpty);
					return;
					}
				}*/

			/*// Контроль РНМ, если указан
			if (!string.IsNullOrWhiteSpace (KKTRNMField.Text))
				{
				if ((KKTRNMField.Text.Length != 16) || !UInt64.TryParse (KKTRNMField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Регистрационный номер ККТ должен быть числом, состоящим из 16 цифр." + leftEmpty);
					return;
					}
				}
			else
				{
				checkRNM = false;
				}*/

			/*// Контроль почтового индекса, если указан или если выбран регион РФ (при условии, что адрес требуется)
			if (AddressIndexField.Enabled && (!string.IsNullOrWhiteSpace (AddressIndexField.Text) ||
				(AddressRegionCodeCombo.SelectedIndex > 0)))
				{
				if ((AddressIndexField.Text.Length != 6) || !UInt64.TryParse (AddressIndexField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Почтовый индекс должен быть числом, состоящим из 6 цифр. В случае, если регион РФ " +
						"выбран, это поле является обязательным для заполнения." + leftEmpty);
					return;
					}
				}*/

			/*// Контроль ФД, если указаны
			uint v1, v2;
			if (string.IsNullOrWhiteSpace (FNOpenFDField.Text))
				{
				v1 = 1;
				}
			else
				{
				try
					{
					v1 = uint.Parse (FNOpenFDField.Text);
					}
				catch
					{
					v1 = 0;
					}
				}

			if (string.IsNullOrWhiteSpace (FNCloseFDField.Text))
				{
				v2 = 1;
				}
			else
				{
				try
					{
					v2 = uint.Parse (FNCloseFDField.Text);
					}
				catch
					{
					v2 = 0;
					}
				}

			if ((v1 < 1) || (v1 > 250000) || (v2 < 1) || (v2 > 250000))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Номер фискального документа (ФД) должен быть числом между 1 и 250000 " +
					"(включительно)." + leftEmpty);
				return;
				}*/

			/*// Контроль ФПД, если указаны
			if (string.IsNullOrWhiteSpace (FNOpenFPDField.Text))
				{
				v1 = 1;
				}
			else
				{
				try
					{
					v1 = uint.Parse (FNOpenFPDField.Text);
					}
				catch
					{
					v1 = 0;
					}
				}

			if (string.IsNullOrWhiteSpace (FNCloseFPDField.Text))
				{
				v2 = 1;
				}
			else
				{
				try
					{
					v2 = uint.Parse (FNCloseFPDField.Text);
					}
				catch
					{
					v2 = 0;
					}
				}

			if ((v1 < 1) || (v1 > 0xFFFFFFFF) || (v2 < 1) || (v2 > 0xFFFFFFFF))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Фискальный признак документа (ФПД) должен быть числом между 1 и " + 0xFFFFFFFF.ToString () +
					" (включительно)." + leftEmpty);
				return;
				}*/

			/*// Контроль причин перерегистрации
			if (IsRegistrationChange && 
				((OFDVariantCombo.SelectedIndex == continueWithOFD) || (OFDVariantCombo.SelectedIndex == continueWithoutOFD)) &&
				!AddressPlaceChangeFlag.Checked &&
				!AutomatChangeFlag.Checked && !FNChangeFlag.Checked &&
				!UserNameChangeFlag.Checked && !OtherChangeFlag.Checked)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Не указана ни одна из причин перерегистрации");
				return;
				}*/

			/*// Контроль РНМ по КС
			if (checkRNM && !string.IsNullOrWhiteSpace (KKTSerialField.Text) &&
				(KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (INNField.Text, KKTSerialField.Text,
				KKTRNMField.Text.Substring (0, 10)) != KKTRNMField.Text))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Указанный регистрационный номер не соответствует ИНН пользователя и ЗН ККТ");
				return;
				}*/

			/*// Контроль последовательности ФД
			if (FNOpenDateField.Enabled && FNCloseDateField.Enabled &&
				(FNCloseDateField.Value.Year > FNCloseDateField.MinDate.Year) &&
				(FNOpenDateField.Value.Year > FNOpenDateField.MinDate.Year) &&

				(FNOpenDateField.Value < FNCloseDateField.Value))
				{
				if (RDInterface.MessageBox (RDMessageFlags.Question | RDMessageFlags.LockSmallSize,
					"Отчёт о (пере)регистрации старше отчёта о закрытии архива ФН. Если это – ожидаемая " +
					"ситуация, и все реквизиты заполнены корректно, нажмите кнопку «Далее»",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel)) != RDMessageButtons.ButtonOne)
					return;
				}*/

			/*byte[] tmp;
			if (IsUnregistration)
				tmp = KassArrayPRResources.Blk1110062;
			else
				tmp = KassArrayPRResources.Blk1110061;
			KAPRSupport.Template = RDGenerics.GetEncoding (RDEncodings.Unicode16).GetString (tmp);

			KAPRSupport.ApplyField (BlankFields.OGRN, OGRNField.Text);
			KAPRSupport.ApplyField (BlankFields.INN, INNField.Text);
			KAPRSupport.ApplyField (BlankFields.KPP, KPPField.Text);

			if (IsRegistrationChange)
				{
				KAPRSupport.ApplyField (BlankFields.BlankType, "2");
				KAPRSupport.ApplyField (BlankFields.OFDChangeFlag, OFDVariantCombo.SelectedIndex == changeOFD);
				KAPRSupport.ApplyField (BlankFields.ToOFDFlag, OFDVariantCombo.SelectedIndex == addOFD);
				KAPRSupport.ApplyField (BlankFields.FromOFDFlag, OFDVariantCombo.SelectedIndex == removeOFD);
				}
			else if (!IsUnregistration)
				{
				KAPRSupport.ApplyField (BlankFields.BlankType, "1");
				KAPRSupport.ApplyField (BlankFields.OFDChangeFlag, false);
				KAPRSupport.ApplyField (BlankFields.ToOFDFlag, false);
				KAPRSupport.ApplyField (BlankFields.FromOFDFlag, false);
				}
			KAPRSupport.ApplyField (BlankFields.AddressPlaceChangeFlag, AddressPlaceChangeFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.AutomatChangeFlag, AutomatChangeFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.FNChangeFlag, FNChangeFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.NameChangeFlag, UserNameChangeFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.OtherChangeFlag, OtherChangeFlag.Checked);*/

			/*string field = UserNameField.Text;
			KAPRSupport.ApplyField (BlankFields.UserName_Line1, field);
			field = KAPRSupport.TrimField (BlankFields.UserName_Line1, field);
			KAPRSupport.ApplyField (BlankFields.UserName_Line2, field);
			field = KAPRSupport.TrimField (BlankFields.UserName_Line2, field);
			KAPRSupport.ApplyField (BlankFields.UserName_Line3, field);

			KAPRSupport.ApplyField (BlankFields.UserPresenterType, PresenterTypeFlag.Checked ? "2" : "1");
			field = PresenterTypeField.Text;
			KAPRSupport.ApplyField (BlankFields.UserPresenter_Line1, field);
			field = KAPRSupport.TrimField (BlankFields.UserPresenter_Line1, field);
			KAPRSupport.ApplyField (BlankFields.UserPresenter_Line2, field);
			field = KAPRSupport.TrimField (BlankFields.UserPresenter_Line2, field);
			KAPRSupport.ApplyField (BlankFields.UserPresenter_Line3, field);
			KAPRSupport.ApplyField (BlankFields.CurrentDate, DateTime.Now.ToString ("dd.MM.yyyy"));
			KAPRSupport.ApplyField (BlankFields.AppendixesCount, "");*/

			/*int l;
			if (IsRegistrationChange)
				{
				KAPRSupport.ApplyField (BlankFields.RegistrationNumber, KKTRNMField.Text);
				}
			else if (!IsUnregistration)
				{
				// Требует ручного заполнения сотрудником ФНС при первичной регистрации
				l = (int)KAPRSupport.GetFieldLength (BlankFields.RegistrationNumber);
				KAPRSupport.ApplyField (BlankFields.RegistrationNumber, " ".PadLeft (l, ' '));
				}

			field = (KKTModelCombo.SelectedIndex == 0) ? "" : KKTModelCombo.Text;
			int idx = field.IndexOf ('(');
			if (idx > 0)
				field = field.Substring (0, idx);
			KAPRSupport.ApplyField (BlankFields.KKTModelName, field);
			KAPRSupport.ApplyField (BlankFields.KKTSerialNumber_Line1, KKTSerialField.Text);
			KAPRSupport.ApplyField (BlankFields.KKTSerialNumber_Line2, "");*/

			/*if (FNModelCombo.SelectedIndex == 0)
				{
				field = "";
				}
			else
				{
				field = FNModelCombo.Text;
				idx = field.IndexOf (',');
				if (idx > 0)
					field = field.Substring (0, idx);

				field = field.Replace (" ", " исполнение ");
				field = "Шифровальное (криптографическое) средство защиты фискальных данных " +
					"фискальный накопитель " + field;
				}
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line1, field);
			field = KAPRSupport.TrimField (BlankFields.FNModelName_Line1, field);
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line2, field);
			field = KAPRSupport.TrimField (BlankFields.FNModelName_Line2, field);
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line3, field);
			field = KAPRSupport.TrimField (BlankFields.FNModelName_Line3, field);
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line4, field);
			field = KAPRSupport.TrimField (BlankFields.FNModelName_Line4, field);
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line5, field);
			field = KAPRSupport.TrimField (BlankFields.FNModelName_Line5, field);
			KAPRSupport.ApplyField (BlankFields.FNModelName_Line6, field);
			KAPRSupport.ApplyField (BlankFields.FNSerialNumber, FNSerialField.Text);*/

			/*l = (int)KAPRSupport.GetFieldLength (BlankFields.UserAddressRegionCode);
			KAPRSupport.ApplyField (BlankFields.UserAddressRegionCode,
				(AddressRegionCodeCombo.SelectedIndex == 0) || !AddressRegionCodeCombo.Enabled ? "" :
				AddressRegionCodeCombo.Text.Substring (0, l));
			KAPRSupport.ApplyField (BlankFields.UserAddressIndex, AddressIndexField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressArea, AddressAreaField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressCity, AddressCityField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressTown, AddressTownField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressStreet, AddressStreetField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressHouseNumber, AddressHouseField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressBuildingNumber, AddressBuildingField.Text);
			KAPRSupport.ApplyField (BlankFields.UserAddressAppartmentNumber, AddressAppartmentField.Text);

			field = PlaceField.Text;
			KAPRSupport.ApplyField (BlankFields.UserPlace_Line1, field);
			field = KAPRSupport.TrimField (BlankFields.UserPlace_Line1, field);
			KAPRSupport.ApplyField (BlankFields.UserPlace_Line2, field);
			field = KAPRSupport.TrimField (BlankFields.UserPlace_Line2, field);
			KAPRSupport.ApplyField (BlankFields.UserPlace_Line3, field);
			field = KAPRSupport.TrimField (BlankFields.UserPlace_Line3, field);
			KAPRSupport.ApplyField (BlankFields.UserPlace_Line4, field);*/

			/*KAPRSupport.ApplyField (BlankFields.AutonomousFlag,
				(OFDVariantCombo.SelectedIndex == continueWithoutOFD) || (OFDVariantCombo.SelectedIndex == removeOFD));
			KAPRSupport.ApplyField (BlankFields.LotteryFlag, LotteryFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.GamblingFlag, GamblingFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.GamblingExchangeFlag, GamblingExchangeFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.BankPaymentAgentFlag, BankAgentFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.PaymentAgentFlag, AgentFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.AutomatFlag, AutomatFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.MarkFlag, MarkFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.InternetFlag, InternetFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.DeliveryFlag, DeliveryFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.BSOFlag, BSOFlag.Checked ? "1" : "");
			KAPRSupport.ApplyField (BlankFields.ExciseFlag, ExciseFlag.Checked);

			KAPRSupport.ApplyField (BlankFields.AutomatNumber, AutomatNumberField.Text);
			KAPRSupport.ApplyField (BlankFields.SecondAutomatNumber, "");*/

			/*if (AutomatAddressIsSame.Checked)
				{
				l = (int)KAPRSupport.GetFieldLength (BlankFields.AutomatAddressRegionCode);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressRegionCode,
					(AddressRegionCodeCombo.SelectedIndex == 0) || !AddressRegionCodeCombo.Enabled ? "" :
					AddressRegionCodeCombo.Text.Substring (0, l));

				KAPRSupport.ApplyField (BlankFields.AutomatAddressIndex, AddressIndexField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressArea, AddressAreaField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressCity, AddressCityField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressTown, AddressTownField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressStreet, AddressStreetField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressHouseNumber, AddressHouseField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressBuildingNumber, AddressBuildingField.Text);
				KAPRSupport.ApplyField (BlankFields.AutomatAddressAppartmentNumber, AddressAppartmentField.Text);

				field = PlaceField.Text;
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line1, field);
				field = KAPRSupport.TrimField (BlankFields.AutomatPlace_Line1, field);
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line2, field);
				field = KAPRSupport.TrimField (BlankFields.AutomatPlace_Line2, field);
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line3, field);
				}
			else
				{
				KAPRSupport.ApplyField (BlankFields.AutomatAddressRegionCode, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressIndex, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressArea, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressCity, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressTown, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressStreet, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressHouseNumber, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressBuildingNumber, "");
				KAPRSupport.ApplyField (BlankFields.AutomatAddressAppartmentNumber, "");
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line1, "");
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line2, "");
				KAPRSupport.ApplyField (BlankFields.AutomatPlace_Line3, "");
				}*/

			/*if ((OFDVariantCombo.SelectedIndex == continueWithoutOFD) || (OFDVariantCombo.SelectedIndex == removeOFD))
				{
				l = (int)KAPRSupport.GetFieldLength (BlankFields.OFDINN);
				KAPRSupport.ApplyField (BlankFields.OFDINN, "0".PadLeft (l, '0'));
				}
			else
				{
				field = kb.Ofd.GetOFDINNByName (OFDNameCombo.Text);
				KAPRSupport.ApplyField (BlankFields.OFDINN, field);
				}

			field = KAPRSupport.GetOFDFullName (field);
			KAPRSupport.ApplyField (BlankFields.OFDName_Line1, field);
			field = KAPRSupport.TrimField (BlankFields.OFDName_Line1, field);
			KAPRSupport.ApplyField (BlankFields.OFDName_Line2, field);
			field = KAPRSupport.TrimField (BlankFields.OFDName_Line2, field);
			KAPRSupport.ApplyField (BlankFields.OFDName_Line3, field);
			field = KAPRSupport.TrimField (BlankFields.OFDName_Line3, field);
			KAPRSupport.ApplyField (BlankFields.OFDName_Line4, field);

			KAPRSupport.ApplyField (BlankFields.FNBrokenFlag, FNBrokenFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.KKTStolenFlag, KKTStolenFlag.Checked);
			KAPRSupport.ApplyField (BlankFields.KKTMissingFlag, KKTMissingFlag.Checked);*/

			/*string dateFiller = KassArrayPRSettings.DontAddStrikeouts ? "  .  .    " : "--.--.----";
			string timeFiller = KassArrayPRSettings.DontAddStrikeouts ? "  :  " : "--:--";

			KAPRSupport.ApplyField (BlankFields.FNOpenDocumentNumber, FNOpenFDField.Text);
			if (FNOpenDateField.Value.Year > FNOpenDateField.MinDate.Year)
				{
				KAPRSupport.ApplyField (BlankFields.FNOpenDate, FNOpenDateField.Value.ToString ("dd.MM.yyyy"));
				KAPRSupport.ApplyField (BlankFields.FNOpenTime, FNOpenDateField.Value.ToString ("HH:mm"));
				}
			else
				{
				KAPRSupport.ApplyField (BlankFields.FNOpenDate, dateFiller);
				KAPRSupport.ApplyField (BlankFields.FNOpenTime, timeFiller);
				}

			if (string.IsNullOrWhiteSpace (FNOpenFPDField.Text))
				{
				KAPRSupport.ApplyField (BlankFields.FNOpenDocumentSign, "");
				}
			else
				{
				l = (int)KAPRSupport.GetFieldLength (BlankFields.FNOpenDocumentSign);
				KAPRSupport.ApplyField (BlankFields.FNOpenDocumentSign, FNOpenFPDField.Text.PadLeft (l, '0'));
				}*/

			/*KAPRSupport.ApplyField (BlankFields.FNCloseDocumentNumber, FNCloseFDField.Text);
			if (FNCloseDateField.Value.Year > FNCloseDateField.MinDate.Year)
				{
				KAPRSupport.ApplyField (BlankFields.FNCloseDate, FNCloseDateField.Value.ToString ("dd.MM.yyyy"));
				KAPRSupport.ApplyField (BlankFields.FNCloseTime, FNCloseDateField.Value.ToString ("HH:mm"));
				}
			else
				{
				KAPRSupport.ApplyField (BlankFields.FNCloseDate, dateFiller);
				KAPRSupport.ApplyField (BlankFields.FNCloseTime, timeFiller);
				}

			if (string.IsNullOrWhiteSpace (FNCloseFPDField.Text))
				{
				KAPRSupport.ApplyField (BlankFields.FNCloseDocumentSign, "");
				}
			else
				{
				l = (int)KAPRSupport.GetFieldLength (BlankFields.FNCloseDocumentSign);
				KAPRSupport.ApplyField (BlankFields.FNCloseDocumentSign, FNCloseFPDField.Text.PadLeft (l, '0'));
				}*/

			/*if (KassArrayPRSettings.AddSignDate)
				{
				KAPRSupport.ApplyField (BlankFields.SignDate, DateTime.Now.ToString ("dd.MM.yyyy"));
				}
			else
				{
				l = (int)KAPRSupport.GetFieldLength (BlankFields.SignDate);
				KAPRSupport.ApplyField (BlankFields.SignDate, "_".PadLeft (l, '_'));
				}*/

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

			string[] values = KAPRSupport.ParseFile (file, kb);
			if (values == null)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Указанный файл повреждён или не является поддерживаемым файлом заявления");
				return;
				}

			for (int i = 0; i < values.Length; i++)
				fieldsSet[i] = values[i];

			// Загрузка
			/*try
				{
				FNChangeFlag.Checked = fields[(int)BlankFields.FNChangeFlag] == "1";
				UserNameField.Text = fields[(int)BlankFields.UserName_Line1];
				INNField.Text = fields[(int)BlankFields.INN];
				OGRNField.Text = fields[(int)BlankFields.OGRN];
				KPPField.Text = fields[(int)BlankFields.KPP];
				PresenterTypeField.Text = fields[(int)BlankFields.UserPresenter_Line1];
				PresenterTypeFlag.Checked = fields[(int)BlankFields.UserPresenterType] == "1";
				UserNameChangeFlag.Checked = fields[(int)BlankFields.NameChangeFlag] == "1";
				KKTStolenFlag.Checked = fields[(int)BlankFields.KKTStolenFlag] == "1";
				KKTMissingFlag.Checked = fields[(int)BlankFields.KKTMissingFlag] == "1";
				FNBrokenFlag.Checked = fields[(int)BlankFields.FNBrokenFlag] == "1";

				KKTSerialField.Text = fields[(int)BlankFields.KKTSerialNumber_Line1];
				FNSerialField.Text = fields[(int)BlankFields.FNSerialNumber];

				if (version <= KBFVersions.V2)
					{
					KKTModelCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.KKTModelName]);
					FNModelCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.FNModelName_Line1]);
					OFDNameCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.OFDName_Line1]);
					}
				else
					{
					KKTModelCombo.Text = fields[(int)BlankFields.KKTModelName];
					FNModelCombo.Text = fields[(int)BlankFields.FNModelName_Line1];
					OFDNameCombo.Text = fields[(int)BlankFields.OFDName_Line1];
					}

				OFDVariantCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.OFDChangeFlag]); // !!!
				KKTRNMField.Text = fields[(int)BlankFields.RegistrationNumber];

				AddressRegionCodeCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.UserAddressRegionCode]);
				AddressAreaField.Text = fields[(int)BlankFields.UserAddressArea];
				AddressCityField.Text = fields[(int)BlankFields.UserAddressCity];
				AddressTownField.Text = fields[(int)BlankFields.UserAddressTown];
				AddressIndexField.Text = fields[(int)BlankFields.UserAddressIndex];
				AddressStreetField.Text = fields[(int)BlankFields.UserAddressStreet];
				AddressHouseField.Text = fields[(int)BlankFields.UserAddressHouseNumber];
				AddressBuildingField.Text = fields[(int)BlankFields.UserAddressBuildingNumber];
				AddressAppartmentField.Text = fields[(int)BlankFields.UserAddressAppartmentNumber];
				PlaceField.Text = fields[(int)BlankFields.UserPlace_Line1];
				AddressPlaceChangeFlag.Checked = fields[(int)BlankFields.AddressPlaceChangeFlag] == "1";

				LotteryFlag.Checked = fields[(int)BlankFields.LotteryFlag] == "1";
				GamblingFlag.Checked = fields[(int)BlankFields.GamblingFlag] == "1";
				GamblingExchangeFlag.Checked = fields[(int)BlankFields.GamblingExchangeFlag] == "1";
				BSOFlag.Checked = fields[(int)BlankFields.BSOFlag] == "1";
				BankAgentFlag.Checked = fields[(int)BlankFields.BankPaymentAgentFlag] == "1";
				AgentFlag.Checked = fields[(int)BlankFields.PaymentAgentFlag] == "1";
				DeliveryFlag.Checked = fields[(int)BlankFields.DeliveryFlag] == "1";
				ExciseFlag.Checked = fields[(int)BlankFields.ExciseFlag] == "1";
				MarkFlag.Checked = fields[(int)BlankFields.MarkFlag] == "1";
				InternetFlag.Checked = fields[(int)BlankFields.InternetFlag] == "1";
				OtherChangeFlag.Checked = fields[(int)BlankFields.OtherChangeFlag] == "1";
				AutomatNumberField.Text = fields[(int)BlankFields.AutomatNumber];
				AutomatAddressIsSame.Checked = fields[(int)BlankFields.AutomatPlace_Line3] == "1";  // !!!
				AutomatChangeFlag.Checked = fields[(int)BlankFields.AutomatChangeFlag] == "1";
				AutomatFlag.Checked = fields[(int)BlankFields.AutomatFlag] == "1";

				FNCloseFDField.Text = fields[(int)BlankFields.FNCloseDocumentNumber];
				FNCloseDateField.Value = DateTime.Parse (fields[(int)BlankFields.FNCloseDate], RDLocale.GetCulture (RDLanguages.ru_ru));
				FNCloseFPDField.Text = fields[(int)BlankFields.FNCloseDocumentSign];
				FNOpenFDField.Text = fields[(int)BlankFields.FNOpenDocumentNumber];
				FNOpenDateField.Value = DateTime.Parse (fields[(int)BlankFields.FNOpenDate], RDLocale.GetCulture (RDLanguages.ru_ru));
				FNOpenFPDField.Text = fields[(int)BlankFields.FNOpenDocumentSign];

				// В последнюю очередь, поскольку событие запускает внутренние проверки
				BlankTypeCombo.SelectedIndex = int.Parse (fields[(int)BlankFields.BlankType]);
				}
			catch
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText | RDMessageFlags.LockSmallSize,
					"Файл заявления повреждён и не может быть загружен полностью. Проверьте поля заявления перед формированием");
				return;
				}*/
			LoadFields ();
			}

		private void LoadFields ()
			{
			// 11 + 5 + 2 + 11 + 15 + 6 + 2 = 52
			try
				{
				FNChangeFlag.Checked = fieldsSet[(int)KBFFields.FNChangeFlag] == "1";
				UserNameField.Text = fieldsSet[(int)KBFFields.UserName];
				INNField.Text = fieldsSet[(int)KBFFields.INN];
				OGRNField.Text = fieldsSet[(int)KBFFields.OGRN];
				KPPField.Text = fieldsSet[(int)KBFFields.KPP];
				PresenterTypeField.Text = fieldsSet[(int)KBFFields.UserPresenter];
				PresenterTypeFlag.Checked = fieldsSet[(int)KBFFields.UserPresenterType] == "1";
				UserNameChangeFlag.Checked = fieldsSet[(int)KBFFields.NameChangeFlag] == "1";
				KKTStolenFlag.Checked = fieldsSet[(int)KBFFields.KKTStolenFlag] == "1";
				KKTMissingFlag.Checked = fieldsSet[(int)KBFFields.KKTMissingFlag] == "1";
				FNBrokenFlag.Checked = fieldsSet[(int)KBFFields.FNBrokenFlag] == "1";

				KKTSerialField.Text = fieldsSet[(int)KBFFields.KKTSerialNumber];
				FNSerialField.Text = fieldsSet[(int)KBFFields.FNSerialNumber];
				KKTModelCombo.Text = fieldsSet[(int)KBFFields.KKTModelName];
				FNModelCombo.Text = fieldsSet[(int)KBFFields.FNModelName];
				OFDNameCombo.Text = fieldsSet[(int)KBFFields.OFDName];

				OFDVariantCombo.SelectedIndex = int.Parse (fieldsSet[(int)KBFFields.OFDVariant]);
				KKTRNMField.Text = fieldsSet[(int)KBFFields.RegistrationNumber];

				AddressRegionCodeCombo.SelectedIndex = int.Parse (fieldsSet[(int)KBFFields.AddressRegionCode]);
				AddressAreaField.Text = fieldsSet[(int)KBFFields.AddressArea];
				AddressCityField.Text = fieldsSet[(int)KBFFields.AddressCity];
				AddressTownField.Text = fieldsSet[(int)KBFFields.AddressTown];
				AddressIndexField.Text = fieldsSet[(int)KBFFields.AddressIndex];
				AddressStreetField.Text = fieldsSet[(int)KBFFields.AddressStreet];
				AddressHouseField.Text = fieldsSet[(int)KBFFields.AddressHouseNumber];
				AddressBuildingField.Text = fieldsSet[(int)KBFFields.AddressBuildingNumber];
				AddressAppartmentField.Text = fieldsSet[(int)KBFFields.AddressAppartmentNumber];
				PlaceField.Text = fieldsSet[(int)KBFFields.Place];
				AddressPlaceChangeFlag.Checked = fieldsSet[(int)KBFFields.AddressPlaceChangeFlag] == "1";

				LotteryFlag.Checked = fieldsSet[(int)KBFFields.LotteryFlag] == "1";
				GamblingFlag.Checked = fieldsSet[(int)KBFFields.GamblingFlag] == "1";
				GamblingExchangeFlag.Checked = fieldsSet[(int)KBFFields.GamblingExchangeFlag] == "1";
				BSOFlag.Checked = fieldsSet[(int)KBFFields.BSOFlag] == "1";
				BankAgentFlag.Checked = fieldsSet[(int)KBFFields.BankPaymentAgentFlag] == "1";
				AgentFlag.Checked = fieldsSet[(int)KBFFields.PaymentAgentFlag] == "1";
				DeliveryFlag.Checked = fieldsSet[(int)KBFFields.DeliveryFlag] == "1";
				ExciseFlag.Checked = fieldsSet[(int)KBFFields.ExciseFlag] == "1";
				MarkFlag.Checked = fieldsSet[(int)KBFFields.MarkFlag] == "1";
				InternetFlag.Checked = fieldsSet[(int)KBFFields.InternetFlag] == "1";
				OtherChangeFlag.Checked = fieldsSet[(int)KBFFields.OtherChangeFlag] == "1";
				AutomatFlag.Checked = fieldsSet[(int)KBFFields.AutomatFlag] == "1";	// Должен предшествовать остальным
				AutomatNumberField.Text = fieldsSet[(int)KBFFields.AutomatNumber];
				AutomatAddressIsSame.Checked = fieldsSet[(int)KBFFields.AutomatAddressIsSameFlag] == "1";
				AutomatChangeFlag.Checked = fieldsSet[(int)KBFFields.AutomatChangeFlag] == "1";

				FNCloseFDField.Text = fieldsSet[(int)KBFFields.FNCloseDocumentNumber];
				FNCloseDateField.Value = DateTime.Parse (fieldsSet[(int)KBFFields.FNCloseDate], RDLocale.GetCulture (RDLanguages.ru_ru));
				FNCloseFPDField.Text = fieldsSet[(int)KBFFields.FNCloseDocumentSign];
				FNOpenFDField.Text = fieldsSet[(int)KBFFields.FNOpenDocumentNumber];
				FNOpenDateField.Value = DateTime.Parse (fieldsSet[(int)KBFFields.FNOpenDate], RDLocale.GetCulture (RDLanguages.ru_ru));
				FNOpenFPDField.Text = fieldsSet[(int)KBFFields.FNOpenDocumentSign];

				// В последнюю очередь, поскольку событие запускает внутренние проверки
				BlankTypeCombo.SelectedIndex = int.Parse (fieldsSet[(int)KBFFields.BlankType]);
				// Версия файла здесь игнорируется
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
			/*SFDialog.FileName = RecommendedFileName;*/
			SFDialog.FileName = KAPRSupport.GetRecommendedFileName (UserNameField.Text,
				(BlankTypes)BlankTypeCombo.SelectedIndex);
			SFDialog.ShowDialog ();
			}

		private void SFDialog_FileOk (object sender, CancelEventArgs e)
			{
			// Формирование списка
			/*string[] fields = new string[kbfFieldsCount[kbfFieldsCount.Length - 1] + 1];
			fields[0] = ((uint)KBFVersions.V4).ToString ();

			fields[(int)BlankFields.BlankType] = BlankTypeCombo.SelectedIndex.ToString ();
			fields[(int)BlankFields.FNChangeFlag] = FNChangeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.UserName_Line1] = UserNameField.Text;
			fields[(int)BlankFields.INN] = INNField.Text;
			fields[(int)BlankFields.OGRN] = OGRNField.Text;
			fields[(int)BlankFields.KPP] = KPPField.Text;
			fields[(int)BlankFields.UserPresenter_Line1] = PresenterTypeField.Text;
			fields[(int)BlankFields.UserPresenterType] = PresenterTypeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.NameChangeFlag] = UserNameChangeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.KKTStolenFlag] = KKTStolenFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.KKTMissingFlag] = KKTMissingFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.FNBrokenFlag] = FNBrokenFlag.Checked ? "1" : "0";

			fields[(int)BlankFields.KKTSerialNumber_Line1] = KKTSerialField.Text;
			fields[(int)BlankFields.KKTModelName] = KKTModelCombo.Text;
			fields[(int)BlankFields.FNSerialNumber] = FNSerialField.Text;
			fields[(int)BlankFields.FNModelName_Line1] = FNModelCombo.Text;
			fields[(int)BlankFields.OFDName_Line1] = OFDNameCombo.Text;

			fields[(int)BlankFields.OFDChangeFlag] = OFDVariantCombo.SelectedIndex.ToString (); // !!!
			fields[(int)BlankFields.RegistrationNumber] = KKTRNMField.Text;

			fields[(int)BlankFields.UserAddressRegionCode] = AddressRegionCodeCombo.SelectedIndex.ToString ();
			fields[(int)BlankFields.UserAddressArea] = AddressAreaField.Text;
			fields[(int)BlankFields.UserAddressCity] = AddressCityField.Text;
			fields[(int)BlankFields.UserAddressTown] = AddressTownField.Text;
			fields[(int)BlankFields.UserAddressIndex] = AddressIndexField.Text;
			fields[(int)BlankFields.UserAddressStreet] = AddressStreetField.Text;
			fields[(int)BlankFields.UserAddressHouseNumber] = AddressHouseField.Text;
			fields[(int)BlankFields.UserAddressBuildingNumber] = AddressBuildingField.Text;
			fields[(int)BlankFields.UserAddressAppartmentNumber] = AddressAppartmentField.Text;
			fields[(int)BlankFields.UserPlace_Line1] = PlaceField.Text;
			fields[(int)BlankFields.AddressPlaceChangeFlag] = AddressPlaceChangeFlag.Checked ? "1" : "0";

			fields[(int)BlankFields.LotteryFlag] = LotteryFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.GamblingFlag] = GamblingFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.GamblingExchangeFlag] = GamblingExchangeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.BSOFlag] = BSOFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.BankPaymentAgentFlag] = BankAgentFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.PaymentAgentFlag] = AgentFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.DeliveryFlag] = DeliveryFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.ExciseFlag] = ExciseFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.MarkFlag] = MarkFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.InternetFlag] = InternetFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.OtherChangeFlag] = OtherChangeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.AutomatNumber] = AutomatNumberField.Text;
			fields[(int)BlankFields.AutomatPlace_Line3] = AutomatAddressIsSame.Checked ? "1" : "0"; // !!!
			fields[(int)BlankFields.AutomatChangeFlag] = AutomatChangeFlag.Checked ? "1" : "0";
			fields[(int)BlankFields.AutomatFlag] = AutomatFlag.Checked ? "1" : "0";

			fields[(int)BlankFields.FNCloseDocumentNumber] = FNCloseFDField.Text;
			fields[(int)BlankFields.FNCloseDate] = FNCloseDateField.Value.ToString (RDLocale.GetCulture (RDLanguages.ru_ru));
			fields[(int)BlankFields.FNCloseDocumentSign] = FNCloseFPDField.Text;
			fields[(int)BlankFields.FNOpenDocumentNumber] = FNOpenFDField.Text;
			fields[(int)BlankFields.FNOpenDate] = FNOpenDateField.Value.ToString (RDLocale.GetCulture (RDLanguages.ru_ru));
			fields[(int)BlankFields.FNOpenDocumentSign] = FNOpenFPDField.Text;*/
			FlushFields ();

			// Запись
			/*string file = "";
			for (int i = 0; i < KAPRSupport.FieldsCount; i++)
				{
				file += fieldsSet[i];
				if (i < KAPRSupport.FieldsCount - 1)
					file += fileSplitter.ToString ();
				}*/

			try
				{
				File.WriteAllText (SFDialog.FileName, KAPRSupport.BuildFile (fieldsSet),
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
			// 13 + 5 + 2 + 11 + 15 + 6 = 52
			fieldsSet[(int)KBFFields.FileVersion] = ((uint)KBFVersions.Actual).ToString ();
			fieldsSet[(int)KBFFields.BlankType] = BlankTypeCombo.SelectedIndex.ToString ();
			fieldsSet[(int)KBFFields.FNChangeFlag] = FNChangeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.UserName] = UserNameField.Text;
			fieldsSet[(int)KBFFields.INN] = INNField.Text;
			fieldsSet[(int)KBFFields.OGRN] = OGRNField.Text;
			fieldsSet[(int)KBFFields.KPP] = KPPField.Text;
			fieldsSet[(int)KBFFields.UserPresenter] = PresenterTypeField.Text;
			fieldsSet[(int)KBFFields.UserPresenterType] = PresenterTypeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.NameChangeFlag] = UserNameChangeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.KKTStolenFlag] = KKTStolenFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.KKTMissingFlag] = KKTMissingFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.FNBrokenFlag] = FNBrokenFlag.Checked ? "1" : "0";

			fieldsSet[(int)KBFFields.KKTSerialNumber] = KKTSerialField.Text;
			fieldsSet[(int)KBFFields.KKTModelName] = KKTModelCombo.Text;
			fieldsSet[(int)KBFFields.FNSerialNumber] = FNSerialField.Text;
			fieldsSet[(int)KBFFields.FNModelName] = FNModelCombo.Text;
			fieldsSet[(int)KBFFields.OFDName] = OFDNameCombo.Text;

			fieldsSet[(int)KBFFields.OFDVariant] = OFDVariantCombo.SelectedIndex.ToString ();
			fieldsSet[(int)KBFFields.RegistrationNumber] = KKTRNMField.Text;

			fieldsSet[(int)KBFFields.AddressRegionCode] = AddressRegionCodeCombo.SelectedIndex.ToString ();
			fieldsSet[(int)KBFFields.AddressArea] = AddressAreaField.Text;
			fieldsSet[(int)KBFFields.AddressCity] = AddressCityField.Text;
			fieldsSet[(int)KBFFields.AddressTown] = AddressTownField.Text;
			fieldsSet[(int)KBFFields.AddressIndex] = AddressIndexField.Text;
			fieldsSet[(int)KBFFields.AddressStreet] = AddressStreetField.Text;
			fieldsSet[(int)KBFFields.AddressHouseNumber] = AddressHouseField.Text;
			fieldsSet[(int)KBFFields.AddressBuildingNumber] = AddressBuildingField.Text;
			fieldsSet[(int)KBFFields.AddressAppartmentNumber] = AddressAppartmentField.Text;
			fieldsSet[(int)KBFFields.Place] = PlaceField.Text;
			fieldsSet[(int)KBFFields.AddressPlaceChangeFlag] = AddressPlaceChangeFlag.Checked ? "1" : "0";

			fieldsSet[(int)KBFFields.LotteryFlag] = LotteryFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.GamblingFlag] = GamblingFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.GamblingExchangeFlag] = GamblingExchangeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.BSOFlag] = BSOFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.BankPaymentAgentFlag] = BankAgentFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.PaymentAgentFlag] = AgentFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.DeliveryFlag] = DeliveryFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.ExciseFlag] = ExciseFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.MarkFlag] = MarkFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.InternetFlag] = InternetFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.OtherChangeFlag] = OtherChangeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.AutomatNumber] = AutomatNumberField.Text;
			fieldsSet[(int)KBFFields.AutomatAddressIsSameFlag] = AutomatAddressIsSame.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.AutomatChangeFlag] = AutomatChangeFlag.Checked ? "1" : "0";
			fieldsSet[(int)KBFFields.AutomatFlag] = AutomatFlag.Checked ? "1" : "0";

			fieldsSet[(int)KBFFields.FNCloseDocumentNumber] = FNCloseFDField.Text;
			fieldsSet[(int)KBFFields.FNCloseDate] = FNCloseDateField.Value.ToString (KAPRSupport.FullDateTimeFormat);
			fieldsSet[(int)KBFFields.FNCloseDocumentSign] = FNCloseFPDField.Text;
			fieldsSet[(int)KBFFields.FNOpenDocumentNumber] = FNOpenFDField.Text;
			fieldsSet[(int)KBFFields.FNOpenDate] = FNOpenDateField.Value.ToString (KAPRSupport.FullDateTimeFormat);
			fieldsSet[(int)KBFFields.FNOpenDocumentSign] = FNOpenFPDField.Text;
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
