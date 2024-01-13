using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс предоставляет сведения о моделях ФН
	/// </summary>
	public class FNSerial
		{
		// Переменные
		private List<string> names = new List<string> ();
		private List<string> serials = new List<string> ();
		private List<FNSerialFlags> flags = new List<FNSerialFlags> ();

		/// <summary>
		/// Флаги-описатели моделей ФН
		/// </summary>
		private enum FNSerialFlags
			{
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
			}

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public FNSerial ()
			{
			// Получение файлов
#if !ANDROID
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (RD_AAOW.Properties.KassArrayDB.FNSN);
#else
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (RD_AAOW.Properties.Resources.FNSN);
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
					if (values.Length != 3)
						continue;

					names.Add (values[1]);
					serials.Add (values[0]);
					flags.Add ((FNSerialFlags)uint.Parse (values[2], RDGenerics.HexNumberStyle));
					}
				}
			catch
				{
				throw new Exception ("FN serial numbers data reading failure, point 1");
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
			for (int i = 0; i < names.Count; i++)
				if (FNSerialNumber.StartsWith (serials[i]))
					{
					string res = names[i];
					if ((flags[i] & FNSerialFlags.FN36) != 0)
						res += ", 36";
					else if ((flags[i] & FNSerialFlags.FN11) != 0)
						res += ", 15";
					else
						res += ", 13";

					return res + " месяцев";
					}

			return "неизвестная модель ФН";
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
		/// Возвращает флаг, указывающий на поддержку ФФД 1.2 моделью ФН, соответствующей указанному ЗН
		/// </summary>
		/// <param name="FNSerialNumber">Заводской номер ФН</param>
		public bool IsFNCompatibleWithFFD12 (string FNSerialNumber)
			{
			return CheckFNState (FNSerialNumber, FNSerialFlags.FNM | FNSerialFlags.FN12) > 0;
			}

		// Проверяет флаги ФН. Возвращает +1 при установленном флаге, –1 при снятом флаге,
		// 0 при отсутствии ФН в базе
		private int CheckFNState (string SN, FNSerialFlags Flags)
			{
			int i = GetFNIndex (SN);
			if (i < 0)
				return 0;

			return ((flags[i] & Flags) != 0) ? 1 : -1;
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
			// Условный признак
			return CheckFNState (FNSerialNumber, FNSerialFlags.FNM) > 0;
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
				if (names[i].ToLower ().Contains (model))
					break;

			if (i >= names.Count)
				return "";

			// Возврат
			return serials[i];
			}

		/// <summary>
		/// Возвращает сообщение об исключении ФН из реестра ФНС
		/// </summary>
		public const string FNIsNotAllowedMessage = RDLocale.RN + "(выбранный ФН исключён из реестра ФНС)";

		/// <summary>
		/// Возвращает сообщение о том, что указанный ФН не рекомендуется использовать при указаных параметрах
		/// </summary>
		public const string FNIsNotRecommendedMessage = RDLocale.RN +
			"(не рекомендуется использовать выбранный ФН с указанными параметрами)";

		/// <summary>
		/// Возвращает сообщение о том, что указанный ФН неприменим при указаных параметрах
		/// </summary>
		public const string FNIsNotAcceptableMessage = RDLocale.RN +
			"(выбранный ФН неприменим с указанными параметрами)";
		}
	}
