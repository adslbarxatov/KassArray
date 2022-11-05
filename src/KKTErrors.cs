using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к сообщениям об ошибках ККТ
	/// </summary>
	public class KKTErrorsList
		{
		// Переменные
		private List<List<KKTError>> errors = new List<List<KKTError>> ();
		private List<string> names = new List<string> ();
		private char[] splitters = new char[] { ';' };
		private int newKKTCount;

		// Константы
		/// <summary>
		/// Код, возвращаемый при указании некорректных параметров
		/// </summary>
		public const string EmptyCode = "\x7";

		/// <summary>
		/// Конструктор. Инициализирует таблицу ошибок
		/// </summary>
		public KKTErrorsList ()
			{
			// Получение файла ошибок
#if !ANDROID
			byte[] s = Properties.TextToKKMResources.Errors;
#else
			byte[] s = Properties.Resources.Errors;
#endif
			string buf = Encoding.UTF8.GetString (s);
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			uint line = 0;
			string str;

			try
				{
				// Чтение количества новых ККТ
				newKKTCount = int.Parse (SR.ReadLine ());

				// Чтение кодов
				while ((str = SR.ReadLine ()) != null)
					{
					// Обработка строки
					line++;
					string[] values = str.Split (splitters);

					if ((values.Length != 1) && (values.Length != 2) && (names.Count >= newKKTCount))
						break;

					switch (values.Length)
						{
						case 1:
							errors.Add (new List<KKTError> ());
							names.Add (values[0]);
							break;

						case 2:
							errors[errors.Count - 1].Add (new KKTError (values[0], values[1]));
							break;

						default:
							continue;
						}
					}

				// Верификация количества
				if (newKKTCount < 1)
					newKKTCount = 1;
				if (newKKTCount > names.Count)
					newKKTCount = names.Count;
				}
			catch
				{
				throw new Exception ("Error codes reading failure, point 1, line " + line.ToString ());
				}

			// Завершено
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список ошибок ККТ
		/// </summary>
		/// <param name="KKTType">Модель ККТ</param>
		public List<KKTError> GetErrors (uint KKTType)
			{
			return errors[(int)KKTType];
			}

		/// <summary>
		/// Метод возвращает список кодов ошибок ККТ
		/// </summary>
		/// <param name="KKTType">Модель ККТ</param>
		public List<string> GetErrorCodesList (uint KKTType)
			{
			List<string> res = new List<string> ();

			for (int i = 0; i < errors[(int)KKTType].Count; i++)
				res.Add (errors[(int)KKTType][i].ErrorCode);

			return res;
			}

		/// <summary>
		/// Метод возвращает описание указанной ошибки
		/// </summary>
		/// <param name="KKTType">Модель ККТ</param>
		/// <param name="ErrorNumber">Порядковый номер сообщения</param>
		/// <returns>Возвращает описание ошибки или \x7 в случае, если входные параметры некорректны</returns>
		public string GetErrorText (uint KKTType, uint ErrorNumber)
			{
			if (((int)KKTType < names.Count) && ((int)ErrorNumber < errors[(int)KKTType].Count))
				return errors[(int)KKTType][(int)ErrorNumber].ErrorText;

			return EmptyCode;
			}

		/// <summary>
		/// Метод возвращает список названий ККТ
		/// </summary>
		public List<string> GetKKTTypeNames ()
			{
			//if (NewOnly)
			//	return names.GetRange (0, newKKTCount);

			return names;
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
		string errorCode;

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
		string errorText;

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
