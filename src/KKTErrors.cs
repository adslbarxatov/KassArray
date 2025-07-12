using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к сообщениям об ошибках ККТ
	/// </summary>
	public class KKTErrorsList
		{
		// Переменные
		private List<List<KKTError>> errors = [];
		private List<string> names = [];
		private char[] splitters = [';'];
		private int lastSearchOffset = 0;
		
		/// <summary>
		/// Конструктор. Инициализирует таблицу ошибок
		/// </summary>
		public KKTErrorsList ()
			{
			// Получение файла ошибок
#if !ANDROID
			byte[] data = KassArrayDBResources.Errors;
#else
			byte[] data = Properties.Resources.Errors;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			uint line = 0;
			string str;

			// Чтение кодов
			while ((str = SR.ReadLine ()) != null)
				{
				// Обработка строки
				line++;
				string[] values = str.Split (splitters, StringSplitOptions.None);

				switch (values.Length)
					{
					case 1:
						errors.Add ([]);
						names.Add (values[0]);
						break;

					case 2:
						errors[errors.Count - 1].Add (new KKTError (values[0], values[1]));
						break;

					case 3:
						continue;

					default:
						break;
					}
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список названий ККТ
		/// </summary>
		public List<string> GetKKTTypeNames ()
			{
			return names;
			}

		/// <summary>
		/// Метод выполняет поиск указанного ключевого слова в списке ошибок ККТ
		/// </summary>
		/// <param name="Criteria">Ключевое слово для поиска</param>
		/// <param name="Continue">Флаг продолжения поиска в порядке следования терминов</param>
		/// <returns>Возвращает описание ошибки либо пустую строку, если ошибка не была найдена</returns>
		public string FindNext (uint KKTType, string Criteria, bool Continue)
			{
			int idx = (int)KKTType;
			string criteria = Criteria.ToLower ();
			if (!Continue)
				lastSearchOffset = 0;
			else
				lastSearchOffset++;

			for (int i = 0; i < errors[idx].Count; i++)
				{
				int j = (i + lastSearchOffset) % errors[idx].Count;
				string code = errors[idx][j].ErrorCode.ToLower ();
				string res = errors[idx][j].ErrorText;

				if (code.Contains (criteria) || res.ToLower ().Contains (criteria) ||
					code.Contains ('?') && criteria.Contains (code.Replace ("?", "")))
					{
					lastSearchOffset = j;
					return errors[idx][j].ErrorCode + ": " + res;
					}
				}

			// Код не найден
			return "(описание ошибки не найдено)";
			}
		}

	/// <summary>
	/// Класс описывает отдельную ошибку ККТ
	/// </summary>
	public class KKTError
		{
		/// <summary>
		/// Код или сообщение ошибки
		/// </summary>
		public string ErrorCode
			{
			get
				{
				return errorCode;
				}
			}
		private string errorCode;

		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorText
			{
			get
				{
				return errorText;
				}
			}
		private string errorText;

		/// <summary>
		/// Конструктор. Создаёт объект-ошибку
		/// </summary>
		/// <param name="Code">Код или сообщение об ошибке</param>
		/// <param name="Text">Текст ошибки</param>
		public KKTError (string Code, string Text)
			{
			errorCode = string.IsNullOrWhiteSpace (Code) ? "—" : Code;
			errorText = string.IsNullOrWhiteSpace (Text) ? "—" : Text;
			}
		}
	}
