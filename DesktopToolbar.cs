using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SevenDeskBar
{

	public class DesktopToolbar : Form
	{
		#region Enums
		public enum AppBarMessages
		{
			/// <summary>
			/// Registers a new appbar and specifies the message identifier
			/// that the system should use to send notification messages to 
			/// the appbar. 
			/// </summary>
			New					= 0x00000000,	
			/// <summary>
			/// Unregisters an appbar, removing the bar from the system's 
			/// internal list.
			/// </summary>
			Remove				= 0x00000001,
			/// <summary>
			/// Requests a size and screen position for an appbar.
			/// </summary>
			QueryPos			= 0x00000002,	
			/// <summary>
			/// Sets the size and screen position of an appbar. 
			/// </summary>
			SetPos				= 0x00000003,	
			/// <summary>
			/// Retrieves the autohide and always-on-top states of the 
			/// Microsoft® Windows® taskbar. 
			/// </summary>
			GetState			= 0x00000004,	
			/// <summary>
			/// Retrieves the bounding rectangle of the Windows taskbar. 
			/// </summary>
			GetTaskBarPos		= 0x00000005,	
			/// <summary>
			/// Notifies the system that an appbar has been activated. 
			/// </summary>
			Activate			= 0x00000006,	
			/// <summary>
			/// Retrieves the handle to the autohide appbar associated with
			/// a particular edge of the screen. 
			/// </summary>
			GetAutoHideBar		= 0x00000007,	
			/// <summary>
			/// Registers or unregisters an autohide appbar for an edge of 
			/// the screen. 
			/// </summary>
			SetAutoHideBar		= 0x00000008,	
			/// <summary>
			/// Notifies the system when an appbar's position has changed. 
			/// </summary>
			WindowPosChanged	= 0x00000009,	
			/// <summary>
			/// Sets the state of the appbar's autohide and always-on-top 
			/// attributes.
			/// </summary>
			SetState			= 0x0000000a	
		}
		

		public enum AppBarNotifications
		{
			/// <summary>
			/// Notifies an appbar that the taskbar's autohide or 
			/// always-on-top state has changed—that is, the user has selected 
			/// or cleared the "Always on top" or "Auto hide" check box on the
			/// taskbar's property sheet. 
			/// </summary>
			StateChange			= 0x00000000,	
			/// <summary>
			/// Notifies an appbar when an event has occurred that may affect 
			/// the appbar's size and position. Events include changes in the
			/// taskbar's size, position, and visibility state, as well as the
			/// addition, removal, or resizing of another appbar on the same 
			/// side of the screen.
			/// </summary>
			PosChanged			= 0x00000001,	
			/// <summary>
			/// Notifies an appbar when a full-screen application is opening or
			/// closing. This notification is sent in the form of an 
			/// application-defined message that is set by the ABM_NEW message. 
			/// </summary>
			FullScreenApp		= 0x00000002,	
			/// <summary>
			/// Notifies an appbar that the user has selected the Cascade, 
			/// Tile Horizontally, or Tile Vertically command from the 
			/// taskbar's shortcut menu.
			/// </summary>
			WindowArrange		= 0x00000003	
		}
		
		[Flags]
		public enum AppBarStates
		{
			AutoHide			= 0x00000001,
			AlwaysOnTop			= 0x00000002
		}

				
		public enum AppBarEdges
		{
			Left				= 0,
			Top					= 1,
			Right				= 2,
			Bottom				= 3,
			Float				= 4
		}
	
		// Window Messages		
		public enum WM
		{
			ACTIVATE				= 0x0006,
			WINDOWPOSCHANGED		= 0x0047,
			NCHITTEST				= 0x0084
		}

		public enum MousePositionCodes
		{
			HTERROR             = (-2),
			HTTRANSPARENT       = (-1),
			HTNOWHERE           = 0,
			HTCLIENT            = 1,
			HTCAPTION           = 2,
			HTSYSMENU           = 3,
			HTGROWBOX           = 4,
			HTSIZE              = HTGROWBOX,
			HTMENU              = 5,
			HTHSCROLL           = 6,
			HTVSCROLL           = 7,
			HTMINBUTTON         = 8,
			HTMAXBUTTON         = 9,
			HTLEFT              = 10,
			HTRIGHT             = 11,
			HTTOP               = 12,
			HTTOPLEFT           = 13,
			HTTOPRIGHT          = 14,
			HTBOTTOM            = 15,
			HTBOTTOMLEFT        = 16,
			HTBOTTOMRIGHT       = 17,
			HTBORDER            = 18,
			HTREDUCE            = HTMINBUTTON,
			HTZOOM              = HTMAXBUTTON,
			HTSIZEFIRST         = HTLEFT,
			HTSIZELAST          = HTBOTTOMRIGHT,
			HTOBJECT            = 19,
			HTCLOSE             = 20,
			HTHELP              = 21
		}

		#endregion Enums
        
		#region AppBar Functions

		private Boolean AppbarNew()
		{
			if (CallbackMessageID == 0)
				throw new Exception("CallbackMessageID is 0");

			if (IsAppbarMode)
				return true;

			m_PrevSize = Size;
			m_PrevLocation = Location;

			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			msgData.uCallbackMessage = CallbackMessageID;

			// install new appbar
			UInt32 retVal = ShellApi.SHAppBarMessage((UInt32)AppBarMessages.New,ref msgData);
			IsAppbarMode = (retVal!=0);
			m_HasLastRect = false;

			SizeAppBar();

			return IsAppbarMode;
		}

		private Boolean AppbarRemove()
		{
			UInt32 retVal = 0;
			if (IsAppbarMode)
			{
				// prepare data structure of message
				ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
				msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
				msgData.hWnd = Handle;

				// remove appbar
				retVal = ShellApi.SHAppBarMessage((UInt32)AppBarMessages.Remove,ref msgData);
			}

			IsAppbarMode = false;
			m_HasLastRect = false;

			if (!m_OverlayDocked)
			{
				Size = m_PrevSize;
				Location = m_PrevLocation;
			}

			return (retVal!=0) ? true : false;
		}
		
		private void AppbarQueryPos(ref ShellApi.RECT appRect)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.rc = appRect;

			// query postion for the appbar
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.QueryPos, ref msgData);
			appRect	= msgData.rc;
		}
		
		private void AppbarSetPos(ref ShellApi.RECT appRect)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.rc = appRect;

			// set postion for the appbar
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.SetPos, ref msgData);
			appRect	= msgData.rc;
		}

		private void AppbarActivate()
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			
			// send activate to the system
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.Activate, ref msgData);
		}

		private void AppbarWindowPosChanged()
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			
			// send windowposchanged to the system 
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.WindowPosChanged, ref msgData);
		}

		private Boolean AppbarSetAutoHideBar(Boolean hideValue)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.hWnd = Handle;
			msgData.uEdge = (UInt32)m_Edge;
			msgData.lParam = (hideValue) ? 1 : 0;
					
			// set auto hide
			UInt32 retVal = ShellApi.SHAppBarMessage((UInt32)AppBarMessages.SetAutoHideBar,ref msgData);
			return (retVal!=0) ? true : false;
		}

		private IntPtr AppbarGetAutoHideBar(AppBarEdges edge)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.uEdge = (UInt32)edge;
					
			// get auto hide
			IntPtr retVal = (IntPtr)ShellApi.SHAppBarMessage((UInt32)AppBarMessages.GetAutoHideBar,ref msgData);
			return retVal;
		}

		private AppBarStates AppbarGetTaskbarState()
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
					
			// get taskbar state
			UInt32 retVal = ShellApi.SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
			return (AppBarStates)retVal;
		}

		private void AppbarSetTaskbarState(AppBarStates state)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
			msgData.lParam = (Int32)state;
					
			// set taskbar state
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
		}

		private void AppbarGetTaskbarPos(out ShellApi.RECT taskRect)
		{
			// prepare data structure of message
			ShellApi.APPBARDATA msgData = new ShellApi.APPBARDATA();
			msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
					
			// get taskbar position
			ShellApi.SHAppBarMessage((UInt32)AppBarMessages.GetTaskBarPos, ref msgData);
			taskRect = msgData.rc;
		}

		#endregion AppBar Functions
		
		#region Private Variables
		
		// saves the current edge
		private AppBarEdges m_Edge = AppBarEdges.Float;

		// saves the callback message id
		private UInt32 CallbackMessageID = 0;

		// are we in dock mode?
		private Boolean IsAppbarMode = false;

		// save the floating size and location
		private Size m_PrevSize;
		private Point m_PrevLocation;

        private int m_StdWidth = 30;
        private int m_StdHeight = 30;

		// true  = the toolbar reserves its space on the screen edge (standard appbar)
		// false = the toolbar is docked on the edge but floats over the other windows
		//         (used with auto hide: showing/hiding the toolbar must not resize
		//          every maximized window and move the desktop icons)
		private bool m_ReserveSpace = true;
		private bool m_OverlayDocked = false;

		// last position given to the shell, used to avoid endless ABM_SETPOS /
		// ABN_POSCHANGED ping-pong between appbars (two SevenDeskBar instances, taskbar...)
		private ShellApi.RECT m_LastAppbarRect;
		private bool m_HasLastRect = false;
		private bool m_InSizeAppBar = false;
		
		#endregion Private Variables
		
		#region Tool window / Show desktop

		private const int WS_EX_TOOLWINDOW = 0x00000080;
		private const int WS_EX_APPWINDOW = 0x00040000;

		/// <summary>
		/// The toolbar must never appear in the Alt+Tab list or in the taskbar.
		/// WS_EX_TOOLWINDOW is set here (and not only with SetWindowLong) because
		/// WinForms re-applies the window styles from CreateParams every time a
		/// property like FormBorderStyle changes, removing styles set by hand.
		/// </summary>
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= WS_EX_TOOLWINDOW;
				cp.ExStyle &= ~WS_EX_APPWINDOW;
				return cp;
			}
		}

		private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
			int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll")]
		private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
			WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

		[DllImport("user32.dll")]
		private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool IsIconic(IntPtr hWnd);

		private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
		private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
		private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
		private const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
		private const int SW_SHOWNOACTIVATE = 4;
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const int WM_SYSCOMMAND = 0x0112;
		private const int SC_MINIMIZE = 0xF020;

		// the delegate must be kept alive as long as the hook is installed
		private WinEventDelegate m_WinEventProc;
		private IntPtr m_HookForeground = IntPtr.Zero;
		private IntPtr m_HookMinimize = IntPtr.Zero;

		private void InstallDesktopHooks()
		{
			if (m_WinEventProc == null)
				m_WinEventProc = new WinEventDelegate(OnWinEvent);
			if (m_HookForeground == IntPtr.Zero)
				m_HookForeground = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
					IntPtr.Zero, m_WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);
			if (m_HookMinimize == IntPtr.Zero)
				m_HookMinimize = SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZESTART,
					IntPtr.Zero, m_WinEventProc, 0, 0, WINEVENT_OUTOFCONTEXT);
		}

		private void RemoveDesktopHooks()
		{
			if (m_HookForeground != IntPtr.Zero)
			{
				UnhookWinEvent(m_HookForeground);
				m_HookForeground = IntPtr.Zero;
			}
			if (m_HookMinimize != IntPtr.Zero)
			{
				UnhookWinEvent(m_HookMinimize);
				m_HookMinimize = IntPtr.Zero;
			}
		}

		private static bool IsDesktopWindow(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
				return false;
			System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
			GetClassName(hwnd, sb, sb.Capacity);
			string cls = sb.ToString();
			return cls == "Progman" || cls == "WorkerW";
		}

		/// <summary>
		/// "Show desktop" (Win+D or the button at the end of the taskbar) brings the
		/// desktop window in foreground and minimizes/hides the other windows.
		/// The toolbar is considered part of the desktop, so when this happens it is
		/// restored (without stealing the focus) and put back on top.
		/// </summary>
		private void OnWinEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
			int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
		{
			if (IsDisposed || !IsHandleCreated || idObject != 0)
				return;
			bool keep = false;
			if (eventType == EVENT_SYSTEM_MINIMIZESTART && hwnd == Handle)
				keep = true;
			else if (eventType == EVENT_SYSTEM_FOREGROUND && IsDesktopWindow(hwnd))
				keep = true;
			if (keep)
				BeginInvoke(new MethodInvoker(KeepOnDesktop));
		}

		private void KeepOnDesktop()
		{
			if (IsDisposed || !IsHandleCreated || !Visible)
				return;
			if (IsIconic(Handle))
				ShowWindow(Handle, SW_SHOWNOACTIVATE);
			if (TopMost)
				SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0,
					SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			InstallDesktopHooks();
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			RemoveDesktopHooks();
			base.OnHandleDestroyed(e);
		}

		#endregion Tool window / Show desktop

		public DesktopToolbar()
		{
			FormBorderStyle = FormBorderStyle.SizableToolWindow;
		    
			// Register a unique message as our callback message
			CallbackMessageID = RegisterCallbackMessage();
			if (CallbackMessageID == 0)
				throw new Exception("RegisterCallbackMessage failed");
		}
			
		private UInt32 RegisterCallbackMessage()
		{
			String uniqueMessageString = Guid.NewGuid().ToString();
			return ShellApi.RegisterWindowMessage(uniqueMessageString);
		}

		#region Monitor info

		[StructLayout(LayoutKind.Sequential)]
		private struct MONITORINFO
		{
			public int cbSize;
			public ShellApi.RECT rcMonitor;
			public ShellApi.RECT rcWork;
			public uint dwFlags;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINTSTRUCT
		{
			public int x;
			public int y;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, uint dwFlags);

		[DllImport("user32.dll")]
		private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

		[DllImport("shcore.dll")]
		private static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

		private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;

		private static IntPtr PrimaryMonitor()
		{
			POINTSTRUCT pt = new POINTSTRUCT();
			return MonitorFromPoint(pt, MONITOR_DEFAULTTOPRIMARY);
		}

		/// <summary>
		/// Bounds and work area of the monitor where the toolbar is docked (the primary one),
		/// read directly from Windows: they must be up to date even when the work area
		/// has just been changed by another appbar.
		/// </summary>
		protected static void GetDockMonitor(out Rectangle bounds, out Rectangle workArea)
		{
			MONITORINFO mi = new MONITORINFO();
			mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
			if (GetMonitorInfo(PrimaryMonitor(), ref mi))
			{
				bounds = Rectangle.FromLTRB(mi.rcMonitor.left, mi.rcMonitor.top, mi.rcMonitor.right, mi.rcMonitor.bottom);
				workArea = Rectangle.FromLTRB(mi.rcWork.left, mi.rcWork.top, mi.rcWork.right, mi.rcWork.bottom);
			}
			else
			{
				bounds = Screen.PrimaryScreen.Bounds;
				workArea = Screen.PrimaryScreen.WorkingArea;
			}
		}

		/// <summary>
		/// DPI (zoom) of the monitor where the toolbar is docked: 96 = 100%, 144 = 150% ...
		/// </summary>
		public static int DockMonitorDpi()
		{
			try
			{
				uint dpiX, dpiY;
				if (GetDpiForMonitor(PrimaryMonitor(), 0, out dpiX, out dpiY) == 0 && dpiY > 0)
					return (int)dpiY;
			}
			catch (DllNotFoundException)
			{
			}
			catch (EntryPointNotFoundException)
			{
			}
			return SystemDpi();
		}

		/// <summary>
		/// "System" DPI, used by Windows to convert font sizes from points to pixels
		/// </summary>
		public static int SystemDpi()
		{
			using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
			{
				return (int)Math.Round(g.DpiY);
			}
		}

		#endregion Monitor info

		private void SizeAppBar()
		{
			if (m_InSizeAppBar)
				return;
			if (!IsAppbarMode && !m_OverlayDocked)
				return;
			m_InSizeAppBar = true;
			try
			{
				Rectangle mon, work;
				GetDockMonitor(out mon, out work);
				ShellApi.RECT rt = new ShellApi.RECT();

				// Along the toolbar the size is the one of the work area, so the
				// Windows taskbar (and other appbars) are not covered.
				if ((m_Edge == AppBarEdges.Left) || (m_Edge == AppBarEdges.Right))
				{
					rt.top = work.Top;
					rt.bottom = work.Bottom;
					if (m_Edge == AppBarEdges.Left)
					{
						rt.left = IsAppbarMode ? mon.Left : work.Left;
						rt.right = rt.left + m_PrevSize.Width;
					}
					else
					{
						rt.right = IsAppbarMode ? mon.Right : work.Right;
						rt.left = rt.right - m_PrevSize.Width;
					}
				}
				else
				{
					rt.left = work.Left;
					rt.right = work.Right;
					if (m_Edge == AppBarEdges.Top)
					{
						rt.top = IsAppbarMode ? mon.Top : work.Top;
						rt.bottom = rt.top + m_PrevSize.Height;
					}
					else
					{
						rt.bottom = IsAppbarMode ? mon.Bottom : work.Bottom;
						rt.top = rt.bottom - m_PrevSize.Height;
					}
				}

				if (IsAppbarMode)
				{
					AppbarQueryPos(ref rt);

					switch (m_Edge)
					{
						case AppBarEdges.Left:
							rt.right = rt.left + m_PrevSize.Width;
							break;
						case AppBarEdges.Right:
							rt.left = rt.right - m_PrevSize.Width;
							break;
						case AppBarEdges.Top:
							rt.bottom = rt.top + m_PrevSize.Height;
							break;
						case AppBarEdges.Bottom:
							rt.top = rt.bottom - m_PrevSize.Height;
							break;
					}

					Rectangle wanted = Rectangle.FromLTRB(rt.left, rt.top, rt.right, rt.bottom);
					Rectangle last = Rectangle.FromLTRB(m_LastAppbarRect.left, m_LastAppbarRect.top, m_LastAppbarRect.right, m_LastAppbarRect.bottom);
					if (m_HasLastRect && wanted == last && Bounds == wanted)
					{
						// nothing changed: a new ABM_SETPOS would only notify again all
						// the other appbars, which would reposition themselves, and so on...
						return;
					}

					AppbarSetPos(ref rt);
					m_LastAppbarRect = rt;
					m_HasLastRect = true;
				}

				Rectangle newBounds = Rectangle.FromLTRB(rt.left, rt.top, rt.right, rt.bottom);
				if (Bounds != newBounds)
				{
					// the size limits must not prevent the toolbar from taking its place
					if (MinimumSize.Width > newBounds.Width || MinimumSize.Height > newBounds.Height)
						MinimumSize = Size.Empty;
					if ((MaximumSize.Width > 0 && MaximumSize.Width < newBounds.Width) || (MaximumSize.Height > 0 && MaximumSize.Height < newBounds.Height))
						MaximumSize = Size.Empty;
					Bounds = newBounds;
				}
			}
			finally
			{
				m_InSizeAppBar = false;
			}
		}

		/// <summary>
		/// See m_ReserveSpace. The change is applied the next time Edge is set.
		/// </summary>
		public bool ReserveSpace
		{
			get { return m_ReserveSpace; }
			set { m_ReserveSpace = value; }
		}
		
		
		#region Message Handlers

		void OnAppbarNotification(ref Message msg)
		{
			AppBarStates state;
			AppBarNotifications msgType = (AppBarNotifications)(Int32)msg.WParam;
					
			switch (msgType)
			{
				case AppBarNotifications.PosChanged:
					SizeAppBar();
					break;
						
				case AppBarNotifications.StateChange:
					// From Windows 7 on ABM_GETSTATE no longer returns ABS_ALWAYSONTOP
					// (the taskbar is always on top), so the old check made the toolbar
					// lose the "always on top" state on Windows 10 (ticket #26).
					// The toolbar simply always stays on top.
					state = AppbarGetTaskbarState();
					TopMost = true;
					BringToFront();
					break;

				case AppBarNotifications.FullScreenApp:
					if ((int)msg.LParam !=0)
					{
						// a full screen application (video, game, presentation...) is open
						TopMost = false;
						SendToBack();
					}
					else
					{
						TopMost = true;
						BringToFront();
					}
					break;

				case AppBarNotifications.WindowArrange:
					if ((int)msg.LParam != 0)	// before
						Visible = false;
					else						// after
						Visible = true;
							
					break;
			}
		}

		void OnNcHitTest(ref Message msg)
		{
			DefWndProc(ref msg);
			if ((m_Edge == AppBarEdges.Top) && ((int)msg.Result == (int)MousePositionCodes.HTBOTTOM))
				0.ToString();	
			else if ((m_Edge == AppBarEdges.Bottom) && ((int)msg.Result == (int)MousePositionCodes.HTTOP))
				0.ToString();	
			else if ((m_Edge == AppBarEdges.Left) && ((int)msg.Result == (int)MousePositionCodes.HTRIGHT))
				0.ToString();	
			else if ((m_Edge == AppBarEdges.Right) && ((int)msg.Result == (int)MousePositionCodes.HTLEFT))
				0.ToString();	
			else if ((int)msg.Result == (int)MousePositionCodes.HTCLOSE)
				0.ToString();	
			else
			{
				msg.Result = (IntPtr)MousePositionCodes.HTCLIENT;
				return;
			}
			base.WndProc(ref msg);
		}


		#endregion Message Handlers

		#region Window Procedure

		protected override void WndProc(ref Message msg)
		{
			// the toolbar can never be minimized (it is part of the desktop)
			if (msg.Msg == WM_SYSCOMMAND && ((int)msg.WParam & 0xFFF0) == SC_MINIMIZE)
				return;

			if (IsAppbarMode)
			{
				if (msg.Msg == CallbackMessageID)
				{
					OnAppbarNotification(ref msg);
				}
				else if (msg.Msg == (int)WM.ACTIVATE)
				{
					AppbarActivate();
				}
				else if (msg.Msg == (int)WM.WINDOWPOSCHANGED)
				{
					AppbarWindowPosChanged();
				}
				else if (msg.Msg == (int)WM.NCHITTEST)
				{
					OnNcHitTest(ref msg);
					return;
				}
			}
				
			base.WndProc(ref msg);
		}
		
		#endregion Window Procedure

		protected override void OnLoad(EventArgs e)
		{
			m_PrevSize = Size;
			m_PrevLocation = Location;
			base.OnLoad(e);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			m_OverlayDocked = false;
			AppbarRemove();
			base.OnClosing(e);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if ((IsAppbarMode || m_OverlayDocked) && !m_InSizeAppBar)
			{
				if (m_Edge == AppBarEdges.Top || m_Edge == AppBarEdges.Bottom)
					m_PrevSize.Height = Size.Height;
				else
					m_PrevSize.Width = Size.Width;

				SizeAppBar();
			}
			
			base.OnSizeChanged(e);
		}
		

		public AppBarEdges Edge 
		{
			get 
			{
				return m_Edge;
			}
			set
			{
				m_Edge = value;
				if (value == AppBarEdges.Float)
				{
					m_OverlayDocked = false;
					AppbarRemove();
				}
				else if (m_ReserveSpace)
				{
					m_OverlayDocked = false;
					AppbarNew();
				}
				else
				{
					// docked without reserving space (auto hide)
					if (IsAppbarMode)
					{
						m_OverlayDocked = true;
						AppbarRemove();
					}
					else if (!m_OverlayDocked)
					{
						m_PrevSize = Size;
						m_PrevLocation = Location;
						m_OverlayDocked = true;
					}
				}

				SizeAppBar();
			}
		}

        public int StandardWidth
        {
            get
            {
                return m_StdWidth;
            }
            set
            {
                m_StdWidth = value;
                Rectangle mon, work;
                GetDockMonitor(out mon, out work);
                // The limits are set before the size: without an explicit minimum size
                // Windows applies its default minimum width (over 100 pixels) and the
                // toolbar could not become thin (auto hide).
                this.MaximumSize = Size.Empty;
                this.MinimumSize = new System.Drawing.Size(value, 0);
                this.MaximumSize = new System.Drawing.Size(value, work.Height);
                this.Width = value;
            }
        }

        public int StandardHeight
        {
            get
            {
                return m_StdHeight;
            }
            set
            {
                m_StdHeight = value;
                Rectangle mon, work;
                GetDockMonitor(out mon, out work);
                this.MaximumSize = Size.Empty;
                this.MinimumSize = new System.Drawing.Size(0, value);
                this.MaximumSize = new System.Drawing.Size(work.Width, value);
                this.Height = value;
            }
        }

        public void SetAutoHideBar(bool AutoHide)
        {
            this.AppbarSetAutoHideBar(AutoHide);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DesktopToolbar
            // 
            this.ClientSize = new System.Drawing.Size(24, 262);
            this.ControlBox = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DesktopToolbar";
            this.Load += new System.EventHandler(this.ApplicationDesktopToolbar_Load);
            this.ResumeLayout(false);

        }

        private void ApplicationDesktopToolbar_Load(object sender, EventArgs e)
        {

        }

        
	}
}