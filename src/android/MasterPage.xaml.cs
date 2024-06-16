namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главный макет приложения
	/// </summary>
	[XamlCompilation (XamlCompilationOptions.Compile)]
	public partial class MasterPage: NavigationPage
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
