using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Флаги особенностей моделей ККТ
	/// </summary>
	public enum KKTSerialFlags
		{
		/// <summary>
		/// Точно известный ЗН
		/// </summary>
		SerialIsKnown = 0x01,

		/// <summary>
		/// Модель имеет то же название в реестре, но сигнатура ЗН и реализация отличаются
		/// </summary>
		DifferentImplementations = 0x02,

		/// <summary>
		/// Модель изменила название (отсутствует в реестре, но технически не исключена из него)
		/// </summary>
		NameChanged = 0x04,

		/// <summary>
		/// Для модели неизвестна сигнатура ЗН
		/// </summary>
		UnknownSignature = 0x08,

		/// <summary>
		/// Модель исключена из реестра
		/// </summary>
		RemovedFromRegistry = 0x10,
		}

	/// <summary>
	/// Доступные статусы поддержки ФФД
	/// </summary>
	public enum FFDSupportStates
		{
		/// <summary>
		/// Статус не задан
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// ФФД 1.05 поддерживается
		/// </summary>
		Supported105 = 0x0001,

		/// <summary>
		/// ФФД 1.1 поддерживается
		/// </summary>
		Supported11 = 0x0002,

		/// <summary>
		/// ФФД 1.2 поддерживается
		/// </summary>
		Supported12 = 0x0004,

		/// <summary>
		/// ФФД 1.05 не поддерживается
		/// </summary>
		Unsupported105 = 0x0010,

		/// <summary>
		/// ФФД 1.1 не поддерживается
		/// </summary>
		Unsupported11 = 0x0020,

		/// <summary>
		/// ФФД 1.2 не поддерживается
		/// </summary>
		Unsupported12 = 0x0040,

		/// <summary>
		/// ФФД 1.05 планируется
		/// </summary>
		Planned105 = 0x0100,

		/// <summary>
		/// ФФД 1.1 планируется
		/// </summary>
		Planned11 = 0x0200,

		/// <summary>
		/// ФФД 1.2 планируется
		/// </summary>
		Planned12 = 0x0400,
		}

	/// <summary>
	/// Класс предоставляет сведения о моделях ККТ
	/// </summary>
	public class KKTSerial
		{
		// Переменные
		private List<string> names = [];
		private List<uint> serialLengths = [];
		private List<string> serialSamples = [];
		private List<uint> serialOffsets = [];
		private List<FFDSupportStates> ffdSupport = [];
		private List<KKTSerialFlags> serialFlags = [];

		private List<string> regions = [];

		private uint[] registryStats = [
			0,	// Всего
			0, 0, 0,	// Поддержка ФФД
			0,	// Известные сигнатуры
			0,	// Точно известные сигнатуры
			0,	// Исключены из реестра
			];

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public KKTSerial ()
			{
			// Получение файла заводских номеров и моделей
#if !ANDROID
			byte[] data = KassArrayDBResources.KKTSN;
#else
			byte[] data = RD_AAOW.Properties.Resources.KKTSN;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;
			char[] splitters = ['\t'];

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);

				// Защита
				if (values.Length < 3)
					continue;

				KKTSerialFlags flags = (KKTSerialFlags)byte.Parse (values[2], RDGenerics.HexNumberStyle);
				serialFlags.Add (flags);

				if (!flags.HasFlag (KKTSerialFlags.DifferentImplementations) &&
					!flags.HasFlag (KKTSerialFlags.NameChanged))
					registryStats[0]++;

				FFDSupportStates state = FFDSupportStates.None;
				for (int i = 0; i < ffdNames.Length; i++)
					{
					switch (values[1][i])
						{
						case '1':
							state |= (FFDSupportStates)(1 << i);

							if (!flags.HasFlag (KKTSerialFlags.DifferentImplementations) &&
								!flags.HasFlag (KKTSerialFlags.NameChanged))
								registryStats[1 + i]++;
							break;

						case '0':
							state |= (FFDSupportStates)((1 << i) << 4);
							break;

						case '+':
							state |= (FFDSupportStates)((1 << i) << 8);
							break;
						}
					}

				// Список команд
				names.Add (values[0]);

				if (!flags.HasFlag (KKTSerialFlags.UnknownSignature) &&
					!flags.HasFlag (KKTSerialFlags.NameChanged))
					{
					serialLengths.Add (uint.Parse (values[3]));
					if (maxSNLength < serialLengths[serialLengths.Count - 1])
						maxSNLength = serialLengths[serialLengths.Count - 1];

					serialSamples.Add (values[4]);
					serialOffsets.Add (uint.Parse (values[5]));

					registryStats[1 + ffdNames.Length]++;
					}
				else
					{
					serialLengths.Add (0);
					serialSamples.Add ("\x1");
					serialOffsets.Add (0);
					}
				ffdSupport.Add (state);

				if (flags.HasFlag (KKTSerialFlags.SerialIsKnown) &&
					!flags.HasFlag (KKTSerialFlags.NameChanged))
					registryStats[2 + ffdNames.Length]++;
				if (flags.HasFlag (KKTSerialFlags.RemovedFromRegistry))
					registryStats[3 + ffdNames.Length]++;
				}

			// Завершено
			SR.Close ();

			// Получение файла регионов
#if !ANDROID
			byte[] data2 = KassArrayDBResources.Regions;
#else
			byte[] data2 = RD_AAOW.Properties.Resources.Regions;
#endif
			buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data2);
			SR = new StringReader (buf);

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				regions.Add (str == "-" ? "" : str);

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
			int number;
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

			return names[i] + (serialFlags[i].HasFlag (KKTSerialFlags.SerialIsKnown) ? "" : " (неточно)");
			}

		// Поиск ККТ по фрагментам ЗН
		private int FindKKT (string KKTSerialNumber)
			{
			if (KKTSerialNumber.StartsWith ('\x1'))
				return int.Parse (KKTSerialNumber.Substring (1));

			for (int i = 0; i < names.Count; i++)
				if ((KKTSerialNumber.Length == serialLengths[i]) &&
					KKTSerialNumber.Substring ((int)serialOffsets[i]).StartsWith (serialSamples[i]))
					return i;

			return -1;
			}

		/// <summary>
		/// Метод возвращает статус поддержки ФФД для ККТ по её заводскому номеру
		/// </summary>
		/// <param name="KKTSerialNumber">Заводской номер ККТ</param>
		/// <returns>Возвращает вектор из трёх состояний для ФФД 1.05, 1.1 и 1.2</returns>
		public string GetFFDSupportStatus (string KKTSerialNumber)
			{
			int i = FindKKT (KKTSerialNumber);
			if (i < 0)
				return "";

			if (serialFlags[i].HasFlag (KKTSerialFlags.RemovedFromRegistry))
				return "(исключена из реестра ККТ)";

			string s = "";
			string us = "";
			FFDSupportStates state = ffdSupport[i];

			for (int j = 0; j < ffdNames.Length; j++)
				{
				if (state.HasFlag ((FFDSupportStates)(1 << j)))
					s += (ffdNames[j] + " ");
				else if (state.HasFlag ((FFDSupportStates)((1 << j) << 4)))
					us += (ffdNames[j] + " ");
				else if (state.HasFlag ((FFDSupportStates)((1 << j) << 8)))
					s += (ffdNames[j] + "&(план) ");
				}

			if (string.IsNullOrEmpty (s))
				s = "только&1.0";
			if (string.IsNullOrEmpty (us))
				us = "нет";

			return "Подд. ФФД: " + s.Trim ().Replace (" ", ", ").Replace ("&", " ") + RDLocale.RN +
				"Неподд. ФФД: " + us.Trim ().Replace (" ", ", ");
			}
		private static string[] ffdNames = ["1.05", "1.1", "1.2"];

		/// <summary>
		/// Метод выполняет поиск по известным моделям ККТ и возвращает сигнатуру ЗН в случае успеха
		/// </summary>
		/// <param name="KKTModel">Часть или полное название модели ККТ</param>
		/// <returns>Сигнатура ЗН или пустая строка в случае отсутствия результатов.
		/// При неизвестной сигнатуре ЗН возвращает номер модели в списке с префиксом U+0001</returns>
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

			if (serialFlags[i].HasFlag (KKTSerialFlags.UnknownSignature))
				return "\x1" + i.ToString ();

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

		/// <summary>
		/// Возвращает статистику по базе ЗН ККТ
		/// </summary>
		public string RegistryStats
			{
			get
				{
#if ANDROID
				string res = "Моделей ККТ в реестре ФНС" + RDLocale.RN +
					"(на " + ProgramDescription.AssemblyLastUpdate + "): " +
					registryStats[0].ToString () + RDLocale.RNRN;
				res += "Из них поддерживают:" + RDLocale.RN;

				for (int i = 0; i < ffdNames.Length; i++)
					res += "  ФФД " + ffdNames[i] + ": " +
						registryStats[1 + i].ToString () + RDLocale.RN;

				res += RDLocale.RN + "Известно сигнатур ЗН: " +
					registryStats[ffdNames.Length + 1] + RDLocale.RN;
				res += "  из них – точно: " + registryStats[ffdNames.Length + 2] + RDLocale.RN;

				res += RDLocale.RN + "Исключены из реестра: " +
					registryStats[ffdNames.Length + 3];
#else
				string res = "\tМоделей ККТ в реестре ФНС" + RDLocale.RN +
					"\t(на " + ProgramDescription.AssemblyLastUpdate + "):\t" +
					registryStats[0].ToString () + RDLocale.RNRN;
				res += "\tИз них поддерживают:" + RDLocale.RN;

				for (int i = 0; i < ffdNames.Length; i++)
					res += "\t  ФФД " + ffdNames[i] + ":  \t\t" +
						registryStats[1 + i].ToString () + RDLocale.RN;

				res += RDLocale.RN + "\tИзвестно сигнатур ЗН:\t" +
					registryStats[ffdNames.Length + 1] + RDLocale.RN;
				res += "\t  из них – точно:\t\t" + registryStats[ffdNames.Length + 2] + RDLocale.RN;

				res += RDLocale.RN + "\tИсключены из реестра:\t" +
					registryStats[ffdNames.Length + 3];
#endif

				return res;
				}
			}
		}
	}
