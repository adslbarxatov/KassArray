﻿using Android.App;
using Android.Runtime;

namespace RD_AAOW
	{
#if DEBUG
	[Application (Debuggable = true)]
#else
	[Application (Debuggable = false)]
#endif
	public class MainApplication: MauiApplication
		{
		public MainApplication (IntPtr handle, JniHandleOwnership ownership)
			: base (handle, ownership)
			{
			}

		protected override MauiApp CreateMauiApp () => MauiProgram.CreateMauiApp ();
		}
	}
