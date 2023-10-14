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
					operations[i].Add ("• " + SR.ReadLine ().Replace ("|", Localization.RN + "• "));

					switch (i)
						{
						case 1:
						case 2:
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("• &1", "#1• ");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&2",
								"Ввести код товара / услуги");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&02",
								"ввести код товара / услуги");
							break;

						case 3:
						case 4:
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("• &1",
								"#1• Закрыть чек согласно способу оплаты");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&2",
								"Ввести код товара / услуги");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&02",
								"ввести код товара / услуги");
							operations[i][operations[i].Count - 1] =
								operations[i][operations[i].Count - 1].Replace ("&3",
								"Отсканировать штрих-код товара");
							break;

						case 6:
							operations[i][operations[i].Count - 1] +=
								";" + Localization.RN + "• Дальнейшие действия совпадают с действиями при продаже";
							break;

						case 7:
							operations[i][operations[i].Count - 1] += ";" + Localization.RN + "• Дождаться снятия отчёта";
							break;

						case 8:
						case 9:
							if (!operations[i][operations[i].Count - 1].StartsWith ("• ("))
								operations[i][operations[i].Count - 1] =
									"! Необходимо предварительно закрыть смену;" + Localization.RN +
									operations[i][operations[i].Count - 1];
							break;

						case 14:
							operations[i][operations[i].Count - 1] =
								"! Необходимо убедиться, что сохранены все важные настройки;" + Localization.RN +
								operations[i][operations[i].Count - 1];
							break;

						case 15:
							if (!operations[i][operations[i].Count - 1].StartsWith ("• ("))
								operations[i][operations[i].Count - 1] =
									"! Необходимо убедиться, что смена закрыта, а дата в ККТ позволяет закрыть архив;" +
									Localization.RN + operations[i][operations[i].Count - 1] +
									";" + Localization.RN + "• Дождаться распечатки отчёта и отправки документов ОФД";
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

					if (operations[i][operations[i].Count - 1].Contains ("&6"))
						operations[i][operations[i].Count - 1] =
						operations[i][operations[i].Count - 1].Replace ("&6", "") +
							Localization.RNRN + "* Порядок действий может отличаться в разных версиях прошивок";
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
			if ((ManualType >= 1) && (ManualType <= 4))
				{
				if (KKTSupport.IsSet (Flags, UserManualsFlags.MoreThanOneItemPerDocument))
					{
					text = text.Replace ("#1", "↑ (повторить предыдущие действия для всех товаров / услуг в чеке);"
						+ Localization.RN);
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
						int right = text.IndexOf (Localization.RN, left);
						if (right >= 0)
							text = text.Substring (0, left - 2) + text.Substring (right + Localization.RN.Length);
						}
					}
				else
					{
					text = text.Replace ("#3", "");
					}
				}

			if (KKTSupport.IsSet (Flags, UserManualsFlags.CashiersHavePasswords))
				{
				text = text.Replace ("#4", "ввести пароль кассира, если он задан, ");
				text = text.Replace ("#5", "Ввести пароль кассира в случае запроса;" + Localization.RN + "• ");
				}
			else
				{
				text = text.Replace ("#4", "").Replace ("#5", "");
				}

			return text;
			}
		}
	}
