using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к наименованиям моделей ККТ
	/// </summary>
	public class KKTSerial
		{
		// Переменные
		private List<string> names = new List<string> ();
		private List<uint> serialLengths = new List<uint> ();
		private List<string> serialSamples = new List<string> ();
		private List<uint> serialOffsets = new List<uint> ();
		private List<List<FFDSupportStatuses>> ffdSupport = new List<List<FFDSupportStatuses>> ();
		private List<string> regions = new List<string> ();

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public KKTSerial ()
			{
			// Получение файла заводских номеров и моделей
#if !ANDROID
			string buf = Encoding.UTF8.GetString (RD_AAOW.Properties.TextToKKMResources.KKTSN);
#else
			string buf = Encoding.UTF8.GetString (RD_AAOW.Properties.Resources.KKTSN);
#endif
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;
			char[] splitters = new char[] { '\t' };

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					{
					string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);

					// Имя протокола
					if (values.Length != 5)
						continue;

					// Список команд
					names.Add (values[0]);
					serialLengths.Add (uint.Parse (values[1]));
					if (maxSNLength < serialLengths[serialLengths.Count - 1])
						maxSNLength = serialLengths[serialLengths.Count - 1];

					serialSamples.Add (values[2]);
					serialOffsets.Add (uint.Parse (values[3]));
					ffdSupport.Add (new List<FFDSupportStatuses> ());

					for (int i = 0; i < 3; i++)
						{
						if (values[4][i] == '1')
							ffdSupport[ffdSupport.Count - 1].Add (FFDSupportStatuses.Supported);
						else if (values[4][i] == '0')
							ffdSupport[ffdSupport.Count - 1].Add (FFDSupportStatuses.Unsupported);
						else if (values[4][i] == '+')
							ffdSupport[ffdSupport.Count - 1].Add (FFDSupportStatuses.Planned);
						else
							ffdSupport[ffdSupport.Count - 1].Add (FFDSupportStatuses.Unknown);
						}
					}
				}
			catch
				{
				throw new Exception ("KKT serial numbers data reading failure, point 1");
				}

			// Завершено
			SR.Close ();

			// Получение файла регионов
#if !ANDROID
			buf = Encoding.UTF8.GetString (RD_AAOW.Properties.TextToKKMResources.Regions);
#else
			buf = Encoding.UTF8.GetString (RD_AAOW.Properties.Resources.Regions);
#endif
			SR = new StringReader (buf);

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					regions.Add (str == "-" ? "" : str);
				}
			catch
				{
				throw new Exception ("KKT serial numbers data reading failure, point 2");
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает название региона по коду ИНН
		/// </summary>
		/// <param name="INN">ИНН пользователя</param>
		/// <returns>Возвращает название региона</returns>
		public string GetRegionName (string INN)
			{
			// Контроль
			const string ur = "неизвестный регион";
			if (INN.Length < 2)
				return ur;

			// Извлечение индекса
			int number = 0;
			try
				{
				number = int.Parse (INN.Substring (0, 2)) - 1;
				}
			catch
				{
				return ur;
				}

			// Контроль
			if ((number < 0) || (number >= regions.Count) || string.IsNullOrWhiteSpace (regions[number]))
				return ur;

			// Успешно
			return regions[number];
			}

		/// <summary>
		/// Метод возвращает модель ККТ по её заводскому номеру
		/// </summary>
		/// <param name="KKTSerialNumber">Заводской номер ККТ</param>
		public string GetKKTModel (string KKTSerialNumber)
			{
			int i = FindKKT (KKTSerialNumber);
			if (i < 0)
				return "неизвестная модель ККТ";

			return names[i];
			}

		/// <summary>
		/// Возвращает делегат для метода GetKKTModel
		/// </summary>
		public Func<string, string> GetKKTModelDelegate
			{
			get
				{
				return GetKKTModel;
				}
			}

		// Поиск ККТ по фрагментам ЗН
		private int FindKKT (string KKTSerialNumber)
			{
			for (int i = 0; i < names.Count; i++)
				if ((KKTSerialNumber.Length == serialLengths[i]) &&
					KKTSerialNumber.Substring ((int)serialOffsets[i]).StartsWith (serialSamples[i]))
					return i;

			return -1;
			}

		/// <summary>
		/// Доступные статусы поддержки ФФД
		/// </summary>
		public enum FFDSupportStatuses
			{
			/// <summary>
			/// Поддерживается
			/// </summary>
			Supported,

			/// <summary>
			/// Не поддерживается
			/// </summary>
			Unsupported,

			/// <summary>
			/// На данный момент не определён
			/// </summary>
			Unknown,

			/// <summary>
			/// Запланирован
			/// </summary>
			Planned
			}
		private FFDSupportStatuses[] emptyStatus = new FFDSupportStatuses[]
			{
			FFDSupportStatuses.Unknown, // 1.05
			FFDSupportStatuses.Unknown, // 1.1
			FFDSupportStatuses.Unknown  // 1.2
			};

		/// <summary>
		/// Метод возвращает статус поддержки ФФД для ККТ по её заводскому номеру
		/// </summary>
		/// <param name="KKTSerialNumber">Заводской номер ККТ</param>
		/// <returns>Возвращает вектор из трёх состояний для ФФД 1.05, 1.1 и 1.2</returns>
		public FFDSupportStatuses[] GetFFDSupportStatus (string KKTSerialNumber)
			{
			int i = FindKKT (KKTSerialNumber);
			if (i < 0)
				return emptyStatus;

			return ffdSupport[i].ToArray ();
			}

		/// <summary>
		/// Метод выполняет поиск по известным моделям ККТ и возвращает сигнатуру ЗН в случае успеха
		/// </summary>
		/// <param name="KKTModel">Часть или полное название модели ККТ</param>
		/// <returns>Сигнатура ЗН или пустая строка в случае отсутствия результатов</returns>
		public string FindSignatureByName (string KKTModel)
			{
			// Защита
			if (string.IsNullOrWhiteSpace (KKTModel))
				return "";

			// Поиск в названиях
			string model = KKTModel.ToLower ();
			int i;
			for (i = 0; i < names.Count; i++)
				if (names[i].ToLower ().Contains (model))
					break;

			if (i >= names.Count)
				return "";

			// Сборка сигнатуры
			string sig = "";
			for (int j = 0; j < serialOffsets[i]; j++)
				sig += "X";
			sig += serialSamples[i];
			while (sig.Length < serialLengths[i])
				sig += "X";

			// Завершено
			return sig;
			}

		/// <summary>
		/// Возвращает максимально возможную длину заводского номера
		/// </summary>
		public uint MaxSerialNumberLength
			{
			get
				{
				return maxSNLength;
				}
			}
		private uint maxSNLength = 0;
		}
	}
