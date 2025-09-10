namespace RD_AAOW
	{
	/// <summary>
	/// Класс объединяет все подклассы справочников и помощников приложения
	/// </summary>
	public class KnowledgeBase
		{
		/// <summary>
		/// Возвращает преобразователь текста в коды символов ККТ
		/// </summary>
		public KKTCodes CodeTables
			{
			get
				{
				return kktc;
				}
			}
		private KKTCodes kktc;

		/// <summary>
		/// Возвращает справочник ошибок ККТ
		/// </summary>
		public KKTErrorsList Errors
			{
			get
				{
				return kkte;
				}
			}
		private KKTErrorsList kkte;

		/// <summary>
		/// Возвращает справочник ОФД
		/// </summary>
		public OFD Ofd
			{
			get
				{
				return ofd;
				}
			}
		private OFD ofd;

		/// <summary>
		/// Возвращает справочник команд нижнего уровня
		/// </summary>
		public TermsDictionary Dictionary
			{
			get
				{
				return td;
				}
			}
		private TermsDictionary td;

		/// <summary>
		/// Возвращает оператор руководств пользователя ККТ
		/// </summary>
		public UserGuides UserGuides
			{
			get
				{
				return ug;
				}
			}
		private UserGuides ug;

		/// <summary>
		/// Возвращает оператор заводских номеров ККТ
		/// </summary>
		public KKTSerial KKTNumbers
			{
			get
				{
				return kkts;
				}
			}
		private KKTSerial kkts;

		/// <summary>
		/// Возвращает оператор заводских номеров ФН
		/// </summary>
		public FNSerial FNNumbers
			{
			get
				{
				return fns;
				}
			}
		private FNSerial fns;

		/// <summary>
		/// Возвращает справочник TLV-тегов
		/// </summary>
		public TLVTags Tags
			{
			get
				{
				return tlvt;
				}
			}
		private TLVTags tlvt;

		/// <summary>
		/// Возвращает оператор штрих-кодов
		/// </summary>
		public BarCodes Barcodes
			{
			get
				{
				return barc;
				}
			}
		private BarCodes barc;

		/// <summary>
		/// Возвращает справочник разъёмов ККТ и торгового оборудования
		/// </summary>
		public Connectors Plugs
			{
			get
				{
				return conn;
				}
			}
		private Connectors conn;

		/// <summary>
		/// Возвращает справочник описаний символов Unicode
		/// </summary>
		public UnicodeDescriptor Unicodes
			{
			get
				{
				return ud;
				}
			}
		private UnicodeDescriptor ud;

#if ANDROID

		/// <summary>
		/// Возвращает справочник команд нижнего уровня
		/// </summary>
		public LowLevel LLCommands
			{
			get
				{
				return ll;
				}
			}
		private LowLevel ll;

#endif

		/// <summary>
		/// Конструктор. Инициализирует базу знаний
		/// </summary>
		public KnowledgeBase ()
			{
			kktc = new KKTCodes ();
			kkte = new KKTErrorsList ();
			ofd = new OFD ();
			td = new TermsDictionary ();
			ug = new UserGuides ();
			kkts = new KKTSerial ();
			fns = new FNSerial ();
			tlvt = new TLVTags ();
			barc = new BarCodes ();
			conn = new Connectors ();
			ud = new UnicodeDescriptor ();
#if ANDROID
			ll = new LowLevel ();
#endif
			}
		}
	}
