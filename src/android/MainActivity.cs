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
		Icon = "@drawable/launcher_foreground",
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

			// Запрет на переход в ждущий режим
			this.Window.AddFlags (WindowManagerFlags.KeepScreenOn);

			// Сохранение менеджера печати
			KKTSupport.ActPrintManager = (PrintManager)originalContext.GetSystemService (Service.PrintService);

			// Инициализация и запуск
			base.OnCreate (savedInstanceState);
			}
		private Intent mainService;

		/// <summary>
		/// Перезапуск службы
		/// </summary>
		protected override void OnStop ()
			{
			// Запрос на остановку при необходимости
			if (!AppSettings.AllowServiceToStart)
				AndroidSupport.StopRequested = true;
			// Иначе служба продолжит работу в фоне

			base.OnStop ();
			}

		/// <summary>
		/// Перезапуск основного приложения
		/// </summary>
		protected override void OnResume ()
			{
			// Перезапуск, если была остановлена (независимо от разрешения)
			if (mainService == null)
				{
				mainService = new Intent (this, typeof (MainService));
				mainService.SetPackage (this.PackageName);
				}
			AndroidSupport.StopRequested = false;

			// Нет смысла запускать сервис, если он не был закрыт приложением.
			// Также функция запуска foreground из свёрнутого состояния недоступна в Android 12 и новее
			if (AppSettings.AllowServiceToStart || !AndroidSupport.IsForegroundStartableFromResumeEvent)
				{
				base.OnResume ();
				return;
				}

			// Повторный запуск службы
			if (AndroidSupport.IsForegroundAvailable)
				StartForegroundService (mainService);
			else
				StartService (mainService);

			base.OnResume ();
			}
		}

	/// <summary>
	/// Класс описывает фоновую службу приложения
	/// </summary>
	[Service (Name = "com.RD_AAOW.TextToKKT",
		ForegroundServiceType = ForegroundService.TypeSpecialUse,
		Label = "KassArray",
		Exported = true)]
	public class MainService: Service
		{
		private const int notServiceID = 4420;

		// Идентификаторы процесса
		private Handler handler;
		private Action runnable;
		private bool isStarted = false;

		// Дескрипторы уведомлений
		private NotificationCompat.Builder notBuilder;
		private NotificationManager notManager;
		/*private const int notServiceID = 4420;*/
		private NotificationCompat.BigTextStyle notTextStyle;

		private Intent masterIntent;
		private PendingIntent masterPendingIntent;

		/*private BroadcastReceiver[] bcReceivers = new BroadcastReceiver[2];*/
		private BroadcastReceiver[] bcReceivers =
			new BroadcastReceiver[AndroidSupport.IntentFiltersForBootReceiver.Length];

		private const uint frameLength = 2500;  // ms

		/// <summary>
		/// Обработчик события создания службы
		/// </summary>
		public override void OnCreate ()
			{
			// Базовая обработка
			base.OnCreate ();

			// Запуск в бэкграунде
			handler = new Handler (Looper.MainLooper);

			// Аналог таймера (создаёт задание, которое само себя ставит в очередь исполнения ОС)
			runnable = new Action (() =>
			{
				if (isStarted)
					{
					TimerTick ();
					handler.PostDelayed (runnable, frameLength);
					}
			});
			}

		// Основной метод службы
		private void TimerTick ()
			{
			// Контроль требования завершения службы (игнорирует все прочие флаги)
			if (isStarted && AndroidSupport.StopRequested)
				{
				// Остановка службы
				isStarted = false;

				// Освобождение ресурсов
				notBuilder.Dispose ();
				masterIntent.Dispose ();
				masterPendingIntent.Dispose ();

				foreach (BroadcastReceiver br in bcReceivers)
					this.UnregisterReceiver (br);

				// Глушение (и отправка события destroy)
				StopSelf ();
				return;
				}
			}

		/// <summary>
		/// Обработчик события запуска службы
		/// </summary>
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
			{
			// Защита
			if (isStarted)
				return StartCommandResult.NotSticky;

			// Инициализация объектов настройки
			notManager = (NotificationManager)this.GetSystemService (Service.NotificationService);
			notBuilder = new NotificationCompat.Builder (this, ProgramDescription.AssemblyMainName.ToLower ());

			// Создание канала (для Android O и выше)
			if (AndroidSupport.IsForegroundAvailable)
				{
				NotificationChannel channel = new NotificationChannel (ProgramDescription.AssemblyMainName.ToLower (),
					ProgramDescription.AssemblyMainName, NotificationImportance.High);

				// Настройка
				channel.Description = ProgramDescription.AssemblyTitle;
				channel.LockscreenVisibility = NotificationVisibility.Public;

				// Запуск
				notManager.CreateNotificationChannel (channel);
				notBuilder.SetChannelId (ProgramDescription.AssemblyMainName.ToLower ());
				}

			// Категория "сообщение"
			notBuilder.SetCategory ("msg");

			// Оттенок заголовков оповещений
			notBuilder.SetColor (0x80FFC0);

			// Android 13 и новее: не позволяет закрыть оповещение вручную
			notBuilder.SetOngoing (true);

			// Android 12 и новее: требует немедленного отображения оповещения
			if (!AndroidSupport.IsForegroundStartableFromResumeEvent)
				notBuilder.SetForegroundServiceBehavior (NotificationCompat.ForegroundServiceImmediate);

			string launchMessage;
			if (AndroidSupport.IsForegroundStartableFromResumeEvent)
				launchMessage = "Нажмите, чтобы вернуться в основное приложение";
			else
				launchMessage = "Служба " + ProgramDescription.AssemblyMainName + " активна";

			notBuilder.SetContentText (launchMessage);
			notBuilder.SetContentTitle (ProgramDescription.AssemblyMainName);
			notBuilder.SetTicker (ProgramDescription.AssemblyMainName);

			// Настройка видимости для стартового сообщения
			
			// Для служебного сообщения
			notBuilder.SetDefaults (0);
			notBuilder.SetPriority (!AndroidSupport.IsForegroundAvailable ? (int)NotificationPriority.Default :
				(int)NotificationPriority.High);

			notBuilder.SetSmallIcon (Resource.Drawable.ic_not);
			if (AndroidSupport.IsLargeIconRequired)
				notBuilder.SetLargeIcon (BitmapFactory.DecodeResource (this.Resources,
					Resource.Drawable.ic_not_large));

			notBuilder.SetVisibility ((int)NotificationVisibility.Public);

			notTextStyle = new NotificationCompat.BigTextStyle (notBuilder);
			notTextStyle.BigText (launchMessage);

			// Прикрепление ссылки для перехода в основное приложение
			masterIntent = new Intent (this, typeof (NotificationLink));
			masterIntent.SetPackage (this.PackageName);

			masterPendingIntent = PendingIntent.GetService (this, notServiceID, masterIntent,
				PendingIntentFlags.Immutable);
			notBuilder.SetContentIntent (masterPendingIntent);

			// Стартовое сообщение
			Android.App.Notification notification = notBuilder.Build ();
			StartForeground (notServiceID, notification, ForegroundService.TypeSpecialUse);

			// Перенастройка для основного режима
			if (!AndroidSupport.IsForegroundAvailable)
				{
				notBuilder.SetDefaults (0);
				notBuilder.SetPriority ((int)NotificationPriority.Max);
				}

			// Запуск петли
			/*this.RegisterReceiver (bcReceivers[0] = new BootReceiver (),
				new IntentFilter (Intent.ActionBootCompleted));
			this.RegisterReceiver (bcReceivers[1] = new BootReceiver (),
				new IntentFilter ("android.intent.action.QUICKBOOT_POWERON"));*/
			for (int i = 0; i < AndroidSupport.IntentFiltersForBootReceiver.Length; i++)
				{
				bcReceivers[i] = new BootReceiver ();
				this.RegisterReceiver (bcReceivers[i], new IntentFilter (AndroidSupport.IntentFiltersForBootReceiver[i]),
					ReceiverFlags.Exported);
				}

			handler.PostDelayed (runnable, frameLength);
			isStarted = true;

			return StartCommandResult.NotSticky;
			}

		/// <summary>
		/// Обработчик остановки службы
		/// </summary>
		public override void OnDestroy ()
			{
			// Освобождение ресурсов, которые нельзя освободить в таймере
			handler.RemoveCallbacks (runnable);
			notManager.Cancel (notServiceID);
			if (AndroidSupport.IsForegroundAvailable)
				notManager.DeleteNotificationChannel (ProgramDescription.AssemblyMainName.ToLower ());
			notManager.Dispose ();

			// Остановка
			if (AndroidSupport.IsForegroundAvailable)
				StopForeground (StopForegroundFlags.Remove);
			else
				StopForeground (true);
			StopSelf ();

			// Стандартная обработка
			base.OnDestroy ();
			}

		/// <summary>
		/// Обработчик привязки службы (заглушка)
		/// </summary>
		public override IBinder OnBind (Intent intent)
			{
			// Return null because this is a pure started service
			return null;
			}

		/// <summary>
		/// Обработчик события снятия задачи (заглушка)
		/// </summary>
		public override void OnTaskRemoved (Intent rootIntent)
			{
			}
		}

	/// <summary>
	/// Класс описывает задание на открытие приложения
	/// </summary>
	[Service (Name = "com.RD_AAOW.KassArrayLink",
		Label = "KassArrayLink",
		ForegroundServiceType = ForegroundService.TypeDataSync,
		Exported = true)]
	public class NotificationLink: JobIntentService
		{
		private const int notServiceID = 4420;

		/// <summary>
		/// Конструктор (заглушка)
		/// </summary>
		public NotificationLink ()
			{
			}

		/// <summary>
		/// Обработка события выполнения задания для Android O и новее
		/// </summary>
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
			{
			if (AndroidSupport.IsForegroundAvailable)
				StartMasterActivity ();

			return base.OnStartCommand (intent, flags, startId);
			}

		/// <summary>
		/// Обработка события выполнения задания для Android N и старше
		/// </summary>
		protected override void OnHandleWork (Intent intent)
			{
			if (!AndroidSupport.IsForegroundAvailable)
				StartMasterActivity ();
			}

		// Общий метод запуска
		private void StartMasterActivity ()
			{
			// Защита от повторов
			if (AndroidSupport.AppIsRunning)
				return;
			AndroidSupport.StopRequested = false;

			if (mainActivity == null)
				{
				mainActivity = new Intent (this, typeof (MainActivity));
				/*mainActivity.PutExtra ("Tab", 0);*/
				mainActivity.SetPackage (this.PackageName);
				}
			PendingIntent.GetActivity (this, notServiceID, mainActivity,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable).Send ();   // Android S+ req
			}
		private Intent mainActivity;
		}

	/// <summary>
	/// Класс описывает приёмник события окончания загрузки ОС
	/// </summary>
	[BroadcastReceiver (Name = "com.RD_AAOW.KassArrayBoot",
		Label = "KassArrayBoot",
		Exported = true)]
	public class BootReceiver: BroadcastReceiver
		{
		/// <summary>
		/// Обработчик события наступления события окончания загрузки
		/// </summary>
		public override void OnReceive (Context context, Intent intent)
			{
			if (!AppSettings.AllowServiceToStart || (intent == null))
				return;

			/*if (intent.Action.Equals (Intent.ActionBootCompleted, StringComparison.CurrentCultureIgnoreCase) ||
				intent.Action.Equals (Intent.ActionReboot, StringComparison.CurrentCultureIgnoreCase))*/
			bool received = false;
			for (int i = 0; i < AndroidSupport.IntentFiltersForBootReceiver.Length; i++)
				if (intent.Action.Equals (AndroidSupport.IntentFiltersForBootReceiver[i],
					StringComparison.CurrentCultureIgnoreCase))
					{
					received = true;
					break;
					}

			if (received)
				{
				if (mainService == null)
					{
					mainService = new Intent (context, typeof (MainService));
					mainService.SetPackage (AppInfo.PackageName);
					}
				AndroidSupport.StopRequested = false;

				if (AndroidSupport.IsForegroundAvailable)
					context.StartForegroundService (mainService);
				else
					context.StartService (mainService);
				}
			}
		private Intent mainService;
		}
	}
