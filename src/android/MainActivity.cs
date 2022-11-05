using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Print;
using Android.Support.V4.App;
using Android.Views;
using System;

#if DEBUG
[assembly: Application (Debuggable = true)]
#else
[assembly: Application (Debuggable = false)]
#endif

namespace RD_AAOW.Droid
	{
	/// <summary>
	/// Класс описывает загрузчик приложения
	/// </summary>
	[Activity (Label = "Text to KKT",
		Icon = "@drawable/launcher_foreground",
		Theme = "@style/SplashTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity:global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
		{
		/// <summary>
		/// Обработчик события создания экземпляра
		/// </summary>
		protected override void OnCreate (Bundle savedInstanceState)
			{
			// Базовая настройка
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			// Отмена темы для splash screen
			base.SetTheme (Resource.Style.MainTheme);

			// Инициализация и запуск
			base.OnCreate (savedInstanceState);
			global::Xamarin.Forms.Forms.Init (this, savedInstanceState);

			// Запуск независимо от разрешения
			Intent mainService = new Intent (this, typeof (MainService));
			AndroidSupport.StopRequested = false;

			if (AndroidSupport.IsForegroundAvailable)
				StartForegroundService (mainService);
			else
				StartService (mainService);

			// Запрет на переход в ждущий режим
			this.Window.AddFlags (WindowManagerFlags.KeepScreenOn);
			LoadApplication (new App ((PrintManager)this.GetSystemService (Service.PrintService)));
			}

		/// <summary>
		/// Перезапуск службы
		/// </summary>
		protected override void OnStop ()
			{
			// Запрос на остановку при необходимости
			if (!AndroidSupport.AllowServiceToStart)
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
			Intent mainService = new Intent (this, typeof (MainService));
			AndroidSupport.StopRequested = false;

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
		Label = "TextToKKT",
		Exported = true)]
	public class MainService:global::Android.App.Service
		{
		// Идентификаторы процесса
		private Handler handler;
		private Action runnable;
		private bool isStarted = false;

		// Дескрипторы уведомлений
		private NotificationCompat.Builder notBuilder;
		private NotificationManager notManager;
		private const int notServiceID = 4420;
		private NotificationCompat.BigTextStyle notTextStyle;

		private Intent masterIntent;
		private PendingIntent masterPendingIntent;

		private BroadcastReceiver[] bcReceivers = new BroadcastReceiver[2];

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
			if (AndroidSupport.StopRequested)
				{
				Intent mainService = new Intent (this, typeof (MainService));
				StopService (mainService);

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

			// Инициализация сообщений
			notBuilder.SetCategory ("msg");     // Категория "сообщение"
			notBuilder.SetColor (0x80FFC0);     // Оттенок заголовков оповещений

			string launchMessage = "Нажмите, чтобы вернуться в основное приложение";
			notBuilder.SetContentText (launchMessage);
			notBuilder.SetContentTitle (ProgramDescription.AssemblyMainName);
			notBuilder.SetTicker (ProgramDescription.AssemblyMainName);

			// Настройка видимости для стартового сообщения
			notBuilder.SetDefaults (0);         // Для служебного сообщения
			notBuilder.SetPriority (!AndroidSupport.IsForegroundAvailable ? (int)NotificationPriority.Default :
				(int)NotificationPriority.High);

			notBuilder.SetSmallIcon (Resource.Drawable.ic_not);
			if (AndroidSupport.IsLargeIconRequired)
				{
				notBuilder.SetLargeIcon (BitmapFactory.DecodeResource (this.Resources, Resource.Drawable.ic_not_large));
				}

			notBuilder.SetVisibility ((int)NotificationVisibility.Public);

			notTextStyle = new NotificationCompat.BigTextStyle (notBuilder);
			notTextStyle.BigText (launchMessage);

			// Прикрепление ссылки для перехода в основное приложение
			masterIntent = new Intent (this, typeof (NotificationLink));
			masterPendingIntent = PendingIntent.GetService (this, 0, masterIntent, PendingIntentFlags.Immutable);   // Android S+ req
			notBuilder.SetContentIntent (masterPendingIntent);

			// Стартовое сообщение
			Android.App.Notification notification = notBuilder.Build ();
			StartForeground (notServiceID, notification);

			// Перенастройка для основного режима
			if (!AndroidSupport.IsForegroundAvailable)
				{
				notBuilder.SetDefaults (0);
				notBuilder.SetPriority ((int)NotificationPriority.Max);
				}

			// Запуск петли
			this.RegisterReceiver (bcReceivers[0] = new BootReceiver (),
				new IntentFilter (Intent.ActionBootCompleted));
			this.RegisterReceiver (bcReceivers[1] = new BootReceiver (),
				new IntentFilter ("android.intent.action.QUICKBOOT_POWERON"));

			handler.PostDelayed (runnable, frameLength);
			isStarted = true;

			return StartCommandResult.NotSticky;
			}

		/// <summary>
		/// Обработчик остановки службы
		/// </summary>
		public override void OnDestroy ()
			{
			// Остановка службы
			handler.RemoveCallbacks (runnable);
			notManager.Cancel (notServiceID);
			if (AndroidSupport.IsForegroundAvailable)
				notManager.DeleteNotificationChannel (ProgramDescription.AssemblyMainName.ToLower ());
			isStarted = false;

			// Освобождение ресурсов
			notBuilder.Dispose ();
			notManager.Dispose ();

			masterIntent.Dispose ();
			masterPendingIntent.Dispose ();

			// Глушение
			if (AndroidSupport.IsForegroundAvailable)
				StopForeground (StopForegroundFlags.Remove);
			else
				StopForeground (true);
			StopSelf ();

			foreach (BroadcastReceiver br in bcReceivers)
				this.UnregisterReceiver (br);

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
	[Service (Name = "com.RD_AAOW.TextToKKTLink",
		Label = "TextToKKTLink",
		Exported = true)]
	public class NotificationLink:JobIntentService
		{
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
				StartActivity ();

			return base.OnStartCommand (intent, flags, startId);
			}

		/// <summary>
		/// Обработка события выполнения задания для Android N и старше
		/// </summary>
		protected override void OnHandleWork (Intent intent)
			{
			if (!AndroidSupport.IsForegroundAvailable)
				StartActivity ();
			}

		// Общий метод запуска
		private void StartActivity ()
			{
			if (AndroidSupport.AppIsRunning)
				return;

			AndroidSupport.StopRequested = false;
			Intent mainActivity = new Intent (this, typeof (MainActivity));
			mainActivity.PutExtra ("Tab", 0);
			PendingIntent.GetActivity (this, 0, mainActivity,
				PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable).Send ();   // Android S+ req
			}
		}

	/// <summary>
	/// Класс описывает приёмник события окончания загрузки ОС
	/// </summary>
	[BroadcastReceiver (Name = "com.RD_AAOW.TextToKKTBoot",
		Label = "TextToKKTBoot",
		Exported = true)]
	public class BootReceiver:BroadcastReceiver
		{
		/// <summary>
		/// Обработчик события наступления события окончания загрузки
		/// </summary>
		public override void OnReceive (Context context, Intent intent)
			{
			if (!AndroidSupport.AllowServiceToStart || (intent == null))
				return;

			if (intent.Action.Equals (Intent.ActionBootCompleted, StringComparison.CurrentCultureIgnoreCase) ||
				intent.Action.Equals (Intent.ActionReboot, StringComparison.CurrentCultureIgnoreCase))
				{
				Intent mainService = new Intent (context, typeof (MainService));
				AndroidSupport.StopRequested = false;

				if (AndroidSupport.IsForegroundAvailable)
					context.StartForegroundService (mainService);
				else
					context.StartService (mainService);
				}
			}
		}
	}
