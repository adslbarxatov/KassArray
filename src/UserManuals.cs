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
					string op = operations[i][operations[i].Count - 1];

					// Выборочные подстановки
					switch (i)
						{
						case 0:
							op = op.Replace ("&B", "для открытия смены");
							op = op.Replace ("• !", "!");
							op = op.Replace ("• " + RDLocale.RN, RDLocale.RN);
							break;

						case 1:
						case 2:
							op = op.Replace ("• &1", "#1• ");
							op = op.Replace ("&2", "#6");
							op = op.Replace ("&02", "#06");
							op = op.Replace ("&A", "для закрытия чека");
							break;

						case 3:
						case 4:
							op = op.Replace ("• &1", "#1• Закрыть чек согласно способу оплаты");
							op = op.Replace ("&2", "#6");
							op = op.Replace ("&02", "#06");
							op = op.Replace ("&3", "Отсканировать штрих-код товара");

							if (i == 4)
								op = "Действие выполняется для всех позиций, " +
									"в которых количество не равно единице:" + RDLocale.RN + op;
							break;

						case 6:
							op = "Если требуется «отменить» чек, который ещё " +
								"не был закрыт, выполняется аннулирование:" + RDLocale.RN + op;
							break;

						case 7:
							op = "Если требуется «отменить» чек, который уже " +
								"был закрыт, выполняется возврат:" + RDLocale.RN + op +
								";" + RDLocale.RN + "• Выполнить действия, описанные " +
								"в разделе «" + operationTypes[1] + "» или «" + operationTypes[2] + "» (в зависимости " +
								"от способа расчёта)";
							break;

						case 8:
							op += ";" + RDLocale.RN + "• Дождаться снятия отчёта";
							break;

						case 9:
						case 10:
							if (!op.StartsWith ("• ("))
								op = "! Необходимо предварительно закрыть смену;" + RDLocale.RN + op;
							break;

						case 15:
							op = "! Необходимо убедиться, что сохранены все важные настройки;" +
								RDLocale.RN + op;
							break;

						case 16:
							if (!op.StartsWith ("• ("))
								op =
									"! Необходимо убедиться, что смена закрыта, а дата в ККТ позволяет закрыть архив;" +
									RDLocale.RN + op +
									";" + RDLocale.RN + "• Дождаться распечатки отчёта и отправки документов ОФД";
							break;
						}

					// Общие подстановки
					op = op.Replace ("&4", "несколько раз до отображения");
					op = op.Replace ("• &5", "! (выполняется от имени");
					op = op.Replace ("&[", "Нажать [");
					op = op.Replace ("&0[", "нажать [");
					op = op.Replace ("&7", "#4");
					op = op.Replace ("&8", "#5");
					op = op.Replace ("???", "(требует уточнения)");
					op = op.Replace ("&9", "На дисплее – ");

					if (op.StartsWith ("• -"))
						op = "(не предусмотрено)";

					// Завершено
					operations[i][operations[i].Count - 1] = op;
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
			bool goods = !Flags.HasFlag (UserManualsFlags.ProductBaseContainsServices);
			if ((ManualType >= 1) && (ManualType <= 4))
				{
				if (Flags.HasFlag (UserManualsFlags.MoreThanOneItemPerDocument))
					{
					text = text.Replace ("#1", "↑ (повторить описанные действия для всех " +
						(goods ? "товаров" : "услуг") + " в чеке);" + RDLocale.RN);
					}
				else
					{
					text = text.Replace ("#1", "");
					}

				if (Flags.HasFlag (UserManualsFlags.ProductBaseContainsPrices))
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
					text = text.Replace ("#3", "Ввести цену,");
					}
				}

			if (ManualType <= 4)
				{
				if (Flags.HasFlag (UserManualsFlags.BaseContainsSingleItem))
					{
					text = text.Replace ("#6", "Нажать [1]");
					text = text.Replace ("#06", "нажать [1]");
					}
				else
					{
					text = text.Replace ("#6", "Ввести код " + (goods ? "товара" : "услуги"));
					text = text.Replace ("#06", "ввести код " + (goods ? "товара" : "услуги"));
					}
				}

			if (Flags.HasFlag (UserManualsFlags.CashiersHavePasswords))
				{
				text = text.Replace ("#4", "ввести пароль кассира, если он задан, ");
				text = text.Replace ("#5", "Ввести пароль кассира в случае запроса;" + RDLocale.RN + "• ");
				}
			else
				{
				text = text.Replace ("#4", "").Replace ("#5", "");
				}

			if (Flags.HasFlag (UserManualsFlags.DocumentsContainMarks))
				{
				text = text.Replace ("#C", ";" + RDLocale.RN + "• Отсканировать код маркировки");
				}
			else
				{
				text = text.Replace ("#C", "");
				}

			return text;
			}

		/// <summary>
		/// Возвращает подсказку, отображаемую перед разделами руководства пользователя
		/// </summary>
		public const string UserManualsTip = "<...> – индикация на дисплее, [...] – клавиши ККТ";
		}

	/// <summary>
	/// Возможные секции руководства пользователя
	/// </summary>
	public enum UserManualsSections
		{
		/// <summary>
		/// Открытие смены
		/// </summary>
		SessionOpen = 0x00001,

		/// <summary>
		/// Продажа за наличные
		/// </summary>
		Cash = 0x00002,

		/// <summary>
		/// Продажа безналичными
		/// </summary>
		Cashless = 0x00004,

		/// <summary>
		/// Продажа со штрихкодом
		/// </summary>
		Barcode = 0x00008,

		/// <summary>
		/// Продажа с количеством
		/// </summary>
		Quantity = 0x00010,

		/// <summary>
		/// Электронный чек
		/// </summary>
		ElectronicDoc = 0x00020,

		/// <summary>
		/// Аннулирование
		/// </summary>
		Cancellation = 0x00040,

		/// <summary>
		/// Возврат
		/// </summary>
		Reverse = 0x00080,

		/// <summary>
		/// Закрытие смены
		/// </summary>
		SessionClose = 0x00100,

		/// <summary>
		/// Коррекция даты
		/// </summary>
		DateCorrection = 0x00200,

		/// <summary>
		/// Коррекция времени
		/// </summary>
		TimeCorrection = 0x00400,

		/// <summary>
		/// Тест связи с сетью интернет
		/// </summary>
		InternetTest = 0x00800,

		/// <summary>
		/// Автотест / информация о ККТ
		/// </summary>
		Autotest = 0x01000,

		/// <summary>
		/// Запрос состояния ФН
		/// </summary>
		FNState = 0x02000,

		/// <summary>
		/// Запрос реквизитов регистраций
		/// </summary>
		RegistrationInfo = 0x04000,

		/// <summary>
		/// Техобнуление
		/// </summary>
		FactoryReset = 0x08000,

		/// <summary>
		/// Закрытие архива ФН
		/// </summary>
		ArchiveClose = 0x10000,
		}
	}
