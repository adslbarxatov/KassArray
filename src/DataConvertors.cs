using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к функционалу преобразования числовых и символьных данных
	/// </summary>
	public static class DataConvertors
		{
		private static double GetNumber (string Value)
			{
			// Прямое преобразование
			double v = double.NaN;
			try
				{
				v = ulong.Parse (Value);
				}
			catch { }

			string res = Value.ToLower ();
			if (res.StartsWith ("0x"))
				res = res.Substring (2);

			// Преобразование из hex
			if (double.IsNaN (v))
				{
				try
					{
					v = ulong.Parse (res, NumberStyles.HexNumber);
					}
				catch { }
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

			string answer = "0d " + n.ToString ("#,0") + "\r\n0x ";
			answer += ((n & 0xFFFF0000) >> 16).ToString ("X4") + " ";
			answer += (n & 0xFFFF).ToString ("X4") + "\r\n0b";

			string b = "";
			for (int i = 1; i <= 32; i++)
				{
				b = (((1u << (i - 1)) & n) != 0 ? "1" : "0") + b;

#if ANDROID
				if (i == 16)
					b = "\r\n   " + b;
				else if (i % 4 == 0)
#else
				if (i % 4 == 0)
#endif
					b = " " + b;
				}

			// Разложение на простые множители
			answer += (b + "\r\n");
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

					if (i > 2048)
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
			res = res.Replace ('0', '⁰');
			res = res.Replace ('1', '¹');
			res = res.Replace ('2', '²');
			res = res.Replace ('3', '³');
			res = res.Replace ('4', '⁴');
			res = res.Replace ('5', '⁵');
			res = res.Replace ('6', '⁶');
			res = res.Replace ('7', '⁷');
			res = res.Replace ('8', '⁸');
			return res.Replace ('9', '⁹');
			}

		/// <summary>
		/// Метод формирует полное описание указанного символа Unicode
		/// </summary>
		/// <param name="Symbol">Символ Unicode или его числовой код</param>
		/// <param name="Increment">Приращение к коду символа</param>
		/// <returns>Возвращает символ и его описание в виде двухэлементного вектора</returns>
		public static string[] GetSymbolDescription (string Symbol, short Increment)
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
				byte[] b = Encoding.UTF32.GetBytes (Symbol);
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
			if (n > 0x10FFFF)
				return answer;

			byte[] ch = new byte[]{
				(byte)(n & 0xFF),
				(byte)((n >> 8) & 0xFF),
				(byte)((n >> 16) & 0xFF),
				(byte)((n >> 24) & 0xFF)
				};
			answer[0] = Encoding.UTF32.GetString (ch);

			// Сборка описания
			answer[1] = "Символ: U+" + n.ToString ("X4") + "\r\nHTML:   &#" + n.ToString () + ";";
			answer[1] += ("\r\nКласс:  " + CharUnicodeInfo.GetUnicodeCategory (answer[0][0]).ToString ());
			answer[2] = "0x" + n.ToString ("X4");
			return answer;
			}

		/// <summary>
		/// Доступные режимы для методов преобразования данных в hex и обратно
		/// </summary>
		public enum ConvertHTModes
			{
			/// <summary>
			/// Кодировка UTF8
			/// </summary>
			UTF8 = 0,

			/// <summary>
			/// Кодировка Unicode
			/// </summary>
			Unicode = 1,

#if !ANDROID

			/// <summary>
			/// Кодировка CP1251 (Windows)
			/// </summary>
			CP1251 = 2,

			/// <summary>
			/// Кодировка 866 (MS DOS)
			/// </summary>
			CP866 = 3,

#else

			/// <summary>
			/// Кодировка ASCII
			/// </summary>
			ASCII = 2,

#endif
			}

		/// <summary>
		/// Возвращает доступные режимы преобразования, дублируя список режимами с поддержкой
		/// BASE64 (список кодировок повторяется дважды)
		/// </summary>
		public static string[] AvailableEncodings
			{
			get
				{
				if (availableEncodings.Count == encodings.Length)
					{
					for (int i = 0; i < encodings.Length; i++)
						availableEncodings.Add ("BASE64 + " + availableEncodings[i]);
					}

				return availableEncodings.ToArray ();
				}
			}
		private static List<string> availableEncodings = new List<string> {
			"UTF8",
			"Unicode",
#if !ANDROID
			"CP1251 (Windows)",
			"CP866 (MS DOS)",
			"KOI8-R",
#else
			"ASCII",
#endif
			};

		/// <summary>
		/// Возвращает количество уникальных доступных кодировок (без BASE64)
		/// </summary>
		public static uint UniqueEncodingsCount
			{
			get
				{
				return (uint)encodings.Length;
				}
			}
		private static Encoding[] encodings = new Encoding[]
			{
			Encoding.UTF8,
			Encoding.Unicode,
#if !ANDROID
			Encoding.GetEncoding (1251),
			Encoding.GetEncoding (866),
			Encoding.GetEncoding (20866),
#else
			Encoding.ASCII,
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
		public static string ConvertHexToText (string HexData, ConvertHTModes Mode, bool FromBASE64)
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
					bytes.Add (byte.Parse (numbers[i], NumberStyles.HexNumber));
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
				return encodings[(int)Mode].GetString (bytes.ToArray ());
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
		public static string ConvertTextToHex (string TextData, ConvertHTModes Mode, bool ToBASE64)
			{
			// Сборка байт-массива
			byte[] bytes;
			try
				{
				bytes = encodings[(int)Mode].GetBytes (TextData);
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
