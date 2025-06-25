using System;

namespace RD_AAOW
	{
	/// <summary>
	/// Поддерживаемые версии ФФД
	/// </summary>
	public enum FFDVersions
		{
		/// <summary>
		/// ФФД 1.0 альфа
		/// </summary>
		FFD_1_0_a = 0,

		/// <summary>
		/// ФФД 1.0 бета
		/// </summary>
		FFD_1_0_b = 1,

		/// <summary>
		/// ФФД 1.05
		/// </summary>
		FFD_1_05 = 2,

		/// <summary>
		/// ФФД 1.1
		/// </summary>
		FFD_1_1 = 3,

		/// <summary>
		/// ФФД 1.2
		/// </summary>
		FFD_1_2 = 4,

		/// <summary>
		/// Неизвестная или неподдерживаемая версия
		/// </summary>
		Unknown = 255
		}

	/// <summary>
	/// Возможные типы фискальных документов
	/// </summary>
	public enum FNDocumentTypes
		{
		/// <summary>
		/// Отчёт о регистрации
		/// </summary>
		Registration = 1,

		/// <summary>
		/// Отчёт об изменении реквизитов регистрации
		/// </summary>
		RegistrationChange = 11,

		/// <summary>
		/// Открытие смены
		/// </summary>
		OpenSession = 2,

		/// <summary>
		/// Отчёт о текущем состоянии расчётов
		/// </summary>
		CurrentState = 0x15,

		/// <summary>
		/// Кассовый чек
		/// </summary>
		Bill = 0x03,

		/// <summary>
		/// Кассовый чек для ФФД 1.1
		/// </summary>
		Bill_11 = 0x83,

		/// <summary>
		/// Кассовый чек для ФФД 1.2
		/// </summary>
		Bill_12 = 0xC3,

		/// <summary>
		/// Чек коррекции
		/// </summary>
		CorrectionBill = 0x1F,

		/// <summary>
		/// Чек коррекции для ФФД 1.1
		/// </summary>
		CorrectionBill_11 = 0x9F,

		/// <summary>
		/// Чек коррекции для ФФД 1.2
		/// </summary>
		CorrectionBill_12 = 0xDF,

		/// <summary>
		/// БСО
		/// </summary>
		Blank = 0x04,

		/// <summary>
		/// БСО для ФФД 1.1
		/// </summary>
		Blank_11 = 0x84,

		/// <summary>
		/// БСО для ФФД 1.2
		/// </summary>
		Blank_12 = 0xC4,

		/// <summary>
		/// БСО коррекции
		/// </summary>
		CorrectionBlank = 0x29,

		/// <summary>
		/// БСО коррекции для ФФД 1.1
		/// </summary>
		CorrectionBlank_11 = 0xA9,

		/// <summary>
		/// БСО коррекции для ФФД 1.2
		/// </summary>
		CorrectionBlank_12 = 0xE9,

		/// <summary>
		/// Закрытие смены
		/// </summary>
		CloseSession = 5,

		/// <summary>
		/// Закрытие фискального режима
		/// </summary>
		CloseFiscalStorage = 6,

		/// <summary>
		/// Подтверждение оператора
		/// </summary>
		Confirmation = 7,

		/// <summary>
		/// Запрос о коде маркировки
		/// </summary>
		MarkCodeRequest = 81,

		/// <summary>
		/// Уведомление о реализации кода маркировки
		/// </summary>
		MarkCodeNotification = 82,

		/// <summary>
		/// Ответ на запрос о коде маркировки
		/// </summary>
		MarkCodeRequestAnswer = 83,

		/// <summary>
		/// Ответ на уведомление о реализации кода маркировки
		/// </summary>
		MarkCodeNotificationAnswer = 84,

		/// <summary>
		/// Неизвестный тип документа
		/// </summary>
		UnknownType = 255
		}

	/// <summary>
	/// Форматы дампов ФН
	/// </summary>
	public enum FNDumpTypes
		{
		/// <summary>
		/// Бинарный формат
		/// </summary>
		Binary = 2,

		/// <summary>
		/// Табличный формат
		/// </summary>
		CSV = 1
		}

	/// <summary>
	/// Возможные кассовые операции
	/// </summary>
	public enum FNOperationTypes
		{
		/// <summary>
		/// Приход
		/// </summary>
		Incoming = 1,

		/// <summary>
		/// Возврат прихода
		/// </summary>
		ReverseIncoming = 2,

		/// <summary>
		/// Расход
		/// </summary>
		Outcoming = 3,

		/// <summary>
		/// Возврат расхода
		/// </summary>
		ReverseOutcoming = 4,

		/// <summary>
		/// Неизвестная операция
		/// </summary>
		UnknownType = 255
		}

	/// <summary>
	/// Возможные режимы считывания данных ФН
	/// </summary>
	public enum FNReadingTypes
		{
		/// <summary>
		/// COM-порт
		/// </summary>
		COM = 0,

		/// <summary>
		/// Внутренний дамп ФН для приложения
		/// </summary>
		FSD = 1,

		/// <summary>
		/// Файл FNC
		/// </summary>
		FNC = 2
		}

	/// <summary>
	/// Режимы работы методов обмена с ОФД
	/// </summary>
	public enum OFDModes
		{
		/// <summary>
		/// Фискальные документы
		/// </summary>
		Documents = 0,

		/// <summary>
		/// Коды маркировки
		/// </summary>
		Marks = 1,

		/// <summary>
		/// Ключи проверки кодов маркировки
		/// </summary>
		Keys = 2
		}

	/// <summary>
	/// Доступные типы принтеров
	/// </summary>
	public enum PrinterTypes
		{
		/// <summary>
		/// Стандартный для А4
		/// </summary>
		DefaultA4 = 0,

		/// <summary>
		/// Чековый для ленты 80 мм
		/// </summary>
		Receipt80mm = 1,

		/// <summary>
		/// Чековый для ленты 80 мм, тонкий шрифт
		/// </summary>
		Receipt80mmThin = 11,

		/// <summary>
		/// Чековый для ленты 57 мм
		/// </summary>
		Receipt57mm = 2,

		/// <summary>
		/// Чековый для ленты 57 мм, тонкий шрифт
		/// </summary>
		Receipt57mmThin = 12,

		/// <summary>
		/// Дополнительный для А4 с настройками под руководства пользователя
		/// </summary>
		ManualA4 = 3,

		/// <summary>
		/// Дополнительный для А4 с настройками под заявления
		/// </summary>
		BlankA4 = 4,
		}

	/// <summary>
	/// Доступные параметры работы принтера
	/// </summary>
	public enum PrinterFlags
		{
		/// <summary>
		/// Без специальных настроек
		/// </summary>
		None = 0x00,

		/// <summary>
		/// Разрешение на использование сохранённых настроек принтера
		/// </summary>
		AllowSavedPrinterSettings = 0x01,

		/// <summary>
		/// Отмена запроса принтера при печати (использование сохранённых настроек)
		/// </summary>
		DontRequestPrinter = 0x02,
		}

	/// <summary>
	/// Доступные уровни детализации
	/// </summary>
	public enum DetailsLevels
		{
		/// <summary>
		/// Уровень не задан
		/// </summary>
		NotDefined = 3,

		/// <summary>
		/// Только основные данные
		/// </summary>
		Minimum = 0,

		/// <summary>
		/// Все доступные данные (включая текстовые)
		/// </summary>
		AvailableTLV = 1,

		/// <summary>
		/// Все доступные данные + неизвестные теги в HEX-формате
		/// </summary>
		Maximum = 2,
		}

	/// <summary>
	/// Доступные варианты инициализации интерфейса данных
	/// </summary>
	public enum DataInterfaceModes
		{
		/// <summary>
		/// Не инициализирован
		/// </summary>
		NotInited = -1,

		/// <summary>
		/// COM-порт (физический ФН)
		/// </summary>
		COM = 0,

		/// <summary>
		/// Файл FSD
		/// </summary>
		FSD = 1,

		/// <summary>
		/// Выгрузка FNC
		/// </summary>
		FNC = 2
		}

	/// <summary>
	/// Список параметров для определения срока жизни ФН
	/// </summary>
	public enum FNLifeFlags
		{
		/// <summary>
		/// Флаг указывает на выбор ФН на 13/15 месяцев вместо 36
		/// </summary>
		FN15 = 0x00001,

		/// <summary>
		/// Флаг указывает на выбор ФН на 13 месяцев вместо 15
		/// </summary>
		FNExactly13 = 0x00002,

		/// <summary>
		/// Флаг указыввает на применение ОСН
		/// </summary>
		GenericTax = 0x00010,

		/// <summary>
		/// Флаг указывает на режим товаров вместо услуг
		/// </summary>
		Goods = 0x00020,

		/// <summary>
		/// Флаг указывает на сезонный режим работы
		/// </summary>
		Season = 0x00040,

		/// <summary>
		/// Флаг указывает на агентскую схему работы
		/// </summary>
		Agents = 0x0080,

		/// <summary>
		/// Флаг указывает на наличие подакцизных товаров
		/// </summary>
		Excise = 0x00100,

		/// <summary>
		/// Флаг указывает на работу без передачи данных
		/// </summary>
		Autonomous = 0x00200,

		/// <summary>
		/// Флаг ФФД 1.2
		/// </summary>
		FFD12 = 0x00400,

		/// <summary>
		/// Флаг ФН с любой поддержкой ФФД 1.2
		/// </summary>
		FFD12Compatible = 0x00800,

		/// <summary>
		/// Флаг торговли маркированными товарами
		/// </summary>
		MarkGoods = 0x01000,

		/// <summary>
		/// Флаг азартных игр и лотерей
		/// </summary>
		GamblingAndLotteries = 0x02000,

		/// <summary>
		/// Флаг ломбардной деятельности и страхования
		/// </summary>
		PawnsAndInsurance = 0x04000,

		/// <summary>
		/// Флаг ФН с полной поддержкой ФФД 1.2
		/// </summary>
		FFD12FullSupport = 0x08000,
		}

	/// <summary>
	/// Доступные настроечные флаги для руководств пользователя
	/// </summary>
	public enum UserGuidesFlags
		{
		/// <summary>
		/// Кассиры работают с паролями
		/// </summary>
		CashiersHavePasswords = 0x0001,

		/// <summary>
		/// В чеках может быть более одной позиции
		/// </summary>
		MoreThanOneItemPerDocument = 0x0002,

		/// <summary>
		/// Номенклатура содержит цены
		/// </summary>
		ProductBaseContainsPrices = 0x0004,

		/// <summary>
		/// Номенклатура содержит услуги
		/// </summary>
		ProductBaseContainsServices = 0x0008,

		/// <summary>
		/// Номенклатура содержит маркированные товары
		/// </summary>
		DocumentsContainMarks = 0x0010,

		/// <summary>
		/// Номенклатура содержит единственное наименование
		/// </summary>
		BaseContainsSingleItem = 0x0020,

#if !ANDROID

		/// <summary>
		/// Необходимо добавить пользовательский логотип в печатное руководство
		/// </summary>
		AddManualLogo = 0x0040,

#endif

		/// <summary>
		/// Руководство для кассира
		/// </summary>
		GuideForCashier = 0x8000,
		}

	/// <summary>
	/// Доступные результаты оценки применимости ФН с указанными параметрами регистрации и сроком жизни
	/// </summary>
	public enum FNLifeStatus
		{
		/// <summary>
		/// ФН может быть использован с указанными параметрами
		/// </summary>
		Acceptable,

		/// <summary>
		/// ФН может быть использован, но не рекомендуется к применению с указанными параметрами
		/// </summary>
		Unwelcome,

		/// <summary>
		/// ФН не поддерживает использование с указанными параметрами
		/// </summary>
		Inacceptable,

		/// <summary>
		/// ФН допускает использование с указанными параметрами, но данная информация не проверена на практике
		/// </summary>
		StronglyUnwelcome
		}

	/// <summary>
	/// Доступные теги регистрации из статуса ФН
	/// </summary>
	public enum RegTags
		{
		/// <summary>
		/// Адрес расчётов
		/// </summary>
		RegistrationAddress = 1009,

		/// <summary>
		/// Заводской номер ККТ
		/// </summary>
		KKTSerialNumber = 1013,

		/// <summary>
		/// ИНН ОФД
		/// </summary>
		OFDINN = 1017,

		/// <summary>
		/// ИНН пользователя
		/// </summary>
		UserINN = 1018,

		/// <summary>
		/// Регистрационный номер
		/// </summary>
		RegistrationNumber = 1037,

		/// <summary>
		/// Заводской номер ФН
		/// </summary>
		FNSerialNumber = 1041,

		/// <summary>
		/// Наименование пользователя
		/// </summary>
		UserName = 1048,

		/// <summary>
		/// Место расчётов
		/// </summary>
		RegistrationPlace = 1187,

		/// <summary>
		/// Версия ФФД
		/// </summary>
		FFDVersion = 1209,

		/// <summary>
		/// Псевдотег – признак автономного режима
		/// </summary>
		AutonomousFlag = 9001,

		/// <summary>
		/// Псевдотег – признак режима услуг
		/// </summary>
		ServiceFlag = 9002,

		/// <summary>
		/// Псевдотег – признак торговли подакцизными товарами
		/// </summary>
		ExciseFlag = 9003,

		/// <summary>
		/// Псевдотег – признак торговли маркированными товарами
		/// </summary>
		MarkingFlag = 9004,

		/// <summary>
		/// Псевдотег – признак ломбардной деятельности
		/// </summary>
		PawnFlag = 9005,

		/// <summary>
		/// Псевдотег – признак страхования
		/// </summary>
		InsuranceFlag = 9006,

		/// <summary>
		/// Псевдотег – признак лотерей
		/// </summary>
		LotteryFlag = 9007,

		/// <summary>
		/// Псевдотег – признак азартных игр
		/// </summary>
		GamblingFlag = 9008,

		/// <summary>
		/// Псевдотег – признак наличия общей системы налогообложения
		/// </summary>
		GenericTaxFlag = 9009,

		/// <summary>
		/// Псевдотег – признак наличия агентской схемы
		/// </summary>
		AgentFlag = 9010,

		/// <summary>
		/// Заводской номер автомата
		/// </summary>
		AutomatSerial = 1036,

		/// <summary>
		/// Псевдотег – признак автоматического режима
		/// </summary>
		AutomatFlag = 9011,

		/// <summary>
		/// Псевдотег – признак режима работы в сети интернет
		/// </summary>
		InternetFlag = 9012,

		/// <summary>
		/// Псевдотег – признак режима бланков строгой отчётности
		/// </summary>
		BSOFlag = 9013,
		}

	/// <summary>
	/// Доступные теги чеков из статуса ФН
	/// </summary>
	public enum ReceiptTags
		{
		/// <summary>
		/// Тип коррекции
		/// </summary>
		CorrectionType = 1173,

		/// <summary>
		/// Дата коррекции
		/// </summary>
		CorrectionDate = 1178,

		/// <summary>
		/// Основание коррекции
		/// </summary>
		CorrectionOrder = 1179,

		/// <summary>
		/// Наименование предмета расчёта
		/// </summary>
		GoodName = 1030,

		/// <summary>
		/// Ограничитель описаний предметов расчёта
		/// </summary>
		GoodDelimiter = 1059,

		/// <summary>
		/// Единица измерения предмета расчёта
		/// </summary>
		UnitNameMark = 2108,

		/// <summary>
		/// Количество предмета расчёта
		/// </summary>
		GoodsCount = 1023,

		/// <summary>
		/// Цена единицы предмета расчёта
		/// </summary>
		ItemCost = 1079,

		/// <summary>
		/// Стоимость предмета расчёта
		/// </summary>
		ItemResult = 1043,

		/// <summary>
		/// Признак предмета расчёта
		/// </summary>
		ResultObject = 1212,

		/// <summary>
		/// Признак способа расчёта
		/// </summary>
		ResultMethod = 1214,

		/// <summary>
		/// Ставка НДС
		/// </summary>
		NDS = 1199,

		/// <summary>
		/// Сумма наличными
		/// </summary>
		CashPaymentValue = 1031,

		/// <summary>
		/// Сумма безналичными
		/// </summary>
		ElectronicPaymentValue = 1081,

		/// <summary>
		/// Сумма авансом
		/// </summary>
		PrepaidPaymentValue = 1215,

		/// <summary>
		/// Сумма кредитом
		/// </summary>
		PostpaidPaymentValue = 1216,

		/// <summary>
		/// Сумма встречным предоставлением
		/// </summary>
		OtherPaymentValue = 1217,

		/// <summary>
		/// Сумма НДС 20%
		/// </summary>
		DocumentSummaNDS20 = 1102,

		/// <summary>
		/// Сумма НДС 10%
		/// </summary>
		DocumentSummaNDS10 = 1103,

		/// <summary>
		/// Сумма НДС 20/120
		/// </summary>
		DocumentSummaNDS120 = 1106,

		/// <summary>
		/// Сумма НДС 10/110
		/// </summary>
		DocumentSummaNDS110 = 1107,

		/// <summary>
		/// Сумма НДС 0%
		/// </summary>
		DocumentSummaNDS0 = 1104,

		/// <summary>
		/// Сумма без НДС
		/// </summary>
		DocumentSummaNoNDS = 1105,

		/// <summary>
		/// Псевдотег – сумма НДС 5%
		/// </summary>
		DocumentSummaNDS5 = 9101,

		/// <summary>
		/// Псевдотег – сумма НДС 7%
		/// </summary>
		DocumentSummaNDS7 = 9102,

		/// <summary>
		/// Псевдотег – сумма НДС 5/105
		/// </summary>
		DocumentSummaNDS105 = 9103,

		/// <summary>
		/// Псевдотег – сумма НДС 7/107
		/// </summary>
		DocumentSummaNDS107 = 9104,

		/// <summary>
		/// Номер смены
		/// </summary>
		SessionNumber = 1038,

		/// <summary>
		/// Номер документа за смену
		/// </summary>
		InSessionDocumentNumber = 1042,

		/// <summary>
		/// ФИО и должность кассира
		/// </summary>
		CashierName = 1021,

		/// <summary>
		/// СНО чека
		/// </summary>
		ReceiptTax = 1055,

		/// <summary>
		/// Дата и время документа
		/// </summary>
		DateTime = 1012,

		/// <summary>
		/// Фискальный признак документа
		/// </summary>
		FiscalSign = 1077,

		/// <summary>
		/// Общая сумма чека
		/// </summary>
		TotalSumma = 1020,

		/// <summary>
		/// Тип операции по чеку
		/// </summary>
		OperationType = 1054,

		/// <summary>
		/// Название дополнительного реквизита пользователя
		/// </summary>
		UserParameterName = 1085,

		/// <summary>
		/// Значение дополнительного реквизита пользователя
		/// </summary>
		UserParameterValue = 1086,

		/// <summary>
		/// Номер документа в ФН
		/// </summary>
		DocumentNumber = 1040,

		/// <summary>
		/// Псевдотег – тип документа
		/// </summary>
		DocumentType = 1000,

		/// <summary>
		/// Необработанный код маркировки
		/// </summary>
		RawMarkCode = 2000,

		/// <summary>
		/// Проверочное значение для кода маркировки
		/// </summary>
		MarkCheckCode = 2115,

		/// <summary>
		/// Результат проверки кода маркировки в ОИСМ
		/// </summary>
		MarkCheckResult = 2106,
		}

	/// <summary>
	/// Класс описывает результат оценки срока жизни ФН и его применимости
	/// </summary>
	public class FNLifeResult
		{
		/// <summary>
		/// Возвращает результат оценки применимости ФН с указанными параметрами регистрации и сроком жизни
		/// </summary>
		public FNLifeStatus Status
			{
			get
				{
				return status;
				}
			}
		private FNLifeStatus status;

		/// <summary>
		/// Срок жизни ФН
		/// </summary>
		public string DeadLine
			{
			get
				{
				return deadLine.ToString ("dd.MM.yyyy");
				}
			}
		private DateTime deadLine;

		/// <summary>
		/// Конструктор. Инициализирует экземпляр результата оценки
		/// </summary>
		/// <param name="PDeadLine">Рассчитанный срок жизни ФН</param>
		/// <param name="PStatus">Результат оценки применимости</param>
		public FNLifeResult (DateTime PDeadLine, FNLifeStatus PStatus)
			{
			deadLine = PDeadLine;
			status = PStatus;
			}
		}
	}
