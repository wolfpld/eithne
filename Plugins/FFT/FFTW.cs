using System;
using System.Runtime.InteropServices;

namespace Eithne
{
	internal sealed class FFTW
	{
		const string dll = "libfftw3-3.dll";

		internal enum Kind
		{
			FFTW_R2HC = 0,		// Fourier
			FFTW_REDFT10 = 5	// Cosine
		}

		internal enum Direction
		{
			Forward = -1,
			Backward = 1
		}

		[DllImport (dll)]
		internal static extern void fftw_execute(IntPtr plan);

		[DllImport (dll)]
		internal static extern void fftw_destroy_plan(IntPtr plan);

		[DllImport (dll)]
		internal static extern void fftw_cleanup();

		[DllImport (dll)]
		internal static extern IntPtr fftw_plan_r2r_2d(int nx, int ny, double[] _in, [Out] double[] _out, Kind kindx,
				Kind kindy, uint flags);

		[DllImport (dll)]
		internal static extern IntPtr fftw_plan_dft_2d(int nx, int ny, double[] _in, [Out] double[] _out, Direction sign,
				uint flags);
	}
}
