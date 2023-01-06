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
			if (n > 0xFFFFFFFF)
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

			// Завершено
			return answer + b;
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
		}
	}
