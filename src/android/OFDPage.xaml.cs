using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает страницу параметров ОФД
	/// </summary>
 	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class OFDPage:ContentPage
		{
		/// <summary>
		/// Конструктор. Запускает страницу
		/// </summary>
		public OFDPage ()
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
