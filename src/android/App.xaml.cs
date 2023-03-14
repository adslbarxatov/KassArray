using Android.Print;
using Android.Widget;
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

			errorsMasterBackColor = Color.FromHex ("#FFF0F0"),
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

			convertorsMasterBackColor = Color.FromHex ("#FFE1EA"),
			convertorsFieldBackColor = Color.FromHex ("#FFD3E1");

		private const string firstStartRegKey = "HelpShownAt";

		#endregion

		#region Переменные страниц

		private ContentPage headersPage, kktCodesPage, errorsPage, aboutPage, connectorsPage,
			ofdPage, fnLifePage, rnmPage, lowLevelPage, userManualsPage, tagsPage, barCodesPage,
			convertorsSNPage, convertorsUCPage, convertorsHTPage;

		private Label kktCodesSourceTextLabel, kktCodesHelpLabel, kktCodesErrorLabel, kktCodesResultText,
			errorsResultText, cableLeftSideText, cableRightSideText, cableLeftPinsText, cableRightPinsText,
			cableDescriptionText,
			fnLifeLabel, fnLifeModelLabel, fnLifeGenericTaxLabel, fnLifeGoodsLabel,
			rnmKKTTypeLabel, rnmINNCheckLabel, rnmRNMCheckLabel, rnmSupport105, rnmSupport11, rnmSupport12,
			lowLevelCommandDescr, unlockLabel,
			tlvDescriptionLabel, tlvTypeLabel, tlvValuesLabel, tlvObligationLabel,
			barcodeDescriptionLabel, rnmTip, ofdDisabledLabel, convNumberResultField, convCodeResultField;
		private List<Label> operationTextLabels = new List<Label> ();

		private Xamarin.Forms.Button kktCodesKKTButton, fnLifeResult, cableTypeButton,
			errorsKKTButton, userManualsKKTButton,
			ofdNameButton, ofdDNSNameButton, ofdIPButton, ofdPortButton, ofdEmailButton, ofdSiteButton,
			ofdDNSNameMButton, ofdIPMButton, ofdPortMButton, ofdINN,
			lowLevelProtocol, lowLevelCommand, lowLevelCommandCode, rnmGenerate, convCodeSymbolField,
			encodingButton;

		private Editor codesSourceText, errorSearchText, commandSearchText, ofdSearchText,
			unlockField, fnLifeSerial, tlvTag, rnmKKTSN, rnmINN, rnmRNM,
			barcodeField, convNumberField, convCodeField, convHexField, convTextField;

		private Xamarin.Forms.Switch fnLife13, fnLifeGenericTax, fnLifeGoods, fnLifeSeason, fnLifeAgents,
			fnLifeExcise, fnLifeAutonomous, fnLifeFFD12, fnLifeGambling, fnLifePawn, fnLifeMarkGoods,
			keepAppState, allowService;

		private Xamarin.Forms.DatePicker fnLifeStartDate;

		private StackLayout userManualLayout, menuLayout;

		#endregion

		#region Основные переменные

		// Опорные классы
		private ConfigAccessor ca;
		private KKTSupport.FNLifeFlags fnlf;
		private PrintManager pm;

		// Справочники
		private OFD ofd = new OFD ();
		private KKTErrorsList kkme = new KKTErrorsList ();
		private KKTCodes kkmc = new KKTCodes ();
		private LowLevel ll = new LowLevel ();
		private FNSerial fns = new FNSerial ();
		private KKTSerial kkts = new KKTSerial ();
		private UserManuals um;
		private TLVTags tlvt = new TLVTags ();
		private BarCodes barc = new BarCodes ();
		private Connectors conn = new Connectors ();

		// Поисковые состояния
		private int lastErrorSearchOffset = 0;
		private int lastCommandSearchOffset = 0;
		private int lastOFDSearchOffset = 0;

		// Дата срока жизни ФН (в чистом виде)
		private string fnLifeResultDate = "";

		#endregion

		#region Основной функционал 

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App (PrintManager PrintingManager)
			{
			// Инициализация
			InitializeComponent ();
			ca = new ConfigAccessor (0, 0);
			um = new UserManuals (ca.ExtendedFunctions);
			pm = PrintingManager;

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
				convertorsMasterBackColor, true);
			convertorsUCPage = ApplyPageSettings ("ConvertorsUCPage", "Конвертор символов Unicode",
				convertorsMasterBackColor, true);
			convertorsHTPage = ApplyPageSettings ("ConvertorsHTPage", "Конвертор двоичных данных",
				convertorsMasterBackColor, true);

			aboutPage = ApplyPageSettings ("AboutPage", "О приложении",
				aboutMasterBackColor, true);

			AndroidSupport.SetMainPage (MainPage);

			#endregion

			#region Страница «оглавления»

			AndroidSupport.ApplyLabelSettings (headersPage, "KeepAppStateLabel",
				"Помнить настройки приложения", AndroidSupport.LabelTypes.DefaultLeft);
			keepAppState = AndroidSupport.ApplySwitchSettings (headersPage, "KeepAppState", false,
				headersFieldBackColor, null, ca.KeepApplicationState);

			AndroidSupport.ApplyLabelSettings (headersPage, "AllowServiceLabel",
				"Оставить службу активной после выхода", AndroidSupport.LabelTypes.DefaultLeft);
			allowService = AndroidSupport.ApplySwitchSettings (headersPage, "AllowService", false,
				headersFieldBackColor, AllowService_Toggled, AndroidSupport.AllowServiceToStart);

			try
				{
				((CarouselPage)MainPage).CurrentPage = ((CarouselPage)MainPage).Children[(int)ca.CurrentTab];
				}
			catch { }

			#endregion

			#region Страница инструкций

			Label ut = AndroidSupport.ApplyLabelSettings (userManualsPage, "SelectionLabel", "Модель ККТ:",
				AndroidSupport.LabelTypes.HeaderLeft);
			userManualLayout = (StackLayout)userManualsPage.FindByName ("UserManualLayout");

			for (int i = 0; i < um.OperationTypes.Length; i++)
				{
				Label lh = new Label ();
				lh.FontAttributes = FontAttributes.Bold;
				lh.FontSize = ut.FontSize;
				lh.HorizontalOptions = LayoutOptions.Start;
				lh.IsVisible = true;
				lh.Margin = ut.Margin;
				lh.Text = um.OperationTypes[i];
				lh.TextColor = ut.TextColor;

				Label lt = new Label ();
				lt.BackgroundColor = userManualsFieldBackColor;
				lt.Text = "   ";
				lt.FontAttributes = FontAttributes.None;
				lt.FontSize = ut.FontSize;
				lt.HorizontalOptions = LayoutOptions.Fill;
				lt.HorizontalTextAlignment = TextAlignment.Start;
				lt.Margin = lt.Padding = ut.Margin;
				lt.TextColor = AndroidSupport.MasterTextColor;

				userManualLayout.Children.Add (lh);
				operationTextLabels.Add (lt);
				userManualLayout.Children.Add (operationTextLabels[operationTextLabels.Count - 1]);
				}

			userManualsKKTButton = AndroidSupport.ApplyButtonSettings (userManualsPage, "KKTButton",
				"   ", userManualsFieldBackColor, UserManualsKKTButton_Clicked, true);
			AndroidSupport.ApplyButtonSettings (userManualsPage, "PrintButton",
				AndroidSupport.ButtonsDefaultNames.Select, userManualsFieldBackColor, PrintManual_Clicked);

			AndroidSupport.ApplyLabelSettings (userManualsPage, "HelpLabel",
				"<...> – индикация на экране, [...] – клавиши ККТ", AndroidSupport.LabelTypes.Tip);

			UserManualsKKTButton_Clicked (null, null);

			#endregion

			#region Страница кодов символов ККТ

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "SelectionLabel", "Модель ККТ:",
				AndroidSupport.LabelTypes.HeaderLeft);

			string kktTypeName;
			try
				{
				kktTypeName = kkmc.GetKKTTypeNames ()[(int)ca.KKTForCodes];
				}
			catch
				{
				kktTypeName = kkmc.GetKKTTypeNames ()[0];
				ca.KKTForCodes = 0;
				}
			kktCodesKKTButton = AndroidSupport.ApplyButtonSettings (kktCodesPage, "KKTButton",
				kktTypeName, kktCodesFieldBackColor, CodesKKTButton_Clicked, true);

			kktCodesSourceTextLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "SourceTextLabel",
				"Исходный текст:", AndroidSupport.LabelTypes.HeaderLeft);

			codesSourceText = AndroidSupport.ApplyEditorSettings (kktCodesPage, "SourceText",
				kktCodesFieldBackColor, Keyboard.Default, 72, ca.CodesText, SourceText_TextChanged, true);
			codesSourceText.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultTextLabel", "Коды ККТ:",
				AndroidSupport.LabelTypes.HeaderLeft);

			kktCodesErrorLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ErrorLabel",
				"Часть введённых символов не поддерживается данной ККТ", AndroidSupport.LabelTypes.ErrorTip);

			kktCodesResultText = AndroidSupport.ApplyLabelSettings (kktCodesPage, "ResultText", " ",
				AndroidSupport.LabelTypes.FieldMonotype, kktCodesFieldBackColor);

			AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpTextLabel", "Пояснения к вводу:",
				AndroidSupport.LabelTypes.HeaderLeft);
			kktCodesHelpLabel = AndroidSupport.ApplyLabelSettings (kktCodesPage, "HelpText",
				kkmc.GetKKTTypeDescription (ca.KKTForCodes), AndroidSupport.LabelTypes.Field,
				kktCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (kktCodesPage, "Clear",
				AndroidSupport.ButtonsDefaultNames.Delete, kktCodesFieldBackColor, CodesClear_Clicked);
			AndroidSupport.ApplyButtonSettings (kktCodesPage, "GetFromClipboard",
				AndroidSupport.ButtonsDefaultNames.Copy, kktCodesFieldBackColor, CodesLineGet_Clicked);

			SourceText_TextChanged (null, null);    // Протягивание кодов

			#endregion

			#region Страница ошибок

			AndroidSupport.ApplyLabelSettings (errorsPage, "SelectionLabel", "Модель ККТ:",
				AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (errorsPage, "SearchLabel", "Поиск ошибки по коду или фрагменту описания:",
				AndroidSupport.LabelTypes.HeaderLeft);

			try
				{
				kktTypeName = kkme.GetKKTTypeNames ()[(int)ca.KKTForErrors];
				}
			catch
				{
				kktTypeName = kkme.GetKKTTypeNames ()[0];
				ca.KKTForErrors = 0;
				}
			errorsKKTButton = AndroidSupport.ApplyButtonSettings (errorsPage, "KKTButton",
				kktTypeName, errorsFieldBackColor, ErrorsKKTButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (errorsPage, "ResultTextLabel", "Описание ошибки:",
				AndroidSupport.LabelTypes.HeaderLeft);

			errorsResultText = AndroidSupport.ApplyLabelSettings (errorsPage, "ResultText",
				"", AndroidSupport.LabelTypes.Field, errorsFieldBackColor);
			errorSearchText = AndroidSupport.ApplyEditorSettings (errorsPage, "ErrorSearchText", errorsFieldBackColor,
				Keyboard.Default, 30, ca.ErrorCode, null, true);
			Errors_Find (null, null);

			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorSearchButton",
				AndroidSupport.ButtonsDefaultNames.Find, errorsFieldBackColor, Errors_Find);
			AndroidSupport.ApplyButtonSettings (errorsPage, "ErrorClearButton",
				AndroidSupport.ButtonsDefaultNames.Delete, errorsFieldBackColor, Errors_Clear);

			#endregion

			#region Страница "О программе"

			AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				ProgramDescription.AssemblyTitle + "\n" +
				ProgramDescription.AssemblyDescription + "\n\n" +
				RDGenerics.AssemblyCopyright + "\nv " +
				ProgramDescription.AssemblyVersion +
				"; " + ProgramDescription.AssemblyLastUpdate, AndroidSupport.LabelTypes.AppAbout);

			AndroidSupport.ApplyLabelSettings (aboutPage, "ManualsLabel", "Справочные материалы",
				AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (aboutPage, "HelpLabel", "Помощь и поддержка",
				AndroidSupport.LabelTypes.HeaderLeft);

			AndroidSupport.ApplyButtonSettings (aboutPage, "AppPage", "Страница проекта",
				aboutFieldBackColor, AppButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "ADPPage", "Политика и EULA",
				aboutFieldBackColor, ADPButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "DevPage", "Спросить разработчика",
				aboutFieldBackColor, DevButton_Clicked, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualPage", "Руководство пользователя",
				aboutFieldBackColor, ManualButton_Clicked, false);

			/*AndroidSupport.ApplyButtonSettings (aboutPage, "FNReaderPage", "Работа с ФН",
				aboutFieldBackColor, UpdateButton_Clicked, false);*/
			AndroidSupport.ApplyButtonSettings (aboutPage, "CommunityPage",
				RDGenerics.AssemblyCompany, aboutFieldBackColor, CommunityButton_Clicked, false);

			// SupportedLanguages.ru_ru
			/*AndroidSupport.ApplyLabelSettings (aboutPage, "Alert", RDGenerics.RuAlertMessage,
				AndroidSupport.LabelTypes.DefaultLeft);*/

			if (!ca.AllowExtendedFunctionsLevel2)
				{
				unlockLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "UnlockLabel", ca.LockMessage,
					AndroidSupport.LabelTypes.DefaultLeft);
				unlockLabel.IsVisible = true;

				unlockField = AndroidSupport.ApplyEditorSettings (aboutPage, "UnlockField", aboutFieldBackColor,
					Keyboard.Default, 32, "", UnlockMethod, true);
				unlockField.IsVisible = true;
				}

			#endregion

			#region Страница определения срока жизни ФН

			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetModelLabel", "ЗН, номинал или модель ФН:",
				AndroidSupport.LabelTypes.HeaderLeft);
			fnLifeSerial = AndroidSupport.ApplyEditorSettings (fnLifePage, "FNLifeSerial", fnLifeFieldBackColor,
				Keyboard.Default, 16, ca.FNSerial, FNLifeSerial_TextChanged, true);
			fnLifeSerial.Margin = new Thickness (0);

			fnLife13 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLife13", true,
				fnLifeFieldBackColor, FnLife13_Toggled, false);

			fnLifeLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeLabel",
				"", AndroidSupport.LabelTypes.DefaultLeft);

			//
			fnLifeModelLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeModelLabel",
				"", AndroidSupport.LabelTypes.Semaphore);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetUserParameters", "Значимые параметры:",
				AndroidSupport.LabelTypes.HeaderLeft);

			fnLifeGenericTax = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGenericTax", true,
				fnLifeFieldBackColor, FnLife13_Toggled, !ca.GenericTaxFlag);
			fnLifeGenericTaxLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGenericTaxLabel",
				"", AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGoods", true,
				fnLifeFieldBackColor, FnLife13_Toggled, !ca.GoodsFlag);
			fnLifeGoodsLabel = AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGoodsLabel",
				"", AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeSeason = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeSeason", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.SeasonFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeSeasonLabel", "Сезонная торговля",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeAgents = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAgents", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.AgentsFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAgentsLabel", "Платёжный (суб)агент",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeExcise = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeExcise", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.ExciseFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeExciseLabel", "Подакцизные товары",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeAutonomous = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeAutonomous", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.AutonomousFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeAutonomousLabel", "Автономный режим",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeFFD12 = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeFFD12", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.FFD12Flag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeFFD12Label", "ФФД 1.1, 1.2",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeGambling = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeGambling", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.GamblingLotteryFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeGamblingLabel", "Азартные игры и лотереи",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifePawn = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifePawn", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.PawnInsuranceFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifePawnLabel", "Ломбарды и страхование",
				AndroidSupport.LabelTypes.DefaultLeft);

			fnLifeMarkGoods = AndroidSupport.ApplySwitchSettings (fnLifePage, "FNLifeMarkGoods", false,
				fnLifeFieldBackColor, FnLife13_Toggled, ca.MarkGoodsFlag);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeMarkGoodsLabel", "Маркированные товары",
				AndroidSupport.LabelTypes.DefaultLeft);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "SetDate", "Дата фискализации:",
				AndroidSupport.LabelTypes.DefaultLeft);
			fnLifeStartDate = AndroidSupport.ApplyDatePickerSettings (fnLifePage, "FNLifeStartDate",
				fnLifeFieldBackColor, FnLifeStartDate_DateSelected);

			//
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeResultLabel", "Результат:",
				AndroidSupport.LabelTypes.HeaderLeft);
			fnLifeResult = AndroidSupport.ApplyButtonSettings (fnLifePage, "FNLifeResult", "",
				fnLifeFieldBackColor, FNLifeResultCopy, true);
			AndroidSupport.ApplyLabelSettings (fnLifePage, "FNLifeHelpLabel",
				"Нажатие кнопки копирует дату окончания срока жизни в буфер обмена",
				AndroidSupport.LabelTypes.Tip);

			//
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Clear",
				AndroidSupport.ButtonsDefaultNames.Delete, fnLifeFieldBackColor, FNLifeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (fnLifePage, "Find",
				AndroidSupport.ButtonsDefaultNames.Find, fnLifeFieldBackColor, FNLifeFind_Clicked);

			// Применение всех названий
			FNLifeSerial_TextChanged (null, null);

			#endregion

			#region Страница заводских и рег. номеров

			AndroidSupport.ApplyLabelSettings (rnmPage, "SNLabel", "ЗН или модель ККТ:",
				AndroidSupport.LabelTypes.HeaderLeft);
			rnmKKTSN = AndroidSupport.ApplyEditorSettings (rnmPage, "SN", rnmFieldBackColor, Keyboard.Default,
				kkts.MaxSerialNumberLength, ca.KKTSerial, RNM_TextChanged, true);

			rnmKKTTypeLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "TypeLabel",
				"", AndroidSupport.LabelTypes.DefaultLeft);

			AndroidSupport.ApplyLabelSettings (rnmPage, "INNLabel", "ИНН пользователя:",
				AndroidSupport.LabelTypes.HeaderLeft);
			rnmINN = AndroidSupport.ApplyEditorSettings (rnmPage, "INN", rnmFieldBackColor, Keyboard.Numeric, 12,
				ca.UserINN, RNM_TextChanged, true);

			rnmINNCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "INNCheckLabel", "",
				AndroidSupport.LabelTypes.Semaphore);

			if (ca.AllowExtendedFunctionsLevel2)
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки или произвольное число для генерации:",
					AndroidSupport.LabelTypes.HeaderLeft);
			else
				AndroidSupport.ApplyLabelSettings (rnmPage, "RNMLabel",
					"Регистрационный номер для проверки:", AndroidSupport.LabelTypes.HeaderLeft);

			rnmRNM = AndroidSupport.ApplyEditorSettings (rnmPage, "RNM", rnmFieldBackColor, Keyboard.Numeric, 16,
				ca.RNMKKT, RNM_TextChanged, true);

			rnmRNMCheckLabel = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMCheckLabel", "",
				AndroidSupport.LabelTypes.Semaphore);

			rnmGenerate = AndroidSupport.ApplyButtonSettings (rnmPage, "RNMGenerate",
				AndroidSupport.ButtonsDefaultNames.Create, rnmFieldBackColor, RNMGenerate_Clicked);
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
				AndroidSupport.LabelTypes.Tip);
			rnmTip.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (rnmPage, "Clear",
				AndroidSupport.ButtonsDefaultNames.Delete, rnmFieldBackColor, RNMClear_Clicked);
			AndroidSupport.ApplyButtonSettings (rnmPage, "Find",
				AndroidSupport.ButtonsDefaultNames.Find, rnmFieldBackColor, RNMFind_Clicked);

			rnmSupport105 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport105", "ФФД 1.05",
				AndroidSupport.LabelTypes.Semaphore);
			rnmSupport11 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport11", "ФФД 1.1",
				AndroidSupport.LabelTypes.Semaphore);
			rnmSupport12 = AndroidSupport.ApplyLabelSettings (rnmPage, "RNMSupport12", "ФФД 1.2",
				AndroidSupport.LabelTypes.Semaphore);
			rnmSupport105.Padding = rnmSupport11.Padding = rnmSupport12.Padding = new Thickness (6);

			RNM_TextChanged (null, null);   // Применение значений

			#endregion

			#region Страница настроек ОФД

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDINNLabel", "ИНН ОФД:",
				AndroidSupport.LabelTypes.HeaderLeft);
			ofdINN = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDINN", ca.OFDINN, ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNameLabel", "Название:",
				AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSearchLabel", "Поиск по ИНН или фрагменту названия:",
				AndroidSupport.LabelTypes.HeaderLeft);
			ofdNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDName", "- Выберите или введите ИНН -",
				ofdFieldBackColor, OFDName_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameLabel", "Адрес ОФД:",
				AndroidSupport.LabelTypes.HeaderLeft);
			ofdDNSNameButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSName", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIP", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPort", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameMLabel", "Адрес ИСМ:",
				AndroidSupport.LabelTypes.HeaderLeft);
			ofdDNSNameMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdIPMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPM", "", ofdFieldBackColor,
				Field_Clicked, true);
			ofdPortMButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortM", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDNSNameKLabel", "Адрес ОКП (стандартный):",
				AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDDNSNameK", OFD.OKPSite,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDIPK", OFD.OKPIP,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDPortK", OFD.OKPPort,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDEmailLabel", "E-mail отправителя чеков:", AndroidSupport.LabelTypes.HeaderLeft);
			ofdEmailButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDEmail", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDSiteLabel", "Сайт ОФД:",
				AndroidSupport.LabelTypes.HeaderLeft);
			ofdSiteButton = AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSite", "", ofdFieldBackColor,
				Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDNalogSiteLabel", "Сайт ФНС:", AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDNalogSite", OFD.FNSSite,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDYaDNSLabel", "Яндекс DNS:", AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDYaDNS1", OFD.YandexDNSReq,
				ofdFieldBackColor, Field_Clicked, true);
			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDYaDNS2", OFD.YandexDNSAlt,
				ofdFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (ofdPage, "OFDHelpLabel",
				"Нажатие кнопок копирует их подписи в буфер обмена", AndroidSupport.LabelTypes.Tip);

			AndroidSupport.ApplyButtonSettings (ofdPage, "Clear",
				AndroidSupport.ButtonsDefaultNames.Delete, ofdFieldBackColor, OFDClear_Clicked);

			ofdSearchText = AndroidSupport.ApplyEditorSettings (ofdPage, "OFDSearchText", ofdFieldBackColor,
				Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (ofdPage, "OFDSearchButton",
				AndroidSupport.ButtonsDefaultNames.Find, ofdFieldBackColor, OFD_Find);

			ofdDisabledLabel = AndroidSupport.ApplyLabelSettings (ofdPage, "OFDDisabledLabel",
				"Аннулирован", AndroidSupport.LabelTypes.ErrorTip);
			ofdDisabledLabel.IsVisible = false;

			OFDINN_TextChanged (null, null); // Протягивание значений

			#endregion

			#region Страница TLV-тегов

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVSearchLabel", "Поиск по номеру или фрагменту описания:",
				AndroidSupport.LabelTypes.HeaderLeft);
			tlvTag = AndroidSupport.ApplyEditorSettings (tagsPage, "TLVSearchText", tagsFieldBackColor,
				Keyboard.Default, 20, ca.TLVData, null, true);

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVSearchButton",
				AndroidSupport.ButtonsDefaultNames.Find, tagsFieldBackColor, TLVFind_Clicked);
			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVClearButton",
				AndroidSupport.ButtonsDefaultNames.Delete, tagsFieldBackColor, TLVClear_Clicked);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescriptionLabel", "Описание тега:",
				AndroidSupport.LabelTypes.HeaderLeft);
			tlvDescriptionLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVDescription", "",
				AndroidSupport.LabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVTypeLabel", "Тип тега:",
				AndroidSupport.LabelTypes.HeaderLeft);
			tlvTypeLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVType", "",
				AndroidSupport.LabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValuesLabel", "Возможные значения тега:",
				AndroidSupport.LabelTypes.HeaderLeft);
			tlvValuesLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVValues", "",
				AndroidSupport.LabelTypes.Field, tagsFieldBackColor);

			AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligationLabel", "Обязательность:",
				AndroidSupport.LabelTypes.HeaderLeft);
			tlvObligationLabel = AndroidSupport.ApplyLabelSettings (tagsPage, "TLVObligation", "",
				AndroidSupport.LabelTypes.Field, tagsFieldBackColor);
			tlvObligationLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (tagsPage, "TLVObligationHelpLabel",
				TLVTags.ObligationBase, tagsFieldBackColor, TLVObligationBase_Click, false);

			TLVFind_Clicked (null, null);

			#endregion

			#region Страница команд нижнего уровня

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "ProtocolLabel", "Протокол:",
				AndroidSupport.LabelTypes.HeaderLeft);
			lowLevelProtocol = AndroidSupport.ApplyButtonSettings (lowLevelPage, "ProtocolButton",
				ll.GetProtocolsNames ()[(int)ca.LowLevelProtocol], lowLevelFieldBackColor,
				LowLevelProtocol_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandLabel", "Команда:",
				AndroidSupport.LabelTypes.HeaderLeft);
			lowLevelCommand = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandButton",
				ll.GetCommandsList (ca.LowLevelProtocol)[(int)ca.LowLevelCode],
				lowLevelFieldBackColor, LowLevelCommandCodeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandSearchLabel", "Поиск по описанию команды:",
				AndroidSupport.LabelTypes.HeaderLeft);
			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandCodeLabel", "Код команды:",
				AndroidSupport.LabelTypes.HeaderLeft);
			lowLevelCommandCode = AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandCodeButton",
				ll.GetCommand (ca.LowLevelProtocol, ca.LowLevelCode, false),
				lowLevelFieldBackColor, Field_Clicked, true);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescrLabel", "Пояснение:",
				AndroidSupport.LabelTypes.HeaderLeft);

			lowLevelCommandDescr = AndroidSupport.ApplyLabelSettings (lowLevelPage, "CommandDescr",
				ll.GetCommand (ca.LowLevelProtocol, ca.LowLevelCode, true), AndroidSupport.LabelTypes.Field,
				lowLevelFieldBackColor);

			AndroidSupport.ApplyLabelSettings (lowLevelPage, "LowLevelHelpLabel",
				"Нажатие кнопки копирует команду в буфер обмена", AndroidSupport.LabelTypes.Tip);

			commandSearchText = AndroidSupport.ApplyEditorSettings (lowLevelPage, "CommandSearchText",
				lowLevelFieldBackColor, Keyboard.Default, 30, "", null, true);

			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandSearchButton",
				AndroidSupport.ButtonsDefaultNames.Find, lowLevelFieldBackColor, Command_Find);
			AndroidSupport.ApplyButtonSettings (lowLevelPage, "CommandClearButton",
				AndroidSupport.ButtonsDefaultNames.Delete, lowLevelFieldBackColor, Command_Clear);

			#endregion

			#region Страница штрих-кодов

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeFieldLabel", "Данные штрих-кода:",
				AndroidSupport.LabelTypes.HeaderLeft);
			barcodeField = AndroidSupport.ApplyEditorSettings (barCodesPage, "BarcodeField",
				barCodesFieldBackColor, Keyboard.Default, BarCodes.MaxSupportedDataLength,
				ca.BarcodeData, BarcodeText_TextChanged, true);

			AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescriptionLabel",
				"Описание штрих-кода:", AndroidSupport.LabelTypes.HeaderLeft);
			barcodeDescriptionLabel = AndroidSupport.ApplyLabelSettings (barCodesPage, "BarcodeDescription", "",
				AndroidSupport.LabelTypes.Field, barCodesFieldBackColor);

			AndroidSupport.ApplyButtonSettings (barCodesPage, "Clear",
				AndroidSupport.ButtonsDefaultNames.Delete, barCodesFieldBackColor, BarcodeClear_Clicked);
			AndroidSupport.ApplyButtonSettings (barCodesPage, "GetFromClipboard",
				AndroidSupport.ButtonsDefaultNames.Copy, barCodesFieldBackColor, BarcodeGet_Clicked);

			BarcodeText_TextChanged (null, null);

			#endregion

			#region Страница распиновок

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLabel", "Тип кабеля:",
				AndroidSupport.LabelTypes.HeaderLeft);
			cableTypeButton = AndroidSupport.ApplyButtonSettings (connectorsPage, "CableTypeButton",
				conn.GetCablesNames ()[(int)ca.CableType], connectorsFieldBackColor, CableTypeButton_Clicked, true);

			AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescriptionLabel", "Сопоставление контактов:",
				AndroidSupport.LabelTypes.HeaderLeft);
			cableLeftSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftSide",
				" ", AndroidSupport.LabelTypes.DefaultLeft);
			cableLeftSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableLeftSideText.HorizontalOptions = LayoutOptions.Center;

			cableLeftPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableLeftPins", " ",
				AndroidSupport.LabelTypes.FieldMonotype, connectorsFieldBackColor);

			cableRightSideText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightSide",
				" ", AndroidSupport.LabelTypes.DefaultLeft);
			cableRightSideText.HorizontalTextAlignment = TextAlignment.Center;
			cableRightSideText.HorizontalOptions = LayoutOptions.Center;

			cableRightPinsText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableRightPins", " ",
				AndroidSupport.LabelTypes.FieldMonotype, connectorsFieldBackColor);
			cableLeftPinsText.HorizontalTextAlignment = cableRightPinsText.HorizontalTextAlignment =
				TextAlignment.Center;

			cableDescriptionText = AndroidSupport.ApplyLabelSettings (connectorsPage, "CableDescription",
				" ", AndroidSupport.LabelTypes.Tip);

			CableTypeButton_Clicked (null, null);

			#endregion

			#region Страница конвертора систем счисления

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberLabel",
				"Число:", AndroidSupport.LabelTypes.HeaderLeft);
			convNumberField = AndroidSupport.ApplyEditorSettings (convertorsSNPage, "ConvNumberField",
				convertorsFieldBackColor, Keyboard.Default, 10, ca.ConversionNumber, ConvNumber_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberInc",
				AndroidSupport.ButtonsDefaultNames.Increase, convertorsFieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberDec",
				AndroidSupport.ButtonsDefaultNames.Decrease, convertorsFieldBackColor, ConvNumberAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsSNPage, "ConvNumberClear",
				AndroidSupport.ButtonsDefaultNames.Delete, convertorsFieldBackColor, ConvNumberClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultLabel", "Представление:",
				AndroidSupport.LabelTypes.HeaderLeft);
			convNumberResultField = AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvNumberResultField",
				" ", AndroidSupport.LabelTypes.FieldMonotype, convertorsFieldBackColor);
			ConvNumber_TextChanged (null, null);

			const string convHelp = "Шестнадцатеричные числа следует начинать с символов “0x”";
			AndroidSupport.ApplyLabelSettings (convertorsSNPage, "ConvHelpLabel", convHelp,
				AndroidSupport.LabelTypes.Tip);

			#endregion

			#region Страница конвертора символов Unicode

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeLabel",
				"Код символа\nили символ:", AndroidSupport.LabelTypes.HeaderLeft);
			convCodeField = AndroidSupport.ApplyEditorSettings (convertorsUCPage, "ConvCodeField",
				convertorsFieldBackColor, Keyboard.Default, 10, ca.ConversionCode, ConvCode_TextChanged, true);

			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeInc",
				AndroidSupport.ButtonsDefaultNames.Increase, convertorsFieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeDec",
				AndroidSupport.ButtonsDefaultNames.Decrease, convertorsFieldBackColor, ConvCodeAdd_Click);
			AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeClear",
				AndroidSupport.ButtonsDefaultNames.Delete, convertorsFieldBackColor, ConvCodeClear_Click);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultLabel", "Символ Unicode:",
				AndroidSupport.LabelTypes.HeaderLeft);
			convCodeResultField = AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvCodeResultField",
				"", AndroidSupport.LabelTypes.FieldMonotype, convertorsFieldBackColor);

			convCodeSymbolField = AndroidSupport.ApplyButtonSettings (convertorsUCPage, "ConvCodeSymbolField",
				" ", convertorsFieldBackColor, CopyCharacter_Click, true);
			convCodeSymbolField.FontSize *= 5;
			ConvCode_TextChanged (null, null);

			AndroidSupport.ApplyLabelSettings (convertorsUCPage, "ConvHelpLabel",
				convHelp + ".\nНажатие кнопки с символом Unicode копирует его в буфер обмена",
				AndroidSupport.LabelTypes.Tip);

			#endregion

			#region Страница конвертора двоичных данных

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "HexLabel",
				"Данные (hex):", AndroidSupport.LabelTypes.HeaderLeft);
			convHexField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "HexField",
				convertorsFieldBackColor, Keyboard.Default, 500, ca.ConversionHex, null, true);
			convHexField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "HexToTextButton",
				AndroidSupport.ButtonsDefaultNames.Down, convertorsFieldBackColor, ConvertHexToText_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "TextToHexButton",
				AndroidSupport.ButtonsDefaultNames.Up, convertorsFieldBackColor, ConvertTextToHex_Click);
			AndroidSupport.ApplyButtonSettings (convertorsHTPage, "ClearButton",
				AndroidSupport.ButtonsDefaultNames.Delete, convertorsFieldBackColor, ClearConvertText_Click);

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "TextLabel",
				"Данные (текст):", AndroidSupport.LabelTypes.HeaderLeft);
			convTextField = AndroidSupport.ApplyEditorSettings (convertorsHTPage, "TextField",
				convertorsFieldBackColor, Keyboard.Default, 250, ca.ConversionText, null, true);
			convTextField.HorizontalOptions = LayoutOptions.Fill;

			AndroidSupport.ApplyLabelSettings (convertorsHTPage, "EncodingLabel",
				"Кодировка:", AndroidSupport.LabelTypes.HeaderLeft);
			encodingButton = AndroidSupport.ApplyButtonSettings (convertorsHTPage, "EncodingButton",
				" ", convertorsFieldBackColor, EncodingButton_Clicked, true);
			EncodingButton_Clicked (null, null);

			#endregion

			// Обязательное принятие Политики и EULA
			AcceptPolicy ();
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
		private async void AcceptPolicy ()
			{
			// Контроль XPR
			while (!Localization.IsXPUNClassAcceptable)
				await AndroidSupport.ShowMessage (Localization.InacceptableXPUNClassMessage, "   ");

			// Политика
			if (RDGenerics.GetAppSettingsValue (firstStartRegKey) != "")
				return;

			while (!await AndroidSupport.ShowMessage
				(AndroidSupport.PolicyAcceptionMessage,
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Accept),
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Read)))
				{
				ADPButton_Clicked (null, null);
				}

			// Вступление
			RDGenerics.SetAppSettingsValue (firstStartRegKey, ProgramDescription.AssemblyVersion); // Только после принятия

			await AndroidSupport.ShowMessage ("Вас приветствует инструмент сервис-инженера ККТ (54-ФЗ)!\r\n\r\n" +

				"На этой странице находится перечень функций приложения, который позволяет перейти " +
				"к нужному разделу. Вернуться сюда можно с помощью кнопки «Назад». Перемещение " +
				"между разделами также доступно по свайпу влево-вправо",

#if CHECK_EXTF
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Next));

			await AndroidSupport.ShowMessage ("Часть функций скрыта от рядовых пользователей. Чтобы открыть " +
				"расширенный функционал для специалистов, перейдите на страницу «О приложении» и ответьте " +
				"на несколько простых вопросов. Вопросы для расширенного и полного набора опций отличаются",
#endif

				Localization.GetDefaultButtonName (Localization.DefaultButtons.OK));
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
			Toast.MakeText (Android.App.Application.Context, "Скопировано в буфер обмена",
				ToastLength.Long).Show ();
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
			ca.GenericTaxFlag = !fnLifeGenericTax.IsToggled;
			ca.GoodsFlag = !fnLifeGoods.IsToggled;
			ca.SeasonFlag = fnLifeSeason.IsToggled;
			ca.AgentsFlag = fnLifeAgents.IsToggled;
			ca.ExciseFlag = fnLifeExcise.IsToggled;
			ca.AutonomousFlag = fnLifeAutonomous.IsToggled;
			ca.FFD12Flag = fnLifeFFD12.IsToggled;
			ca.GamblingLotteryFlag = fnLifeGambling.IsToggled;
			ca.PawnInsuranceFlag = fnLifePawn.IsToggled;
			ca.MarkGoodsFlag = fnLifeMarkGoods.IsToggled;

			ca.KKTSerial = rnmKKTSN.Text;
			ca.UserINN = rnmINN.Text;
			ca.RNMKKT = rnmRNM.Text;

			ca.OFDINN = ofdINN.Text;

			//ca.LowLevelProtocol	// -||-
			//ca.LowLevelCode		// -||-

			//ca.KKTForCodes	// -||-
			ca.CodesText = codesSourceText.Text;

			ca.BarcodeData = barcodeField.Text;
			//ca.CableType		// -||-

			ca.TLVData = tlvTag.Text;

			ca.ConversionNumber = convNumberField.Text;
			ca.ConversionCode = convCodeField.Text;
			ca.ConversionHex = convHexField.Text;
			ca.ConversionText = convTextField.Text;
			//ca.EncodingForConvertor	// -||-
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
			List<string> list = kkme.GetKKTTypeNames ();

			// Установка модели
			int res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);
			if (res < 0)
				return;

			errorsKKTButton.Text = list[res];
			ca.KKTForErrors = (uint)res;
			Errors_Find (null, null);
			}

		// Поиск по тексту ошибки
		private void Errors_Find (object sender, EventArgs e)
			{
			List<string> codes = kkme.GetErrorCodesList (ca.KKTForErrors);
			string text = errorSearchText.Text.ToLower ();

			lastErrorSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				{
				int j = (i + lastErrorSearchOffset) % codes.Count;
				string code = codes[j].ToLower ();
				string res = kkme.GetErrorText (ca.KKTForErrors, (uint)j);

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
			kktCodesResultText.Text = kkmc.DecodeText (ca.KKTForCodes, codesSourceText.Text);
			kktCodesErrorLabel.IsVisible = kktCodesResultText.Text.Contains (KKTCodes.EmptyCode);
			kktCodesSourceTextLabel.Text = "Исходный текст (" + codesSourceText.Text.Length.ToString () + "):";
			}

		// Выбор модели ККТ
		private async void CodesKKTButton_Clicked (object sender, EventArgs e)
			{
			// Запрос модели ККТ
			List<string> list = kkmc.GetKKTTypeNames ();

			// Установка модели
			int res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);
			if (res < 0)
				return;

			kktCodesKKTButton.Text = list[res];
			ca.KKTForCodes = (uint)res;
			kktCodesHelpLabel.Text = kkmc.GetKKTTypeDescription (ca.KKTForCodes);

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
			codesSourceText.Text = await RDGenerics.GetFromClipboard ();
			}

		#endregion

		#region Команды нижнего уровня

		// Выбор команды нижнего уровня
		private async void LowLevelCommandCodeButton_Clicked (object sender, EventArgs e)
			{
			// Запрос кода ошибки
			List<string> list = ll.GetCommandsList (ca.LowLevelProtocol);
			int res = 0;
			if (e != null)
				res = await AndroidSupport.ShowList ("Выберите команду:",
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);

			// Установка результата
			if ((e == null) || (res >= 0))
				{
				ca.LowLevelCode = (uint)res;
				lowLevelCommand.Text = list[res];

				lowLevelCommandCode.Text = ll.GetCommand (ca.LowLevelProtocol, (uint)res, false);
				lowLevelCommandDescr.Text = ll.GetCommand (ca.LowLevelProtocol, (uint)res, true);
				}

			list.Clear ();
			}

		// Выбор списка команд
		private async void LowLevelProtocol_Clicked (object sender, EventArgs e)
			{
			// Запрос кода ошибки
			List<string> list = ll.GetProtocolsNames ();

			// Установка результата
			int res = await AndroidSupport.ShowList ("Выберите протокол:",
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);
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
			List<string> codes = ll.GetCommandsList (ca.LowLevelProtocol);
			string text = commandSearchText.Text.ToLower ();

			lastCommandSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastCommandSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastCommandSearchOffset = (i + lastCommandSearchOffset) % codes.Count;

					lowLevelCommand.Text = codes[lastCommandSearchOffset];
					ca.LowLevelCode = (uint)lastCommandSearchOffset;

					lowLevelCommandCode.Text = ll.GetCommand (ca.LowLevelProtocol,
						(uint)lastCommandSearchOffset, false);
					lowLevelCommandDescr.Text = ll.GetCommand (ca.LowLevelProtocol,
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
			if (fnLifeSerial.Text != "")
				fnLifeModelLabel.Text = fns.GetFNName (fnLifeSerial.Text);
			else
				fnLifeModelLabel.Text = "(введите ЗН ФН)";

			// Определение длины ключа
			if (fnLifeModelLabel.Text.Contains ("(13)") || fnLifeModelLabel.Text.Contains ("(15)"))
				{
				fnLife13.IsToggled = false;
				fnLife13.IsVisible = fnLife13.IsEnabled = false;
				}
			else if (fnLifeModelLabel.Text.Contains ("(36)"))
				{
				fnLife13.IsToggled = true;
				fnLife13.IsVisible = fnLife13.IsEnabled = false;
				}
			else
				{
				fnLife13.IsVisible = fnLife13.IsEnabled = true;
				}

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
			fnlf.FN15 = !fnLife13.IsToggled;
			fnlf.FNExactly13 = fnLifeModelLabel.Text.Contains ("(13)");
			fnlf.GenericTax = !fnLifeGenericTax.IsToggled;
			fnlf.Goods = !fnLifeGoods.IsToggled;
			fnlf.Season = fnLifeSeason.IsToggled;
			fnlf.Agents = fnLifeAgents.IsToggled;
			fnlf.Excise = fnLifeExcise.IsToggled;
			fnlf.Autonomous = fnLifeAutonomous.IsToggled;
			fnlf.FFD12 = fnLifeFFD12.IsToggled;
			fnlf.GamblingAndLotteries = fnLifeGambling.IsToggled;
			fnlf.PawnsAndInsurance = fnLifePawn.IsToggled;
			fnlf.MarkGoods = fnLifeMarkGoods.IsToggled;

			fnlf.MarkFN = fnLife13.IsEnabled || fns.IsFNCompatibleWithFFD12 (fnLifeSerial.Text);
			// Признак распознанного ЗН ФН

			string res = KKTSupport.GetFNLifeEndDate (fnLifeStartDate.Date, fnlf);

			fnLifeResult.Text = "ФН прекратит работу ";
			if (res.Contains (KKTSupport.FNLifeInacceptableSign))
				{
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Unsupported);
				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate + "\n(выбранный ФН неприменим с указанными параметрами)");
				}
			else if (res.Contains (KKTSupport.FNLifeUnwelcomeSign))
				{
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Planned);
				fnLifeResultDate = res.Substring (1);
				fnLifeResult.Text += (fnLifeResultDate +
					"\n(не рекомендуется использовать выбранный ФН с указанными параметрами)");
				}
			else
				{
				fnLifeResult.BackgroundColor = StatusToColor (KKTSerial.FFDSupportStatuses.Supported);
				fnLifeResultDate = res;
				fnLifeResult.Text += res;
				}

			if (!fnLife13.IsEnabled) // Признак корректно заданного ЗН ФН
				{
				if (!fnlf.MarkFN)
					{
					fnLifeResult.TextColor = AndroidSupport.DefaultErrorColor;

					fnLifeResult.Text += ("\n(выбранный ФН исключён из реестра ФНС)");
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
			string sig = fns.FindSignatureByName (fnLifeSerial.Text);
			if (sig != "")
				fnLifeSerial.Text = sig;
			}

		#endregion

		#region РНМ и ЗН

		// Изменение ИНН ОФД и РН ККТ
		private void RNM_TextChanged (object sender, TextChangedEventArgs e)
			{
			// ЗН ККТ
			if (rnmKKTSN.Text != "")
				{
				rnmKKTTypeLabel.Text = kkts.GetKKTModel (rnmKKTSN.Text);

				KKTSerial.FFDSupportStatuses[] statuses = kkts.GetFFDSupportStatus (rnmKKTSN.Text);
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
			rnmINNCheckLabel.Text = kkts.GetRegionName (rnmINN.Text);
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
			string sig = kkts.FindSignatureByName (rnmKKTSN.Text);
			if (sig != "")
				rnmKKTSN.Text = sig;
			}

		#endregion

		#region ОФД

		// Ввод названия или ИНН ОФД
		private void OFDINN_TextChanged (object sender, TextChangedEventArgs e)
			{
			List<string> parameters = ofd.GetOFDParameters (ofdINN.Text);

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
			List<string> list = ofd.GetOFDNames ();

			// Установка результата
			int res = await AndroidSupport.ShowList ("Выберите название ОФД:",
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);
			if (res < 0)
				return;

			ofdNameButton.Text = list[res];
			SendToClipboard (list[res].Replace ('«', '\"').Replace ('»', '\"'));

			string s = ofd.GetOFDINNByName (ofdNameButton.Text);
			if (s != "")
				{
				ofdINN.Text = s;
				OFDINN_TextChanged (null, null);
				}
			}

		// Поиск по названию ОФД
		private void OFD_Find (object sender, EventArgs e)
			{
			List<string> codes = ofd.GetOFDNames ();
			codes.AddRange (ofd.GetOFDINNs ());

			string text = ofdSearchText.Text.ToLower ();

			lastOFDSearchOffset++;
			for (int i = 0; i < codes.Count; i++)
				if (codes[(i + lastOFDSearchOffset) % codes.Count].ToLower ().Contains (text))
					{
					lastOFDSearchOffset = (i + lastOFDSearchOffset) % codes.Count;
					ofdNameButton.Text = codes[lastOFDSearchOffset % (codes.Count / 2)];

					string s = ofd.GetOFDINNByName (ofdNameButton.Text);
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

		// Страница проекта
		private async void AppButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (RDGenerics.AssemblyGitLink + ProgramDescription.AssemblyMainName);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (false),
					ToastLength.Long).Show ();
				}
			}

		// Страница политики и EULA
		private async void ADPButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (RDGenerics.GetADPLink (true));
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (false),
					ToastLength.Long).Show ();
				}
			}

		// Видеоруководство пользователя
		private async void ManualButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				await Launcher.OpenAsync (KKTSupport.FNReaderLink);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (false),
					ToastLength.Long).Show ();
				}
			}

		// Страница лаборатории
		private async void CommunityButton_Clicked (object sender, EventArgs e)
			{
			List<string> comm = new List<string> (RDGenerics.GetCommunitiesNames (false));

			int res = await AndroidSupport.ShowList ("Выберите сообщество",
				Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), comm);
			if (res < 0)
				return;

			string link = RDGenerics.GetCommunityLink (comm[res], false);

			if (string.IsNullOrWhiteSpace (link))
				return;

			try
				{
				await Launcher.OpenAsync (link);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (false),
					ToastLength.Long).Show ();
				}
			}

		// Страница политики и EULA
		private async void DevButton_Clicked (object sender, EventArgs e)
			{
			try
				{
				EmailMessage message = new EmailMessage
					{
					Subject = "Wish, advice or bug in " + ProgramDescription.AssemblyTitle,
					Body = "",
					To = new List<string> () { RDGenerics.LabMailLink }
					};
				await Email.ComposeAsync (message);
				}
			catch
				{
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (true),
					ToastLength.Long).Show ();
				}
			}

		// Разблокировка расширенного функционала
		private void UnlockMethod (object sender, TextChangedEventArgs e)
			{
			if (ca.TestPass (unlockField.Text))
				{
				unlockLabel.Text = ca.LockMessage;
				if (!ca.AllowExtendedFunctionsLevel2)
					{
					unlockField.Text = "";
					}
				else
					{
					unlockField.IsEnabled = false;
					unlockLabel.HorizontalTextAlignment = TextAlignment.Center;
					}
				}
			}

		#endregion

		#region Руководства пользователей

		// Выбор модели ККТ
		private async void UserManualsKKTButton_Clicked (object sender, EventArgs e)
			{
			int res = (int)ca.KKTForManuals;
			List<string> list = um.GetKKTList ();

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите модель ККТ:",
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);

				// Установка модели
				if (res < 0)
					return;
				}

			userManualsKKTButton.Text = list[res];
			ca.KKTForManuals = (uint)res;

			for (int i = 0; i < operationTextLabels.Count; i++)
				operationTextLabels[i].Text = um.GetManual ((uint)res, (uint)i);
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
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), modes);
				if (res < 0)
					return;
				}

			// Печать
			string text = KKTSupport.BuildUserManual (um, ca.KKTForManuals, res == 0);
			KKTSupport.PrintManual (text, pm, res == 0);
			}

		#endregion

		#region Теги TLV

		// Поиск тега
		private void TLVFind_Clicked (object sender, EventArgs e)
			{
			if (tlvt.FindTag (tlvTag.Text))
				{
				tlvDescriptionLabel.Text = tlvt.LastDescription;
				tlvTypeLabel.Text = tlvt.LastType;
				tlvValuesLabel.Text = tlvt.LastValuesSet;
				tlvObligationLabel.Text = tlvt.LastObligation;
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
				Toast.MakeText (Android.App.Application.Context, AndroidSupport.GetNoRequiredAppMessage (false),
					ToastLength.Long).Show ();
				}
			}

		#endregion

		#region Штрих-коды

		// Ввод данных штрих-кода
		private void BarcodeText_TextChanged (object sender, TextChangedEventArgs e)
			{
			barcodeField.Text = BarCodes.ConvertFromRussianKeyboard (barcodeField.Text);

			barcodeDescriptionLabel.Text = barc.GetBarcodeDescription (barcodeField.Text);
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
			List<string> list = conn.GetCablesNames ();

			if (sender != null)
				{
				// Запрос модели ККТ
				res = await AndroidSupport.ShowList ("Выберите тип кабеля:",
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);

				// Установка типа кабеля
				if (res < 0)
					return;
				}

			// Установка полей
			cableTypeButton.Text = list[res];
			ca.CableType = (uint)res;

			cableLeftSideText.Text = "Со стороны " + conn.GetCableConnector ((uint)res, false);
			cableLeftPinsText.Text = conn.GetCableConnectorPins ((uint)res, false);

			cableRightSideText.Text = "Со стороны " + conn.GetCableConnector ((uint)res, true);
			cableRightPinsText.Text = conn.GetCableConnectorPins ((uint)res, true);

			cableDescriptionText.Text = conn.GetCableConnectorDescription ((uint)res, false) + "\n\n" +
				conn.GetCableConnectorDescription ((uint)res, true);
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
				AndroidSupport.ButtonsDefaultNames.Increase);

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
				AndroidSupport.ButtonsDefaultNames.Increase);

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
				(DataConvertors.ConvertHTModes)(ca.EncodingForConvertor % DataConvertors.UniqueEncodingsCount),
				ca.EncodingForConvertor >= DataConvertors.UniqueEncodingsCount);
			}

		// Преобразование текста в hex-данные
		private void ConvertTextToHex_Click (object sender, EventArgs e)
			{
			convHexField.Text = DataConvertors.ConvertTextToHex (convTextField.Text,
				(DataConvertors.ConvertHTModes)(ca.EncodingForConvertor % DataConvertors.UniqueEncodingsCount),
				ca.EncodingForConvertor >= DataConvertors.UniqueEncodingsCount);
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
					Localization.GetDefaultButtonName (Localization.DefaultButtons.Cancel), list);

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
