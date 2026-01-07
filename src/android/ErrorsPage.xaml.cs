using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу расшифровок ошибок ККТ
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class ErrorsPage: ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public ErrorsPage ()
			{
			InitializeComponent ();
			}
		}
	}
