﻿namespace Eithne
{
	namespace Plugin
	{
		public abstract class In : Base
		{
			public override int NumIn			{ get { return 0; } }
			public override string DescIn(int n)		{ return null; }
			public override string[] MatchIn		{ get {return null; } }
		}
	}
}
