using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RD_AAOW
	{
	/// <summary>
	/// Типы поддерживаемых штрих-кодов
	/// </summary>
	public enum SupportedBarcodesTypes
		{
		/// <summary>
		/// EAN-8
		/// </summary>
		EAN8 = 1,

		/// <summary>
		/// EAN-13
		/// </summary>
		EAN13 = 2,

		/// <summary>
		/// DataMatrix с маркировкой для пачек сигарет
		/// </summary>
		DataMatrixCigarettes = 3,

		/// <summary>
		/// DataMatrix с маркировкой для блоков сигарет
		/// </summary>
		DataMatrixCigarettesBlocks = 4,

		/// <summary>
		/// DataMatrix с маркировкой для лекарств
		/// </summary>
		DataMatrixMedicines = 5,

		/// <summary>
		/// DataMatrix с маркировкой для обуви
		/// </summary>
		DataMatrixShoes = 6,

		/// <summary>
		/// DataMatrix с маркировкой для молочной продукции
		/// </summary>
		DataMatrixMilk = 7,

		/// <summary>
		/// Неподдерживаемый тип штрих-кода
		/// </summary>
		Unsupported = -1
		}

	/// <summary>
	/// Класс обеспечивает доступ к функционалу контроля и расшифровки штрих-кодов
	/// </summary>
	public class BarCodes
		{
		// Переменные для EAN-8 и EAN-13
		private List<uint> rangeStart = new List<uint> ();
		private List<uint> rangeEnd = new List<uint> ();
		private List<string> descriptions = new List<string> ();
		private List<bool> country = new List<bool> ();

		// Константы для DataMatrix
		private const string dmEncodingLine = "ABCDEFGHIJKLMNOP" +
			"QRSTUVWXYZabcdef" +
			"ghijklmnopqrstuv" +
			"wxyz0123456789!\"" +
			"%&'*+-./_,:;=<>?";

		// Строка преобразования русской раскладки в английскую
		private const string REEncodingString = "?????????\x09\x0A??\x0D???????????????\x1D??" +
			" !@#$%&'()*+?-/|0123456789^$<=>&" +
			"@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_" +
			"`abcdefghijklmnopqrstuvwxyz{|}~?" +
			"ЂЃ‚ѓ„…†‡€‰Љ‹ЊЌЋЏђ‘’“”•–—?™љ›њќћџ" +
			" ЎўЈ¤Ґ¦§~©Є«¬?®Ї°±Ііґµ¶·`#є»јЅѕї" +
			"F<DULT:PBQRKVYJGHCNEA{WXIO}SM\">Z" +
			"f,dult;pbqrkvyjghcnea[wxio]sm'.z";

		/// <summary>
		/// Максимальная поддерживаемая длина данных штрих-кода
		/// </summary>
		public const uint MaxSupportedDataLength = 131;

		/// <summary>
		/// Конструктор. Инициализирует таблицу
		/// </summary>
		public BarCodes ()
			{
			// Получение файлов
#if !ANDROID
			string buf = Encoding.UTF8.GetString (RD_AAOW.Properties.TextToKKMResources.BarCodes);
#else
			string buf = Encoding.UTF8.GetString (RD_AAOW.Properties.Resources.BarCodes);
#endif
			StringReader SR = new StringReader (buf);

			// Формирование массива 
			string str;
			char[] splitters = new char[] { ';' };

			try
				{
				// Чтение параметров
				while ((str = SR.ReadLine ()) != null)
					{
					string[] values = str.Split (splitters);

					// Описания тегов
					if (values.Length < 3)
						continue;

					rangeStart.Add (uint.Parse (values[0]));
					if (values[1] == "")
						rangeEnd.Add (rangeStart[rangeStart.Count - 1]);
					else
						rangeEnd.Add (uint.Parse (values[1]));
					descriptions.Add (values[2]);
					country.Add (values.Length == 3);
					}
				}
			catch
				{
				throw new Exception ("Barcodes data reading failure, point 1");
				}

			// Завершено
			SR.Close ();
			}

		// Метод определяет тип штрих-кода
		private SupportedBarcodesTypes GetBarcodeType (string BarcodeData)
			{
			// Контроль
			string data = BarcodeData.Trim ();

			// Определение типа
			if ((data.Length == 8) && CheckEAN (data, false))
				return SupportedBarcodesTypes.EAN8;

			if ((data.Length == 13) && CheckEAN (data, true))
				return SupportedBarcodesTypes.EAN13;

			if (data.Length == 29)
				return SupportedBarcodesTypes.DataMatrixCigarettes;

			if (data.Length == 41)
				return SupportedBarcodesTypes.DataMatrixCigarettesBlocks;

			if (data.Length == 83)
				return SupportedBarcodesTypes.DataMatrixMedicines;

			if (data.Length == 127)
				return SupportedBarcodesTypes.DataMatrixShoes;

			if ((data.Length == 45) || (data.Length == 51) || (data.Length == 30))
				return SupportedBarcodesTypes.DataMatrixMilk;

			return SupportedBarcodesTypes.Unsupported;
			}

		// Возвращает читаемое название типа штрих-кода
		private string GetBarcodeTypeName (SupportedBarcodesTypes Type)
			{
			switch (Type)
				{
				case SupportedBarcodesTypes.EAN8:
				case SupportedBarcodesTypes.EAN13:
					return Type.ToString ();

				case SupportedBarcodesTypes.DataMatrixCigarettes:
					return "DataMatrix для пачек сигарет";

				case SupportedBarcodesTypes.DataMatrixCigarettesBlocks:
					return "DataMatrix для блоков сигарет";

				case SupportedBarcodesTypes.DataMatrixMedicines:
					return "DataMatrix для лекарств, остатков обуви, одежды и парфюмерии";

				case SupportedBarcodesTypes.DataMatrixShoes:
					return "DataMatrix для обуви";

				case SupportedBarcodesTypes.DataMatrixMilk:
					return "DataMatrix для молочной продукции";

				default:
					throw new Exception ("Unexpected barcode type, point 1");
				}
			}

		// Контроль целостности кодов типа EAN
		private bool CheckEAN (string BarcodeData, bool EAN13)
			{
			// Разбор штрихкода
			int checksum = 0;
			for (int i = 0; i < (EAN13 ? 12 : 7); i++)
				{
				byte step = ((i % 2) == (EAN13 ? 1 : 0)) ? (byte)3 : (byte)1;
				try
					{
					checksum += (byte.Parse (BarcodeData[i].ToString ()) * step);
					}
				catch
					{
					return false;
					}
				}

			return ((10 - (checksum % 10)).ToString () == BarcodeData[EAN13 ? 12 : 7].ToString ());
			}

		// Метод возвращает область применения штрих-кода или страну-производителя, которой он соответствует
		private string GetEANUsage (string BarcodeData)
			{
			// Контроль
			const string unknownPrefix = "применение: неизвестно";
			if (string.IsNullOrWhiteSpace (BarcodeData) || (BarcodeData.Length < 3))
				return unknownPrefix;

			// Поиск
			uint prefix = 0;
			try
				{
				prefix = uint.Parse (BarcodeData.Substring (0, 3));
				}
			catch
				{
				return unknownPrefix;
				}

			for (int i = 0; i < rangeStart.Count; i++)
				if ((prefix >= rangeStart[i]) && (prefix <= rangeEnd[i]))
					return (country[i] ? "производитель: " : "применение: ") + descriptions[i];

			return unknownPrefix;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода по его данным
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string GetBarcodeDescription (string BarcodeData)
			{
			// Контроль
			if (string.IsNullOrWhiteSpace (BarcodeData))
				return "Штрих-код неполный, некорректный или не поддерживается";

			// Тип штрих-кода
			string data = BarcodeData.Replace ("\x1D", ""); // Сброс суффиксов DataMatrix
			SupportedBarcodesTypes type = GetBarcodeType (data);

			string res = "Длина данных (без спецсимволов): " + data.Length.ToString () + "\r\n";
			if (type == SupportedBarcodesTypes.Unsupported)
				return res + "Штрих-код неполный, некорректный или не поддерживается";
			res += "Тип штрих-кода: " + GetBarcodeTypeName (type) + "\r\n";

			// Разбор
			switch (type)
				{
				case SupportedBarcodesTypes.EAN8:
				case SupportedBarcodesTypes.EAN13:
					res += ("  " + GetEANUsage (data));
					break;

				case SupportedBarcodesTypes.DataMatrixCigarettes:
					res += ("\r\n" + ParseCigarettesDataMatrix (data));
					break;

				case SupportedBarcodesTypes.DataMatrixCigarettesBlocks:
					res += ("\r\n" + ParseCigarettesBlocksDataMatrix (data));
					break;

				case SupportedBarcodesTypes.DataMatrixMedicines:
					res += ("\r\n" + ParseMedicinesDataMatrix (data));
					break;

				case SupportedBarcodesTypes.DataMatrixShoes:
					res += ("\r\n" + ParseShoesDataMatrix (data));
					break;

				case SupportedBarcodesTypes.DataMatrixMilk:
					res += ("\r\n" + ParseMilkDataMatrix (data));
					break;
				}

			// Завершено
			return res;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода DataMatrix для маркированных сигарет
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string ParseCigarettesDataMatrix (string BarcodeData)
			{
			string res = "GTIN: " + BarcodeData.Substring (0, 14) + ", ";
			if (!CheckEAN (BarcodeData.Substring (1, 13), true))
				res += "EAN-13 некорректен;\r\n";
			else if (BarcodeData.StartsWith ("000000"))
				res += (GetEANUsage (BarcodeData.Substring (6, 8)) + "\r\n");
			else
				res += (GetEANUsage (BarcodeData.Substring (1, 13)) + "\r\n");
			res += ("Серийный номер упаковки: " + BarcodeData.Substring (14, 7) + " (" +
				DecodeDMLine (BarcodeData.Substring (14, 7), false) + ")\r\n\r\n");

			res += ("Максимальная розничная цена: " +
				DecodeDMLine (BarcodeData.Substring (21, 4), true) + "\r\n");
			res += ("Код проверки: " + BarcodeData.Substring (25, 4) + " (" +
				DecodeDMLine (BarcodeData.Substring (25, 4), false) + ")");

			return res;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода DataMatrix для маркированных лекарств
		/// (стандарт ППРФ 1556)
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string ParseMedicinesDataMatrix (string BarcodeData)
			{
			// Контроль разметки
			if ((BarcodeData.Substring (0, 2) != "01") || (BarcodeData.Substring (16, 2) != "21") ||
				(BarcodeData.Substring (31, 2) != "91") || (BarcodeData.Substring (37, 2) != "92"))
				return "Разметка данных DataMatrix не соответствует стандарту";

			string res = "GTIN: " + BarcodeData.Substring (2, 14) + ", ";
			if (!CheckEAN (BarcodeData.Substring (3, 13), true))
				res += "EAN-13 некорректен\r\n";
			else
				res += (GetEANUsage (BarcodeData.Substring (3, 13)) + "\r\n");
			res += ("Серийный номер: " + BarcodeData.Substring (18, 13) + "\r\n\r\n");

			res += ("Номер кода проверки: " + BarcodeData.Substring (33, 4) + "\r\n");
			res += ("Код проверки:\r\n" + BarcodeData.Substring (39, 44) + "\r\n(\r\n" +
				 ConvertFromBASE64 (BarcodeData.Substring (39, 44)) + "\r\n)");

			return res;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода DataMatrix для блоков сигарет
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string ParseCigarettesBlocksDataMatrix (string BarcodeData)
			{
			// Контроль разметки
			if ((BarcodeData.Substring (0, 2) != "01") || (BarcodeData.Substring (16, 2) != "21") ||
				(BarcodeData.Substring (25, 4) != "8005") ||
				(BarcodeData.Substring (35, 2) != "93"))
				return "Разметка данных DataMatrix не соответствует стандарту";

			string res = "GTIN: " + BarcodeData.Substring (2, 14) + ", ";
			if (!CheckEAN (BarcodeData.Substring (3, 13), true))
				res += "EAN-13 некорректен\r\n";
			else
				res += (GetEANUsage (BarcodeData.Substring (3, 13)) + "\r\n");
			res += ("Серийный номер: " + BarcodeData.Substring (18, 7) + "\r\n\r\n");

			string price = "(не читается)";
			try
				{
				price = (ulong.Parse (BarcodeData.Substring (29, 6)) / 100.0).ToString ("F02") + " р.";
				}
			catch { }
			res += ("Максимальная розничная цена: " + price + "\r\n");
			res += ("Код проверки: " + BarcodeData.Substring (37, 4) + " (" +
				DecodeDMLine (BarcodeData.Substring (37, 4), false) + ")");

			return res;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода DataMatrix для обуви
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string ParseShoesDataMatrix (string BarcodeData)
			{
			// Контроль разметки
			if ((BarcodeData.Substring (0, 2) != "01") || (BarcodeData.Substring (16, 2) != "21") ||
				(BarcodeData.Substring (31, 2) != "91") || (BarcodeData.Substring (37, 2) != "92"))
				return "Разметка данных DataMatrix не соответствует стандарту";

			string res = "GTIN: " + BarcodeData.Substring (2, 14) + ", ";
			if (!CheckEAN (BarcodeData.Substring (3, 13), true))
				res += "EAN-13 некорректен;\r\n";
			else
				res += (GetEANUsage (BarcodeData.Substring (3, 13)) + "\r\n");
			res += ("Серийный номер: " + BarcodeData.Substring (18, 13) + " (" +
				DecodeDMLine (BarcodeData.Substring (18, 13), false) + ")\r\n\r\n");

			res += ("Номер кода проверки: " + BarcodeData.Substring (33, 4) + "\r\n");
			res += ("Код проверки:\r\n" + BarcodeData.Substring (39, 88) + "\r\n(\r\n" +
				 ConvertFromBASE64 (BarcodeData.Substring (39, 88)) + "\r\n)");

			return res;
			}

		/// <summary>
		/// Метод возвращает описание штрих-кода DataMatrix для маркированных лекарств
		/// </summary>
		/// <param name="BarcodeData">Данные штрих-кода</param>
		public string ParseMilkDataMatrix (string BarcodeData)
			{
			// Контроль разметки
			if (BarcodeData.Length != 30)
				{
				if ((BarcodeData.Substring (0, 2) != "01") || (BarcodeData.Substring (16, 2) != "21") ||
					(BarcodeData.Length == 45) && ((BarcodeData.Substring (31, 2) != "17") ||
					(BarcodeData.Substring (39, 2) != "93")) ||
					(BarcodeData.Length == 51) && ((BarcodeData.Substring (31, 4) != "7003") ||
					(BarcodeData.Substring (45, 2) != "93")))
					return "Разметка данных DataMatrix не соответствует стандарту";
				}
			else
				{
				if ((BarcodeData.Substring (0, 2) != "01") || (BarcodeData.Substring (16, 2) != "21") ||
					(BarcodeData.Substring (24, 2) != "93"))
					return "Разметка данных DataMatrix не соответствует стандарту";
				}

			string res = "GTIN: " + BarcodeData.Substring (2, 14) + ", ";
			if (!CheckEAN (BarcodeData.Substring (3, 13), true))
				res += "EAN-13 некорректен\r\n";
			else
				res += (GetEANUsage (BarcodeData.Substring (3, 13)) + "\r\n");

			int offset = 41;
			if (BarcodeData.Length != 30)
				{
				res += ("Серийный номер: " + BarcodeData.Substring (18, 13) + "\r\n\r\n");

				res += ("Срок годности: до ");
				if (BarcodeData.Substring (31, 2) == "17")
					{
					res += (BarcodeData.Substring (37, 2) + "." + BarcodeData.Substring (35, 2) + ".20" +
						BarcodeData.Substring (33, 2));
					}
				else
					{
					res += (BarcodeData.Substring (39, 2) + "." + BarcodeData.Substring (37, 2) + ".20" +
						BarcodeData.Substring (35, 2) + ", " + BarcodeData.Substring (41, 2) + ":" +
						BarcodeData.Substring (43, 2));
					offset += 6;
					}
				}
			else
				{
				res += ("Серийный номер: " + BarcodeData.Substring (18, 6) + "\r\n");

				offset = 26;
				}

			res += ("\r\nКод проверки: " + BarcodeData.Substring (offset, 4) + " (" +
				DecodeDMLine (BarcodeData.Substring (offset, 4), false) + ")");

			return res;
			}

		// Расшифровка числовых данных в DataMatrix
		private string DecodeDMLine (string Data, bool AsPrice)
			{
			// Сборка числа
			decimal v = 0, mul1, mul2;

			for (int i = 1; i <= Data.Length; i++)
				{
				mul1 = (decimal)Math.Pow (dmEncodingLine.Length, Data.Length - i);
				if ((mul2 = dmEncodingLine.IndexOf (Data[i - 1])) < 0)
					return "значение не читается";
				v += mul1 * mul2;
				}

			// Вывод
			if (AsPrice)
				return (v / (decimal)100.0).ToString ("F02") + " р.";

			return v.ToString ();
			}

		// Метод собирает hex-представление данных из строки BASE64
		private string ConvertFromBASE64 (string Data)
			{
			if (string.IsNullOrWhiteSpace (Data))
				return "";

			byte[] data;
			try
				{
				data = Convert.FromBase64String (Data);
				}
			catch
				{
				return "";
				}

			string res = "";
			for (int i = 0; i < data.Length; i++)
				res += (data[i].ToString ("X02") + " ");

			return res.Trim ();
			}

		/// <summary>
		/// Метод меняет раскладку клавиатуры в данных штрих-кода (при необходимости)
		/// </summary>
		/// <param name="Data">Данные штрих-кода</param>
		/// <returns>Преобразованные данные</returns>
		public static string ConvertFromRussianKeyboard (string Data)
			{
			// Контроль состава штрих-кода
			if (string.IsNullOrWhiteSpace (Data))
				return Data;

			byte[] data = new byte[Data.Length];
			for (int i = 0; i < Data.Length; i++)
				data[i] = KKTSupport.CharToCP1251 (Data[i]);

			bool needsDecoding = false;
			for (int i = 0; i < data.Length; i++)
				if (data[i] > 127)
					{
					needsDecoding = true;
					break;
					}
			if (!needsDecoding)
				return Data;

			// Рекодировка
			string result = "";
			for (int i = 0; i < data.Length; i++)
				result += REEncodingString[data[i]];

			return result;
			}
		}
	}
