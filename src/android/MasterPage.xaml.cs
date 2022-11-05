using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главный макет приложения
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class MasterPage:CarouselPage
		{
		/// <summary>
		/// Конструктор. Создаёт макет приложения
		/// </summary>
		public MasterPage ()
			{
			InitializeComponent ();
			}
		}
	}
