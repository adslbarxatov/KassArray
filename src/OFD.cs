using System;
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

		private const string notFound = "[не найдено]";
		private const string notFoundFlag = "?";
		private const string equivalentFlag = "=";

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
			char[] splitters = ['\t'];
			uint line = 0;

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				line++;
				string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);
				if (values.Length != 11)
					continue;

				inn.Add (values[0]);
				names.Add (values[1]);

				dnsNames.Add (values[2] == notFoundFlag ? notFound : values[2]);
				ip.Add (values[3] == notFoundFlag ? notFound : values[3]);
				ports.Add (values[4] == notFoundFlag ? "[???]" : values[4]);
				emails.Add (values[5] == notFoundFlag ? notFound : values[5]);
				links.Add (values[6] == notFoundFlag ? notFound : values[6]);

				if (values[7] == notFoundFlag)
					dnsNamesM.Add (notFound);
				else if (values[7] == equivalentFlag)
					dnsNamesM.Add (dnsNames[dnsNames.Count - 1]);
				else
					dnsNamesM.Add (values[7]);

				if (values[8] == notFoundFlag)
					ipM.Add (notFound);
				else if (values[8] == equivalentFlag)
					ipM.Add (ip[ip.Count - 1]);
				else
					ipM.Add (values[8]);

				if (values[9] == notFoundFlag)
					portsM.Add ("[???]");
				else if (values[9] == equivalentFlag)
					portsM.Add (ports[ports.Count - 1]);
				else
					portsM.Add (values[9]);

				disabledMessages.Add ((values[10] == "-") ? "" : values[10]);
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
		/// Метод возвращает список ИНН ОФД
		/// </summary>
		public List<string> GetOFDINNs ()
			{
			return new List<string> (inn);
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
		}
	}
