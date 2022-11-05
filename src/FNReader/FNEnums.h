// Общие перечисления

// Возможные типы фискальных документов
enum FNDocumentTypes
	{
	// Отчёт о регистрации
	Registration = 0x01,

	// Отчёт о регистрации под ФФД 1.1 и 1.2
	Registration_11_12 = 0x41,

	// Отчёт об изменении реквизитов регистрации
	RegistrationChange = 0x0B,

	// Отчёт об изменении реквизитов регистрации под ФФД 1.1 и 1.2
	RegistrationChange_11_12 = 0x4B,

	// Открытие смены
	OpenSession = 0x02,

	// Отчёт о текущем состоянии расчётов
	CurrentState = 0x15,

	// Кассовый чек
	Bill = 0x03,	// 0x30

	// Кассовый чек под ФФД 1.1
	Bill_11 = 0x83,	// 0x32

	// Кассовый чек под ФФД 1.2
	Bill_12 = 0xC3,	// 0x37

	// Чек коррекции (31)
	CorrectionBill = 0x1F,		// 0x30

	// Чек коррекции под ФФД 1.1
	CorrectionBill_11 = 0x9F,	// 0x32

	// Чек коррекции под ФФД 1.2
	CorrectionBill_12 = 0xDF,	// 0x37

	// БСО
	Blank = 0x04,

	// БСО под ФФД 1.1
	Blank_11 = 0x84,

	// БСО под ФФД 1.2
	Blank_12 = 0xC4,

	// БСО коррекции (41)
	CorrectionBlank = 0x29,

	// БСО коррекции под ФФД 1.1
	CorrectionBlank_11 = 0xA9,

	// БСО коррекции под ФФД 1.2
	CorrectionBlank_12 = 0xE9,

	// Закрытие смены
	CloseSession = 0x05,

	// Закрытие фискального режима
	CloseFiscalStorage = 0x06,

	// Подтверждение оператора
	Confirmation = 0x07,

	// Запрос о коде маркировки
	MarkCodeRequest = 81,

	// Уведомление о реализации кода маркировки
	MarkCodeNotification = 82,

	// Ответ на запрос о коде маркировки
	MarkCodeRequestAnswer = 83,

	// Ответ на уведомление о реализации кода маркировки
	MarkCodeNotificationAnswer = 84,

	// Неизвестный тип документа
	UnknownType = 255
	};

#define REG_CAUSE(type)		(type == Registration) || (type == RegistrationChange) || \
	(type == Registration_11_12) || (type == RegistrationChange_11_12)
#define REREG_CAUSE(type)	(type == RegistrationChange) || (type == RegistrationChange_11_12)

// Возможные фазы жизни ФН
enum FNLifePhases
	{
	// Технологический режим
	FactoryMode = 0x00,

	// Готовность к фискализации
	ReadyForLaunch = 0x01,

	// Фискальный режим
	FiscalMode = 0x03,

	// Архив закрыт, передача документов
	AfterFiscalMode = 0x07,

	// Архив закрыт, документы переданы
	ArchiveClosed = 0x0F,

	// Неизвестный режим
	UnknownMode = -1
	};

// Возможные результаты общения с COM-портом
enum SendAndReceiveResults
	{
	// Успешно
	Ok = 0,

	// Ошибка записи
	WriteError = -11,

	// Таймаут чтения
	ReadTimeout = -21,

	// Ошибка чтения
	ReadError = -22,

	// Некорректная длина ответного сообщения
	AnswerLengthError = -23,

	// Некорректная контрольная сумма ответного сообщения
	AnswerCRCError = -24,

	// Получен ответ, содержащий сообщение об ошибке
	AnswerLogicError = -31
	};

// Обрабатываемые TLV-теги
enum TLVTags
	{
	// Стандартные теги

	// Флаг работы в составе автоматического устройства расчётов (1001)
	AutomaticFlag = 0x03E9,

#define	PROC_AUTOMATICFLAG(src,dest,type)\
		case AutomaticFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s  - ККТ установлена в автомате\r\n", dest);\
			break;

	// Флаг автономного режима (1002)
	AutonomousFlag = 0x03EA,

#define PROC_AUTONOMOUSFLAG(src,dest,type)\
		case AutonomousFlag:\
			if (REG_CAUSE (type))\
				sprintf (dest, src ? "%s  - Автономный режим (без ОФД)\r\n" : "%s  - Режим передачи данных\r\n", dest);\
			break;

	// E-mail/телефон покупателя (1008)
	ClientAddress = 0x03F0,

#define PROC_CLIENTADDRESS(src,dest)\
		case ClientAddress:\
			sprintf (dest, "%s  E-mail или телефон покупателя: %s\r\n", dest, src);\
			break;

	// Адрес расчётов (1009)
	RegistrationAddress = 0x03F1,

#define PROC_REGISTRATIONADDRESS(src,dest,type,preproc)\
		case RegistrationAddress:\
			if (REG_CAUSE (type))\
				{\
				preproc;\
				sprintf (dest, "%s  Адрес расчёта: %s\r\n", dest, src);\
				}\
			break;

	// Дата и время документа (1012)
	DocumentDateTime = 0x03F4,

	// Заводской номер ККТ (1013)
	RegisterSerialNumber = 0x03F5,

	// ИНН ОФД (1017)
	OFD_INN = 0x03F9,

	// ИНН пользователя (1018)
	UserINN = 0x03FA,

#define PROC_USERINN	"%s  ИНН пользователя: %s\r\n"

	// Сумма расчёта (1020)
	OperationSumma = 0x03FC,

	// Имя кассира (1021)
	CashierName = 0x03FD,

#define PROC_CASHIERNAME(src,dest,preproc)\
		case CashierName:\
			preproc;\
			sprintf (dest, "%s  Кассир: %s\r\n", dest, src);\
			break;

	// Количество товара (1023)
	GoodsCount = 0x03FF,

	// Наименование предмета расчёта (1030)
	GoodName = 0x0406,

	// Сумма наличными (1031)
	RealCashValue = 0x0407,

	// Номер автомата (терминала) (1036)
	TerminalNumber = 0x040C,

#define PROC_TERMINALNUMBER(src,dest,type)\
		case TerminalNumber:\
			if (REG_CAUSE (type))\
				sprintf (dest, "%s  Номер автомата: %s\r\n", dest, src);\
			break;

	// Регистрационный номер ККТ (1037)
	RegistrationNumber = 0x040D,

#define PROC_REGISTRATIONNUMBER	"%s  Регистрационный номер ККТ: %s\r\n"

	// Номер смены, в которой создан документ (1038)
	SessionNumber = 0x040E,

#define PROC_SESSIONNUMBER	"%s  Номер смены: %u\r\n"

	// Номер фискального документа (1040)
	FiscalDocumentNumber = 0x0410,

	// Заводской номер ФН (1041)
	FNSerialNumber = 0x0411,

	// Номер документа за смену (1042)
	InSessionDocumentNumber = 0x0412,

	// Стоимость товарной позиции (1043)
	ItemResult = 0x0413,

	// Название ОФД (1046)
	OFDNameTag = 0x0416,

	// Имя пользователя (1048)
	UserName = 0x0418,

#define PROC_USERNAME	"%s  Пользователь: %s\r\n"

	// Предупреждения о состоянии ФН (1050 - 1053)
	FN30DaysAlert = 0x041A,

	FN3DaysAlert = 0x041B,

	FNIsFullAlert = 0x041C,

	OFDNotRespondAlert = 0x041D,

	// Признак расчёта (1054)
	OperationType = 0x041E,

	// Применённая СНО (1055)
	AppliedTaxSystem = 0x041F,

#define PROC_APPTAX(src,dest)	\
		case AppliedTaxSystem:\
			sprintf (dest, "%s  Применённая СНО: %s\r\n", dest, GetTaxFlags (src));\
			break;

	// Флаг шифрования (1056)
	EncryptionFlag = 0x0420,

#define PROC_ENCRYPTIONFLAG(src,dest,type)\
	case EncryptionFlag:\
		if ((REG_CAUSE (type)) && src)\
			sprintf (dest, "%s  - Шифрование фискальных данных\r\n", dest);\
		break;

	// Признаки агента (1057)
	AgentType = 0x0421,

#define PROC_AGENTTYPE(src,dest)\
	case AgentType:\
		sprintf (dest, "%s  Признаки агента: %s\r\n", dest, GetAgentFlags (src));\
		break;

	// Название предмета расчёта (1059)
	PaymentObject = 0x0423,

	// Адрес сайта ФНС (1060)
	TaxServiceAddress = 0x0424,

	// Флаги систем налогообложения (1062)
	TaxFlags = 0x0426,

#define PROC_REGTAXFLAGS(src,dest)	\
		case TaxFlags:\
			sprintf (dest, "%s  Налогообложение: %s\r\n", dest, GetTaxFlags (src));\
			break;

	// Фискальный признак документа (1077)
	FiscalSignature = 0x0435,

	// Фискальный признак сообщения (квитанция, 1078)
	FiscalConfirmation = 0x0436,

	// Стоимость единицы товара (1079)
	ItemCost = 0x0437,

	// Сумма безналичными (1081)
	ElectronicCashValue = 0x0439,

	// Дополнительный реквизит пользователя (1084)
	ExtraTag = 0x043C,
	ExtraTagName = 0x043D,
	ExtraTagValue = 0x043E,

	// Количество непереданных документов (1097)
	UnsentDocumentsCount = 0x0449,

	// Дата и время первого непереданного документа (1098)
	FirstUnsentDocumentDate = 0x044A,

	// Причина перерегистрации (1101)
	RegistrationChangeCause = 0x044D,

#define PROC_REGISTRATIONCHANGECAUSE(src,dest,type)\
		case RegistrationChangeCause:\
			if (REREG_CAUSE (type))\
				sprintf (dest, "%s  Причина перерегистрации: %s\r\n", dest, GetRegistrationChangeCause (src));\
			break;

	// Сумма к оплате по ставкам НДС (1102 - 1107)
	DocumentSummaNDS20 = 0x044E,
	DocumentSummaNDS10 = 0x044F,
	DocumentSummaNDS0 = 0x0450,
	DocumentSummaNoNDS = 0x0451,
	DocumentSummaNDS120 = 0x0452,
	DocumentSummaNDS110 = 0x0453,

	// Флаг работы в сети Интернет (1108)
	InternetFlag = 0x0454,

#define PROC_INTERNETFLAG(src,dest,type)\
		case InternetFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s  - Работа в сети интернет\r\n", dest);\
			break;

	// Флаг режима услуг (1109)
	ServiceFlag = 0x0455,

#define PROC_SERVICEFLAG(src,dest,type)\
		case ServiceFlag:\
			if (REG_CAUSE (type))\
				sprintf (dest, src ? "%s  - Режим услуг\r\n" : "%s  - Режим товаров\r\n", dest);\
			break;

	// Флаг формирования БСО (1110)
	BlankFlag = 0x0456,

#define PROC_BLANKFLAG(src,dest,type)\
		case BlankFlag:\
			if (REG_CAUSE (type))\
				sprintf (dest, src ? "%s  - Режим системы БСО\r\n": "%s  - Режим кассовых чеков\r\n", dest);\
			break;

	// Количество документов за смену (1111)
	InSessionDocumentsCount = 0x0457,

#define PROC_DOCUMENTSCOUNT(src,dest)\
		case InSessionDocumentsCount:\
			sprintf (dest, "%s  Документов за смену: %s\r\n", dest, src); \
			break;

	// Номер первого непереданного документа (1116)
	FirstUnsentDocumentNumber = 0x045C,

	// Адрес отправителя чеков (1117)
	BlankSenderAddress = 0x045D,

	// Количество чеков и БСО за смену (1118)
	InSessionBlanksCount = 0x045E,

	// Флаг режима лотерей (1126)
	LotteryFlag = 0x0466,

#define PROC_LOTTERYFLAG(src,dest,type)\
		case LotteryFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s  - Используется при проведении лотерей\r\n", dest);\
			break;

	// Итоги по признакам расчёта (1129 - 1132)
	TotalsIncoming1 = 0x0469,
	TotalsRevIncoming1 = 0x046A,
	TotalsOutcoming1 = 0x046B,
	TotalsRevOutcoming1 = 0x046C,

	// Общие счётчики (1133)
	TotalsCounts = 0x046D,

	// Число документов в счётчиках итогов, общее и по отдельным признакам (1134, 1135)
	TotalsDocumentsCount1 = 0x046E,
	TotalsDocumentsCount2 = 0x046F,

	// Итоговая сумма, наличными (1136)
	TotalsSummaCash = 0x0470,

	// Итоговая сумма, безналичными (1138)
	TotalsSummaCard = 0x0472,

	// Итоговые суммы по ставкам НДС (1139 - 1143)
	TotalsSummaNDS20 = 0x0473,
	TotalsSummaNDS10 = 0x0474,
	TotalsSummaNDS120 = 0x0475,
	TotalsSummaNDS110 = 0x0476,
	TotalsSummaNDS0 = 0x0477,

	// Число документов коррекции и непереданных документов (1144)
	TotalsDocumentsCount3 = 0x0478,

	// Итоги по признакам (1145, 1146)
	TotalsIncoming2 = 0x0479,
	TotalsOutcoming2 = 0x047A,

	// Счётчики итогов в ФН (1157)
	TotalCounters = 0x0485,

	// Счётчики итогов непереданных ФД (1158)
	TotalUnsentCounters = 0x0486,

	// Код товарной позиции ФФД 1.05 (1162)
	GoodCode = 0x048A,

	// Код маркировки ФФД 1.2 (1163)
	GoodCodeMark = 0x048B,
		
	// Телефон или E-mail поставщика (1171)
	ProviderPhone = 0x0493,
		
	// Самостоятельная или предписанная коррекция (1173)
	CorrectionType = 0x0495,
		
	// Основание для коррекции (1174)
	CorrectionBase = 0x0496,
		
	// Дата выполнения корректируемого расчёта (1178)
	CorrectionDate = 0x049A,
		
	// Предписание ФНС для выполнения коррекции (1179)
	CorrectionOrder = 0x049B,
		
	// Общая сумма по отдельному признаку расчёта (1183)
	TotalsSummaPerType = 0x049F,

	// Место расчётов (1187)
	RegistrationPlace = 0x04A3,

#define PROC_REGISTRATIONPLACE(src,dest,type,preproc)\
		case RegistrationPlace:\
			if (REG_CAUSE (type))\
				{\
				preproc;\
				sprintf (dest, "%s  Место расчёта: %s\r\n", dest, src);\
				}\
			break;

	// Версия ПО ККТ (1188)
	SoftwareVersionOfKKT = 0x04A4,

#define PROC_KKTSOFTWARE(src,dest)\
		case SoftwareVersionOfKKT:\
			if (REG_CAUSE (documentType))\
				sprintf (dest, "%s  Версия ПО ККТ: %s\r\n", dest, src);\
			break;

	// Максимальная версия ФФД, поддерживаемая ККТ (1189)
	FFDVersionOfKKT = 0x04A5,

	// Максимальная версия ФФД, поддерживаемая ФН (1190)
	FFDVersionOfFN = 0x04A6,

	// Флаг азартных игр (1193)
	GamesFlag = 0x04A9,

#define PROC_GAMESFLAG(src,dest,type)\
		case GamesFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s  - Используется при проведении азартных игр\r\n", dest);\
			break;

	// Счётчики итогов смены (1194)
	TotalSessionCounters = 0x04AA,

	// Единица измерения товара / услуги (1197)
	UnitName = 0x04AD,
		
	// Ставка НДС (1199)
	NDS = 0x04AF,

	// Сумма НДС за товар / услугу (1200)
	NDSSumma = 0x04B0,

	// Общая сумма с учётом авансов и кредитов (1201)
	TotalsSummaDebitCredit = 0x04B1,

	// ИНН кассира (1203)
	CashierINN = 0x04B3,

#define PROC_CASHIERINN(src,dest)\
		case CashierINN:\
			sprintf (dest, "%s  ИНН кассира: %s\r\n", dest, src);\
			break;

	// Флаги причин перерегистрации (1205)
	ExtendedReregFlags = 0x04B5,

#define PROC_EXTENDEDREREGFLAGS(src,dest,type)\
		case ExtendedReregFlags:\
			if (REREG_CAUSE (type))\
				sprintf (dest, "%s  Причины перерегистрации: %s\r\n", dest, GetRegistrationChangeCauseExtended (src));\
				break;

	// Флаги предупреждений ОФД (1206)
	OFDAttentionFlags = 0x04B6,

#define PROC_OFDATTENTION(src,dest)\
		case OFDAttentionFlags:\
			sprintf (dest, "%s  Флаги предупреждений ОФД: %s\r\n", dest, GetOFDAttentionFlags (src));\
			break;

	// Флаг подакцизных товаров (1207)
	ExciseFlag = 0x04B7,

#define PROC_EXCISEFLAG(src,dest,type)\
		case ExciseFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s  - Используется для продажи подакцизных товаров\r\n", dest);\
			break;

	// Версия формата фискальных документов (1209)
	FFDVersion = 0x04B9,

#define PROC_FFDVERSION(is_reg,src,dest)\
		case FFDVersion:\
			if (is_reg && (src <= 4) && (src > maxFFDVersion))\
				maxFFDVersion = src;\
			sprintf (dest, "%s  Версия ФФД: %s\r\n", dest, GetFFDVersion (src));\
			break;

	// Признак предмета расчёта (1212)
	ResultObject = 0x04BC,

	// Срок жизни ФН в днях (1213)
	FNLifeLength = 0x04BD,

#define PROC_FNLIFELENGTH(src,dest)\
		case FNLifeLength:\
			if (REG_CAUSE (documentType))\
				sprintf (dest, "%s  Срок жизни ФН, дней: %s\r\n", dest, src);\
			break;

	// Признак способа расчёта (1214)
	ResultMethod = 0x04BE,

	// Сумма предоплатой / авансом (1215)
	PrepaidCashValue = 0x04BF,

	// Сумма постоплатой / кредитом (1216)
	PostpaidCashValue = 0x04C0,

	// Сумма иной оплатой (1217)
	OtherCashValue = 0x04C1,

	// Итоговая сумма авансом, кредитом и иными способами (1218 - 1220)
	TotalsSummaPrepaid = 0x04C2,
	TotalsSummaPostpaid = 0x04C3,
	TotalsSummaOthers = 0x04C4,

	// Флаг установки внутри автоматического устройства расчётов (1221)
	AutomaticInsideFlag = 0x04C5,

	// Наименования и ИНН поставщиков и покупателей (1224 - 1228)
	ProviderData = 0x04C8,
	ProviderName = 0x04C9,
	ProviderINN = 0x04CA,
	BuyerName = 0x04CB,
	BuyerINN = 0x04CC,

	// Итоги по признакам (1232, 1233)
	TotalsRevIncoming2 = 0x04D0,
	TotalsRevOutcoming2 = 0x04D1,

	// Отраслевые реквизиты чека и предмета расчёта (1260, 1261)
	IndustryPropsForObject = 0x04EC,
	IndustryPropsForBill = 0x04ED,
		
	// Отраслевые реквизиты чека и предмета расчёта (1262 - 1265)
	IndustryProps_FOIV_ID = 0x04EE,
	IndustryProps_Date = 0x04EF,
	IndustryProps_Number = 0x04F0,
	IndustryProps_Content = 0x04F1,

	// Расширенные признаки регистрации (1290)
	ExtendedRegOptions = 0x050A,

#define PROC_EXTENDEDREGOPT(src,dest,type)\
		case ExtendedRegOptions:\
			if (REG_CAUSE (type))\
				sprintf (dest, "%s  - Признаки регистрации: %s\r\n", dest, GetExtendedRegOptions (src));\
			break;

	// Типы кодов маркировки (1300 - 1309, 1320 - 1325)
	MarkCode_Unknown = 0x0514,
	MarkCode_EAN8 = 0x0515,
	MarkCode_EAN13 = 0x0516,
	MarkCode_ITF14 = 0x0517,
	MarkCode_GS10 = 0x0518,
	MarkCode_GS1M = 0x0519,
	MarkCode_Short = 0x051A,
	MarkCode_Fur = 0x051B,
	MarkCode_EGAIS20 = 0x051C,
	MarkCode_EGAIS30 = 0x051D,

	MarkCode_F1 = 0x0528,
	MarkCode_F2 = 0x0529,
	MarkCode_F3 = 0x052A,
	MarkCode_F4 = 0x052B,
	MarkCode_F5 = 0x052C,
	MarkCode_F6 = 0x052D,

	// Режим обработки КМ (2102)
	MarkFlags_ProcessingMode = 0x0836,
		
	// Количество непереданных уведомлений для кодов маркировки (2104)
	UnsentMarkCodes = 0x0838,
		
	// Результат проверки КМ (2106)
	MarkFlags_CheckingResult = 0x083A,
		
	// Признак наличия отрицательных результатов проверки КМ (2107)
	MarkFlags_HasWrongCodes = 0x083B,
		
	// Единица измерения товара / услуги (2108)
	UnitNameMark = 0x083C,

	// Признаки наличия некорректных кодов маркировки (2112)
	MarkFlags_IncorrectCodes = 0x0840,
		
	// Признаки наличия некорректных запросов и уведомлений (2113)
	MarkFlags_IncorrectRequests = 0x0841,
		
	// Теги файлов архивов ФН

	// Отчёт о регистрации
	FNA_Registration = Registration + 100,
	FNA_Registration_11_12 = Registration_11_12 + 100,

	// Отчёт об изменении реквизитов регистрации
	FNA_RegistrationChange = RegistrationChange + 100,
	FNA_RegistrationChange_11_12 = RegistrationChange_11_12 + 100,

	// Открытие смены
	FNA_OpenSession = OpenSession + 100,

	// Отчёт о текущем состоянии расчётов
	FNA_CurrentState = CurrentState + 100,

	// Кассовый чек
	FNA_Bill = Bill + 100,

	// Чек коррекции
	FNA_CorrectionBill = CorrectionBill + 100,

	// БСО
	FNA_Blank = Blank + 100,

	// БСО коррекции
	FNA_CorrectionBlank = CorrectionBlank + 100,

	// Закрытие смены
	FNA_CloseSession = CloseSession + 100,

	// Закрытие фискального режима
	FNA_CloseFiscalStorage = CloseFiscalStorage + 100,

	// Подтверждение оператора
	FNA_Confirmation = Confirmation + 100,

	// Фискальный документ (автономный режим, 65001)
	FNA_AutonomousArray = 0xFDE9,

	// Фискальный документ (режим передачи данных, 65002)
	FNA_OFDArray = 0xFDEA,

	// Фискальный документ (автономный режим, ФФД 1.1, 65011)
	FNA_AutonomousArray_1_1 = 0xFDF3,

	// Фискальный документ (режим передачи данных, ФФД 1.1, 65012)
	FNA_OFDArray_1_1 = 0xFDF4,

	// Фискальный документ (автономный режим, ФФД 1.2, 65021)
	FNA_AutonomousArray_1_2 = 0xFDFD,

	// Фискальный документ (режим передачи данных, ФФД 1.2, 65022)
	FNA_OFDArray_1_2 = 0xFDFE,

	// Тег, сообщающий о внутренней ошибке ФН при считывании
	ReadingFailure = 9999,

	// ФП уведомления о коде маркировки
	FNMSignature = 201,
		
	// ФП сообщения
	FNA12Signature = 300,
	FNASignature = 301,

	// Заголовок выгрузки данных регистрации
	FNARegistrationProtocol = 304,

	// Заголовок выгрузки данных закрытия архива
	FNAClosingProtocol = 305,
	};
