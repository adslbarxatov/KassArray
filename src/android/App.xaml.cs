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

		private Color[][] uiColors = [
			// MasterBackColor, FieldBackColor
			[ Color.FromArgb ("#E8E8E8"), Color.FromArgb ("#E0E0E0") ],	// 0. Headers
			[ Color.FromArgb ("#F4E8FF"), Color.FromArgb ("#ECD8FF") ],	// 1. User guides
			[ Color.FromArgb ("#FFEEEE"), Color.FromArgb ("#FFD0D0") ],	// 2. Errors
			[ Color.FromArgb ("#FFF0E0"), Color.FromArgb ("#FFE0C0") ],	// 3. FN life
			[ Color.FromArgb ("#E0F0FF"), Color.FromArgb ("#C0E0FF") ],	// 4. RNM
			[ Color.FromArgb ("#F4F4FF"), Color.FromArgb ("#D0D0FF") ],	// 5. OFD
			[ Color.FromArgb ("#E0FFF0"), Color.FromArgb ("#C8FFE4") ],	// 6. TLV
			[ Color.FromArgb ("#FFF0FF"), Color.FromArgb ("#FFC8FF") ],	// 7. Terms dictionary
			[ Color.FromArgb ("#FFFFF0"), Color.FromArgb ("#FFFFD0") ],	// 8. KKT codes
			[ Color.FromArgb ("#F8FFF0"), Color.FromArgb ("#F0FFE0") ],	// 9. Connectors
			[ Color.FromArgb ("#FFF4E2"), Color.FromArgb ("#E8D9CF") ],	// 10. Barcodes
			[ Color.FromArgb ("#E1FFF1"), Color.FromArgb ("#D7F4E7") ],	// 11. Convertors HT
			[ Color.FromArgb ("#E5FFF5"), Color.FromArgb ("#DBF4EB") ],	// 12. Convertors SN
			[ Color.FromArgb ("#E9FFF9"), Color.FromArgb ("#DFF4EF") ],	// 13. Convertors UN
			[ Color.FromArgb ("#F0FFF0"), Color.FromArgb ("#D0FFD0") ],	// 14. Settings
			[ Color.FromArgb ("#F0FFF0"), Color.FromArgb ("#D0FFD0") ],	// 15. App about
			];
		private const int hdrPage = 0;
		private const int usgPage = 1;
		private const int errPage = 2;
		private const int fnlPage = 3;
		private const int rnmPage = 4;
		private const int ofdPage = 5;
		private const int tlvPage = 6;
		private const int tdcPage = 7;
		private const int codPage = 8;
		private const int conPage = 9;
		private const int bcdPage = 10;
		private const int cvhPage = 11;
		private const int cvsPage = 12;
		private const int cvuPage = 13;
		private const int setPage = 14;
		private const int aabPage = 15;

		private const int cBack = 0;
		private const int cField = 1;

		#endregion

		#region Переменные страниц

		private List<ContentPage> uiPages = [];

		private Label kktCodesHelpLabel, kktCodesErrorLabel, kktCodesResultText,
			kktCodesLengthLabel,
			errorsResultText, cableLeftSideText, cableRightSideText, cableLeftPinsText, cableRightPinsText,
			cableDescriptionText,
			fnLifeLabel, fnLifeModelLabel, fnLifeGenericTaxLabel, fnLifeGoodsLabel,
			rnmKKTTypeLabel, rnmINNCheckLabel, rnmRNMCheckLabel,
			/*lowLevelCommandDescr,*/
			dictionaryDescriptionField,
			tlvDescriptionLabel, tlvTypeLabel, tlvValuesLabel, tlvObligationLabel,
			barcodeDescriptionLabel, ofdDisabledLabel, convNumberResultField, convCodeResultField,
			fontSizeField;
		private List<Label> operationTextLabels = [];
		private List<Button> operationTextButtons = [];
		private const string operationButtonSignatureShow = "🖨 ";
		private const string operationButtonSignatureHide = "🚫 ";

		private Button kktCodesKKTButton, fnLifeDate, cableTypeButton, kktCodesCenterButton,
			errorsKKTButton, userManualsKKTButton, userManualsPrintButton,
			ofdNameButton, ofdDNSNameButton, ofdIPButton, ofdPortButton, ofdEmailButton, ofdSiteButton,
			ofdDNSNameMButton, ofdIPMButton, ofdPortMButton, ofdINN,
			/*lowLevelProtocol, lowLevelCommand, lowLevelCommandCode,*/ rnmGenerate, convCodeSymbolField,
			encodingButton, fnLifeStatus, sampleNextButton;
		private List<Button> uiButtons = [];

		private Editor codesSourceText, rnmINN, rnmRNM,
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

		// Сообщение о применимости модели ФН
		private string fnLifeMessage = "";

		// Число режимов преобразования
		private uint encodingModesCount;

		// Список ранее сохранённых подстановок для раздела кодов символов
		private List<string> kktCodesOftenTexts = [];

		// Список вариантов печати руководств
		private List<string> userGuideVariants = [];

		// Список вариантов обработки ссылки на ФФД
		private List<string> ffdBaseVariants = [];

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();
			}

		// Замена определению MainPage = new MasterPage ()
		protected override Window CreateWindow (IActivationState activationState)
			{
			return new Window (AppShell ());
			}

		// Инициализация интерфейса
		private Page AppShell ()
			{
			Page mainPage = new MasterPage ();
			RDAppStartupFlags flags = RDGenerics.GetAppStartupFlags (RDAppStartupFlags.DisableXPUN);

			kb = new KnowledgeBase ();

			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			#region Общая конструкция страниц приложения

			uiPages.Add (ApplyPageSettings (new HeadersPage (),
				"Разделы приложения", uiColors[hdrPage][0], false));
			menuLayout = (StackLayout)uiPages[hdrPage].FindByName ("MenuField");

			uiPages.Add (ApplyPageSettings (new UserManualsPage (),
				"Инструкции по работе с ККТ", uiColors[usgPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ErrorsPage (),
				"Коды ошибок ККТ", uiColors[errPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new FNLifePage (),
				"Срок жизни ФН", uiColors[fnlPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new RNMPage (),
				"Заводской и рег. номер ККТ", uiColors[rnmPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new OFDPage (),
				"Параметры ОФД", uiColors[ofdPage][cBack], true));

			uiPages.Add (ApplyPageSettings (new TagsPage (),
				"TLV-теги", uiColors[tlvPage][cBack], true));
			uiButtons[tlvPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			/*uiPages.Add (ApplyPageSettings (new LowLevelPage (),
				"Команды нижнего уровня", uiColors[llvPage][cBack], true));*/
			uiPages.Add (ApplyPageSettings (new TermsDictionaryPage (),
				"Словарь терминов", uiColors[tdcPage][cBack], true));

			// !!! ВРЕМЕННО !!!
			uiButtons[tdcPage - 1].IsVisible = false;
			// !!! ВРЕМЕННО !!!

			/*uiButtons[llvPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2*/

			uiPages.Add (ApplyPageSettings (new KKTCodesPage (),
				"Перевод текста в коды ККТ", uiColors[codPage][cBack], true));
			uiButtons[codPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 1

			uiPages.Add (ApplyPageSettings (new ConnectorsPage (),
				"Разъёмы", uiColors[conPage][cBack], true));
			uiButtons[conPage - 1].IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			uiPages.Add (ApplyPageSettings (new BarCodesPage (),
				"Штрих-коды", uiColors[bcdPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsHTPage (),
				"Конвертор двоичных данных", uiColors[cvhPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsSNPage (),
				"Конвертор систем счисления", uiColors[cvsPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new ConvertorsUCPage (),
				"Конвертор символов Unicode", uiColors[cvuPage][cBack], true));

			// С отступом
			menuLayout.Children.Add (new Label ());
			uiPages.Add (ApplyPageSettings (new SettingsPage (),
				"Настройки", uiColors[setPage][cBack], true));
			uiPages.Add (ApplyPageSettings (new AboutPage (),
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				uiColors[aabPage][cBack], true));

			#endregion

			#region Настройки приложения

			RDInterface.ApplyLabelSettings (uiPages[setPage], "ExtendedModeLabel",
				"Режим сервис-инженера", RDLabelTypes.DefaultLeft);
			extendedMode = RDInterface.ApplySwitchSettings (uiPages[setPage], "ExtendedMode", false,
				uiColors[setPage][cField], ExtendedMode_Toggled, AppSettings.EnableExtendedMode);

			RDInterface.ApplyLabelSettings (uiPages[setPage], "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.TipCenter);

			RDInterface.ApplyLabelSettings (uiPages[setPage], "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			RDInterface.ApplyButtonSettings (uiPages[setPage], "FontSizeInc",
				RDDefaultButtons.Increase, uiColors[setPage][cField], FontSizeButton_Clicked);
			RDInterface.ApplyButtonSettings (uiPages[setPage], "FontSizeDec",
				RDDefaultButtons.Decrease, uiColors[setPage][cField], FontSizeButton_Clicked);
			fontSizeField = RDInterface.ApplyLabelSettings (uiPages[setPage], "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);

			#endregion

			RDInterface.SetMasterPage (mainPage, uiPages[hdrPage], uiColors[hdrPage][cBack]);

			#region Страница «оглавления»

			RDInterface.ApplyLabelSettings (uiPages[hdrPage], "FunctionsLabel",
				"Разделы приложения", RDLabelTypes.HeaderCenter);
			/*RDInterface.ApplyLabelSettings (uiPages[hdrPage], "SettingsLabel",
				"Настройки", RDLabelTypes.HeaderCenter);*/

			if (AppSettings.CurrentTab <= aabPage)
				RDInterface.SetCurrentPage (uiPages[(int)AppSettings.CurrentTab],
					uiColors[(int)AppSettings.CurrentTab][cBack]);

			#endregion

			#region Страница инструкций

			Label ut = RDInterface.ApplyLabelSettings (uiPages[usgPage], "SelectionLabel", "Модель ККТ:",
				RDLabelTypes.HeaderLeft);
			userManualLayout = (StackLayout)uiPages[usgPage].FindByName ("UserManualLayout");
			int operationsCount = UserGuides.OperationTypes (!AppSettings.EnableExtendedMode).Length;

			uint sections = AppSettings.UserGuidesSectionsState;
			for (int i = 0; i < operationsCount; i++)
				{
				bool sectionEnabled = ((sections & (1u << i)) != 0);

				Button bh = new Button ();
				bh.BackgroundColor = uiColors[usgPage][cBack];
				bh.Clicked += OperationTextButton_Clicked;
				bh.FontAttributes = FontAttributes.Bold;
				bh.FontSize = ut.FontSize;
				bh.HorizontalOptions = LayoutOptions.Center;
				bh.IsVisible = true;
				bh.Margin = ut.Margin;
				bh.Padding = new Thickness (6, 0);
				bh.Text = (sectionEnabled ? operationButtonSignatureShow : operationButtonSignatureHide) +
					UserGuides.OperationTypes (false)[i];
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
				lt.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.AndroidTextColor);

				operationTextButtons.Add (bh);
				userManualLayout.Children.Add (operationTextButtons[operationTextButtons.Count - 1]);
				operationTextLabels.Add (lt);
				userManualLayout.Children.Add (operationTextLabels[operationTextLabels.Count - 1]);
				}

			userManualsKKTButton = RDInterface.ApplyButtonSettings (uiPages[usgPage], "KKTButton",
				"   ", uiColors[usgPage][cField], UserManualsKKTButton_Clicked, true);
			userManualsPrintButton = RDInterface.ApplyButtonSettings (uiPages[usgPage], "PrintButton",
				"В файл / на печать", uiColors[usgPage][cField], PrintManual_Clicked, false);
			userManualsPrintButton.FontSize -= 2.0;
			userManualsPrintButton.Margin = new Thickness (0);
			userManualsPrintButton.Padding = new Thickness (3, 0);
			userManualsPrintButton.HeightRequest = userManualsPrintButton.FontSize * 2.0;

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "HelpLabel",
				UserGuides.UserManualsTip + RDLocale.RN +
				"Нажатие на заголовки разделов позволяет добавить или скрыть их в видимой и печатной " +
				"версиях этой инструкции",
				RDLabelTypes.TipCenter);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "OneItemLabel",
				"В чеках бывает более одной позиции", RDLabelTypes.DefaultLeft);
			moreThanOneItemPerDocument = RDInterface.ApplySwitchSettings (uiPages[usgPage], "OneItemSwitch",
				false, uiColors[usgPage][cField], null, false);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "PricesLabel",
				"В номенклатуре есть цены", RDLabelTypes.DefaultLeft);
			productBaseContainsPrices = RDInterface.ApplySwitchSettings (uiPages[usgPage], "PricesSwitch",
				false, uiColors[usgPage][cField], null, false);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "PasswordsLabel",
				"Кассиры пользуются паролями", RDLabelTypes.DefaultLeft);
			cashiersHavePasswords = RDInterface.ApplySwitchSettings (uiPages[usgPage], "PasswordsSwitch",
				false, uiColors[usgPage][cField], null, false);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "ServicesLabel",
				"В номенклатуре содержатся услуги", RDLabelTypes.DefaultLeft);
			baseContainsServices = RDInterface.ApplySwitchSettings (uiPages[usgPage], "ServicesSwitch",
				false, uiColors[usgPage][cField], null, false);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "MarksLabel",
				"Среди товаров есть маркированные (ТМТ)", RDLabelTypes.DefaultLeft);
			documentsContainMarks = RDInterface.ApplySwitchSettings (uiPages[usgPage], "MarksSwitch",
				false, uiColors[usgPage][cField], null, false);

			RDInterface.ApplyLabelSettings (uiPages[usgPage], "SingleItemLabel",
				"Номенклатурная позиция – единственная", RDLabelTypes.DefaultLeft);
			productBaseContainsSingleItem = RDInterface.ApplySwitchSettings (uiPages[usgPage], "SingleItemSwitch",
				false, uiColors[usgPage][cField], null, false);

			UserManualFlags = (UserGuidesFlags)AppSettings.UserGuidesFlags;

			moreThanOneItemPerDocument.Toggled += UserManualsFlags_Clicked;
			productBaseContainsPrices.Toggled += UserManualsFlags_Clicked;
			cashiersHavePasswords.Toggled += UserManualsFlags_Clicked;
			baseContainsServices.Toggled += UserManualsFlags_Clicked;
			documentsContainMarks.Toggled += UserManualsFlags_Clicked;
			productBaseContainsSingleItem.Toggled += UserManualsFlags_Clicked;

			UserManualsKKTButton_Clicked (null, null);

			#endregion

			#region Страница кодов символов ККТ

			RDInterface.ApplyLabelSettings (uiPages[codPage], "SelectionLabel", "Модель ККТ:",
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

			kktCodesKKTButton = RDInterface.ApplyButtonSettings (uiPages[codPage], "KKTButton",
				kktTypeName, uiColors[codPage][cField], CodesKKTButton_Clicked, true);
			RDInterface.ApplyLabelSettings (uiPages[codPage], "SourceTextLabel",
				"Исходный текст:", RDLabelTypes.HeaderLeft);

			codesSourceText = RDInterface.ApplyEditorSettings (uiPages[codPage], "SourceText",
				uiColors[codPage][cField], Keyboard.Default, 72, AppSettings.CodesText, SourceText_TextChanged, true);
			codesSourceText.HorizontalOptions = LayoutOptions.Fill;

			RDInterface.ApplyLabelSettings (uiPages[codPage], "ResultTextLabel", "Коды ККТ:",
				RDLabelTypes.HeaderLeft);

			kktCodesErrorLabel = RDInterface.ApplyLabelSettings (uiPages[codPage], "ErrorLabel",
				"Часть введённых символов не поддерживается данной ККТ", RDLabelTypes.ErrorTip);

			kktCodesResultText = RDInterface.ApplyLabelSettings (uiPages[codPage], "ResultText", " ",
				RDLabelTypes.FieldMonotype, uiColors[codPage][cField]);

			RDInterface.ApplyLabelSettings (uiPages[codPage], "HelpTextLabel", "Пояснения к вводу:",
				RDLabelTypes.HeaderLeft);
			kktCodesHelpLabel = RDInterface.ApplyLabelSettings (uiPages[codPage], "HelpText",
				kb.CodeTables.GetKKTTypeDescription (AppSettings.KKTForCodes), RDLabelTypes.Field,
				uiColors[codPage][cField]);

			RDInterface.ApplyButtonSettings (uiPages[codPage], "ClearText",
				RDDefaultButtons.Delete, uiColors[codPage][cField], CodesClear_Clicked);
			RDInterface.ApplyButtonSettings (uiPages[codPage], "SelectSavedText",
				"Выбрать", uiColors[codPage][cField], CodesLineGet_Clicked, false);

			kktCodesLengthLabel = RDInterface.ApplyLabelSettings (uiPages[codPage], "LengthLabel",
				"Длина:", RDLabelTypes.HeaderLeft);
			kktCodesCenterButton = RDInterface.ApplyButtonSettings (uiPages[codPage], "CenterText",
				"Центрировать", uiColors[codPage][cField], TextToConvertCenter_Click, false);
			RDInterface.ApplyButtonSettings (uiPages[codPage], "RememberText",
				"Запомнить", uiColors[codPage][cField], TextToConvertRemember_Click, false);

			SourceText_TextChanged (null, null);    // Протягивание кодов

			#endregion

			#region Страница ошибок

			RDInterface.ApplyLabelSettings (uiPages[errPage], "SelectionLabel",
				"Модель ККТ:", RDLabelTypes.HeaderLeft);

			try
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[(int)AppSettings.KKTForErrors];
				}
			catch
				{
				kktTypeName = kb.Errors.GetKKTTypeNames ()[0];
				AppSettings.KKTForErrors = 0;
				}
			errorsKKTButton = RDInterface.ApplyButtonSettings (uiPages[errPage], "KKTButton",
				kktTypeName, uiColors[errPage][cField], ErrorsKKTButton_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[errPage], "ResultTextLabel",
				"Описание ошибки:", RDLabelTypes.HeaderLeft);

			errorsResultText = RDInterface.ApplyLabelSettings (uiPages[errPage], "ResultText",
				"", RDLabelTypes.Field, uiColors[errPage][cField]);

			RDInterface.ApplyButtonSettings (uiPages[errPage], "ErrorFindButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Find), uiColors[errPage][cField],
				ErrorFind_Clicked, false);
			sampleNextButton = RDInterface.ApplyButtonSettings (uiPages[errPage], "ErrorFindNextButton",
				NextButton, uiColors[errPage][cField], ErrorFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[errPage], "ErrorFindBufferButton",
				BufferButton, uiColors[errPage][cField], ErrorFind_Clicked, false);
			ErrorFind_Clicked (sampleNextButton, null);

			#endregion

			#region Страница "О программе"

			RDInterface.ApplyLabelSettings (uiPages[aabPage], "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			RDInterface.ApplyButtonSettings (uiPages[aabPage], "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				uiColors[aabPage][cField], ReferenceButton_Click, false);
			RDInterface.ApplyButtonSettings (uiPages[aabPage], "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				uiColors[aabPage][cField], HelpButton_Click, false);
			/*RDInterface.ApplyLabelSettings (uiPages[aabPage], "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);*/

			RDInterface.ApplyLabelSettings (uiPages[aabPage], "HelpHeaderLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				RDLabelTypes.HeaderLeft);
			Label htl = RDInterface.ApplyLabelSettings (uiPages[aabPage], "HelpTextLabel",
				RDGenerics.GetAppHelpText (), RDLabelTypes.SmallLeft);
			htl.TextType = TextType.Html;

			FontSizeButton_Clicked (null, null);

			#endregion

			#region Страница определения срока жизни ФН

			RDInterface.ApplyButtonSettings (uiPages[fnlPage], "FNFindButton",
				"Найти модель ФН", uiColors[fnlPage][cField],
				FNLifeFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[fnlPage], "FNFindBufferButton",
				BufferButton, uiColors[fnlPage][cField], FNLifeFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[fnlPage], "FNResetButton",
				RDDefaultButtons.Delete, uiColors[fnlPage][cField], FNLife_Reset);

			fnLife13 = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLife13", true,
				uiColors[fnlPage][cField], FnLife13_Toggled, false);
			fnLifeLabel = RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeLabel",
				"", RDLabelTypes.DefaultLeft);

			//
			fnLifeModelLabel = RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeModelLabel",
				"", RDLabelTypes.Semaphore);

			//
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "SetUserParameters",
				"Значимые параметры:", RDLabelTypes.HeaderLeft);

			fnLifeGenericTax = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGenericTax", true,
				uiColors[fnlPage][cField], null, false);
			fnLifeGenericTaxLabel = RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGenericTaxLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeGoods = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGoods", true,
				uiColors[fnlPage][cField], null, false);
			fnLifeGoodsLabel = RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGoodsLabel",
				"", RDLabelTypes.DefaultLeft);

			fnLifeSeason = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeSeason", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeSeasonLabel",
				"Сезонная\nторговля", RDLabelTypes.DefaultLeft);

			fnLifeAgents = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeAgents", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeAgentsLabel",
				"Платёжный\n(суб)агент", RDLabelTypes.DefaultLeft);

			fnLifeExcise = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeExcise", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeExciseLabel",
				"Подакцизные\nтовары", RDLabelTypes.DefaultLeft);

			fnLifeAutonomous = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeAutonomous", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeAutonomousLabel",
				"Автономный\nрежим", RDLabelTypes.DefaultLeft);

			fnLifeFFD12 = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeFFD12", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeFFD12Label",
				"ФФД 1.1, 1.2", RDLabelTypes.DefaultLeft);

			fnLifeGambling = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeGambling", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeGamblingLabel",
				"Азартные игры\nи лотереи", RDLabelTypes.DefaultLeft);

			fnLifePawn = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifePawn", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifePawnLabel",
				"Ломбарды\nи страхование", RDLabelTypes.DefaultLeft);

			fnLifeMarkGoods = RDInterface.ApplySwitchSettings (uiPages[fnlPage], "FNLifeMarkGoods", false,
				uiColors[fnlPage][cField], null, false);
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeMarkGoodsLabel",
				"Маркированные\nтовары", RDLabelTypes.DefaultLeft);

			//
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "SetDate",
				"Дата фискализации:", RDLabelTypes.DefaultLeft);
			fnLifeStartDate = RDInterface.ApplyDatePickerSettings (uiPages[fnlPage], "FNLifeStartDate",
				uiColors[fnlPage][cField], FnLifeStartDate_DateSelected);

			//
			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeResultLabel",
				"Результат:", RDLabelTypes.HeaderLeft);

			fnLifeDate = RDInterface.ApplyButtonSettings (uiPages[fnlPage], "FNLifeDate", "",
				uiColors[fnlPage][cField], FNLifeDateCopy, true);
			fnLifeStatus = RDInterface.ApplyButtonSettings (uiPages[fnlPage], "FNLifeStatus", "?",
				uiColors[fnlPage][cField], FNLifeStatusClick, true);

			// Применение параметров, которые не могут быть рассчитаны при инициализации
			fnLifeStatus.WidthRequest = fnLifeStatus.HeightRequest =
				fnLifeDate.HeightRequest = 10 * RDInterface.MasterFontSize / 3;

			RDInterface.ApplyLabelSettings (uiPages[fnlPage], "FNLifeHelpLabel",
				"Нажатие кнопки копирует дату окончания срока жизни в буфер обмена",
				RDLabelTypes.TipCenter);

			RDInterface.ApplyButtonSettings (uiPages[fnlPage], "RegistryStats",
				"Статистика реестра ФН", uiColors[fnlPage][cField], FNStats_Clicked, false);

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

			FNLifeFind_Clicked (sampleNextButton, null);

			#endregion

			#region Страница заводских и регистрационных номеров

			RDInterface.ApplyButtonSettings (uiPages[rnmPage], "RNMFindButton",
				"Найти модель ККТ", uiColors[rnmPage][cField],
				RNMSerial_Search, false);
			RDInterface.ApplyButtonSettings (uiPages[rnmPage], "RNMFindBufferButton",
				BufferButton, uiColors[rnmPage][cField], RNMSerial_Search, false);
			RDInterface.ApplyButtonSettings (uiPages[rnmPage], "RNMResetButton",
				RDDefaultButtons.Delete, uiColors[rnmPage][cField], RNMClear_Clicked);

			rnmKKTTypeLabel = RDInterface.ApplyLabelSettings (uiPages[rnmPage], "TypeLabel",
				"", RDLabelTypes.Semaphore);

			RDInterface.ApplyLabelSettings (uiPages[rnmPage], "INNLabel",
				"ИНН пользователя:", RDLabelTypes.HeaderLeft);
			rnmINN = RDInterface.ApplyEditorSettings (uiPages[rnmPage], "INN",
				uiColors[rnmPage][cField], Keyboard.Numeric, 12, AppSettings.UserINN,
				RNMINNUpdate, true);

			rnmINNCheckLabel = RDInterface.ApplyLabelSettings (uiPages[rnmPage], "INNCheckLabel", "",
				RDLabelTypes.Semaphore);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				RDInterface.ApplyLabelSettings (uiPages[rnmPage], "RNMLabel",
					"Регистрационный номер для проверки или произвольное число для генерации:",
					RDLabelTypes.HeaderLeft);
			else
				RDInterface.ApplyLabelSettings (uiPages[rnmPage], "RNMLabel",
					"Регистрационный номер для проверки:", RDLabelTypes.HeaderLeft);

			rnmRNM = RDInterface.ApplyEditorSettings (uiPages[rnmPage], "RNM",
				uiColors[rnmPage][cField], Keyboard.Numeric, 16, AppSettings.RNMKKT,
				RNMRNMUpdate, true);

			rnmRNMCheckLabel = RDInterface.ApplyLabelSettings (uiPages[rnmPage], "RNMCheckLabel", "",
				RDLabelTypes.Semaphore);

			rnmGenerate = RDInterface.ApplyButtonSettings (uiPages[rnmPage], "RNMGenerate",
				"Сгенерировать", uiColors[rnmPage][cField], RNMGenerate_Clicked, false);
			rnmGenerate.IsVisible = AppSettings.EnableExtendedMode;  // Уровень 2

			RDInterface.ApplyButtonSettings (uiPages[rnmPage], "RegistryStats",
				"Статистика реестра ККТ", uiColors[rnmPage][cField], RNMStats_Clicked, false);

			if (AppSettings.EnableExtendedMode)  // Уровень 2
				RDInterface.ApplyLabelSettings (uiPages[rnmPage], "RNMAbout",
					KKTSupport.RNMTip, RDLabelTypes.TipJustify);

			RNMSerial_Search (sampleNextButton, null);
			RNMINNUpdate (null, null);

			#endregion

			#region Страница настроек ОФД

			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDFindButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Find), uiColors[ofdPage][cField],
				OFDFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDFindNextButton",
				NextButton, uiColors[ofdPage][cField], OFDFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDFindBufferButton",
				BufferButton, uiColors[ofdPage][cField], OFDFind_Clicked, false);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDINNLabel",
				"ИНН ОФД:", RDLabelTypes.HeaderLeft);
			ofdINN = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDINN",
				AppSettings.OFDINN, uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDNameLabel",
				"Название:", RDLabelTypes.HeaderLeft);
			ofdNameButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDName",
				"- Выберите или введите ИНН -", uiColors[ofdPage][cField], OFDName_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameLabel",
				"Адрес ОФД:", RDLabelTypes.HeaderLeft);
			ofdDNSNameButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSName",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdIPButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDIP",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdPortButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDPort",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameMLabel",
				"Адрес ИСМ:", RDLabelTypes.HeaderLeft);
			ofdDNSNameMButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSNameM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdIPMButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDIPM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);
			ofdPortMButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDPortM",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDDNSNameKLabel",
				"Адрес ОКП (стандартный):", RDLabelTypes.HeaderLeft);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDDNSNameK", OFD.OKPSite,
				uiColors[ofdPage][cField], Field_Clicked, true);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDIPK", OFD.OKPIP,
				uiColors[ofdPage][cField], Field_Clicked, true);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDPortK", OFD.OKPPort,
				uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDEmailLabel",
				"E-mail отправителя чеков:", RDLabelTypes.HeaderLeft);
			ofdEmailButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDEmail",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDSiteLabel",
				"Сайт ОФД:", RDLabelTypes.HeaderLeft);
			ofdSiteButton = RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDSite",
				"", uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDNalogSiteLabel",
				"Сайт ФНС:", RDLabelTypes.HeaderLeft);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "OFDNalogSite", OFD.FNSSite,
				uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "CDNSiteLabel",
				"CDN-площадка ЦРПТ:", RDLabelTypes.HeaderLeft);
			RDInterface.ApplyButtonSettings (uiPages[ofdPage], "CDNSite", OFD.CDNSite,
				uiColors[ofdPage][cField], Field_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDHelpLabel",
				"Нажатие кнопок копирует их подписи в буфер обмена",
				RDLabelTypes.TipCenter);

			ofdDisabledLabel = RDInterface.ApplyLabelSettings (uiPages[ofdPage], "OFDDisabledLabel",
				"Аннулирован", RDLabelTypes.ErrorTip);
			ofdDisabledLabel.IsVisible = false;

			OFDINN_TextChanged (null, null); // Протягивание значений

			#endregion

			#region Страница TLV-тегов

			RDInterface.ApplyButtonSettings (uiPages[tlvPage], "TLVFindButton",
				"Найти тег", uiColors[tlvPage][cField], TLVFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[tlvPage], "TLVFindNextButton",
				NextButton, uiColors[tlvPage][cField], TLVFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[tlvPage], "TLVFindBufferButton",
				BufferButton, uiColors[tlvPage][cField], TLVFind_Clicked, false);

			RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVDescriptionLabel",
				"Описание тега:", RDLabelTypes.HeaderLeft);
			tlvDescriptionLabel = RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVDescription",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVTypeLabel", "Тип тега:",
				RDLabelTypes.HeaderLeft);
			tlvTypeLabel = RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVType",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVValuesLabel",
				"Возможные значения тега:", RDLabelTypes.HeaderLeft);
			tlvValuesLabel = RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVValues",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);

			RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVObligationLabel",
				"Обязательность:", RDLabelTypes.HeaderLeft);
			tlvObligationLabel = RDInterface.ApplyLabelSettings (uiPages[tlvPage], "TLVObligation",
				"", RDLabelTypes.Field, uiColors[tlvPage][cField]);
			tlvObligationLabel.TextType = TextType.Html;

			RDInterface.ApplyButtonSettings (uiPages[tlvPage], "TLVObligationHelpLabel",
				TLVTags.ObligationBase, uiColors[tlvPage][cField], TLVObligationBase_Click, false);

			TLVFind_Clicked (sampleNextButton, null);

			#endregion

			#region Страница словаря терминов

			RDInterface.ApplyButtonSettings (uiPages[tdcPage], "DictionaryFindButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Find), uiColors[tdcPage][cField],
				DictionaryFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[tdcPage], "DictionaryFindNextButton",
				NextButton, uiColors[tdcPage][cField], DictionaryFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[tdcPage], "DictionaryFindBufferButton",
				BufferButton, uiColors[tdcPage][cField], DictionaryFind_Clicked, false);

			/*RDInterface.ApplyLabelSettings (uiPages[llvPage], "ProtocolLabel",
				"Протокол:", RDLabelTypes.HeaderLeft);
			lowLevelProtocol = RDInterface.ApplyButtonSettings (uiPages[llvPage], "ProtocolButton",
				kb.LLCommands.GetProtocolsNames ()[(int)AppSettings.LowLevelProtocol],
				uiColors[llvPage][cField], LowLevelProtocol_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[llvPage], "CommandLabel",
				"Команда:", RDLabelTypes.HeaderLeft);
			lowLevelCommand = RDInterface.ApplyButtonSettings (uiPages[llvPage], "CommandButton",
				kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol)[(int)AppSettings.LowLevelCode],
				uiColors[llvPage][cField], LowLevelCommandCodeButton_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[llvPage], "CommandCodeLabel",
				"Код команды:", RDLabelTypes.HeaderLeft);
			lowLevelCommandCode = RDInterface.ApplyButtonSettings (uiPages[llvPage], "CommandCodeButton",
				kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, false),
				uiColors[llvPage][cField], Field_Clicked, true);*/

			RDInterface.ApplyLabelSettings (uiPages[tdcPage], "DictionaryDescrLabel",
				"Описание:", RDLabelTypes.HeaderLeft);

			dictionaryDescriptionField = RDInterface.ApplyLabelSettings (uiPages[tdcPage], "DictionaryDescr",
				"",
				/*kb.LLCommands.GetCommand (AppSettings.LowLevelProtocol, AppSettings.LowLevelCode, true)*/
				RDLabelTypes.Field, uiColors[tdcPage][cField]);

			/*RDInterface.ApplyLabelSettings (uiPages[llvPage], "LowLevelHelpLabel",
				"Нажатие кнопки копирует команду в буфер обмена",
				RDLabelTypes.TipCenter);*/

			#endregion

			#region Страница штрих-кодов

			RDInterface.ApplyLabelSettings (uiPages[bcdPage], "BarcodeFieldLabel",
				"Данные штрих-кода:", RDLabelTypes.HeaderLeft);
			barcodeField = RDInterface.ApplyEditorSettings (uiPages[bcdPage], "BarcodeField",
				uiColors[bcdPage][cField], Keyboard.Default, BarCodes.MaxSupportedDataLength,
				AppSettings.BarcodeData, BarcodeText_TextChanged, true);

			RDInterface.ApplyLabelSettings (uiPages[bcdPage], "BarcodeDescriptionLabel",
				"Описание штрих-кода:", RDLabelTypes.HeaderLeft);
			barcodeDescriptionLabel = RDInterface.ApplyLabelSettings (uiPages[bcdPage], "BarcodeDescription",
				"", RDLabelTypes.Field, uiColors[bcdPage][cField]);

			RDInterface.ApplyButtonSettings (uiPages[bcdPage], "Clear",
				RDDefaultButtons.Delete, uiColors[bcdPage][cField], BarcodeClear_Clicked);
			RDInterface.ApplyButtonSettings (uiPages[bcdPage], "GetFromClipboard",
				RDDefaultButtons.Copy, uiColors[bcdPage][cField], BarcodeGet_Clicked);

			BarcodeText_TextChanged (null, null);

			#endregion

			#region Страница распиновок

			RDInterface.ApplyButtonSettings (uiPages[conPage], "ConnFindButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Find), uiColors[conPage][cField],
				ConnFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[conPage], "ConnFindNextButton",
				NextButton, uiColors[conPage][cField], ConnFind_Clicked, false);
			RDInterface.ApplyButtonSettings (uiPages[conPage], "ConnFindBufferButton",
				BufferButton, uiColors[conPage][cField], ConnFind_Clicked, false);

			RDInterface.ApplyLabelSettings (uiPages[conPage], "CableLabel",
				"Тип кабеля:", RDLabelTypes.HeaderLeft);
			cableTypeButton = RDInterface.ApplyButtonSettings (uiPages[conPage], "CableTypeButton",
				kb.Plugs.GetCablesNames ()[(int)AppSettings.CableType], uiColors[conPage][cField],
				CableTypeButton_Clicked, true);

			RDInterface.ApplyLabelSettings (uiPages[conPage], "CableDescriptionLabel",
				"Сопоставление контактов:", RDLabelTypes.HeaderLeft);
			cableLeftSideText = RDInterface.ApplyLabelSettings (uiPages[conPage], "CableLeftSide",
				" ", RDLabelTypes.DefaultLeft);
			cableLeftSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableLeftSideText.HorizontalOptions = LayoutOptions.Center;

			cableLeftPinsText = RDInterface.ApplyLabelSettings (uiPages[conPage], "CableLeftPins",
				" ", RDLabelTypes.FieldMonotype, uiColors[conPage][cField]);

			cableRightSideText = RDInterface.ApplyLabelSettings (uiPages[conPage], "CableRightSide",
				" ", RDLabelTypes.DefaultLeft);
			cableRightSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableRightSideText.HorizontalOptions = LayoutOptions.Center;

			cableRightPinsText = RDInterface.ApplyLabelSettings (uiPages[conPage], "CableRightPins",
				" ", RDLabelTypes.FieldMonotype, uiColors[conPage][cField]);
			cableLeftPinsText.HorizontalTextAlignment = cableRightPinsText.HorizontalTextAlignment =
				TextAlignment.Center;

			cableDescriptionText = RDInterface.ApplyLabelSettings (uiPages[conPage], "CableDescription",
				" ", RDLabelTypes.TipJustify);

			CableTypeButton_Clicked (null, null);

			#endregion

			#region Страница конвертора систем счисления

			RDInterface.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberLabel",
				"Число:", RDLabelTypes.HeaderLeft);
			convNumberField = RDInterface.ApplyEditorSettings (uiPages[cvsPage], "ConvNumberField",
				uiColors[cvsPage][cField], Keyboard.Default, 10, AppSettings.ConversionNumber,
				ConvNumber_TextChanged, true);

			RDInterface.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberInc",
				RDDefaultButtons.Increase, uiColors[cvsPage][cField], ConvNumberAdd_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberDec",
				RDDefaultButtons.Decrease, uiColors[cvsPage][cField], ConvNumberAdd_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvsPage], "ConvNumberClear",
				RDDefaultButtons.Delete, uiColors[cvsPage][cField], ConvNumberClear_Click);

			RDInterface.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberResultLabel",
				"Представление:", RDLabelTypes.HeaderLeft);
			convNumberResultField = RDInterface.ApplyLabelSettings (uiPages[cvsPage], "ConvNumberResultField",
				" ", RDLabelTypes.FieldMonotype, uiColors[cvsPage][cField]);
			ConvNumber_TextChanged (null, null);

			const string convHelp = "Шестнадцатеричные числа следует" + RDLocale.RN + "начинать с символов “0x”";
			RDInterface.ApplyLabelSettings (uiPages[cvsPage], "ConvHelpLabel", convHelp, RDLabelTypes.TipCenter);

			#endregion

			#region Страница конвертора символов Unicode

			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeFindButton",
				"Найти блок", uiColors[cvuPage][cField], ConvCodeFind_Click, false);
			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeFindNextButton",
				NextButton, uiColors[cvuPage][cField], ConvCodeFind_Click, false);
			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeFindBufferButton",
				BufferButton, uiColors[cvuPage][cField], ConvCodeFind_Click, false);

			encodingModesCount = (uint)(DataConvertors.AvailableEncodings.Length / 2);

			RDInterface.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeLabel",
				"Код символа или символ:", RDLabelTypes.HeaderLeft);
			convCodeField = RDInterface.ApplyEditorSettings (uiPages[cvuPage], "ConvCodeField",
				uiColors[cvuPage][cField], Keyboard.Default, 10, AppSettings.ConversionCode,
				ConvCode_TextChanged, true);

			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeInc",
				RDDefaultButtons.Increase, uiColors[cvuPage][cField], ConvCodeAdd_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeDec",
				RDDefaultButtons.Decrease, uiColors[cvuPage][cField], ConvCodeAdd_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeClear",
				RDDefaultButtons.Delete, uiColors[cvuPage][cField], ConvCodeClear_Click);

			RDInterface.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeResultLabel",
				"Символ Unicode:", RDLabelTypes.HeaderLeft);
			convCodeResultField = RDInterface.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeResultField",
				"", RDLabelTypes.FieldMonotype, uiColors[cvuPage][cField]);
			RDInterface.ApplyLabelSettings (uiPages[cvuPage], "ConvCodeDescrLabel",
				"Описание символа:", RDLabelTypes.HeaderLeft);

			convCodeSymbolField = RDInterface.ApplyButtonSettings (uiPages[cvuPage], "ConvCodeSymbolField",
				" ", uiColors[cvuPage][cField], CopyCharacter_Click, true);
			convCodeSymbolField.FontSize *= 5;
			ConvCode_TextChanged (null, null);

			RDInterface.ApplyLabelSettings (uiPages[cvuPage], "ConvHelpLabel",
				convHelp + "." + RDLocale.RN + "Нажатие кнопки с символом Unicode" + RDLocale.RN +
				"копирует его в буфер обмена", RDLabelTypes.TipCenter);

			#endregion

			#region Страница конвертора двоичных данных

			RDInterface.ApplyLabelSettings (uiPages[cvhPage], "HexLabel",
				"Данные (hex или BASE64):", RDLabelTypes.HeaderLeft);
			convHexField = RDInterface.ApplyEditorSettings (uiPages[cvhPage], "HexField",
				uiColors[cvhPage][cField], Keyboard.Default, 500, AppSettings.ConversionHex, null, true);
			convHexField.HorizontalOptions = LayoutOptions.Fill;

			RDInterface.ApplyButtonSettings (uiPages[cvhPage], "HexToTextButton",
				RDDefaultButtons.Down, uiColors[cvhPage][cField], ConvertHexToText_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvhPage], "TextToHexButton",
				RDDefaultButtons.Up, uiColors[cvhPage][cField], ConvertTextToHex_Click);
			RDInterface.ApplyButtonSettings (uiPages[cvhPage], "ClearButton",
				RDDefaultButtons.Delete, uiColors[cvhPage][cField], ClearConvertText_Click);

			RDInterface.ApplyLabelSettings (uiPages[cvhPage], "TextLabel",
				"Данные (текст):", RDLabelTypes.HeaderLeft);
			convTextField = RDInterface.ApplyEditorSettings (uiPages[cvhPage], "TextField",
				uiColors[cvhPage][cField], Keyboard.Default, 250, AppSettings.ConversionText, null, true);
			convTextField.HorizontalOptions = LayoutOptions.Fill;

			RDInterface.ApplyLabelSettings (uiPages[cvhPage], "EncodingLabel",
				"Кодировка:", RDLabelTypes.HeaderLeft);
			encodingButton = RDInterface.ApplyButtonSettings (uiPages[cvhPage], "EncodingButton",
				" ", uiColors[cvhPage][cField], EncodingButton_Clicked, true);
			EncodingButton_Clicked (null, null);

			#endregion

			// Обязательное принятие Политики и EULA
			AcceptPolicy (flags.HasFlag (RDAppStartupFlags.DisableXPUN));
			return mainPage;
			}

		// Локальный оформитель страниц приложения
		private ContentPage ApplyPageSettings (ContentPage CreatedPage, string PageTitle,
			Color PageBackColor, bool AddToMenu)
			{
			// Инициализация страницы
			ContentPage page = RDInterface.ApplyPageSettings (CreatedPage, PageTitle, PageBackColor);

			// Добавление в содержание
			if (!AddToMenu)
				return page;

			Button b = new Button ();
			b.Text = PageTitle;
			b.BackgroundColor = PageBackColor;
			b.Clicked += HeaderButton_Clicked;
			b.TextTransform = TextTransform.None;

			b.FontAttributes = FontAttributes.None;
			b.FontSize = 5 * RDInterface.MasterFontSize / 4;
			b.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.AndroidTextColor);
			b.Margin = b.Padding = new Thickness (1);

			uiButtons.Add (b);
			menuLayout.Children.Add (b);

			return page;
			}

		// Контроль принятия Политики и EULA
		private static async void AcceptPolicy (bool DisableXPUN)
			{
			// Контроль XPUN
			if (!DisableXPUN)
				await RDInterface.XPUNLoop ();

			// Политика
			if (RDGenerics.TipsState != 0)
				return;

			await RDInterface.PolicyLoop ();

			// Только после принятия
			await RDInterface.ShowMessage ("Вас приветствует " + ProgramDescription.AssemblyMainName +
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
				RDInterface.SetCurrentPage (uiPages[idx + 1], uiColors[idx + 1][0]);
			}

		// Включение / выключение режима сервис-инженера
		private async void ExtendedMode_Toggled (object sender, EventArgs e)
			{
			if (extendedMode.IsToggled)
				{
				await RDInterface.ShowMessage (AppSettings.ExtendedModeMessage,
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				AppSettings.EnableExtendedMode = true;
				}
			else
				{
				await RDInterface.ShowMessage (AppSettings.NoExtendedModeMessage,
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				AppSettings.EnableExtendedMode = false;
				}
			}

		/// <summary>
		/// Обработчик события перехода в ждущий режим
		/// </summary>
		protected override void OnSleep ()
			{
			/*// Переключение состояния
			RDGenerics.AppIsRunning = false;*/

			// Сохранение настроек
			AppSettings.CurrentTab = (uint)uiPages.IndexOf ((ContentPage)RDInterface.MasterPage.CurrentPage);

			// ca.KKTForErrors	// Обновляется в коде программы

			AppSettings.FNLifeEvFlags = (uint)FNLifeEvFlags;
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

			AppSettings.ConversionNumber = convNumberField.Text;
			AppSettings.ConversionCode = convCodeField.Text;
			AppSettings.ConversionHex = convHexField.Text;
			AppSettings.ConversionText = convTextField.Text;
			//ca.EncodingForConvertor	// -||-

			AppSettings.UserGuidesFlags = (uint)UserManualFlags;
			}

		#endregion

		#region Коды ошибок ККТ

		// Выбор модели ККТ
		private async void ErrorsKKTButton_Clicked (object sender, EventArgs e)
			{
			// Запрос модели ККТ
			List<string> list = kb.Errors.GetKKTTypeNames ();

			// Установка модели
			int res = await RDInterface.ShowList ("Выберите модель ККТ:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			errorsKKTButton.Text = list[res];
			AppSettings.KKTForErrors = (uint)res;
			ErrorFind_Clicked (sampleNextButton, null);
			}

		// Поиск по тексту ошибки
		private static string ButtonNameFromText (Button Ctrl)
			{
			if (Ctrl.Text == NextButton)
				return "Next";

			if (Ctrl.Text == BufferButton)
				return "Buffer";

			return "Button";
			}
		private const string NextButton = "Далее";
		private const string BufferButton = "Из буфера";

		private async void ErrorFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.ErrorCode, "Введите код ошибки или фрагмент её текста", AppSettings.ErrorCodeMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.ErrorCode = search[0];
			if (search[1] == "I")
				lastErrorSearchOffset++;
			else
				lastErrorSearchOffset = 0;

			// 
			List<string> codes = kb.Errors.GetErrorCodesList (AppSettings.KKTForErrors);

			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				string code = codes[j].ToLower ();
				string res = kb.Errors.GetErrorText (AppSettings.KKTForErrors, (uint)j);

				if (code.Contains (search[0]) || res.ToLower ().Contains (search[0]) ||
					code.Contains ('?') && search[0].Contains (code.Replace ("?", "")))
					{
					lastErrorSearchOffset = (i + lastErrorSearchOffset) % codes.Count;
					errorsResultText.Text = codes[j] + ": " + res;

					return;
					}
				}

			// Код не найден
			errorsResultText.Text = "(описание ошибки не найдено)";
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
			int res = await RDInterface.ShowList ("Выберите модель ККТ:",
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

			int res = await RDInterface.ShowList ("Доступные варианты:",
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
				if (text.Contains (' '))
					text = text.Insert (text.IndexOf (' '), " ");

			codesSourceText.Text = text.PadLeft ((int)(total + text.Length) / 2, ' ');
			}

		#endregion

		#region Словарь терминов

		/*// Выбор команды нижнего уровня
		private async void LowLevelCommandCodeButton_Clicked (object sender, EventArgs e)
			{
			// Запрос кода ошибки
			List<string> list = kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol);
			int res = 0;
			if (e != null)
				res = await RDInterface.ShowList ("Выберите команду:",
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
			int res = await RDInterface.ShowList ("Выберите протокол:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);
			if (res < 0)
				return;

			AppSettings.LowLevelProtocol = (uint)res;
			lowLevelProtocol.Text = list[res];

			// Вызов вложенного обработчика
			LowLevelCommandCodeButton_Clicked (sender, null);
			}*/

		// Поиск по названию команды нижнего уровня
		private async void DictionaryFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.DictionarySearch, "Введите термин или его сокращение",
				AppSettings.DictionarySearchMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.DictionarySearch = search[0];
			if (search[1] == "I")
				lastCommandSearchOffset++;
			else
				lastCommandSearchOffset = 0;

			/*List<string> codes = kb.LLCommands.GetCommandsList (AppSettings.LowLevelProtocol);

			for (int i = 0; i < codes.Count; i++)
				{
				if (codes[(i + lastCommandSearchOffset) % codes.Count].ToLower ().Contains (search[0]))
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
				}*/
			}

		#endregion

		#region Срок жизни ФН

		// Поиск ЗН ФН в разделе определения срока жизни
		private async void FNLifeFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.FNSerial, "Введите заводской номер ФН или название модели",
				AppSettings.FNSerialMaxLength);
			if (search[1] == "C")
				return;

			// Подмена названия сигнатурой ЗН
			string sig = kb.FNNumbers.FindSignatureByName (search[0]);
			if (!string.IsNullOrWhiteSpace (sig))
				search[0] = sig;

			AppSettings.FNSerial = search[0];

			// Поиск
			if (!string.IsNullOrWhiteSpace (search[0]))
				fnLifeModelLabel.Text = kb.FNNumbers.GetFNName (search[0]);
			else
				fnLifeModelLabel.Text = "(модель ФН не задана)";

			// Определение длины ключа
			fnLife13.IsToggled = !kb.FNNumbers.IsNotFor36Months (search[0]);
			fnLife13.IsVisible = !kb.FNNumbers.IsFNKnown (search[0]);

			// Принудительное изменение
			FnLife13_Toggled (null, null);
			}

		// Сброс номера ФН
		private void FNLife_Reset (object sender, EventArgs e)
			{
			AppSettings.FNSerial = "";
			FNLifeFind_Clicked (sampleNextButton, null);
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
				fnLifeGenericTaxLabel.Text = "ОСН или\nсовмещение с ОСН";

			if (fnLifeGoods.IsToggled)
				fnLifeGoodsLabel.Text = "услуги";
			else
				fnLifeGoodsLabel.Text = "товары";

			// Расчёт срока
			FNLifeResult res = KKTSupport.GetFNLifeEndDate (fnLifeStartDate.Date, FNLifeEvFlags);

			fnLifeDate.Text = res.DeadLine;
			switch (res.Status)
				{
				case FNLifeStatus.Inacceptable:
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);

					fnLifeMessage = FNSerial.FNIsNotAcceptableMessage;
					break;

				case FNLifeStatus.Unwelcome:
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.WarningMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.WarningText);

					fnLifeMessage = FNSerial.FNIsNotRecommendedMessage;
					break;

				case FNLifeStatus.Acceptable:
				default:
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessText);

					fnLifeMessage = FNSerial.FNIsAcceptableMessage;
					break;

				case FNLifeStatus.StronglyUnwelcome:
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionText);

					fnLifeMessage = FNSerial.FNIsStronglyUnwelcomeMessage;
					break;
				}

			if (kb.FNNumbers.IsFNKnown (AppSettings.FNSerial))
				{
				if (!kb.FNNumbers.IsFNAllowed (AppSettings.FNSerial))
					{
					fnLifeStatus.BackgroundColor = fnLifeDate.BackgroundColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeStatus.TextColor = fnLifeDate.TextColor =
						RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);

					fnLifeMessage += (RDLocale.RN + FNSerial.FNIsNotAllowedMessage);

					fnLifeModelLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
					fnLifeModelLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);
					}
				else
					{
					fnLifeModelLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
					fnLifeModelLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessText);
					}
				}
			else
				{
				fnLifeModelLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				fnLifeModelLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionText);
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
			await RDInterface.ShowMessage (fnLifeMessage, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		// Статистика по базе ЗН ФН
		private async void FNStats_Clicked (object sender, EventArgs e)
			{
			await RDInterface.ShowMessage (kb.FNNumbers.RegistryStats,
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
				if (kb.FNNumbers.IsFNExactly13 (AppSettings.FNSerial))
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

				if (!kb.FNNumbers.IsFNKnown (AppSettings.FNSerial) ||
					kb.FNNumbers.IsFNCompatibleWithFFD12 (AppSettings.FNSerial))
					flags |= FNLifeFlags.FFD12Compatible;
				if (!kb.FNNumbers.IsFNKnown (AppSettings.FNSerial) ||
					kb.FNNumbers.IsFNCompletelySupportingFFD12 (AppSettings.FNSerial))
					flags |= FNLifeFlags.FFD12FullSupport;

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

		// Поиск модели ККТ
		private async void RNMSerial_Search (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.KKTSerial, "Введите заводской номер ККТ или название модели",
				kb.KKTNumbers.MaxSerialNumberLength);
			if (search[1] == "C")
				return;
			AppSettings.KKTSerial = search[0];

			// Подмена названия сигнатурой ЗН
			string sig = kb.KKTNumbers.FindSignatureByName (search[0]);
			if (!string.IsNullOrWhiteSpace (sig))
				search[0] = sig;

			// Заводской номер ККТ
			if (!string.IsNullOrWhiteSpace (search[0]))
				{
				rnmKKTTypeLabel.Text = "Модель ККТ: " + kb.KKTNumbers.GetKKTModel (search[0]);

				string s = kb.KKTNumbers.GetFFDSupportStatus (search[0]);
				if (!string.IsNullOrWhiteSpace (s))
					rnmKKTTypeLabel.Text += RDLocale.RN + s;
				}
			else
				{
				rnmKKTTypeLabel.Text = "(модель ККТ не задана)";
				}

			// Остальные поля
			RNMRNMUpdate (null, null);
			}

		// Изменение ИНН ОФД и РН ККТ
		private void RNMINNUpdate (object sender, TextChangedEventArgs e)
			{
			// ИНН пользователя
			rnmINNCheckLabel.Text = kb.KKTNumbers.GetRegionName (rnmINN.Text);
			if (KKTSupport.CheckINN (rnmINN.Text) < 0)
				{
				rnmINNCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				rnmINNCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionText);
				rnmINNCheckLabel.Text += " (неполный)";
				}
			else if (KKTSupport.CheckINN (rnmINN.Text) == 0)
				{
				rnmINNCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
				rnmINNCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessText);
				rnmINNCheckLabel.Text += " (ОК)";
				}
			else
				{
				rnmINNCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.WarningMessage);
				rnmINNCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.WarningText);
				rnmINNCheckLabel.Text += " (возможно, некорректный)";
				}

			// Остальные поля
			RNMRNMUpdate (null, null);
			}

		private void RNMRNMUpdate (object sender, TextChangedEventArgs e)
			{
			// РН
			if (rnmRNM.Text.Length < 10)
				{
				rnmRNMCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionMessage);
				rnmRNMCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.QuestionText);
				rnmRNMCheckLabel.Text = "(неполный)";
				}
			else if (KKTSupport.GetFullRNM (rnmINN.Text, AppSettings.KKTSerial,
				rnmRNM.Text.Substring (0, 10)) == rnmRNM.Text)
				{
				rnmRNMCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessMessage);
				rnmRNMCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.SuccessText);
				rnmRNMCheckLabel.Text = "(OK)";
				}
			else
				{
				rnmRNMCheckLabel.BackgroundColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorMessage);
				rnmRNMCheckLabel.TextColor = RDInterface.GetInterfaceColor (RDInterfaceColors.ErrorText);
				rnmRNMCheckLabel.Text = "(некорректный)";
				}
			}

		// Метод генерирует регистрационный номер ККТ
		private void RNMGenerate_Clicked (object sender, EventArgs e)
			{
			if (rnmRNM.Text.Length < 1)
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, AppSettings.KKTSerial, "0");
			else if (rnmRNM.Text.Length < 10)
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, AppSettings.KKTSerial, rnmRNM.Text);
			else
				rnmRNM.Text = KKTSupport.GetFullRNM (rnmINN.Text, AppSettings.KKTSerial,
					rnmRNM.Text.Substring (0, 10));

			if (string.IsNullOrWhiteSpace (rnmRNM.Text))
				RDInterface.ShowBalloon ("Убедитесь, что заводской номер и ИНН введены корректно", true);
			}

		// Очистка полей
		private void RNMClear_Clicked (object sender, EventArgs e)
			{
			AppSettings.KKTSerial = "";
			rnmINN.Text = "";
			rnmRNM.Text = "";

			RNMSerial_Search (sampleNextButton, null);
			}

		// Статистика по базе ЗН ККТ
		private async void RNMStats_Clicked (object sender, EventArgs e)
			{
			await RDInterface.ShowMessage (kb.KKTNumbers.RegistryStats,
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
			int res = await RDInterface.ShowList ("Выберите название ОФД:",
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
		private async void OFDFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.OFDSearch, "Введите ИНН ОФД или фрагмент его названия",
				AppSettings.OFDSearchMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.OFDSearch = search[0];
			if (search[1] == "I")
				lastOFDSearchOffset++;
			else
				lastOFDSearchOffset = 0;

			// Поиск
			List<string> codes = kb.Ofd.GetOFDNames (false);
			codes.AddRange (kb.Ofd.GetOFDINNs ());

			for (int i = 0; i < codes.Count; i++)
				{
				if (codes[(i + lastOFDSearchOffset) % codes.Count].ToLower ().Contains (search[0]))
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
			}

		#endregion

		#region О приложении

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
			await RDInterface.CallHelpMaterials (RDHelpMaterials.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await RDInterface.CallHelpMaterials (RDHelpMaterials.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Button b = (Button)sender;
				if (RDInterface.IsNameDefault (b.Text, RDDefaultButtons.Increase))
					RDInterface.MasterFontSize += 0.5;
				else if (RDInterface.IsNameDefault (b.Text, RDDefaultButtons.Decrease))
					RDInterface.MasterFontSize -= 0.5;
				}

			fontSizeField.Text = RDInterface.MasterFontSize.ToString ("F1");
			fontSizeField.FontSize = RDInterface.MasterFontSize;
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
				res = await RDInterface.ShowList ("Выберите модель ККТ:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			userManualsKKTButton.Text = list[res];
			AppSettings.KKTForManuals = (uint)res;

			for (int i = 0; i < operationTextLabels.Count; i++)
				{
				uint sections = AppSettings.UserGuidesSectionsState;
				bool state = ((sections & (1u << i)) != 0);

				operationTextButtons[i].Text = (state ? operationButtonSignatureShow : operationButtonSignatureHide) +
					UserGuides.OperationTypes (false)[i];
				operationTextLabels[i].Text = kb.UserGuides.GetGuide ((uint)res, (UserGuidesTypes)i,
					UserManualFlags);
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
				if (userGuideVariants.Count < 1)
					userGuideVariants = ["Для кассира", "Для сервис-инженера"];

				res = await RDInterface.ShowList ("Сохранить / распечатать руководство:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), userGuideVariants);
				if (res < 0)
					return;
				}

			// Печать
			UserGuidesFlags flags = UserManualFlags;
			if (res == 0)
				flags |= UserGuidesFlags.GuideForCashier;

			string text = KKTSupport.BuildUserGuide (kb.UserGuides, AppSettings.KKTForManuals,
				AppSettings.UserGuidesSectionsState, flags);
			KKTSupport.PrintManual (text, res == 0);
			}

		// Смена состояния кнопки
		private void OperationTextButton_Clicked (object sender, EventArgs e)
			{
			byte idx = (byte)operationTextButtons.IndexOf ((Button)sender);
			bool state = operationTextButtons[idx].Text.Contains (operationButtonSignatureHide);
			uint sections = AppSettings.UserGuidesSectionsState;

			if (state)
				sections |= (1u << idx);
			else
				sections &= ~(1u << idx);
			AppSettings.UserGuidesSectionsState = (uint)sections;

			operationTextButtons[idx].Text = (state ? operationButtonSignatureShow : operationButtonSignatureHide) +
				UserGuides.OperationTypes (false)[idx];
			operationTextLabels[idx].IsVisible = state;
			}

		/// <summary>
		/// Возвращает или задаёт состав флагов для руководства пользователя
		/// </summary>
		private UserGuidesFlags UserManualFlags
			{
			get
				{
				UserGuidesFlags flags = 0;

				if (moreThanOneItemPerDocument.IsToggled)
					flags |= UserGuidesFlags.MoreThanOneItemPerDocument;
				if (productBaseContainsPrices.IsToggled)
					flags |= UserGuidesFlags.ProductBaseContainsPrices;
				if (cashiersHavePasswords.IsToggled)
					flags |= UserGuidesFlags.CashiersHavePasswords;
				if (baseContainsServices.IsToggled)
					flags |= UserGuidesFlags.ProductBaseContainsServices;
				if (documentsContainMarks.IsToggled)
					flags |= UserGuidesFlags.DocumentsContainMarks;
				if (productBaseContainsSingleItem.IsToggled)
					flags |= UserGuidesFlags.BaseContainsSingleItem;

				return flags;
				}
			set
				{
				moreThanOneItemPerDocument.IsToggled = value.HasFlag (UserGuidesFlags.MoreThanOneItemPerDocument);
				productBaseContainsPrices.IsToggled = value.HasFlag (UserGuidesFlags.ProductBaseContainsPrices);
				cashiersHavePasswords.IsToggled = value.HasFlag (UserGuidesFlags.CashiersHavePasswords);
				baseContainsServices.IsToggled = value.HasFlag (UserGuidesFlags.ProductBaseContainsServices);
				documentsContainMarks.IsToggled = value.HasFlag (UserGuidesFlags.DocumentsContainMarks);
				productBaseContainsSingleItem.IsToggled = value.HasFlag (UserGuidesFlags.BaseContainsSingleItem);
				}
			}

		#endregion

		#region Теги TLV

		// Поиск тега
		private async void TLVFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.TLVData, "Введите номер TLV-тега или фрагмент его описания",
				AppSettings.TLVDataMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.TLVData = search[0];

			// Заполнение
			if (kb.Tags.FindTag (search[0]))
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

		// Ссылка на приказ-обоснование обязательности тегов
		private async void TLVObligationBase_Click (object sender, EventArgs e)
			{
			// Выбор варианта
			if (ffdBaseVariants.Count < 1)
				ffdBaseVariants = ["Скопировать основание", "Перейти к его тексту"];

			int res = await RDInterface.ShowList ("Что следует сделать?",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel),
				ffdBaseVariants);
			if (res < 0)
				return;

			// Запуск
			switch (res)
				{
				case 0:
					RDGenerics.SendToClipboard (TLVTags.ObligationBase, true);
					break;

				case 1:
					await RDGenerics.RunURL (TLVTags.ObligationBaseLink, true);
					break;
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
				res = await RDInterface.ShowList ("Выберите тип кабеля:",
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
		private async void ConnFind_Clicked (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.CableSearch, "Введите тип или назначение распиновки",
				AppSettings.CableSearchMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.CableSearch = search[0];
			if (search[1] == "I")
				lastConnSearchOffset++;
			else
				lastConnSearchOffset = 0;

			List<string> conns = kb.Plugs.GetCablesNames ();

			for (int i = 0; i < conns.Count; i++)
				{
				int j = (i + lastConnSearchOffset) % conns.Count;
				string conn = conns[j].ToLower ();

				if (conn.Contains (search[0]))
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

		#endregion

		#region Конверторы

		// Ввод значений для конверсии
		private void ConvNumber_TextChanged (object sender, EventArgs e)
			{
			convNumberResultField.Text = DataConvertors.GetNumberDescription (convNumberField.Text);
			}

		private void ConvNumberAdd_Click (object sender, EventArgs e)
			{
			bool plus = RDInterface.IsNameDefault (((Button)sender).Text,
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
			RDGenerics.HideKeyboard (convNumberField);
			}

		private void ConvCode_TextChanged (object sender, EventArgs e)
			{
			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text, 0, kb.Unicodes);
			convCodeSymbolField.Text = " " + res[0] + " ";
			convCodeResultField.Text = res[1];
			}

		private void ConvCodeAdd_Click (object sender, EventArgs e)
			{
			bool plus = RDInterface.IsNameDefault (((Button)sender).Text,
				RDDefaultButtons.Increase);

			string[] res = DataConvertors.GetSymbolDescription (convCodeField.Text,
				(short)(plus ? 1 : -1), kb.Unicodes);
			convCodeField.Text = res[2];
			RDGenerics.HideKeyboard (convCodeField);
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

		// Поиск диапазона символов Unicode
		private async void ConvCodeFind_Click (object sender, EventArgs e)
			{
			// Определение запроса
			string[] search = await KKTSupport.ObtainSearchCriteria (ButtonNameFromText ((Button)sender),
				AppSettings.ConversionCodeSearch, "Введите название или часть названия блока символов Unicode",
				AppSettings.ConversionCodeSearchMaxLength);
			if (search[1] == "C")
				return;

			AppSettings.ConversionCodeSearch = search[0];

			ulong left = kb.Unicodes.FindRange (search[0]);
			if (left != 0)
				{
				string[] res = DataConvertors.GetSymbolDescription ("\x0", (long)left, kb.Unicodes);
				convCodeField.Text = res[2];
				}
			}

		// Преобразование hex-данных в текст
		private void ConvertHexToText_Click (object sender, EventArgs e)
			{
			convTextField.Text = DataConvertors.ConvertHexToText (convHexField.Text,
				(RDEncodings)(AppSettings.EncodingForConvertor % encodingModesCount),
				AppSettings.EncodingForConvertor >= encodingModesCount);
			RDGenerics.HideKeyboard (convTextField);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			convHexField.Text = DataConvertors.ConvertTextToHex (convTextField.Text,
				(RDEncodings)(AppSettings.EncodingForConvertor % encodingModesCount),
				AppSettings.EncodingForConvertor >= encodingModesCount);
			RDGenerics.HideKeyboard (convHexField);
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
				res = await RDInterface.ShowList ("Выберите кодировку:",
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
