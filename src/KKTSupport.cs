﻿using System;
using System.IO;
using System.Collections.Generic;


#if ANDROID
	using Android.Graphics;
	using Android.OS;
	using Android.Print;
	using Android.Print.Pdf;
	using Android.Runtime;
#else
	using System.Drawing;
	using System.Drawing.Printing;
	using System.Windows.Forms;
#endif

namespace RD_AAOW
	{
	/// <summary>
	/// Список параметров для определения срока жизни ФН
	/// </summary>
	public enum FNLifeFlags
		{
		/// <summary>
		/// Флаг указывает на выбор ФН на 13/15 месяцев вместо 36
		/// </summary>
		FN15 = 0x00001,

		/// <summary>
		/// Флаг указывает на выбор ФН на 13 месяцев вместо 15
		/// </summary>
		FNExactly13 = 0x00002,

		/// <summary>
		/// Флаг указыввает на применение ОСН
		/// </summary>
		GenericTax = 0x00010,

		/// <summary>
		/// Флаг указывает на режим товаров вместо услуг
		/// </summary>
		Goods = 0x00020,

		/// <summary>
		/// Флаг указывает на сезонный режим работы
		/// </summary>
		Season = 0x00040,

		/// <summary>
		/// Флаг указывает на агентскую схему работы
		/// </summary>
		Agents = 0x0080,

		/// <summary>
		/// Флаг указывает на наличие подакцизных товаров
		/// </summary>
		Excise = 0x00100,

		/// <summary>
		/// Флаг указывает на работу без передачи данных
		/// </summary>
		Autonomous = 0x00200,

		/// <summary>
		/// Флаг ФФД 1.2
		/// </summary>
		FFD12 = 0x00400,

		/// <summary>
		/// Флаг ФН с любой поддержкой ФФД 1.2
		/// </summary>
		FFD12Compatible = 0x00800,

		/// <summary>
		/// Флаг торговли маркированными товарами
		/// </summary>
		MarkGoods = 0x01000,

		/// <summary>
		/// Флаг азартных игр и лотерей
		/// </summary>
		GamblingAndLotteries = 0x02000,

		/// <summary>
		/// Флаг ломбардной деятельности и страхования
		/// </summary>
		PawnsAndInsurance = 0x04000,

		/// <summary>
		/// Флаг ФН с полной поддержкой ФФД 1.2
		/// </summary>
		FFD12FullSupport = 0x08000,
		}

	/// <summary>
	/// Доступные настроечные флаги для руководств пользователя
	/// </summary>
	public enum UserGuidesFlags
		{
		/// <summary>
		/// Кассиры работают с паролями
		/// </summary>
		CashiersHavePasswords = 0x0001,

		/// <summary>
		/// В чеках может быть более одной позиции
		/// </summary>
		MoreThanOneItemPerDocument = 0x0002,

		/// <summary>
		/// Номенклатура содержит цены
		/// </summary>
		ProductBaseContainsPrices = 0x0004,

		/// <summary>
		/// Номенклатура содержит услуги
		/// </summary>
		ProductBaseContainsServices = 0x0008,

		/// <summary>
		/// Номенклатура содержит маркированные товары
		/// </summary>
		DocumentsContainMarks = 0x0010,

		/// <summary>
		/// Номенклатура содержит единственное наименование
		/// </summary>
		BaseContainsSingleItem = 0x0020,

#if !ANDROID

		/// <summary>
		/// Необходимо добавить пользовательский логотип в печатное руководство
		/// </summary>
		AddManualLogo = 0x0040,

#endif

		/// <summary>
		/// Руководство для кассира
		/// </summary>
		GuideForCashier = 0x8000,
		}

	/// <summary>
	/// Доступные результаты оценки применимости ФН с указанными параметрами регистрации и сроком жизни
	/// </summary>
	public enum FNLifeStatus
		{
		/// <summary>
		/// ФН может быть использован с указанными параметрами
		/// </summary>
		Acceptable,

		/// <summary>
		/// ФН может быть использован, но не рекомендуется к применению с указанными параметрами
		/// </summary>
		Unwelcome,

		/// <summary>
		/// ФН не поддерживает использование с указанными параметрами
		/// </summary>
		Inacceptable,

		/// <summary>
		/// ФН допускает использование с указанными параметрами, но данная информация не проверена на практике
		/// </summary>
		StronglyUnwelcome
		}

	/// <summary>
	/// Доступные теги регистрации из статуса ФН
	/// </summary>
	public enum RegTags
		{
		/// <summary>
		/// Адрес расчётов
		/// </summary>
		RegistrationAddress = 1009,

		/// <summary>
		/// Заводской номер ККТ
		/// </summary>
		KKTSerialNumber = 1013,

		/// <summary>
		/// ИНН ОФД
		/// </summary>
		OFDINN = 1017,

		/// <summary>
		/// ИНН пользователя
		/// </summary>
		UserINN = 1018,

		/// <summary>
		/// Регистрационный номер
		/// </summary>
		RegistrationNumber = 1037,

		/// <summary>
		/// Заводской номер ФН
		/// </summary>
		FNSerialNumber = 1041,

		/// <summary>
		/// Наименование пользователя
		/// </summary>
		UserName = 1048,

		/// <summary>
		/// Место расчётов
		/// </summary>
		RegistrationPlace = 1187,

		/// <summary>
		/// Версия ФФД
		/// </summary>
		FFDVersion = 1209,

		/// <summary>
		/// Псевдотег – признак автономного режима
		/// </summary>
		AutonomousFlag = 9001,

		/// <summary>
		/// Псевдотег – признак режима услуг
		/// </summary>
		ServiceFlag = 9002,

		/// <summary>
		/// Псевдотег – признак торговли подакцизными товарами
		/// </summary>
		ExciseFlag = 9003,

		/// <summary>
		/// Псевдотег – признак торговли маркированными товарами
		/// </summary>
		MarkingFlag = 9004,

		/// <summary>
		/// Псевдотег – признак ломбардной деятельности
		/// </summary>
		PawnFlag = 9005,

		/// <summary>
		/// Псевдотег – признак страхования
		/// </summary>
		InsuranceFlag = 9006,

		/// <summary>
		/// Псевдотег – признак лотерей
		/// </summary>
		LotteryFlag = 9007,

		/// <summary>
		/// Псевдотег – признак азартных игр
		/// </summary>
		GamblingFlag = 9008,

		/// <summary>
		/// Псевдотег – признак наличия общей системы налогообложения
		/// </summary>
		GenericTaxFlag = 9009,

		/// <summary>
		/// Псевдотег – признак наличия агентской схемы
		/// </summary>
		AgentFlag = 9010,
		}

	/// <summary>
	/// Доступные теги чеков из статуса ФН
	/// </summary>
	public enum ReceiptTags
		{
		/// <summary>
		/// Тип коррекции
		/// </summary>
		CorrectionType = 1173,

		/// <summary>
		/// Дата коррекции
		/// </summary>
		CorrectionDate = 1178,

		/// <summary>
		/// Основание коррекции
		/// </summary>
		CorrectionOrder = 1179,

		/// <summary>
		/// Наименование предмета расчёта
		/// </summary>
		GoodName = 1030,

		/// <summary>
		/// Ограничитель описаний предметов расчёта
		/// </summary>
		GoodDelimiter = 1059,

		/// <summary>
		/// Единица измерения предмета расчёта
		/// </summary>
		UnitNameMark = 2108,

		/// <summary>
		/// Количество предмета расчёта
		/// </summary>
		GoodsCount = 1023,

		/// <summary>
		/// Цена единицы предмета расчёта
		/// </summary>
		ItemCost = 1079,

		/// <summary>
		/// Стоимость предмета расчёта
		/// </summary>
		ItemResult = 1043,

		/// <summary>
		/// Признак предмета расчёта
		/// </summary>
		ResultObject = 1212,

		/// <summary>
		/// Признак способа расчёта
		/// </summary>
		ResultMethod = 1214,

		/// <summary>
		/// Ставка НДС
		/// </summary>
		NDS = 1199,

		/// <summary>
		/// Сумма наличными
		/// </summary>
		CashPaymentValue = 1031,

		/// <summary>
		/// Сумма безналичными
		/// </summary>
		ElectronicPaymentValue = 1081,

		/// <summary>
		/// Сумма авансом
		/// </summary>
		PrepaidPaymentValue = 1215,

		/// <summary>
		/// Сумма кредитом
		/// </summary>
		PostpaidPaymentValue = 1216,

		/// <summary>
		/// Сумма встречным предоставлением
		/// </summary>
		OtherPaymentValue = 1217,

		/// <summary>
		/// Сумма НДС 20%
		/// </summary>
		DocumentSummaNDS20 = 1102,

		/// <summary>
		/// Сумма НДС 10%
		/// </summary>
		DocumentSummaNDS10 = 1103,

		/// <summary>
		/// Сумма НДС 20/120
		/// </summary>
		DocumentSummaNDS120 = 1106,

		/// <summary>
		/// Сумма НДС 10/110
		/// </summary>
		DocumentSummaNDS110 = 1107,

		/// <summary>
		/// Сумма НДС 0%
		/// </summary>
		DocumentSummaNDS0 = 1104,

		/// <summary>
		/// Сумма без НДС
		/// </summary>
		DocumentSummaNoNDS = 1105,

		/// <summary>
		/// Псевдотег – сумма НДС 5%
		/// </summary>
		DocumentSummaNDS5 = 9101,

		/// <summary>
		/// Псевдотег – сумма НДС 7%
		/// </summary>
		DocumentSummaNDS7 = 9102,

		/// <summary>
		/// Псевдотег – сумма НДС 5/105
		/// </summary>
		DocumentSummaNDS105 = 9103,

		/// <summary>
		/// Псевдотег – сумма НДС 7/107
		/// </summary>
		DocumentSummaNDS107 = 9104,

		/// <summary>
		/// Номер смены
		/// </summary>
		SessionNumber = 1038,

		/// <summary>
		/// Номер документа за смену
		/// </summary>
		InSessionDocumentNumber = 1042,

		/// <summary>
		/// ФИО и должность кассира
		/// </summary>
		CashierName = 1021,
		}

	/// <summary>
	/// Класс описывает результат оценки срока жизни ФН и его применимости
	/// </summary>
	public class FNLifeResult
		{
		/// <summary>
		/// Возвращает результат оценки применимости ФН с указанными параметрами регистрации и сроком жизни
		/// </summary>
		public FNLifeStatus Status
			{
			get
				{
				return status;
				}
			}
		private FNLifeStatus status;

		/// <summary>
		/// Срок жизни ФН
		/// </summary>
		public string DeadLine
			{
			get
				{
				return deadLine.ToString ("dd.MM.yyyy");
				}
			}
		private DateTime deadLine;

		/// <summary>
		/// Конструктор. Инициализирует экземпляр результата оценки
		/// </summary>
		/// <param name="PDeadLine">Рассчитанный срок жизни ФН</param>
		/// <param name="PStatus">Результат оценки применимости</param>
		public FNLifeResult (DateTime PDeadLine, FNLifeStatus PStatus)
			{
			deadLine = PDeadLine;
			status = PStatus;
			}
		}

	/// <summary>
	/// Класс описывает вспомогательные методы
	/// </summary>
	public static class KKTSupport
		{
		/// <summary>
		/// Метод формирует дату истечения срока эксплуатации ФН с указанными параметрами
		/// </summary>
		/// <param name="StartDate">Дата фискализации</param>
		/// <param name="Flags">Параметры расчёта срока действия</param>
		/// <returns>Возвращает строку с датой;
		/// может возвращать признаки недопустимости или нежелательности применения
		/// указанной модели с заданными параметрами</returns>
		public static FNLifeResult GetFNLifeEndDate (DateTime StartDate, FNLifeFlags Flags)
			{
			// Определение недопустимых вариантов
			FNLifeStatus res = FNLifeStatus.Acceptable;
			uint length = GetFNLifeLength (Flags);

			// Нельзя игнорировать
			if (Flags.HasFlag (FNLifeFlags.GenericTax) && !Flags.HasFlag (FNLifeFlags.FN15) &&
				Flags.HasFlag (FNLifeFlags.Goods))
				{
				if (Flags.HasFlag (FNLifeFlags.FFD12FullSupport))   // Проверено
					res = FNLifeStatus.Unwelcome;
				else
					res = FNLifeStatus.Inacceptable;
				}

			// Невозможные варианты
			else if (Flags.HasFlag (FNLifeFlags.FFD12) && !Flags.HasFlag (FNLifeFlags.FFD12Compatible) ||

				!Flags.HasFlag (FNLifeFlags.Goods) && (Flags.HasFlag (FNLifeFlags.Excise) ||
				Flags.HasFlag (FNLifeFlags.MarkGoods)) ||

				Flags.HasFlag (FNLifeFlags.Goods) && Flags.HasFlag (FNLifeFlags.GamblingAndLotteries) ||

				Flags.HasFlag (FNLifeFlags.MarkGoods) && !Flags.HasFlag (FNLifeFlags.FFD12))
				{
				res = FNLifeStatus.Inacceptable;
				}

			// Определение нежелательных вариантов
			else if (!Flags.HasFlag (FNLifeFlags.GenericTax) && Flags.HasFlag (FNLifeFlags.FN15) &&
				!Flags.HasFlag (FNLifeFlags.Season) && !Flags.HasFlag (FNLifeFlags.Agents) &&
				!Flags.HasFlag (FNLifeFlags.Excise) && !Flags.HasFlag (FNLifeFlags.Autonomous) ||

				!Flags.HasFlag (FNLifeFlags.FN15) && Flags.HasFlag (FNLifeFlags.Excise) ||

				!Flags.HasFlag (FNLifeFlags.FN15) && Flags.HasFlag (FNLifeFlags.Autonomous))
				{
				res = FNLifeStatus.Unwelcome;
				}

			// Результат
			return new FNLifeResult (StartDate.AddDays (length), res);
			}

		private static uint GetFNLifeLength (FNLifeFlags Flags)
			{
			// Определение срока жизни
			uint length = 1110u;

			if ((Flags.HasFlag (FNLifeFlags.GamblingAndLotteries) || Flags.HasFlag (FNLifeFlags.PawnsAndInsurance)) &&
				Flags.HasFlag (FNLifeFlags.FFD12) && Flags.HasFlag (FNLifeFlags.FN15) ||
				Flags.HasFlag (FNLifeFlags.Excise) && Flags.HasFlag (FNLifeFlags.FFD12) ||
				!Flags.HasFlag (FNLifeFlags.FN15) && Flags.HasFlag (FNLifeFlags.Excise))    // Новое условие
				{
				length = 410u;
				}

			else if (Flags.HasFlag (FNLifeFlags.Autonomous))
				{
				if (Flags.HasFlag (FNLifeFlags.FN15) || Flags.HasFlag (FNLifeFlags.FFD12) &&
					Flags.HasFlag (FNLifeFlags.Goods) && Flags.HasFlag (FNLifeFlags.GenericTax))
					length = 410u;
				else
					length = 560u;
				}

			else if (!Flags.HasFlag (FNLifeFlags.FN15) && Flags.HasFlag (FNLifeFlags.GenericTax) &&
				(Flags.HasFlag (FNLifeFlags.Goods) || Flags.HasFlag (FNLifeFlags.Agents) &&
				Flags.HasFlag (FNLifeFlags.FFD12)))
				{
				length = 560u;
				}

			else if (Flags.HasFlag (FNLifeFlags.FN15))
				{
				length = Flags.HasFlag (FNLifeFlags.FNExactly13) ? 410u : 470u;
				}

			return length;
			}

		// Контрольная последовательность для определения корректности ИНН
		private static byte[] innCheckSequence = [3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8];

		/// <summary>
		/// Метод проверяет корректность ввода ИНН
		/// </summary>
		/// <param name="INN">ИНН для проверки</param>
		/// <returns>Возвращает 0, если ИНН корректен, 1, если ИНН имеет некорректную КС, 
		/// -1, если строка не является ИНН</returns>
		public static int CheckINN (string INN)
			{
			// Контроль параметра
			if ((INN.Length != 10) && (INN.Length != 12))
				return -1;

			UInt64 inn;
			try
				{
				inn = UInt64.Parse (INN);
				}
			catch
				{
				return -1;
				}

			// Расчёт контрольной суммы
			uint n1 = 0, n2 = 0;
			UInt64 d;

			// Для 10 цифр
			if (INN.Length == 10)
				{
				d = 10;
				for (int i = 0; i < 9; i++)
					{
					n1 += (uint)((byte)((inn / d) % 10) * innCheckSequence[10 - i]);
					d *= 10;
					}

				if ((n1 % 11) == (inn % 10))
					return 0;

				return 1;
				}

			// Для 12 цифр
			d = 100;
			for (int i = 0; i < 10; i++)
				{
				n1 += (uint)((byte)((inn / d) % 10) * innCheckSequence[10 - i]);
				d *= 10;
				}

			if ((n1 % 11) != ((inn / 10) % 10))
				return 1;

			d = 10;
			for (int i = 0; i < 11; i++)
				{
				n2 += (uint)((byte)((inn / d) % 10) * innCheckSequence[10 - i]);
				d *= 10;
				}

			if ((n2 % 11) != (inn % 10))
				return 1;

			return 0;
			}

		// Таблица полинома CRC16
		private static UInt16[] crc16_table = [
			0, 4129, 8258, 12387, 16516, 20645, 24774, 28903, 33032, 37161, 41290, 45419, 49548, 53677, 57806, 61935,
			4657, 528, 12915, 8786, 21173, 17044, 29431, 25302, 37689, 33560, 45947, 41818, 54205, 50076, 62463, 58334,
			9314, 13379, 1056, 5121, 25830, 29895, 17572, 21637, 42346, 46411, 34088, 38153, 58862, 62927, 50604, 54669,
			13907, 9842, 5649, 1584, 30423, 26358, 22165, 18100, 46939, 42874, 38681, 34616, 63455, 59390, 55197, 51132,
			18628, 22757, 26758, 30887, 2112, 6241, 10242, 14371, 51660, 55789, 59790, 63919, 35144, 39273, 43274, 47403,
			23285, 19156, 31415, 27286, 6769, 2640, 14899, 10770, 56317, 52188, 64447, 60318, 39801, 35672, 47931, 43802,
			27814, 31879, 19684, 23749, 11298, 15363, 3168, 7233, 60846, 64911, 52716, 56781, 44330, 48395, 36200, 40265,
			32407, 28342, 24277, 20212, 15891, 11826, 7761, 3696, 65439, 61374, 57309, 53244, 48923, 44858, 40793, 36728,
			37256, 33193, 45514, 41451, 53516, 49453, 61774, 57711, 4224, 161, 12482, 8419, 20484, 16421, 28742, 24679,
			33721, 37784, 41979, 46042, 49981, 54044, 58239, 62302, 689, 4752, 8947, 13010, 16949, 21012, 25207, 29270,
			46570, 42443, 38312, 34185, 62830, 58703, 54572, 50445, 13538, 9411, 5280, 1153, 29798, 25671, 21540, 17413,
			42971, 47098, 34713, 38840, 59231, 63358, 50973, 55100, 9939, 14066, 1681, 5808, 26199, 30326, 17941, 22068,
			55628, 51565, 63758, 59695, 39368, 35305, 47498, 43435, 22596, 18533, 30726, 26663, 6336, 2273, 14466, 10403,
			52093, 56156, 60223, 64286, 35833, 39896, 43963, 48026, 19061, 23124, 27191, 31254, 2801, 6864, 10931, 14994,
			64814, 60687, 56684, 52557, 48554, 44427, 40424, 36297, 31782, 27655, 23652, 19525, 15522, 11395, 7392, 3265,
			61215, 65342, 53085, 57212, 44955, 49082, 36825, 40952, 28183, 32310, 20053, 24180, 11923, 16050, 3793, 7920
			];

		/// <summary>
		/// Метод рассчитывает CRC16 для сообщения
		/// </summary>
		/// <param name="Message">Строка сообщения</param>
		/// <returns>Значение CRC16</returns>
		public static UInt16 GetCRC16 (string Message)
			{
			// Контроль
			if ((Message == null) || (Message == ""))
				return 0;
			byte[] msg = RDGenerics.GetEncoding (RDEncodings.UTF8).GetBytes (Message);

			// Переменные
			UInt16 crc16 = 0xFFFF;
			UInt16 crc16_i = 0;
			UInt16 crc16_e = (UInt16)(msg.Length - 1);

			// Вычисление
			for (; crc16_i <= crc16_e; ++crc16_i)
				{
				crc16 = (UInt16)((crc16 << 8) ^ crc16_table[(crc16 >> 8) ^ (UInt16)msg[crc16_i]]);
				}

			return crc16;
			}

		/// <summary>
		/// Метод формирует полный регистрационный номер ККТ
		/// </summary>
		/// <param name="INN">ИНН пользователя</param>
		/// <param name="RNMFirstPart">Первая часть (10 цифр) регистрационного номера ККТ 
		/// (порядковый номер в ФНС)</param>
		/// <param name="Serial">Заводской номер ККТ</param>
		/// <returns>Возвращает полный регистрационный номер (с проверочным кодом)</returns>
		public static string GetFullRNM (string INN, string Serial, string RNMFirstPart)
			{
			// Контроль параметра
			try
				{
				UInt64 serial = UInt64.Parse (Serial);
				UInt64 rnmStart = UInt64.Parse (RNMFirstPart);
				}
			catch
				{
				return "";
				}

			if ((CheckINN (INN) < 0) || (Serial.Length > 20) || (RNMFirstPart.Length > 10))
				return "";

			// Расчёт контрольной суммы
			string msg = RNMFirstPart.PadLeft (10, '0') + INN.PadLeft (12, '0') + Serial.PadLeft (20, '0');
			UInt16 crc = GetCRC16 (msg);

			return RNMFirstPart.PadLeft (10, '0') + crc.ToString ().PadLeft (6, '0');
			}

		/// <summary>
		/// Возвращает подсказку по генерации регистрационного номера ККТ
		/// </summary>
		public const string RNMTip = "Первые 10 цифр являются порядковым номером ККТ в реестре. При генерации " +
			"РНМ их можно указать вручную – остальные будут достроены программой";

		/// <summary>
		/// Метод-преобразователь символов в коды CP1251
		/// </summary>
		/// <param name="Character">Символ</param>
		/// <returns>Код символа по CP1251</returns>
		public static byte CharToCP1251 (char Character)
			{
			switch (Character)
				{
				// Поддерживаемые символы
				case ' ':
					return 32;
				case '!':
					return 33;
				case '"':
					return 34;
				case '#':
					return 35;
				case '$':
					return 36;
				case '%':
					return 37;
				case '&':
					return 38;
				case '\'':
					return 39;
				case '(':
					return 40;
				case ')':
					return 41;
				case '*':
					return 42;
				case '+':
					return 43;
				case ',':
					return 44;
				case '-':
					return 45;
				case '.':
					return 46;
				case '/':
					return 47;
				case '0':
					return 48;
				case '1':
					return 49;
				case '2':
					return 50;
				case '3':
					return 51;
				case '4':
					return 52;
				case '5':
					return 53;
				case '6':
					return 54;
				case '7':
					return 55;
				case '8':
					return 56;
				case '9':
					return 57;
				case ':':
					return 58;
				case ';':
					return 59;
				case '<':
					return 60;
				case '=':
					return 61;
				case '>':
					return 62;
				case '?':
					return 63;
				case '@':
					return 64;
				case 'A':
					return 65;
				case 'B':
					return 66;
				case 'C':
					return 67;
				case 'D':
					return 68;
				case 'E':
					return 69;
				case 'F':
					return 70;
				case 'G':
					return 71;
				case 'H':
					return 72;
				case 'I':
					return 73;
				case 'J':
					return 74;
				case 'K':
					return 75;
				case 'L':
					return 76;
				case 'M':
					return 77;
				case 'N':
					return 78;
				case 'O':
					return 79;
				case 'P':
					return 80;
				case 'Q':
					return 81;
				case 'R':
					return 82;
				case 'S':
					return 83;
				case 'T':
					return 84;
				case 'U':
					return 85;
				case 'V':
					return 86;
				case 'W':
					return 87;
				case 'X':
					return 88;
				case 'Y':
					return 89;
				case 'Z':
					return 90;
				case '[':
					return 91;
				case '\\':
					return 92;
				case ']':
					return 93;
				case '^':
					return 94;
				case '_':
					return 95;
				case '`':
					return 96;
				case 'a':
					return 97;
				case 'b':
					return 98;
				case 'c':
					return 99;
				case 'd':
					return 100;
				case 'e':
					return 101;
				case 'f':
					return 102;
				case 'g':
					return 103;
				case 'h':
					return 104;
				case 'i':
					return 105;
				case 'j':
					return 106;
				case 'k':
					return 107;
				case 'l':
					return 108;
				case 'm':
					return 109;
				case 'n':
					return 110;
				case 'o':
					return 111;
				case 'p':
					return 112;
				case 'q':
					return 113;
				case 'r':
					return 114;
				case 's':
					return 115;
				case 't':
					return 116;
				case 'u':
					return 117;
				case 'v':
					return 118;
				case 'w':
					return 119;
				case 'x':
					return 120;
				case 'y':
					return 121;
				case 'z':
					return 122;
				case '{':
					return 123;
				case '|':
					return 124;
				case '}':
					return 125;
				case '~':
					return 126;
				case '':
					return 127;
				case 'Ђ':
					return 128;
				case 'Ѓ':
					return 129;
				case '‚':
					return 130;
				case 'ѓ':
					return 131;
				case '„':
					return 132;
				case '…':
					return 133;
				case '†':
					return 134;
				case '‡':
					return 135;
				case '€':
					return 136;
				case '‰':
					return 137;
				case 'Љ':
					return 138;
				case '‹':
					return 139;
				case 'Њ':
					return 140;
				case 'Ќ':
					return 141;
				case 'Ћ':
					return 142;
				case 'Џ':
					return 143;
				case 'ђ':
					return 144;
				case '‘':
					return 145;
				case '’':
					return 146;
				case '“':
					return 147;
				case '”':
					return 148;
				case '•':
					return 149;
				case '–':
					return 150;
				case '—':
					return 151;
				case '':
					return 152;
				case '™':
					return 153;
				case 'љ':
					return 154;
				case '›':
					return 155;
				case 'њ':
					return 156;
				case 'ќ':
					return 157;
				case 'ћ':
					return 158;
				case 'џ':
					return 159;
				case ' ':
					return 160;
				case 'Ў':
					return 161;
				case 'ў':
					return 162;
				case 'Ј':
					return 163;
				case '¤':
					return 164;
				case 'Ґ':
					return 165;
				case '¦':
					return 166;
				case '§':
					return 167;
				case 'Ё':
					return 168;
				case '©':
					return 169;
				case 'Є':
					return 170;
				case '«':
					return 171;
				case '¬':
					return 172;
				case '­':
					return 173;
				case '®':
					return 174;
				case 'Ї':
					return 175;
				case '°':
					return 176;
				case '±':
					return 177;
				case 'І':
					return 178;
				case 'і':
					return 179;
				case 'ґ':
					return 180;
				case 'µ':
					return 181;
				case '¶':
					return 182;
				case '·':
					return 183;
				case 'ё':
					return 184;
				case '№':
					return 185;
				case 'є':
					return 186;
				case '»':
					return 187;
				case 'ј':
					return 188;
				case 'Ѕ':
					return 189;
				case 'ѕ':
					return 190;
				case 'ї':
					return 191;
				case 'А':
					return 192;
				case 'Б':
					return 193;
				case 'В':
					return 194;
				case 'Г':
					return 195;
				case 'Д':
					return 196;
				case 'Е':
					return 197;
				case 'Ж':
					return 198;
				case 'З':
					return 199;
				case 'И':
					return 200;
				case 'Й':
					return 201;
				case 'К':
					return 202;
				case 'Л':
					return 203;
				case 'М':
					return 204;
				case 'Н':
					return 205;
				case 'О':
					return 206;
				case 'П':
					return 207;
				case 'Р':
					return 208;
				case 'С':
					return 209;
				case 'Т':
					return 210;
				case 'У':
					return 211;
				case 'Ф':
					return 212;
				case 'Х':
					return 213;
				case 'Ц':
					return 214;
				case 'Ч':
					return 215;
				case 'Ш':
					return 216;
				case 'Щ':
					return 217;
				case 'Ъ':
					return 218;
				case 'Ы':
					return 219;
				case 'Ь':
					return 220;
				case 'Э':
					return 221;
				case 'Ю':
					return 222;
				case 'Я':
					return 223;
				case 'а':
					return 224;
				case 'б':
					return 225;
				case 'в':
					return 226;
				case 'г':
					return 227;
				case 'д':
					return 228;
				case 'е':
					return 229;
				case 'ж':
					return 230;
				case 'з':
					return 231;
				case 'и':
					return 232;
				case 'й':
					return 233;
				case 'к':
					return 234;
				case 'л':
					return 235;
				case 'м':
					return 236;
				case 'н':
					return 237;
				case 'о':
					return 238;
				case 'п':
					return 239;
				case 'р':
					return 240;
				case 'с':
					return 241;
				case 'т':
					return 242;
				case 'у':
					return 243;
				case 'ф':
					return 244;
				case 'х':
					return 245;
				case 'ц':
					return 246;
				case 'ч':
					return 247;
				case 'ш':
					return 248;
				case 'щ':
					return 249;
				case 'ъ':
					return 250;
				case 'ы':
					return 251;
				case 'ь':
					return 252;
				case 'э':
					return 253;
				case 'ю':
					return 254;
				case 'я':
					return 255;

				// Все необрабатываемые символы
				default:
					return 0;
				}
			}

		/// <summary>
		/// Возвращает число символов в строке для формата печати руководств пользователя
		/// </summary>
		public const int ManualA4CharPerLine = 99;

#if ANDROID

		/// <summary>
		/// Метод выводит на печать руководство пользователя
		/// </summary>
		/// <param name="TextForPrinting">Текст для печати</param>
		/// <param name="ForCashier">Флаг сокращённого варианта руководства (для кассира)</param>
		public static void PrintManual (string TextForPrinting, bool ForCashier)
			{
			pm.Print ("Manual.pdf", new CustomPrintDocumentAdapter (TextForPrinting, ForCashier), null);
			}

		/// <summary>
		/// Возвращает или задаёт текущий менеджер печати
		/// </summary>
		public static PrintManager ActPrintManager
			{
			get
				{
				return pm;
				}
			set
				{
				pm = value;
				}
			}
		private static PrintManager pm;

#else

		// Параметры печатного документа
		private static StringReader printStream;
		private static int charactersPerLine;      // Зависимое значение
		private static PrinterTypes internalPrinterType;
		private static uint pageNumber;
		private static bool[][] qrCodeData;
#if UMPRINT
		private static bool addManualLogo;
#endif

		private static Font printFont;             // Зависимое значение
		private static Brush printBrush = new SolidBrush (Color.FromArgb (0, 0, 0));

		/// <summary>
		/// Метод выводит печатную форму чека на печать с указанными настройками
		/// </summary>
		/// <param name="TextForPrinting">Текст ПФ</param>
		/// <param name="QRCodeData">Данные QR-кода</param>
		/// <param name="PrinterType">Тип принтера</param>
		/// <returns>Возвращает текст ошибки или null в случае успеха</returns>
		public static string PrintReceipt (string TextForPrinting, bool[][] QRCodeData, PrinterTypes PrinterType)
			{
			// Контроль
			if (string.IsNullOrWhiteSpace (TextForPrinting))
				return "No text specified";

			string txt = TextForPrinting;
			internalPrinterType = PrinterType;
			pageNumber = 0;
			qrCodeData = QRCodeData;

			// Удаление отступов (только для чековой ленты)
			int pageLength = 0;
			if (!IsA4)
				{
				if (qrCodeData == null)
					while (txt.Contains ("\n "))
						txt = txt.Replace ("\n ", "\n");
				txt = txt.Replace ("\r", "");

				// Расчёт примерной длины страницы (A4 или менее)
				string chk = txt.Replace ("\n", "");
				pageLength = 14 * (txt.Length - chk.Length + 2);
				// +1 - линия отреза
				// ~0,4 см на строку (* 3 / 10), в сотых дюйма (* 10000 / 254)

				if (pageLength > 1170)  // ~300000 / 254, А4
					pageLength = 1170;
				}

			// Создание потока и запуск диалога
			printStream = new StringReader (txt);

			PrintDocument pd = new PrintDocument ();
			pd.PrintPage += new PrintPageEventHandler (PrintPage);

			// Размер бумаги (считается в сотых долях дюйма)
			switch (internalPrinterType)
				{
				case PrinterTypes.Receipt57mm:
				case PrinterTypes.Receipt57mmThin:
					pd.DefaultPageSettings.PaperSize = new PaperSize ("Receipt57", 225, pageLength);    // 57000 / 254
					break;

				case PrinterTypes.Receipt80mm:
				case PrinterTypes.Receipt80mmThin:
					pd.DefaultPageSettings.PaperSize = new PaperSize ("Receipt80", 315, pageLength);    // 80000 / 254
					break;
				}

			PrintDialog spd = new PrintDialog ();
			spd.AllowCurrentPage = spd.AllowPrintToFile = spd.AllowSelection = spd.AllowSomePages = false;
			spd.PrintToFile = spd.ShowHelp = false;
			spd.ShowNetwork = true;
			spd.UseEXDialog = true;
			spd.Document = pd;

			if (spd.ShowDialog () == DialogResult.OK)
				{
				try
					{
					pd.PrinterSettings = spd.PrinterSettings;
					pd.Print ();
					}
				catch (Exception ex)
					{
					return ex.Message;
					}
				}

			// Успешно
			printStream.Close ();
			pd.Dispose ();
			txt = null;
			return null;
			}

		/// <summary>
		/// Метод выводит произвольный текст на печать с указанными настройками
		/// </summary>
		/// <param name="TextForPrinting">Текст для печати</param>
		/// <param name="PrinterType">Тип принтера</param>
		/// <returns>Возвращает текст ошибки или null в случае успеха</returns>
		public static string PrintText (string TextForPrinting, PrinterTypes PrinterType)
			{
			return PrintReceipt (TextForPrinting, null, PrinterType);

			/*// Контроль
			if (string.IsNullOrWhiteSpace (TextForPrinting))
				return "No text specified";

			string txt = TextForPrinting;
			internalPrinterType = PrinterType;
			pageNumber = 0;
			qrCodeData = null;

			// Удаление отступов (только для чековой ленты)
			int pageLength = 0;
			if (!IsA4)
				{
				while (txt.Contains ("\n "))
					txt = txt.Replace ("\n ", "\n");
				txt = txt.Replace ("\r", "");

				// Расчёт примерной длины страницы (A4 или менее)
				string chk = txt.Replace ("\n", "");
				pageLength = 14 * (txt.Length - chk.Length + 2);
				// +1 - линия отреза
				// ~0,4 см на строку (* 3 / 10), в сотых дюйма (* 10000 / 254)

				if (pageLength > 1170)  // ~300000 / 254, А4
					pageLength = 1170;
				}

			// Создание потока и запуск диалога
			printStream = new StringReader (txt);

			PrintDocument pd = new PrintDocument ();
			pd.PrintPage += new PrintPageEventHandler (PrintPage);

			// Размер бумаги (считается в сотых долях дюйма)
			switch (internalPrinterType)
				{
				case PrinterTypes.Receipt57mm:
				case PrinterTypes.Receipt57mmThin:
					pd.DefaultPageSettings.PaperSize = new PaperSize ("Receipt57", 225, pageLength);    // 57000 / 254
					break;

				case PrinterTypes.Receipt80mm:
				case PrinterTypes.Receipt80mmThin:
					pd.DefaultPageSettings.PaperSize = new PaperSize ("Receipt80", 315, pageLength);    // 80000 / 254
					break;
				}

			PrintDialog spd = new PrintDialog ();
			spd.AllowCurrentPage = spd.AllowPrintToFile = spd.AllowSelection = spd.AllowSomePages = false;
			spd.PrintToFile = spd.ShowHelp = false;
			spd.ShowNetwork = true;
			spd.UseEXDialog = true;
			spd.Document = pd;

			if (spd.ShowDialog () == DialogResult.OK)
				{
				try
					{
					pd.PrinterSettings = spd.PrinterSettings;
					pd.Print ();
					}
				catch (Exception ex)
					{
					return ex.Message;
					}
				}

			// Успешно
			printStream.Close ();
			pd.Dispose ();
			txt = null;
			return null;*/
			}

		/// <summary>
		/// Возвращает имя файла логотипа для добавления в руководство пользователя
		/// </summary>
		public const string ManualLogoFileName = "ManualLogo.p";

		// Обработчик событий принтера
		private static bool IsA4
			{
			get
				{
				return ((internalPrinterType == PrinterTypes.DefaultA4) ||
					(internalPrinterType == PrinterTypes.ManualA4));
				}
			}

		private static void PrintPage (object sender, PrintPageEventArgs ev)
			{
			// Инициализация
			if (printFont != null)
				printFont.Dispose ();
			bool receipt = (qrCodeData != null);

			// Вычисление числа строк на странице и параметров шрифта
			float leftMargin, topMargin, linesPerPage, yPos;
			pageNumber++;

			switch (internalPrinterType)
				{
				case PrinterTypes.DefaultA4:
				default:
					if (receipt)
						{
						printFont = new Font (fontConsolas, 80 * 6.0f / 57, FontStyle.Bold);
						charactersPerLine = receiptLineLength;
						}
					else
						{
						printFont = new Font (fontCourier, 10, FontStyle.Bold);
						charactersPerLine = 80;
						}
					leftMargin = topMargin = 70;
					linesPerPage = ev.MarginBounds.Height / printFont.GetHeight (ev.Graphics);
					break;

				case PrinterTypes.ManualA4:
					printFont = new Font (fontConsolas, 9, FontStyle.Bold);
					charactersPerLine = ManualA4CharPerLine;
					leftMargin = topMargin = 70;
					linesPerPage = ev.MarginBounds.Height / printFont.GetHeight (ev.Graphics) + 1;
					break;

				case PrinterTypes.Receipt80mm:
				case PrinterTypes.Receipt80mmThin:
					bool bold80 = (internalPrinterType == PrinterTypes.Receipt80mm);
					if (receipt)
						{
						printFont = new Font (fontConsolas, 80 * 6.0f / 57, bold80 ? FontStyle.Bold : FontStyle.Regular);
						charactersPerLine = receiptLineLength;
						}
					else
						{
						printFont = new Font (fontCourier, 7, bold80 ? FontStyle.Bold : FontStyle.Regular);
						charactersPerLine = 47;
						}
					leftMargin = topMargin = 0;
					linesPerPage = 102;
					break;

				case PrinterTypes.Receipt57mm:
				case PrinterTypes.Receipt57mmThin:
					bool bold57 = (internalPrinterType == PrinterTypes.Receipt57mm);
					if (receipt)
						{
						printFont = new Font (fontConsolas, 6.0f, bold57 ? FontStyle.Bold : FontStyle.Regular);
						charactersPerLine = receiptLineLength;
						}
					else
						{
						printFont = new Font (fontCourier, 6, bold57 ? FontStyle.Bold : FontStyle.Regular);
						charactersPerLine = 39;
						}
					leftMargin = topMargin = 0;
					linesPerPage = 118;
					break;
				}

			// Печать строк
			string line = "";
			if (pageNumber == 1)
				{
				line = "• " + ProgramDescription.AssemblyMainName + " • v " + ProgramDescription.AssemblyVersion + " •";
				line = line.PadLeft ((charactersPerLine - line.Length) / 2 + line.Length) +
					" ".PadLeft (charactersPerLine / 2);

#if UMPRINT
				if (IsA4)
					ev.Graphics.DrawImage (KassArrayDBResources.KAQR, ev.PageBounds.Width -
						leftMargin - KassArrayDBResources.KAQR.Width / 12, topMargin);

				if (addManualLogo && (internalPrinterType == PrinterTypes.ManualA4))
					{
					try
						{
						Bitmap b = (Bitmap)Image.FromFile (RDGenerics.AppStartupPath + ManualLogoFileName);
						ev.Graphics.DrawImage (b, ev.PageBounds.Width - leftMargin - b.Width / 12,
							ev.PageBounds.Height - topMargin - b.Height/12);
						b.Dispose ();
						}
					catch { }
					}
#endif
				}

			int count = 0;
			while (count < linesPerPage)
				{
				// Запрос следующей строки
				if (line == "")
					line = printStream.ReadLine ();

				// Отсечка при завершении печати
				if (line == null)
					{
					if (!IsA4)
						{
						yPos = topMargin + (count * printFont.GetHeight (ev.Graphics));
						ev.Graphics.DrawString ("_".PadLeft (charactersPerLine, '_'),
							printFont, printBrush, leftMargin, yPos, new StringFormat ());
						}

					break;
					}

				yPos = topMargin + (count * printFont.GetHeight (ev.Graphics));
				string s;
				if (line.Length > charactersPerLine)
					s = line.Substring (0, charactersPerLine);
				else
					s = line;

				if (receipt && s.StartsWith ('\x1'))
					{
					Font f = new Font (printFont.FontFamily, printFont.Size * 2, printFont.Style);
					s = s.Substring (1);
					ev.Graphics.DrawString (s, f, printBrush, leftMargin, yPos, new StringFormat ());
					f.Dispose ();
					count++;
					}
				else
					{
					ev.Graphics.DrawString (s, printFont, printBrush, leftMargin, yPos, new StringFormat ());
					}
				count++;

				if (line.Length > charactersPerLine)
					{
					// Обрезка напечатанной части
					line = line.Substring (charactersPerLine);

					// Отступ для переносимых строк (вложенность)
					if (!IsA4)
						line = "  " + line;
					}
				else
					{
					line = "";
					}
				}

			// Отрисовка QR-кода при необходимости (тоже плохо – ручной подбор размеров)
			if (receipt)
				{
				// Расчёт масштаба и смещения
				SizeF qrWidth = ev.Graphics.MeasureString ("A".PadLeft (17, 'A'), printFont);
				float pixelWidth = qrWidth.Width / qrCodeData.Length;

				SizeF qrLeftOffset = ev.Graphics.MeasureString ("A".PadLeft (24, 'A'), printFont);
				float qrLeft = qrLeftOffset.Width + leftMargin;
				float qrTop = topMargin + ((count - 8) * printFont.GetHeight (ev.Graphics));

				// Отрисовка
				for (int r = 0; r < qrCodeData.Length; r++)
					{
					for (int c = 0; c < qrCodeData[r].Length; c++)
						{
						if (qrCodeData[r][c])
							ev.Graphics.FillRectangle (printBrush, qrLeft + c * pixelWidth,
								qrTop + r * pixelWidth, pixelWidth, pixelWidth);
						}
					}
				}

			// Переход на следующую страницу при необходимости
			ev.HasMorePages = line != null;
			}

		private const string fontCourier = "Courier New";
		private const string fontConsolas = "Consolas";
		private const int receiptLineLength = 42;


#endif

#if UMPRINT || ANDROID

		/// <summary>
		/// Метод формирует руководство пользователя для ККТ
		/// </summary>
		/// <param name="Guides">Активный экземпляр оператора руководств ККТ</param>
		/// <param name="GuideNumber">Номер руководства</param>
		/// <param name="Flags">Флаги формирования руководства</param>
		/// <param name="Sections">Список разрешённых секций руководства пользователя</param>
		/// <returns>Возвращает инструкцию пользователя</returns>
		public static string BuildUserGuide (UserGuides Guides, uint GuideNumber, uint Sections,
			UserGuidesFlags Flags)
			{
			bool forCashier = Flags.HasFlag (UserGuidesFlags.GuideForCashier);
#if !ANDROID
			addManualLogo = Flags.HasFlag (UserGuidesFlags.AddManualLogo);
#endif

			string text = "Инструкция к ККТ " + Guides.GetKKTList ()[(int)GuideNumber] +
				" (" + (forCashier ? "для кассиров" : "полная") + ")";
			text = text.PadLeft ((ManualA4CharPerLine - text.Length) / 2 + text.Length);

			string tmp = UserGuides.UserManualsTip;
			tmp = tmp.PadLeft ((ManualA4CharPerLine - tmp.Length) / 2 + tmp.Length);
			text += (RDLocale.RN + tmp);

			string[] operations = UserGuides.OperationTypes (false);
			uint operationsCount = forCashier ? (uint)UserGuides.OperationTypes (true).Length :
				(uint)operations.Length;

			for (int i = 0; i < operationsCount; i++)
				{
				if ((Sections & (1ul << i)) == 0)
					continue;

				text += ((i != 0 ? RDLocale.RN : "") + RDLocale.RNRN + operations[i] + RDLocale.RNRN);
				text += Guides.GetGuide (GuideNumber, (UserGuidesTypes)i, Flags);
				}

			return text;
			}

#endif

#if !ANDROID

		/// <summary>
		/// Возвращает имя настройки, хранящей флаг переопределения действия кнопки закрытия окна
		/// </summary>
		public const string OverrideCloseButtonPar = "OCB";

		/// <summary>
		/// Метод получает значение тега регистрации из последнего считанного статуса
		/// </summary>
		/// <param name="TagNumber">Номер тега</param>
		/// <param name="Initial">Первичная загрузка данных текущего чека</param>
		/// <returns>Возвращает значение или пустую строку, если статус не был запрошен,
		/// или ФН не содержит регистраций</returns>
		public static string GetRegTagValue (RegTags TagNumber, bool Initial)
			{
			// Попытка чтения
			if (!Initial)
				goto skipRegFile;

			try
				{
				regTagsFile = File.ReadAllText (statusesDirectory + "\\Registration.fsr",
					RDGenerics.GetEncoding (RDEncodings.CP1251));
				}
			catch
				{
				return "";
				}

			// Попытка извлечения
			skipRegFile:
			string signature = "[" + ((uint)TagNumber).ToString () + "] ";
			int left;
			switch (TagNumber)
				{
				case RegTags.FNSerialNumber:
				case RegTags.KKTSerialNumber:
				case RegTags.RegistrationNumber:
				case RegTags.UserINN:
					left = regTagsFile.IndexOf (signature);
					break;

				default:
					left = regTagsFile.LastIndexOf (signature);
					break;
				}

			if (left < 0)
				return "";
			left += signature.Length;

			int right = regTagsFile.IndexOf (RDLocale.RN, left);
			if (right < 0)
				return "";

			// Успешно
			string res = regTagsFile.Substring (left, right - left);
			return res./*Replace ("\r", "").*/Trim ();
			}
		private static string regTagsFile;

		/// <summary>
		/// Метод получает первое значение тега чека из последнего считанного документа
		/// </summary>
		/// <param name="TagNumber">Номер тега</param>
		/// <returns>Возвращает значение или пустую строку, если чек не был запрошен</returns>
		public static string GetReceiptTagValue (ReceiptTags TagNumber)
			{
			string[] values = GetReceiptTagValues (TagNumber, false);
			if (values.Length < 1)
				return "";

			return values[0];
			}

		/// <summary>
		/// Метод получает список значений тега чека из последнего считанного документа
		/// </summary>
		/// <param name="TagNumber">Номер тега</param>
		/// <returns>Возвращает список значений или пустой массив, если чек не был запрошен</returns>
		public static string[] GetReceiptTagValues (ReceiptTags TagNumber)
			{
			return GetReceiptTagValues (TagNumber, false);
			}

		private static string[] GetReceiptTagValues (ReceiptTags TagNumber, bool Initial)
			{
			// Попытка чтения
			if (!Initial)
				goto findTag;

			receiptTags.Clear ();
			receiptTagsValues.Clear ();
			receiptOffsets.Clear ();

			string file;
			try
				{
				file = File.ReadAllText (statusesDirectory + "\\Receipt.fsr",
					RDGenerics.GetEncoding (RDEncodings.CP1251));
				}
			catch
				{
				return [];
				}

			// Извлечение полного состава тегов
			string[] values = file.Split (['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < values.Length; i++)
				{
				if (values[i].Length < 8)
					continue;

				uint v;
				try
					{
					v = uint.Parse (values[i].Substring (1, 4));
					}
				catch
					{
					continue;
					}

				receiptTags.Add ((ReceiptTags)v);
				receiptTagsValues.Add (values[i].Substring (7).Trim ());
				if ((ReceiptTags)v == ReceiptTags.GoodDelimiter)
					receiptOffsets.Add (i);
				}

			// Определение типа целевого тега
			findTag:
			bool multiple;
			switch (TagNumber)
				{
				case ReceiptTags.GoodsCount:
				case ReceiptTags.GoodName:
				case ReceiptTags.ItemCost:
				case ReceiptTags.ItemResult:
				case ReceiptTags.NDS:
				case ReceiptTags.ResultMethod:
				case ReceiptTags.ResultObject:
				case ReceiptTags.UnitNameMark:
					multiple = true;
					break;

				default:
					multiple = false;
					break;
				}

			// Одиночные теги
			int idx;
			if (!multiple)
				{
				idx = receiptTags.IndexOf (TagNumber);
				if (idx < 0)
					return [];

				return [receiptTagsValues[idx]];
				}

			// Множественные теги
			List<string> res = [];
			for (int i = 0; i < receiptOffsets.Count; i += 2)
				{
				idx = receiptTags.IndexOf (TagNumber, receiptOffsets[i], receiptOffsets[i + 1] - receiptOffsets[i]);
				if (idx < 0)
					res.Add ("");
				else
					res.Add (receiptTagsValues[idx]);
				}

			return res.ToArray ();
			}
		private static List<ReceiptTags> receiptTags = [];
		private static List<string> receiptTagsValues = [];
		private static List<int> receiptOffsets = [];

		/// <summary>
		/// Возвращает количество предметов расчёта в последнем считанном чеке
		/// </summary>
		public static uint ObjectsCountInReceipt
			{
			get
				{
				_ = GetReceiptTagValues (ReceiptTags.GoodDelimiter, true);
				return (uint)receiptOffsets.Count / 2;
				}
			}

		/// <summary>
		/// Метод формирует путь к файлу статуса, включая в него уникальный идентификатор
		/// </summary>
		/// <param name="UID">Идентификатор статуса (обычно это ЗН ФН).
		/// Если null, метод возвращает путь к стандартному файлу хранения текущего считанного статуса</param>
		/// <returns></returns>
		public static string CreateStatusFileName (string UID)
			{
			// Контроль директории
			if (!Directory.Exists (statusesDirectory))
				try
					{
					Directory.CreateDirectory (statusesDirectory);
					}
				catch
					{
					return null;
					}

			// Формирование имени
			return statusesDirectory + "\\" + (string.IsNullOrWhiteSpace (UID) ? "KassArrayStatus" : UID)
				+ ".fss";
			}

		/// <summary>
		/// Возвращает путь к сформированным файлам статусов
		/// </summary>
		public static string StatusesDirectory
			{
			get
				{
				return statusesDirectory;
				}
			}
		private static string statusesDirectory = RDGenerics.AppStartupPath + "CachedStatuses";

#endif

		/// <summary>
		/// Метод получает поисковый критерий в зависимости от условий, с которыми он был запрошен
		/// </summary>
		/// <param name="ButtonName">Имя кнопки, с помощью которой вызван метод:
		/// - при наличии в имени слова Next метод использует заданный ранее критерий;
		/// - при наличии в имени слова Buffer метод извлечёт критерий из буфера обмена;
		/// - в остальных случаях будет отображено окно ввода критерия</param>
		/// <param name="OldCriteria">Старое значение критерия</param>
		/// <param name="InputCaption">Комментарий к окну ввода критерия</param>
		/// <param name="MaxInputLength">Максимальная длина строки критерия</param>
		/// <returns>Возвращает две строки:
		/// - строка с новым поисковым критерием
		/// - строка результатов обработки:
		///   I - необходимо увеличить поисковое смещение
		///   Z - необходимо обнулить поисковое смещение
		///   C - ввод был отменён</returns>
		public static
#if ANDROID
			async Task<string[]>
#else
			string[]
#endif
			ObtainSearchCriteria (string ButtonName, string OldCriteria, string InputCaption, uint MaxInputLength)
			{
			string search;
			string[] res = ["", ""];

			if (ButtonName.Contains ("Next"))
				{
				search = res[0] = OldCriteria;
				res[1] = "I";
				}

			else if (ButtonName.Contains ("Buffer"))
				{
#if ANDROID
				search = await RDGenerics.GetFromClipboard ();
#else
				search = RDGenerics.GetFromClipboard ();
#endif

				if (search.Length > MaxInputLength)
					search = search.Substring (0, (int)MaxInputLength);
				if (!string.IsNullOrWhiteSpace (search))
					{
					res[0] = search;
					res[1] = "I";
					}
				else
					{
					res[0] = OldCriteria;
					res[1] = "C";
					}
				}

			else
				{
#if ANDROID
				search = await RDInterface.ShowInput ("Поиск", InputCaption,
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Find),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel),
					MaxInputLength, Keyboard.Default, OldCriteria);
#else
				search = RDInterface.MessageBox (InputCaption, true, MaxInputLength, OldCriteria);
#endif

				if (!string.IsNullOrWhiteSpace (search))
					{
					res[0] = search;
					res[1] = "Z";
					}
				else
					{
					res[0] = OldCriteria;
					res[1] = "C";
					}
				}

			return res;
			}

		}

#if ANDROID

	/// <summary>
	/// Класс описывает сборщик задания на печать
	/// </summary>
	public class CustomPrintDocumentAdapter: PrintDocumentAdapter
		{
		// Переменные
		private PrintedPdfDocument document;
		private int pagesCount;
		private int pageNumber;
		private Android.Graphics.Paint p;
		private StringReader printStream;
		private string text, name;

		private float leftMargin = 60;
		private float topMargin = 60;
		private float linesPerPage = 67;
		private float lineHeight = 11.0f;
		private float fontSize = 9.0f;

		/// <summary>
		/// Конструктор. Создаёт сборщик задания
		/// </summary>
		/// <param name="ForCashier">Вариант задания для кассира (сокращённый)</param>
		/// <param name="Text">Текст для печати</param>
		public CustomPrintDocumentAdapter (string Text, bool ForCashier)
			{
			pagesCount = ForCashier ? 1 : 2;
			text = Text;

			int i = text.IndexOf ('\n');
			if (i >= 0)
				name = text.Substring (0, i).Trim ();
			else
				name = "Руководство";
			name += ".pdf";
			}

		/// <summary>
		/// Метод-обработчик вызова функции печати
		/// </summary>
		public override void OnLayout (PrintAttributes oldAttributes, PrintAttributes newAttributes,
			CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
			{
			// Инициализация документа
			document = new PrintedPdfDocument (Android.App.Application.Context, newAttributes);

			PrintDocumentInfo.Builder pdb = new PrintDocumentInfo.Builder (name);
			pdb.SetContentType (PrintContentType.Document);
			pdb.SetPageCount (pagesCount);

			// Настройка кисти
			p = new Android.Graphics.Paint ();
			p.Color = Android.Graphics.Color.Black;
			p.SetStyle (Android.Graphics.Paint.Style.Fill);
			p.StrokeWidth = 1.0f;
			p.Dither = false;
			p.SetTypeface (Typeface.Monospace);
			p.TextSize = fontSize;

			// Завершение
			PrintDocumentInfo pdi = pdb.Build ();
			callback.OnLayoutFinished (pdi, true);
			}

		/// <summary>
		/// Метод-сборщик задания на печать
		/// </summary>
		public override void OnWrite (PageRange[] pages, ParcelFileDescriptor destination,
			CancellationSignal cancellationSignal, WriteResultCallback callback)
			{
			// Формирование страниц
			printStream = new StringReader (text);
			for (pageNumber = 0; pageNumber < pagesCount; pageNumber++)
				{
				PrintedPdfDocument.Page page = document.StartPage (pageNumber);
				BuildPage (page.Canvas);
				document.FinishPage (page);
				}

			// Запись файла PDF
			Java.IO.FileOutputStream javaStream = new Java.IO.FileOutputStream (destination.FileDescriptor);
			OutputStreamInvoker osi = new OutputStreamInvoker (javaStream);

			document.WriteTo (osi);

			osi.Close ();

			// Завершено
			printStream.Close ();
			printStream.Dispose ();

			callback.OnWriteFinished (pages);
			}

		// Сборщик страницы
		private void BuildPage (Canvas g)
			{
			// Вычисление числа строк на странице и параметров шрифта
			float yPos;

			// Печать строк
			string line = "";
			if (pageNumber == 0)
				{
				line = "• " + ProgramDescription.AssemblyMainName + " • v " + ProgramDescription.AssemblyVersion + " •";
				line = line.PadLeft ((KKTSupport.ManualA4CharPerLine - line.Length) / 2 + line.Length) +
					" ".PadLeft (KKTSupport.ManualA4CharPerLine / 2);
				}

			int count = 0;
			while (count < linesPerPage)
				{
				if (line == "")
					line = printStream.ReadLine ();
				if (line == null)
					break;

				yPos = topMargin + count * lineHeight;
				g.DrawText ((line.Length > KKTSupport.ManualA4CharPerLine) ?
					line.Substring (0, KKTSupport.ManualA4CharPerLine) : line, leftMargin, yPos, p);
				count++;

				if (line.Length > KKTSupport.ManualA4CharPerLine)
					// Обрезка напечатанной части
					line = line.Substring (KKTSupport.ManualA4CharPerLine);
				else
					line = "";
				}

			// Завершено
			}
		}

#endif
	}
