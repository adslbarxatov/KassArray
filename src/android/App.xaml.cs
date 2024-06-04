using Android.Print;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает функционал приложения
	/// </summary>
	public partial class App: Application
		{
		#region Настройки стилей отображения

		private readonly Color
			headersMasterBackColor = Color.FromHex ("#E8E8E8"),
			headersFieldBackColor = Color.FromHex ("#E0E0E0"),

			userManualsMasterBackColor = Color.FromHex ("#F4E8FF"),
			userManualsFieldBackColor = Color.FromHex ("#ECD8FF"),

			errorsMasterBackColor = Color.FromHex ("#FFEEEE"),
			errorsFieldBackColor = Color.FromHex ("#FFD0D0"),

			fnLifeMasterBackColor = Color.FromHex ("#FFF0E0"),
			fnLifeFieldBackColor = Color.FromHex ("#FFE0C0"),

			rnmMasterBackColor = Color.FromHex ("#E0F0FF"),
			rnmFieldBackColor = Color.FromHex ("#C0E0FF"),

			ofdMasterBackColor = Color.FromHex ("#F4F4FF"),
			ofdFieldBackColor = Color.FromHex ("#D0D0FF"),

			tagsMasterBackColor = Color.FromHex ("#E0FFF0"),
			tagsFieldBackColor = Color.FromHex ("#C8FFE4"),

			lowLevelMasterBackColor = Color.FromHex ("#FFF0FF"),
			lowLevelFieldBackColor = Color.FromHex ("#FFC8FF"),

			kktCodesMasterBackColor = Color.FromHex ("#FFFFF0"),
			kktCodesFieldBackColor = Color.FromHex ("#FFFFD0"),

			aboutMasterBackColor = Color.FromHex ("#F0FFF0"),
			aboutFieldBackColor = Color.FromHex ("#D0FFD0"),

			connectorsMasterBackColor = Color.FromHex ("#F8FFF0"),
			connectorsFieldBackColor = Color.FromHex ("#F0FFE0"),

			barCodesMasterBackColor = Color.FromHex ("#FFF4E2"),
			barCodesFieldBackColor = Color.FromHex ("#E8D9CF"),

			convertors1MasterBackColor = Color.FromHex ("#E1FFF1"),
			convertors1FieldBackColor = Color.FromHex ("#D7F4E7"),

			convertors2MasterBackColor = Color.FromHex ("#E5FFF5"),
			convertors2FieldBackColor = Color.FromHex ("#DBF4EB"),

			convertors3MasterBackColor = Color.FromHex ("#E9FFF9"),
			convertors3FieldBackColor = Color.FromHex ("#DFF4EF");

		private const string firstStartRegKey = "HelpShownAt";

		#endregion

		#region Переменные страниц

		private ContentPage headersPage, kktCodesPage, errorsPage, aboutPage, connectorsPage,
			ofdPage, fnLifePage, rnmPage, lowLevelPage, userManualsPage, tagsPage, barCodesPage,
			convertorsSNPage, convertorsUCPage, convertorsHTPage;

		private Label kktCodesHelpLabel, kktCodesErrorLabel, kktCodesResultText,
			kktCodesLengthLabel,
			errorsResultText, cableLeftSideText, cableRightSideText, cableLeftPinsText, cableRightPinsText,
			cableDescriptionText,
			fnLifeLabel, fnLifeModelLabel, fnLifeGenericTaxLabel, fnLifeGoodsLabel,
			rnmKKTTypeLabel, rnmINNCheckLabel, rnmRNMCheckLabel,
			lowLevelCommandDescr,
			tlvDescriptionLabel, tlvTypeLabel, tlvValuesLabel, tlvObligationLabel,
			barcodeDescriptionLabel, ofdDisabledLabel, convNumberResultField, convCodeResultField,
			aboutFontSizeField;
		private List<Label> operationTextLabels = new List<Label> ();
		private List<Xamarin.Forms.Button> operationTextButtons = new List<Xamarin.Forms.Button> ();
		private const string operationButtonSignature = " (скрыто)";

		private Xamarin.Forms.Button kktCodesKKTButton, fnLifeResult, cableTypeButton, kktCodesCenterButton,
			errorsKKTButton, userManualsKKTButton, userManualsPrintButton,
			ofdNameButton, ofdDNSNameButton, ofdIPButton, ofdPortButton, ofdEmailButton, ofdSiteButton,
			ofdDNSNameMButton, ofdIPMButton, ofdPortMButton, ofdINN,
			lowLevelProtocol, lowLevelCommand, lowLevelCommandCode, rnmGenerate, convCodeSymbolField,
			encodingButton;

		private Editor codesSourceText, errorSearchText, commandSearchText, ofdSearchText,
			fnLifeSerial, tlvTag, rnmKKTSN, rnmINN, rnmRNM, connSearchText,
			barcodeField, convNumberField, convCodeField, convHexField, convTextField;

		private Xamarin.Forms.Switch fnLife13, fnLifeGenericTax, fnLifeGoods, fnLifeSeason, fnLifeAgents,
			fnLifeExcise, fnLifeAutonomous, fnLifeFFD12, fnLifeGambling, fnLifePawn, fnLifeMarkGoods,
			allowService, extendedMode,
			moreThanOneItemPerDocument, productBaseContainsPrices, cashiersHavePasswords, baseContainsServices,
			documentsContainMarks;

		private Xamarin.Forms.DatePicker fnLifeStartDate;

		private StackLayout userManualLayout, menuLayout;

		#endregion

		#region Основные переменные

		// Опорные классы
		private PrintManager pm;
		private KnowledgeBase kb;

		// Поисковые состояния
		private int lastErrorSearchOffset = 0;
		private int lastCommandSearchOffset = 0;
		private int lastOFDSearchOffset = 0;
		private int lastConnSearchOffset = 0;

		// Дата срока жизни ФН (в чистом виде)
		private string fnLifeResultDate = "";

		// Число режимов преобразования
		private uint encodingModesCount;

		// Список ранее сохранённых подстановок для раздела кодов символов
		private List<string> kktCodesOftenTexts = new List<string> ();

		#endregion

		#region Основной функционал 

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App (PrintManager PrintingManager, RDAppStartupFlags Flags)
			{
			// Инициализация
			InitializeComponent ();
			pm = PrintingManager;
			kb = new KnowledgeBase ();

			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			// Переход в статус запуска для отмены вызова из оповещения
			AndroidSupport.AppIsRunning = true;

			// Переопределение цветов для закрытых функций
			if (!AppSettings.EnableExtendedMode) // Уровень 1
				{
				kktCodesFieldBackColor = AndroidSupport.DefaultGreyColors[2];
				kktCodesMasterBackColor = AndroidSupport.DefaultGreyColors[3];
				}
			if (!AppSettings.EnableExtendedMode) // Уровень 2
				{
				tagsFieldBackColor = lowLevelFieldBackColor = connectorsFieldBackColor =
					AndroidSupport.DefaultGreyColors[2];
				tagsMasterBackColor = lowLevelMasterBackColor = connectorsMasterBackColor =
					AndroidSupport.DefaultGreyColors[3];
				}

			#region Общая конструкция страниц приложения

			MainPage = new MasterPage ();

			headersPage = ApplyPageSettings ("HeadersPage", "Разделы и настройки приложения",
				headersMasterBackColor, false);
			menuLayout = (StackLayout)headersPage.FindByName ("MenuField");

			userManualsPage = ApplyPageSettings ("UserManualsPage", "Инструкции по работе с ККТ",
				userManualsMasterBackColor, true);
			errorsPage = ApplyPageSettings ("ErrorsPage", "Коды ошибок ККТ",
				errorsMasterBackColor, true);
			fnLifePage = ApplyPageSettings ("FNLifePage", "Срок жизни ФН",
				fnLifeMasterBackColor, true);
			rnmPage = ApplyPageSettings ("RNMPage", "Заводской и регистрационный номер ККТ",
				rnmMasterBackColor, true);
			ofdPage = ApplyPageSettings ("OFDPage", "Параметры ОФД",
				ofdMasterBackColor, true);

			tagsPage = ApplyPageSettings ("TagsPage", "TLV-теги", tagsMasterBackColor, true);
			tagsPage.IsEnabled = AppSettings.EnableExtendedMode; // Уровень 2

			lowLevelPage = ApplyPageSettings ("LowLevelPage", "Команды нижнего уровня",
				lowLevelMasterBackColor, true);
			lowLevelPage.IsEnabled = AppSettings.EnableExtendedMode;   // Уровень 2

			kktCodesPage = ApplyPageSettings ("KKTCodesPage", "Перевод текста в коды ККТ",
				kktCodesMasterBackColor, true);
			kktCodesPage.IsEnabled = AppSettings.EnableExtendedMode;   // Уровень 1

			connectorsPage = ApplyPageSettings ("ConnectorsPage", "Разъёмы",
				connectorsMasterBackColor, true);
			connectorsPage.IsEnabled = AppSettings.EnableExtendedMode; // Уровень 2

			barCodesPage = ApplyPageSettings ("BarCodesPage", "Штрих-коды",
				barCodesMasterBackColor, true);
			convertorsSNPage = ApplyPageSettings ("ConvertorsSNPage", "Конвертор систем счисления",
				convertors1MasterBackColor, true);
			convertorsUCPage = ApplyPageSettings ("ConvertorsUCPage", "Конвертор символов Unicode",
				convertors2MasterBackColor, true);
			convertorsHTPage = ApplyPageSettings ("ConvertorsHTPage", "Конвертор двоичных данных",
				convertors3MasterBackColor, true);

			aboutPage = ApplyPageSettings ("AboutPage",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				aboutMasterBackColor, true);

			AndroidSupport.SetMainPage (MainPage);

			#endregion

			#region Страница «оглавления»

			AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceLabel",
				"Оставить службу активной после выхода", RDLabelTypes.DefaultLeft);
			allowService = AndroidSupport.ApplySwitchSettings (headersPage, "AllowService", false,
				headersFieldBackColor, AllowService_Toggled, AppSettings.AllowServiceToStart);

			Label allowServiceTip;
			Button allowServiceButton;
			if (!Flags.HasFlag (RDAppStartupFlags.CanShowNotifications))
				{
				allowService.IsEnabled = false;
				allowServiceTip = AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceTip",
					RDLocale.GetDefaultText (RDLDefaultTexts.Message_NotificationPermission), RDLabelTypes.ErrorTip);

				allowServiceButton = AndroidSupport.ApplyButtonSettings (headersPage, "AllowServiceButton",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_GoTo), headersFieldBackColor,
					CallAppSettings, false);
				allowServiceButton.HorizontalOptions = LayoutOptions.Center;
				}
			else if (!AndroidSupport.IsForegroundStartableFromResumeEvent)
				{
				allowServiceTip = AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceTip",
					"Если закреплённое оповещение " + ProgramDescription.AssemblyMainName +
					" отсутствует в верхней части экрана, эта опция потребует перезапуска приложения." + RDLocale.RNRN +
					"В Android 12 и выше отсутствует адекватная реализация перехода к приложению " +
					"при нажатии на оповещение. Поэтому для возвращения в приложение необходимо будет запустить " +
					"его заново. Оповещение в данной версии ОС лишь помогает ускорить его запуск" + RDLocale.RN,
					RDLabelTypes.Tip);

				allowServiceButton = AndroidSupport.ApplyButtonSettings (headersPage, "AllowServiceButton",
					" ", headersFieldBackColor, null, false);
				allowServiceButton.IsVisible = false;
				}
			else
				{
				allowServiceTip = AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceTip",
					" ", RDLabelTypes.Tip);
				allowServiceTip.IsVisible = false;

				allowServiceButton = AndroidSupport.ApplyButtonSettings (headersPage, "AllowServiceButton",
					" ", headersFieldBackColor, null, false);
				allowServiceButton.IsVisible = false;
				}

			AndroidSupport.ApplyLabelSettings (headersPage, "ExtendedModeLabel",
				"Режим сервис-инженера", RDLabelTypes.DefaultLeft);
			extendedMode = AndroidSupport.ApplySwitchSettings (headersPage, "ExtendedMode", false,
				headersFieldBackColor, ExtendedMode_Toggled, AppSettings.EnableExtendedMode);

			AndroidSupport.ApplyLabelSettings (headersPage, "FunctionsLabel",
				"Разделы приложения", RDLabelTypes.HeaderCenter);
			AndroidSupport.ApplyLabelSettings (headersPage, "SettingsLabel",
				"Настройки", RDLabelTypes.HeaderCenter);

			try
				{
				((CarouselPage)MainPage).CurrentPage = ((CarouselPage)MainPage).Children[(int)AppSettings.CurrentTab];
				}
			catch { }

			#endregion

			#region Страница инструкций

			Label ut = AndroidSupport.ApplyLabelSettings (userManualsPage, "SelectionLabel", "Модель ККТ:",
				RDLabelTypes.HeaderLeft);
			userManualLayout = (StackLayout)userManualsPage.FindByName ("UserManualLayout");
			int operationsCount = AppSettings.EnableExtendedMode ? UserManuals.OperationTypes.Length :
				UserManuals.OperationsForCashiers.Length;   // Уровень 1

			UserManualsSections sections = (UserManualsSections)AppSettings.UserManualSectionsState;
			for (int i = 0; i < operationsCount; i++)
				{
				bool sectionEnabled = sections.HasFlag ((UserManualsSections)(1u << i));

				Xamarin.Forms.Button bh = new Xamarin.Forms.Button ();
				bh.BackgroundColor = userManualsMasterBackColor;
				bh.Clicked += OperationTextButton_Clicked;
				bh.FontAttributes = FontAttributes.Bold;
				bh.FontSize = ut.FontSize;
				bh.HorizontalOptions = LayoutOptions.Center;
				bh.IsVisible = true;
				bh.Margin = ut.Margin;
				bh.Padding = bh.Margin = new Thickness (0);
				bh.Text = UserManuals.OperationTypes[i] +
					(sectionEnabled ? "" : operationButtonSignature);
				bh.TextColor = ut.TextColor;

				Label lt = new Label ();
				lt.BackgroundColor = userManualsFieldBackColor;
				lt.FontAttributes = FontAttributes.None;
				lt.FontSize = ut.FontSize;
				lt.HorizontalOptions = LayoutOptions.Fill;
				lt.HorizontalTextAlignment = TextAlignment.Start;
				lt.IsVisible = sectionEnabled;
				lt.Margin = lt.Padding = ut.Margin;
				lt.Text = "   ";
				lt.TextColor = AndroidSupport.MasterTextColor;

				operationTextButtons.Add (bh);
				userManualLayout.Children.Add (operationTextButtons[operationTextButtons.Count - 1]);
				operationTextLabels.Add (lt);
				userManualLayout.Children.Add (operationTextLabels[operationTextLabels.Count - 1]);
				}

			userManualsKKTButton = AndroidSupport.ApplyButtonSettings (userManualsPage, "KKTButton",
				"   ", userManualsFieldBackColor, UserManualsKKTButton_Clicked, true);
			userManualsPrintButton = AndroidSupport.ApplyButtonSettings (userManualsPage, "PrintButton",
				"В файл / на печать", userManualsFieldBackColor, PrintManual_Clicked, false);
			userManualsPrintButton.FontSize -= 2.0;
			userManualsPrintButton.Margin = new Thickness (0);
			userManualsPrintButton.Padding = new Thickness (3, 0);
			userManualsPrintButton.HeightRequest = userManualsPrintButton.FontSize * 2.0;

			AndroidSupport.ApplyLabelSettings (userManualsPage, "HelpLabel",
				"<...> – индикация на экране, [...] – клавиши ККТ." + RDLocale.RN +
				"Нажатие на заголовки разделов позволяет добавить или скрыть их в видимой и печатной " +
				"версиях этой инструкции",
				RDLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "OneItemLabel", "В чеках бывает более одной позиции",
				RDLabelTypes.DefaultLeft);
			moreThanOneItemPerDocument = AndroidSupport.ApplySwitchSettings (userManualsPage, "OneItemSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "PricesLabel", "В базе товаров есть цены",
				RDLabelTypes.DefaultLeft);
			productBaseContainsPrices = AndroidSupport.ApplySwitchSettings (userManualsPage, "PricesSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "PasswordsLabel", "Кассиры пользуются паролями",
				RDLabelTypes.DefaultLeft);
			cashiersHavePasswords = AndroidSupport.ApplySwitchSettings (userManualsPage, "PasswordsSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "ServicesLabel", "В базе ККТ содержатся услуги",
				RDLabelTypes.DefaultLeft);
			baseContainsServices = AndroidSupport.ApplySwitchSettings (userManualsPage, "ServicesSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "MarksLabel", "Среди товаров есть маркированные (ТМТ)",
				RDLabelTypes.DefaultLeft);
			documentsContainMarks = AndroidSupport.ApplySwitchSettings (userManualsPage, "MarksSwitch",
				false, userManualsFieldBackColor, null, false);

			UserManualFlags = (UserManualsFlags)AppSettings.UserManualFlags;

			moreThanOneItemPerDocument.Toggled += UserManualsFlags_Clicked;
			productBaseContainsPrices.Toggled += UserManualsFlags_Clicked;
			cashiersHavePasswords.Toggled += UserManualsFlags_Clicked;
			baseContainsServices.Toggled += UserManualsFlags_Clicked;
			documentsContainMarks.Toggled += UserManualsFlags_Clicked;

			UserManualsKKTButton_Clicked (null, null);

			#endregion

			#region Страница кодов символов ККТ

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "SelectionLabel", "Модель ККТ:",
				RDLabelTypes.HeaderLeft);

			string kktTypeName;
			try
				{
				kktTypeName = kb.CodeTables.GetKKTTypeNames ()[(int)AppSettings.KKTForCodes];
				}
			catch
				{
				kktTypeName = kb.CodeTables.GetKKTTypeNames ()[0];
				AppSettings.KKTForCodes = 0;
				}
			kktCodesOftenTexts.AddRange (AppSettings.CodesOftenTexts);

			kktCodesKKTButton = AndroidSupport.ApplyButtonSettings (kktCodesPage, "KKTButton",
				kktTypeName, kktCodesFieldBackColor, CodesKKTButton_Clicked, true);
			AndroidSupport.ApplyLabelSettings (kktCodesPage, "SourceTextLabel",
				"Исходный текст:", RDLabelTypes.HeaderLeft);

			codesSourceText = AndroidSupport.ApplyEditorSettings (kktCodesPage, "SourceText",
				kktCodesFieldBackColor, Keyboard.Default, 72, AppSettings.CodesText, SourceText_TextChanged, true);
			codesSourceText.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultTextLabel", "Коды ККТ:",
				RDLabelTypes.HeaderLeft);

			kktCodesErrorLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ErrorLabel",
				"Часть введённых символов не поддерживается данной ККТ", RDLabelTypes.ErrorTip);

			kktCodesResultText = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultText", " ",
				RDLabelTypes.FieldMonotype, kktCodesFieldBackColor);

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpTextLabel", "Пояснения к вводу:",
				RDLabelTypes.HeaderLeft);
			kktCodesHelpLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpText",
				kb.CodeTables.GetKKTTypeDescription (AppSettings.KKTForCodes), RDLabelTypes.Field,
				kktCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (kktCodesPage, "ClearText",
				RDDefaultButtons.Delete, kktCodesFieldBackColor, CodesClear_Clicked);
			AndroidSupport.ApplyButtonSettings (kktCodesPage, "SelectSavedText",
				RDDefaultButtons.Select, kktCodesFieldBackColor, CodesLineGet_Clicked);

			kktCodesLengthLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "LengthLabel",
				"Длина:", RDLabelTypes.HeaderLeft);
			kktCodesCenterButton = AndroidSupport.ApplyButtonSettings (kktCodesPage, "CenterText",
				"Центрировать", kktCodesFieldBackColor, TextToConvertCenter_Click, false);
			AndroidSupport.ApplyButtonSettings (kktCodesPage, "RememberText",
				"Запомнить", kktCodesFieldBackColor, TextToConvertRemember_Click, false);

			SourceText_TextChanged (null, null);    // Протягивание кодов

			#endregion

			#region Страница ошибок

			AndroidSupport.ApplyLabelSettings (errorsPage, "SelectionLabel", "Модель ККТ:",
				RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (errorsPage, "SearchLabel", "Поиск ошибки по коду или фрагменту описания:",
				RDLabelTypes.HeaderLeft);

			try
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[(int)AppSettings.KKTForErrors];
				}
			catch
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[0];
				AppSettings.KKTForErrors = 0;
				}
			errorsKKTButton = AndroidSupport.ApplyButtonSettings (errorsPage, "KKTButton",
				kktTypeName, errorsFieldBackColor, ErrorsKKTButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (errorsPage, "ResultTextLabel", "Описание ошибки:",
				RDLabelTypes.HeaderLeft);

			errorsResultText = AndroidSupport.ApplyLabelSettings (errorsPage, "ResultText",
				"", RDLabelTypes.Field, errorsFieldBackColor);
			errorSearchText = AndroidSupport.ApplyEditorSettings (errorsPage, "ErrorSearchText",
				errorsFieldBackColor, Keyboard.Default, 30, AppSettings.ErrorCode, null, true);
			Errors_Find (null, null);

			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorSearchButton",
				RDDefaultButtons.Find, errorsFieldBackColor, Errors_Find);
			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorClearButton",
				RDDefaultButtons.Delete, errorsFieldBackColor, Errors_Clear);

			#endregion

			#region Страница "О программе"

			AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeInc",
				RDDefaultButtons.Increase, aboutFieldBackColor, FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeDec",
				RDDefaultButtons.Decrease, aboutFieldBackColor, FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);

			AndroidSupport.ApplyLabelSettings (aboutPage, "HelpTextLabel",
				RDGenerics.GetEncoding (RDEncodings.UTF8).
				GetString (RD_AAOW.Properties.Resources.KassArray_ru_ru_dph), RDLabelTypes.SmallLeft);

			FontSizeButton_Clicked (null, null);

			#endregion

			#region Страница определения срока жизни ФН

			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetModelLabel", "ЗН, номинал или модель ФН:",
				RDLabelTypes.HeaderLeft);
			fnLifeSerial = AndroidSupport.ApplyEditorSettings (fnLifePage, "FNLifeSerial", fnLifeFieldBackColor,
				Keyboard.Default, 16, AppSettings.FNSerial, FNLifeSerial_TextChanged, true);
			fnLifeSerial.Margin = new Thickness (0);

			fnLife13 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLife13", true,
				fnLifeFieldBackColor, FnLife13_Toggled, false);

			fnLifeLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeLabel",
				"", RDLabelTypes.DefaultLeft);

			//
			fnLifeModelLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeModelLabel",
				"", RDLabelTypes.Semaphore);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetUserParameters", "Значимые параметры:",
				RDLabelTypes.HeaderLeft);

			fnLifeGenericTax = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGenericTax", true,
				fnLifeFieldBackColor, null, false);
			fnLifeGenericTaxLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGenericTaxLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGoods", true,
				fnLifeFieldBackColor, null, false);
			fnLifeGoodsLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGoodsLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeSeason = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeSeason", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeSeasonLabel", "Сезонная торговля",
				RDLabelTypes.DefaultLeft);

			fnLifeAgents = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAgents", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAgentsLabel", "Платёжный (суб)агент",
				RDLabelTypes.DefaultLeft);

			fnLifeExcise = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeExcise", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeExciseLabel", "Подакцизные товары",
				RDLabelTypes.DefaultLeft);

			fnLifeAutonomous = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAutonomous", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAutonomousLabel", "Автономный режим",
				RDLabelTypes.DefaultLeft);

			fnLifeFFD12 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeFFD12", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeFFD12Label", "ФФД 1.1, 1.2",
				RDLabelTypes.DefaultLeft);

			fnLifeGambling = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGambling", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGamblingLabel", "Азартные игры и лотереи",
				RDLabelTypes.DefaultLeft);

			fnLifePawn = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifePawn", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifePawnLabel", "Ломбарды и страхование",
				RDLabelTypes.DefaultLeft);

			fnLifeMarkGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeMarkGoods", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeMarkGoodsLabel", "Маркированные товары",
				RDLabelTypes.DefaultLeft);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetDate", "Дата фискализации:",
				RDLabelTypes.DefaultLeft);
			fnLifeStartDate = AndroidSupport.ApplyDatePickerSettings (fnLifePage, "FNLifeStartDate",
				fnLifeFieldBackColor, FnLifeStartDate_DateSelected);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeResultLabel", "Результат:",
				RDLabelTypes.HeaderLeft);
			fnLifeResult = AndroidSupport.ApplyButtonSettings (fnLifePage, "FNLifeResult", "",
				fnLifeFieldBackColor, FNLifeResultCopy, true);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeHelpLabel",
				"Нажатие кнопки копирует дату окончания срока жизни в буфер обмена",
				RDLabelTypes.Tip);

			//
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Clear",
				RDDefaultButtons.Delete, fnLifeFieldBackColor, FNLifeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Find",
				RDDefaultButtons.Find, fnLifeFieldBackColor, FNLifeFind_Clicked);

			// Применение всех названий
			FNLifeEvFlags = (FNLifeFlags)AppSettings.FNLifeEvFlags;
			fnLifeGenericTax.Toggled += FnLife13_Toggled;
			fnLifeGoods.Toggled += FnLife13_Toggled;
			fnLifeSeason.Toggled += FnLife13_Toggled;
			fnLifeAgents.Toggled += FnLife13_Toggled;
			fnLifeExcise.Toggled += FnLife13_Toggled;
			fnLifeAutonomous.Toggled += FnLife13_Toggled;
			fnLifeFFD12.Toggled += FnLife13_Toggled;
			fnLifeGambling.Toggled += FnLife13_Toggled;
			fnLifePawn.Toggled += FnLife13_Toggled;
			fnLifeMarkGoods.Toggled += FnLife13_Toggled;

			FNLifeSerial_TextChanged (null, null);

			#endregion

			#region Страница заводских и рег. номеров

			AndroidSupport.ApplyLabelSettings (rnmPage, "SNLabel", "ЗН или модель ККТ:",
				RDLabelTypes.HeaderLeft);
			rnmKKTSN = AndroidSupport.ApplyEditorSettings (rnmPage, "SN", rnmFieldBackColor, Keyboard.Default,
				kb.KKTNumbers.MaxSerialNumberLength, AppSettings.KKTSerial, RNM_TextChanged, true);

			rnmKKTTypeLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "TypeLabel",
				"", RDLabelTypes.Semaphore);

			AndroidSupport.ApplyLabelSettings (rnmPage, "INNLabel", "ИНН пользователя:",
				RDLabelTypes.HeaderLeft);
			rnmINN = AndroidSupport.ApplyEditorSettings (rnmPage, "INN", rnmFieldBackColor, Keyboard.Numeric, 12,
				AppSettings.UserINN, RNM_TextChanged, true);

			rnmINNCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "INNCheckLabel", "",
				RDLabelTypes.Semaphore);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки или произвольное число для генерации:",
					RDLabelTypes.HeaderLeft);
			else
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки:", RDLabelTypes.HeaderLeft);

			rnmRNM = AndroidSupport.ApplyEditorSettings (rnmPage, "RNM", rnmFieldBackColor, Keyboard.Numeric, 16,
				AppSettings.RNMKKT, RNM_TextChanged, true);

			rnmRNMCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMCheckLabel", "",
				RDLabelTypes.Semaphore);

			rnmGenerate = AndroidSupport.ApplyButtonSettings (rnmPage, "RNMGenerate",
				RDDefaultButtons.Create, rnmFieldBackColor, RNMGenerate_Clicked);
			rnmGenerate.IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			AndroidSupport.ApplyButtonSettings (rnmPage, "RegistryStats", "Статистика реестра ККТ",
				rnmFieldBackColor, RNMStats_Clicked, false);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMAbout",
					"Первые 10 цифр являются порядковым номером ККТ в реестре. При генерации " +
					"РНМ их можно указать вручную – остальные будут достроены программой", RDLabelTypes.Tip);

			AndroidSupport.ApplyButtonSettings (rnmPage, "Clear",
				RDDefaultButtons.Delete, rnmFieldBackColor, RNMClear_Clicked);
			AndroidSupport.ApplyButtonSettings (rnmPage, "Find",
				RDDefaultButtons.Find, rnmFieldBackColor, RNMFind_Clicked);

			RNM_TextChanged (null, null);   // Применение значений

			#endregion

			#region Страница настроек ОФД

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDINNLabel", "ИНН ОФД:",
				RDLabelTypes.HeaderLeft);
			ofdINN = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDINN", AppSettings.OFDINN, ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNameLabel", "Название:",
				RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSearchLabel", "Поиск по ИНН или фрагменту названия:",
				RDLabelTypes.HeaderLeft);
			ofdNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDName", "- Выберите или введите ИНН -",
				ofdFieldBackColor, OFDName_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameLabel", "Адрес ОФД:",
				RDLabelTypes.HeaderLeft);
			ofdDNSNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSName", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIP", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPort", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameMLabel", "Адрес ИСМ:",
				RDLabelTypes.HeaderLeft);
			ofdDNSNameMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortM", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameKLabel", "Адрес ОКП (стандартный):",
				RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameK", OFD.OKPSite,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPK", OFD.OKPIP,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortK", OFD.OKPPort,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDEmailLabel", "E-mail отправителя чеков:",
				RDLabelTypes.HeaderLeft);
			ofdEmailButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDEmail", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSiteLabel", "Сайт ОФД:",
				RDLabelTypes.HeaderLeft);
			ofdSiteButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSite", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNalogSiteLabel", "Сайт ФНС:",
				RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDNalogSite", OFD.FNSSite,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDHelpLabel",
				"Нажатие кнопок копирует их подписи в буфер обмена", RDLabelTypes.Tip);

			AndroidSupport.ApplyButtonSettings (ofdPage, "Clear",
				RDDefaultButtons.Delete, ofdFieldBackColor, OFDClear_Clicked);

			ofdSearchText = AndroidSupport.ApplyEditorSettings (ofdPage, "OFDSearchText", ofdFieldBackColor,
				Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSearchButton",
				RDDefaultButtons.Find, ofdFieldBackColor, OFD_Find);

			ofdDisabledLabel = AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDisabledLabel",
				"Аннулирован", RDLabelTypes.ErrorTip);
			ofdDisabledLabel.IsVisible = false;

			OFDINN_TextChanged (null, null); // Протягивание значений

			#endregion

			#region Страница TLV-тегов

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVSearchLabel", "Поиск по номеру или фрагменту описания:",
				RDLabelTypes.HeaderLeft);
			tlvTag = AndroidSupport.ApplyEditorSettings (tagsPage, "TLVSearchText", tagsFieldBackColor,
				Keyboard.Default, 20, AppSettings.TLVData, null, true);

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVSearchButton",
				RDDefaultButtons.Find, tagsFieldBackColor, TLVFind_Clicked);
			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVClearButton",
				RDDefaultButtons.Delete, tagsFieldBackColor, TLVClear_Clicked);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescriptionLabel", "Описание тега:",
				RDLabelTypes.HeaderLeft);
			tlvDescriptionLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescription", "",
				RDLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVTypeLabel", "Тип тега:",
				RDLabelTypes.HeaderLeft);
			tlvTypeLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVType", "",
				RDLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValuesLabel", "Возможные значения тега:",
				RDLabelTypes.HeaderLeft);
			tlvValuesLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValues", "",
				RDLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligationLabel", "Обязательность:",
				RDLabelTypes.HeaderLeft);
			tlvObligationLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligation", "",
				RDLabelTypes.Field, tagsFieldBackColor);
			tlvObligationLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVObligationHelpLabel",
				TLVTags.ObligationBase, tagsFieldBackColor, TLVObligationBase_Click, false);

			TLVFind_Clicked (null, null);

			#endregion

			#region Страница команд нижнего уровня

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "ProtocolLabel", "Протокол:",
				RDLabelTypes.HeaderLeft);
			lowLevelProtocol = AndroidSupport.ApplyButtonSettings (lowLevelPage, "ProtocolButton",
				kb.LLCommands.GetProtocolsNames ()[(int)AppSettings.LowLevelProtocol], lowLevelFieldBackColor,
				LowLevelProtocol_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandLabel", "Команда:",
				RDLabelTypes.HeaderLeft);
			lowLevelCommand = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandButton",
				kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol)[(int)AppSettings.LowLevelCode],
				lowLevelFieldBackColor, LowLevelCommandCodeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandSearchLabel", "Поиск по описанию команды:",
				RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandCodeLabel", "Код команды:",
				RDLabelTypes.HeaderLeft);
			lowLevelCommandCode = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandCodeButton",
				kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, false),
				lowLevelFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescrLabel", "Пояснение:",
				RDLabelTypes.HeaderLeft);

			lowLevelCommandDescr = AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescr",
				kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, true),
				RDLabelTypes.Field, lowLevelFieldBackColor);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "LowLevelHelpLabel",
				"Нажатие кнопки копирует команду в буфер обмена", RDLabelTypes.Tip);

			commandSearchText = AndroidSupport.ApplyEditorSettings (lowLevelPage, "CommandSearchText",
				lowLevelFieldBackColor, Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandSearchButton",
				RDDefaultButtons.Find, lowLevelFieldBackColor, Command_Find);
			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandClearButton",
				RDDefaultButtons.Delete, lowLevelFieldBackColor, Command_Clear);

			#endregion

			#region Страница штрих-кодов

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeFieldLabel", "Данные штрих-кода:",
				RDLabelTypes.HeaderLeft);
			barcodeField = AndroidSupport.ApplyEditorSettings (barCodesPage, "BarcodeField",
				barCodesFieldBackColor, Keyboard.Default, BarCodes.MaxSupportedDataLength,
				AppSettings.BarcodeData, BarcodeText_TextChanged, true);

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescriptionLabel",
				"Описание штрих-кода:", RDLabelTypes.HeaderLeft);
			barcodeDescriptionLabel = AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescription", "",
				RDLabelTypes.Field, barCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (barCodesPage, "Clear",
				RDDefaultButtons.Delete, barCodesFieldBackColor, BarcodeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (barCodesPage, "GetFromClipboard",
				RDDefaultButtons.Copy, barCodesFieldBackColor, BarcodeGet_Clicked);

			BarcodeText_TextChanged (null, null);

			#endregion

			#region Страница распиновок

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLabel", "Тип кабеля:",
				RDLabelTypes.HeaderLeft);
			cableTypeButton = AndroidSupport.ApplyButtonSettings (connectorsPage, "CableTypeButton",
				kb.Plugs.GetCablesNames ()[(int)AppSettings.CableType], connectorsFieldBackColor,
				CableTypeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescriptionLabel", "Сопоставление контактов:",
				RDLabelTypes.HeaderLeft);
			cableLeftSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftSide",
				" ", RDLabelTypes.DefaultLeft);
			cableLeftSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableLeftSideText.HorizontalOptions = LayoutOptions.Center;

			cableLeftPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftPins", " ",
				RDLabelTypes.FieldMonotype, connectorsFieldBackColor);

			cableRightSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightSide",
				" ", RDLabelTypes.DefaultLeft);
			cableRightSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableRightSideText.HorizontalOptions = LayoutOptions.Center;

			cableRightPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightPins", " ",
				RDLabelTypes.FieldMonotype, connectorsFieldBackColor);
			cableLeftPinsText.HorizontalTextAlignment = cableRightPinsText.HorizontalTextAlignment =
				TextAlignment.Center;

			cableDescriptionText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescription",
				" ", RDLabelTypes.Tip);

			connSearchText = AndroidSupport.ApplyEditorSettings (connectorsPage, "ConnSearchText",
				connectorsFieldBackColor, Keyboard.Default, 30, "", null, true);
			AndroidSupport.ApplyButtonSettings (connectorsPage, "ConnSearchButton",
				RDDefaultButtons.Find, connectorsFieldBackColor, Conn_Find);
			AndroidSupport.ApplyButtonSettings (connectorsPage, "ConnClearButton",
				RDDefaultButtons.Delete, connectorsFieldBackColor, Conn_Clear);

			CableTypeButton_Clicked (null, null);

			#endregion

			#region Страница конвертора систем счисления

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberLabel",
				"Число:", RDLabelTypes.HeaderLeft);
			convNumberField = AndroidSupport.ApplyEditorSettings (convertorsSNPage, "ConvNumberField",
				convertors1FieldBackColor, Keyboard.Default, 10, AppSettings.ConversionNumber,
				ConvNumber_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberInc",
				RDDefaultButtons.Increase, convertors1FieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberDec",
				RDDefaultButtons.Decrease, convertors1FieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberClear",
				RDDefaultButtons.Delete, convertors1FieldBackColor, ConvNumberClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultLabel", "Представление:",
				RDLabelTypes.HeaderLeft);
			convNumberResultField = AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultField",
				" ", RDLabelTypes.FieldMonotype, convertors1FieldBackColor);
			ConvNumber_TextChanged (null, null);

			const string convHelp = "Шестнадцатеричные числа следует начинать с символов “0x”";
			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvHelpLabel", convHelp,
				RDLabelTypes.Tip);

			#endregion

			#region Страница конвертора символов Unicode

			encodingModesCount = (uint)(DataConvertors.AvailableEncodings.Length / 2);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeLabel",
				"Код символа" + RDLocale.RN + "или символ:", RDLabelTypes.HeaderLeft);
			convCodeField = AndroidSupport.ApplyEditorSettings (convertorsUCPage, "ConvCodeField",
				convertors2FieldBackColor, Keyboard.Default, 10, AppSettings.ConversionCode,
				ConvCode_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeInc",
				RDDefaultButtons.Increase, convertors2FieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeDec",
				RDDefaultButtons.Decrease, convertors2FieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeClear",
				RDDefaultButtons.Delete, convertors2FieldBackColor, ConvCodeClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultLabel", "Символ Unicode:",
				RDLabelTypes.HeaderLeft);
			convCodeResultField = AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultField",
				"", RDLabelTypes.FieldMonotype, convertors2FieldBackColor);

			convCodeSymbolField = AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeSymbolField",
				" ", convertors2FieldBackColor, CopyCharacter_Click, true);
			convCodeSymbolField.FontSize *= 5;
			ConvCode_TextChanged (null, null);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvHelpLabel",
				convHelp + "." + RDLocale.RN + "Нажатие кнопки с символом Unicode копирует его в буфер обмена",
				RDLabelTypes.Tip);

			#endregion

			#region Страница конвертора двоичных данных

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "HexLabel",
				"Данные (hex):", RDLabelTypes.HeaderLeft);
			convHexField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "HexField",
				convertors3FieldBackColor, Keyboard.Default, 500, AppSettings.ConversionHex, null, true);
			convHexField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "HexToTextButton",
				RDDefaultButtons.Down, convertors3FieldBackColor, ConvertHexToText_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "TextToHexButton",
				RDDefaultButtons.Up, convertors3FieldBackColor, ConvertTextToHex_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "ClearButton",
				RDDefaultButtons.Delete, convertors3FieldBackColor, ClearConvertText_Click);

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "TextLabel",
				"Данные (текст):", RDLabelTypes.HeaderLeft);
			convTextField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "TextField",
				convertors3FieldBackColor, Keyboard.Default, 250, AppSettings.ConversionText, null, true);
			convTextField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "EncodingLabel",
				"Кодировка:", RDLabelTypes.HeaderLeft);
			encodingButton = AndroidSupport.ApplyButtonSettings (convertorsHTPage, "EncodingButton",
				" ", convertors3FieldBackColor, EncodingButton_Clicked, true);
			EncodingButton_Clicked (null, null);

			#endregion

			// Обязательное принятие Политики и EULA
			AcceptPolicy (Flags.HasFlag (RDAppStartupFlags.Huawei));
			}

		// Локальный оформитель страниц приложения
		private ContentPage ApplyPageSettings (string PageName, string PageTitle, Color PageBackColor,
			bool AddToMenu)
			{
			// Инициализация страницы
			ContentPage page = AndroidSupport.ApplyPageSettings (MainPage, PageName, PageTitle, PageBackColor);

			// Добавление в содержание
			if (!AddToMenu)
				return page;

			Xamarin.Forms.Button b = new Xamarin.Forms.Button ();
			b.Text = PageTitle;
			b.BackgroundColor = PageBackColor;
			b.Clicked += HeaderButton_Clicked;
			b.TextTransform = TextTransform.None;

			b.FontAttributes = FontAttributes.None;
			b.FontSize = 5 * AndroidSupport.MasterFontSize / 4;
			b.TextColor = AndroidSupport.MasterTextColor;
			b.Margin = b.Padding = new Thickness (1);

			b.CommandParameter = page;
			menuLayout.Children.Add (b);

			return page;
			}

		// Контроль принятия Политики и EULA
		private async void AcceptPolicy (bool Huawei)
			{
			// Контроль XPUN
			if (!Huawei)
				await AndroidSupport.XPUNLoop ();

			// Политика
			if (RDGenerics.GetAppRegistryValue (firstStartRegKey) == "")
				{
				await AndroidSupport.PolicyLoop ();

				// Только после принятия
				RDGenerics.SetAppRegistryValue (firstStartRegKey, ProgramDescription.AssemblyVersion);

				await AndroidSupport.ShowMessage ("Вас приветствует " + ProgramDescription.AssemblyMainName +
					" – " + ProgramDescription.AssemblyDescription + RDLocale.RNRN +
					"На этой странице находится перечень функций приложения, который позволяет перейти " +
					"к нужному разделу. Вернуться сюда можно с помощью кнопки «Назад». Перемещение " +
					"между разделами также доступно по свайпу влево-вправо",

					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				}
			}

		// Отправка значения кнопки в буфер
		private void Field_Clicked (object sender, EventArgs e)
			{
			SendToClipboard (((Xamarin.Forms.Button)sender).Text);
			}

		private void SendToClipboard (string Value)
			{
			RDGenerics.SendToClipboard (Value);
			AndroidSupport.ShowBalloon ("Скопировано в буфер обмена", true);
			}

		// Выбор элемента содержания
		private void HeaderButton_Clicked (object sender, EventArgs e)
			{
			Xamarin.Forms.Button b = (Xamarin.Forms.Button)sender;
			ContentPage p = (ContentPage)b.CommandParameter;
			((CarouselPage)MainPage).CurrentPage = p;
			}

		/// <summary>
		/// Метод выполняет возврат на страницу содержания
		/// </summary>
		public void CallHeadersPage ()
			{
			((CarouselPage)MainPage).CurrentPage = headersPage;
			}

		// Включение / выключение фоновой службы
		private void AllowService_Toggled (object sender, ToggledEventArgs e)
			{
			AppSettings.AllowServiceToStart = allowService.IsToggled;
			}

		// Включение / выключение режима сервис-инженера
		private async void ExtendedMode_Toggled (object sender, EventArgs e)
			{
			if (extendedMode.IsToggled)
				{
				await AndroidSupport.ShowMessage (AppSettings.ExtendedModeMessage,
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				AppSettings.EnableExtendedMode = true;
				}
			else
				{
				await AndroidSupport.ShowMessage (AppSettings.NoExtendedModeMessage,
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				AppSettings.EnableExtendedMode = false;
				}
			}

		/// <summary>
		/// Обработчик события перехода в ждущий режим
		/// </summary>
		protected override void OnSleep ()
			{
			// Переключение состояния
			if (!allowService.IsToggled)
				AndroidSupport.StopRequested = true;

			AndroidSupport.AppIsRunning = false;

			// Сохранение настроек
			AppSettings.CurrentTab = (uint)((CarouselPage)MainPage).Children.IndexOf
				(((CarouselPage)MainPage).CurrentPage);

			// ca.KKTForErrors	// Обновляется в коде программы
			// ca.ErrorCode		// -||-

			AppSettings.FNSerial = fnLifeSerial.Text;
			AppSettings.FNLifeEvFlags = (uint)FNLifeEvFlags;

			AppSettings.KKTSerial = rnmKKTSN.Text;
			AppSettings.UserINN = rnmINN.Text;
			AppSettings.RNMKKT = rnmRNM.Text;

			AppSettings.OFDINN = ofdINN.Text;

			//ca.LowLevelProtocol	// -||-
			//ca.LowLevelCode		// -||-

			//ca.KKTForCodes	// -||-
			AppSettings.CodesText = codesSourceText.Text;
			AppSettings.CodesOftenTexts = kktCodesOftenTexts.ToArray ();

			AppSettings.BarcodeData = barcodeField.Text;
			//ca.CableType		// -||-

			AppSettings.TLVData = tlvTag.Text;

			AppSettings.ConversionNumber = convNumberField.Text;
			AppSettings.ConversionCode = convCodeField.Text;
			AppSettings.ConversionHex = convHexField.Text;
			AppSettings.ConversionText = convTextField.Text;
			//ca.EncodingForConvertor	// -||-

			AppSettings.UserManualFlags = (uint)UserManualFlags;
			}

		/// <summary>
		/// Возврат в интерфейс
		/// </summary>
		protected override void OnResume ()
			{
			AndroidSupport.AppIsRunning = true;
			}

		// Вызов настроек приложения (для Android 12 и выше)
		private void CallAppSettings (object sender, EventArgs e)
			{
			AndroidSupport.CallAppSettings ();
			}

		#endregion

		#region Коды ошибок ККТ

		// Выбор модели ККТ
		private async void ErrorsKKTButton_Clicked (object sender, EventArgs e)
			{
			// Запрос модели ККТ
			List<string> list = kb.Errors.GetKKTTypeNames ();

			// Установка модели
			int res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			errorsKKTButton.Text = list[res];
			AppSettings.KKTForErrors = (uint)res;
			Errors_Find (null, null);
			}

		// Поиск по тексту ошибки
		private void Errors_Find (object sender, EventArgs e)
			{
			List<string> codes = kb.Errors.GetErrorCodesList (AppSettings.KKTForErrors);
			string text = errorSearchText.Text.ToLower ();

			lastErrorSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				string code = codes[j].ToLower ();
				string res = kb.Errors.GetErrorText (AppSettings.KKTForErrors, (uint)j);

				if (code.Contains (text) || res.ToLower ().Contains (text) ||
					code.Contains ("?") && text.Contains (code.Replace ("?", "")))
					{
					lastErrorSearchOffset = (i + lastErrorSearchOffset) % codes.Count;
					AppSettings.ErrorCode = errorSearchText.Text;
					errorsResultText.Text = codes[j] + ": " + res;

					return;
					}
				}

			// Код не найден
			errorsResultText.Text = "(описание ошибки не найдено)";
			}

		private void Errors_Clear (object sender, EventArgs e)
			{
			errorSearchText.Text = "";
			}

		#endregion

		#region Коды символов ККТ

		// Ввод текста для перевода в коды символов
		private void SourceText_TextChanged (object sender, TextChangedEventArgs e)
			{
			kktCodesResultText.Text = kb.CodeTables.DecodeText (AppSettings.KKTForCodes, codesSourceText.Text);
			kktCodesErrorLabel.IsVisible = kktCodesResultText.Text.Contains (KKTCodes.EmptyCode);

			kktCodesLengthLabel.Text = "Длина: " + codesSourceText.Text.Length.ToString ();
			kktCodesCenterButton.IsEnabled = (codesSourceText.Text.Length > 0) &&
				(codesSourceText.Text.Length <= kb.CodeTables.GetKKTStringLength (AppSettings.KKTForCodes));
			}

		// Выбор модели ККТ
		private async void CodesKKTButton_Clicked (object sender, EventArgs e)
			{
			// Запрос модели ККТ
			List<string> list = kb.CodeTables.GetKKTTypeNames ();

			// Установка модели
			int res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			kktCodesKKTButton.Text = list[res];
			AppSettings.KKTForCodes = (uint)res;
			kktCodesHelpLabel.Text = kb.CodeTables.GetKKTTypeDescription (AppSettings.KKTForCodes);

			SourceText_TextChanged (null, null);
			}

		// Очистка полей
		private void CodesClear_Clicked (object sender, EventArgs e)
			{
			codesSourceText.Text = "";
			}

		// Получение из буфера
		private async void CodesLineGet_Clicked (object sender, EventArgs e)
			{
			List<string> items = new List<string> (kktCodesOftenTexts);
			items.Add ("[из буфера обмена]");

			int res = await AndroidSupport.ShowList ("Доступные варианты:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), items);
			if (res < 0)
				return;

			if (res < kktCodesOftenTexts.Count)
				codesSourceText.Text = kktCodesOftenTexts[res];
			else
				codesSourceText.Text = await RDGenerics.GetFromClipboard ();
			}

		// Запоминание текста из поля ввода
		private void TextToConvertRemember_Click (object sender, EventArgs e)
			{
			kktCodesOftenTexts.Add (codesSourceText.Text);
			}

		// Центрирование текста на поле ввода ККТ
		private void TextToConvertCenter_Click (object sender, EventArgs e)
			{
			string text = codesSourceText.Text.Trim ();
			uint total = kb.CodeTables.GetKKTStringLength (AppSettings.KKTForCodes);

			if (text.Length % 2 != 0)
				if (text.Contains (" "))
					text = text.Insert (text.IndexOf (' '), " ");

			codesSourceText.Text = text.PadLeft ((int)(total + text.Length) / 2, ' ');
			}

		#endregion

		#region Команды нижнего уровня

		// Выбор команды нижнего уровня
		private async void LowLevelCommandCodeButton_Clicked (object sender, EventArgs e)
			{
			// Запрос кода ошибки
			List<string> list = kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol);
			int res = 0;
			if (e != null)
				res = await AndroidSupport.ShowList ("Выберите команду:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);

			// Установка результата
			if ((e == null) || (res >= 0))
				{
				AppSettings.LowLevelCode = (uint)res;
				lowLevelCommand.Text = list[res];

				lowLevelCommandCode.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, (uint)res, false);
				lowLevelCommandDescr.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, (uint)res, true);
				}

			list.Clear ();
			}

		// Выбор списка команд
		private async void LowLevelProtocol_Clicked (object sender, EventArgs e)
			{
			// Запрос кода ошибки
			List<string> list = kb.LLCommands.GetProtocolsNames ();

			// Установка результата
			int res = await AndroidSupport.ShowList ("Выберите протокол:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			AppSettings.LowLevelProtocol = (uint)res;
			lowLevelProtocol.Text = list[res];

			// Вызов вложенного обработчика
			LowLevelCommandCodeButton_Clicked (sender, null);
			}

		// Поиск по названию команды нижнего уровня
		private void Command_Find (object sender, EventArgs e)
			{
			List<string> codes = kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol);
			string text = commandSearchText.Text.ToLower ();

			lastCommandSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastCommandSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastCommandSearchOffset = (i + lastCommandSearchOffset) % codes.Count;

					lowLevelCommand.Text = codes[lastCommandSearchOffset];
					AppSettings.LowLevelCode = (uint)lastCommandSearchOffset;

					lowLevelCommandCode.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol,
						(uint)lastCommandSearchOffset, false);
					lowLevelCommandDescr.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol,
						(uint)lastCommandSearchOffset, true);
					return;
					}
			}

		private void Command_Clear (object sender, EventArgs e)
			{
			commandSearchText.Text = "";
			}

		#endregion

		#region Срок жизни ФН

		// Ввод ЗН ФН в разделе определения срока жизни
		private void FNLifeSerial_TextChanged (object sender, TextChangedEventArgs e)
			{
			// Получение описания
			if (!string.IsNullOrWhiteSpace (fnLifeSerial.Text))
				fnLifeModelLabel.Text = kb.FNNumbers.GetFNName (fnLifeSerial.Text);
			else
				fnLifeModelLabel.Text = "(введите ЗН ФН)";

			// Определение длины ключа
			fnLife13.IsToggled = !kb.FNNumbers.IsNotFor36Months (fnLifeSerial.Text);
			fnLife13.IsVisible = !kb.FNNumbers.IsFNKnown (fnLifeSerial.Text);

			// Принудительное изменение
			FnLife13_Toggled (null, null);
			}

		// Изменение параметров пользователя и даты
		private void FnLifeStartDate_DateSelected (object sender, DateChangedEventArgs e)
			{
			FnLife13_Toggled (null, null);
			}

		private void FnLife13_Toggled (object sender, ToggledEventArgs e)
			{
			// Обновление состояний
			if (fnLife13.IsToggled)
				fnLifeLabel.Text = "36 месяцев";
			else
				fnLifeLabel.Text = "13/15 месяцев";

			if (fnLifeGenericTax.IsToggled)
				fnLifeGenericTaxLabel.Text = "УСН / ЕСХН / ПСН";
			else
				fnLifeGenericTaxLabel.Text = "ОСН / совмещение с ОСН";

			if (fnLifeGoods.IsToggled)
				fnLifeGoodsLabel.Text = "услуги";
			else
				fnLifeGoodsLabel.Text = "товары";

			// Расчёт срока
			string res = KKTSupport.GetFNLifeEndDate (fnLifeStartDate.Date, FNLifeEvFlags);

			fnLifeResult.Text = "ФН прекратит работу ";
			if (res.Contains (KKTSupport.FNLifeInacceptableSign))
				{
				fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
				fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);

				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotAcceptableMessage);
				}
			else if (res.Contains (KKTSupport.FNLifeUnwelcomeSign))
				{
				fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningMessage);
				fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningText);

				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotRecommendedMessage);
				}
			else
				{
				fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
				fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);

				fnLifeResultDate = res;
				fnLifeResult.Text += res;
				}

			if (kb.FNNumbers.IsFNKnown (fnLifeSerial.Text))
				{
				if (!kb.FNNumbers.IsFNAllowed (fnLifeSerial.Text))
					{
					fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);

					fnLifeResult.Text += FNSerial.FNIsNotAllowedMessage;
					fnLifeModelLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeModelLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);
					}
				else
					{
					fnLifeModelLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					fnLifeModelLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);
					}
				}
			else
				{
				fnLifeModelLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				fnLifeModelLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionText);
				}
			}

		// Копирование срока жизни ФН
		private void FNLifeResultCopy (object sender, EventArgs e)
			{
			SendToClipboard (fnLifeResultDate);
			}

		// Очистка полей
		private void FNLifeClear_Clicked (object sender, EventArgs e)
			{
			fnLifeSerial.Text = "";
			}

		// Поиск по сигнатуре
		private void FNLifeFind_Clicked (object sender, EventArgs e)
			{
			string sig = kb.FNNumbers.FindSignatureByName (fnLifeSerial.Text);
			if (sig != "")
				fnLifeSerial.Text = sig;
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для расчёта срока жизни ФН
		/// </summary>
		private FNLifeFlags FNLifeEvFlags
			{
			get
				{
				FNLifeFlags flags = KKTSupport.SetFlag (0, FNLifeFlags.FN15, !fnLife13.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.FNExactly13,
					kb.FNNumbers.IsFNExactly13 (fnLifeSerial.Text));
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.GenericTax, !fnLifeGenericTax.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.Goods, !fnLifeGoods.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.Season, fnLifeSeason.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.Agents, fnLifeAgents.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.Excise, fnLifeExcise.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.Autonomous, fnLifeAutonomous.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.FFD12, fnLifeFFD12.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.GamblingAndLotteries, fnLifeGambling.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.PawnsAndInsurance, fnLifePawn.IsToggled);
				flags = KKTSupport.SetFlag (flags, FNLifeFlags.MarkGoods, fnLifeMarkGoods.IsToggled);

				flags = KKTSupport.SetFlag (flags, FNLifeFlags.MarkFN,
					!kb.FNNumbers.IsFNKnown (fnLifeSerial.Text) ||
					kb.FNNumbers.IsFNCompatibleWithFFD12 (fnLifeSerial.Text));

				return flags;
				}
			set
				{
				fnLife13.IsToggled = !KKTSupport.IsSet (value, FNLifeFlags.FN15);
				// .Exactly13 – вспомогательный нехранимый флаг
				fnLifeGenericTax.IsToggled = !KKTSupport.IsSet (value, FNLifeFlags.GenericTax);
				fnLifeGoods.IsToggled = !KKTSupport.IsSet (value, FNLifeFlags.Goods);
				fnLifeSeason.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.Season);
				fnLifeAgents.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.Agents);
				fnLifeExcise.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.Excise);
				fnLifeAutonomous.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.Autonomous);
				fnLifeFFD12.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.FFD12);
				fnLifeGambling.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.GamblingAndLotteries);
				fnLifePawn.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.PawnsAndInsurance);
				fnLifeMarkGoods.IsToggled = KKTSupport.IsSet (value, FNLifeFlags.MarkGoods);
				// .MarkFN – вспомогательный нехранимый флаг
				}
			}

		#endregion

		#region РНМ и ЗН

		// Изменение ИНН ОФД и РН ККТ
		private void RNM_TextChanged (object sender, TextChangedEventArgs e)
			{
			// ЗН ККТ
			if (rnmKKTSN.Text != "")
				{
				rnmKKTTypeLabel.Text = kb.KKTNumbers.GetKKTModel (rnmKKTSN.Text);
				string s = kb.KKTNumbers.GetFFDSupportStatus (rnmKKTSN.Text);
				if (!string.IsNullOrWhiteSpace (s))
					{
					rnmKKTTypeLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					rnmKKTTypeLabel.Text += RDLocale.RN + s;
					}
				else
					{
					rnmKKTTypeLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
					}
				}
			else
				{
				rnmKKTTypeLabel.Text = "(введите ЗН ККТ)";
				rnmKKTTypeLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				}

			// ИНН пользователя
			rnmINNCheckLabel.Text = kb.KKTNumbers.GetRegionName (rnmINN.Text);
			if (KKTSupport.CheckINN (rnmINN.Text) < 0)
				{
				rnmINNCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				rnmINNCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionText);
				rnmINNCheckLabel.Text += " (неполный)";
				}
			else if (KKTSupport.CheckINN (rnmINN.Text) == 0)
				{
				rnmINNCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
				rnmINNCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);
				rnmINNCheckLabel.Text += " (ОК)";
				}
			else
				{
				rnmINNCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningMessage);
				rnmINNCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningText);
				rnmINNCheckLabel.Text += " (возможно, некорректный)";
				}

			// РН
			if (rnmRNM.Text.Length < 10)
				{
				rnmRNMCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				rnmRNMCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionText);
				rnmRNMCheckLabel.Text = "(неполный)";
				}
			else if (KKTSupport.GetFullRNM (rnmINN.Text, rnmKKTSN.Text, rnmRNM.Text.Substring (0, 10)) == rnmRNM.Text)
				{
				rnmRNMCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
				rnmRNMCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);
				rnmRNMCheckLabel.Text = "(OK)";
				}
			else
				{
				rnmRNMCheckLabel.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
				rnmRNMCheckLabel.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);
				rnmRNMCheckLabel.Text = "(некорректный)";
				}
			}

		// Метод генерирует регистрационный номер ККТ
		private void RNMGenerate_Clicked (object sender, EventArgs e)
			{
			if (rnmRNM.Text.Length < 1)
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, rnmKKTSN.Text, "0");
			else if (rnmRNM.Text.Length < 10)
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, rnmKKTSN.Text, rnmRNM.Text);
			else
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, rnmKKTSN.Text, rnmRNM.Text.Substring (0, 10));
			}

		// Очистка полей
		private void RNMClear_Clicked (object sender, EventArgs e)
			{
			rnmKKTSN.Text = "";
			rnmINN.Text = "";
			rnmRNM.Text = "";
			}

		// Поиск по сигнатуре
		private void RNMFind_Clicked (object sender, EventArgs e)
			{
			string sig = kb.KKTNumbers.FindSignatureByName (rnmKKTSN.Text);
			if (sig != "")
				rnmKKTSN.Text = sig;
			}

		// Статистика по базе ЗН ККТ
		private async void RNMStats_Clicked (object sender, EventArgs e)
			{
			await AndroidSupport.ShowMessage (kb.KKTNumbers.RegistryStats,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		#endregion

		#region ОФД

		// Ввод названия или ИНН ОФД
		private void OFDINN_TextChanged (object sender, TextChangedEventArgs e)
			{
			List<string> parameters = kb.Ofd.GetOFDParameters (ofdINN.Text);

			ofdNameButton.Text = parameters[1];

			ofdDNSNameButton.Text = parameters[2];
			ofdIPButton.Text = parameters[3];
			ofdPortButton.Text = parameters[4];

			ofdEmailButton.Text = parameters[5];
			ofdSiteButton.Text = parameters[6];

			ofdDNSNameMButton.Text = parameters[7];
			ofdIPMButton.Text = parameters[8];
			ofdPortMButton.Text = parameters[9];

			// Обработка аннулированных ОФД
			ofdDisabledLabel.IsVisible = !string.IsNullOrWhiteSpace (parameters[10]);
			if (ofdDisabledLabel.IsVisible)
				ofdDisabledLabel.Text = parameters[10];
			}

		private async void OFDName_Clicked (object sender, EventArgs e)
			{
			// Запрос ОФД по имени
			List<string> list = kb.Ofd.GetOFDNames ();

			// Установка результата
			int res = await AndroidSupport.ShowList ("Выберите название ОФД:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			ofdNameButton.Text = list[res];
			SendToClipboard (list[res].Replace ('«', '\"').Replace ('»', '\"'));

			string s = kb.Ofd.GetOFDINNByName (ofdNameButton.Text);
			if (s != "")
				{
				ofdINN.Text = s;
				OFDINN_TextChanged (null, null);
				}
			}

		// Поиск по названию ОФД
		private void OFD_Find (object sender, EventArgs e)
			{
			List<string> codes = kb.Ofd.GetOFDNames ();
			codes.AddRange (kb.Ofd.GetOFDINNs ());

			string text = ofdSearchText.Text.ToLower ();

			lastOFDSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastOFDSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastOFDSearchOffset = (i + lastOFDSearchOffset) % codes.Count;
					ofdNameButton.Text = codes[lastOFDSearchOffset % (codes.Count / 2)];

					string s = kb.Ofd.GetOFDINNByName (ofdNameButton.Text);
					if (s != "")
						AppSettings.OFDINN = ofdINN.Text = s;

					OFDINN_TextChanged (null, null);
					return;
					}
			}

		// Очистка полей
		private void OFDClear_Clicked (object sender, EventArgs e)
			{
			ofdSearchText.Text = "";
			}

		#endregion

		#region О приложении

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Xamarin.Forms.Button b = (Xamarin.Forms.Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Increase))
					AndroidSupport.MasterFontSize += 0.5;
				else if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Decrease))
					AndroidSupport.MasterFontSize -= 0.5;
				}

			aboutFontSizeField.Text = AndroidSupport.MasterFontSize.ToString ("F1");
			aboutFontSizeField.FontSize = AndroidSupport.MasterFontSize;
			}

		#endregion

		#region Руководства пользователей

		// Выбор модели ККТ
		private void UserManualsFlags_Clicked (object sender, EventArgs e)
			{
			UserManualsKKTButton_Clicked (null, null);
			}

		private async void UserManualsKKTButton_Clicked (object sender, EventArgs e)
			{
			int res = (int)AppSettings.KKTForManuals;
			List<string> list = new List<string> (kb.UserGuides.GetKKTList ());

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			userManualsKKTButton.Text = list[res];
			AppSettings.KKTForManuals = (uint)res;

			for (int i = 0; i < operationTextLabels.Count; i++)
				{
				var sections = (UserManualsSections)AppSettings.UserManualSectionsState;
				bool state = sections.HasFlag ((UserManualsSections)(1u << i));

				operationTextButtons[i].Text = UserManuals.OperationTypes[i] + (state ?
					"" : operationButtonSignature);
				operationTextLabels[i].Text = kb.UserGuides.GetManual ((uint)res, (uint)i, UserManualFlags);
				operationTextLabels[i].IsVisible = state;
				}
			}

		// Печать руководства пользователя
		private async void PrintManual_Clicked (object sender, EventArgs e)
			{
			// Выбор варианта (если доступно)
			int res = 0;

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				{
				List<string> modes = new List<string> { "Для кассира", "Для сервис-инженера" };

				res = await AndroidSupport.ShowList ("Сохранить / распечатать руководство:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), modes);
				if (res < 0)
					return;
				}

			// Печать
			UserManualsFlags flags = UserManualFlags;
			if (res == 0)
				flags |= UserManualsFlags.GuideForCashier;

			string text = KKTSupport.BuildUserManual (kb.UserGuides, AppSettings.KKTForManuals,
				AppSettings.UserManualSectionsState, flags);
			KKTSupport.PrintManual (text, pm, res == 0);
			}

		// Смена состояния кнопки
		private void OperationTextButton_Clicked (object sender, EventArgs e)
			{
			byte idx = (byte)operationTextButtons.IndexOf ((Xamarin.Forms.Button)sender);
			bool state = !!operationTextButtons[idx].Text.Contains (operationButtonSignature);
			UserManualsSections sections = (UserManualsSections)AppSettings.UserManualSectionsState;

			if (state)
				sections |= (UserManualsSections)(1u << idx);
			else
				sections &= ~(UserManualsSections)(1u << idx);
			AppSettings.UserManualSectionsState = (uint)sections;

			operationTextButtons[idx].Text = UserManuals.OperationTypes[idx] +
				(state ? "" : operationButtonSignature);
			operationTextLabels[idx].IsVisible = state;
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для руководства пользователя
		/// </summary>
		private UserManualsFlags UserManualFlags
			{
			get
				{
				UserManualsFlags flags = KKTSupport.SetFlag (0, UserManualsFlags.MoreThanOneItemPerDocument,
					moreThanOneItemPerDocument.IsToggled);
				flags = KKTSupport.SetFlag (flags, UserManualsFlags.ProductBaseContainsPrices,
					productBaseContainsPrices.IsToggled);
				flags = KKTSupport.SetFlag (flags, UserManualsFlags.CashiersHavePasswords,
					cashiersHavePasswords.IsToggled);
				flags = KKTSupport.SetFlag (flags, UserManualsFlags.ProductBaseContainsServices,
					baseContainsServices.IsToggled);
				flags = KKTSupport.SetFlag (flags, UserManualsFlags.DocumentsContainMarks,
					documentsContainMarks.IsToggled);

				return flags;
				}
			set
				{
				moreThanOneItemPerDocument.IsToggled = KKTSupport.IsSet (value,
					UserManualsFlags.MoreThanOneItemPerDocument);
				productBaseContainsPrices.IsToggled = KKTSupport.IsSet (value,
					UserManualsFlags.ProductBaseContainsPrices);
				cashiersHavePasswords.IsToggled = KKTSupport.IsSet (value,
					UserManualsFlags.CashiersHavePasswords);
				baseContainsServices.IsToggled = KKTSupport.IsSet (value,
					UserManualsFlags.ProductBaseContainsServices);
				documentsContainMarks.IsToggled = KKTSupport.IsSet (value,
					UserManualsFlags.DocumentsContainMarks);
				}
			}

		#endregion

		#region Теги TLV

		// Поиск тега
		private void TLVFind_Clicked (object sender, EventArgs e)
			{
			if (kb.Tags.FindTag (tlvTag.Text, TLVTags_FFDVersions.FFD_105))
				{
				tlvDescriptionLabel.Text = kb.Tags.LastDescription;
				tlvTypeLabel.Text = kb.Tags.LastType;
				tlvValuesLabel.Text = kb.Tags.LastValuesSet;
				tlvObligationLabel.Text = kb.Tags.LastObligation;
				}
			else
				{
				tlvDescriptionLabel.Text = tlvTypeLabel.Text = tlvValuesLabel.Text =
					tlvObligationLabel.Text = "(не найдено)";
				}
			}

		// Сброс поискового запроса
		private void TLVClear_Clicked (object sender, EventArgs e)
			{
			tlvTag.Text = "";
			TLVFind_Clicked (null, null);
			}

		// Ссылка на приказ-обоснование обязательности тегов
		private async void TLVObligationBase_Click (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (TLVTags.ObligationBaseLink);
				}
			catch
				{
				AndroidSupport.ShowBalloon (RDLocale.GetDefaultText
					(RDLDefaultTexts.Message_BrowserNotAvailable), true);
				}
			}

		#endregion

		#region Штрих-коды

		// Ввод данных штрих-кода
		private void BarcodeText_TextChanged (object sender, TextChangedEventArgs e)
			{
			barcodeField.Text = BarCodes.ConvertFromRussianKeyboard (barcodeField.Text);

			barcodeDescriptionLabel.Text = kb.Barcodes.GetBarcodeDescription (barcodeField.Text);
			}

		// Очистка полей
		private void BarcodeClear_Clicked (object sender, EventArgs e)
			{
			barcodeField.Text = "";
			}

		// Получение из буфера
		private async void BarcodeGet_Clicked (object sender, EventArgs e)
			{
			barcodeField.Text = await RDGenerics.GetFromClipboard ();
			}

		#endregion

		#region Распиновки

		// Выбор кабеля
		private async void CableTypeButton_Clicked (object sender, EventArgs e)
			{
			int res = (int)AppSettings.CableType;
			List<string> list = kb.Plugs.GetCablesNames ();

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите тип кабеля:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);

				// Установка типа кабеля
				if (res < 0)
					return;
				}

			// Установка полей
			cableTypeButton.Text = list[res];
			AppSettings.CableType = (uint)res;

			cableLeftSideText.Text = "Со стороны " + kb.Plugs.GetCableConnector ((uint)res, false);
			cableLeftPinsText.Text = kb.Plugs.GetCableConnectorPins ((uint)res, false);

			cableRightSideText.Text = "Со стороны " + kb.Plugs.GetCableConnector ((uint)res, true);
			cableRightPinsText.Text = kb.Plugs.GetCableConnectorPins ((uint)res, true);

			cableDescriptionText.Text = kb.Plugs.GetCableConnectorDescription ((uint)res, false) +
				RDLocale.RNRN + kb.Plugs.GetCableConnectorDescription ((uint)res, true);
			}

		// Поиск по тексту названия кабеля
		private void Conn_Find (object sender, EventArgs e)
			{
			List<string> conns = kb.Plugs.GetCablesNames ();
			string text = connSearchText.Text.ToLower ();

			lastConnSearchOffset++;
			for (int i = 0; i < conns.Count; i++)
				{
				int j = (i + lastConnSearchOffset) % conns.Count;
				string conn = conns[j].ToLower ();

				if (conn.Contains (text))
					{
					AppSettings.CableType = (uint)j;
					lastConnSearchOffset = j;

					CableTypeButton_Clicked (null, null);
					return;
					}
				}

			// Код не найден
			cableLeftSideText.Text = "(описание не найдено)";
			cableLeftPinsText.Text = cableRightSideText.Text = cableRightPinsText.Text =
				cableDescriptionText.Text = "";
			}

		private void Conn_Clear (object sender, EventArgs e)
			{
			connSearchText.Text = "";
			}

		#endregion

		#region Конверторы

		// Ввод значений для конверсии
		private void ConvNumber_TextChanged (object sender, EventArgs e)
			{
			convNumberResultField.Text = DataConvertors.GetNumberDescription (convNumberField.Text);
			}

		private void ConvNumberAdd_Click (object sender, EventArgs e)
			{
			bool plus = AndroidSupport.IsNameDefault (((Xamarin.Forms.Button)sender).Text,
				RDDefaultButtons.Increase);

			// Извлечение значения с защитой
			double res = DataConvertors.GetNumber (convNumberField.Text);
			if (double.IsNaN (res))
				res = 0.0;

			// Обновление и возврат
			if (plus)
				{
				if (res < DataConvertors.MaxValue)
					res += 1.0;
				else
					res = DataConvertors.MaxValue;
				}
			else
				{
				if (res > 0.0)
					res -= 1.0;
				else
					res = 0.0;
				}

			convNumberField.Text = ((uint)res).ToString ();
			}

		private void ConvCode_TextChanged (object sender, EventArgs e)
			{
			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text, 0);
			convCodeSymbolField.Text = " " + res[0] + " ";
			convCodeResultField.Text = res[1];
			}

		private void ConvCodeAdd_Click (object sender, EventArgs e)
			{
			bool plus = AndroidSupport.IsNameDefault (((Xamarin.Forms.Button)sender).Text,
				RDDefaultButtons.Increase);

			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text, (short)(plus ? 1 : -1));
			convCodeField.Text = res[2];
			}

		// Копирование символа в буфер
		private void CopyCharacter_Click (object sender, EventArgs e)
			{
			SendToClipboard (convCodeSymbolField.Text.Trim ());
			}

		// Сброс полей ввода
		private void ConvNumberClear_Click (object sender, EventArgs e)
			{
			convNumberField.Text = "";
			}

		private void ConvCodeClear_Click (object sender, EventArgs e)
			{
			convCodeField.Text = "";
			}

		// Преобразование hex-данных в текст
		private void ConvertHexToText_Click (object sender, EventArgs e)
			{
			convTextField.Text = DataConvertors.ConvertHexToText (convHexField.Text,
				(RDEncodings)(AppSettings.EncodingForConvertor % encodingModesCount),
				AppSettings.EncodingForConvertor >= encodingModesCount);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			convHexField.Text = DataConvertors.ConvertTextToHex (convTextField.Text,
				(RDEncodings)(AppSettings.EncodingForConvertor % encodingModesCount),
				AppSettings.EncodingForConvertor >= encodingModesCount);
			}

		// Очистка вкладки преобразования данных
		private void ClearConvertText_Click (object sender, EventArgs e)
			{
			convTextField.Text = "";
			convHexField.Text = "";
			}

		// Выбор модели ККТ
		private async void EncodingButton_Clicked (object sender, EventArgs e)
			{
			int res = (int)AppSettings.EncodingForConvertor;
			List<string> list = new List<string> (DataConvertors.AvailableEncodings);

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите кодировку:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			encodingButton.Text = list[res];
			AppSettings.EncodingForConvertor = (uint)res;
			}

		#endregion
		}
	}
