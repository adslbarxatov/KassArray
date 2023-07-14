// ����� ������������

// ��������� ���� ���������� ����������
enum FNDocumentTypes
	{
	// ����� � �����������
	Registration = 0x01,

	// ����� � ����������� ��� ��� 1.1 � 1.2
	Registration_11_12 = 0x41,

	// ����� �� ��������� ���������� �����������
	RegistrationChange = 0x0B,

	// ����� �� ��������� ���������� ����������� ��� ��� 1.1 � 1.2
	RegistrationChange_11_12 = 0x4B,

	// �������� �����
	OpenSession = 0x02,

	// ����� � ������� ��������� ��������
	CurrentState = 0x15,

	// �������� ���
	Bill = 0x03,	// 0x30

	// �������� ��� ��� ��� 1.1
	Bill_11 = 0x83,	// 0x32

	// �������� ��� ��� ��� 1.2
	Bill_12 = 0xC3,	// 0x37

	// ��� ��������� (31)
	CorrectionBill = 0x1F,		// 0x30

	// ��� ��������� ��� ��� 1.1
	CorrectionBill_11 = 0x9F,	// 0x32

	// ��� ��������� ��� ��� 1.2
	CorrectionBill_12 = 0xDF,	// 0x37

	// ���
	Blank = 0x04,

	// ��� ��� ��� 1.1
	Blank_11 = 0x84,

	// ��� ��� ��� 1.2
	Blank_12 = 0xC4,

	// ��� ��������� (41)
	CorrectionBlank = 0x29,

	// ��� ��������� ��� ��� 1.1
	CorrectionBlank_11 = 0xA9,

	// ��� ��������� ��� ��� 1.2
	CorrectionBlank_12 = 0xE9,

	// �������� �����
	CloseSession = 0x05,

	// �������� ����������� ������
	CloseFiscalStorage = 0x06,

	// ������������� ���������
	Confirmation = 0x07,

	// ������ � ���� ����������
	MarkCodeRequest = 81,

	// ����������� � ���������� ���� ����������
	MarkCodeNotification = 82,

	// ����� �� ������ � ���� ����������
	MarkCodeRequestAnswer = 83,

	// ����� �� ����������� � ���������� ���� ����������
	MarkCodeNotificationAnswer = 84,

	// ����������� ��� ���������
	UnknownType = 255
	};

#define REG_CAUSE(type)		(type == Registration) || (type == RegistrationChange) || \
	(type == Registration_11_12) || (type == RegistrationChange_11_12)
#define REREG_CAUSE(type)	(type == RegistrationChange) || (type == RegistrationChange_11_12)

// ��������� ���� ����� ��
enum FNLifePhases
	{
	// ��������������� �����
	FactoryMode = 0x00,

	// ���������� � ������������
	ReadyForLaunch = 0x01,

	// ���������� �����
	FiscalMode = 0x03,

	// ����� ������, �������� ����������
	AfterFiscalMode = 0x07,

	// ����� ������, ��������� ��������
	ArchiveClosed = 0x0F,

	// ����������� �����
	UnknownMode = -1
	};

// ��������� ���������� ������� � COM-������
enum SendAndReceiveResults
	{
	// �������
	Ok = 0,

	// ������ ������
	WriteError = -11,

	// ������� ������
	ReadTimeout = -21,

	// ������ ������
	ReadError = -22,

	// ������������ ����� ��������� ���������
	AnswerLengthError = -23,

	// ������������ ����������� ����� ��������� ���������
	AnswerCRCError = -24,

	// ������ ��� ������������ �������
	RequestLogicError = -31,

	// ������, ���������� �� ��
	InternalFNError = -32,
	};

// �������������� TLV-����
enum TLVTags
	{
	// ����������� ����

	// ���� ������ � ������� ��������������� ���������� �������� (1001)
	AutomaticFlag = 0x03E9,

#define	PROC_AUTOMATICFLAG(src,dest,type)\
		case AutomaticFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s%s �: ��� ����������� � ��������\r\n", dest, RegFlagSign);\
			break;

	// ���� ����������� ������ (1002)
	AutonomousFlag = 0x03EA,

#define PROC_AUTONOMOUSFLAG(src,dest,type)\
		case AutonomousFlag:\
			if (REG_CAUSE (type)) {\
				autonomousFlag = src;\
				sprintf (dest, src ? "%s%s �: ���������� ��� (��� ���)\r\n" :\
					"%s%s �: ��� ������� ������ ���\r\n", \
					dest, RegFlagSign);\
				}\
			break;

	// E-mail/������� ���������� (1008)
	ClientAddress = 0x03F0,

#define PROC_CLIENTADDRESS(src,dest)\
		case ClientAddress:\
			sprintf (dest, "%s  E-mail ��� ������� ����������: %s\r\n", dest, src);\
			break;

	// ����� �������� (1009)
	RegistrationAddress = 0x03F1,

#define PROC_REGISTRATIONADDRESS(src,dest,type,preproc)\
		case RegistrationAddress:\
			if (REG_CAUSE (type))\
				{\
				preproc;\
				sprintf (dest, "%s  ����� �������: %s\r\n", dest, src);\
				}\
			break;

	// ���� � ����� ��������� (1012)
	DocumentDateTime = 0x03F4,

	// ��������� ����� ��� (1013)
	RegisterSerialNumber = 0x03F5,

	// ��� ��� (1017)
	OFD_INN = 0x03F9,

	// ��� ������������ (1018)
	UserINN = 0x03FA,

#define PROC_USERINN	"%s  ��� ������������: %s\r\n"

	// ����� ������� (1020)
	OperationSumma = 0x03FC,

	// ��� ������� (1021)
	CashierName = 0x03FD,

#define PROC_CASHIERNAME(src,dest,preproc)\
		case CashierName:\
			preproc;\
			sprintf (dest, "%s  ������: %s\r\n", dest, src);\
			break;

	// ���������� ������ (1023)
	GoodsCount = 0x03FF,

	// ������������ �������� ������� (1030)
	GoodName = 0x0406,

	// ����� ��������� (1031)
	RealCashValue = 0x0407,

#define PROC_CASHVALUE(src,dst,offset,sz,nm)\
	if ((sz > 0) && ((sz > 1) || src[offset]))\
		sprintf (dst, "%s  " nm ": %s �.\r\n", dst, BuildNumber (src, offset, sz, 1));

	// ����� �������� (���������) (1036)
	TerminalNumber = 0x040C,

#define PROC_TERMINALNUMBER(src,dest,type)\
		case TerminalNumber:\
			if (REG_CAUSE (type))\
				sprintf (dest, "%s  ����� ��������: %s\r\n", dest, src);\
			break;

	// ��������������� ����� ��� (1037)
	RegistrationNumber = 0x040D,

#define PROC_REGISTRATIONNUMBER	"%s  ��������������� ����� ���: %s\r\n"

	// ����� �����, � ������� ������ �������� (1038)
	SessionNumber = 0x040E,

#define PROC_SESSIONNUMBER	"%s  ����� �����: %u\r\n"

	// ����� ����������� ��������� (1040)
	FiscalDocumentNumber = 0x0410,

	// ��������� ����� �� (1041)
	FNSerialNumber = 0x0411,

	// ����� ��������� �� ����� (1042)
	InSessionDocumentNumber = 0x0412,

	// ��������� �������� ������� (1043)
	ItemResult = 0x0413,

	// �������� ��� (1046)
	OFDNameTag = 0x0416,

	// ��� ������������ (1048)
	UserName = 0x0418,

#define PROC_USERNAME	"%s  ������������: %s\r\n"

	// �������������� � ��������� �� (1050 - 1053)
	FN30DaysAlert = 0x041A,

	FN3DaysAlert = 0x041B,

	FNIsFullAlert = 0x041C,

	OFDNotRespondAlert = 0x041D,

	// ������� ������� (1054)
	OperationType = 0x041E,

	// ���������� ��� (1055)
	AppliedTaxSystem = 0x041F,

#define PROC_APPTAX(src,dest)	\
		case AppliedTaxSystem:\
			sprintf (dest, "%s  ���������� ���: %s\r\n", dest, GetTaxFlags (src));\
			break;

	// ���� ���������� (1056)
	EncryptionFlag = 0x0420,

#define PROC_ENCRYPTIONFLAG(src,dest,type)\
	case EncryptionFlag:\
		if ((REG_CAUSE (type)) && src)\
			sprintf (dest, "%s%s �: �������� ���������� ������\r\n", dest, RegFlagSign);\
		break;

	// �������� ������ (1057)
	AgentType = 0x0421,

#define PROC_AGENTTYPE(src,dest)\
	case AgentType:\
		strcat (dest, GetAgentFlags (src));\
		break;

	// �������� �������� ������� (1059)
	PaymentObject = 0x0423,

	// ����� ����� ��� (1060)
	TaxServiceAddress = 0x0424,

	// ����� ������ ��������������� (1062)
	TaxFlags = 0x0426,

#define PROC_REGTAXFLAGS(src,dest)	\
		case TaxFlags:\
			sprintf (dest, "%s  ���������������: %s\r\n", dest, GetTaxFlags (src));\
			break;

	// ���������� ������� ��������� (1077)
	FiscalSignature = 0x0435,

	// ���������� ������� ��������� (���������, 1078)
	FiscalConfirmation = 0x0436,

	// ��������� ������� ������ (1079)
	ItemCost = 0x0437,

	// ����� ������������ (1081)
	ElectronicCashValue = 0x0439,

	// �������������� �������� ������������ (1084)
	ExtraTag = 0x043C,
	ExtraTagName = 0x043D,
	ExtraTagValue = 0x043E,

	// ���������� ������������ ���������� (1097)
	UnsentDocumentsCount = 0x0449,

	// ���� � ����� ������� ������������� ��������� (1098)
	FirstUnsentDocumentDate = 0x044A,

	// ������� ��������������� (1101)
	RegistrationChangeCause = 0x044D,

#define PROC_REGISTRATIONCHANGECAUSE(src,dest,type)\
		case RegistrationChangeCause:\
			if (REREG_CAUSE (type))\
				strcat (dest, GetRegistrationChangeCause (src));\
			break;

	// ����� � ������ �� ������� ��� (1102 - 1107)
	DocumentSummaNDS20 = 0x044E,
	DocumentSummaNDS10 = 0x044F,
	DocumentSummaNDS0 = 0x0450,
	DocumentSummaNoNDS = 0x0451,
	DocumentSummaNDS120 = 0x0452,
	DocumentSummaNDS110 = 0x0453,

	// ���� ������ � ���� �������� (1108)
	InternetFlag = 0x0454,

#define PROC_INTERNETFLAG(src,dest,type)\
		case InternetFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s%s �: ������ � ���� ��������\r\n", dest, RegFlagSign);\
			break;

	// ���� ������ ����� (1109)
	ServiceFlag = 0x0455,

#define PROC_SERVICEFLAG(src,dest,type)\
		case ServiceFlag:\
			if (REG_CAUSE (type))\
				sprintf (dest, src ? "%s%s �: ���������� �����\r\n" : "%s%s �: ���������� �������\r\n",\
					dest, RegFlagSign);\
			break;

	// ���� ������������ ��� (1110)
	BlankFlag = 0x0456,

#define PROC_BLANKFLAG(src,dest,type)\
		case BlankFlag:\
			if (REG_CAUSE (type))\
				sprintf (dest, src ? "%s%s �: ���������� ���\r\n" : "%s%s �: ���������� �������� �����\r\n",\
					dest, RegFlagSign);\
			break;

	// ���������� ���������� �� ����� (1111)
	InSessionDocumentsCount = 0x0457,

#define PROC_DOCUMENTSCOUNT(src,dest)\
		case InSessionDocumentsCount:\
			sprintf (dest, "%s  ���������� �� �����: %s\r\n", dest, src); \
			break;

	// ����� ������� ������������� ��������� (1116)
	FirstUnsentDocumentNumber = 0x045C,

	// ����� ����������� ����� (1117)
	BlankSenderAddress = 0x045D,

	// ���������� ����� � ��� �� ����� (1118)
	InSessionBlanksCount = 0x045E,

	// ���� ������ ������� (1126)
	LotteryFlag = 0x0466,

#define PROC_LOTTERYFLAG(src,dest,type)\
		case LotteryFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s%s �: ���������� �������\r\n", dest, RegFlagSign);\
			break;

	// ����� �� ��������� ������� (1129 - 1132)
	TotalsIncoming1 = 0x0469,
	TotalsRevIncoming1 = 0x046A,
	TotalsOutcoming1 = 0x046B,
	TotalsRevOutcoming1 = 0x046C,

	// ����� �������� (1133)
	TotalsCounts = 0x046D,

	// ����� ���������� � ��������� ������, ����� � �� ��������� ��������� (1134, 1135)
	TotalsDocumentsCount1 = 0x046E,
	TotalsDocumentsCount2 = 0x046F,

	// �������� �����, ��������� (1136)
	TotalsSummaCash = 0x0470,

	// �������� �����, ������������ (1138)
	TotalsSummaCard = 0x0472,

	// �������� ����� �� ������� ��� (1139 - 1143)
	TotalsSummaNDS20 = 0x0473,
	TotalsSummaNDS10 = 0x0474,
	TotalsSummaNDS120 = 0x0475,
	TotalsSummaNDS110 = 0x0476,
	TotalsSummaNDS0 = 0x0477,

	// ����� ���������� ��������� � ������������ ���������� (1144)
	TotalsDocumentsCount3 = 0x0478,

	// ����� �� ��������� (1145, 1146)
	TotalsIncoming2 = 0x0479,
	TotalsOutcoming2 = 0x047A,

	// �������� ������ � �� (1157)
	TotalCounters = 0x0485,

	// �������� ������ ������������ �� (1158)
	TotalUnsentCounters = 0x0486,

	// ��� �������� ������� ��� 1.05 (1162)
	GoodCode = 0x048A,

	// ��� ���������� ��� 1.2 (1163)
	GoodCodeMark = 0x048B,
		
	// ������� ��� E-mail ���������� (1171)
	ProviderPhone = 0x0493,
		
	// ��������������� ��� ������������ ��������� (1173)
	CorrectionType = 0x0495,
		
	// ��������� ��� ��������� (1174)
	CorrectionBase = 0x0496,
		
	// ���� ���������� ��������������� ������� (1178)
	CorrectionDate = 0x049A,
		
	// ����������� ��� ��� ���������� ��������� (1179)
	CorrectionOrder = 0x049B,
		
	// ����� ����� �� ���������� �������� ������� (1183)
	TotalsSummaPerType = 0x049F,

	// ����� �������� (1187)
	RegistrationPlace = 0x04A3,

#define PROC_REGISTRATIONPLACE(src,dest,type,preproc)\
		case RegistrationPlace:\
			if (REG_CAUSE (type))\
				{\
				preproc;\
				sprintf (dest, "%s  ����� �������: %s\r\n", dest, src);\
				}\
			break;

	// ������ �� ��� (1188)
	SoftwareVersionOfKKT = 0x04A4,

#define PROC_KKTSOFTWARE(src,dest)\
		case SoftwareVersionOfKKT:\
			if (REG_CAUSE (documentType))\
				sprintf (dest, "%s  ������ �� ���: %s\r\n", dest, src);\
			break;

	// ������������ ������ ���, �������������� ��� (1189)
	FFDVersionOfKKT = 0x04A5,

	// ������������ ������ ���, �������������� �� (1190)
	FFDVersionOfFN = 0x04A6,

	// ���� �������� ��� (1193)
	GamesFlag = 0x04A9,

#define PROC_GAMESFLAG(src,dest,type)\
		case GamesFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s%s �: ���������� �������� ���\r\n", dest, RegFlagSign);\
			break;

	// �������� ������ ����� (1194)
	TotalSessionCounters = 0x04AA,

	// ������� ��������� ������ / ������ (1197)
	UnitName = 0x04AD,
		
	// ������ ��� (1199)
	NDS = 0x04AF,

	// ����� ��� �� ����� / ������ (1200)
	NDSSumma = 0x04B0,

	// ����� ����� � ������ ������� � �������� (1201)
	TotalsSummaDebitCredit = 0x04B1,

	// ��� ������� (1203)
	CashierINN = 0x04B3,

#define PROC_CASHIERINN(src,dest)\
		case CashierINN:\
			sprintf (dest, "%s  ��� �������: %s\r\n", dest, src);\
			break;

	// ����� ������ ��������������� (1205)
	ExtendedReregFlags = 0x04B5,

#define PROC_EXTENDEDREREGFLAGS(src,dest,type)\
		case ExtendedReregFlags:\
			if (REREG_CAUSE (type))\
				strcat (dest, GetRegistrationChangeCauseExtended (src));\
				break;

	// ����� �������������� ��� (1206)
	OFDAttentionFlags = 0x04B6,

#define PROC_OFDATTENTION(src,dest)\
		case OFDAttentionFlags:\
			strcat (dest, GetOFDAttentionFlags (src));\
			break;

	// ���� ����������� ������� (1207)
	ExciseFlag = 0x04B7,

#define PROC_EXCISEFLAG(src,dest,type)\
		case ExciseFlag:\
			if ((REG_CAUSE (type)) && src)\
				sprintf (dest, "%s%s �: ������� ����������� �������\r\n", dest, RegFlagSign);\
			break;

	// ������ ������� ���������� ���������� (1209)
	FFDVersion = 0x04B9,

#define PROC_FFDVERSION(is_reg,src,dest)\
		case FFDVersion:\
			if (is_reg && (src <= 4) && (src > maxFFDVersion))\
				maxFFDVersion = src;\
			sprintf (dest, "%s  ������ ���: %s\r\n", dest, GetFFDVersion (src));\
			break;

	// ������� �������� ������� (1212)
	ResultObject = 0x04BC,

	// ���� ����� �� � ���� (1213)
	FNLifeLength = 0x04BD,

#define PROC_FNLIFELENGTH(src,dest)\
		case FNLifeLength:\
			if (REG_CAUSE (documentType))\
				sprintf (dest, "%s  ���� ����� ��, ����: %s\r\n", dest, src);\
			break;

	// ������� ������� ������� (1214)
	ResultMethod = 0x04BE,

	// ����� ����������� / ������� (1215)
	PrepaidCashValue = 0x04BF,

	// ����� ����������� / �������� (1216)
	PostpaidCashValue = 0x04C0,

	// ����� ���� ������� (1217)
	OtherCashValue = 0x04C1,

	// �������� ����� �������, �������� � ����� ��������� (1218 - 1220)
	TotalsSummaPrepaid = 0x04C2,
	TotalsSummaPostpaid = 0x04C3,
	TotalsSummaOthers = 0x04C4,

	// ���� ��������� ������ ��������������� ���������� �������� (1221)
	AutomaticInsideFlag = 0x04C5,

	// ������������ � ��� ����������� � ����������� (1224 - 1228)
	ProviderData = 0x04C8,
	ProviderName = 0x04C9,
	ProviderINN = 0x04CA,
	BuyerName = 0x04CB,
	BuyerINN = 0x04CC,

	// ����� �� ��������� (1232, 1233)
	TotalsRevIncoming2 = 0x04D0,
	TotalsRevOutcoming2 = 0x04D1,

	// ���������� ��������� ���� � �������� ������� (1260, 1261)
	IndustryPropsForObject = 0x04EC,
	IndustryPropsForBill = 0x04ED,
		
	// ���������� ��������� ���� � �������� ������� (1262 - 1265)
	IndustryProps_FOIV_ID = 0x04EE,
	IndustryProps_Date = 0x04EF,
	IndustryProps_Number = 0x04F0,
	IndustryProps_Content = 0x04F1,

	// ����������� �������� ����������� (1290)
	ExtendedRegOptions = 0x050A,

#define PROC_EXTENDEDREGOPT(src,dest,type)\
		case ExtendedRegOptions:\
			if (REG_CAUSE (type))\
				strcat (dest, GetExtendedRegOptions (src));\
			break;

	// ���� ����� ���������� (1300 - 1309, 1320 - 1325)
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

	// ����� ��������� �� (2102)
	MarkFlags_ProcessingMode = 0x0836,
		
	// ���������� ������������ ����������� ��� ����� ���������� (2104)
	UnsentMarkCodes = 0x0838,
		
	// ��������� �������� �� (2106)
	MarkFlags_CheckingResult = 0x083A,
		
	// ������� ������� ������������� ����������� �������� �� (2107)
	MarkFlags_HasWrongCodes = 0x083B,
		
	// ������� ��������� ������ / ������ (2108)
	UnitNameMark = 0x083C,

	// �������� ������� ������������ ����� ���������� (2112)
	MarkFlags_IncorrectCodes = 0x0840,
		
	// �������� ������� ������������ �������� � ����������� (2113)
	MarkFlags_IncorrectRequests = 0x0841,
		
	// ���� ������ ������� ��

	// ����� � �����������
	FNA_Registration = Registration + 100,
	FNA_Registration_11_12 = Registration_11_12 + 100,

	// ����� �� ��������� ���������� �����������
	FNA_RegistrationChange = RegistrationChange + 100,
	FNA_RegistrationChange_11_12 = RegistrationChange_11_12 + 100,

	// �������� �����
	FNA_OpenSession = OpenSession + 100,

	// ����� � ������� ��������� ��������
	FNA_CurrentState = CurrentState + 100,

	// �������� ���
	FNA_Bill = Bill + 100,

	// ��� ���������
	FNA_CorrectionBill = CorrectionBill + 100,

	// ���
	FNA_Blank = Blank + 100,

	// ��� ���������
	FNA_CorrectionBlank = CorrectionBlank + 100,

	// �������� �����
	FNA_CloseSession = CloseSession + 100,

	// �������� ����������� ������
	FNA_CloseFiscalStorage = CloseFiscalStorage + 100,

	// ������������� ���������
	FNA_Confirmation = Confirmation + 100,

	// ���������� �������� (���������� �����, 65001)
	FNA_AutonomousArray = 0xFDE9,

	// ���������� �������� (����� �������� ������, 65002)
	FNA_OFDArray = 0xFDEA,

	// ���������� �������� (���������� �����, ��� 1.1, 65011)
	FNA_AutonomousArray_1_1 = 0xFDF3,

	// ���������� �������� (����� �������� ������, ��� 1.1, 65012)
	FNA_OFDArray_1_1 = 0xFDF4,

	// ���������� �������� (���������� �����, ��� 1.2, 65021)
	FNA_AutonomousArray_1_2 = 0xFDFD,

	// ���������� �������� (����� �������� ������, ��� 1.2, 65022)
	FNA_OFDArray_1_2 = 0xFDFE,

	// ���, ���������� � ���������� ������ �� ��� ����������
	ReadingFailure = 9999,

	// �� ����������� � ���� ����������
	FNMSignature = 201,
		
	// �� ���������
	FNA12Signature = 300,
	FNASignature = 301,

	// ��������� �������� ������ �����������
	FNARegistrationProtocol = 304,

	// ��������� �������� ������ �������� ������
	FNAClosingProtocol = 305,
	};
