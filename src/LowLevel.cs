using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к командам нижнего уровня
	/// </summary>
	public class LowLevel
		{
		// Переменные
		private List<List<string>> names = new List<List<string>> (),
			commands = new List<List<string>> (),
			descriptions = new List<List<string>> ();
		private List<string> protocols = new List<string> ();

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public LowLevel ()
			{
			// Получение файлов
#if !ANDROID
			byte[] s1 = Properties.TextToKKMResources.LowLevel;
#else
			byte[] s1 = Properties.Resources.LowLevel;
#endif
			string buf = Encoding.UTF8.GetString (s1);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;
			char[] splitters = new char[] { ';' };

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					{
					string[] values = str.Split (splitters, StringSplitOptions.RemoveEmptyEntries);

					// Имя протокола
					if (values.Length == 1)
						{
						names.Add (new List<string> ());
						commands.Add (new List<string> ());
						descriptions.Add (new List<string> ());

						protocols.Add (values[0]);
						}

					// Список команд
					else if (values.Length == 3)
						{
						names[names.Count - 1].Add (values[0]);
						commands[commands.Count - 1].Add (values[1]);
						descriptions[descriptions.Count - 1].Add (values[2].Replace ("|", "\r\n"));
						}
					}
				}
			catch
				{
				throw new Exception ("Low level commands data reading failure, point 1");
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
