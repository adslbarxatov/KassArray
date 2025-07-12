using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к словарю терминов
	/// </summary>
	public class TermsDictionary
		{
		// Переменные
		private List<string> shortNames = [];
		private List<string> longNames = [];
		private List<string> descriptions = [];
		private int lastSearchOffset = 0;

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public TermsDictionary ()
			{
			// Получение файлов
#if !ANDROID
			byte[] data = KassArrayDBResources.TermsDictionary;
#else
			byte[] data = Properties.Resources.TermsDictionary;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data);
			StringReader SR = new StringReader (buf);

			// Формирование массива
			string str;

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				shortNames.Add (str);
				longNames.Add (SR.ReadLine ());
				descriptions.Add ("– " + SR.ReadLine ().Replace ("|", RDLocale.RNRN));
				SR.ReadLine ();
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод выполняет поиск указанного ключевого слова в словаре терминов
		/// </summary>
		/// <param name="Criteria">Ключевое слово для поиска</param>
		/// <param name="Continue">Флаг продолжения поиска в порядке следования терминов</param>
		/// <returns>Возвращает термин и его определение либо пустую строку, если термин
		/// не был найден</returns>
		public string FindNext (string Criteria, bool Continue)
			{
			if (!Continue)
				lastSearchOffset = 0;
			else
				lastSearchOffset++;

			string criteria = Criteria.ToLower ();

			for (int i = 0; i < shortNames.Count; i++)
				{
				int idx = (i + lastSearchOffset) % shortNames.Count;
				if (shortNames[idx].ToLower ().Contains (criteria) ||
					longNames[idx].ToLower ().Contains (criteria))
					{
					lastSearchOffset = idx;
					return shortNames[idx] + RDLocale.RN + longNames[idx] + "\x1" + descriptions[idx];
					}
				}

			return "(описание термина не найдено)";
			}
		}
	}
