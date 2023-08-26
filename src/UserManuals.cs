using System;
using System.Collections.Generic;
using System.IO;

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
		public static string[] OperationTypes2
			{
			get
				{
				return operationTypes;
				/*availableOperations.ToArray ();*/
				}
			}
		/*private List<string> availableOperations = new List<string> ();*/

		/// <summary>
		/// Возвращает список операций, допустимых для кассира (неспециалиста)
		/// </summary>
		public static string[] OperationsForCashiers2
			{
			get
				{
				return new string[] {
					operationTypes[0],
					operationTypes[1],
					operationTypes[2],
					operationTypes[3],
					operationTypes[4],
					operationTypes[5],
					operationTypes[6],
					operationTypes[7],
					};
				}
			}

		private static string[] operationTypes = new string[] {
			"Открытие смены",
			"Продажа за наличные",
			"Продажа по карте",
			"Продажа по штрих-коду",	// 0x???7
			"Продажа с количеством",
			"Продажа с электронным чеком",
			"Возврат",
			"Закрытие смены",	// 7, 0x??D?
			"Коррекция даты",
			"Коррекция времени",
			"Тест связи с сетью интернет",
			"Автотест / информация о ККТ",
			"Запрос состояния ФН",
			"Запрос реквизитов регистраций",
			"Техобнуление",
			"Закрытие архива ФН",	// 15
			};

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public UserManuals ()
			{
			/* Формирование списка
			availableOperations.AddRange (operationTypes1);
			if (Level1)
				availableOperations.AddRange (operationTypes2);
			if (Level2)
				availableOperations.AddRange (operationTypes3);*/

			// Получение файлов
#if !ANDROID
			byte[] s1 = Properties.TextToKKMResources.UserManuals;
#else
			byte[] s1 = Properties.Resources.UserManuals;
#endif
			string buf = RDGenerics.GetEncoding (SupportedEncodings.UTF8).GetString (s1);
			StringReader SR = new StringReader (buf);
			string str;

			// Формирование массива 
			while (operations.Count < operationTypes.Length)
				operations.Add (new List<string> ());

			// Чтение параметров
			while ((str = SR.ReadLine ()) != null)
				{
				if (str == "")
					continue;

				names.Add (str);

				// Загрузка файла целиком (требует структура)
				for (int i = 0; i < operations.Count; i++)
					{
					/* Оплата по свободной цене более не актуальна – пропуск
					if (i == 3)
						SR.ReadLine ();*/

					operations[i].Add ("• " + SR.ReadLine ().Replace ("|", Localization.RN + "• "));

					if ((i >= 1) && (i < 3))
						operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&",
							"Повторить предыдущие действия для всех позиций чека");
					if ((i >= 3) && (i <= 4))
						operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&",
							"Повторить предыдущие действия для всех позиций чека;" + Localization.RN +
							"• Закрыть чек согласно способу оплаты");
					if (i == 6)
						operations[i][operations[i].Count - 1] +=
							";" + Localization.RN + "• Дальнейшие действия совпадают с действиями при продаже";
					if (i == 7)
						operations[i][operations[i].Count - 1] += ";" + Localization.RN +
							"• Дождаться снятия отчёта";
					if ((i >= 8) && (i <= 9))
						operations[i][operations[i].Count - 1] =
							"• Настоятельно рекомендуется предварительно закрыть смену;" + Localization.RN +
							operations[i][operations[i].Count - 1];
					if (i == 14)
						{
						operations[i][operations[i].Count - 1] =
							"• Убедиться, что сохранены все необходимые настройки;" + Localization.RN +
							operations[i][operations[i].Count - 1];
						}
					if (i == 15)
						{
						operations[i][operations[i].Count - 1] =
							"• Убедиться, что смена закрыта, а дата в ККТ позволяет выполнить закрытие архива;" +
							Localization.RN + operations[i][operations[i].Count - 1] +
							";" + Localization.RN + "• Дождаться распечатки отчёта и отправки документов ОФД";
						}

					if (operations[i][operations[i].Count - 1].StartsWith ("• -"))
						operations[i][operations[i].Count - 1] = "(не предусмотрено)";
					if (operations[i][operations[i].Count - 1].Contains ("#"))
						operations[i][operations[i].Count - 1] =
							operations[i][operations[i].Count - 1].Replace ("#", "") + Localization.RNRN +
							"* Порядок действий может отличаться в разных версиях прошивок";
					}
				}

			// Первая часть завершена
			SR.Close ();
			}

		/// <summary>
		/// Метод возвращает список ККТ, для которых доступны инструкции
		/// </summary>
		public string[] GetKKTList ()
			{
			return names.ToArray ();
			}

		/// <summary>
		/// Возвращает инструкцию по указанному типу операции
		/// </summary>
		/// <param name="KKTType">Тип ККТ</param>
		/// <param name="ManualType">Операция</param>
		public string GetManual2 (uint KKTType, uint ManualType)
			{
			if (KKTType >= names.Count)
				return "";

			if (ManualType < operationTypes.Length)
				return operations[(int)ManualType][(int)KKTType];

			return names[(int)KKTType];
			}
		}
	}
