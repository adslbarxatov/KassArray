﻿using System;
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
		private char[] splitters = [ ';' ];

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
			List<string> res = [];

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
