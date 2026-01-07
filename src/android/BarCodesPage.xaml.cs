using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу расшифровки штрих-кодов
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class BarCodesPage: ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public BarCodesPage ()
			{
			InitializeComponent ();
			}
		}
	}
