extern alias KassArrayDB;

using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class KassArrayTBForm: Form
		{
		// Переменные и константы
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;
		private bool closeWindowOnError = false;

		private const string regionPar = "BCRegion";

		// Ресивер сообщений на повторное открытие окна
		private EventWaitHandle ewh;

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		public KassArrayTBForm ()
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

			this.Text = ProgramDescription.AssemblyVisibleName;

			// Настройка контролов
			KKTModelCombo.Items.Add ("(вписывается вручную)");
			KKTModelCombo.Items.AddRange (kb.KKTNumbers.EnumerateAvailableModels ());
			KKTModelCombo.SelectedIndex = 0;

			FNModelCombo.Items.Add ("(вписывается вручную)");
			FNModelCombo.Items.AddRange (kb.FNNumbers.EnumerateAvailableModels ());
			FNModelCombo.SelectedIndex = 0;

			OFDNameCombo.Items.Add ("(вписывается вручную)");
			OFDNameCombo.Items.AddRange (kb.Ofd.GetOFDNames (true).ToArray ());
			OFDNameCombo.SelectedIndex = 0;

			OFDVariantCombo.Items.Add ("Продолжить работу с ОФД");
			OFDVariantCombo.Items.Add ("Продолжить работу без ОФД");
			OFDVariantCombo.Items.Add ("Перейти на работу с ОФД");
			OFDVariantCombo.Items.Add ("Перейти на работу без ОФД");
			OFDVariantCombo.Items.Add ("Сменить ОФД");
			OFDVariantCombo.SelectedIndex = 0;

			AddressRegionCodeCombo.Items.Add ("(вписывается вручную)");
			AddressRegionCodeCombo.Items.AddRange (kb.KKTNumbers.EnumerateAvailableRegions ());
			AddressRegionCodeCombo.SelectedIndex = 0;

			AutomatFlag_CheckedChanged (null, null);

			// В последнюю очередь, т.к. запускает изменение прочих состояний
			BlankTypeCombo.Items.Add ("Заявление о первичной регистрации ККТ");
			BlankTypeCombo.Items.Add ("Заявление о перерегистрации ККТ");
			BlankTypeCombo.Items.Add ("Заявление о снятии ККТ с учёта");
			BlankTypeCombo.SelectedIndex = 0;

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);
			try
				{
				AddressRegionCodeCombo.SelectedIndex = (int)RDGenerics.GetSettings (regionPar, 0);
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
					ewh = EventWaitHandle.OpenExisting (KassArrayDB::RD_AAOW.ProgramDescription.AssemblyTitle);
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
			}

		private void KassArrayTBForm_Shown (object sender, EventArgs e)
			{
			if (closeWindowOnError)
				this.Close ();
			}

		private void KassArrayTBForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Контроль
			if (closeWindowOnError)
				return;

			RDGenerics.SaveWindowDimensions (this);
			RDGenerics.SetSettings (regionPar, (uint)AddressRegionCodeCombo.SelectedIndex);
			}

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

		// Завершение работы
		private void BExit_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		// Поиск пользователя
		private void FindUserButton_Click (object sender, EventArgs e)
			{
			RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
				"ИНН пользователя скопирован в буфер", 1000);
			RDGenerics.SendToClipboard (INNField.Text, false);
			RDGenerics.RunURL ("https://egrul.nalog.ru");
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
			OFDNameLabel.Enabled = OFDNameCombo.Enabled = (OFDVariantCombo.SelectedIndex % 2 == 0) &&
				OFDVariantCombo.Enabled;
			if (!OFDNameLabel.Enabled)
				OFDNameCombo.SelectedIndex = 0;
			}

		// Выбор типа заявления
		private void BlankTypeCombo_SelectedIndexChanged (object sender, EventArgs e)
			{
			bool rereg = (BlankTypeCombo.SelectedIndex == 1);
			bool finish = (BlankTypeCombo.SelectedIndex == 2);

			KKTStolenFlag.Enabled = KKTMissingFlag.Enabled = finish;
			if (!KKTStolenFlag.Enabled)
				KKTStolenFlag.Checked = KKTMissingFlag.Checked = false;
			FNBrokenFlag.Enabled = rereg || finish;
			if (!FNBrokenFlag.Enabled)
				FNBrokenFlag.Checked = false;

			UserNameChangeFlag.Enabled = FNChangeFlag.Enabled =
				KKTRNMLabel.Enabled = KKTRNMField.Enabled =
				AddressPlaceChangeFlag.Enabled = AutomatChangeFlag.Enabled =
				OtherChangeFlag.Enabled = rereg;
			if (!UserNameChangeFlag.Enabled)
				UserNameChangeFlag.Checked = FNChangeFlag.Checked =
				AddressPlaceChangeFlag.Checked = AutomatChangeFlag.Checked =
				OtherChangeFlag.Checked = false;

			FNModelLabel.Enabled = FNModelCombo.Enabled = FNSerialLabel.Enabled = FNSerialField.Enabled = !finish;
			if (!FNModelLabel.Enabled)
				FNModelCombo.SelectedIndex = 0;

			OFDVariantLabel.Enabled = OFDVariantCombo.Enabled = !finish;
			if (!OFDVariantLabel.Enabled)
				OFDVariantCombo.SelectedIndex = 0;
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
				PlaceLabel.Enabled = PlaceField.Enabled = !finish;
			/*if (!AddressRegionCodeLabel.Enabled)
				AddressRegionCodeCombo.SelectedIndex = 0;*/

			LotteryFlag.Enabled = GamblingFlag.Enabled = GamblingExchangeFlag.Enabled =
				BankAgentFlag.Enabled = AgentFlag.Enabled = DeliveryFlag.Enabled =
				MarkFlag.Enabled = ExciseFlag.Enabled = InternetFlag.Enabled =
				BSOFlag.Enabled = AutomatFlag.Enabled = !finish;
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
			bool finish = (BlankTypeCombo.SelectedIndex == 2);

			FNCloseLabel.Enabled = FNCloseFDLabel.Enabled = FNCloseFDField.Enabled =
				FNCloseDateLabel.Enabled = FNCloseDateField.Enabled =
				FNCloseFPDLabel.Enabled = FNCloseFPDField.Enabled =
				(FNChangeFlag.Checked || finish) && !FNBrokenFlag.Checked;
			if (!FNCloseLabel.Enabled)
				{
				FNCloseDateField.Value = FNCloseDateField.MinDate;
				FNCloseFDField.Text = FNCloseFPDField.Text = "";
				}
			}

		private void UpdateFNOpen ()
			{
			bool rereg = (BlankTypeCombo.SelectedIndex == 1);

			FNOpenLabel.Enabled = FNOpenFDLabel.Enabled = FNOpenFDField.Enabled =
				FNOpenDateLabel.Enabled = FNOpenDateField.Enabled =
				FNOpenFPDLabel.Enabled = FNOpenFPDField.Enabled = rereg;
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

		// Формирование заявления
		private void CreateBlank_Click (object sender, EventArgs e)
			{
			bool rereg = (BlankTypeCombo.SelectedIndex == 1);
			bool finish = (BlankTypeCombo.SelectedIndex == 2);

			#region Контроль значений

			bool checkRNM = true;
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
				}

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
				}

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
				}

			if (!string.IsNullOrWhiteSpace (FNSerialField.Text))
				{
				if ((FNSerialField.Text.Length != 16) || !UInt64.TryParse (FNSerialField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Заводской номер ФН должен быть числом, состоящим из 16 цифр." + leftEmpty);
					return;
					}
				}

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
				}

			if (!string.IsNullOrWhiteSpace (AddressIndexField.Text) ||
				(AddressRegionCodeCombo.SelectedIndex > 0))
				{
				if ((AddressIndexField.Text.Length != 6) || !UInt64.TryParse (AddressIndexField.Text, out _))
					{
					RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
						"Почтовый индекс должен быть числом, состоящим из 6 цифр. В случае, если регион РФ " +
						"выбран, это поле является обязательным для заполнения." + leftEmpty);
					return;
					}
				}

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
				}

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
				}

			if (rereg && (OFDVariantCombo.SelectedIndex < 2) && !AddressPlaceChangeFlag.Checked &&
				!AutomatChangeFlag.Checked && !FNChangeFlag.Checked &&
				!UserNameChangeFlag.Checked && !OtherChangeFlag.Checked)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Не указана ни одна из причин перерегистрации");
				return;
				}

			if (checkRNM && !string.IsNullOrWhiteSpace (KKTSerialField.Text) &&
				(KassArrayDB::RD_AAOW.KKTSupport.GetFullRNM (INNField.Text, KKTSerialField.Text,
				KKTRNMField.Text.Substring (0, 10)) != KKTRNMField.Text))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.LockSmallSize,
					"Указанный регистрационный номер не соответствует ИНН пользователя и ЗН ККТ");
				return;
				}

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
				}

			#endregion

			#region Сборка шаблона заявления

			byte[] tmp;
			if (finish)
				tmp = KassArrayTBResources.Blk1110062;
			else
				tmp = KassArrayTBResources.Blk1110061;
			KATBSupport.Template = RDGenerics.GetEncoding (RDEncodings.Unicode16).GetString (tmp);
			KATBSupport.UseFillers = !DontUseFillers.Checked;

			KATBSupport.ApplyField (BlankFields.OGRN, OGRNField.Text);
			KATBSupport.ApplyField (BlankFields.INN, INNField.Text);
			KATBSupport.ApplyField (BlankFields.KPP, KPPField.Text);

			if (rereg)
				{
				KATBSupport.ApplyField (BlankFields.BlankType, "2");
				KATBSupport.ApplyField (BlankFields.OFDChangeFlag, OFDVariantCombo.SelectedIndex == 4);
				KATBSupport.ApplyField (BlankFields.ToOFDFlag, OFDVariantCombo.SelectedIndex == 2);
				KATBSupport.ApplyField (BlankFields.FromOFDFlag, OFDVariantCombo.SelectedIndex == 3);
				}
			else if (!finish)
				{
				KATBSupport.ApplyField (BlankFields.BlankType, "1");
				KATBSupport.ApplyField (BlankFields.OFDChangeFlag, false);
				KATBSupport.ApplyField (BlankFields.ToOFDFlag, false);
				KATBSupport.ApplyField (BlankFields.FromOFDFlag, false);
				}
			KATBSupport.ApplyField (BlankFields.AddressPlaceChangeFlag, AddressPlaceChangeFlag.Checked);
			KATBSupport.ApplyField (BlankFields.AutomatChangeFlag, AutomatChangeFlag.Checked);
			KATBSupport.ApplyField (BlankFields.FNChangeFlag, FNChangeFlag.Checked);
			KATBSupport.ApplyField (BlankFields.NameChangeFlag, UserNameChangeFlag.Checked);
			KATBSupport.ApplyField (BlankFields.OtherChangeFlag, OtherChangeFlag.Checked);

			string field = UserNameField.Text;
			KATBSupport.ApplyField (BlankFields.UserName_Line1, field);
			field = KATBSupport.TrimField (BlankFields.UserName_Line1, field);
			KATBSupport.ApplyField (BlankFields.UserName_Line2, field);
			field = KATBSupport.TrimField (BlankFields.UserName_Line2, field);
			KATBSupport.ApplyField (BlankFields.UserName_Line3, field);

			KATBSupport.ApplyField (BlankFields.UserPresenterType, PresenterTypeFlag.Checked ? "2" : "1");
			field = PresenterTypeField.Text;
			KATBSupport.ApplyField (BlankFields.UserPresenter_Line1, field);
			field = KATBSupport.TrimField (BlankFields.UserPresenter_Line1, field);
			KATBSupport.ApplyField (BlankFields.UserPresenter_Line2, field);
			field = KATBSupport.TrimField (BlankFields.UserPresenter_Line2, field);
			KATBSupport.ApplyField (BlankFields.UserPresenter_Line3, field);
			KATBSupport.ApplyField (BlankFields.CurrentDate, DateTime.Now.ToString ("dd.MM.yyyy"));
			KATBSupport.ApplyField (BlankFields.AppendixesCount, "");

			KATBSupport.ApplyField (BlankFields.RegistrationNumber, KKTRNMField.Text);

			field = (KKTModelCombo.SelectedIndex == 0) ? "" : KKTModelCombo.Text;
			int idx = field.IndexOf ('(');
			if (idx > 0)
				field = field.Substring (0, idx);
			KATBSupport.ApplyField (BlankFields.KKTModelName, field);
			KATBSupport.ApplyField (BlankFields.KKTSerialNumber_Line1, KKTSerialField.Text);
			KATBSupport.ApplyField (BlankFields.KKTSerialNumber_Line2, "");

			if (FNModelCombo.SelectedIndex == 0)
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
			KATBSupport.ApplyField (BlankFields.FNModelName_Line1, field);
			field = KATBSupport.TrimField (BlankFields.FNModelName_Line1, field);
			KATBSupport.ApplyField (BlankFields.FNModelName_Line2, field);
			field = KATBSupport.TrimField (BlankFields.FNModelName_Line2, field);
			KATBSupport.ApplyField (BlankFields.FNModelName_Line3, field);
			field = KATBSupport.TrimField (BlankFields.FNModelName_Line3, field);
			KATBSupport.ApplyField (BlankFields.FNModelName_Line4, field);
			field = KATBSupport.TrimField (BlankFields.FNModelName_Line4, field);
			KATBSupport.ApplyField (BlankFields.FNModelName_Line5, field);
			field = KATBSupport.TrimField (BlankFields.FNModelName_Line5, field);
			KATBSupport.ApplyField (BlankFields.FNModelName_Line6, field);
			KATBSupport.ApplyField (BlankFields.FNSerialNumber, FNSerialField.Text);

			KATBSupport.ApplyField (BlankFields.UserAddressRegionCode,
				(AddressRegionCodeCombo.SelectedIndex == 0) || !AddressRegionCodeCombo.Enabled ? "" :
				AddressRegionCodeCombo.Text.Substring (0, 2));
			KATBSupport.ApplyField (BlankFields.UserAddressIndex, AddressIndexField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressArea, AddressAreaField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressCity, AddressCityField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressTown, AddressTownField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressStreet, AddressStreetField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressHouseNumber, AddressHouseField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressBuildingNumber, AddressBuildingField.Text);
			KATBSupport.ApplyField (BlankFields.UserAddressAppartmentNumber, AddressAppartmentField.Text);

			field = PlaceField.Text;
			KATBSupport.ApplyField (BlankFields.UserPlace_Line1, field);
			field = KATBSupport.TrimField (BlankFields.UserPlace_Line1, field);
			KATBSupport.ApplyField (BlankFields.UserPlace_Line2, field);
			field = KATBSupport.TrimField (BlankFields.UserPlace_Line2, field);
			KATBSupport.ApplyField (BlankFields.UserPlace_Line3, field);
			field = KATBSupport.TrimField (BlankFields.UserPlace_Line3, field);
			KATBSupport.ApplyField (BlankFields.UserPlace_Line4, field);

			KATBSupport.ApplyField (BlankFields.AutonomousFlag, OFDVariantCombo.SelectedIndex % 2 == 1);
			KATBSupport.ApplyField (BlankFields.LotteryFlag, LotteryFlag.Checked);
			KATBSupport.ApplyField (BlankFields.GamblingFlag, GamblingFlag.Checked);
			KATBSupport.ApplyField (BlankFields.GamblingExchangeFlag, GamblingExchangeFlag.Checked);
			KATBSupport.ApplyField (BlankFields.BankPaymentAgentFlag, BankAgentFlag.Checked);
			KATBSupport.ApplyField (BlankFields.PaymentAgentFlag, AgentFlag.Checked);
			KATBSupport.ApplyField (BlankFields.AutomatFlag, AutomatFlag.Checked);
			KATBSupport.ApplyField (BlankFields.MarkFlag, MarkFlag.Checked);
			KATBSupport.ApplyField (BlankFields.InternetFlag, InternetFlag.Checked);
			KATBSupport.ApplyField (BlankFields.DeliveryFlag, DeliveryFlag.Checked);
			KATBSupport.ApplyField (BlankFields.BSOFlag, BSOFlag.Checked ? "1" : "");
			KATBSupport.ApplyField (BlankFields.ExciseFlag, ExciseFlag.Checked);

			KATBSupport.ApplyField (BlankFields.AutomatNumber, AutomatNumberField.Text);
			if (AutomatAddressIsSame.Checked)
				{
				KATBSupport.ApplyField (BlankFields.AutomatAddressRegionCode,
					(AddressRegionCodeCombo.SelectedIndex == 0) || !AddressRegionCodeCombo.Enabled ? "" :
					AddressRegionCodeCombo.Text.Substring (0, 2));

				KATBSupport.ApplyField (BlankFields.AutomatAddressIndex, AddressIndexField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressArea, AddressAreaField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressCity, AddressCityField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressTown, AddressTownField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressStreet, AddressStreetField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressHouseNumber, AddressHouseField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressBuildingNumber, AddressBuildingField.Text);
				KATBSupport.ApplyField (BlankFields.AutomatAddressAppartmentNumber, AddressAppartmentField.Text);

				field = PlaceField.Text;
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line1, field);
				field = KATBSupport.TrimField (BlankFields.AutomatPlace_Line1, field);
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line2, field);
				field = KATBSupport.TrimField (BlankFields.AutomatPlace_Line2, field);
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line3, field);
				}
			else
				{
				KATBSupport.ApplyField (BlankFields.AutomatAddressRegionCode, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressIndex, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressArea, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressCity, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressTown, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressStreet, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressHouseNumber, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressBuildingNumber, "");
				KATBSupport.ApplyField (BlankFields.AutomatAddressAppartmentNumber, "");
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line1, "");
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line2, "");
				KATBSupport.ApplyField (BlankFields.AutomatPlace_Line3, "");
				}

			if (OFDVariantCombo.SelectedIndex % 2 == 1)
				{
				KATBSupport.ApplyField (BlankFields.OFDINN, "0".PadLeft (12, '0'));
				}
			else
				{
				field = kb.Ofd.GetOFDINNByName (OFDNameCombo.Text);
				KATBSupport.ApplyField (BlankFields.OFDINN, field);
				}

			field = KATBSupport.GetOFDFullName (field);
			KATBSupport.ApplyField (BlankFields.OFDName_Line1, field);
			field = KATBSupport.TrimField (BlankFields.OFDName_Line1, field);
			KATBSupport.ApplyField (BlankFields.OFDName_Line2, field);
			field = KATBSupport.TrimField (BlankFields.OFDName_Line2, field);
			KATBSupport.ApplyField (BlankFields.OFDName_Line3, field);
			field = KATBSupport.TrimField (BlankFields.OFDName_Line3, field);
			KATBSupport.ApplyField (BlankFields.OFDName_Line4, field);

			KATBSupport.ApplyField (BlankFields.FNBrokenFlag, FNBrokenFlag.Checked);
			KATBSupport.ApplyField (BlankFields.KKTStolenFlag, KKTStolenFlag.Checked);
			KATBSupport.ApplyField (BlankFields.KKTMissingFlag, KKTMissingFlag.Checked);

			string dateFiller = DontUseFillers.Checked ? "  .  .    " : "--.--.----";
			string timeFiller = DontUseFillers.Checked ? "  :  " : "--:--";

			KATBSupport.ApplyField (BlankFields.FNOpenDocumentNumber, FNOpenFDField.Text);
			if (FNOpenDateField.Value.Year > FNOpenDateField.MinDate.Year)
				{
				KATBSupport.ApplyField (BlankFields.FNOpenDate, FNOpenDateField.Value.ToString ("dd.MM.yyyy"));
				KATBSupport.ApplyField (BlankFields.FNOpenTime, FNOpenDateField.Value.ToString ("HH:mm"));
				}
			else
				{
				KATBSupport.ApplyField (BlankFields.FNOpenDate, dateFiller);
				KATBSupport.ApplyField (BlankFields.FNOpenTime, timeFiller);
				}

			if (string.IsNullOrWhiteSpace (FNOpenFPDField.Text))
				KATBSupport.ApplyField (BlankFields.FNOpenDocumentSign, "");
			else
				KATBSupport.ApplyField (BlankFields.FNOpenDocumentSign, FNOpenFPDField.Text.PadLeft (10, '0'));

			KATBSupport.ApplyField (BlankFields.FNCloseDocumentNumber, FNCloseFDField.Text);
			if (FNCloseDateField.Value.Year > FNCloseDateField.MinDate.Year)
				{
				KATBSupport.ApplyField (BlankFields.FNCloseDate, FNCloseDateField.Value.ToString ("dd.MM.yyyy"));
				KATBSupport.ApplyField (BlankFields.FNCloseTime, FNCloseDateField.Value.ToString ("HH:mm"));
				}
			else
				{
				KATBSupport.ApplyField (BlankFields.FNCloseDate, dateFiller);
				KATBSupport.ApplyField (BlankFields.FNCloseTime, timeFiller);
				}

			if (string.IsNullOrWhiteSpace (FNCloseFPDField.Text))
				KATBSupport.ApplyField (BlankFields.FNCloseDocumentSign, "");
			else
				KATBSupport.ApplyField (BlankFields.FNCloseDocumentSign, FNCloseFPDField.Text.PadLeft (10, '0'));

			#endregion

			// Запуск на печать
			field = KassArrayDB::RD_AAOW.KKTSupport.PrintText (KATBSupport.Template,
				KassArrayDB::RD_AAOW.PrinterTypes.BlankA4,
				KassArrayDB::RD_AAOW.PrinterFlags.None);
			if (!string.IsNullOrWhiteSpace (field))
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Не удалось распечатать документ" + RDLocale.RN + field);
			else
				RDInterface.MessageBox (RDMessageFlags.Success | RDMessageFlags.CenterText | RDMessageFlags.NoSound,
					"Задание отправлено на печать", 700);
			}

		private const string leftEmpty = RDLocale.RNRN + "Оставьте это поле пустым, чтобы заполнить его вручную " +
			"в распечатанном заявлении";

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
						FNCloseDateField.Value = DateTime.Parse (lines[3].Substring (idx + 2).Replace (',', ' '));
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
						idx = lines[2].IndexOf (':');
						FNOpenFDField.Text = lines[2].Substring (idx + 2);
						FNOpenDateField.Value = dto;
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

			UserNameField.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.UserName, false);
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
				OFDVariantCombo.SelectedIndex = 1;
				}
			else
				{
				OFDVariantCombo.SelectedIndex = 0;
				string ofd = kb.Ofd.GetOFDByINN (ofdINN, 1);
				if (OFDNameCombo.Items.Contains (ofd))
					OFDNameCombo.Text = ofd;
				}

			AddressFromFN.Text = KassArrayDB::RD_AAOW.KKTSupport.GetRegTagValue
				(KassArrayDB::RD_AAOW.RegTags.RegistrationAddress, false);
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
				RDGenerics.RunURL ("https://google.com/search?q=почтовый+индекс+" +
					AddressTownField.Text.Replace (' ', '+').ToLower ());
				return;
				}

			RDGenerics.RunURL ("https://google.com/search?q=почтовый+индекс+" +
				AddressCityField.Text.Replace (' ', '+').ToLower ());
			}
		}
	}
