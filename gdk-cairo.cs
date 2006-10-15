using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gdk
{
	public class Context
	{
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_cairo_create(IntPtr handle);

		public static Cairo.Context CreateDrawable(Gdk.Drawable drawable)
		{
			Cairo.Context g = new Cairo.Context(gdk_cairo_create(drawable.Handle));

			return g;
		}
	}
}
