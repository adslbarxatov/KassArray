using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс предоставляет справочник описаний для блоков символов Unicode
	/// </summary>
	public class UnicodeDescriptor
		{
		// Списки
		private List<ulong> lefts = new List<ulong> ();
		private List<ulong> rights = new List<ulong> ();
		private List<string> descriptions = new List<string> ();

		/// <summary>
		/// Конструктор. Выполняет загрузку справочника
		/// </summary>
		public UnicodeDescriptor ()
			{
			// Получение файла
			var enc = RDGenerics.GetEncoding (RDEncodings.UTF8);
#if !ANDROID
			string ud = enc.GetString (RD_AAOW.Properties.KassArrayDB.Unicodes);
#else
			string ud = enc.GetString (RD_AAOW.Properties.Resources.Unicodes);
#endif
			StringReader udSR = new StringReader (ud);


			// Загрузка
			char[] udSplitter = new char[] { '\t' };
			string s;
			while ((s = udSR.ReadLine ()) != null)
				{
				string[] values = s.Split (udSplitter, StringSplitOptions.RemoveEmptyEntries);
				switch (values.Length)
					{
					default:
						continue;

					case 2:
						if (values[1] == "*")
							descriptions.Add ("(неиспользуемый блок)");
						else
							descriptions.Add (values[1]);

						lefts.Add (ulong.Parse (values[0], NumberStyles.HexNumber));
						rights.Add (0);
						/*rights.Add (ulong.Parse (values[2], NumberStyles.HexNumber));*/
						break;

					case 1:
						rights[rights.Count - 1] = ulong.Parse (values[0], NumberStyles.HexNumber);
						break;
					}
				}

			// Готово. Протягивание правых краёв диапазонов
			udSR.Close ();
			for (int i = descriptions.Count - 2; i >= 0; i--)
				rights[i] = lefts[i + 1] - 1;

			/*// !!! КОПИЯ !!!
			FileStream FSO = new FileStream ("Unicodes.new", FileMode.Create);
			StreamWriter SWO = new StreamWriter (FSO, RDGenerics.GetEncoding (RDEncodings.UTF8));

			for (int i = 0; i < descriptions.Count; i++)
				{
				if (descriptions[i].StartsWith ("("))
					SWO.Write (lefts[i].ToString ("X6") + "\t*\n");
				else
					SWO.Write (lefts[i].ToString ("X6") + "\t" +
						descriptions[i] + "\n");
				}

			SWO.Close ();
			FSO.Close ();*/
			}

		/// <summary>
		/// Возвращает максимальное допустимое значение для конвертора Unicode
		/// </summary>
		public ulong MaxValue
			{
			get
				{
				return rights[rights.Count - 1];
				}
			}

		/// <summary>
		/// Метод запрашивает описание блока, в который входит указанный символ
		/// </summary>
		/// <param name="UnicodeCharacter">Код символа Unicode</param>
		/// <returns>Описание блока Unicode</returns>
		public string GetDescription (ulong UnicodeCharacter)
			{
			if (UnicodeCharacter > MaxValue)
				return "(символ вне диапазона Unicode)";

			int i;
			for (i = 0; i < descriptions.Count; i++)
				if ((UnicodeCharacter >= lefts[i]) && (UnicodeCharacter <= rights[i]))
					break;

			return descriptions[i];
			}

		/// <summary>
		/// Метод возвращает начало диапазона Unicode по его описанию
		/// </summary>
		/// <param name="Description">Описание блока или его фрагмент</param>
		/// <returns>Возвращает true в случае обнаружения</returns>
		public ulong FindRange (string Description)
			{
			// Защита
			if (string.IsNullOrWhiteSpace (Description))
				return 0;
			string description = Description.ToLower ();

			// Поиск по описанию
			lastIndex++;
			int i;
			for (i = 0; i < descriptions.Count; i++)
				if (descriptions[(i + lastIndex) % descriptions.Count].ToLower ().Contains (description))
					{
					i = lastIndex = (i + lastIndex) % descriptions.Count;
					break;
					}

			// Проверка
			if ((i < 0) || (i >= descriptions.Count))
				return 0;

			// Найдено
			return lefts[i];
			}
		private int lastIndex = 0;
		}

	/// <summary>
	/// Класс обеспечивает доступ к функционалу преобразования числовых и символьных данных
	/// </summary>
	public static class DataConvertors
		{
		/// <summary>
		/// Метод получает число из строки, учитывая возможный шестнадцатеричный префикс
		/// </summary>
		/// <param name="Value">Строка, содержащая десятичное или шестнадцатеричное число</param>
		public static double GetNumber (string Value)
			{
			// Прямое преобразование
			double v = double.NaN;
			try
				{
				v = ulong.Parse (Value);
				}
			catch { }

			if (!double.IsNaN (v))
				return v;

			// Преобразование из hex
			string res = Value.ToLower ();
			if (res.StartsWith ("0x"))
				res = res.Substring (2);

			try
				{
				v = ulong.Parse (res, RDGenerics.HexNumberStyle);
				}
			catch
				{
				return double.NaN;
				}

			return v;
			}

		/// <summary>
		/// Возвращает максимальное число, с которым может работать конвертор в текущей версии
		/// </summary>
		public const uint MaxValue = 0xFFFFFFFF;

		/// <summary>
		/// Метод формирует полное описание указанного числа
		/// </summary>
		/// <param name="Number">Строка, представляющая число (десятичное или шестнадцатеричное)</param>
		/// <returns>Возвращает описание числа или ошибку, если введено неподходящее число</returns>
		public static string GetNumberDescription (string Number)
			{
			// Попытка преобразования
			double v = GetNumber (Number);
			if (double.IsNaN (v))
				return "(введите положительное целое число)";

			// Форматирование
			ulong n = (ulong)v;
			if (n > MaxValue)
				return "(введённое число слишком велико)";

			string answer = "0d " + n.ToString ("#,0") + RDLocale.RN + "0x ";
			answer += ((n & 0xFFFF0000) >> 16).ToString ("X4") + " ";
			answer += (n & 0xFFFF).ToString ("X4") + RDLocale.RN + "0b";

			string b = "";
			for (int i = 1; i <= 32; i++)
				{
				b = (((1u << (i - 1)) & n) != 0 ? "1" : "0") + b;

#if ANDROID
				if (i == 16)
					b = RDLocale.RN + "   " + b;
				else if (i % 4 == 0)
#else
				if (i % 4 == 0)
#endif
					b = " " + b;
				}

			// Разложение на простые множители
			answer += (b + RDLocale.RN);
			return answer + SplitNumberToSimpleMultipliers (n);
			}

		private static string SplitNumberToSimpleMultipliers (ulong Value)
			{
			// Защита
			if (Value == 0)
				return "Число не является натуральным";

			if (Value == 1)
				return "Число не имеет других делителей, кроме самого себя";

			// Сборка списков
			ulong i = 2;
			ulong n = Value;
			ulong p = 0;
			string res = "0d ";

			while (n != 1)
				{
				if (n % i == 0)
					{
					n /= i;
					p++;
					}
				else
					{
					if (p != 0)
						res += (i.ToString () + MakePower (p) + " × ");

					p = 0;
					if (i == 2)
						i++;
					else
						i += 2;

					if (i > (1 << 16))
						return "Факторизация числа превышает возможности приложения";
					}
				}

			// Завершение
			res += (i.ToString () + MakePower (p));
			return res;
			}

		private static string MakePower (ulong Value)
			{
			string res = Value.ToString ();
			for (int i = 0; i < powers.Length; i++)
				res = res.Replace (powers[i][0], powers[i][1]);

			return res;
			}
		private static char[][] powers = new char[][] {
			new char[] { '0', '⁰' },
			new char[] { '1', '¹' },
			new char[] { '2', '²' },
			new char[] { '3', '³' },
			new char[] { '4', '⁴' },
			new char[] { '5', '⁵' },
			new char[] { '6', '⁶' },
			new char[] { '7', '⁷' },
			new char[] { '8', '⁸' },
			new char[] { '9', '⁹' },
			};

		/// <summary>
		/// Метод формирует полное описание указанного символа Unicode
		/// </summary>
		/// <param name="Symbol">Символ Unicode или его числовой код</param>
		/// <param name="Increment">Приращение к коду символа</param>
		/// <param name="UD">Инициализированный справочник описаний символов Unicode</param>
		/// <returns>Возвращает массив из трёх строк:
		/// - сам символ
		/// - его полное описание
		/// - шестнадцатеричное представление его кода</returns>
		public static string[] GetSymbolDescription (string Symbol, long Increment, UnicodeDescriptor UD)
			{
			// Защита
			string[] answer = new string[] { " ", "(введите символ или его код)", "0" };
			if (string.IsNullOrEmpty (Symbol))
				return answer;

			// Попытка преобразования
			double v = GetNumber (Symbol);

			// Получение кода от символа при необходимости
			ulong n = 0;
			if (double.IsNaN (v))
				{
				byte[] b = RDGenerics.GetEncoding (RDEncodings.Unicode32).GetBytes (Symbol);
				for (int i = 0; i < 4; i++)
					n |= (ulong)b[i] << (i * 8);
				}
			else
				{
				n = (ulong)v;
				}

			// Получение символа
			if ((long)n + Increment < 0)
				n = 0;
			else
				n = (ulong)((long)n + Increment);
			if (n > UD.MaxValue)
				return answer;

			byte[] ch = new byte[]{
				(byte)(n & 0xFF),
				(byte)((n >> 8) & 0xFF),
				(byte)((n >> 16) & 0xFF),
				(byte)((n >> 24) & 0xFF)
				};
			answer[0] = RDGenerics.GetEncoding (RDEncodings.Unicode32).GetString (ch);

			// Сборка описания
			answer[1] = "Символ: U+" + n.ToString ("X6") + RDLocale.RN + "HTML:   &#" + n.ToString () + ";";
			answer[1] += (RDLocale.RN + "Класс:  " + CharUnicodeInfo.GetUnicodeCategory (answer[0][0]).ToString ());
			answer[1] += (RDLocale.RN + "Блок:   " + UD.GetDescription (n));
			answer[2] = "0x" + n.ToString ("X6");
			return answer;
			}

		/// <summary>
		/// Возвращает доступные режимы преобразования, дублируя список режимами с поддержкой
		/// BASE64 (список кодировок повторяется дважды)
		/// </summary>
		public static string[] AvailableEncodings
			{
			get
				{
				if (availableEncodings.Count == RDGenerics.EncodingsCount)
					{
					for (int i = 0; i < RDGenerics.EncodingsCount; i++)
						availableEncodings.Add ("BASE64 + " + availableEncodings[i]);
					}

				return availableEncodings.ToArray ();
				}
			}
		private static List<string> availableEncodings = new List<string> {
			"UTF8",
			"Unicode (16 bit)",
			"Unicode (32 bit)",
#if !ANDROID
			"CP1251 (Windows)",
			"CP866 (MS DOS)",
			"KOI8-R",
#else
			"ASCII",
#endif
			};

		private const string hexLine = "0123456789ABCDEF";
		private const string encodingFailure = "(не удаётся преобразовать данные в выбранной кодировке)";

		/// <summary>
		/// Метод преобразует данные hex в текстовую строку в указанной кодировке
		/// </summary>
		/// <param name="HexData">Данные в двоичной форме или в кодировке BASE64</param>
		/// <param name="FromBASE64">Флаг, указывающий, что исходные данные представлены в BASE64</param>
		/// <param name="Mode">Кодировка исходных данных</param>
		/// <returns>Исходный текст</returns>
		public static string ConvertHexToText (string HexData, RDEncodings Mode, bool FromBASE64)
			{
			// Обычные данные
			List<byte> bytes = new List<byte> ();
			if (!FromBASE64)
				{
				List<string> numbers = new List<string> { "" };
				string source = HexData.ToUpper ();
				if (string.IsNullOrWhiteSpace (source))
					return "";

				// Сборка потока
				int p = 0;
				for (int i = 0; i < source.Length; i++)
					{
					if ((p % 2 == 0) && (numbers[numbers.Count - 1].Length > 0))
						numbers.Add ("");

					if (hexLine.Contains (source[i].ToString ()))
						{
						p++;
						numbers[numbers.Count - 1] += source[i].ToString ();
						}
					}

				// Контроль
				if (numbers[numbers.Count - 1].Length != 2)
					numbers[numbers.Count - 1] += "0";
				if (numbers.Count < 1)
					return "";

				// Преобразование в байт-поток
				for (int i = 0; i < numbers.Count; i++)
					bytes.Add (byte.Parse (numbers[i], RDGenerics.HexNumberStyle));
				numbers.Clear ();
				}

			// Предварительное извлечение из BASE64
			else
				{
				try
					{
					bytes.AddRange (Convert.FromBase64String (HexData));
					}
				catch
					{
					return encodingFailure;
					}
				}

			// Сборка результата
			try
				{
				return RDGenerics.GetEncoding (Mode).GetString (bytes.ToArray ());
				}
			catch
				{
				return encodingFailure;
				}
			}

		/// <summary>
		/// Метод преобразует текстовые данные в двоичные данные в указанной кодировке
		/// </summary>
		/// <param name="TextData">Данные в текстовой форме</param>
		/// <param name="ToBASE64">Флаг, указывающий, что конечные данные должны быть 
		/// представлены в BASE64</param>
		/// <param name="Mode">Кодировка конечных данных</param>
		/// <returns>Исходный текст</returns>
		public static string ConvertTextToHex (string TextData, RDEncodings Mode, bool ToBASE64)
			{
			// Сборка байт-массива
			byte[] bytes;
			try
				{
				bytes = RDGenerics.GetEncoding (Mode).GetBytes (TextData);
				}
			catch
				{
				return encodingFailure;
				}

			// Сборка результата
			if (ToBASE64)
				return Convert.ToBase64String (bytes);

			string res = "";
			for (int i = 0; i < bytes.Length; i++)
				res += (bytes[i].ToString ("X02") + " ");

			return res;
			}
		}
	}
