using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Eithne
{
	internal sealed class FFTW
	{
		const string dll = "libfftw3-3.dll";

		internal static Mutex mutex = new Mutex(false, "FFTW mutex");

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

		internal static void Execute(IntPtr plan)
		{
			mutex.WaitOne();
			fftw_execute(plan);
			mutex.ReleaseMutex();
		}

		internal static void DestroyPlan(IntPtr plan)
		{
			mutex.WaitOne();
			fftw_destroy_plan(plan);
			mutex.ReleaseMutex();
		}

		internal static void Cleanup()
		{
			mutex.WaitOne();
			fftw_cleanup();
			mutex.ReleaseMutex();
		}

		internal static IntPtr PlanR2R2D(int nx, int ny, double[] _in, [Out] double[] _out, Kind kindx, Kind kindy, uint flags)
		{
			mutex.WaitOne();
			IntPtr ret = fftw_plan_r2r_2d(nx, ny, _in, _out, kindx, kindy, flags);
			mutex.ReleaseMutex();

			return ret;
		}

		internal static IntPtr PlanDFT2D(int nx, int ny, double[] _in, [Out] double[] _out, Direction sign, uint flags)
		{
			mutex.WaitOne();
			IntPtr ret = fftw_plan_dft_2d(nx, ny, _in, _out, sign, flags);
			mutex.ReleaseMutex();

			return ret;
		}

		[DllImport (dll)]
		private static extern void fftw_execute(IntPtr plan);

		[DllImport (dll)]
		private static extern void fftw_destroy_plan(IntPtr plan);

		[DllImport (dll)]
		private static extern void fftw_cleanup();

		[DllImport (dll)]
		private static extern IntPtr fftw_plan_r2r_2d(int nx, int ny, double[] _in, [Out] double[] _out, Kind kindx,
				Kind kindy, uint flags);

		[DllImport (dll)]
		private static extern IntPtr fftw_plan_dft_2d(int nx, int ny, double[] _in, [Out] double[] _out, Direction sign,
				uint flags);
	}
}
