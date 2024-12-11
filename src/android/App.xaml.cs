using Microsoft.Maui.Controls;
using System.ComponentModel;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает функционал приложения
	/// </summary>
	public partial class App: Application
		{
		#region Настройки стилей отображения

		private Color[][] uiColors = new Color[][] {
			// MasterBackColor, FieldBackColor
			new Color[] { Color.FromArgb ("#E8E8E8"), Color.FromArgb ("#E0E0E0") },	// 0. Headers
			new Color[] { Color.FromArgb ("#F4E8FF"), Color.FromArgb ("#ECD8FF") },	// 1. User guides
			new Color[] { Color.FromArgb ("#FFEEEE"), Color.FromArgb ("#FFD0D0") },	// 2. Errors
			new Color[] { Color.FromArgb ("#FFF0E0"), Color.FromArgb ("#FFE0C0") },	// 3. FN life
			new Color[] { Color.FromArgb ("#E0F0FF"), Color.FromArgb ("#C0E0FF") },	// 4. RNM
			new Color[] { Color.FromArgb ("#F4F4FF"), Color.FromArgb ("#D0D0FF") },	// 5. OFD
			new Color[] { Color.FromArgb ("#E0FFF0"), Color.FromArgb ("#C8FFE4") },	// 6. TLV
			new Color[] { Color.FromArgb ("#FFF0FF"), Color.FromArgb ("#FFC8FF") },	// 7. Low level
			new Color[] { Color.FromArgb ("#FFFFF0"), Color.FromArgb ("#FFFFD0") },	// 8. KKT codes
			new Color[] { Color.FromArgb ("#F8FFF0"), Color.FromArgb ("#F0FFE0") },	// 9. Connectors
			new Color[] { Color.FromArgb ("#FFF4E2"), Color.FromArgb ("#E8D9CF") },	// 10. Barcodes
			new Color[] { Color.FromArgb ("#E1FFF1"), Color.FromArgb ("#D7F4E7") },	// 11. Convertors HT
			new Color[] { Color.FromArgb ("#E5FFF5"), Color.FromArgb ("#DBF4EB") },	// 12. Convertors SN
			new Color[] { Color.FromArgb ("#E9FFF9"), Color.FromArgb ("#DFF4EF") },	// 13. Convertors UN
			new Color[] { Color.FromArgb ("#F0FFF0"), Color.FromArgb ("#D0FFD0") },	// 14. App about
			};
		private const int hdrPage = 0;
		private const int usgPage = 1;
		private const int errPage = 2;
		private const int fnlPage = 3;
		private const int rnmPage = 4;
		private const int ofdPage = 5;
		private const int tlvPage = 6;
		private const int llvPage = 7;
		private const int codPage = 8;
		private const int conPage = 9;
		private const int bcdPage = 10;
		private const int cvhPage = 11;
		private const int cvsPage = 12;
		private const int cvuPage = 13;
		private const int aabPage = 14;

		private const int cBack = 0;
		private const int cField = 1;

		private const string firstStartRegKey = "HelpShownAt";

		#endregion

		#region Переменные страниц

		private List<ContentPage> uiPages = new List<ContentPage> ();

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
		private List<Button> operationTextButtons = new List<Button> ();
		private const string operationButtonSignature = " (скрыто)";

		private Button kktCodesKKTButton, fnLifeDate, cableTypeButton, kktCodesCenterButton,
			errorsKKTButton, userManualsKKTButton, userManualsPrintButton,
			ofdNameButton, ofdDNSNameButton, ofdIPButton, ofdPortButton, ofdEmailButton, ofdSiteButton,
			ofdDNSNameMButton, ofdIPMButton, ofdPortMButton, ofdINN,
			lowLevelProtocol, lowLevelCommand, lowLevelCommandCode, rnmGenerate, convCodeSymbolField,
			encodingButton, fnLifeStatus;
		private List<Button> uiButtons = new List<Button> ();

		private Editor codesSourceText, errorSearchText, commandSearchText, ofdSearchText,
			fnLifeSerial, tlvTag, rnmKKTSN, rnmINN, rnmRNM, connSearchText,
			barcodeField, convNumberField, convCodeField, convHexField, convTextField;

		private Switch fnLife13, fnLifeGenericTax, fnLifeGoods, fnLifeSeason, fnLifeAgents,
			fnLifeExcise, fnLifeAutonomous, fnLifeFFD12, fnLifeGambling, fnLifePawn, fnLifeMarkGoods,
			extendedMode, moreThanOneItemPerDocument, productBaseContainsPrices, cashiersHavePasswords,
			baseContainsServices, documentsContainMarks, productBaseContainsSingleItem;

		private DatePicker fnLifeStartDate;

		private StackLayout userManualLayout, menuLayout;

		#endregion

		#region Основные переменные

		// Опорные классы
		private KnowledgeBase kb;

		// Поисковые состояния
		private int lastErrorSearchOffset = 0;
		private int lastCommandSearchOffset = 0;
		private int lastOFDSearchOffset = 0;
		private int lastConnSearchOffset = 0;

		/*// Дата срока жизни ФН (в чистом виде)
		private string fnLifeResultDate = "";*/

		// Сообщение о применимости модели ФН
		private string fnLifeMessage = "";

		// Число режимов преобразования
		private uint encodingModesCount;

		// Список ранее сохранённых подстановок для раздела кодов символов
		private List<string> kktCodesOftenTexts = new List<string> ();

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();
			RDAppStartupFlags flags = AndroidSupport.GetAppStartupFlags (RDAppStartupFlags.DisableXPUN);

			kb = new KnowledgeBase ();

			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			// Переход в статус запуска для отмены вызова из оповещения
			AndroidSupport.AppIsRunning = true;

			#region Общая конструкция страниц приложения

			MainPage = new MasterPage ();

			uiPages.Add (ApplyPageSettings (new HeadersPage (), "HeadersPage",
				"Разделы приложения", uiColors[hdrPage][0], false));
			menuLayout = (StackLayout)uiPages[hdrPage].FindByName ("MenuField");

			uiPages.Add (ApplyPageSettings (new UserManualsPage (), "UserManualsPage",
				"Инструкции по работе с ККТ", uiColors[usgPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ErrorsPage (), "ErrorsPage",
				"Коды ошибок ККТ", uiColors[errPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new FNLifePage (), "FNLifePage",
				"Срок жизни ФН", uiColors[fnlPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new RNMPage (), "RNMPage",
				"Заводской и рег. номер ККТ", uiColors[rnmPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new OFDPage (), "OFDPage",
				"Параметры ОФД", uiColors[ofdPage][cBack], true));

			uiPages.Add (ApplyPageSettings (new TagsPage (), "TagsPage",
				"TLV-теги", uiColors[tlvPage][cBack], true));
			uiButtons[tlvPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			uiPages.Add (ApplyPageSettings (new LowLevelPage (), "LowLevelPage",
				"Команды нижнего уровня", uiColors[llvPage][cBack], true));
			uiButtons[llvPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			uiPages.Add (ApplyPageSettings (new KKTCodesPage (), "KKTCodesPage",
				"Перевод текста в коды ККТ", uiColors[codPage][cBack], true));
			uiButtons[codPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 1

			uiPages.Add (ApplyPageSettings (new ConnectorsPage (), "ConnectorsPage",
				"Разъёмы", uiColors[conPage][cBack], true));
			uiButtons[conPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			uiPages.Add (ApplyPageSettings (new BarCodesPage (), "BarCodesPage",
				"Штрих-коды", uiColors[bcdPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsHTPage (), "ConvertorsHTPage",
				"Конвертор двоичных данных", uiColors[cvhPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsSNPage (), "ConvertorsSNPage",
				"Конвертор систем счисления", uiColors[cvsPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsUCPage (), "ConvertorsUCPage",
				"Конвертор символов Unicode", uiColors[cvuPage][cBack], true));

			// С отступом
			menuLayout.Children.Add (new Label ());
			uiPages.Add (ApplyPageSettings (new AboutPage (), "AboutPage",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				uiColors[aabPage][cBack], true));

			#endregion

			AndroidSupport.SetMasterPage (MainPage, uiPages[hdrPage], uiColors[hdrPage][cBack]);

			#region Страница «оглавления»

			AndroidSupport.ApplyLabelSettings (uiPages[hdrPage], "ExtendedModeLabel",
				"Режим сервис-инженера", RDLabelTypes.DefaultLeft);
			extendedMode = AndroidSupport.ApplySwitchSettings (uiPages[hdrPage], "ExtendedMode", false,
				uiColors[hdrPage][cField], ExtendedMode_Toggled, AppSettings.EnableExtendedMode);

			AndroidSupport.ApplyLabelSettings (uiPages[hdrPage], "FunctionsLabel",
				"Разделы приложения", RDLabelTypes.HeaderCenter);
			AndroidSupport.ApplyLabelSettings (uiPages[hdrPage], "SettingsLabel",
				"Настройки", RDLabelTypes.HeaderCenter);

			if (AppSettings.CurrentTab <= aabPage)
				AndroidSupport.SetCurrentPage (uiPages[(int)AppSettings.CurrentTab],
					uiColors[(int)AppSettings.CurrentTab][cBack]);

			#endregion

			#region Страница инструкций

			Label ut = AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "SelectionLabel", "Модель ККТ:",
				RDLabelTypes.HeaderLeft);
			userManualLayout = (StackLayout)uiPages[usgPage].FindByName ("UserManualLayout");
			int operationsCount = AppSettings.EnableExtendedMode ? UserManuals.OperationTypes.Length :
				UserManuals.OperationsForCashiers.Length;   // Уровень 1

			UserManualsSections sections = (UserManualsSections)AppSettings.UserManualSectionsState;
			for (int i = 0; i < operationsCount; i++)
				{
				bool sectionEnabled = sections.HasFlag ((UserManualsSections)(1u << i));

				Button bh = new Button ();
				bh.BackgroundColor = uiColors[usgPage][cBack];
				bh.Clicked += OperationTextButton_Clicked;
				bh.FontAttributes = FontAttributes.Bold;
				bh.FontSize = ut.FontSize;
				bh.HorizontalOptions = LayoutOptions.Center;
				bh.IsVisible = true;
				bh.Margin = ut.Margin;
				bh.Padding = new Thickness (6, 0);
				bh.Text = UserManuals.OperationTypes[i] +
					(sectionEnabled ? "" : operationButtonSignature);
				bh.TextColor = ut.TextColor;
				bh.HeightRequest = bh.FontSize * 1.5;

				Label lt = new Label ();
				lt.BackgroundColor = uiColors[usgPage][cField];
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

			userManualsKKTButton = AndroidSupport.ApplyButtonSettings (uiPages[usgPage], "KKTButton",
				"   ", uiColors[usgPage][cField], UserManualsKKTButton_Clicked, true);
			userManualsPrintButton = AndroidSupport.ApplyButtonSettings (uiPages[usgPage], "PrintButton",
				"В файл / на печать", uiColors[usgPage][cField], PrintManual_Clicked, false);
			userManualsPrintButton.FontSize -= 2.0;
			userManualsPrintButton.Margin = new Thickness (0);
			userManualsPrintButton.Padding = new Thickness (3, 0);
			userManualsPrintButton.HeightRequest = userManualsPrintButton.FontSize * 2.0;

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "HelpLabel",
				UserManuals.UserManualsTip + RDLocale.RN +
				"Нажатие на заголовки разделов позволяет добавить или скрыть их в видимой и печатной " +
				"версиях этой инструкции",
				RDLabelTypes.TipCenter);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "OneItemLabel",
				"В чеках бывает более одной позиции", RDLabelTypes.DefaultLeft);
			moreThanOneItemPerDocument = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "OneItemSwitch",
				false, uiColors[usgPage][cField], null, false);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "PricesLabel",
				"В номенклатуре есть цены", RDLabelTypes.DefaultLeft);
			productBaseContainsPrices = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "PricesSwitch",
				false, uiColors[usgPage][cField], null, false);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "PasswordsLabel",
				"Кассиры пользуются паролями", RDLabelTypes.DefaultLeft);
			cashiersHavePasswords = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "PasswordsSwitch",
				false, uiColors[usgPage][cField], null, false);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "ServicesLabel",
				"В номенклатуре содержатся услуги", RDLabelTypes.DefaultLeft);
			baseContainsServices = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "ServicesSwitch",
				false, uiColors[usgPage][cField], null, false);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "MarksLabel",
				"Среди товаров есть маркированные (ТМТ)", RDLabelTypes.DefaultLeft);
			documentsContainMarks = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "MarksSwitch",
				false, uiColors[usgPage][cField], null, false);

			AndroidSupport.ApplyLabelSettings (uiPages[usgPage], "SingleItemLabel",
				"Номенклатурная позиция – единственная", RDLabelTypes.DefaultLeft);
			productBaseContainsSingleItem = AndroidSupport.ApplySwitchSettings (uiPages[usgPage], "SingleItemSwitch",
				false, uiColors[usgPage][cField], null, false);

			UserManualFlags = (UserManualsFlags)AppSettings.UserManualFlags;

			moreThanOneItemPerDocument.Toggled += UserManualsFlags_Clicked;
			productBaseContainsPrices.Toggled += UserManualsFlags_Clicked;
			cashiersHavePasswords.Toggled += UserManualsFlags_Clicked;
			baseContainsServices.Toggled += UserManualsFlags_Clicked;
			documentsContainMarks.Toggled += UserManualsFlags_Clicked;
			productBaseContainsSingleItem.Toggled += UserManualsFlags_Clicked;

			UserManualsKKTButton_Clicked (null, null);

			#endregion

			#region Страница кодов символов ККТ

			AndroidSupport.ApplyLabelSettings (uiPages[codPage], "SelectionLabel", "Модель ККТ:",
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

			kktCodesKKTButton = AndroidSupport.ApplyButtonSettings (uiPages[codPage], "KKTButton",
				kktTypeName, uiColors[codPage][cField], CodesKKTButton_Clicked, true);
			AndroidSupport.ApplyLabelSettings (uiPages[codPage], "SourceTextLabel",
				"Исходный текст:", RDLabelTypes.HeaderLeft);

			codesSourceText = AndroidSupport.ApplyEditorSettings (uiPages[codPage], "SourceText",
				uiColors[codPage][cField], Keyboard.Default, 72, AppSettings.CodesText, SourceText_TextChanged, true);
			codesSourceText.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (uiPages[codPage], "ResultTextLabel", "Коды ККТ:",
				RDLabelTypes.HeaderLeft);

			kktCodesErrorLabel = AndroidSupport.ApplyLabelSettings (uiPages[codPage], "ErrorLabel",
				"Часть введённых символов не поддерживается данной ККТ", RDLabelTypes.ErrorTip);

			kktCodesResultText = AndroidSupport.ApplyLabelSettings (uiPages[codPage], "ResultText", " ",
				RDLabelTypes.FieldMonotype, uiColors[codPage][cField]);

			AndroidSupport.ApplyLabelSettings (uiPages[codPage], "HelpTextLabel", "Пояснения к вводу:",
				RDLabelTypes.HeaderLeft);
			kktCodesHelpLabel = AndroidSupport.ApplyLabelSettings (uiPages[codPage], "HelpText",
				kb.CodeTables.GetKKTTypeDescription (AppSettings.KKTForCodes), RDLabelTypes.Field,
				uiColors[codPage][cField]);

			AndroidSupport.ApplyButtonSettings (uiPages[codPage], "ClearText",
				RDDefaultButtons.Delete, uiColors[codPage][cField], CodesClear_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[codPage], "SelectSavedText",
				RDDefaultButtons.Select, uiColors[codPage][cField], CodesLineGet_Clicked);

			kktCodesLengthLabel = AndroidSupport.ApplyLabelSettings (uiPages[codPage], "LengthLabel",
				"Длина:", RDLabelTypes.HeaderLeft);
			kktCodesCenterButton = AndroidSupport.ApplyButtonSettings (uiPages[codPage], "CenterText",
				"Центрировать", uiColors[codPage][cField], TextToConvertCenter_Click, false);
			AndroidSupport.ApplyButtonSettings (uiPages[codPage], "RememberText",
				"Запомнить", uiColors[codPage][cField], TextToConvertRemember_Click, false);

			SourceText_TextChanged (null, null);    // Протягивание кодов

			#endregion

			#region Страница ошибок

			AndroidSupport.ApplyLabelSettings (uiPages[errPage], "SelectionLabel",
				"Модель ККТ:", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (uiPages[errPage], "SearchLabel",
				"Поиск ошибки по коду или фрагменту описания:", RDLabelTypes.HeaderLeft);

			try
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[(int)AppSettings.KKTForErrors];
				}
			catch
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[0];
				AppSettings.KKTForErrors = 0;
				}
			errorsKKTButton = AndroidSupport.ApplyButtonSettings (uiPages[errPage], "KKTButton",
				kktTypeName, uiColors[errPage][cField], ErrorsKKTButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[errPage], "ResultTextLabel",
				"Описание ошибки:", RDLabelTypes.HeaderLeft);

			errorsResultText = AndroidSupport.ApplyLabelSettings (uiPages[errPage], "ResultText",
				"", RDLabelTypes.Field, uiColors[errPage][cField]);
			errorSearchText = AndroidSupport.ApplyEditorSettings (uiPages[errPage], "ErrorSearchText",
				uiColors[errPage][cField], Keyboard.Default, 30, AppSettings.ErrorCode, null, true);
			Errors_Find (null, null);

			AndroidSupport.ApplyButtonSettings (uiPages[errPage], "ErrorSearchButton",
				RDDefaultButtons.Find, uiColors[errPage][cField], Errors_Find);
			AndroidSupport.ApplyButtonSettings (uiPages[errPage], "ErrorClearButton",
				RDDefaultButtons.Delete, uiColors[errPage][cField], Errors_Clear);

			#endregion

			#region Страница "О программе"

			AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (uiPages[aabPage], "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				uiColors[aabPage][cField], ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (uiPages[aabPage], "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				uiColors[aabPage][cField], HelpButton_Click, false);
			AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.TipCenter);

			AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (uiPages[aabPage], "FontSizeInc",
				RDDefaultButtons.Increase, uiColors[aabPage][cField], FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[aabPage], "FontSizeDec",
				RDDefaultButtons.Decrease, uiColors[aabPage][cField], FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);

			AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "HelpHeaderLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				RDLabelTypes.HeaderLeft);
			Label htl = AndroidSupport.ApplyLabelSettings (uiPages[aabPage], "HelpTextLabel",
				AndroidSupport.GetAppHelpText (), RDLabelTypes.SmallLeft);
			htl.TextType = TextType.Html;

			FontSizeButton_Clicked (null, null);

			#endregion

			#region Страница определения срока жизни ФН

			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "SetModelLabel",
				"ЗН, номинал или модель ФН:", RDLabelTypes.HeaderLeft);
			fnLifeSerial = AndroidSupport.ApplyEditorSettings (uiPages[fnlPage], "FNLifeSerial",
				uiColors[fnlPage][cField], Keyboard.Default, 16, AppSettings.FNSerial,
				FNLifeSerial_TextChanged, true);
			fnLifeSerial.Margin = new Thickness (0);

			fnLife13 = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLife13", true,
				uiColors[fnlPage][cField], FnLife13_Toggled, false);

			fnLifeLabel = AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeLabel",
				"", RDLabelTypes.DefaultLeft);

			//
			fnLifeModelLabel = AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeModelLabel",
				"", RDLabelTypes.Semaphore);

			//
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "SetUserParameters",
				"Значимые параметры:", RDLabelTypes.HeaderLeft);

			fnLifeGenericTax = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGenericTax", true,
				uiColors[fnlPage][cField], null, false);
			fnLifeGenericTaxLabel = AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGenericTaxLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeGoods = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGoods", true,
				uiColors[fnlPage][cField], null, false);
			fnLifeGoodsLabel = AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGoodsLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeSeason = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeSeason", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeSeasonLabel",
				"Сезонная торговля", RDLabelTypes.DefaultLeft);

			fnLifeAgents = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeAgents", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeAgentsLabel",
				"Платёжный (суб)агент", RDLabelTypes.DefaultLeft);

			fnLifeExcise = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeExcise", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeExciseLabel",
				"Подакцизные товары", RDLabelTypes.DefaultLeft);

			fnLifeAutonomous = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeAutonomous", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeAutonomousLabel",
				"Автономный режим", RDLabelTypes.DefaultLeft);

			fnLifeFFD12 = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeFFD12", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeFFD12Label",
				"ФФД 1.1, 1.2", RDLabelTypes.DefaultLeft);

			fnLifeGambling = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGambling", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGamblingLabel",
				"Азартные игры и лотереи", RDLabelTypes.DefaultLeft);

			fnLifePawn = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifePawn", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifePawnLabel",
				"Ломбарды и страхование", RDLabelTypes.DefaultLeft);

			fnLifeMarkGoods = AndroidSupport.ApplySwitchSettings (uiPages[fnlPage], "FNLifeMarkGoods", false,
				uiColors[fnlPage][cField], null, false);
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeMarkGoodsLabel",
				"Маркированные товары", RDLabelTypes.DefaultLeft);

			//
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "SetDate",
				"Дата фискализации:", RDLabelTypes.DefaultLeft);
			fnLifeStartDate = AndroidSupport.ApplyDatePickerSettings (uiPages[fnlPage], "FNLifeStartDate",
				uiColors[fnlPage][cField], FnLifeStartDate_DateSelected);

			//
			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeResultLabel",
				"Результат:", RDLabelTypes.HeaderLeft);

			fnLifeDate = AndroidSupport.ApplyButtonSettings (uiPages[fnlPage], "FNLifeDate", "",
				uiColors[fnlPage][cField], FNLifeDateCopy, true);
			fnLifeStatus = AndroidSupport.ApplyButtonSettings (uiPages[fnlPage], "FNLifeStatus", "?",
				uiColors[fnlPage][cField], FNLifeStatusClick, true);

			// Применение параметров, которые не могут быть рассчитаны при инициализации
			fnLifeStatus.WidthRequest = fnLifeStatus.HeightRequest =
				fnLifeDate.HeightRequest = 10 * AndroidSupport.MasterFontSize / 3;

			AndroidSupport.ApplyLabelSettings (uiPages[fnlPage], "FNLifeHelpLabel",
				"Нажатие кнопки копирует дату окончания срока жизни в буфер обмена",
				RDLabelTypes.TipCenter);

			AndroidSupport.ApplyButtonSettings (uiPages[fnlPage], "RegistryStats",
				"Статистика реестра ФН", uiColors[fnlPage][cField], FNStats_Clicked, false);

			//
			AndroidSupport.ApplyButtonSettings (uiPages[fnlPage], "Clear",
				RDDefaultButtons.Delete, uiColors[fnlPage][cField], FNLifeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[fnlPage], "Find",
				RDDefaultButtons.Find, uiColors[fnlPage][cField], FNLifeFind_Clicked);

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

			#region Страница заводских и регистрационных номеров

			AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "SNLabel",
				"ЗН или" + RDLocale.RN + "модель ККТ:", RDLabelTypes.HeaderLeft);
			rnmKKTSN = AndroidSupport.ApplyEditorSettings (uiPages[rnmPage], "SN",
				uiColors[rnmPage][cField], Keyboard.Default, kb.KKTNumbers.MaxSerialNumberLength,
				AppSettings.KKTSerial, RNM_TextChanged, true);

			rnmKKTTypeLabel = AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "TypeLabel",
				"", RDLabelTypes.Semaphore);

			AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "INNLabel",
				"ИНН пользователя:", RDLabelTypes.HeaderLeft);
			rnmINN = AndroidSupport.ApplyEditorSettings (uiPages[rnmPage], "INN",
				uiColors[rnmPage][cField], Keyboard.Numeric, 12, AppSettings.UserINN,
				RNM_TextChanged, true);

			rnmINNCheckLabel = AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "INNCheckLabel", "",
				RDLabelTypes.Semaphore);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "RNMLabel",
					"Регистрационный номер для проверки или произвольное число для генерации:",
					RDLabelTypes.HeaderLeft);
			else
				AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "RNMLabel",
					"Регистрационный номер для проверки:", RDLabelTypes.HeaderLeft);

			rnmRNM = AndroidSupport.ApplyEditorSettings (uiPages[rnmPage], "RNM",
				uiColors[rnmPage][cField], Keyboard.Numeric, 16, AppSettings.RNMKKT,
				RNM_TextChanged, true);

			rnmRNMCheckLabel = AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "RNMCheckLabel", "",
				RDLabelTypes.Semaphore);

			rnmGenerate = AndroidSupport.ApplyButtonSettings (uiPages[rnmPage], "RNMGenerate",
				RDDefaultButtons.Create, uiColors[rnmPage][cField], RNMGenerate_Clicked);
			rnmGenerate.IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			AndroidSupport.ApplyButtonSettings (uiPages[rnmPage], "RegistryStats",
				"Статистика реестра ККТ", uiColors[rnmPage][cField], RNMStats_Clicked, false);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				AndroidSupport.ApplyLabelSettings (uiPages[rnmPage], "RNMAbout",
					"Первые 10 цифр являются порядковым номером ККТ в реестре. При генерации " +
					"РНМ их можно указать вручную – остальные будут достроены программой",
					RDLabelTypes.TipLeft);

			AndroidSupport.ApplyButtonSettings (uiPages[rnmPage], "Clear",
				RDDefaultButtons.Delete, uiColors[rnmPage][cField], RNMClear_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[rnmPage], "Find",
				RDDefaultButtons.Find, uiColors[rnmPage][cField], RNMFind_Clicked);

			RNM_TextChanged (null, null);   // Применение значений

			#endregion

			#region Страница настроек ОФД

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDINNLabel",
				"ИНН ОФД:", RDLabelTypes.HeaderLeft);
			ofdINN = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDINN",
				AppSettings.OFDINN, uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDNameLabel",
				"Название:", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDSearchLabel",
				"Поиск по ИНН или фрагменту названия:", RDLabelTypes.HeaderLeft);
			ofdNameButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDName",
				"- Выберите или введите ИНН -", uiColors[ofdPage][cField], OFDName_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameLabel",
				"Адрес ОФД:", RDLabelTypes.HeaderLeft);
			ofdDNSNameButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSName",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdIPButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDIP",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdPortButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDPort",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameMLabel",
				"Адрес ИСМ:", RDLabelTypes.HeaderLeft);
			ofdDNSNameMButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSNameM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdIPMButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDIPM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdPortMButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDPortM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameKLabel",
				"Адрес ОКП (стандартный):", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSNameK", OFD.OKPSite,
				uiColors[ofdPage][cField], Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDIPK", OFD.OKPIP,
				uiColors[ofdPage][cField], Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDPortK", OFD.OKPPort,
				uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDEmailLabel",
				"E-mail отправителя чеков:", RDLabelTypes.HeaderLeft);
			ofdEmailButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDEmail",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDSiteLabel",
				"Сайт ОФД:", RDLabelTypes.HeaderLeft);
			ofdSiteButton = AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDSite",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDNalogSiteLabel",
				"Сайт ФНС:", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDNalogSite", OFD.FNSSite,
				uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "CDNSiteLabel",
				"CDN-площадка ЦРПТ:", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "CDNSite", OFD.CDNSite,
				uiColors[ofdPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDHelpLabel",
				"Нажатие кнопок копирует их подписи в буфер обмена",
				RDLabelTypes.TipCenter);

			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "Clear",
				RDDefaultButtons.Delete, uiColors[ofdPage][cField], OFDClear_Clicked);

			ofdSearchText = AndroidSupport.ApplyEditorSettings (uiPages[ofdPage], "OFDSearchText",
				uiColors[ofdPage][cField], Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (uiPages[ofdPage], "OFDSearchButton",
				RDDefaultButtons.Find, uiColors[ofdPage][cField], OFD_Find);

			ofdDisabledLabel = AndroidSupport.ApplyLabelSettings (uiPages[ofdPage], "OFDDisabledLabel",
				"Аннулирован", RDLabelTypes.ErrorTip);
			ofdDisabledLabel.IsVisible = false;

			OFDINN_TextChanged (null, null); // Протягивание значений

			#endregion

			#region Страница TLV-тегов

			AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVSearchLabel",
				"Поиск по номеру или фрагменту описания:", RDLabelTypes.HeaderLeft);
			tlvTag = AndroidSupport.ApplyEditorSettings (uiPages[tlvPage], "TLVSearchText",
				uiColors[tlvPage][cField], Keyboard.Default, 20, AppSettings.TLVData, null, true);

			AndroidSupport.ApplyButtonSettings (uiPages[tlvPage], "TLVSearchButton",
				RDDefaultButtons.Find, uiColors[tlvPage][cField], TLVFind_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[tlvPage], "TLVClearButton",
				RDDefaultButtons.Delete, uiColors[tlvPage][cField], TLVClear_Clicked);

			AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVDescriptionLabel",
				"Описание тега:", RDLabelTypes.HeaderLeft);
			tlvDescriptionLabel = AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVDescription",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVTypeLabel", "Тип тега:",
				RDLabelTypes.HeaderLeft);
			tlvTypeLabel = AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVType",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVValuesLabel",
				"Возможные значения тега:", RDLabelTypes.HeaderLeft);
			tlvValuesLabel = AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVValues",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVObligationLabel",
				"Обязательность:", RDLabelTypes.HeaderLeft);
			tlvObligationLabel = AndroidSupport.ApplyLabelSettings (uiPages[tlvPage], "TLVObligation",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);
			tlvObligationLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (uiPages[tlvPage], "TLVObligationHelpLabel",
				TLVTags.ObligationBase, uiColors[tlvPage][cField], TLVObligationBase_Click, false);

			TLVFind_Clicked (null, null);

			#endregion

			#region Страница команд нижнего уровня

			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "ProtocolLabel",
				"Протокол:", RDLabelTypes.HeaderLeft);
			lowLevelProtocol = AndroidSupport.ApplyButtonSettings (uiPages[llvPage], "ProtocolButton",
				kb.LLCommands.GetProtocolsNames ()[(int)AppSettings.LowLevelProtocol],
				uiColors[llvPage][cField], LowLevelProtocol_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "CommandLabel",
				"Команда:", RDLabelTypes.HeaderLeft);
			lowLevelCommand = AndroidSupport.ApplyButtonSettings (uiPages[llvPage], "CommandButton",
				kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol)[(int)AppSettings.LowLevelCode],
				uiColors[llvPage][cField], LowLevelCommandCodeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "CommandSearchLabel",
				"Поиск по описанию команды:", RDLabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "CommandCodeLabel",
				"Код команды:", RDLabelTypes.HeaderLeft);
			lowLevelCommandCode = AndroidSupport.ApplyButtonSettings (uiPages[llvPage], "CommandCodeButton",
				kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, false),
				uiColors[llvPage][cField], Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "CommandDescrLabel",
				"Пояснение:", RDLabelTypes.HeaderLeft);

			lowLevelCommandDescr = AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "CommandDescr",
				kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, true),
				RDLabelTypes.Field, uiColors[llvPage][cField]);

			AndroidSupport.ApplyLabelSettings (uiPages[llvPage], "LowLevelHelpLabel",
				"Нажатие кнопки копирует команду в буфер обмена",
				RDLabelTypes.TipCenter);

			commandSearchText = AndroidSupport.ApplyEditorSettings (uiPages[llvPage], "CommandSearchText",
				uiColors[llvPage][cField], Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (uiPages[llvPage], "CommandSearchButton",
				RDDefaultButtons.Find, uiColors[llvPage][cField], Command_Find);
			AndroidSupport.ApplyButtonSettings (uiPages[llvPage], "CommandClearButton",
				RDDefaultButtons.Delete, uiColors[llvPage][cField], Command_Clear);

			#endregion

			#region Страница штрих-кодов

			AndroidSupport.ApplyLabelSettings (uiPages[bcdPage], "BarcodeFieldLabel",
				"Данные штрих-кода:", RDLabelTypes.HeaderLeft);
			barcodeField = AndroidSupport.ApplyEditorSettings (uiPages[bcdPage], "BarcodeField",
				uiColors[bcdPage][cField], Keyboard.Default, BarCodes.MaxSupportedDataLength,
				AppSettings.BarcodeData, BarcodeText_TextChanged, true);

			AndroidSupport.ApplyLabelSettings (uiPages[bcdPage], "BarcodeDescriptionLabel",
				"Описание штрих-кода:", RDLabelTypes.HeaderLeft);
			barcodeDescriptionLabel = AndroidSupport.ApplyLabelSettings (uiPages[bcdPage], "BarcodeDescription",
				"", RDLabelTypes.Field, uiColors[bcdPage][cField]);

			AndroidSupport.ApplyButtonSettings (uiPages[bcdPage], "Clear",
				RDDefaultButtons.Delete, uiColors[bcdPage][cField], BarcodeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (uiPages[bcdPage], "GetFromClipboard",
				RDDefaultButtons.Copy, uiColors[bcdPage][cField], BarcodeGet_Clicked);

			BarcodeText_TextChanged (null, null);

			#endregion

			#region Страница распиновок

			AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableLabel",
				"Тип кабеля:", RDLabelTypes.HeaderLeft);
			cableTypeButton = AndroidSupport.ApplyButtonSettings (uiPages[conPage], "CableTypeButton",
				kb.Plugs.GetCablesNames ()[(int)AppSettings.CableType], uiColors[conPage][cField],
				CableTypeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableDescriptionLabel",
				"Сопоставление контактов:", RDLabelTypes.HeaderLeft);
			cableLeftSideText = AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableLeftSide",
				" ", RDLabelTypes.DefaultLeft);
			cableLeftSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableLeftSideText.HorizontalOptions = LayoutOptions.Center;

			cableLeftPinsText = AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableLeftPins",
				" ", RDLabelTypes.FieldMonotype, uiColors[conPage][cField]);

			cableRightSideText = AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableRightSide",
				" ", RDLabelTypes.DefaultLeft);
			cableRightSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableRightSideText.HorizontalOptions = LayoutOptions.Center;

			cableRightPinsText = AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableRightPins",
				" ", RDLabelTypes.FieldMonotype, uiColors[conPage][cField]);
			cableLeftPinsText.HorizontalTextAlignment = cableRightPinsText.HorizontalTextAlignment =
				TextAlignment.Center;

			cableDescriptionText = AndroidSupport.ApplyLabelSettings (uiPages[conPage], "CableDescription",
				" ", RDLabelTypes.TipLeft);

			connSearchText = AndroidSupport.ApplyEditorSettings (uiPages[conPage], "ConnSearchText",
				uiColors[conPage][cField], Keyboard.Default, 30, "", null, true);
			AndroidSupport.ApplyButtonSettings (uiPages[conPage], "ConnSearchButton",
				RDDefaultButtons.Find, uiColors[conPage][cField], Conn_Find);
			AndroidSupport.ApplyButtonSettings (uiPages[conPage], "ConnClearButton",
				RDDefaultButtons.Delete, uiColors[conPage][cField], Conn_Clear);

			CableTypeButton_Clicked (null, null);

			#endregion

			#region Страница конвертора систем счисления

			AndroidSupport.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberLabel",
				"Число:", RDLabelTypes.HeaderLeft);
			convNumberField = AndroidSupport.ApplyEditorSettings (uiPages[cvsPage], "ConvNumberField",
				uiColors[cvsPage][cField], Keyboard.Default, 10, AppSettings.ConversionNumber,
				ConvNumber_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberInc",
				RDDefaultButtons.Increase, uiColors[cvsPage][cField], ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberDec",
				RDDefaultButtons.Decrease, uiColors[cvsPage][cField], ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberClear",
				RDDefaultButtons.Delete, uiColors[cvsPage][cField], ConvNumberClear_Click);

			AndroidSupport.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberResultLabel",
				"Представление:", RDLabelTypes.HeaderLeft);
			convNumberResultField = AndroidSupport.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberResultField",
				" ", RDLabelTypes.FieldMonotype, uiColors[cvsPage][cField]);
			ConvNumber_TextChanged (null, null);

			const string convHelp = "Шестнадцатеричные числа следует" + RDLocale.RN + "начинать с символов “0x”";
			AndroidSupport.ApplyLabelSettings (uiPages[cvsPage], "ConvHelpLabel", convHelp, RDLabelTypes.TipCenter);

			#endregion

			#region Страница конвертора символов Unicode

			encodingModesCount = (uint)(DataConvertors.AvailableEncodings.Length / 2);

			AndroidSupport.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeLabel",
				"Код символа" + RDLocale.RN + "или символ:", RDLabelTypes.HeaderLeft);
			convCodeField = AndroidSupport.ApplyEditorSettings (uiPages[cvuPage], "ConvCodeField",
				uiColors[cvuPage][cField], Keyboard.Default, 10, AppSettings.ConversionCode,
				ConvCode_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeInc",
				RDDefaultButtons.Increase, uiColors[cvuPage][cField], ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeDec",
				RDDefaultButtons.Decrease, uiColors[cvuPage][cField], ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeClear",
				RDDefaultButtons.Delete, uiColors[cvuPage][cField], ConvCodeClear_Click);

			AndroidSupport.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeResultLabel",
				"Символ Unicode:", RDLabelTypes.HeaderLeft);
			convCodeResultField = AndroidSupport.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeResultField",
				"", RDLabelTypes.FieldMonotype, uiColors[cvuPage][cField]);

			convCodeSymbolField = AndroidSupport.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeSymbolField",
				" ", uiColors[cvuPage][cField], CopyCharacter_Click, true);
			convCodeSymbolField.FontSize *= 5;
			ConvCode_TextChanged (null, null);

			AndroidSupport.ApplyLabelSettings (uiPages[cvuPage], "ConvHelpLabel",
				convHelp + "." + RDLocale.RN + "Нажатие кнопки с символом Unicode" + RDLocale.RN +
				"копирует его в буфер обмена", RDLabelTypes.TipCenter);

			#endregion

			#region Страница конвертора двоичных данных

			AndroidSupport.ApplyLabelSettings (uiPages[cvhPage], "HexLabel",
				"Данные (hex или BASE64):", RDLabelTypes.HeaderLeft);
			convHexField = AndroidSupport.ApplyEditorSettings (uiPages[cvhPage], "HexField",
				uiColors[cvhPage][cField], Keyboard.Default, 500, AppSettings.ConversionHex, null, true);
			convHexField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyButtonSettings (uiPages[cvhPage], "HexToTextButton",
				RDDefaultButtons.Down, uiColors[cvhPage][cField], ConvertHexToText_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvhPage], "TextToHexButton",
				RDDefaultButtons.Up, uiColors[cvhPage][cField], ConvertTextToHex_Click);
			AndroidSupport.ApplyButtonSettings (uiPages[cvhPage], "ClearButton",
				RDDefaultButtons.Delete, uiColors[cvhPage][cField], ClearConvertText_Click);

			AndroidSupport.ApplyLabelSettings (uiPages[cvhPage], "TextLabel",
				"Данные (текст):", RDLabelTypes.HeaderLeft);
			convTextField = AndroidSupport.ApplyEditorSettings (uiPages[cvhPage], "TextField",
				uiColors[cvhPage][cField], Keyboard.Default, 250, AppSettings.ConversionText, null, true);
			convTextField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (uiPages[cvhPage], "EncodingLabel",
				"Кодировка:", RDLabelTypes.HeaderLeft);
			encodingButton = AndroidSupport.ApplyButtonSettings (uiPages[cvhPage], "EncodingButton",
				" ", uiColors[cvhPage][cField], EncodingButton_Clicked, true);
			EncodingButton_Clicked (null, null);

			#endregion

			// Обязательное принятие Политики и EULA
			AcceptPolicy (flags.HasFlag (RDAppStartupFlags.DisableXPUN));
			}

		// Локальный оформитель страниц приложения
		private ContentPage ApplyPageSettings (ContentPage CreatedPage, string PageName, string PageTitle,
			Color PageBackColor, bool AddToMenu)
			{
			// Инициализация страницы
			ContentPage page = AndroidSupport.ApplyPageSettings (CreatedPage, PageName, PageTitle, PageBackColor);

			// Добавление в содержание
			if (!AddToMenu)
				return page;

			Button b = new Button ();
			b.Text = PageTitle;
			b.BackgroundColor = PageBackColor;
			b.Clicked += HeaderButton_Clicked;
			b.TextTransform = TextTransform.None;

			b.FontAttributes = FontAttributes.None;
			b.FontSize = 5 * AndroidSupport.MasterFontSize / 4;
			b.TextColor = AndroidSupport.MasterTextColor;
			b.Margin = b.Padding = new Thickness (1);

			uiButtons.Add (b);
			menuLayout.Children.Add (b);

			return page;
			}

		// Контроль принятия Политики и EULA
		private async void AcceptPolicy (bool DisableXPUN)
			{
			// Контроль XPUN
			if (!DisableXPUN)
				await AndroidSupport.XPUNLoop ();

			// Политика
			if (RDGenerics.GetAppRegistryValue (firstStartRegKey) != "")
				return;

			await AndroidSupport.PolicyLoop ();

			// Только после принятия
			RDGenerics.SetAppRegistryValue (firstStartRegKey, ProgramDescription.AssemblyVersion);

			await AndroidSupport.ShowMessage ("Вас приветствует " + ProgramDescription.AssemblyMainName +
				" – " + ProgramDescription.AssemblyDescription + RDLocale.RNRN +
				"На этой странице находится перечень функций приложения, который позволяет перейти " +
				"к нужному разделу. Вернуться сюда можно с помощью кнопки «Назад»",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		// Отправка значения кнопки в буфер
		private void Field_Clicked (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (((Button)sender).Text, true);
			}

		// Выбор элемента содержания
		private void HeaderButton_Clicked (object sender, EventArgs e)
			{
			int idx = uiButtons.IndexOf ((Button)sender);
			if (idx >= 0)
				AndroidSupport.SetCurrentPage (uiPages[idx + 1], uiColors[idx + 1][0]);
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
			AndroidSupport.AppIsRunning = false;

			// Сохранение настроек
			AppSettings.CurrentTab = (uint)uiPages.IndexOf ((ContentPage)AndroidSupport.MasterPage.CurrentPage);

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

					AndroidSupport.HideKeyboard (errorSearchText);
					return;
					}
				}

			// Код не найден
			errorsResultText.Text = "(описание ошибки не найдено)";
			}

		// Очистка полей
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
			kktCodesCenterButton.IsVisible = (codesSourceText.Text.Length > 0) &&
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
				{
				if (codes[(i + lastCommandSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastCommandSearchOffset = (i + lastCommandSearchOffset) % codes.Count;

					lowLevelCommand.Text = codes[lastCommandSearchOffset];
					AppSettings.LowLevelCode = (uint)lastCommandSearchOffset;

					lowLevelCommandCode.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol,
						(uint)lastCommandSearchOffset, false);
					lowLevelCommandDescr.Text = kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol,
						(uint)lastCommandSearchOffset, true);

					AndroidSupport.HideKeyboard (commandSearchText);
					return;
					}
				}
			}

		// Очистка полей
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
		private void FnLifeStartDate_DateSelected (object sender, PropertyChangedEventArgs e)
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
			FNLifeResult res = KKTSupport.GetFNLifeEndDate (fnLifeStartDate.Date, FNLifeEvFlags);

			/*fnLifeResult.Text = "ФН прекратит работу ";
			if (res.Contains (KKTSupport.FNLifeInacceptableSign))*/
			fnLifeDate.Text = res.DeadLine;
			switch (res.Status)
				{
				case FNLifeStatus.Inacceptable:
					/*fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);*/
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);

					/*fnLifeResultDate = res.Substring (1);
					fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotAcceptableMessage);*/
					fnLifeMessage = FNSerial.FNIsNotAcceptableMessage;
					break;

				case FNLifeStatus.Unwelcome:
					/*else if (res.Contains (KKTSupport.FNLifeUnwelcomeSign))
					fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningMessage);
					fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningText);*/
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.WarningText);

					/*fnLifeResultDate = res.Substring (1);
					fnLifeResult.Text += (fnLifeResultDate + FNSerial.FNIsNotRecommendedMessage);*/
					fnLifeMessage = FNSerial.FNIsNotRecommendedMessage;
					break;

				case FNLifeStatus.Acceptable:
				default:
					/*else
					fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);*/
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.SuccessText);

					/*fnLifeResultDate = res;
					fnLifeResult.Text += res;*/
					fnLifeMessage = FNSerial.FNIsAcceptableMessage;
					break;

				case FNLifeStatus.StronglyUnwelcome:
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.QuestionText);

					fnLifeMessage = FNSerial.FNIsStronglyUnwelcomeMessage;
					break;
				}

			if (kb.FNNumbers.IsFNKnown (fnLifeSerial.Text))
				{
				if (!kb.FNNumbers.IsFNAllowed (fnLifeSerial.Text))
					{
					/*fnLifeResult.BackgroundColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeResult.TextColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);*/
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDGenerics.GetInterfaceColor (RDInterfaceColors.ErrorText);

					/*fnLifeResult.Text += FNSerial.FNIsNotAllowedMessage;*/
					fnLifeMessage += (RDLocale.RN + FNSerial.FNIsNotAllowedMessage);

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
		private void FNLifeDateCopy (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (fnLifeDate.Text, true);
			}

		// Сообщение о применимости модели ФН
		private async void FNLifeStatusClick (object sender, EventArgs e)
			{
			await AndroidSupport.ShowMessage (fnLifeMessage, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
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
				{
				fnLifeSerial.Text = sig;
				AndroidSupport.HideKeyboard (fnLifeSerial);
				}
			}

		// Статистика по базе ЗН ФН
		private async void FNStats_Clicked (object sender, EventArgs e)
			{
			await AndroidSupport.ShowMessage (kb.FNNumbers.RegistryStats,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для расчёта срока жизни ФН
		/// </summary>
		private FNLifeFlags FNLifeEvFlags
			{
			get
				{
				FNLifeFlags flags = 0;

				if (!fnLife13.IsToggled)
					flags |= FNLifeFlags.FN15;
				if (kb.FNNumbers.IsFNExactly13 (fnLifeSerial.Text))
					flags |= FNLifeFlags.FNExactly13;
				if (!fnLifeGenericTax.IsToggled)
					flags |= FNLifeFlags.GenericTax;
				if (!fnLifeGoods.IsToggled)
					flags |= FNLifeFlags.Goods;
				if (fnLifeSeason.IsToggled)
					flags |= FNLifeFlags.Season;
				if (fnLifeAgents.IsToggled)
					flags |= FNLifeFlags.Agents;
				if (fnLifeExcise.IsToggled)
					flags |= FNLifeFlags.Excise;
				if (fnLifeAutonomous.IsToggled)
					flags |= FNLifeFlags.Autonomous;
				if (fnLifeFFD12.IsToggled)
					flags |= FNLifeFlags.FFD12;
				if (fnLifeGambling.IsToggled)
					flags |= FNLifeFlags.GamblingAndLotteries;
				if (fnLifePawn.IsToggled)
					flags |= FNLifeFlags.PawnsAndInsurance;
				if (fnLifeMarkGoods.IsToggled)
					flags |= FNLifeFlags.MarkGoods;

				if (!kb.FNNumbers.IsFNKnown (fnLifeSerial.Text) ||
					kb.FNNumbers.IsFNCompatibleWithFFD12 (fnLifeSerial.Text))
					flags |= FNLifeFlags.MarkFN;

				return flags;
				}
			set
				{
				fnLife13.IsToggled = !value.HasFlag (FNLifeFlags.FN15);
				// .Exactly13 – вспомогательный нехранимый флаг
				fnLifeGenericTax.IsToggled = !value.HasFlag (FNLifeFlags.GenericTax);
				fnLifeGoods.IsToggled = !value.HasFlag (FNLifeFlags.Goods);
				fnLifeSeason.IsToggled = value.HasFlag (FNLifeFlags.Season);
				fnLifeAgents.IsToggled = value.HasFlag (FNLifeFlags.Agents);
				fnLifeExcise.IsToggled = value.HasFlag (FNLifeFlags.Excise);
				fnLifeAutonomous.IsToggled = value.HasFlag (FNLifeFlags.Autonomous);
				fnLifeFFD12.IsToggled = value.HasFlag (FNLifeFlags.FFD12);
				fnLifeGambling.IsToggled = value.HasFlag (FNLifeFlags.GamblingAndLotteries);
				fnLifePawn.IsToggled = value.HasFlag (FNLifeFlags.PawnsAndInsurance);
				fnLifeMarkGoods.IsToggled = value.HasFlag (FNLifeFlags.MarkGoods);
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
				{
				rnmKKTSN.Text = sig;
				AndroidSupport.HideKeyboard (rnmKKTSN);
				}
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
			List<string> list = kb.Ofd.GetOFDNames (false);

			// Установка результата
			int res = await AndroidSupport.ShowList ("Выберите название ОФД:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			ofdNameButton.Text = list[res];
			RDGenerics.SendToClipboard (list[res].Replace ('«', '\"').Replace ('»', '\"'), true);

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
			List<string> codes = kb.Ofd.GetOFDNames (false);
			codes.AddRange (kb.Ofd.GetOFDINNs ());

			string text = ofdSearchText.Text.ToLower ();

			lastOFDSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				if (codes[(i + lastOFDSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastOFDSearchOffset = (i + lastOFDSearchOffset) % codes.Count;
					ofdNameButton.Text = codes[lastOFDSearchOffset % (codes.Count / 2)];

					string s = kb.Ofd.GetOFDINNByName (ofdNameButton.Text);
					if (s != "")
						AppSettings.OFDINN = ofdINN.Text = s;

					OFDINN_TextChanged (null, null);
					AndroidSupport.HideKeyboard (ofdSearchText);
					return;
					}
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
				Button b = (Button)sender;
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
			KKTSupport.PrintManual (text, res == 0);
			}

		// Смена состояния кнопки
		private void OperationTextButton_Clicked (object sender, EventArgs e)
			{
			byte idx = (byte)operationTextButtons.IndexOf ((Button)sender);
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
				UserManualsFlags flags = 0;

				if (moreThanOneItemPerDocument.IsToggled)
					flags |= UserManualsFlags.MoreThanOneItemPerDocument;
				if (productBaseContainsPrices.IsToggled)
					flags |= UserManualsFlags.ProductBaseContainsPrices;
				if (cashiersHavePasswords.IsToggled)
					flags |= UserManualsFlags.CashiersHavePasswords;
				if (baseContainsServices.IsToggled)
					flags |= UserManualsFlags.ProductBaseContainsServices;
				if (documentsContainMarks.IsToggled)
					flags |= UserManualsFlags.DocumentsContainMarks;
				if (productBaseContainsSingleItem.IsToggled)
					flags |= UserManualsFlags.BaseContainsSingleItem;

				return flags;
				}
			set
				{
				moreThanOneItemPerDocument.IsToggled = value.HasFlag (UserManualsFlags.MoreThanOneItemPerDocument);
				productBaseContainsPrices.IsToggled = value.HasFlag (UserManualsFlags.ProductBaseContainsPrices);
				cashiersHavePasswords.IsToggled = value.HasFlag (UserManualsFlags.CashiersHavePasswords);
				baseContainsServices.IsToggled = value.HasFlag (UserManualsFlags.ProductBaseContainsServices);
				documentsContainMarks.IsToggled = value.HasFlag (UserManualsFlags.DocumentsContainMarks);
				productBaseContainsSingleItem.IsToggled = value.HasFlag (UserManualsFlags.BaseContainsSingleItem);
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

				AndroidSupport.HideKeyboard (tlvTag);   // Мешает просмотру текста
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
					AndroidSupport.HideKeyboard (connSearchText);
					return;
					}
				}

			// Код не найден
			cableLeftSideText.Text = "(описание не найдено)";
			cableLeftPinsText.Text = cableRightSideText.Text = cableRightPinsText.Text =
				cableDescriptionText.Text = "";
			}

		// Очистка полей
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
			bool plus = AndroidSupport.IsNameDefault (((Button)sender).Text,
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
			AndroidSupport.HideKeyboard (convNumberField);
			}

		private void ConvCode_TextChanged (object sender, EventArgs e)
			{
			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text, 0);
			convCodeSymbolField.Text = " " + res[0] + " ";
			convCodeResultField.Text = res[1];
			}

		private void ConvCodeAdd_Click (object sender, EventArgs e)
			{
			bool plus = AndroidSupport.IsNameDefault (((Button)sender).Text,
				RDDefaultButtons.Increase);

			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text, (short)(plus ? 1 : -1));
			convCodeField.Text = res[2];
			AndroidSupport.HideKeyboard (convCodeField);
			}

		// Копирование символа в буфер
		private void CopyCharacter_Click (object sender, EventArgs e)
			{
			RDGenerics.SendToClipboard (convCodeSymbolField.Text.Trim (), true);
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
			AndroidSupport.HideKeyboard (convTextField);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			convHexField.Text = DataConvertors.ConvertTextToHex (convTextField.Text,
				(RDEncodings)(AppSettings.EncodingForConvertor % encodingModesCount),
				AppSettings.EncodingForConvertor >= encodingModesCount);
			AndroidSupport.HideKeyboard (convHexField);
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
