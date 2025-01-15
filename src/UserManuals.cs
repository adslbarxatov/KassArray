using System.Collections.Generic;
using System.IO;

namespace RD_AAOW
	{
	/// <summary>
	/// Поддерживаемые секции в разделе пользовательских инструкций
	/// </summary>
	public enum UserGuidesTypes
		{
		/// <summary>
		/// Открытие смены
		/// </summary>
		SessionOpen,

		/// <summary>
		/// Продажа за наличные
		/// </summary>
		SellForCash,

		/// <summary>
		/// Продажа по карте
		/// </summary>
		SellForCard,

		/// <summary>
		/// Продажа по штрих-коду
		/// </summary>
		SellWithBarcode,

		/// <summary>
		/// Продажа с количеством
		/// </summary>
		SellWithQuantity,

		/// <summary>
		/// Продажа с электронным чеком
		/// </summary>
		SellWithSMS,

		/// <summary>
		/// Аннулирование
		/// </summary>
		Annulment,

		/// <summary>
		/// Возврат
		/// </summary>
		Reverse,

		/// <summary>
		/// Внесение наличных
		/// </summary>
		CashDeposit,

		/// <summary>
		/// Закрытие смены
		/// </summary>
		SessionClose,

		/// <summary>
		/// Коррекция даты
		/// </summary>
		DateCrorrection,

		/// <summary>
		/// Коррекция времени
		/// </summary>
		TimeCorrection,

		/// <summary>
		/// Тест связи с сетью интернет
		/// </summary>
		ConnectionTest,

		/// <summary>
		/// Автотест / информация о ККТ
		/// </summary>
		Autotest,

		/// <summary>
		/// Запрос состояния ФН
		/// </summary>
		FiscalStorageStatus,

		/// <summary>
		/// Запрос реквизитов регистраций
		/// </summary>
		RegistrationsRequest,

		/// <summary>
		/// Техобнуление
		/// </summary>
		SystemReset,

		/// <summary>
		/// Закрытие архива ФН
		/// </summary>
		FiscalStorageClose,
		}

	/// <summary>
	/// Класс обеспечивает доступ к инструкциям по работе с ККТ
	/// </summary>
	public class UserGuides
		{
		// Переменные
		private List<string> names = new List<string> ();
		private List<List<string>> operations = new List<List<string>> ();

		/// <summary>
		/// Возвращает список операций, для которых доступны инструкции
		/// </summary>
		/// <param name="ForCashiers">Флаг возврата операций, доступных для кассира</param>
		public static string[] OperationTypes (bool ForCashiers)
			{
			if (ForCashiers)
				return new string[] {
					operationTypes[(int)UserGuidesTypes.SessionOpen],
					operationTypes[(int)UserGuidesTypes.SellForCash],
					operationTypes[(int)UserGuidesTypes.SellForCard],
					operationTypes[(int)UserGuidesTypes.SellWithBarcode],
					operationTypes[(int)UserGuidesTypes.SellWithQuantity],
					operationTypes[(int)UserGuidesTypes.SellWithSMS],
					operationTypes[(int)UserGuidesTypes.Annulment],
					operationTypes[(int)UserGuidesTypes.Reverse],
					operationTypes[(int)UserGuidesTypes.CashDeposit],
					operationTypes[(int)UserGuidesTypes.SessionClose],
					};

			return operationTypes;
			}

		/// <summary>
		/// Возвращает рекомендуемое начальное состояние разделов руководства пользователя
		/// </summary>
		public const uint GuidesSectionsInitialState = 0x0FED7;

		/// <summary>
		/// Возвращает рекомендуемое начальное состояние флагов формирования руководства пользователя
		/// </summary>
		public const uint GuidesFlagsInitialState = 0x02;

		private static string[] operationTypes = new string[] {
			"Открытие смены",
			"Продажа за наличные",
			"Продажа по карте",
			"Продажа по штрих-коду",		// 3, 0x????7
			"Продажа с количеством",
			"Продажа с электронным чеком",
			"Аннулирование",
			"Возврат",						// 7, 0x???D?
			"Внесение",
			"Закрытие смены",

			"Коррекция даты",
			"Коррекция времени",			// 11, 0x??E??
			"Тест связи с сетью интернет",
			"Автотест / информация о ККТ",
			"Запрос состояния ФН",
			"Запрос реквизитов регистраций",// 15, 0x?F???
			"Техобнуление",
			"Закрытие архива ФН",			// 17, 0x0????
			};

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public UserGuides ()
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
					UserGuidesTypes type = (UserGuidesTypes)i;

					// Выборочные подстановки
					switch (type)
						{
						case UserGuidesTypes.SessionOpen:
							op = op.Replace ("&B", "для открытия смены");
							op = op.Replace ("• !", "!");
							op = op.Replace ("• " + RDLocale.RN, RDLocale.RN);
							break;

						case UserGuidesTypes.SellForCash:
						case UserGuidesTypes.SellForCard:
							op = op.Replace ("• &1", "#1• ");
							op = op.Replace ("&2", "#6");
							op = op.Replace ("&02", "#06");
							op = op.Replace ("&A", "для закрытия чека");
							break;

						case UserGuidesTypes.SellWithBarcode:
						case UserGuidesTypes.SellWithQuantity:
							op = op.Replace ("• &1", "#1• Закрыть чек согласно способу оплаты");
							op = op.Replace ("&2", "#6");
							op = op.Replace ("&02", "#06");
							op = op.Replace ("&3", "Отсканировать штрих-код товара");

							if (type == UserGuidesTypes.SellWithQuantity)
								op = "Действие выполняется для всех позиций, " +
									"в которых количество не равно единице:" + RDLocale.RN + op;
							break;

						case UserGuidesTypes.SellWithSMS:
							op = op.Replace ("• !", "!");
							break;

						case UserGuidesTypes.Annulment:
							op = "Если требуется «отменить» чек, который ещё " +
								"не был закрыт, выполняется аннулирование:" + RDLocale.RN + op;
							break;

						case UserGuidesTypes.Reverse:
							op = "Если требуется «отменить» чек, который уже " +
								"был закрыт, выполняется возврат:" + RDLocale.RN + op +
								";" + RDLocale.RN + "• Выполнить действия, описанные " +
								"в разделе «" + operationTypes[1] + "» или «" + operationTypes[2] + "» (в зависимости " +
								"от способа расчёта)";
							break;

						case UserGuidesTypes.CashDeposit:
							op = "Если ККТ сообщает о недостатке наличных для возврата, следует " +
								"выполнить внесение:" + RDLocale.RN + op;
							break;

						case UserGuidesTypes.SessionClose:
							op += ";" + RDLocale.RN + "• Дождаться снятия отчёта";
							break;

						case UserGuidesTypes.DateCrorrection:
						case UserGuidesTypes.TimeCorrection:
							if (!op.StartsWith ("• ("))
								op = "! Необходимо предварительно закрыть смену;" + RDLocale.RN + op;
							break;

						case UserGuidesTypes.SystemReset:
							op = "! Необходимо убедиться, что сохранены все важные настройки;" +
								RDLocale.RN + op;
							break;

						case UserGuidesTypes.FiscalStorageClose:
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
		/// <param name="GuideType">Операция</param>
		/// <param name="Flags">Флаги, определающие состав руководства</param>
		public string GetGuide (uint KKTType, UserGuidesTypes GuideType, UserGuidesFlags Flags)
			{
			if (KKTType >= names.Count)
				return "";

			string text = operations[(int)GuideType][(int)KKTType];
			bool goods = !Flags.HasFlag (UserGuidesFlags.ProductBaseContainsServices);
			if ((GuideType >= UserGuidesTypes.SellForCash) && (GuideType <= UserGuidesTypes.SellWithQuantity))
				{
				if (Flags.HasFlag (UserGuidesFlags.MoreThanOneItemPerDocument))
					{
					text = text.Replace ("#1", "↑ (повторить описанные действия для всех " +
						(goods ? "товаров" : "услуг") + " в чеке);" + RDLocale.RN);
					}
				else
					{
					text = text.Replace ("#1", "");
					}

				if (Flags.HasFlag (UserGuidesFlags.ProductBaseContainsPrices))
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

			if (GuideType <= UserGuidesTypes.SellWithQuantity)
				{
				if (Flags.HasFlag (UserGuidesFlags.BaseContainsSingleItem))
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

			if (Flags.HasFlag (UserGuidesFlags.CashiersHavePasswords))
				{
				text = text.Replace ("#4", "ввести пароль кассира, если он задан, ");
				text = text.Replace ("#5", "Ввести пароль кассира в случае запроса;" + RDLocale.RN + "• ");
				}
			else
				{
				text = text.Replace ("#4", "").Replace ("#5", "");
				}

			if (Flags.HasFlag (UserGuidesFlags.DocumentsContainMarks))
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
	}
