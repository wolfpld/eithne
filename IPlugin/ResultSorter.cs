using System;
using System.Collections;

namespace Eithne
{
	class ResultSorter : IComparer
	{
		private double identity;
		private double[] res;

		public ResultSorter(double identity, double[] res)
		{
			this.identity = identity;
			this.res = res;
		}

		public int Compare(object x, object y)
		{
			double vx = Math.Abs(identity - res[(int)x]);
			double vy = Math.Abs(identity - res[(int)y]);

			if(vx < vy)
				return -1;
			else if(vx > vy)
				return 1;
			else
				return 0;
		}
	}
}
