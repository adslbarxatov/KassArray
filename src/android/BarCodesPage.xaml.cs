using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу преобразования текста в коды ККТ
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class BarCodesPage:ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public BarCodesPage ()
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
