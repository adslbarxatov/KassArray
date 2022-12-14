using System;
using System.Collections.Generic;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к кодам символов ККТ
	/// </summary>
	public class KKTCodes
		{
		// Переменные
		private List<string> names = new List<string> ();
		private List<List<int>> codes = new List<List<int>> ();
		private List<string> presentations = new List<string> ();
		private List<string> descriptions = new List<string> ();

		/// <summary>
		/// Код, возвращаемый при указании некорректных параметров
		/// </summary>
		public const string EmptyCode = "xxx";

		// Разделители
		private char[] splitters = new char[] { '\n', '\t' };

		/// <summary>
		/// Конструктор. Инициализирует таблицы кодов
		/// </summary>
		public KKTCodes ()
			{
			// Получение файла символов
#if !ANDROID
			byte[] s = Properties.TextToKKMResources.KKTCodes;
#else
			byte[] s = Properties.Resources.KKTCodes;
#endif
			string[] buf = Encoding.UTF8.GetString (s).Split (splitters, StringSplitOptions.RemoveEmptyEntries);

			// Формирование массива 
			int line = 0;
			try
				{
				// Чтение кодов
				while (line < buf.Length)
					{
					// Чтение названия
					names.Add (buf[line++]);

					// Чтение кодов
					codes.Add (new List<int> ());

					for (int i = 0; i < 0x100; i++)
						{
						if (i < 32)
							codes[codes.Count - 1].Add (-1);
						else
							codes[codes.Count - 1].Add (int.Parse (buf[line++]));
						}

					// Чтение представления и примечания
					presentations.Add (buf[line++]);
					descriptions.Add (buf[line++]);
					}

				if ((codes.Count != names.Count) || (names.Count != descriptions.Count) ||
					(descriptions.Count != presentations.Count))
					throw new Exception ("KKT codes reading failure, point 1");
				}
			catch
				{
				throw new Exception ("KKT codes reading failure, point 2, line " + line.ToString ());
				}
			}

		// Метод возвращает код указанного символа
		private int GetCode (uint KKTType, byte CodeNumber)
			{
			// Защита
			if (codes[(int)KKTType][CodeNumber] < 0)
				return -1;

			// Возврат
			return codes[(int)KKTType][CodeNumber];
			}

		/// <summary>
		/// Метод декодирует строку текста в коды символов ККТ
		/// </summary>
		/// <param name="Text">Исходная строка текста</param>
		/// <param name="KKTType">Индекс выбранной модели ККТ</param>
		/// <returns>Сформированный порядок кодов ККТ; содержит код «xxx», если в строке есть
		/// неподдерживаемые символы</returns>
		public string DecodeText (uint KKTType, string Text)
			{
			// Защита
			if (KKTType >= names.Count)
				return "";

			// Разбор строки
			char[] text = Text.ToCharArray ();
			string resultText = "";

			for (int i = 0; i < text.Length; i++)
				{
				byte b = KKTSupport.CharToCP1251 (text[i]);
				int c = GetCode (KKTType, b);

				// Обработка
				if (c < 0)
					{
					resultText += EmptyCode;
					}
				else if (presentations[(int)KKTType] == "?2")
					{
					if (c >= 200)
						resultText += ("1СК~1СК~" + (c - 200).ToString ().PadLeft (3) + "~1СК");
					else if (c >= 100)
						resultText += ("1СК~" + (c - 100).ToString ("D2").PadLeft (3) + "~1СК~1СК");
					else
						resultText += c.ToString ("D2").PadLeft (3);
					}
				else if (presentations[(int)KKTType] == "?1")
					{
					if (c >= 100)
						resultText += ("1СК~" + (c - 100).ToString ().PadLeft (3) + "~1СК");
					else
						resultText += c.ToString ("D2").PadLeft (3);
					}
				else
					{
					resultText += c.ToString (presentations[(int)KKTType]).PadLeft (3);
					}
				resultText += "~";
				}

			// Пост-обработка
			if (presentations[(int)KKTType] == "?2")
				resultText = resultText.Replace ("1СК~1СК~1СК~", "");
			else if (presentations[(int)KKTType] == "?1")
				resultText = resultText.Replace ("1СК~1СК~", "");

			// Сборка
			char[] resText = resultText.ToCharArray ();
			int idx = 1;
			resultText = "";

			for (int i = 0; i < resText.Length; i++)
				{
				if (resText[i] == '~')
#if ANDROID
					resultText += ((idx++ % 5 != 0) ? "    " : "\r\n");
#else
					resultText += ((idx++ % 5 != 0) ? "\t" : "\r\n");
#endif
				else
					resultText += resText[i];
				}

			// Завершено
			return resultText;
			}

		/// <summary>
		/// Метод возвращает список названий ККТ
		/// </summary>
		public List<string> GetKKTTypeNames ()
			{
			return names;
			}

		/// <summary>
		/// Метод возвращает пояснение к способу ввода текста в ККТ по её типу
		/// </summary>
		/// <param name="KKTType">Модель ККТ</param>
		/// <returns>Возвращает пояснение или пустую строку, если входные параметры некорректны</returns>
		public string GetKKTTypeDescription (uint KKTType)
			{
			if ((int)KKTType < names.Count)
				return descriptions[(int)KKTType];

			return "";
			}
		}
	}
