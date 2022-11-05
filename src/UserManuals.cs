using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс обеспечивает доступ к инструкциям по работе с ККТ
	/// </summary>
	public class UserManuals
		{
		// Переменные
		private List<string> names = new List<string> ();
		private List<List<string>> operations = new List<List<string>> ();

		/// <summary>
		/// Возвращает список операций, для которых доступны инструкции
		/// </summary>
		public string[] OperationTypes
			{
			get
				{
				return availableOperations.ToArray ();
				}
			}
		private List<string> availableOperations = new List<string> ();

		/// <summary>
		/// Возвращает количество операций, допустимых для кассира (неспециалиста)
		/// </summary>
		public static uint OperationsForCashiers
			{
			get
				{
				return (uint)operationTypes1.Length;
				}
			}

		private static string[] operationTypes1 = new string[] {
			"Открытие смены",
			"Продажа по коду товара",
			"Продажа по карте",
			//"Продажа по свободной цене",
			"Продажа с количеством",
			"Продажа с электронным чеком",
			"Возврат",
			"Закрытие смены" };
		private static string[] operationTypes2 = new string[] {
			"Коррекция даты",
			"Коррекция времени" };
		private static string[] operationTypes3 = new string[] {
			"Автотестирование / информация о ККТ",
			"Запрос состояния ФН",
			"Запрос реквизитов регистраций",
			"Техобнуление" };

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		/// <param name="AllowExtended">Битовое поле разрешает отображение расширенных инструкций</param>
		public UserManuals (byte AllowExtended)
			{
			// Формирование списка
			availableOperations.AddRange (operationTypes1);
			if ((AllowExtended & 0x1) != 0)
				availableOperations.AddRange (operationTypes2);
			if ((AllowExtended & 0x2) != 0)
				availableOperations.AddRange (operationTypes3);

			// Получение файлов
#if !ANDROID
			byte[] s1 = Properties.TextToKKMResources.UserManuals;
#else
			byte[] s1 = Properties.Resources.UserManuals;
#endif
			string buf = Encoding.UTF8.GetString (s1);
			StringReader SR = new StringReader (buf);
			string str;

			// Формирование массива 
			while (operations.Count < (operationTypes1.Length + operationTypes2.Length + operationTypes3.Length))
				operations.Add (new List<string> ());

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					{
					if (str == "")
						continue;

					names.Add (str);

					// Загрузка файла целиком (требует структура)
					for (int i = 0; i < operations.Count; i++)
						{
						// Оплата по свободной цене более не актуальна – пропуск
						if (i == 3)
							SR.ReadLine ();

						operations[i].Add ("• " + SR.ReadLine ().Replace ("|", "\r\n• "));

						if ((i >= 1) || (i <= 3))
							operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&",
								"Повторить предыдущие действия для всех позиций чека");

						//if (i == 3)
						//	{
						//	operations[i][operations[i].Count - 1] +=
						//	" (отдельно для каждой позиции в чеке);\r\n• Закрыть чек в зависимости от способа оплаты";
						//	}

						if (i == 5)
							operations[i][operations[i].Count - 1] +=
								";\r\n• Дальнейшие действия совпадают с действиями при продаже";
						if (i == 6)
							operations[i][operations[i].Count - 1] += ";\r\n• Дождаться снятия отчёта";
						if ((i == 7) || (i == 8))
							operations[i][operations[i].Count - 1] =
								"• Настоятельно рекомендуется предварительно закрыть смену;\r\n" +
								operations[i][operations[i].Count - 1];

						if (operations[i][operations[i].Count - 1].StartsWith ("• -"))
							operations[i][operations[i].Count - 1] = "(не предусмотрено)";
						if (operations[i][operations[i].Count - 1].Contains ("#"))
							operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("#", "") +
								"\r\n\r\n* Порядок действий может отличаться в разных версиях прошивок";
						}
					}
				}
			catch
				{
				throw new Exception ("User manuals data reading failure, point 1");
				}

			// Первая часть завершена
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список ККТ, для которых доступны инструкции
		/// </summary>
		public List<string> GetKKTList ()
			{
			return new List<string> (names);
			}

		/// <summary>
		/// Возвращает инструкцию по указанному типу операции
		/// </summary>
		/// <param name="KKTType">Тип ККТ</param>
		/// <param name="ManualType">Операция</param>
		public string GetManual (uint KKTType, uint ManualType)
			{
			if (KKTType >= names.Count)
				return "";

			if (ManualType < availableOperations.Count)
				return operations[(int)ManualType][(int)KKTType];

			return names[(int)KKTType];
			}
		}
	}
