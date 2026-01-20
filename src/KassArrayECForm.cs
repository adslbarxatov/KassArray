extern alias KassArrayDB;

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class KassArrayECForm: Form
		{
		// Переменные и константы
		private KassArrayDB::RD_AAOW.KnowledgeBase kb;
		private bool getValuesFromRegistration;

		// Ресивер сообщений на повторное открытие окна
		private EventWaitHandle ewh;

		// Список сохранённых реквизитов
		private KAECList kl;

		// Иконка в трее
		private bool closeWindowOnError = false;
		private bool closeWindowOnRequest = false;
		private bool hideWindow = false;
		private NotifyIcon ni = new NotifyIcon ();

		// Текущая выбранная строка
		private uint selectedIndex = 0;

		// Число оповещений, видимых в поле списка без прокрутки
		private const uint visibleNotificationsInList = 10;

		// Стандартные компоненты стиля полей списка ККТ
		private Font kktFont;
		private Padding kktMargin = new Padding (3, 3, 3, 3);

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		/// <param name="GetValuesFromRegistration">Флаг командной строки, указывающий на необходимость
		/// получения параметров ККТ из статуса ФН</param>
		public KassArrayECForm (bool GetValuesFromRegistration, bool HideWindow)
			{
			// Инициализация
			InitializeComponent ();
			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;
			RDGenerics.LoadWindowDimensions (this);

			if (!RDGenerics.CheckLibrariesVersions (ProgramDescription.AssemblyLibraries, true))
				{
				closeWindowOnError = true;
				return;
				}

			getValuesFromRegistration = GetValuesFromRegistration;
			kb = new KassArrayDB::RD_AAOW.KnowledgeBase ();
			hideWindow = HideWindow;

			this.Text = RDGenerics.DefaultAssemblyVisibleName;
			TipLabel.Text = "ККТ в списке отсортированы по порядку истечения сроков жизни ФН, начиная с тех, " +
				"замена которых потребуется раньше всех (относительно текущей даты)";

			if (KassArrayDB::RD_AAOW.KKTSupport.OverrideCloseButton)
				MExit.Text = "Свернуть";

			// Настройка иконки в трее
			ni.Icon = KassArrayECResources.KATray_Green;
			ni.Text = RDGenerics.DefaultAssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenuStrip = new ContextMenuStrip ();
			ni.ContextMenuStrip.ShowImageMargin = false;

			ni.ContextMenuStrip.Items.Add (RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit),
				null, CloseService);

			ni.MouseDown += ReturnWindow;
			ni.BalloonTipClicked += ReturnWindow;

			// Подключение к прослушиванию системного события вызова окна
			if (!RDGenerics.StartedFromMSStore)
				{
				try
					{
					ewh = EventWaitHandle.OpenExisting (KassArrayDB::RD_AAOW.ProgramDescription.AssemblyMainName +
						KassArrayDB::RD_AAOW.ProgramDescription.KassArrayECAlias);
					}
				catch { }
				ShowWindowTimer.Enabled = true;
				}

			// Загрузка списка
			kl = new KAECList ();
			kktFont = new Font (this.Font, FontStyle.Bold);
			ReloadList ();
			}

		private void KassArrayECForm_Shown (object sender, EventArgs e)
			{
			// Скрытие при автозапуске
			if (hideWindow)
				this.Hide ();

			// Прерывание при ошибке
			if (closeWindowOnError)
				{
				this.Close ();
				return;
				}

			// Запуск переданных реквизитов на редактирование
			if (getValuesFromRegistration)
				{
				KassArrayECEntry kaec = new KassArrayECEntry (kl, true);
				if (!kaec.Cancelled)
					ReloadList ();
				kaec.Dispose ();
				}
			}

		// Закрытие окна
		private void CloseService (object sender, EventArgs e)
			{
			closeWindowOnRequest = true;
			this.Close ();
			}

		private void MExit_Click (object sender, EventArgs e)
			{
			if (KassArrayDB::RD_AAOW.KKTSupport.OverrideCloseButton)
				{
				this.Hide ();
				return;
				}

			closeWindowOnRequest = true;
			this.Close ();
			}

		private void KassArrayECForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			/*// Контроль
			if (closeWindowOnError)
				return;

			RDGenerics.SaveWindowDimensions (this);*/
			if (closeWindowOnError)
				return;

			if (KassArrayDB::RD_AAOW.KKTSupport.OverrideCloseButton && !closeWindowOnRequest)
				{
				e.Cancel = true;
				MExit_Click (null, null);
				return;
				}

			// Завершение
			RDGenerics.SaveWindowDimensions (this);
			ni.Visible = false;
			}

		// Возврат окна приложения
		private void ReturnWindow (object sender, MouseEventArgs e)
			{
			if (e.Button != MouseButtons.Left)
				return;

			if (this.Visible)
				{
				MExit_Click (null, null);
				}
			else
				{
				this.Show ();

				this.TopMost = true;
				this.TopMost = false;
				this.WindowState = FormWindowState.Normal;
				}
			}

		private void ReturnWindow (object sender, EventArgs e)
			{
			this.Show ();

			this.TopMost = true;
			this.TopMost = false;
			this.WindowState = FormWindowState.Normal;
			}

		// Отображение справки
		private void MHelp_Clicked (object sender, EventArgs e)
			{
			RDInterface.ShowAbout (false);
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

		// Перезагрузка списка ККТ
		private void ReloadList ()
			{
			/*// Сохранение позиции
			int idx = KKTList.SelectedIndex;
			if (idx < 0)
				idx = 0;

			// Сброс списка
			KKTList.Items.Clear ();*/
			// Сброс списка
			KKTList2.Controls.Clear ();

			if (kl.ItemsCount < 1)
				selectedIndex = 0;
			else if (selectedIndex >= kl.ItemsCount)
				selectedIndex = kl.ItemsCount - 1;

			uint yWarnings = 0;
			uint rWarnings = 0;
			uint yellowTs = KassArrayECSettings.YellowWarningThreshold;
			uint redTs = KassArrayECSettings.RedWarningThreshold;

			// Формирование контролов
			for (uint i = 0; i < kl.ItemsCount; i++)
				{
				string[] values = kl.GetRequisites (i);
				string model = kb.KKTNumbers.GetKKTModel (values[0]);
				/*string warning;

				if ((kl.GetDaysToFNExpiration (i) < redTs) || (kl.GetDaysToOFDExpiration (i) < redTs))
					{
					warning = "❌  ";
					rWarnings++;
					}
				else if ((kl.GetDaysToFNExpiration (i) < yellowTs) || (kl.GetDaysToOFDExpiration (i) < yellowTs))
					{
					warning = "⚠  ";
					yWarnings++;
					}
				else
					{
					warning = "";
					}

				KKTList.Items.Add (warning + model + "  |  " + values[1]);*/

				// Сборка контрола
				Label l = new Label ();
				l.AutoSize = false;

				if ((kl.GetDaysToFNExpiration (i) < redTs) || (kl.GetDaysToOFDExpiration (i) < redTs))
					{
					l.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					rWarnings++;
					}
				else if ((kl.GetDaysToFNExpiration (i) < yellowTs) || (kl.GetDaysToOFDExpiration (i) < yellowTs))
					{
					l.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.WarningMessage);
					yWarnings++;
					}
				else
					{
					l.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					}

				l.ForeColor = RDInterface.GetInterfaceColor (RDInterfaceColors.DefaultText);
				l.Click += KKTList_LabelClicked;
				l.Font = kktFont;
				l.Text = model + "  |  " + values[1];
				l.Margin = kktMargin;
				l.Padding = kktMargin;

				// Блокировка и применение размера
				l.MaximumSize = l.MinimumSize = new Size (KKTList2.Width - 6 -
					((kl.ItemsCount > visibleNotificationsInList) ? 18 : 0), 0);
				l.AutoSize = true;

				// Добавление
				KKTList2.Controls.Add (l);
				}

			/*// Восстановление позиции
			if (KKTList.Items.Count > 0)
				{
				if (idx < KKTList.Items.Count)
					KKTList.SelectedIndex = idx;
				else
					KKTList.SelectedIndex = KKTList.Items.Count - 1;
				}
			((Label)KKTList2.Controls[(int)selectedIndex]).BorderStyle = BorderStyle.FixedSingle;*/

			// Прочее
			BAddSameOwner.Enabled = BUpdate.Enabled = BRemove.Enabled = BUpdateContacts.Enabled =
				SearchButton.Enabled = SearchField.Enabled = (kl.ItemsCount > 0);

			CountLabel.Text = "Отслеживается касс: " + kl.ItemsCount.ToString () +
				"  |  Число владельцев: " + kl.OwnersCount.ToString () + RDLocale.RN +
				"Предупреждений: " + (yWarnings + rWarnings).ToString ();

			RDInterfaceColors color;
			if (rWarnings > 0)
				{
				color = RDInterfaceColors.ErrorMessage;
				ni.Icon = KassArrayECResources.KATray_Red;

				if (!this.Visible)
					ni.ShowBalloonTip (5000, RDGenerics.DefaultAssemblyVisibleName,
						"Есть ККТ, срок действия ФН или тарифа ОФД на который почти истёк",
						ToolTipIcon.Error);
				}
			else if (yWarnings > 0)
				{
				color = RDInterfaceColors.WarningMessage;
				ni.Icon = KassArrayECResources.KATray_Yellow;

				if (!this.Visible)
					ni.ShowBalloonTip (3000, RDGenerics.DefaultAssemblyVisibleName,
						"Есть ККТ, срок действия ФН или тарифа ОФД на который скоро истекает",
						ToolTipIcon.Warning);
				}
			else
				{
				color = RDInterfaceColors.SuccessMessage;
				ni.Icon = KassArrayECResources.KATray_Green;
				}

			CountLabel.BackColor = RDInterface.GetInterfaceColor (color);

			// Загрузка описания
			KKTList_LabelClicked (null, null);

			if (kl.ItemsCount > 0)
				KKTList2.ScrollControlIntoView (KKTList2.Controls[(int)selectedIndex]);
			}

		// Выбор ККТ в списке
		private void KKTList_LabelClicked (object sender, EventArgs e)
			{
			// Поиск индекса
			/*if (KKTList.SelectedIndex < 0)
				return;*/
			if (sender != null)
				{
				int idx = KKTList2.Controls.IndexOf ((Control)sender);
				if (idx < 0)
					return;
				else
					selectedIndex = (uint)idx;
				}

			// Обновление стиля
			for (int i = 0; i < kl.ItemsCount; i++)
				{
				/*((Label)KKTList2.Controls[i]).BorderStyle = (i == selectedIndex) ?
					BorderStyle.FixedSingle : BorderStyle.None;*/
				((Label)KKTList2.Controls[i]).BorderStyle = (i == selectedIndex) ? BorderStyle.FixedSingle :
					BorderStyle.None;
				}

			// Загрузка состояния
			/*uint idx = (uint)KKTList.SelectedIndex;
				string[] values = kl.GetRequisites (idx);*/
			if (kl.ItemsCount < 1)
				{
				InfoLabel.Text = "(отсутствуют ККТ для отображения)";
				InfoLabel.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.LightGrey);

				return;
				}

			string[] values = kl.GetRequisites (selectedIndex);
			string model = kb.KKTNumbers.GetKKTModel (values[0]);

			InfoLabel.Text = "Заводской номер ККТ: " + values[0] + RDLocale.RN;
			InfoLabel.Text += "Модель ККТ: " + model + RDLocale.RN;
			InfoLabel.Text += "Владелец: " + values[1] + RDLocale.RNRN;

			InfoLabel.Text += "Местоположение: " + values[3] + RDLocale.RN;
			InfoLabel.Text += "Контактные данные: " + values[2] + RDLocale.RNRN;

			InfoLabel.Text += "Срок действия ФН: " + values[4] + RDLocale.RN;
			InfoLabel.Text += "  Осталось дней: " + values[5] + RDLocale.RN;

			if (values[6] != KAECList.NoOFDAlias)
				{
				InfoLabel.Text += "Срок тарифа ОФД: " + values[6] + RDLocale.RN;
				InfoLabel.Text += "  Осталось дней: " + values[7];
				}

			/*int fnDays = kl.GetDaysToFNExpiration (idx);
			int ofdDays = kl.GetDaysToOFDExpiration (idx);*/
			int fnDays = kl.GetDaysToFNExpiration (selectedIndex);
			int ofdDays = kl.GetDaysToOFDExpiration (selectedIndex);
			uint yellowTs = KassArrayECSettings.YellowWarningThreshold;
			uint redTs = KassArrayECSettings.RedWarningThreshold;

			if ((fnDays < redTs) || (ofdDays < redTs))
				InfoLabel.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
			else if ((fnDays < yellowTs) || (ofdDays < yellowTs))
				InfoLabel.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.WarningMessage);
			else
				InfoLabel.BackColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
			}

		// Добавление новой ККТ
		private void BAddNew_Click (object sender, EventArgs e)
			{
			KassArrayECEntry kaec = new KassArrayECEntry (kl, false);
			if (!kaec.Cancelled)
				ReloadList ();
			kaec.Dispose ();
			}

		// Добавление ККТ к существующему пользователю
		private void BAddSameOwner_Click (object sender, EventArgs e)
			{
			/*if (KKTList.SelectedIndex < 0)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"ККТ для копирования не выбрана в списке", 1500);
				return;
				}

			KassArrayECEntry kaec = new KassArrayECEntry (kl, (uint)KKTList.SelectedIndex, true);*/
			if (kl.ItemsCount < 1)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Отсутствуют ККТ для копирования реквизитов", 1500);
				return;
				}

			KassArrayECEntry kaec = new KassArrayECEntry (kl, selectedIndex, true);
			if (!kaec.Cancelled)
				ReloadList ();
			kaec.Dispose ();
			}

		// Обновление записи
		private void BUpdate_Click (object sender, EventArgs e)
			{
			/*if (KKTList.SelectedIndex < 0)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"ККТ для изменения не выбрана в списке", 1500);
				return;
				}

			KassArrayECEntry kaec = new KassArrayECEntry (kl, (uint)KKTList.SelectedIndex, false);*/
			if (kl.ItemsCount < 1)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Нет ККТ для обновления", 1000);
				return;
				}

			KassArrayECEntry kaec = new KassArrayECEntry (kl, selectedIndex, false);
			if (!kaec.Cancelled)
				ReloadList ();
			kaec.Dispose ();
			}

		// Удаление записи
		private void BRemove_Click (object sender, EventArgs e)
			{
			// Контроль
			/*if (KKTList.SelectedIndex < 0)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"ККТ для удаления не выбрана в списке", 1500);
				return;
				}*/
			if (kl.ItemsCount < 1)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Список ККТ пуст", 1000);
				return;
				}

			if (RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText, "Удалить выбранную ККТ?",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_YesNoFocus),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) != RDMessageButtons.ButtonOne)
				return;

			// Удаление
			/*kl.RemoveEntry ((uint)KKTList.SelectedIndex);*/
			kl.RemoveEntry (selectedIndex);
			ReloadList ();
			}

		// Поиск записи
		private void SearchButton_Click (object sender, EventArgs e)
			{
			// Контроль
			if (string.IsNullOrWhiteSpace (SearchField.Text))
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Укажите критерий для поиска", 1000);
				return;
				}

			// Поиск
			int idx = kl.FindEntry (SearchField.Text);
			if (idx < 0)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Не найдены ККТ, соответствующие указанному критерию", 1500);
				return;
				}

			// Успешно
			/*KKTList.SelectedIndex = idx;*/
			selectedIndex = (uint)idx;
			KKTList_LabelClicked (null, null);
			KKTList2.ScrollControlIntoView (KKTList2.Controls[(int)selectedIndex]);
			}

		private void SearchField_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				SearchButton_Click (null, null);
			}

		// Обновление контактов
		private void BUpdateContacts_Click (object sender, EventArgs e)
			{
			/*if (KKTList.SelectedIndex < 0)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"ККТ для изменения не выбрана в списке", 1500);
				return;
				}
			uint idx = (uint)KKTList.SelectedIndex;*/
			if (kl.ItemsCount < 1)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Нет ККТ для обновления", 1000);
				return;
				}

			/*string currectContact = kl.GetRequisites (idx)[2];*/
			string currectContact = kl.GetRequisites (selectedIndex)[2];
			string newContact = RDInterface.MessageBox ("Укажите новые контакты владельца ККТ", true,
				50, currectContact);
			if (string.IsNullOrWhiteSpace (newContact))
				return;

			/*kl.UpdateContact (idx, newContact);
			KKTList_SelectedIndexChanged (null, null);*/
			kl.UpdateContact (selectedIndex, newContact);
			KKTList_LabelClicked (null, null);
			}

		// Вызов настроек
		private void MSettings_Click (object sender, EventArgs e)
			{
			_ = new KassArrayECSettings ();
			ReloadList ();
			}

		// Экспорт данных
		private void MExport_Click (object sender, EventArgs e)
			{
			if (kl.ItemsCount < 1)
				{
				RDInterface.MessageBox (RDMessageFlags.Warning | RDMessageFlags.CenterText,
					"Нет сведений для экспорта", 1000);
				return;
				}

			/*int idx = KKTList.SelectedIndex;
			if (idx < 0)
				idx = 0;

			string[] values = kl.GetRequisites ((uint)idx);*/
			string[] values = kl.GetRequisites (selectedIndex);
			_ = new KassArrayECExport (kl, values[1], kb);
			}
		}
	}
