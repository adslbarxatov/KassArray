using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу настроек программы
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class SettingsPage: ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public SettingsPage ()
			{
			InitializeComponent ();
			}
		}
	}
