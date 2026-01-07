using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Флаги-описатели моделей ФН
	/// </summary>
	public enum FNSerialFlags
		{
		/// <summary>
		/// ФН-1
		/// </summary>
		None = 0x00,

		/// <summary>
		/// ФН-1.1 и выше (включает замещение 13-месячных 15-месячными)
		/// </summary>
		FN11 = 0x01,

		/// <summary>
		/// ФН на 36 месяцев
		/// </summary>
		FN36 = 0x02,

		/// <summary>
		/// ФН-М (неполные 1.2)
		/// </summary>
		FNM = 0x04,

		/// <summary>
		/// ФН-1.2 (полные 1.2)
		/// </summary>
		FN12 = 0x08,

		/// <summary>
		/// ФН-1.2 с поддержкой частичной продажи
		/// </summary>
		FNPS = 0x10,

		/// <summary>
		/// Модель ФН неизвестна приложению
		/// </summary>
		UnknownFN = 0x80,
		}

	/// <summary>
	/// Класс предоставляет сведения о моделях ФН
	/// </summary>
	public class FNSerial
		{
		// Переменные
		private List<string> names = [];
		private List<string> serials = [];
		private List<FNSerialFlags> flags = [];
		private List<UInt16> addresses = [];
		private List<bool> isAllowed = [];

		private uint[] registryStats = [
			0,	// В реестре
			0,	// Из них - 36
			0,	// Известные серии ЗН
			0,	// Точно известные серии ЗН
			0,	// Серии ЗН, для которых доступно чтение
			];

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public FNSerial ()
			{
			// Получение файлов
#if !ANDROID
			byte[] data = KassArrayDBResources.FNSN;
#else
			byte[] data = RD_AAOW.Properties.Resources.FNSN;
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

				// Имя протокола
				if (values.Length != 5)
					continue;
				registryStats[2]++;

				bool newOne = !names.Contains (values[1]);
				names.Add (values[1]);
				serials.Add (values[0]);
				isAllowed.Add (values[2] == "A");
				flags.Add ((FNSerialFlags)uint.Parse (values[3], RDGenerics.HexNumberStyle));
				addresses.Add (UInt16.Parse (values[4], RDGenerics.HexNumberStyle));

				// Статистика
				if (isAllowed[isAllowed.Count - 1] && newOne)
					{
					registryStats[0]++;
					if (flags[flags.Count - 1].HasFlag (FNSerialFlags.FN36))
						registryStats[1]++;
					}

				if (!serials[serials.Count - 1].Contains ('?'))
					registryStats[3]++;

				if (addresses[addresses.Count - 1] != 0)
					registryStats[4]++;
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает модель ФН по его заводскому номеру
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		/// <returns>Модель ФН</returns>
		public string GetFNName (string FNSerialNumber)
			{
			int i = GetFNIndex (FNSerialNumber);
			if (i < 0)
				return "неизвестная модель ФН";

			string res = names[i];
			if ((flags[i] & FNSerialFlags.FN36) != 0)
				res += ", 36";
			else if ((flags[i] & FNSerialFlags.FN11) != 0)
				res += ", 15";
			else
				res += ", 13";

			return res + " месяцев";
			}

		/// <summary>
		/// Возвращает флаг, указывающий, что указанный номер позволяет определить модель ФН
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNKnown (string FNSerialNumber)
			{
			return GetFNIndex (FNSerialNumber) >= 0;
			}

		private int GetFNIndex (string SN)
			{
			for (int i = 0; i < names.Count; i++)
				if (SN.StartsWith (serials[i]))
					return i;

			return -1;
			}

		/// <summary>
		/// Возвращает флаг, указывающий на общую поддержку ФФД 1.2 моделью ФН, соответствующей указанному ЗН
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNCompatibleWithFFD12 (string FNSerialNumber)
			{
			return CheckFNState (FNSerialNumber, FNSerialFlags.FNM) > 0;
			}

		/// <summary>
		/// Возвращает флаг, указывающий на полную поддержку ФФД 1.2 моделью ФН, соответствующей указанному ЗН
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNCompletelySupportingFFD12 (string FNSerialNumber)
			{
			return CheckFNState (FNSerialNumber, FNSerialFlags.FNPS) > 0;
			}

		// Проверяет флаги ФН. Возвращает:
		// +1 при установленном флаге,
		// –1 при снятом флаге,
		// 0 при отсутствии ФН в базе,
		// +/– 2 при установленном / снятом флаге и отсутствии базового адреса чтения
		private int CheckFNState (string SN, FNSerialFlags Flags)
			{
			// Признаки отсутствия ФН в базе
			int i = GetFNIndex (SN);
			if (i < 0)
				return 0;

			int v = 1;
			if (addresses[i] == 0)
				v = 2;

			return (flags[i].HasFlag (Flags) ? v : -v);
			}

		/// <summary>
		/// Возвращает флаг, указывающий, что ФН не рассчитан на 36 месяцев
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsNotFor36Months (string FNSerialNumber)
			{
			return CheckFNState (FNSerialNumber, FNSerialFlags.FN36) <= 0;
			}

		/// <summary>
		/// Возвращает флаг, указывающий, что ФН находится в реестре ФНС
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNAllowed (string FNSerialNumber)
			{
			int i = GetFNIndex (FNSerialNumber);
			if (i < 0)
				return false;

			return isAllowed[i];
			}

		/// <summary>
		/// Возвращает флаг, указывающий, что ФН является 13-месячным, а не 15-месячным
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNExactly13 (string FNSerialNumber)
			{
			// Неизвестные ФН считать 15-месячными
			return CheckFNState (FNSerialNumber, FNSerialFlags.FN11) < 0;
			}

		/// <summary>
		/// Метод выполняет поиск по известным моделям ФН и возвращает сигнатуру ЗН в случае успеха
		/// </summary>
		/// <param name="FNModel">Часть или полное название модели ФН</param>
		/// <returns>Сигнатура ЗН или пустая строка в случае отсутствия результатов</returns>
		public string FindSignatureByName (string FNModel)
			{
			// Защита
			if (string.IsNullOrWhiteSpace (FNModel))
				return "";

			// Поиск в названиях
			string model = FNModel.ToLower ();
			int i;
			for (i = 0; i < names.Count; i++)
				{
				if (names[i].ToLower ().Contains (model))
					break;
				}

			if (i >= names.Count)
				return "";

			// Возврат
			return serials[i];
			}

		/// <summary>
		/// Возвращает сообщение об исключении ФН из реестра ФНС
		/// </summary>
		public const string FNIsNotAllowedMessage = "• Выбранный ФН исключён из реестра ФНС";

		/// <summary>
		/// Возвращает сообщение о том, что указанный ФН не рекомендуется использовать при указаных параметрах
		/// </summary>
		public const string FNIsNotRecommendedMessage =
			"• Использование выбранного ФН с указанными параметрами возможно, но не рекомендуется";

		/// <summary>
		/// Возвращает сообщение о том, что указанный ФН неприменим при указаных параметрах
		/// </summary>
		public const string FNIsNotAcceptableMessage =
			"• Выбранный ФН невозможно использовать с указанными параметрами";

		/// <summary>
		/// Возвращает сообщение о том, что указанный может быть использован при указаных параметрах
		/// </summary>
		public const string FNIsAcceptableMessage =
			"• Выбранный ФН может быть использован с указанными параметрами без ограничений";

		/// <summary>
		/// Метод возвращает флаги ФН по его заводскому номеру
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public Byte GetFNFlags (string FNSerialNumber)
			{
			int i = GetFNIndex (FNSerialNumber);
			if (i < 0)
				return (Byte)FNSerialFlags.UnknownFN;

			return (Byte)flags[i];
			}

		/// <summary>
		/// Метод возвращает адрес чтения данных из ФН по его заводскому номеру
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public UInt16 GetFNAddress (string FNSerialNumber)
			{
			int i = GetFNIndex (FNSerialNumber);
			if (i < 0)
				return 0x0200;

			return addresses[i];
			}

		/// <summary>
		/// Возвращает доступные переопределяющие базовые адреса чтения ФН
		/// </summary>
		public static UInt16[] OverrideAddresses
			{
			get
				{
				return overrideAddresses;
				}
			}
		private static UInt16[] overrideAddresses = [
			0x0B00,
			0x0C00,
			];

		/// <summary>
		/// Возвращает статистику по базе ЗН ФН
		/// </summary>
		public string RegistryStats
			{
			get
				{
#if ANDROID
				string res = "Моделей ФН в реестре ФНС" + RDLocale.RN +
					"(на " + ProgramDescription.AssemblyLastUpdate + "): " +
					registryStats[0].ToString () + RDLocale.RN;
				res += "  из них – на 36 месяцев: " +
					registryStats[1].ToString () + RDLocale.RNRN;

				res += "Известно сигнатур ЗН: " +
					registryStats[2].ToString () + RDLocale.RN;
				res += "  из них – точно: " + registryStats[3] + RDLocale.RN;
				res += "  из них чтение доступно для: " + registryStats[4];
#else
				string res = "\tМоделей ФН в реестре ФНС" + RDLocale.RN +
					"\t(на " + ProgramDescription.AssemblyLastUpdate + "):\t" +
					registryStats[0].ToString () + RDLocale.RN;
				res += "\t  из них – на 36 месяцев:\t" +
					registryStats[1].ToString () + RDLocale.RNRN;

				res += "\tИзвестно сигнатур ЗН:\t" +
					registryStats[2].ToString () + RDLocale.RN;
				res += "\t  из них – точно:\t\t" + registryStats[3] + RDLocale.RN;
				res += "\t  из них чтение доступно для:\t" + registryStats[4];
#endif

				return res;
				}
			}

		/// <summary>
		/// Метод возвращает список моделей ФН, доступных для регистрации
		/// </summary>
		public string[] EnumerateAvailableModels ()
			{
			List<string> models = [];
			for (int i = 0; i < names.Count; i++)
				if (!models.Contains (names[i]) && isAllowed[i])
					models.Add (names[i]);

			return models.ToArray ();
			}
		}
	}
