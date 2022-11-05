using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Версии ФФД, описанные в текущем приказе
	/// </summary>
	public enum TLVTags_FFDVersions
		{
		/// <summary>
		/// Версии ФФД 1.05 и ниже
		/// </summary>
		FFD_105 = 2,

		/// <summary>
		/// Версия ФФД 1.1
		/// </summary>
		FFD_110 = 3,

		/// <summary>
		/// Версия ФФД 1.2
		/// </summary>
		FFD_120 = 4
		}

	/// <summary>
	/// Типы документов, описанные в текущем приказе
	/// </summary>
	public enum TLVTags_DocumentTypes
		{
		/// <summary>
		/// Отчёт о регистрации или перерегистрации
		/// </summary>
		Registration = 1,

		/// <summary>
		/// Открытие смены
		/// </summary>
		OpenSession = 2,

		/// <summary>
		/// Отчёт о ТСР
		/// </summary>
		CurrentState = 3,

		/// <summary>
		/// Чек или БСО
		/// </summary>
		Bill = 4,

		/// <summary>
		/// Чеки и БСО, включая коррекцию (общие теги)
		/// </summary>
		BillAndCorrection = 20,

		/// <summary>
		/// Чеки и БСО коррекции
		/// </summary>
		Correction = 5,

		/// <summary>
		/// Закрытие смены
		/// </summary>
		CloseSession = 6,

		/// <summary>
		/// Закрытие архива ФН
		/// </summary>
		CloseArchive = 7,

		/// <summary>
		/// Подтверждение ОФД
		/// </summary>
		OperatorConfirmation = 8,

		/// <summary>
		/// Все типы документов
		/// </summary>
		AllTypes = 9,

		/// <summary>
		/// Запрос о коде маркировки
		/// </summary>
		CodeRequest = 31,

		/// <summary>
		/// Уведомление о реализации кода маркировки
		/// </summary>
		ReleaseRequest = 33,

		/// <summary>
		/// Ответ на запрос
		/// </summary>
		CodeAnswer = 32,

		/// <summary>
		/// Квитанция на уведомление
		/// </summary>
		ReleaseAnswer = 34
		}

	/// <summary>
	/// Типы обязательности тегов, описанные в текущем приказе
	/// </summary>
	public enum TLVTags_ObligationStates
		{
		/// <summary>
		/// Не используется
		/// </summary>
		Unused = 0,

		/// <summary>
		/// Обязателен
		/// </summary>
		Required = 1,

		/// <summary>
		/// Обязателен при выполнении примечаний
		/// </summary>
		RequiredWith = 2,

		/// <summary>
		/// Необязателен при выполнении примечаний
		/// </summary>
		RecommendedWith = 3
		}

	/// <summary>
	/// Класс обеспечивает доступ к описаниям тегов TLV согласно актуальному ФФД
	/// </summary>
	public class TLVTags
		{
		// Переменные
		private List<uint> tags = new List<uint> ();
		private List<string> descriptions = new List<string> ();
		private List<string> types = new List<string> ();
		private List<string> possibleValues = new List<string> ();
		private List<int> links = new List<int> ();
		private List<List<int>> oblIndices = new List<List<int>> ();

		private List<TLVTags_FFDVersions> oblFFDVersions = new List<TLVTags_FFDVersions> ();
		private List<TLVTags_DocumentTypes> oblDocTypes = new List<TLVTags_DocumentTypes> ();
		private List<TLVTags_ObligationStates> oblPrintObligations = new List<TLVTags_ObligationStates> (),
			oblVirtualObligations = new List<TLVTags_ObligationStates> ();
		private List<string> oblPrintConditions = new List<string> (),
			oblVirtualConditions = new List<string> (),
			oblTables = new List<string> (),
			oblParents = new List<string> ();

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public TLVTags ()
			{
			// Получение файлов
#if !ANDROID
			string tlv = Encoding.UTF8.GetString (RD_AAOW.Properties.TextToKKMResources.TLVTags);
			string obl = Encoding.UTF8.GetString (RD_AAOW.Properties.TextToKKMResources.Obligation);
#else
			string tlv = Encoding.UTF8.GetString (RD_AAOW.Properties.Resources.TLVTags);
			string obl = Encoding.UTF8.GetString (RD_AAOW.Properties.Resources.Obligation);
#endif
			StringReader tlvSR = new StringReader (tlv);
			StringReader oblSR = new StringReader (obl);

			// Формирование массива 
			string str;
			char[] tlvSplitter = new char[] { ';' };
			char[] oblSplitter = new char[] { '\t' };

			string[] tlvValues,
				oblValues = oblSR.ReadLine ().Split (oblSplitter, StringSplitOptions.RemoveEmptyEntries);

			try
				{
				// Чтение параметров
				while ((str = tlvSR.ReadLine ()) != null)
					{
					tlvValues = str.Split (tlvSplitter, StringSplitOptions.RemoveEmptyEntries);

					// Описания тегов
					if (tlvValues.Length >= 3)
						{
						// Список команд
						tags.Add (uint.Parse (tlvValues[0]));
						descriptions.Add (BuildDescription (tlvValues[1], tags[tags.Count - 1]));
						types.Add (BuildType (tlvValues[2]));

						if (tlvValues.Length > 3)
							links.Add (int.Parse (tlvValues[3]) - 1);
						else
							links.Add (-1);

						oblIndices.Add (new List<int> ());

						// Разбор списка обязательности
						while ((oblValues.Length == 8) && (uint.Parse (oblValues[1]) == tags[tags.Count - 1]))
							{
							uint type = uint.Parse (oblValues[0]);
							oblFFDVersions.Add ((TLVTags_FFDVersions)(type / 100));
							oblDocTypes.Add ((TLVTags_DocumentTypes)(type % 100));

							oblPrintObligations.Add ((TLVTags_ObligationStates)uint.Parse (oblValues[2]));
							oblPrintConditions.Add ((oblValues[3] == "-") ? "" : oblValues[3]);
							oblVirtualObligations.Add ((TLVTags_ObligationStates)uint.Parse (oblValues[4]));
							oblVirtualConditions.Add ((oblValues[5] == "-") ? "" : oblValues[5]);
							oblTables.Add (oblValues[6]);
							oblParents.Add ((oblValues[7] == "-") ? "" : oblValues[7]);

							oblIndices[oblIndices.Count - 1].Add (oblFFDVersions.Count - 1);
							if ((str = oblSR.ReadLine ()) == null)
								break;
							oblValues = str.Split (oblSplitter, StringSplitOptions.RemoveEmptyEntries);
							}
						}

					// Описания значений тегов
					else if (tlvValues.Length == 2)
						{
						if (int.Parse (tlvValues[0]) > possibleValues.Count)
							possibleValues.Add ("");

						possibleValues[possibleValues.Count - 1] += (tlvValues[1] + "\r\n");
						}

					// Пропуски
					else
						{
						continue;
						}
					}
				}
			catch
				{
				throw new Exception ("TLV tags data reading failure, point 1");
				}

			// Завершено
			tlvSR.Close ();
			oblSR.Close ();
			}

		/// <summary>
		/// Метод возвращает описание тип тега в соответствующие свойства экземпляра TLVTags
		/// </summary>
		/// <param name="Tag">Номер тега или фрагмент описания</param>
		/// <param name="FFD">Версия ФФД для определения обязательности</param>
		/// <returns>Возвращает true в случае обнаружения</returns>
#if ANDROID
		public bool FindTag (string Tag)
#else
		public bool FindTag (string Tag, TLVTags_FFDVersions FFD)
#endif
			{
			// Защита
			if (string.IsNullOrWhiteSpace (Tag))
				return false;
			string tagString = Tag.ToLower ();

			// Поиск по коду
			uint tag = 0;
			int i;
			if (uint.TryParse (tagString, out tag))
				{
				i = tags.IndexOf (tag);
				}

			// Поиск по описанию
			else
				{
				lastIndex++;
				for (i = 0; i < descriptions.Count; i++)
					if (descriptions[(i + lastIndex) % descriptions.Count].ToLower ().Contains (tagString))
						{
						i = lastIndex = (i + lastIndex) % descriptions.Count;
						break;
						}
				}

			// Проверка
			if ((i < 0) || (i >= descriptions.Count))
				return false;

			// Найдено
			lastType = types[i];
			lastDescription = descriptions[i];

			if (links[i] < 0)
				lastValuesSet = "";
			else
				lastValuesSet = possibleValues[links[i]];
			if (lastValuesSet.EndsWith ("\r\n"))
				lastValuesSet = lastValuesSet.Substring (0, lastValuesSet.Length - 2);

#if ANDROID
			lastObligation = "<b>Для ФФД 1.05:</b><br/><i>" +
				BuildObligation (i, TLVTags_FFDVersions.FFD_105).Replace ("\r\n", "<br/>") +
				"</i><br/><br/><b>Для ФФД 1.1:</b><br/><i>" +
				BuildObligation (i, TLVTags_FFDVersions.FFD_110).Replace ("\r\n", "<br/>") +
				"</i><br/><br/><b>Для ФФД 1.2:</b><br/><i>" +
				BuildObligation (i, TLVTags_FFDVersions.FFD_120).Replace ("\r\n", "<br/>") + "</i>";
#else
			lastObligation = BuildObligation (i, FFD);
#endif

			return true;
			}
		private int lastIndex = 0;

		// Метод распаковывает текст описания
		private string BuildDescription (string Source, uint TagNumber)
			{
			string res = Source;

			res = res.Replace ("FLAGPR", "(печатается, только если указан)");
			res = res.Replace ("OBJ", "товара, работы, услуги, платежа, выплаты или иного предмета расчёта");
			res = res.Replace ("APPEND", "(дополнительный реквизит чека, условия применения и значение которого " +
				"определяется ФНС России)");
			res = res.Replace ("KTFMT", "Код товара в формате");
			res = res.Replace ("OLDCOR", "Итоговая сумма корректировок НДС по");
			res = res.Replace ("TOTALS", "Итоговые количества и суммы расчётов");
			res = res.Replace ("EXTREQ", "Дополнительный реквизит отчёта");
			res = res.Replace ("EXTREP", "Дополнительные данные отчёта");
			res = res.Replace ("OFD", "оператора фискальных данных");
			res = res.Replace ("INC1", "Псевдотег, инкапсулирующий");
			res = res.Replace ("INC2", "в выгрузке архива ФН");
			res = res.Replace ("OLD", "[устарел]");

			return TagNumber.ToString () + " (0x" + TagNumber.ToString ("X4") + "): " + res;
			}

		// Метод распаковывает тип тега
		private string BuildType (string Source)
			{
			string res = Source;

			// Отдельная подмена для ссылок на таблицы
			if (res.Contains ("TABLE"))
				{
				int i = res.IndexOf ("TABLE");
				res = res.Replace ("TABLE", "");
				uint v = uint.Parse (res.Substring (i));
				res = res.Substring (0, i) + " (значения приведены в таблице " + v.ToString () + ")";
				}

			// Простые замены
			if (res == "BOOL")
				return "Целое, 1 байт (0 или 1)";
			if (res == "UNIX")
				return "UnixTime (целое, 4 байта)";
			if (res == "INNF")
				return "Строка длиной 12 байт (дополняется пробелами справа)";
			if (res == "STRD")
				return "Строка длиной 10 байт в формате ДД.ММ.ГГГГ";
			if (res == "SCTUS")
				return "Структура произвольной длины";
			if (res == "PRICE")
				return "Целое с фиксированной запятой (до 6 байт, 2 знака)";

			// Замены с длинами
			if (res.StartsWith ("STRLE"))
				{
				res = res.Replace ("STRLE", "");
				uint v = uint.Parse (res);
				return "Строка длиной не более " + v.ToString () + " байт";
				}
			if (res.StartsWith ("STREA"))
				{
				res = res.Replace ("STREA", "");
				uint v = uint.Parse (res);
				return "Строка длиной " + v.ToString () + " байт (дополняется пробелами справа при необходимости)";
				}
			if (res.StartsWith ("STRE"))
				{
				res = res.Replace ("STRE", "");
				uint v = uint.Parse (res);
				return "Строка длиной " + v.ToString () + " байт";
				}
			if (res.StartsWith ("SCTLE"))
				{
				res = res.Replace ("SCTLE", "");
				uint v = uint.Parse (res);
				return "Структура длиной не более " + v.ToString () + " байт";
				}
			if (res.StartsWith ("ARRAY"))
				{
				res = res.Replace ("ARRAY", "");
				uint v = uint.Parse (res);
				return "Байт-массив длиной не более " + v.ToString () + " байт";
				}

			// Подстановки
			if (res.StartsWith ("UINT8"))
				res = res.Replace ("UINT8", "Целое, 1 байт");
			if (res.StartsWith ("UINT32"))
				res = res.Replace ("UINT32", "Целое, 4 байта");
			if (res.StartsWith ("BIT8"))
				res = res.Replace ("BIT8", "Битовое поле, 1 байт");
			if (res.StartsWith ("UINT16"))
				res = res.Replace ("UINT16", "Целое, 2 байта");
			if (res.StartsWith ("BIT32"))
				res = res.Replace ("BIT32", "Битовое поле, 4 байта");

			return res;
			}

		// Метод собирает требование обязательности
		private string BuildObligation (int Index, TLVTags_FFDVersions FFD)
			{
			string res = "";

			for (int i = 0; i < oblIndices[Index].Count; i++)
				{
				if (oblFFDVersions[oblIndices[Index][i]] != FFD)
					continue;

				res += "• Для ";

				// Типы документов
				switch (oblDocTypes[oblIndices[Index][i]])
					{
					case TLVTags_DocumentTypes.AllTypes:
						res += "всех типов документов";
						break;

					case TLVTags_DocumentTypes.Bill:
						res += "чеков и БСО";
						break;

					case TLVTags_DocumentTypes.BillAndCorrection:
						res += "чеков и БСО (в том числе – коррекции)";
						break;

					case TLVTags_DocumentTypes.CloseArchive:
						res += "отчётов о закрытии архива ФН";
						break;

					case TLVTags_DocumentTypes.CloseSession:
						res += "отчётов о закрытии смены";
						break;

					case TLVTags_DocumentTypes.CodeAnswer:
						res += "ответов на запросы о кодах маркировки";
						break;

					case TLVTags_DocumentTypes.CodeRequest:
						res += "запросов о кодах маркировки";
						break;

					case TLVTags_DocumentTypes.Correction:
						res += "чеков и БСО коррекции";
						break;

					case TLVTags_DocumentTypes.CurrentState:
						res += "отчётов о состоянии расчётов";
						break;

					case TLVTags_DocumentTypes.OpenSession:
						res += "отчётов об открытии смены";
						break;

					case TLVTags_DocumentTypes.OperatorConfirmation:
						res += "подтверждений ОФД";
						break;

					case TLVTags_DocumentTypes.Registration:
						res += "отчётов о регистрации и перерегистрации";
						break;

					case TLVTags_DocumentTypes.ReleaseAnswer:
						res += "квитанций о реализации маркированного товара";
						break;

					case TLVTags_DocumentTypes.ReleaseRequest:
						res += "уведомлений о реализации маркированного товара";
						break;
					}

				// Обязательность
				res += " ";
				for (int j = 0; j < 2; j++)
					{
					TLVTags_ObligationStates os = ((j == 0) ? oblPrintObligations[oblIndices[Index][i]] :
						oblVirtualObligations[oblIndices[Index][i]]);
					switch (os)
						{
						case TLVTags_ObligationStates.Unused:
							res += "не предусмотрен";
							break;

						case TLVTags_ObligationStates.Required:
							res += "обязателен";
							break;

						case TLVTags_ObligationStates.RequiredWith:
						case TLVTags_ObligationStates.RecommendedWith:
							if (os == TLVTags_ObligationStates.RecommendedWith)
								res += "не";
							res += "обязателен";

							string condition = ((j == 0) ? oblPrintConditions[oblIndices[Index][i]] :
								oblVirtualConditions[oblIndices[Index][i]]);
							bool cond = !string.IsNullOrWhiteSpace (condition);
							bool parent = !string.IsNullOrWhiteSpace (oblParents[oblIndices[Index][i]]);

							if (cond || parent)
								{
								res += " (";
								if (cond)
									res += ("при соблюдении прим. " + condition + " к табл. " +
										oblTables[oblIndices[Index][i]]);

								if (cond && parent)
									res += " и ";

								if (parent)
									{
									if (os == TLVTags_ObligationStates.RecommendedWith)
										res += ("даже ");

									res += ("при наличии тег");
									if (oblParents[oblIndices[Index][i]].Length > 4)
										res += "ов ";
									else
										res += "а ";
									res += oblParents[oblIndices[Index][i]];
									}

								res += ")";
								}
							break;
						}

					res += ((j == 0) ? " в печ. форме; " : " в эл. форме;\r\n");
					}
				}

			// Завершено
			if (res.EndsWith (";\r\n"))
				res = res.Substring (0, res.Length - 3);
			if (string.IsNullOrWhiteSpace (res))
				res = "• Нет сведений об обязательности тега в данном ФФД";

			return res;
			}

		/// <summary>
		/// Возвращает последний найденный тип тега
		/// </summary>
		public string LastType
			{
			get
				{
				return lastType;
				}
			}
		private string lastType = "";

		/// <summary>
		/// Возвращает последнее найденное описание тега
		/// </summary>
		public string LastDescription
			{
			get
				{
				return lastDescription;
				}
			}
		private string lastDescription = "";

		/// <summary>
		/// Возвращает последний найденный набор значений тега
		/// </summary>
		public string LastValuesSet
			{
			get
				{
				return lastValuesSet;
				}
			}
		private string lastValuesSet = "";

		/// <summary>
		/// Возвращает последнее найденное требование обязательности тега
		/// </summary>
		public string LastObligation
			{
			get
				{
				return lastObligation;
				}
			}
		private string lastObligation = "";

		/// <summary>
		/// Возвращает приказ, обосновывающий обязательность реквизитов
		/// </summary>
		public const string ObligationBase = "Основание: прил. №2 к приказу ФНС от 14.09.20 № ЕД-7-20/662@";

		/// <summary>
		/// Возвращает активную ссылку на приказ, обосновывающий обязательность реквизитов
		/// </summary>
		public const string ObligationBaseLink = "https://nalog.gov.ru/rn77/about_fts/docs/10020801/";
		}
	}
