using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к параметрам ОФД
	/// </summary>
	public class OFD
		{
		// Переменные
		private List<string> names = [];
		private List<string> inn = [];

		private List<string> dnsNames = [];
		private List<string> ip = [];
		private List<string> ports = [];
		private List<string> emails = [];
		private List<string> links = [];

		private List<string> dnsNamesM = [];
		private List<string> ipM = [];
		private List<string> portsM = [];

		private List<string> disabledMessages = [];

		private int lastSearchOffset = 0;
		private const string notFound = "[не найдено]";
		private const string notFoundFlag = "?";
		private const string equivalentFlag = "=";
		private const string activeFlag = "-";

		private List<string> searchList = [];

		/// <summary>
		/// Общий адрес сайта ФНС
		/// </summary>
		public const string FNSSite = "www.nalog.gov.ru";

		/// <summary>
		/// Общий адрес сервера обновления ключей проверки кодов маркировки
		/// </summary>
		public const string OKPSite = "prod01.okp-fn.ru";

		/// <summary>
		/// Общий IP-адрес сервера обновления ключей проверки кодов маркировки
		/// </summary>
		public const string OKPIP = "31.44.83.184";

		/// <summary>
		/// Общий порт сервера обновления ключей проверки кодов маркировки
		/// </summary>
		public const string OKPPort = "26101";

		/// <summary>
		/// Общий адрес сервера ГИС МТ
		/// </summary>
		public const string CDNSite = "cdn.crpt.ru";

		/// <summary>
		/// Конструктор. Инициализирует таблицу ОФД
		/// </summary>
		public OFD ()
			{
			// Получение файла символов
#if !ANDROID
			byte[] data = KassArrayDBResources.OFD;
#else
			byte[] data = RD_AAOW.Properties.Resources.OFD;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				if (str.StartsWith ('~'))
					continue;

				inn.Add (str);
				names.Add (SR.ReadLine ());

				str = SR.ReadLine ();
				dnsNames.Add (str == notFoundFlag ? notFound : str);
				str = SR.ReadLine ();
				ip.Add (str == notFoundFlag ? notFound : str);
				str = SR.ReadLine ();
				ports.Add (str == notFoundFlag ? "[???]" : str);
				str = SR.ReadLine ();
				emails.Add (str == notFoundFlag ? notFound : str);
				str = SR.ReadLine ();
				links.Add (str == notFoundFlag ? notFound : str);

				str = SR.ReadLine ();
				if (str == notFoundFlag)
					dnsNamesM.Add (notFound);
				else if (str == equivalentFlag)
					dnsNamesM.Add (dnsNames[dnsNames.Count - 1]);
				else
					dnsNamesM.Add (str);

				str = SR.ReadLine ();
				if (str == notFoundFlag)
					ipM.Add (notFound);
				else if (str == equivalentFlag)
					ipM.Add (ip[ip.Count - 1]);
				else
					ipM.Add (str);

				str = SR.ReadLine ();
				if (str == notFoundFlag)
					portsM.Add ("[???]");
				else if (str == equivalentFlag)
					portsM.Add (ports[ports.Count - 1]);
				else
					portsM.Add (str);

				str = SR.ReadLine ();
				disabledMessages.Add ((str == activeFlag) ? "" : str);
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список названий ОФД
		/// </summary>
		/// <param name="EnabledOnly">Флаг указывает на возвращение только активных ОФД</param>
		public List<string> GetOFDNames (bool EnabledOnly)
			{
			List<string> res = [];

			for (int i = 0; i < names.Count; i++)
				if (!EnabledOnly || string.IsNullOrWhiteSpace (disabledMessages[i]))
					res.Add (names[i]);

			return res;
			}

		/// <summary>
		/// Метод возвращает параметры указанного ОФД
		/// </summary>
		/// <param name="INN">ИНН требуемого ОФД</param>
		/// <returns>Параметры ОФД в порядке:
		/// ИНН,
		/// название,
		/// DNS-имя,
		/// IP,
		/// порт,
		/// E-mail,
		/// сайт,
		/// DNS-имя (маркировка),
		/// IP (маркировка),
		/// порт (маркировка),
		/// сообщение об аннулировании</returns>
		public List<string> GetOFDParameters (string INN)
			{
			// Защита
			if (INN.Contains ("0000000000"))
				return ["?", "Без ОФД", "", "", "", "", "", "", "", "", ""];

			if (!inn.Contains (INN))
				return ["?", "Неизвестный ОФД", "", "", "", "", "", "", "", "", ""];

			// Возврат
			int i = inn.IndexOf (INN);
			return [inn[i], names[i],
				dnsNames[i], ip[i], ports[i], emails[i], links[i],
				dnsNamesM[i], ipM[i], portsM[i], disabledMessages[i]];
			}

		/// <summary>
		/// Метод возвращает параметры ОФД по его ИНН
		/// </summary>
		/// <param name="INN">ИНН ОФД</param>
		/// <param name="Parameter">Номер параметра ОФД</param>
		public string GetOFDByINN (string INN, uint Parameter)
			{
			List<string> values = GetOFDParameters (INN);
			if (Parameter >= values.Count)
				return "";

			return values[(int)Parameter];
			}

		/// <summary>
		/// Метод возвращает ИНН ОФД по названию
		/// </summary>
		/// <param name="OFDName">Название ОФД</param>
		/// <returns>ИНН ОФД</returns>
		public string GetOFDINNByName (string OFDName)
			{
			// Защита
			if (!names.Contains (OFDName))
				return "";

			// Возврат
			return inn[names.IndexOf (OFDName)];
			}

		/// <summary>
		/// Метод выполняет поиск указанного ключевого слова в списке ОФД
		/// </summary>
		/// <param name="Criteria">Ключевое слово для поиска</param>
		/// <param name="Continue">Флаг продолжения поиска в порядке следования терминов</param>
		/// <returns>Возвращает номер найденного ОФД либо -1, если ОФД не был найден</returns>
		public int FindNext (string Criteria, bool Continue)
			{
			string criteria = Criteria.ToLower ();
			if (!Continue)
				lastSearchOffset = 0;
			else
				lastSearchOffset++;

			// Поиск
			if (searchList.Count < 1)
				{
				searchList.AddRange (names);
				for (int i = 0; i < searchList.Count; i++)
					searchList[i] = searchList[i].ToLower ();

				searchList.AddRange (inn);
				}

			for (int i = 0; i < searchList.Count; i++)
				{
				if (searchList[(i + lastSearchOffset) % searchList.Count].Contains (criteria))
					{
					lastSearchOffset = (i + lastSearchOffset) % searchList.Count;
					return (lastSearchOffset % (searchList.Count / 2));
					}
				}

			// Не найдено
			return -1;
			}
		}
	}
