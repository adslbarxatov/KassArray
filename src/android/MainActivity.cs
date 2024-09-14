using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Print;
using Android.Views;
using AndroidX.Core.App;

namespace RD_AAOW
	{
	[Activity (Label = "KassArray",
		Icon = "@drawable/icon",
		Theme = "@style/SplashTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity: MauiAppCompatActivity
		{
		/// <summary>
		/// Принудительная установка масштаба шрифта
		/// </summary>
		/// <param name="base">Существующий набор параметров</param>
		protected override void AttachBaseContext (Context @base)
			{
			if (baseContextOverriden)
				{
				base.AttachBaseContext (@base);
				return;
				}

			originalContext = @base;
			Android.Content.Res.Configuration overrideConfiguration = new Android.Content.Res.Configuration ();
			overrideConfiguration = @base.Resources.Configuration;
			overrideConfiguration.FontScale = 0.9f;

			Context context = @base.CreateConfigurationContext (overrideConfiguration);
			baseContextOverriden = true;

			base.AttachBaseContext (context);
			}
		private bool baseContextOverriden = false;
		private Context originalContext;

		/// <summary>
		/// Обработчик события создания экземпляра
		/// </summary>
		protected override void OnCreate (Bundle savedInstanceState)
			{
			// Отмена темы для splash screen
			base.SetTheme (Microsoft.Maui.Controls.Resource.Style.MainTheme);

#if FORE_SVC
			// Получение списка доступных прав
			RDAppStartupFlags flags = AndroidSupport.GetAppStartupFlags (RDAppStartupFlags.CanShowNotifications |
				RDAppStartupFlags.Huawei);

			// Запуск независимо от разрешения
			if (mainService == null)
				{
				mainService = new Intent (this, typeof (MainService));
				mainService.SetPackage (this.PackageName);
				}
			AndroidSupport.StopRequested = false;

			// Для Android 12 и выше запуск службы возможен только здесь
			if (flags.HasFlag (RDAppStartupFlags.CanShowNotifications))
				{
				if (AndroidSupport.IsForegroundAvailable)
					StartForegroundService (mainService);
				else
					StartService (mainService);
				}
#endif

			// Запрет на переход в ждущий режим
			this.Window.AddFlags (WindowManagerFlags.KeepScreenOn);

			// Сохранение менеджера печати
			KKTSupport.ActPrintManager = (PrintManager)originalContext.GetSystemService (Service.PrintService);

			// Инициализация и запуск
			base.OnCreate (savedInstanceState);
			}
		}
	}
