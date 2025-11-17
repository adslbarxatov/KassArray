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
	/// Доступные типы бумаги для принтеров ККТ
	/// </summary>
	public enum KKTPaperTypes
		{
		/// <summary>
		/// 57 мм термо
		/// </summary>
		Termo57 = 5,

		/// <summary>
		/// 80 мм термо
		/// </summary>
		Termo80 = 8,

		/// <summary>
		/// 69 мм без термослоя
		/// </summary>
		NonTermo69,

		/// <summary>
		/// Зависит от принтера
		/// </summary>
		DependsOnPrinter = 1,

		/// <summary>
		/// Не используется
		/// </summary>
		NotUsed = 0,

		/// <summary>
		/// Неизвестный тип бумаги
		/// </summary>
		Unknown = -1,
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
		private List<string> serialVersions = [];
		private List<KKTPaperTypes> serialPaperWidths = [];
		private List<byte> serialPaperLengths = [];
		private List<string> serialTSPI = [];

		private List<string> regions = [];

		private uint[] registryStats = [
			0,	// Всего
			0, 0, 0,	// Поддержка ФФД
			0,	// Известные сигнатуры
			0,	// Точно известные сигнатуры
			0,	// Исключены из реестра
			];

		private int lastSearchOffset = 0;

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public KKTSerial ()
			{
			// Получение файла ТСПИ
#if !ANDROID
			byte[] tspiData = KassArrayDBResources.TSPI;
#else
			byte[] tspiData = RD_AAOW.Properties.Resources.TSPI;
#endif
			string tspi = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (tspiData);
			string[] tspiValues = tspi.Split (['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

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
			SR.ReadLine (); // Заголовок
			SR.ReadLine ();
			while ((str = SR.ReadLine ()) != null)
				{
				string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);

				// Защита
				if (values.Length < 6)
					throw new Exception ("Invalid data at point 1, debug is required");

				KKTSerialFlags flags = (KKTSerialFlags)byte.Parse (values[5], RDGenerics.HexNumberStyle);
				serialFlags.Add (flags);

				if (!flags.HasFlag (KKTSerialFlags.DifferentImplementations))
					registryStats[0]++;

				FFDSupportStates state = FFDSupportStates.None;
				for (int i = 0; i < ffdNames.Length; i++)
					{
					switch (values[3][i])
						{
						case 'S':
							state |= (FFDSupportStates)(1 << i);

							if (!flags.HasFlag (KKTSerialFlags.DifferentImplementations) &&
								!flags.HasFlag (KKTSerialFlags.NameChanged))
								registryStats[1 + i]++;
							break;

						case 'U':
							state |= (FFDSupportStates)((1 << i) << 4);
							break;

						case 'P':
							state |= (FFDSupportStates)((1 << i) << 8);
							break;

						default:
							throw new Exception ("Invalid data at point 2, debug is required");
						}
					}

				// Список моделей
				names.Add (values[0]);
				serialVersions.Add (values[1]);

				switch (values[2])
					{
					case "?":
						serialPaperWidths.Add (KKTPaperTypes.Unknown);
						serialPaperLengths.Add (0);
						break;

					case "P":
						serialPaperWidths.Add (KKTPaperTypes.DependsOnPrinter);
						serialPaperLengths.Add (0);
						break;

					case "N":
						serialPaperWidths.Add (KKTPaperTypes.NotUsed);
						serialPaperLengths.Add (0);
						break;

					default:
						uint paper = uint.Parse (values[2]);
						serialPaperWidths.Add ((KKTPaperTypes)(paper / 100));
						serialPaperLengths.Add ((byte)(paper % 100));
						break;
					}

				if (!flags.HasFlag (KKTSerialFlags.UnknownSignature) &&
					!flags.HasFlag (KKTSerialFlags.NameChanged))
					{
					serialLengths.Add (uint.Parse (values[6]));
					if (maxSNLength < serialLengths[serialLengths.Count - 1])
						maxSNLength = serialLengths[serialLengths.Count - 1];

					serialSamples.Add (values[7]);
					serialOffsets.Add (uint.Parse (values[8]));

					registryStats[1 + ffdNames.Length]++;
					}
				else
					{
					serialLengths.Add (0);
					serialSamples.Add ("\x2");
					serialOffsets.Add (0);
					}
				ffdSupport.Add (state);

				if (flags.HasFlag (KKTSerialFlags.SerialIsKnown) &&
					!flags.HasFlag (KKTSerialFlags.NameChanged))
					registryStats[2 + ffdNames.Length]++;
				if (flags.HasFlag (KKTSerialFlags.RemovedFromRegistry))
					registryStats[3 + ffdNames.Length]++;

				switch (values[4])
					{
					case "-":
						serialTSPI.Add ("нет сведений");
						break;

					case "U":
						serialTSPI.Add ("не поддерживается");
						break;

					default:
						int tspiIdx = int.Parse (values[4]) - 1;
						serialTSPI.Add (tspiValues[tspiIdx].Substring (2));
						break;
					}
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

			lastSearchOffset = i;
			return names[i] + (serialFlags[i].HasFlag (KKTSerialFlags.SerialIsKnown) ? "" : " (неточно)");
			}

		// Поиск ККТ по фрагментам ЗН
		private int FindKKT (string KKTSerialNumber)
			{
			if (KKTSerialNumber.StartsWith ('\x1'))
				return int.Parse (KKTSerialNumber.Substring (1));

			for (int i = 0; i < names.Count; i++)
				if (!serialFlags[i].HasFlag (KKTSerialFlags.NameChanged) &&
					(KKTSerialNumber.Length == serialLengths[i]) &&
					KKTSerialNumber.Substring ((int)serialOffsets[i]).StartsWith (serialSamples[i]))
					return i;

			return -1;
			}

		/// <summary>
		/// Метод возвращает характеристики ККТ, найденной при последнем поиске
		/// </summary>
		/// <returns>Возвращает строку с описанием ККТ</returns>
		public string GetKKTDescription ()
			{
			/*string sig = FindSignatureByName (KKTModelOrSerial, false);
			if (string.IsNullOrWhiteSpace (sig))
				sig = KKTModelOrSerial;

			int i = FindKKT (sig);
			if (i < 0)
				return "";*/

			// Общие сведения
			int i = lastSearchOffset;
			string res = "Модель ККТ: " + names[i] + RDLocale.RN + "Статус: ";

			// Поддержка ФФД
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

			// Состояние в реестре ФНС
			if (serialFlags[i].HasFlag (KKTSerialFlags.RemovedFromRegistry))
				res += "! исключена из реестра ФНС !";
			else if (!s.Contains (ffdNames[2]))
				res += "! присутствует в реестре ФНС, но не обновляется !";
			else
				res += "присутствует в реестре ФНС";
			res += (RDLocale.RNRN + "Актуальная версия ПО: " + serialVersions[i] + RDLocale.RN);

			// Поддержка ФФД
			res += ("  Поддерживаемые ФФД: " + s.Trim ().Replace (" ", ", ").Replace ("&", " ") + RDLocale.RN);
			res += ("  Неподдерживаемые ФФД: " + us.Trim ().Replace (" ", ", ") + RDLocale.RN);
			res += ("  Совместимое ТС ПИоТ: " + serialTSPI[i] + RDLocale.RNRN);

			// Чековая лента
			res += "Чековая лента: ";
			bool addLength = true;
			switch (serialPaperWidths[i])
				{
				case KKTPaperTypes.DependsOnPrinter:
					res += "зависит от печатающего устройства";
					addLength = false;
					break;

				case KKTPaperTypes.Unknown:
					res += "(нет сведений)";
					addLength = false;
					break;

				case KKTPaperTypes.NotUsed:
					res += "не используется";
					addLength = false;
					break;

				case KKTPaperTypes.Termo57:
					res += "57 мм, термо";
					break;

				case KKTPaperTypes.Termo80:
					res += "80 мм, термо";
					break;

				case KKTPaperTypes.NonTermo69:
					res += "69 мм, без термослоя";
					break;
				}

			if (addLength)
				res += (RDLocale.RN + "  Намотка, точно помещающаяся в принтер: " +
					serialPaperLengths[i].ToString () + " м");

			// Готово
			return res;
			}
		private static string[] ffdNames = ["1.05", "1.1", "1.2"];

		/// <summary>
		/// Метод выполняет поиск по известным моделям ККТ и возвращает сигнатуру ЗН в случае успеха
		/// </summary>
		/// <param name="KKTModel">Часть или полное название модели ККТ</param>
		/// <param name="Continue">Флаг продолжения поиска в порядке следования моделей</param>
		/// <returns>Сигнатура ЗН или пустая строка в случае отсутствия результатов.
		/// При неизвестной сигнатуре ЗН возвращает номер модели в списке с префиксом U+0001</returns>
		public string FindSignatureByName (string KKTModel, bool Continue)
			{
			// Защита
			if (string.IsNullOrWhiteSpace (KKTModel))
				return "";

			if (!Continue)
				lastSearchOffset = 0;
			else
				lastSearchOffset++;

			// Поиск в названиях
			string model = KKTModel.ToLower ();
			int i;
			/*for (i = 0; i < names.Count; i++)
				if (!serialFlags[i].HasFlag (KKTSerialFlags.NameChanged) &&
					names[i].ToLower ().Contains (model))
					break;*/

			for (i = 0; i < names.Count; i++)
				{
				if (!serialFlags[i].HasFlag (KKTSerialFlags.NameChanged) &&
					names[(i + lastSearchOffset) % names.Count].ToLower ().Contains (model))
					break;
				}

			if (i >= names.Count)
				return "";
			lastSearchOffset = (i + lastSearchOffset) % names.Count;
			i = lastSearchOffset;

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

		/// <summary>
		/// Метод возвращает последнюю версию ПО ККТ по её заводскому номеру
		/// </summary>
		/// <param name="KKTSerialNumber">Заводской номер ККТ</param>
		/// <returns>Возвращает строку с версией или пустую строку, если ККТ не была найдена</returns>
		public string GetSoftwareVersion (string KKTSerialNumber)
			{
			int i = FindKKT (KKTSerialNumber);
			if (i < 0)
				return "";

			return serialVersions[i];
			}

		/// <summary>
		/// Метод возвращает список моделей ККТ, доступных для регистрации
		/// </summary>
		public string[] EnumerateAvailableModels ()
			{
			List<string> models = [];
			for (int i = 0; i < names.Count; i++)
				if (!serialFlags[i].HasFlag (KKTSerialFlags.DifferentImplementations) &&
					!serialFlags[i].HasFlag (KKTSerialFlags.NameChanged) &&
					!serialFlags[i].HasFlag (KKTSerialFlags.RemovedFromRegistry))
					models.Add (names[i]);

			return models.ToArray ();
			}

		/// <summary>
		/// Метод возвращает список регионов РФ
		/// </summary>
		public string[] EnumerateAvailableRegions ()
			{
			List<string> regs = [];
			for (int i = 0; i < regions.Count; i++)
				if (!string.IsNullOrWhiteSpace (regions[i]))
					regs.Add ((i + 1).ToString ("D2") + " – " + regions[i]);

			return regs.ToArray ();
			}
		}
	}
