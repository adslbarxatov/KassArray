using System;
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
				descriptions.Add (SR.ReadLine ());
				}

			// Завершено
			SR.Close ();
			}
		}
	}
