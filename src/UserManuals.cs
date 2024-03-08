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
		public static string[] OperationTypes
			{
			get
				{
				return operationTypes;
				}
			}

		/// <summary>
		/// Возвращает список операций, допустимых для кассира (неспециалиста)
		/// </summary>
		public static string[] OperationsForCashiers
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
					operationTypes[8],
					};
				}
			}

		private static string[] operationTypes = new string[] {
			"Открытие смены",
			"Продажа за наличные",
			"Продажа по карте",
			"Продажа по штрих-коду",		// 3, 0x????7
			"Продажа с количеством",
			"Продажа с электронным чеком",
			"Аннулирование",
			"Возврат",						// 7, 0x???D?
			"Закрытие смены",

			"Коррекция даты",
			"Коррекция времени",
			"Тест связи с сетью интернет",	// 11, 0x??F??
			"Автотест / информация о ККТ",
			"Запрос состояния ФН",
			"Запрос реквизитов регистраций",
			"Техобнуление",					// 15, 0x?7???
			"Закрытие архива ФН",			// 16, 0x0????
			};

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public UserManuals ()
			{
			// Получение файлов
#if !ANDROID
			byte[] s1 = Properties.KassArrayDB.UserManuals;
#else
			byte[] s1 = Properties.Resources.UserManuals;
#endif
			string buf = RDGenerics.GetEncoding (RDEncodings.UTF8).GetString (s1);
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
					operations[i].Add ("• " + SR.ReadLine ().Replace ("|", RDLocale.RN + "• "));

					switch (i)
						{
						case 1:
						case 2:
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("• &1", "#1• ");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&2",
								"Ввести код #6");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&02",
								"ввести код #6");
							break;

						case 3:
						case 4:
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("• &1",
								"#1• Закрыть чек согласно способу оплаты");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&2",
								"Ввести код #6");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&02",
								"ввести код #6");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&3",
								"Отсканировать штрих-код товара");

							if (i == 4)
								operations[i][operations[i].Count - 1] = "Действие выполняется для всех позиций, " +
									"в которых количество не равно единице:" + RDLocale.RN +
									operations[i][operations[i].Count - 1];
							break;

						case 6:
							operations[i][operations[i].Count - 1] = "Если требуется «отменить» чек, который ещё " +
								"не был закрыт, выполняется аннулирование:" + RDLocale.RN +
								operations[i][operations[i].Count - 1];
							break;

						case 7:
							operations[i][operations[i].Count - 1] = "Если требуется «отменить» чек, который уже " +
								"был закрыт, выполняется возврат:" + RDLocale.RN +
								operations[i][operations[i].Count - 1] +
								";" + RDLocale.RN + "• Дальнейшие действия совпадают с действиями при продаже";
							break;

						case 8:
							operations[i][operations[i].Count - 1] += ";" + RDLocale.RN + "• Дождаться снятия отчёта";
							break;

						case 9:
						case 10:
							if (!operations[i][operations[i].Count - 1].StartsWith ("• ("))
								operations[i][operations[i].Count - 1] =
									"! Необходимо предварительно закрыть смену;" + RDLocale.RN +
									operations[i][operations[i].Count - 1];
							break;

						case 15:
							operations[i][operations[i].Count - 1] =
								"! Необходимо убедиться, что сохранены все важные настройки;" + RDLocale.RN +
								operations[i][operations[i].Count - 1];
							break;

						case 16:
							if (!operations[i][operations[i].Count - 1].StartsWith ("• ("))
								operations[i][operations[i].Count - 1] =
									"! Необходимо убедиться, что смена закрыта, а дата в ККТ позволяет закрыть архив;" +
									RDLocale.RN + operations[i][operations[i].Count - 1] +
									";" + RDLocale.RN + "• Дождаться распечатки отчёта и отправки документов ОФД";
							break;
						}

					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&4",
						"несколько раз до отображения");
					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("• &5",
						"! (выполняется от имени");
					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&[",
						"Нажать [");
					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&0[",
						"нажать [");
					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&7",
						"#4");
					operations[i][operations[i].Count - 1] = operations[i][operations[i].Count - 1].Replace ("&8",
						"#5");

					if (operations[i][operations[i].Count - 1].StartsWith ("• -"))
						operations[i][operations[i].Count - 1] = "(не предусмотрено)";
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
		/// <param name="Flags">Флаги, определающие состав руководства</param>
		public string GetManual (uint KKTType, uint ManualType, UserManualsFlags Flags)
			{
			if (KKTType >= names.Count)
				return "";

			if (ManualType >= operationTypes.Length)
				return names[(int)KKTType];

			string text = operations[(int)ManualType][(int)KKTType];
			bool goods = !KKTSupport.IsSet (Flags, UserManualsFlags.ProductBaseContainsServices);
			if ((ManualType >= 1) && (ManualType <= 4))
				{
				if (KKTSupport.IsSet (Flags, UserManualsFlags.MoreThanOneItemPerDocument))
					{
					text = text.Replace ("#1", "↑ (повторить предыдущие действия для всех " +
						(goods ? "товаров" : "услуг") + " в чеке);" + RDLocale.RN);
					}
				else
					{
					text = text.Replace ("#1", "");
					}

				if (KKTSupport.IsSet (Flags, UserManualsFlags.ProductBaseContainsPrices))
					{
					int left = text.IndexOf ("#3");
					if (left >= 0)
						{
						int right = text.IndexOf (RDLocale.RN, left);
						if (right >= 0)
							text = text.Substring (0, left - 2) + text.Substring (right + RDLocale.RN.Length);
						}
					}
				else
					{
					text = text.Replace ("#3", "");
					}

				text = text.Replace ("#6", goods ? "товара" : "услуги");
				}

			if (KKTSupport.IsSet (Flags, UserManualsFlags.CashiersHavePasswords))
				{
				text = text.Replace ("#4", "ввести пароль кассира, если он задан, ");
				text = text.Replace ("#5", "Ввести пароль кассира в случае запроса;" + RDLocale.RN + "• ");
				}
			else
				{
				text = text.Replace ("#4", "").Replace ("#5", "");
				}

			return text;
			}
		}
	}
