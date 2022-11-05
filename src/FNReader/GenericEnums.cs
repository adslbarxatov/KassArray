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
		/// Дополнительный для А4
		/// </summary>
		ManualA4 = 3,
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
	}
