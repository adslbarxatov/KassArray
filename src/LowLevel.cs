using System;
using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к командам нижнего уровня
	/// </summary>
	public class LowLevel
		{
		// Переменные
		private List<List<string>> names = [];
		private List<List<string>> commands = [];
		private List<List<string>> descriptions = [];
		private List<string> protocols = [];

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public LowLevel ()
			{
			// Получение файлов
#if !ANDROID
			byte[] data = KassArrayDBResources.LowLevel;
#else
			byte[] data = Properties.Resources.LowLevel;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (data);
			StringReader SR = new StringReader (buf);

			// Формирование массива
			string str;
			char[] splitters = [ ';' ];

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);

				// Имя протокола
				if (values.Length == 1)
					{
					names.Add ([]);
					commands.Add ([]);
					descriptions.Add ([]);

					protocols.Add (values[0]);
					}

				// Список команд
				else if (values.Length == 3)
					{
					names[names.Count - 1].Add (values[0]);
					commands[commands.Count - 1].Add (values[1]);
					descriptions[descriptions.Count - 1].Add (values[2].Replace ("|", RDLocale.RN));
					}
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список команд
		/// </summary>
		/// <param name="ArrayNumber">Номер списка команд</param>
		public List<string> GetCommandsList (uint ArrayNumber)
			{
			if (ArrayNumber < names.Count)
				return new List<string> (names[(int)ArrayNumber]);

			return null;
			}

		/// <summary>
		/// Метод возвращает содержимое команды
		/// </summary>
		/// <param name="CommandNumber">Номер команды из списка</param>
		/// <param name="ReturnDescription">Флаг указывает на возврат описания вместо команды</param>
		/// <param name="ArrayNumber">Номер списка команд</param>
		public string GetCommand (uint ArrayNumber, uint CommandNumber, bool ReturnDescription)
			{
			if ((ArrayNumber >= names.Count) || (CommandNumber >= names[(int)ArrayNumber].Count))
				return "";

			return (ReturnDescription ? descriptions[(int)ArrayNumber][(int)CommandNumber] :
				commands[(int)ArrayNumber][(int)CommandNumber]);
			}

		/// <summary>
		/// Метод возвращает список поддерживаемых протоколов
		/// </summary>
		public List<string> GetProtocolsNames ()
			{
			return protocols;
			}
		}
	}
