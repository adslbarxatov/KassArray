using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/*/// <summary>
	/// Класс обеспечивает преобразование таблицы в пригодный для импорта в программу вид
	/// </summary>
	public static class TablesInterpreter
		{
		/// <summary>
		/// Запускает трансляцию выбранной таблицы
		/// </summary>
		public static int Interpretate ()
			{
			FileStream FI = null, FO = null;
			List<int> codes = new List<int> ();
			for (int i = 0; i < 256; i++)
				codes.Add (-1);

			string kkm = "Орион100Ф";

			try
				{
				FI = new FileStream (kkm + ".txt", FileMode.Open);
				}
			catch
				{
				MessageBox.Show ("Не удалось открыть входной файл", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return -1;
				}

			try
				{
				FO = new FileStream (kkm + ".dat", FileMode.Create);
				}
			catch
				{
				MessageBox.Show ("Не удалось создать выходной файл", ProgramDescription.AssemblyTitle,
					MessageBoxButtons.OK, MessageBoxIcon.Error);
				return -2;
				}

			Encoding enc = Encoding.GetEncoding (1251);
			StreamReader SR = new StreamReader (FI, enc);
			StreamWriter SW = new StreamWriter (FO, enc);

			string line;
			SW.WriteLine (kkm);
			while ((line = SR.ReadLine ()) != null)
				{
				try
					{
					byte[] b = enc.GetBytes (line.ToCharArray (), 0, 1);
					codes[b[0]] = byte.Parse (line.Substring (1));
					}
				catch
					{
					MessageBox.Show ("Сбой интерпретации строки", ProgramDescription.AssemblyTitle,
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}

			SR.Close ();
			FI.Close ();

			for (int i = 0; i < codes.Count; i++)
				SW.WriteLine (codes[i]);

			SW.WriteLine ("D2");
			SW.WriteLine ("16 символов в строке");

			SW.Close ();
			FO.Close ();
			return 0;
			}
		}*/
	}
