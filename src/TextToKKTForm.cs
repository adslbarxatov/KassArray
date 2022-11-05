using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму программы
	/// </summary>
	public partial class TextToKKTForm: Form
		{
		// Переменные
		private KKTCodes kkmc = null;               // Дескрипторы информационных классов
		private KKTErrorsList kkme = null;
		private OFD ofd = null;
		private LowLevel ll = null;
		private UserManuals um = null;
		private ConfigAccessor ca = null;
		private KKTSerial kkts = null;
		private FNSerial fns = null;
		private TLVTags tlvt = null;
		private BarCodes barc = null;
		private Connectors conn = null;

		private NotifyIcon ni = new NotifyIcon ();  // Дескриптор иконки в трее

		private KKTSupport.FNLifeFlags fnlf;        // Параметры вычисления срока жизни ФН
		private string fnLifeResult = "";           // Рассчитанный срок жизни ФН

		private int lastErrorSearchOffset = 0;      // Ссылки на текущие смещения в списках поиска
		private int lastOFDSearchOffset = 0;
		private int lastLowLevelSearchOffset = 0;

		#region Главный интерфейс

		/// <summary>
		/// Конструктор. Запускает главную форму
		/// </summary>
		/// <param name="DumpFileForFNReader">Путь к файлу дампа для FNReader</param>
		public TextToKKTForm (string DumpFileForFNReader)
			{
			// Инициализация
			InitializeComponent ();
			ca = new ConfigAccessor (this.Width, this.Height);

			// Загрузка списка кодов и ошибок
			kkmc = new KKTCodes ();
			kkme = new KKTErrorsList ();
			ofd = new OFD ();
			ll = new LowLevel ();
			um = new UserManuals (ca.ExtendedFunctions);
			kkts = new KKTSerial ();
			fns = new FNSerial ();
			tlvt = new TLVTags ();
			barc = new BarCodes ();
			conn = new Connectors ();

			// Настройка контролов
			KKTListForCodes.Items.AddRange (kkmc.GetKKTTypeNames ().ToArray ());
			KKTListForCodes.SelectedIndex = 0;

			KKTListForErrors.Items.AddRange (kkme.GetKKTTypeNames ().ToArray ());
			KKTListForErrors.SelectedIndex = 0;

			LowLevelProtocol.Items.AddRange (ll.GetProtocolsNames ().ToArray ());
			LowLevelProtocol.SelectedIndex = (int)ca.LowLevelProtocol;
			LowLevelProtocol_CheckedChanged (null, null);

			FNLifeStartDate.Value = DateTime.Now;

			OFDNamesList.Items.Add ("Неизвестный ОФД");
			OFDNamesList.Items.AddRange (ofd.GetOFDNames ().ToArray ());
			OFDNamesList.SelectedIndex = 0;

			CableType.Items.AddRange (conn.GetCablesNames ().ToArray ());
			CableType.SelectedIndex = 0;

			this.Text = ProgramDescription.AssemblyVisibleName;

			// Получение настроек
			this.Left = ca.WindowLeft;
			this.Top = ca.WindowTop;

			KeepAppState.Checked = ca.KeepApplicationState;
			TopFlag.Checked = ca.TopMost;

			MainTabControl.SelectedIndex = (int)ca.CurrentTab;

			try
				{
				KKTListForErrors.SelectedIndex = (int)ca.KKTForErrors;
				ErrorCodesList.SelectedIndex = (int)ca.ErrorCode;
				}
			catch { }

			FNLifeSN.Text = ca.FNSerial;
			if (ca.GenericTaxFlag)
				GenericTaxFlag.Checked = true;
			else
				OtherTaxFlag.Checked = true;
			if (ca.GoodsFlag)
				GoodsFlag.Checked = true;
			else
				ServicesFlag.Checked = true;
			SeasonFlag.Checked = ca.SeasonFlag;
			AgentsFlag.Checked = ca.AgentsFlag;
			ExciseFlag.Checked = ca.ExciseFlag;
			AutonomousFlag.Checked = ca.AutonomousFlag;
			FFD12Flag.Checked = ca.FFD12Flag;

			RNMSerial.MaxLength = (int)kkts.MaxSerialNumberLength;
			RNMSerial.Text = ca.KKTSerial;
			RNMUserINN.Text = ca.UserINN;
			RNMValue.Text = ca.RNMKKT;
			RNMSerial_TextChanged (null, null); // Для протяжки пустых полей

			OFDINN.Text = ca.OFDINN;
			OFDNalogSite.Text = OFD.FNSSite;
			OFDDNSNameK.Text = OFD.OKPSite;
			OFDIPK.Text = OFD.OKPIP;
			OFDPortK.Text = OFD.OKPPort;
			OFDYaDNS1.Text = OFD.YandexDNSReq;
			OFDYaDNS2.Text = OFD.YandexDNSAlt;

			LowLevelCommand.SelectedIndex = (int)ca.LowLevelCode;

			try
				{
				KKTListForCodes.SelectedIndex = (int)ca.KKTForCodes;
				}
			catch { }
			TextToConvert.Text = ca.CodesText;

			KKTListForManuals.Items.AddRange (um.GetKKTList ().ToArray ());
			KKTListForManuals.SelectedIndex = (int)ca.KKTForManuals;
			OperationsListForManuals.Items.AddRange (um.OperationTypes);
			try
				{
				OperationsListForManuals.SelectedIndex = (int)ca.OperationForManuals;
				}
			catch
				{
				OperationsListForManuals.SelectedIndex = 0;
				}

			BarcodeData.MaxLength = (int)BarCodes.MaxSupportedDataLength;
			BarcodeData.Text = ca.BarcodeData;

			CableType.SelectedIndex = (int)ca.CableType;

			TLV_FFDCombo.Items.Add ("ФФД 1.05");
			TLV_FFDCombo.Items.Add ("ФФД 1.1");
			TLV_FFDCombo.Items.Add ("ФФД 1.2");
			TLV_FFDCombo.SelectedIndex = (int)ca.FFDForTLV;

			TLVFind.Text = ca.TLVData;
			TLV_ObligationBase.Text = TLVTags.ObligationBase;
			TLVButton_Click (null, null);

			// Блокировка расширенных функций при необходимости
			RNMGenerate.Visible = RNMFromFNReader.Visible = LowLevelTab.Enabled = TLVTab.Enabled =
				ConnectorsTab.Enabled = OFDFromFNReader.Visible = PrintFullUserManual.Visible =
				ca.AllowExtendedFunctionsLevel2;
			CodesTab.Enabled = ca.AllowExtendedFunctionsLevel1;

			RNMTip.Text = "Индикатор ФФД: красный – поддержка не планируется; зелёный – поддерживается; " +
				"жёлтый – планируется; синий – нет сведений\n(на момент релиза этой версии приложения)";
			if (ca.AllowExtendedFunctionsLevel2)
				{
				RNMTip.Text += ("\n\nПервые 10 цифр РН являются порядковым номером ККТ в реестре и могут быть указаны " +
					"вручную при генерации");
				}
			else
				{
				RNMLabel.Text = "Укажите регистрационный номер для проверки:";
				UnlockField.Visible = UnlockLabel.Visible = true;
				UnlockLabel.Text = ca.LockMessage;
				FNReader.Enabled = false;
				}

			// Настройка иконки в трее
			ni.Icon = Properties.TextToKKMResources.TextToKKTTray;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();

			ni.ContextMenu.MenuItems.Add (new MenuItem ("FNReader", FNReader_Click));
			ni.ContextMenu.MenuItems[0].Enabled = FNReader.Enabled;
			ni.ContextMenu.MenuItems.Add (new MenuItem ("В&ыход", CloseService));

			ni.MouseDown += ReturnWindow;
			ni.ContextMenu.MenuItems[1].DefaultItem = true;

			if (!File.Exists (RDGenerics.AutorunLinkPath))
				ni.ContextMenu.MenuItems.Add (new MenuItem ("Добавить в &автозапуск", AddToStartup));

			// Запуск файла дампа, если представлен
			if (ca.AllowExtendedFunctionsLevel2 && (DumpFileForFNReader != ""))
				CallFNReader (DumpFileForFNReader);
			}

		// Добавление в автозапуск
		private void AddToStartup (object sender, EventArgs e)
			{
			// Попытка создания
			WindowsShortcut.CreateStartupShortcut (Application.ExecutablePath, ProgramDescription.AssemblyMainName, "");

			// Контроль
			ni.ContextMenu.MenuItems[ni.ContextMenu.MenuItems.Count - 1].Enabled = !File.Exists (RDGenerics.AutorunLinkPath);
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
			if ((FNReaderInstance != null) && FNReaderInstance.IsActive)
				{
				MessageBox.Show ("Завершите работу с модулем FNReader, чтобы выйти из приложения",
					ProgramDescription.AssemblyVisibleName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				e.Cancel = true;
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
			ca.WindowLeft = this.Left;
			ca.WindowTop = this.Top;

			ca.KeepApplicationState = KeepAppState.Checked;
			ca.TopMost = TopFlag.Checked;
			if (!ca.KeepApplicationState)
				return;

			ca.CurrentTab = (uint)MainTabControl.SelectedIndex;

			ca.KKTForErrors = (uint)KKTListForErrors.SelectedIndex;
			ca.ErrorCode = (uint)ErrorCodesList.SelectedIndex;

			ca.FNSerial = FNLifeSN.Text;
			ca.GenericTaxFlag = GenericTaxFlag.Checked;
			ca.GoodsFlag = GoodsFlag.Checked;
			ca.SeasonFlag = SeasonFlag.Checked;
			ca.AgentsFlag = AgentsFlag.Checked;
			ca.ExciseFlag = ExciseFlag.Checked;
			ca.AutonomousFlag = AutonomousFlag.Checked;
			ca.FFD12Flag = FFD12Flag.Checked;

			ca.KKTSerial = RNMSerial.Text;
			ca.UserINN = RNMUserINN.Text;
			ca.RNMKKT = RNMValue.Text;

			ca.OFDINN = OFDINN.Text;

			ca.LowLevelProtocol = (uint)LowLevelProtocol.SelectedIndex;
			ca.LowLevelCode = (uint)LowLevelCommand.SelectedIndex;

			ca.KKTForCodes = (uint)KKTListForCodes.SelectedIndex;
			ca.CodesText = TextToConvert.Text;

			ca.BarcodeData = BarcodeData.Text;
			ca.CableType = (uint)CableType.SelectedIndex;
			ca.TLVData = TLVFind.Text;

			ca.KKTForManuals = (uint)KKTListForManuals.SelectedIndex;
			ca.OperationForManuals = (uint)OperationsListForManuals.SelectedIndex;
			ca.FFDForTLV = (uint)TLV_FFDCombo.SelectedIndex;
			}

		// Отображение справки
		private void BHelp_Clicked (object sender, EventArgs e)
			{
			this.TopMost = false;
			ProgramDescription.ShowAbout (false);
			this.TopMost = TopFlag.Checked;
			}

		// Дополнительные функции
		private void FNReaderUserManual_Click (object sender, EventArgs e)
			{
			try
				{
				Process.Start (ProgramDescription.AssemblyFNReaderLink);
				}
			catch { }
			}

		// Запрос цвета, соответствующего статусу поддержки
		private Color StatusToColor (KKTSerial.FFDSupportStatuses Status)
			{
			if (Status == KKTSerial.FFDSupportStatuses.Planned)
				return Color.FromArgb (255, 255, 200);

			if (Status == KKTSerial.FFDSupportStatuses.Supported)
				return Color.FromArgb (200, 255, 200);

			if (Status == KKTSerial.FFDSupportStatuses.Unsupported)
				return Color.FromArgb (255, 200, 200);

			// Остальные
			return Color.FromArgb (200, 200, 255);
			}

		// Разблокировка функционала
		private void UnlockField_TextChanged (object sender, EventArgs e)
			{
			if (ca.TestPass (UnlockField.Text))
				{
				UnlockLabel.Text = ca.LockMessage;
				if (!ca.AllowExtendedFunctionsLevel2)
					{
					UnlockField.Text = "";
					}
				else
					{
					UnlockField.Enabled = false;
					UnlockLabel.TextAlign = ContentAlignment.MiddleCenter;
					}
				}
			}

		// Вызов библиотеки FNReader
		private void FNReader_Click (object sender, EventArgs e)
			{
			CallFNReader ("");
			}

		private Assembly FNReaderDLL;
		private Type FNReaderProgram;
		private dynamic FNReaderInstance;

		private void CallFNReader (string DumpPath)
			{
			// Контроль
			bool result = true;
			if (!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.FNReaderDLL) ||
				!File.Exists (RDGenerics.AppStartupPath + ProgramDescription.FNReaderSubDLL))
				result = false;

			if (result && (FNReaderDLL == null))
				{
				try
					{
					FNReaderDLL = Assembly.LoadFile (RDGenerics.AppStartupPath + ProgramDescription.FNReaderDLL);

					FNReaderProgram = FNReaderDLL.GetType ("RD_AAOW.Program");
					FNReaderInstance = Activator.CreateInstance (FNReaderProgram);
					}
				catch
					{
					result = false;
					}
				}

			if (!result)
				{
				MessageBox.Show ("Модуль FNReader для работы с данными фискального накопителя отсутствует.\n\n" +
					"Данный компонент можно загрузить с актуальным обновлением из интерфейса «О приложении»" +
					" (раздел «Прочее», кнопка «О программе»)",
					ProgramDescription.AssemblyVisibleName, MessageBoxButtons.OK, MessageBoxIcon.Information);

				return;
				}

			// Контроль версии
			if (FNReaderInstance.LibVersion != ProgramDescription.AssemblyVersion)
				{
				MessageBox.Show ("Версия библиотеки «" + ProgramDescription.FNReaderDLL + "» не подходит для " +
					"текущей версии программы.\n\n" +
					"Корректную версию можно загрузить с актуальным обновлением из интерфейса «О приложении»" +
					" (раздел «Прочее», кнопка «О программе»)",
					ProgramDescription.AssemblyVisibleName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

				return;
				}

			// Проверки прошли успешно, запуск
			if (FNReaderDLL != null)
				FNReaderInstance.FNReaderEx (DumpPath, fns.GetFNNameDelegate, kkts.GetKKTModelDelegate,
					ofd.GetOFDByINNDelegate, ofd.GetOFDDataDelegate);
			}

		/// <summary>
		/// Ручная обработка сообщения для окна по спецкоду
		/// </summary>
		protected override void WndProc (ref Message m)
			{
			if ((m.Msg == ConfigAccessor.NextDumpPathMsg) && (ConfigAccessor.NextDumpPath != ""))
				{
				// Делается для защиты от непредвиденных сбросов состояния приложения
				if ((FNReaderInstance != null) && FNReaderInstance.IsActive)
					{
					ConfigAccessor.NextDumpPath = "";
					MessageBox.Show ("Завершите работу с модулем FNReader, чтобы открыть новый файл",
						ProgramDescription.AssemblyVisibleName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				else
					{
					CallFNReader (ConfigAccessor.NextDumpPath);
					ConfigAccessor.NextDumpPath = "";
					}
				}

			base.WndProc (ref m);
			}

		// Переключение состояния "поверх всех окон"
		private void TopFlag_CheckedChanged (object sender, EventArgs e)
			{
			this.TopMost = TopFlag.Checked;
			}

		// Получение данных от FNReader
		private void RNMFromFNReader_Click (object sender, EventArgs e)
			{
			// Контроль
			if ((FNReaderInstance == null) || string.IsNullOrEmpty (FNReaderInstance.FNStatus))
				{
				MessageBox.Show ("Статус ФН в FNReader ещё не запрашивался или содержит не все требуемые поля",
					ProgramDescription.AssemblyVisibleName, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
				}

			// Разбор
			string status = FNReaderInstance.FNStatus;
			int left, right;

			// Раздел параметров ОФД
			if (((Button)sender).Name == OFDFromFNReader.Name)
				{
				if (((left = status.LastIndexOf ("ИНН ОФД: ")) >= 0) && ((right = status.IndexOf ("(", left)) >= 0))
					{
					left += 9;
					OFDINN.Text = status.Substring (left, right - left).Trim ();
					}

				return;
				}

			// Раздел срока жизни ФН
			if (((Button)sender).Name == FNFromFNReader.Name)
				{
				if (((left = status.LastIndexOf ("номер ФН: ")) >= 0) && ((right = status.IndexOf ("(", left)) >= 0))
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

				if (status.IndexOf ("Режим товаров", left) >= 0)
					GoodsFlag.Checked = true;
				else
					ServicesFlag.Checked = true;

				AutonomousFlag.Checked = (status.IndexOf ("Автономный", left) >= 0);
				ExciseFlag.Checked = (status.IndexOf ("подакцизн", left) >= 0);
				FFD12Flag.Checked = (status.IndexOf ("ФФД: 1.2", left) >= 0);
				AgentsFlag.Checked = (status.IndexOf ("агент", left) >= 0);
				// Дату и сезонный режим не запрашиваем

				return;
				}

			// Раздел контроля ЗН ККТ, РНМ и ИНН
			if (((left = status.LastIndexOf ("Заводской номер ККТ: ")) >= 0) && ((right = status.IndexOf ("(", left)) >= 0))
				{
				left += 21;
				RNMSerial.Text = status.Substring (left, right - left).Trim ();
				}
			if (((left = status.LastIndexOf ("Регистрационный номер ККТ: ")) >= 0) && ((right = status.IndexOf ("\r", left)) >= 0))
				{
				left += 27;
				RNMValue.Text = status.Substring (left, right - left).Trim ();
				}
			if (((left = status.LastIndexOf ("ИНН пользователя: ")) >= 0) && ((right = status.IndexOf ("\r", left)) >= 0))
				{
				left += 18;
				RNMUserINN.Text = status.Substring (left, right - left).Trim ();
				}
			}

		#endregion

		#region Коды символов ККТ

		// Изменение текста и его кодировка
		private void TextToConvert_TextChanged (object sender, EventArgs e)
			{
			ResultText.Text = "";

			ErrorLabel.Visible = !Decode ();

			TextLabel.Text = "Текст (" + TextToConvert.Text.Length + "):";

			DescriptionLabel.Text = kkmc.GetKKTTypeDescription ((uint)KKTListForCodes.SelectedIndex);
			}

		// Функция трансляции строки в набор кодов
		private bool Decode ()
			{
			bool res = true;
			char[] text = TextToConvert.Text.ToCharArray ();

			for (int i = 0; i < TextToConvert.Text.Length; i++)
				{
				string s;
				byte b = KKTSupport.CharToCP1251 (text[i]);

				if ((s = kkmc.GetCode ((uint)KKTListForCodes.SelectedIndex, b)) == KKTCodes.EmptyCode)
					{
					ResultText.Text += "xxx";
					res = false;
					}
				else
					{
					ResultText.Text += s;
					}

				ResultText.Text += (((i + 1) % 5 != 0) ? "\t" : "\r\n");
				}

			// Означает успех/ошибку преобразования
			return res;
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

		#endregion

		#region Коды ошибок ККТ

		// Выбор ошибки
		private void ErrorCodesList_SelectedIndexChanged (object sender, EventArgs e)
			{
			ErrorText.Text = kkme.GetErrorText ((uint)KKTListForErrors.SelectedIndex, (uint)ErrorCodesList.SelectedIndex);
			}

		// Выбор модели аппарата
		private void KKTListForErrors_SelectedIndexChanged (object sender, EventArgs e)
			{
			// Перезаполнение списка
			ErrorCodesList.DataSource = kkme.GetErrors ((uint)KKTListForErrors.SelectedIndex);
			ErrorCodesList.DisplayMember = ErrorCodesList.ValueMember = "ErrorCode";
			ErrorCodesList.SelectedIndex = 0;
			}

		// Поиск по тексту ошибки
		private void ErrorFindButton_Click (object sender, EventArgs e)
			{
			List<string> codes = kkme.GetErrorCodesList ((uint)KKTListForErrors.SelectedIndex);
			string text = ErrorSearchText.Text.ToLower ();

			lastErrorSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				if (codes[j].ToLower ().Contains (text) ||
					kkme.GetErrorText ((uint)KKTListForErrors.SelectedIndex, (uint)j).ToLower ().Contains (text))
					{
					lastErrorSearchOffset = ErrorCodesList.SelectedIndex = j;
					return;
					}
				}
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
			if (FNLifeSN.Text != "")
				FNLifeName.Text = fns.GetFNName (FNLifeSN.Text);
			else
				FNLifeName.Text = "(введите ЗН ФН)";

			// Определение длины ключа
			if (FNLifeName.Text.Contains ("(13)") || FNLifeName.Text.Contains ("(15)"))
				{
				FNLife36.Enabled = false;
				FNLife13.Checked = true;
				}
			else
				{
				FNLife36.Enabled = true;
				}

			if (FNLifeName.Text.Contains ("(36)"))
				{
				FNLife13.Enabled = false;
				FNLife36.Checked = true;
				}
			else
				{
				FNLife13.Enabled = true;
				}

			// Принудительное изменение
			FNLifeStartDate_ValueChanged (null, null);
			}

		// Изменение параметров, влияющих на срок жизни ФН
		private void FNLifeStartDate_ValueChanged (object sender, EventArgs e)
			{
			fnlf.FN15 = FNLife13.Checked;
			fnlf.FNExactly13 = FNLifeName.Text.Contains ("(13)");
			fnlf.GenericTax = GenericTaxFlag.Checked;
			fnlf.Goods = GoodsFlag.Checked;
			fnlf.SeasonOrAgents = SeasonFlag.Checked || AgentsFlag.Checked;
			fnlf.Excise = ExciseFlag.Checked;
			fnlf.Autonomous = AutonomousFlag.Checked;
			fnlf.FFD12 = FFD12Flag.Checked;
			fnlf.MarkFN = FNLife13.Enabled && FNLife36.Enabled || fns.IsFNCompatibleWithFFD12 (FNLifeSN.Text);  // Корректный ЗН ФН

			string res = KKTSupport.GetFNLifeEndDate (FNLifeStartDate.Value, fnlf);

			FNLifeResult.Text = "ФН прекратит работу ";
			if (res.Contains ("!"))
				{
				FNLifeResult.ForeColor = Color.FromArgb (255, 0, 0);
				fnLifeResult = res.Substring (1);
				FNLifeResult.Text += (fnLifeResult + "\n(выбранный ФН неприменим с указанными параметрами)");
				}
			else
				{
				FNLifeResult.ForeColor = Color.FromArgb (0, 0, 0);
				fnLifeResult = res;
				FNLifeResult.Text += res;
				}

			if (!(FNLife13.Enabled && FNLife36.Enabled)) // Признак корректно заданного ЗН ФН
				{
				if (!fnlf.MarkFN)
					{
					FNLifeResult.ForeColor = Color.FromArgb (255, 0, 0);

					FNLifeResult.Text += ("\n(выбранный ФН исключён из реестра ФНС)");
					FNLifeName.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
					}
				else
					{
					FNLifeName.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
					}
				}
			else
				{
				FNLifeName.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
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
			string sig = fns.FindSignatureByName (FNLifeSN.Text);
			if (sig != "")
				FNLifeSN.Text = sig;
			}

		private void FNLifeSN_KeyDown (object sender, KeyEventArgs e)
			{
			if (e.KeyCode == Keys.Return)
				FNFindSN_Click (null, null);
			}

		#endregion

		#region РНМ и ЗН

		// Ввод номеров в разделе РН
		private void RNMSerial_TextChanged (object sender, EventArgs e)
			{
			// Заводской номер ККТ
			if (RNMSerial.Text != "")
				{
				RNMSerialResult.Text = kkts.GetKKTModel (RNMSerial.Text);

				KKTSerial.FFDSupportStatuses[] statuses = kkts.GetFFDSupportStatus (RNMSerial.Text);
				RNMSupport105.BackColor = StatusToColor (statuses[0]);
				RNMSupport11.BackColor = StatusToColor (statuses[1]);
				RNMSupport12.BackColor = StatusToColor (statuses[2]);
				}
			else
				{
				RNMSerialResult.Text = "(введите ЗН ККТ)";
				RNMSupport105.BackColor = RNMSupport11.BackColor = RNMSupport12.BackColor =
					StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
				}

			// ИНН пользователя
			RegionLabel.Text = "";
			int checkINN = KKTSupport.CheckINN (RNMUserINN.Text);
			if (checkINN < 0)
				RNMUserINN.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
			else if (checkINN == 0)
				RNMUserINN.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
			else
				RNMUserINN.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Planned);   // Не ошибка
			RegionLabel.Text = kkts.GetRegionName (RNMUserINN.Text);

			// РН
			if (RNMValue.Text.Length < 10)
				RNMValue.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
			else if (KKTSupport.GetFullRNM (RNMUserINN.Text, RNMSerial.Text,
				RNMValue.Text.Substring (0, 10)) == RNMValue.Text)
				RNMValue.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
			else
				RNMValue.BackColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
			}

		// Генерация регистрационного номера
		private void RNMGenerate_Click (object sender, EventArgs e)
			{
			if (RNMValue.Text.Length < 1)
				RNMValue.Text = KKTSupport.GetFullRNM (RNMUserINN.Text, RNMSerial.Text, "0");
			else if (RNMValue.Text.Length < 10)
				RNMValue.Text = KKTSupport.GetFullRNM (RNMUserINN.Text, RNMSerial.Text, RNMValue.Text);
			else
				RNMValue.Text = KKTSupport.GetFullRNM (RNMUserINN.Text, RNMSerial.Text, RNMValue.Text.Substring (0, 10));
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
			string sig = kkts.FindSignatureByName (RNMSerial.Text);
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
			string s = ofd.GetOFDINNByName (OFDNamesList.Text);
			OFDINN.Text = string.IsNullOrWhiteSpace (s) ? "" : s;
			}

		// Выбор ОФД
		private void OFDINN_TextChanged (object sender, EventArgs e)
			{
			List<string> parameters = ofd.GetOFDParameters (OFDINN.Text.Contains ("0000000000") ? "" : OFDINN.Text);

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
				OFDDisabledLabel.Text = parameters[10] + "\n";
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
			List<string> codes = ofd.GetOFDNames ();
			codes.AddRange (ofd.GetOFDINNs ());

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
			OFDINN.Text = OFDSearchText.Text = "";
			}

		#endregion

		#region Команды нижнего уровня

		// Выбор списка команд нижнего уровня
		private void LowLevelProtocol_CheckedChanged (object sender, EventArgs e)
			{
			LowLevelCommand.Items.Clear ();
			LowLevelCommand.Items.AddRange (ll.GetCommandsList ((uint)LowLevelProtocol.SelectedIndex).ToArray ());
			LowLevelCommand.SelectedIndex = 0;
			}

		// Выбор команды
		private void LowLevelCommand_SelectedIndexChanged (object sender, EventArgs e)
			{
			LowLevelCommandCode.Text = ll.GetCommand ((uint)LowLevelProtocol.SelectedIndex,
				(uint)LowLevelCommand.SelectedIndex, false);
			LowLevelCommandDescr.Text = ll.GetCommand ((uint)LowLevelProtocol.SelectedIndex,
				(uint)LowLevelCommand.SelectedIndex, true);
			}

		// Поиск по тексту ошибки
		private void LowLevelFindButton_Click (object sender, EventArgs e)
			{
			List<string> codes = ll.GetCommandsList ((uint)LowLevelProtocol.SelectedIndex);
			string text = LowLevelSearchText.Text.ToLower ();

			lastLowLevelSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastLowLevelSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastLowLevelSearchOffset = LowLevelCommand.SelectedIndex = (i + lastLowLevelSearchOffset) % codes.Count;
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
			UMOperationText.Text = um.GetManual ((uint)KKTListForManuals.SelectedIndex,
				(uint)OperationsListForManuals.SelectedIndex);
			}

		// Печать инструкции
		private void PrintUserManual_Click (object sender, EventArgs e)
			{
			// Сборка задания на печать
			string text = KKTSupport.BuildUserManual (um, (uint)KKTListForManuals.SelectedIndex,
				((Button)sender).Name.Contains ("Cashier"));

			// Печать
			KKTSupport.PrintText (text, PrinterTypes.ManualA4);
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
			if (tlvt.FindTag (TLVFind.Text, (TLVTags_FFDVersions)(TLV_FFDCombo.SelectedIndex + 2)))
				{
				TLVDescription.Text = tlvt.LastDescription;
				TLVType.Text = tlvt.LastType;

				if (!string.IsNullOrWhiteSpace (tlvt.LastValuesSet))
					TLVValues.Text = tlvt.LastValuesSet + "\r\n\r\n";
				else
					TLVValues.Text = "";
				TLVValues.Text += tlvt.LastObligation;
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
				Process.Start (TLVTags.ObligationBaseLink);
				}
			catch { }
			}

		#endregion

		#region Штрих-коды

		// Ввод штрих-кода
		private void BarcodeData_TextChanged (object sender, EventArgs e)
			{
			string s = BarCodes.ConvertFromRussianKeyboard (BarcodeData.Text);
			if (s != BarcodeData.Text)
				{
				BarcodeData.Text = s;
				BarcodeData.SelectionStart = BarcodeData.Text.Length;
				}

			BarcodeDescription.Text = barc.GetBarcodeDescription (BarcodeData.Text);
			}

		private void BarcodeClear_Click (object sender, EventArgs e)
			{
			BarcodeData.Text = "";
			}

		#endregion

		#region Распайки кабелей

		// Выбор типа кабеля
		private void CableType_SelectedIndexChanged (object sender, EventArgs e)
			{
			uint i = (uint)CableType.SelectedIndex;

			CableLeftSide.Text = "Со стороны " + conn.GetCableConnector (i, false);
			CableLeftPins.Text = conn.GetCableConnectorPins (i, false);

			CableRightSide.Text = "Со стороны " + conn.GetCableConnector (i, true);
			CableRightPins.Text = conn.GetCableConnectorPins (i, true);

			CableLeftDescription.Text = conn.GetCableConnectorDescription (i, false) + "\n\n" +
				conn.GetCableConnectorDescription (i, true);
			}

		#endregion
		}
	}
