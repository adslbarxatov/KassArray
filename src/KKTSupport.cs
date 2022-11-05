using System;
using System.IO;
using System.Text;

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
	/// Класс описывает вспомогательные методы
	/// </summary>
	public static class KKTSupport
		{
		/// <summary>
		/// Структура используется для передачи списка параметров определения срока жизни ФН
		/// </summary>
		public struct FNLifeFlags
			{
			/// <summary>
			/// Флаг указывает на выбор ФН на 13/15 месяцев вместо 36
			/// </summary>
			public bool FN15;

			/// <summary>
			/// Флаг указывает на выбор ФН на 13 месяцев вместо 15
			/// </summary>
			public bool FNExactly13;

			/// <summary>
			/// Флаг указыввает на применение ОСН
			/// </summary>
			public bool GenericTax;

			/// <summary>
			/// Флаг указывает на режим товаров вместо услуг
			/// </summary>
			public bool Goods;

			/// <summary>
			/// Флаг указывает на агентскую схему или сезонный режим работы
			/// </summary>
			public bool SeasonOrAgents;

			/// <summary>
			/// Флаг указывает на наличие подакцизных товаров
			/// </summary>
			public bool Excise;

			/// <summary>
			/// Флаг указывает на работу без передачи данных
			/// </summary>
			public bool Autonomous;

			/// <summary>
			/// Флаг ФФД 1.2
			/// </summary>
			public bool FFD12;

			/// <summary>
			/// Флаг маркировочного ФН
			/// </summary>
			public bool MarkFN;
			}

		/// <summary>
		/// Метод формирует дату истечения срока эксплуатации ФН с указанными параметрами
		/// </summary>
		/// <param name="StartDate">Дата фискализации</param>
		/// <param name="Flags">Параметры расчёта срока действия</param>
		/// <returns>Возвращает строку с датой или пустую строку, если указанная модель ФН
		/// не может быть использована с указанными режимами и параметрами</returns>
		public static string GetFNLifeEndDate (DateTime StartDate, FNLifeFlags Flags)
			{
			string res = "";

			// Отсечение недопустимых вариантов
			if (Flags.GenericTax && !Flags.FN15 && Flags.Goods ||               // Нельзя игнорировать

				Flags.FFD12 && !Flags.GenericTax && Flags.FN15 &&
				!Flags.SeasonOrAgents && !Flags.Excise && !Flags.Autonomous ||

				Flags.FFD12 && !Flags.MarkFN)
				{
				res = "!";
				}

			// Определение срока жизни
			int length = 1110;

			if (Flags.Excise && Flags.FFD12)
				{
				length = 410;
				}
			else if (Flags.Autonomous)
				{
				if (Flags.FN15)
					length = 410;
				else
					length = 560;
				}
			else if (!Flags.FN15 && Flags.GenericTax && Flags.Goods)
				{
				length = 560;   // Нововведение ФН-М 36
				}
			else if (Flags.FN15)
				{
				length = Flags.FNExactly13 ? 410 : 470;
				}

			// Результат
			return res + StartDate.AddDays (length).ToString ("dd.MM.yyyy");
			}

		// Контрольная последовательность для определения корректности ИНН
		private static byte[] innCheckSequence = new byte[] { 3, 7, 2, 4, 10, 3, 5, 9, 4, 6, 8 };

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

			UInt64 inn = 0;
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
		private static UInt16[] crc16_table = new UInt16[] {
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
			};

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
			byte[] msg = Encoding.UTF8.GetBytes (Message);

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

		/*/// <summary>
		/// Метод возвращает название региона по коду ИНН
		/// </summary>
		/// <param name="INN">ИНН пользователя</param>
		/// <returns>Возвращает название региона</returns>
		public static string GetRegionName (string INN)
			{
			// Контроль
			if (INN.Length < 2)
				return "неизвестный регион";

			// Обработка
			switch (INN.Substring (0, 2))
				{
				case "01":
					return "Республика Адыгея";
				case "02":
					return "Республика Башкортостан";
				case "03":
					return "Республика Бурятия";
				case "04":
					return "Республика Алтай";
				case "05":
					return "Республика Дагестан";
				case "06":
					return "Республика Ингушетия";
				case "07":
					return "Кабардино-Балкарская республика";
				case "08":
					return "Республика Калмыкия";
				case "09":
					return "Республика Карачаево-Черкесия";
				case "10":
					return "Республика Карелия";
				case "11":
					return "Республика Коми";
				case "12":
					return "Республика Марий Эл";
				case "13":
					return "Республика Мордовия";
				case "14":
					return "Республика Саха (Якутия)";
				case "15":
					return "Республика Северная Осетия – Алания";
				case "16":
					return "Республика Татарстан";
				case "17":
					return "Республика Тыва";
				case "18":
					return "Удмуртская республика";
				case "19":
					return "Республика Хакасия";
				case "20":
					return "Чеченская республика";
				case "21":
					return "Чувашская республика";
				case "22":
					return "Алтайский край";
				case "23":
					return "Краснодарский край";
				case "24":
				case "84":
				case "88":
					return "Красноярский край";
				case "25":
					return "Приморский край";
				case "26":
					return "Ставропольский край";
				case "27":
					return "Хабаровский край";
				case "28":
					return "Амурская область";
				case "29":
					return "Архангельская область";
				case "30":
					return "Астраханская область";
				case "31":
					return "Белгородская область";
				case "32":
					return "Брянская область";
				case "33":
					return "Владимирская область";
				case "34":
					return "Волгоградская область";
				case "35":
					return "Вологодская область";
				case "36":
					return "Воронежская область";
				case "37":
					return "Ивановская область";
				case "38":
				case "85":
					return "Иркутская область";
				case "39":
					return "Калининградская область";
				case "40":
					return "Калужская область";
				case "41":
				case "82":
					return "Камчатский край";
				case "42":
					return "Кемеровская область";
				case "43":
					return "Кировская область";
				case "44":
					return "Костромская область";
				case "45":
					return "Курганская область";
				case "46":
					return "Курская область";
				case "47":
					return "Ленинградская область";
				case "48":
					return "Липецкая область";
				case "49":
					return "Магаданская область";
				case "50":
					return "Московская область";
				case "51":
					return "Мурманская область";
				case "52":
					return "Нижегородская область";
				case "53":
					return "Новгородская область";
				case "54":
					return "Новосибирская область";
				case "55":
					return "Омская область";
				case "56":
					return "Оренбургская область";
				case "57":
					return "Орловская область";
				case "58":
					return "Пензенская область";
				case "59":
				case "81":
					return "Пермский край";
				case "60":
					return "Псковская область";
				case "61":
					return "Ростовская область";
				case "62":
					return "Рязанская область";
				case "63":
					return "Самарская область";
				case "64":
					return "Саратовская область";
				case "65":
					return "Сахалинская область";
				case "66":
					return "Свердловская область";
				case "67":
					return "Смоленская область";
				case "68":
					return "Тамбовская область";
				case "69":
					return "Тверская область";
				case "70":
					return "Томская область";
				case "71":
					return "Тульская область";
				case "72":
					return "Тюменская область";
				case "73":
					return "Ульяновская область";
				case "74":
					return "Челябинская область";
				case "75":
				case "80":
					return "Забайкальский край";
				case "76":
					return "Ярославская область";
				case "77":
				case "97":
					return "Москва";
				case "78":
					return "Санкт-Петербург";
				case "79":
					return "Еврейская автономная область";
				case "91":
					return "Республика Крым";
				case "83":
					return "Ненецкий автономный округ";
				case "86":
					return "Ханты-Мансийский автономный округ – Югра";
				case "87":
					return "Чукотский автономный округ";
				case "89":
					return "Ямало-Ненецкий автономный округ";
				case "92":
					return "Севастополь";
				case "99":
					return "Байконур";

				default:
					return "неизвестный регион";
				}
			}*/

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
		/// <param name="PrintingManager">Дескриптор сервиса управления печатью</param>
		/// <param name="ForCashier">Флаг сокращённого варианта руководства (для кассира)</param>
		public static void PrintManual (string TextForPrinting, PrintManager PrintingManager, bool ForCashier)
			{
			PrintingManager.Print ("Manual.pdf", new CustomPrintDocumentAdapter (TextForPrinting, ForCashier), null);
			}

#else

		// Параметры печатного документа
		private static StringReader printStream;
		private static int charactersPerLine;      // Зависимое значение
		private static PrinterTypes internalPrinterType;
		private static uint pageNumber;

		private static Font printFont;             // Зависимое значение
		private static Brush printBrush = new SolidBrush (Color.FromArgb (0, 0, 0));

		/// <summary>
		/// Метод выводит произвольный текст на печать с указанными настройками
		/// </summary>
		/// <param name="TextForPrinting">Текст для печати</param>
		/// <param name="PrinterType">Пользовательский тип принтера</param>
		/// <returns>Возвращает текст ошибки или null в случае успеха</returns>
		public static string PrintText (string TextForPrinting, PrinterTypes PrinterType)
			{
			// Контроль
			if (string.IsNullOrWhiteSpace (TextForPrinting))
				return "No text specified";
			string txt = TextForPrinting;
			internalPrinterType = PrinterType;
			pageNumber = 0;

			// Удаление отступов (только для чековой ленты)
			int pageLength = 0;
			if ((internalPrinterType != PrinterTypes.DefaultA4) && (internalPrinterType != PrinterTypes.ManualA4))
				{
				while (txt.Contains ("\n "))
					txt = txt.Replace ("\n ", "\n");

				// Расчёт примерной длины страницы (A4 или менее)
				string chk = txt.Replace ("\n", "");
				pageLength = 14 * (txt.Length - chk.Length); // ~0,4 см на строку (* 3 / 10), в сотых дюйма (* 10000 / 254)
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

		// Обработчик событий принтера
		private static void PrintPage (object sender, PrintPageEventArgs ev)
			{
			// Инициализация
			if (printFont != null)
				printFont.Dispose ();

			// Вычисление числа строк на странице и параметров шрифта
			float leftMargin, topMargin, linesPerPage, yPos;
			pageNumber++;

			switch (internalPrinterType)
				{
				case PrinterTypes.DefaultA4:
				default:
					printFont = new Font ("Courier New", 10, FontStyle.Bold);
					charactersPerLine = 80;
					leftMargin = topMargin = 70;
					linesPerPage = ev.MarginBounds.Height / printFont.GetHeight (ev.Graphics);
					break;

				case PrinterTypes.ManualA4:
					printFont = new Font ("Consolas", 9, FontStyle.Bold);
					charactersPerLine = ManualA4CharPerLine;
					leftMargin = topMargin = 70;
					linesPerPage = ev.MarginBounds.Height / printFont.GetHeight (ev.Graphics);
					break;

				case PrinterTypes.Receipt80mm:
				case PrinterTypes.Receipt80mmThin:
					printFont = new Font ("Courier New", 7,
						(internalPrinterType == PrinterTypes.Receipt80mm) ? FontStyle.Bold : FontStyle.Regular);
					charactersPerLine = 47;
					leftMargin = topMargin = 0;
					linesPerPage = 102;
					break;

				case PrinterTypes.Receipt57mm:
				case PrinterTypes.Receipt57mmThin:
					printFont = new Font ("Courier New", 6,
						(internalPrinterType == PrinterTypes.Receipt57mm) ? FontStyle.Bold : FontStyle.Regular);
					charactersPerLine = 39;
					leftMargin = topMargin = 0;
					linesPerPage = 118;
					break;
				}

			// Печать строк
			string line = "";
			if (pageNumber == 1)
				{
				line = "• " + ProgramDescription.AssemblyMainName + " • v " + ProgramDescription.AssemblyVersion + " •";
				line = line.PadLeft ((charactersPerLine - line.Length) / 2 + line.Length) + " ".PadLeft (charactersPerLine / 2);
				}

			int count = 0;
			while (count < linesPerPage)
				{
				if (line == "")
					line = printStream.ReadLine ();
				if (line == null)
					break;

				yPos = topMargin + (count * printFont.GetHeight (ev.Graphics));
				ev.Graphics.DrawString ((line.Length > charactersPerLine) ? line.Substring (0, charactersPerLine) : line,
					printFont, printBrush, leftMargin, yPos, new StringFormat ());
				count++;

				if (line.Length > charactersPerLine)
					{
					// Обрезка напечатанной части
					line = line.Substring (charactersPerLine);

					// Отступ для переносимых строк (вложенность)
					if ((internalPrinterType != PrinterTypes.DefaultA4) && (internalPrinterType != PrinterTypes.ManualA4))
						line = "  " + line;
					}
				else
					{
					line = "";
					}
				}

			// Переход на следующую страницу при необходимости
			ev.HasMorePages = line != null;
			}


#endif

#if UMPRINT || ANDROID

		/// <summary>
		/// Метод формирует руководство пользователя для ККТ
		/// </summary>
		/// <param name="Manuals">Активный экземпляр оператора руководств ККТ</param>
		/// <param name="ManualNumber">Номер руководства</param>
		/// <param name="ForCashier">Флаг указывает на сокращённый вариант руководства (для кассира)</param>
		/// <returns>Возвращает инструкцию пользователя</returns>
		public static string BuildUserManual (UserManuals Manuals, uint ManualNumber, bool ForCashier)
			{
			string text = "Инструкция к ККТ " + Manuals.GetKKTList ()[(int)ManualNumber] +
				" (" + (ForCashier ? "для кассиров" : "полная") + ")";
			text = text.PadLeft ((ManualA4CharPerLine - text.Length) / 2 + text.Length);

			string tmp = "(<> – индикация на дисплее, [] – кнопки клавиатуры)";
			tmp = tmp.PadLeft ((ManualA4CharPerLine - tmp.Length) / 2 + tmp.Length);
			text += ("\n" + tmp);

			string[] operations = Manuals.OperationTypes;
			uint operationsCount = ForCashier ? UserManuals.OperationsForCashiers :
				(uint)operations.Length;

			for (int i = 0; i < operationsCount; i++)
				{
				text += ((i != 0 ? "\n" : "") + "\n\n" + operations[i] + "\n\n");
				text += Manuals.GetManual (ManualNumber, (uint)i);
				}

			return text;
			}

#endif
		}

#if ANDROID

	/// <summary>
	/// Класс описывает сборщик задания на печать
	/// </summary>
	public class CustomPrintDocumentAdapter:PrintDocumentAdapter
		{
		// Переменные
		private PrintedPdfDocument document;
		private int pagesCount;
		private int pageNumber;
		private Paint p;
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

			int i = text.IndexOf ("\n");
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
			p = new Paint ();
			p.Color = Color.Black;
			p.SetStyle (Paint.Style.Fill);
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
			javaStream.Close ();

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
