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
		private int newKKTCount;

		/// <summary>
		/// Код, возвращаемый при указании некорректных параметров
		/// </summary>
		public const string EmptyCode = "\x7";

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
				// Чтение количества новых ККТ
				newKKTCount = int.Parse (buf[line++]);

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

					// Отсечка по новым кодам
					if (names.Count >= newKKTCount)
						break;
					}

				if ((codes.Count != names.Count) || (names.Count != descriptions.Count) ||
					(descriptions.Count != presentations.Count))
					throw new Exception ("KKT codes reading failure, point 1");

				// Верификация количества
				if (newKKTCount < 1)
					newKKTCount = 1;
				if (newKKTCount > names.Count)
					newKKTCount = names.Count;
				}
			catch
				{
				throw new Exception ("KKT codes reading failure, point 2, line " + line.ToString ());
				}
			}

		/// <summary>
		/// Метод возвращает код указанного символа
		/// </summary>
		/// <param name="KKTType">Модель ККТ</param>
		/// <param name="CodeNumber">Порядковый номер символа в таблице</param>
		/// <returns>Возвращает код (сообщение) ошибки или \x7 в случае, если входные параметры некорректны</returns>
		public string GetCode (uint KKTType, byte CodeNumber)
			{
			if ((int)KKTType < names.Count)
				{
				if (codes[(int)KKTType][CodeNumber] < 0)
					return EmptyCode;

				return codes[(int)KKTType][CodeNumber].ToString (presentations[(int)KKTType]);
				}

			return EmptyCode;
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
		/// <returns>Возвращает пояснение или \x7 в случае, если входные параметры некорректны</returns>
		public string GetKKTTypeDescription (uint KKTType)
			{
			if ((int)KKTType < names.Count)
				return descriptions[(int)KKTType];

			return EmptyCode;
			}
		}
	}
