using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к распиновкам интерфейсных кабелей
	/// </summary>
	public class Connectors
		{
		// Переменные
		private List<string> connectorsNames = new List<string> (),
			connectorsDescriptions = new List<string> ();
		private List<string> cablesNames = new List<string> (),
			cablesLeftSides = new List<string> (),
			cablesLeftPins = new List<string> (),
			cablesRightSides = new List<string> (),
			cablesRightPins = new List<string> ();
		private List<uint> cablesLeftDescriptions = new List<uint> (),
			cablesRightDescriptions = new List<uint> ();

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public Connectors ()
			{
			// Получение файлов
#if !ANDROID
			byte[] s1 = Properties.TextToKKMResources.Connectors;
#else
			byte[] s1 = Properties.Resources.Connectors;
#endif
			string buf = Encoding.UTF8.GetString (s1);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;
			char[] splitters = new char[] { '\t' };
			bool connectors = true;

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					{
					// Переключение
					if (string.IsNullOrWhiteSpace (str))
						{
						connectors = false;
						continue;
						}

					// Чтение разъёмов
					if (connectors)
						{
						connectorsNames.Add (str);
						connectorsDescriptions.Add (SR.ReadLine ());
						}

					// Чтение кабелей
					else
						{
						cablesNames.Add (str);

						string[] values = SR.ReadLine ().Split (splitters, StringSplitOptions.RemoveEmptyEntries);
						cablesLeftSides.Add (values[0]);
						cablesLeftDescriptions.Add (uint.Parse (values[1]));

						values = SR.ReadLine ().Split (splitters, StringSplitOptions.RemoveEmptyEntries);
						cablesRightSides.Add (values[0]);
						cablesRightDescriptions.Add (uint.Parse (values[1]));

						cablesLeftPins.Add (SR.ReadLine ().Replace ("\t", " | "));
						cablesRightPins.Add (SR.ReadLine ().Replace ("\t", " | "));
						}
					}
				}
			catch
				{
				throw new Exception ("Connectors data reading failure, point 1");
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Возвращает список названий кабелей
		/// </summary>
		public List<string> GetCablesNames ()
			{
			return new List<string> (cablesNames);
			}

		/// <summary>
		/// Возвращает точку подключения и тип коннектора
		/// </summary>
		/// <param name="CableNumber">Номер кабеля из списка</param>
		/// <param name="Right">Флаг правой стороны</param>
		public string GetCableConnector (uint CableNumber, bool Right)
			{
			return GetCableInfo (CableNumber, 0, Right);
			}

		/// <summary>
		/// Возвращает описание коннектора
		/// </summary>
		/// <param name="CableNumber">Номер кабеля из списка</param>
		/// <param name="Right">Флаг правой стороны</param>
		public string GetCableConnectorDescription (uint CableNumber, bool Right)
			{
			return GetCableInfo (CableNumber, 1, Right);
			}

		/// <summary>
		/// Возвращает распиновку коннектора
		/// </summary>
		/// <param name="CableNumber">Номер кабеля из списка</param>
		/// <param name="Right">Флаг правой стороны</param>
		public string GetCableConnectorPins (uint CableNumber, bool Right)
			{
			return GetCableInfo (CableNumber, 2, Right);
			}

		// Общий метод запроса информации о кабеле
		private string GetCableInfo (uint CableNumber, uint InfoField, bool Right)
			{
			// Контроль
			if (CableNumber >= cablesNames.Count)
				return "";
			int i = (int)CableNumber;

			// Выбор варианта
			switch (InfoField)
				{
				// Точка подключения и тип коннектора
				case 0:
					if (Right)
						return cablesRightSides[i] + ": " + connectorsNames[(int)cablesRightDescriptions[i]];
					else
						return cablesLeftSides[i] + ": " + connectorsNames[(int)cablesLeftDescriptions[i]];

				// Описание коннектора
				case 1:
					if (Right)
						return connectorsNames[(int)cablesRightDescriptions[i]] + ": " +
							connectorsDescriptions[(int)cablesRightDescriptions[i]];
					else
						return connectorsNames[(int)cablesLeftDescriptions[i]] + ": " +
							connectorsDescriptions[(int)cablesLeftDescriptions[i]];

				// Распиновка коннектора
				case 2:
					if (Right)
						return cablesRightPins[i];
					else
						return cablesLeftPins[i];

				// Потенциально невозможный вариант
				default:
					return "";
				}
			}
		}
	}
