using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу сведений о программе
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class AboutPage: ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public AboutPage ()
			{
			InitializeComponent ();
			}
		}
	}
