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
			rnmKKTTypeLabel, rnmINNCheckLabel, rnmRNMCheckLabel, rnmSupport105, rnmSupport11, rnmSupport12,
			lowLevelCommandDescr,
			tlvDescriptionLabel, tlvTypeLabel, tlvValuesLabel, tlvObligationLabel,
			barcodeDescriptionLabel, rnmTip, ofdDisabledLabel, convNumberResultField, convCodeResultField,
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
			fnLifeSerial, tlvTag, rnmKKTSN, rnmINN, rnmRNM,
			barcodeField, convNumberField, convCodeField, convHexField, convTextField;

		private Xamarin.Forms.Switch fnLife13, fnLifeGenericTax, fnLifeGoods, fnLifeSeason, fnLifeAgents,
			fnLifeExcise, fnLifeAutonomous, fnLifeFFD12, fnLifeGambling, fnLifePawn, fnLifeMarkGoods,
			keepAppState, allowService, extendedMode,
			moreThanOneItemPerDocument, productBaseContainsPrices, cashiersHavePasswords;

		private Xamarin.Forms.DatePicker fnLifeStartDate;

		private StackLayout userManualLayout, menuLayout;

		#endregion

		#region Основные переменные

		// Опорные классы
		private ConfigAccessor ca;
		private PrintManager pm;
		private KnowledgeBase kb;

		// Поисковые состояния
		private int lastErrorSearchOffset = 0;
		private int lastCommandSearchOffset = 0;
		private int lastOFDSearchOffset = 0;

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
		public App (PrintManager PrintingManager, bool Huawei)
			{
			// Инициализация
			InitializeComponent ();
			ca = new ConfigAccessor ();
			pm = PrintingManager;
			kb = new KnowledgeBase ();

			if (!Localization.IsCurrentLanguageRuRu)
				Localization.CurrentLanguage = SupportedLanguages.ru_ru;

			// Переход в статус запуска для отмены вызова из оповещения
			AndroidSupport.AppIsRunning = true;

			// Переопределение цветов для закрытых функций
			if (!ca.AllowExtendedFunctionsLevel1)
				{
				kktCodesFieldBackColor = AndroidSupport.DefaultGreyColors[2];
				kktCodesMasterBackColor = AndroidSupport.DefaultGreyColors[3];
				}
			if (!ca.AllowExtendedFunctionsLevel2)
				{
				tagsFieldBackColor = lowLevelFieldBackColor = connectorsFieldBackColor =
					AndroidSupport.DefaultGreyColors[2];
				tagsMasterBackColor = lowLevelMasterBackColor = connectorsMasterBackColor =
					AndroidSupport.DefaultGreyColors[3];
				}

			#region Общая конструкция страниц приложения

			MainPage = new MasterPage ();

			headersPage = ApplyPageSettings ("HeadersPage", "Функции приложения",
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
			tagsPage.IsEnabled = ca.AllowExtendedFunctionsLevel2;

			lowLevelPage = ApplyPageSettings ("LowLevelPage", "Команды нижнего уровня",
				lowLevelMasterBackColor, true);
			lowLevelPage.IsEnabled = ca.AllowExtendedFunctionsLevel2;

			kktCodesPage = ApplyPageSettings ("KKTCodesPage", "Перевод текста в коды ККТ",
				kktCodesMasterBackColor, true);
			kktCodesPage.IsEnabled = ca.AllowExtendedFunctionsLevel1;

			connectorsPage = ApplyPageSettings ("ConnectorsPage", "Разъёмы",
				connectorsMasterBackColor, true);
			connectorsPage.IsEnabled = ca.AllowExtendedFunctionsLevel2;

			barCodesPage = ApplyPageSettings ("BarCodesPage", "Штрих-коды",
				barCodesMasterBackColor, true);
			convertorsSNPage = ApplyPageSettings ("ConvertorsSNPage", "Конвертор систем счисления",
				convertors1MasterBackColor, true);
			convertorsUCPage = ApplyPageSettings ("ConvertorsUCPage", "Конвертор символов Unicode",
				convertors2MasterBackColor, true);
			convertorsHTPage = ApplyPageSettings ("ConvertorsHTPage", "Конвертор двоичных данных",
				convertors3MasterBackColor, true);

			aboutPage = ApplyPageSettings ("AboutPage",
				Localization.GetDefaultText (LzDefaultTextValues.Control_AppAbout),
				aboutMasterBackColor, true);

			AndroidSupport.SetMainPage (MainPage);

			#endregion

			#region Страница «оглавления»

			AndroidSupport.ApplyLabelSettings (headersPage, "KeepAppStateLabel",
				"Помнить настройки приложения", ASLabelTypes.DefaultLeft);
			keepAppState = AndroidSupport.ApplySwitchSettings (headersPage, "KeepAppState", false,
				headersFieldBackColor, null, ca.KeepApplicationState);

			AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceLabel",
				"Оставить службу активной после выхода", ASLabelTypes.DefaultLeft);
			allowService = AndroidSupport.ApplySwitchSettings (headersPage, "AllowService", false,
				headersFieldBackColor, AllowService_Toggled, AndroidSupport.AllowServiceToStart);

			AndroidSupport.ApplyLabelSettings (headersPage, "ExtendedModeLabel",
				"Режим сервис-инженера", ASLabelTypes.DefaultLeft);
			extendedMode = AndroidSupport.ApplySwitchSettings (headersPage, "ExtendedMode", false,
				headersFieldBackColor, ExtendedMode_Toggled, ca.AllowExtendedMode);

			try
				{
				((CarouselPage)MainPage).CurrentPage = ((CarouselPage)MainPage).Children[(int)ca.CurrentTab];
				}
			catch { }

			#endregion

			#region Страница инструкций

			Label ut = AndroidSupport.ApplyLabelSettings (userManualsPage, "SelectionLabel", "Модель ККТ:",
				ASLabelTypes.HeaderLeft);
			userManualLayout = (StackLayout)userManualsPage.FindByName ("UserManualLayout");
			int operationsCount = ca.AllowExtendedFunctionsLevel1 ? UserManuals.OperationTypes.Length :
				UserManuals.OperationsForCashiers.Length;

			for (int i = 0; i < operationsCount; i++)
				{
				Xamarin.Forms.Button bh = new Xamarin.Forms.Button ();
				bh.BackgroundColor = userManualsMasterBackColor;
				bh.Clicked += OperationTextButton_Clicked;
				bh.FontAttributes = FontAttributes.Bold;
				bh.FontSize = ut.FontSize;
				bh.HorizontalOptions = LayoutOptions.Center;
				bh.IsVisible = true;
				bh.Margin = ut.Margin;
				bh.Padding = bh.Margin = new Thickness (0);
				bh.Text = UserManuals.OperationTypes[i] + (ca.GetUserManualSectionState ((byte)i) ?
					"" : operationButtonSignature);
				bh.TextColor = ut.TextColor;

				Label lt = new Label ();
				lt.BackgroundColor = userManualsFieldBackColor;
				lt.FontAttributes = FontAttributes.None;
				lt.FontSize = ut.FontSize;
				lt.HorizontalOptions = LayoutOptions.Fill;
				lt.HorizontalTextAlignment = TextAlignment.Start;
				lt.IsVisible = ca.GetUserManualSectionState ((byte)i);
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
			userManualsPrintButton.Padding = new Thickness (0);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "HelpLabel",
				"<...> – индикация на экране, [...] – клавиши ККТ." + Localization.RN +
				"Нажатие на заголовки разделов позволяет добавить или скрыть их в видимой и печатной " +
				"версиях этой инструкции",
				ASLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "OneItemLabel", "В чеках бывает более одной позиции",
				ASLabelTypes.DefaultLeft);
			moreThanOneItemPerDocument = AndroidSupport.ApplySwitchSettings (userManualsPage, "OneItemSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "PricesLabel", "В базе товаров есть цены",
				ASLabelTypes.DefaultLeft);
			productBaseContainsPrices = AndroidSupport.ApplySwitchSettings (userManualsPage, "PricesSwitch",
				false, userManualsFieldBackColor, null, false);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "PasswordsLabel", "Кассиры пользуются паролями",
				ASLabelTypes.DefaultLeft);
			cashiersHavePasswords = AndroidSupport.ApplySwitchSettings (userManualsPage, "PasswordsSwitch",
				false, userManualsFieldBackColor, null, false);

			UserManualFlags = ca.UserManualFlags;
			moreThanOneItemPerDocument.Toggled += UserManualsFlags_Clicked;
			productBaseContainsPrices.Toggled += UserManualsFlags_Clicked;
			cashiersHavePasswords.Toggled += UserManualsFlags_Clicked;

			UserManualsKKTButton_Clicked (null, null);

			#endregion

			#region Страница кодов символов ККТ

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "SelectionLabel", "Модель ККТ:",
				ASLabelTypes.HeaderLeft);

			string kktTypeName;
			try
				{
				kktTypeName = kb.CodeTables.GetKKTTypeNames ()[(int)ca.KKTForCodes];
				}
			catch
				{
				kktTypeName = kb.CodeTables.GetKKTTypeNames ()[0];
				ca.KKTForCodes = 0;
				}
			kktCodesOftenTexts.AddRange (ca.CodesOftenTexts);

			kktCodesKKTButton = AndroidSupport.ApplyButtonSettings (kktCodesPage, "KKTButton",
				kktTypeName, kktCodesFieldBackColor, CodesKKTButton_Clicked, true);
			AndroidSupport.ApplyLabelSettings (kktCodesPage, "SourceTextLabel",
				"Исходный текст:", ASLabelTypes.HeaderLeft);

			codesSourceText = AndroidSupport.ApplyEditorSettings (kktCodesPage, "SourceText",
				kktCodesFieldBackColor, Keyboard.Default, 72, ca.CodesText, SourceText_TextChanged, true);
			codesSourceText.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultTextLabel", "Коды ККТ:",
				ASLabelTypes.HeaderLeft);

			kktCodesErrorLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ErrorLabel",
				"Часть введённых символов не поддерживается данной ККТ", ASLabelTypes.ErrorTip);

			kktCodesResultText = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultText", " ",
				ASLabelTypes.FieldMonotype, kktCodesFieldBackColor);

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpTextLabel", "Пояснения к вводу:",
				ASLabelTypes.HeaderLeft);
			kktCodesHelpLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpText",
				kb.CodeTables.GetKKTTypeDescription (ca.KKTForCodes), ASLabelTypes.Field,
				kktCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (kktCodesPage, "ClearText",
				ASButtonDefaultTypes.Delete, kktCodesFieldBackColor, CodesClear_Clicked);
			AndroidSupport.ApplyButtonSettings (kktCodesPage, "SelectSavedText",
				ASButtonDefaultTypes.Select, kktCodesFieldBackColor, CodesLineGet_Clicked);

			kktCodesLengthLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "LengthLabel",
				"Длина:", ASLabelTypes.HeaderLeft);
			kktCodesCenterButton = AndroidSupport.ApplyButtonSettings (kktCodesPage, "CenterText",
				"Центрировать", kktCodesFieldBackColor, TextToConvertCenter_Click, false);
			AndroidSupport.ApplyButtonSettings (kktCodesPage, "RememberText",
				"Запомнить", kktCodesFieldBackColor, TextToConvertRemember_Click, false);

			SourceText_TextChanged (null, null);    // Протягивание кодов

			#endregion

			#region Страница ошибок

			AndroidSupport.ApplyLabelSettings (errorsPage, "SelectionLabel", "Модель ККТ:",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (errorsPage, "SearchLabel", "Поиск ошибки по коду или фрагменту описания:",
				ASLabelTypes.HeaderLeft);

			try
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[(int)ca.KKTForErrors];
				}
			catch
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[0];
				ca.KKTForErrors = 0;
				}
			errorsKKTButton = AndroidSupport.ApplyButtonSettings (errorsPage, "KKTButton",
				kktTypeName, errorsFieldBackColor, ErrorsKKTButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (errorsPage, "ResultTextLabel", "Описание ошибки:",
				ASLabelTypes.HeaderLeft);

			errorsResultText = AndroidSupport.ApplyLabelSettings (errorsPage, "ResultText",
				"", ASLabelTypes.Field, errorsFieldBackColor);
			errorSearchText = AndroidSupport.ApplyEditorSettings (errorsPage, "ErrorSearchText", errorsFieldBackColor,
				Keyboard.Default, 30, ca.ErrorCode, null, true);
			Errors_Find (null, null);

			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorSearchButton",
				ASButtonDefaultTypes.Find, errorsFieldBackColor, Errors_Find);
			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorClearButton",
				ASButtonDefaultTypes.Delete, errorsFieldBackColor, Errors_Clear);

			#endregion

			#region Страница "О программе"

			AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, ASLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				Localization.GetDefaultText (LzDefaultTextValues.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				Localization.GetDefaultText (LzDefaultTextValues.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Control_GenericSettings),
				ASLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Message_RestartRequired),
				ASLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeLabel",
				Localization.GetDefaultText (LzDefaultTextValues.Control_InterfaceFontSize),
				ASLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeInc",
				ASButtonDefaultTypes.Increase, aboutFieldBackColor, FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeDec",
				ASButtonDefaultTypes.Decrease, aboutFieldBackColor, FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeField",
				" ", ASLabelTypes.DefaultCenter);

			FontSizeButton_Clicked (null, null);

			#endregion

			#region Страница определения срока жизни ФН

			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetModelLabel", "ЗН, номинал или модель ФН:",
				ASLabelTypes.HeaderLeft);
			fnLifeSerial = AndroidSupport.ApplyEditorSettings (fnLifePage, "FNLifeSerial", fnLifeFieldBackColor,
				Keyboard.Default, 16, ca.FNSerial, FNLifeSerial_TextChanged, true);
			fnLifeSerial.Margin = new Thickness (0);

			fnLife13 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLife13", true,
				fnLifeFieldBackColor, FnLife13_Toggled, false);

			fnLifeLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeLabel",
				"", ASLabelTypes.DefaultLeft);

			//
			fnLifeModelLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeModelLabel",
				"", ASLabelTypes.Semaphore);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetUserParameters", "Значимые параметры:",
				ASLabelTypes.HeaderLeft);

			fnLifeGenericTax = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGenericTax", true,
				fnLifeFieldBackColor, null, false);
			fnLifeGenericTaxLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGenericTaxLabel",
				"", ASLabelTypes.DefaultLeft);

			fnLifeGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGoods", true,
				fnLifeFieldBackColor, null, false);
			fnLifeGoodsLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGoodsLabel",
				"", ASLabelTypes.DefaultLeft);

			fnLifeSeason = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeSeason", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeSeasonLabel", "Сезонная торговля",
				ASLabelTypes.DefaultLeft);

			fnLifeAgents = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAgents", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAgentsLabel", "Платёжный (суб)агент",
				ASLabelTypes.DefaultLeft);

			fnLifeExcise = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeExcise", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeExciseLabel", "Подакцизные товары",
				ASLabelTypes.DefaultLeft);

			fnLifeAutonomous = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAutonomous", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAutonomousLabel", "Автономный режим",
				ASLabelTypes.DefaultLeft);

			fnLifeFFD12 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeFFD12", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeFFD12Label", "ФФД 1.1, 1.2",
				ASLabelTypes.DefaultLeft);

			fnLifeGambling = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGambling", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGamblingLabel", "Азартные игры и лотереи",
				ASLabelTypes.DefaultLeft);

			fnLifePawn = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifePawn", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifePawnLabel", "Ломбарды и страхование",
				ASLabelTypes.DefaultLeft);

			fnLifeMarkGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeMarkGoods", false,
				fnLifeFieldBackColor, null, false);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeMarkGoodsLabel", "Маркированные товары",
				ASLabelTypes.DefaultLeft);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetDate", "Дата фискализации:",
				ASLabelTypes.DefaultLeft);
			fnLifeStartDate = AndroidSupport.ApplyDatePickerSettings (fnLifePage, "FNLifeStartDate",
				fnLifeFieldBackColor, FnLifeStartDate_DateSelected);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeResultLabel", "Результат:",
				ASLabelTypes.HeaderLeft);
			fnLifeResult = AndroidSupport.ApplyButtonSettings (fnLifePage, "FNLifeResult", "",
				fnLifeFieldBackColor, FNLifeResultCopy, true);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeHelpLabel",
				"Нажатие кнопки копирует дату окончания срока жизни в буфер обмена",
				ASLabelTypes.Tip);

			//
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Clear",
				ASButtonDefaultTypes.Delete, fnLifeFieldBackColor, FNLifeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Find",
				ASButtonDefaultTypes.Find, fnLifeFieldBackColor, FNLifeFind_Clicked);

			// Применение всех названий
			FNLifeEvFlags = ca.FNLifeEvFlags;
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
				ASLabelTypes.HeaderLeft);
			rnmKKTSN = AndroidSupport.ApplyEditorSettings (rnmPage, "SN", rnmFieldBackColor, Keyboard.Default,
				kb.KKTNumbers.MaxSerialNumberLength, ca.KKTSerial, RNM_TextChanged, true);

			rnmKKTTypeLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "TypeLabel",
				"", ASLabelTypes.DefaultLeft);

			AndroidSupport.ApplyLabelSettings (rnmPage, "INNLabel", "ИНН пользователя:",
				ASLabelTypes.HeaderLeft);
			rnmINN = AndroidSupport.ApplyEditorSettings (rnmPage, "INN", rnmFieldBackColor, Keyboard.Numeric, 12,
				ca.UserINN, RNM_TextChanged, true);

			rnmINNCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "INNCheckLabel", "",
				ASLabelTypes.Semaphore);

			if (ca.AllowExtendedFunctionsLevel2)
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки или произвольное число для генерации:",
					ASLabelTypes.HeaderLeft);
			else
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки:", ASLabelTypes.HeaderLeft);

			rnmRNM = AndroidSupport.ApplyEditorSettings (rnmPage, "RNM", rnmFieldBackColor, Keyboard.Numeric, 16,
				ca.RNMKKT, RNM_TextChanged, true);

			rnmRNMCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMCheckLabel", "",
				ASLabelTypes.Semaphore);

			rnmGenerate = AndroidSupport.ApplyButtonSettings (rnmPage, "RNMGenerate",
				ASButtonDefaultTypes.Create, rnmFieldBackColor, RNMGenerate_Clicked);
			rnmGenerate.IsVisible = ca.AllowExtendedFunctionsLevel2;

			string rnmAbout = "Индикатор ФФД: " +
				"<b><font color=\"#FF4040\">красный</font></b> – поддержка не планируется; " +
				"<b><font color=\"#00C000\">зелёный</font></b> – поддерживается; " +
				"<b><font color=\"#FFFF00\">жёлтый</font></b> – планируется; " +
				"<b><font color=\"#6060FF\">синий</font></b> – нет сведений (на момент релиза этой версии приложения)";
			if (ca.AllowExtendedFunctionsLevel2)
				rnmAbout += ("<br/><br/>Первые 10 цифр РН являются порядковым номером ККТ в реестре и " +
					"могут быть указаны вручную при генерации");

			rnmTip = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMAbout", rnmAbout,
				ASLabelTypes.Tip);
			rnmTip.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (rnmPage, "Clear",
				ASButtonDefaultTypes.Delete, rnmFieldBackColor, RNMClear_Clicked);
			AndroidSupport.ApplyButtonSettings (rnmPage, "Find",
				ASButtonDefaultTypes.Find, rnmFieldBackColor, RNMFind_Clicked);

			rnmSupport105 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport105", "ФФД 1.05",
				ASLabelTypes.Semaphore);
			rnmSupport11 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport11", "ФФД 1.1",
				ASLabelTypes.Semaphore);
			rnmSupport12 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport12", "ФФД 1.2",
				ASLabelTypes.Semaphore);
			rnmSupport105.Padding = rnmSupport11.Padding = rnmSupport12.Padding = new Thickness (6);

			RNM_TextChanged (null, null);   // Применение значений

			#endregion

			#region Страница настроек ОФД

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDINNLabel", "ИНН ОФД:",
				ASLabelTypes.HeaderLeft);
			ofdINN = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDINN", ca.OFDINN, ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNameLabel", "Название:",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSearchLabel", "Поиск по ИНН или фрагменту названия:",
				ASLabelTypes.HeaderLeft);
			ofdNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDName", "- Выберите или введите ИНН -",
				ofdFieldBackColor, OFDName_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameLabel", "Адрес ОФД:",
				ASLabelTypes.HeaderLeft);
			ofdDNSNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSName", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIP", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPort", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameMLabel", "Адрес ИСМ:",
				ASLabelTypes.HeaderLeft);
			ofdDNSNameMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortM", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameKLabel", "Адрес ОКП (стандартный):",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameK", OFD.OKPSite,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPK", OFD.OKPIP,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortK", OFD.OKPPort,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDEmailLabel", "E-mail отправителя чеков:",
				ASLabelTypes.HeaderLeft);
			ofdEmailButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDEmail", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSiteLabel", "Сайт ОФД:",
				ASLabelTypes.HeaderLeft);
			ofdSiteButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSite", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNalogSiteLabel", "Сайт ФНС:",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDNalogSite", OFD.FNSSite,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDYaDNSLabel", "Яндекс DNS:",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDYaDNS1", OFD.YandexDNSReq,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDYaDNS2", OFD.YandexDNSAlt,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDHelpLabel",
				"Нажатие кнопок копирует их подписи в буфер обмена", ASLabelTypes.Tip);

			AndroidSupport.ApplyButtonSettings (ofdPage, "Clear",
				ASButtonDefaultTypes.Delete, ofdFieldBackColor, OFDClear_Clicked);

			ofdSearchText = AndroidSupport.ApplyEditorSettings (ofdPage, "OFDSearchText", ofdFieldBackColor,
				Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSearchButton",
				ASButtonDefaultTypes.Find, ofdFieldBackColor, OFD_Find);

			ofdDisabledLabel = AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDisabledLabel",
				"Аннулирован", ASLabelTypes.ErrorTip);
			ofdDisabledLabel.IsVisible = false;

			OFDINN_TextChanged (null, null); // Протягивание значений

			#endregion

			#region Страница TLV-тегов

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVSearchLabel", "Поиск по номеру или фрагменту описания:",
				ASLabelTypes.HeaderLeft);
			tlvTag = AndroidSupport.ApplyEditorSettings (tagsPage, "TLVSearchText", tagsFieldBackColor,
				Keyboard.Default, 20, ca.TLVData, null, true);

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVSearchButton",
				ASButtonDefaultTypes.Find, tagsFieldBackColor, TLVFind_Clicked);
			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVClearButton",
				ASButtonDefaultTypes.Delete, tagsFieldBackColor, TLVClear_Clicked);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescriptionLabel", "Описание тега:",
				ASLabelTypes.HeaderLeft);
			tlvDescriptionLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescription", "",
				ASLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVTypeLabel", "Тип тега:",
				ASLabelTypes.HeaderLeft);
			tlvTypeLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVType", "",
				ASLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValuesLabel", "Возможные значения тега:",
				ASLabelTypes.HeaderLeft);
			tlvValuesLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValues", "",
				ASLabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligationLabel", "Обязательность:",
				ASLabelTypes.HeaderLeft);
			tlvObligationLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligation", "",
				ASLabelTypes.Field, tagsFieldBackColor);
			tlvObligationLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVObligationHelpLabel",
				TLVTags.ObligationBase, tagsFieldBackColor, TLVObligationBase_Click, false);

			TLVFind_Clicked (null, null);

			#endregion

			#region Страница команд нижнего уровня

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "ProtocolLabel", "Протокол:",
				ASLabelTypes.HeaderLeft);
			lowLevelProtocol = AndroidSupport.ApplyButtonSettings (lowLevelPage, "ProtocolButton",
				kb.LLCommands.GetProtocolsNames ()[(int)ca.LowLevelProtocol], lowLevelFieldBackColor,
				LowLevelProtocol_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandLabel", "Команда:",
				ASLabelTypes.HeaderLeft);
			lowLevelCommand = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandButton",
				kb.LLCommands.GetCommandsList (ca.LowLevelProtocol)[(int)ca.LowLevelCode],
				lowLevelFieldBackColor, LowLevelCommandCodeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandSearchLabel", "Поиск по описанию команды:",
				ASLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandCodeLabel", "Код команды:",
				ASLabelTypes.HeaderLeft);
			lowLevelCommandCode = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandCodeButton",
				kb.LLCommands.GetCommand (ca.LowLevelProtocol, ca.LowLevelCode, false),
				lowLevelFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescrLabel", "Пояснение:",
				ASLabelTypes.HeaderLeft);

			lowLevelCommandDescr = AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescr",
				kb.LLCommands.GetCommand (ca.LowLevelProtocol, ca.LowLevelCode, true), ASLabelTypes.Field,
				lowLevelFieldBackColor);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "LowLevelHelpLabel",
				"Нажатие кнопки копирует команду в буфер обмена", ASLabelTypes.Tip);

			commandSearchText = AndroidSupport.ApplyEditorSettings (lowLevelPage, "CommandSearchText",
				lowLevelFieldBackColor, Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandSearchButton",
				ASButtonDefaultTypes.Find, lowLevelFieldBackColor, Command_Find);
			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandClearButton",
				ASButtonDefaultTypes.Delete, lowLevelFieldBackColor, Command_Clear);

			#endregion

			#region Страница штрих-кодов

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeFieldLabel", "Данные штрих-кода:",
				ASLabelTypes.HeaderLeft);
			barcodeField = AndroidSupport.ApplyEditorSettings (barCodesPage, "BarcodeField",
				barCodesFieldBackColor, Keyboard.Default, BarCodes.MaxSupportedDataLength,
				ca.BarcodeData, BarcodeText_TextChanged, true);

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescriptionLabel",
				"Описание штрих-кода:", ASLabelTypes.HeaderLeft);
			barcodeDescriptionLabel = AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescription", "",
				ASLabelTypes.Field, barCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (barCodesPage, "Clear",
				ASButtonDefaultTypes.Delete, barCodesFieldBackColor, BarcodeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (barCodesPage, "GetFromClipboard",
				ASButtonDefaultTypes.Copy, barCodesFieldBackColor, BarcodeGet_Clicked);

			BarcodeText_TextChanged (null, null);

			#endregion

			#region Страница распиновок

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLabel", "Тип кабеля:",
				ASLabelTypes.HeaderLeft);
			cableTypeButton = AndroidSupport.ApplyButtonSettings (connectorsPage, "CableTypeButton",
				kb.Plugs.GetCablesNames ()[(int)ca.CableType], connectorsFieldBackColor, CableTypeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescriptionLabel", "Сопоставление контактов:",
				ASLabelTypes.HeaderLeft);
			cableLeftSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftSide",
				" ", ASLabelTypes.DefaultLeft);
			cableLeftSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableLeftSideText.HorizontalOptions = LayoutOptions.Center;

			cableLeftPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftPins", " ",
				ASLabelTypes.FieldMonotype, connectorsFieldBackColor);

			cableRightSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightSide",
				" ", ASLabelTypes.DefaultLeft);
			cableRightSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableRightSideText.HorizontalOptions = LayoutOptions.Center;

			cableRightPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightPins", " ",
				ASLabelTypes.FieldMonotype, connectorsFieldBackColor);
			cableLeftPinsText.HorizontalTextAlignment = cableRightPinsText.HorizontalTextAlignment =
				TextAlignment.Center;

			cableDescriptionText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescription",
				" ", ASLabelTypes.Tip);

			CableTypeButton_Clicked (null, null);

			#endregion

			#region Страница конвертора систем счисления

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberLabel",
				"Число:", ASLabelTypes.HeaderLeft);
			convNumberField = AndroidSupport.ApplyEditorSettings (convertorsSNPage, "ConvNumberField",
				convertors1FieldBackColor, Keyboard.Default, 10, ca.ConversionNumber, ConvNumber_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberInc",
				ASButtonDefaultTypes.Increase, convertors1FieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberDec",
				ASButtonDefaultTypes.Decrease, convertors1FieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberClear",
				ASButtonDefaultTypes.Delete, convertors1FieldBackColor, ConvNumberClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultLabel", "Представление:",
				ASLabelTypes.HeaderLeft);
			convNumberResultField = AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultField",
				" ", ASLabelTypes.FieldMonotype, convertors1FieldBackColor);
			ConvNumber_TextChanged (null, null);

			const string convHelp = "Шестнадцатеричные числа следует начинать с символов “0x”";
			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvHelpLabel", convHelp,
				ASLabelTypes.Tip);

			#endregion

			#region Страница конвертора символов Unicode

			encodingModesCount = (uint)(DataConvertors.AvailableEncodings.Length / 2);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeLabel",
				"Код символа" + Localization.RN + "или символ:", ASLabelTypes.HeaderLeft);
			convCodeField = AndroidSupport.ApplyEditorSettings (convertorsUCPage, "ConvCodeField",
				convertors2FieldBackColor, Keyboard.Default, 10, ca.ConversionCode, ConvCode_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeInc",
				ASButtonDefaultTypes.Increase, convertors2FieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeDec",
				ASButtonDefaultTypes.Decrease, convertors2FieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeClear",
				ASButtonDefaultTypes.Delete, convertors2FieldBackColor, ConvCodeClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultLabel", "Символ Unicode:",
				ASLabelTypes.HeaderLeft);
			convCodeResultField = AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultField",
				"", ASLabelTypes.FieldMonotype, convertors2FieldBackColor);

			convCodeSymbolField = AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeSymbolField",
				" ", convertors2FieldBackColor, CopyCharacter_Click, true);
			convCodeSymbolField.FontSize *= 5;
			ConvCode_TextChanged (null, null);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvHelpLabel",
				convHelp + "." + Localization.RN + "Нажатие кнопки с символом Unicode копирует его в буфер обмена",
				ASLabelTypes.Tip);

			#endregion

			#region Страница конвертора двоичных данных

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "HexLabel",
				"Данные (hex):", ASLabelTypes.HeaderLeft);
			convHexField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "HexField",
				convertors3FieldBackColor, Keyboard.Default, 500, ca.ConversionHex, null, true);
			convHexField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "HexToTextButton",
				ASButtonDefaultTypes.Down, convertors3FieldBackColor, ConvertHexToText_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "TextToHexButton",
				ASButtonDefaultTypes.Up, convertors3FieldBackColor, ConvertTextToHex_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "ClearButton",
				ASButtonDefaultTypes.Delete, convertors3FieldBackColor, ClearConvertText_Click);

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "TextLabel",
				"Данные (текст):", ASLabelTypes.HeaderLeft);
			convTextField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "TextField",
				convertors3FieldBackColor, Keyboard.Default, 250, ca.ConversionText, null, true);
			convTextField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "EncodingLabel",
				"Кодировка:", ASLabelTypes.HeaderLeft);
			encodingButton = AndroidSupport.ApplyButtonSettings (convertorsHTPage, "EncodingButton",
				" ", convertors3FieldBackColor, EncodingButton_Clicked, true);
			EncodingButton_Clicked (null, null);

			#endregion

			// Обязательное принятие Политики и EULA
			AcceptPolicy (Huawei);
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
			await AndroidSupport.XPUNLoop (Huawei);

			// Политика
			if (RDGenerics.GetAppSettingsValue (firstStartRegKey) == "")
				{
				await AndroidSupport.PolicyLoop ();

				// Вступление
				RDGenerics.SetAppSettingsValue (firstStartRegKey, ProgramDescription.AssemblyVersion);
				// Только после принятия

				await AndroidSupport.ShowMessage ("Вас приветствует инструмент сервис-инженера ККТ (54-ФЗ)!" +
					Localization.RNRN +
					"На этой странице находится перечень функций приложения, который позволяет перейти " +
					"к нужному разделу. Вернуться сюда можно с помощью кнопки «Назад». Перемещение " +
					"между разделами также доступно по свайпу влево-вправо",

					Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
				}

			// Шрифт интерфейса
			if (AndroidSupport.AllowFontSizeTip)
				{
				await AndroidSupport.ShowMessage (
					Localization.GetDefaultText (LzDefaultTextValues.Message_FontSizeAvailable),
					Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
				}
			}

		// Запрос цвета, соответствующего статусу поддержки
		private Color StatusToColor (KKTSerial.FFDSupportStatuses Status)
			{
			if (Status == KKTSerial.FFDSupportStatuses.Planned)
				return Color.FromHex ("#FFFFC8");

			if (Status == KKTSerial.FFDSupportStatuses.Supported)
				return Color.FromHex ("#C8FFC8");

			if (Status == KKTSerial.FFDSupportStatuses.Unsupported)
				return Color.FromHex ("#FFC8C8");

			// Остальные
			return Color.FromHex ("#C8C8FF");
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
			AndroidSupport.AllowServiceToStart = allowService.IsToggled;
			}

		// Включение / выключение режима сервис-инженера
		private async void ExtendedMode_Toggled (object sender, EventArgs e)
			{
			if (extendedMode.IsToggled)
				{
				await AndroidSupport.ShowMessage (ConfigAccessor.ExtendedModeMessage,
					Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
				ca.AllowExtendedMode = true;
				}
			else
				{
				await AndroidSupport.ShowMessage (ConfigAccessor.NoExtendedModeMessage,
					Localization.GetDefaultText (LzDefaultTextValues.Button_OK));
				ca.AllowExtendedMode = false;
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
			ca.KeepApplicationState = keepAppState.IsToggled;
			if (!keepAppState.IsToggled)
				return;

			ca.CurrentTab = (uint)((CarouselPage)MainPage).Children.IndexOf (((CarouselPage)MainPage).CurrentPage);

			// ca.KKTForErrors	// Обновляется в коде программы
			// ca.ErrorCode		// -||-

			ca.FNSerial = fnLifeSerial.Text;
			ca.FNLifeEvFlags = FNLifeEvFlags;

			ca.KKTSerial = rnmKKTSN.Text;
			ca.UserINN = rnmINN.Text;
			ca.RNMKKT = rnmRNM.Text;

			ca.OFDINN = ofdINN.Text;

			//ca.LowLevelProtocol	// -||-
			//ca.LowLevelCode		// -||-

			//ca.KKTForCodes	// -||-
			ca.CodesText = codesSourceText.Text;
			ca.CodesOftenTexts = kktCodesOftenTexts.ToArray ();

			ca.BarcodeData = barcodeField.Text;
			//ca.CableType		// -||-

			ca.TLVData = tlvTag.Text;

			ca.ConversionNumber = convNumberField.Text;
			ca.ConversionCode = convCodeField.Text;
			ca.ConversionHex = convHexField.Text;
			ca.ConversionText = convTextField.Text;
			//ca.EncodingForConvertor	// -||-

			ca.UserManualFlags = UserManualFlags;
			}

		/// <summary>
		/// Возврат в интерфейс
		/// </summary>
		protected override void OnResume ()
			{
			AndroidSupport.AppIsRunning = true;
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
				Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);
			if (res < 0)
				return;

			errorsKKTButton.Text = list[res];
			ca.KKTForErrors = (uint)res;
			Errors_Find (null, null);
			}

		// Поиск по тексту ошибки
		private void Errors_Find (object sender, EventArgs e)
			{
			List<string> codes = kb.Errors.GetErrorCodesList (ca.KKTForErrors);
			string text = errorSearchText.Text.ToLower ();

			lastErrorSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				string code = codes[j].ToLower ();
				string res = kb.Errors.GetErrorText (ca.KKTForErrors, (uint)j);

				if (code.Contains (text) || res.ToLower ().Contains (text) ||
					code.Contains ("?") && text.Contains (code.Replace ("?", "")))
					{
					lastErrorSearchOffset = (i + lastErrorSearchOffset) % codes.Count;
					ca.ErrorCode = errorSearchText.Text;
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
			kktCodesResultText.Text = kb.CodeTables.DecodeText (ca.KKTForCodes, codesSourceText.Text);
			kktCodesErrorLabel.IsVisible = kktCodesResultText.Text.Contains (KKTCodes.EmptyCode);

			kktCodesLengthLabel.Text = "Длина: " + codesSourceText.Text.Length.ToString ();
			kktCodesCenterButton.IsEnabled = (codesSourceText.Text.Length > 0) &&
				(codesSourceText.Text.Length <= kb.CodeTables.GetKKTStringLength (ca.KKTForCodes));
			}

		// Выбор модели ККТ
		private async void CodesKKTButton_Clicked (object sender, EventArgs e)
			{
			// Запрос модели ККТ
			List<string> list = kb.CodeTables.GetKKTTypeNames ();

			// Установка модели
			int res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
				Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);
			if (res < 0)
				return;

			kktCodesKKTButton.Text = list[res];
			ca.KKTForCodes = (uint)res;
			kktCodesHelpLabel.Text = kb.CodeTables.GetKKTTypeDescription (ca.KKTForCodes);

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
				Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), items);
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
			uint total = kb.CodeTables.GetKKTStringLength (ca.KKTForCodes);

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
			List<string> list = kb.LLCommands.GetCommandsList (ca.LowLevelProtocol);
			int res = 0;
			if (e != null)
				res = await AndroidSupport.ShowList ("Выберите команду:",
					Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);

			// Установка результата
			if ((e == null) || (res >= 0))
				{
				ca.LowLevelCode = (uint)res;
				lowLevelCommand.Text = list[res];

				lowLevelCommandCode.Text = kb.LLCommands.GetCommand (ca.LowLevelProtocol, (uint)res, false);
				lowLevelCommandDescr.Text = kb.LLCommands.GetCommand (ca.LowLevelProtocol, (uint)res, true);
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
				Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);
			if (res < 0)
				return;

			ca.LowLevelProtocol = (uint)res;
			lowLevelProtocol.Text = list[res];

			// Вызов вложенного обработчика
			LowLevelCommandCodeButton_Clicked (sender, null);
			}

		// Поиск по названию команды нижнего уровня
		private void Command_Find (object sender, EventArgs e)
			{
			List<string> codes = kb.LLCommands.GetCommandsList (ca.LowLevelProtocol);
			string text = commandSearchText.Text.ToLower ();

			lastCommandSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastCommandSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastCommandSearchOffset = (i + lastCommandSearchOffset) % codes.Count;

					lowLevelCommand.Text = codes[lastCommandSearchOffset];
					ca.LowLevelCode = (uint)lastCommandSearchOffset;

					lowLevelCommandCode.Text = kb.LLCommands.GetCommand (ca.LowLevelProtocol,
						(uint)lastCommandSearchOffset, false);
					lowLevelCommandDescr.Text = kb.LLCommands.GetCommand (ca.LowLevelProtocol,
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
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotAcceptableMessage);
				}
			else if (res.Contains (KKTSupport.FNLifeUnwelcomeSign))
				{
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Planned);
				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotRecommendedMessage);
				}
			else
				{
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
				fnLifeResultDate = res;
				fnLifeResult.Text += res;
				}

			if (kb.FNNumbers.IsFNKnown (fnLifeSerial.Text))
				{
				if (!kb.FNNumbers.IsFNAllowed (fnLifeSerial.Text))
					{
					fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);

					fnLifeResult.Text += FNSerial.FNIsNotAllowedMessage;
					fnLifeModelLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
					}
				else
					{
					fnLifeModelLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
					}
				}
			else
				{
				fnLifeModelLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
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

				KKTSerial.FFDSupportStatuses[] statuses = kb.KKTNumbers.GetFFDSupportStatus (rnmKKTSN.Text);
				rnmSupport105.BackgroundColor = StatusToColor (statuses[0]);
				rnmSupport11.BackgroundColor = StatusToColor (statuses[1]);
				rnmSupport12.BackgroundColor = StatusToColor (statuses[2]);
				}
			else
				{
				rnmKKTTypeLabel.Text = "";
				rnmSupport105.BackgroundColor = rnmSupport11.BackgroundColor = rnmSupport12.BackgroundColor =
					StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
				}

			// ИНН пользователя
			rnmINNCheckLabel.Text = kb.KKTNumbers.GetRegionName (rnmINN.Text);
			if (KKTSupport.CheckINN (rnmINN.Text) < 0)
				{
				rnmINNCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
				rnmINNCheckLabel.Text += " (неполный)";
				}
			else if (KKTSupport.CheckINN (rnmINN.Text) == 0)
				{
				rnmINNCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
				rnmINNCheckLabel.Text += " (ОК)";
				}
			else
				{
				rnmINNCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Planned);
				rnmINNCheckLabel.Text += " (возможно, некорректный)";
				}

			// РН
			if (rnmRNM.Text.Length < 10)
				{
				rnmRNMCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unknown);
				rnmRNMCheckLabel.Text = "(неполный)";
				}
			else if (KKTSupport.GetFullRNM (rnmINN.Text, rnmKKTSN.Text, rnmRNM.Text.Substring (0, 10)) == rnmRNM.Text)
				{
				rnmRNMCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
				rnmRNMCheckLabel.Text = "(OK)";
				}
			else
				{
				rnmRNMCheckLabel.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
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
				Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);
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
						ca.OFDINN = ofdINN.Text = s;

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
			await AndroidSupport.CallHelpMaterials (HelpMaterialsSets.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (HelpMaterialsSets.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Xamarin.Forms.Button b = (Xamarin.Forms.Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, ASButtonDefaultTypes.Increase))
					AndroidSupport.MasterFontSize += 0.5;
				else if (AndroidSupport.IsNameDefault (b.Text, ASButtonDefaultTypes.Decrease))
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
			int res = (int)ca.KKTForManuals;
			List<string> list = new List<string> (kb.UserGuides.GetKKTList ());

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
					Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			userManualsKKTButton.Text = list[res];
			ca.KKTForManuals = (uint)res;

			for (int i = 0; i < operationTextLabels.Count; i++)
				{
				bool state = ca.GetUserManualSectionState ((byte)i);

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

			if (ca.AllowExtendedFunctionsLevel2)
				{
				List<string> modes = new List<string> { "Для кассира", "Для сервис-инженера" };

				res = await AndroidSupport.ShowList ("Сохранить / распечатать руководство:",
					Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), modes);
				if (res < 0)
					return;
				}

			// Печать
			UserManualsFlags flags = UserManualFlags;
			if (res == 0)
				flags |= UserManualsFlags.GuideForCashier;

			string text = KKTSupport.BuildUserManual (kb.UserGuides, ca.KKTForManuals, ca, flags);
			KKTSupport.PrintManual (text, pm, res == 0);
			}

		// Смена состояния кнопки
		private void OperationTextButton_Clicked (object sender, EventArgs e)
			{
			byte idx = (byte)operationTextButtons.IndexOf ((Xamarin.Forms.Button)sender);
			bool state = !!operationTextButtons[idx].Text.Contains (operationButtonSignature);

			ca.SetUserManualSectionState (idx, state);
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
				}
			}

		#endregion

		#region Теги TLV

		// Поиск тега
		private void TLVFind_Clicked (object sender, EventArgs e)
			{
			if (kb.Tags.FindTag (tlvTag.Text))
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
				AndroidSupport.ShowBalloon (Localization.GetDefaultText
					(LzDefaultTextValues.Message_BrowserNotAvailable), true);
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
			int res = (int)ca.CableType;
			List<string> list = kb.Plugs.GetCablesNames ();

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите тип кабеля:",
					Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);

				// Установка типа кабеля
				if (res < 0)
					return;
				}

			// Установка полей
			cableTypeButton.Text = list[res];
			ca.CableType = (uint)res;

			cableLeftSideText.Text = "Со стороны " + kb.Plugs.GetCableConnector ((uint)res, false);
			cableLeftPinsText.Text = kb.Plugs.GetCableConnectorPins ((uint)res, false);

			cableRightSideText.Text = "Со стороны " + kb.Plugs.GetCableConnector ((uint)res, true);
			cableRightPinsText.Text = kb.Plugs.GetCableConnectorPins ((uint)res, true);

			cableDescriptionText.Text = kb.Plugs.GetCableConnectorDescription ((uint)res, false) +
				Localization.RNRN + kb.Plugs.GetCableConnectorDescription ((uint)res, true);
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
				ASButtonDefaultTypes.Increase);

			// Извлечение значения с защитой
			uint v = 0;
			try
				{
				v = uint.Parse (convNumberField.Text);
				}
			catch { }

			// Обновление и возврат
			if (plus)
				{
				if (v < DataConvertors.MaxValue)
					v++;
				else
					v = DataConvertors.MaxValue;
				}
			else
				{
				if (v > 0)
					v--;
				else
					v = 0;
				}

			convNumberField.Text = v.ToString ();
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
				ASButtonDefaultTypes.Increase);

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
				(SupportedEncodings)(ca.EncodingForConvertor % encodingModesCount),
				ca.EncodingForConvertor >= encodingModesCount);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			convHexField.Text = DataConvertors.ConvertTextToHex (convTextField.Text,
				(SupportedEncodings)(ca.EncodingForConvertor % encodingModesCount),
				ca.EncodingForConvertor >= encodingModesCount);
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
			int res = (int)ca.EncodingForConvertor;
			List<string> list = new List<string> (DataConvertors.AvailableEncodings);

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите кодировку:",
					Localization.GetDefaultText (LzDefaultTextValues.Button_Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			encodingButton.Text = list[res];
			ca.EncodingForConvertor = (uint)res;
			}

		#endregion
		}
	}
