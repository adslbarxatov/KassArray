using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу преобразования систем счисления и символов Unicode
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class ConvertorsPage:ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public ConvertorsPage ()
			{
			InitializeComponent ();
			}

		/// <summary>
		/// Переопределение для кнопки возврата
		/// </summary>
		protected override bool OnBackButtonPressed ()
			{
			App app = (App)App.Current;
			app.CallHeadersPage ();

			return true;
			}
		}
	}
