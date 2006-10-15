using System;
using System.Collections;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	class Modify
	{
		[Widget] Window		ModifyWindow;
		[Widget] Button		CloseButton;
		[Widget] Image		Inv1;
		[Widget] Image		Inv2;
		[Widget] Image		Inv3;
		[Widget] Image		Inv4;
		[Widget] Image		Inv5;
		[Widget] Image		Inv6;
		[Widget] Image		Inv7;
		[Widget] Image		Base1;
		[Widget] Image		Base2;
		[Widget] Image		Base3;
		[Widget] Image		Base4;
		[Widget] Image		Test1;
		[Widget] Image		Test2;
		[Widget] Image		Test3;
		[Widget] Image		Test4;
		[Widget] Image		Odd1;
		[Widget] Image		Odd2;
		[Widget] Image		Odd3;
		[Widget] Image		Odd4;
		[Widget] Image		Odd5;
		[Widget] Image		Odd6;
		[Widget] Image		First1;
		[Widget] Image		First2;
		[Widget] Image		First3;
		[Widget] Image		First4;
		[Widget] Image		First5;
		[Widget] Image		First6;
		[Widget] Image		Last1;
		[Widget] Image		Last2;
		[Widget] Image		Last3;
		[Widget] Image		Last4;
		[Widget] Image		Last5;
		[Widget] Image		Last6;
		[Widget] Image		Rand1;
		[Widget] Image		Rand2;
		[Widget] Image		Rand3;
		[Widget] Image		Rand4;
		[Widget] Image		Rand5;
		[Widget] Image		Rand6;
		[Widget] Button		InverseButton;
		[Widget] Button		BaseButton;
		[Widget] Button		TestButton;
		[Widget] Button		OddEvenButton;
		[Widget] Button		FirstHalfButton;
		[Widget] Button		SecondHalfButton;
		[Widget] Button		RandomButton;

		private FileList	filelist;
		private ArrayList	cat;
		private IBlock		b;

		private static Gdk.Pixbuf TestIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-test-22.png");
		private static Gdk.Pixbuf BaseIcon = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "image-base-22.png");
		private static Gdk.Pixbuf Arrow = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "go-next.png");

		public Modify(ArrayList cat, FileList filelist, IBlock b)
		{
			this.cat = cat;
			this.filelist = filelist;
			this.b = b;

			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "Modify.glade", "ModifyWindow", null);
			gxml.BindFields(this);

			ModifyWindow.DeleteEvent += CloseWindow;
			CloseButton.Clicked += CloseWindow;

			InverseButton.Clicked += InverseClicked;
			BaseButton.Clicked += BaseClicked;
			TestButton.Clicked += TestClicked;
			OddEvenButton.Clicked += OddEvenClicked;
			FirstHalfButton.Clicked += FirstHalfClicked;
			SecondHalfButton.Clicked += SecondHalfClicked;
			RandomButton.Clicked += RandomClicked;

			Inv1.FromPixbuf = TestIcon;
			Inv2.FromPixbuf = BaseIcon;
			Inv3.FromPixbuf = TestIcon;
			Inv4.FromPixbuf = Arrow;
			Inv5.FromPixbuf = BaseIcon;
			Inv6.FromPixbuf = TestIcon;
			Inv7.FromPixbuf = BaseIcon;

			Base1.FromPixbuf = BaseIcon;
			Base2.FromPixbuf = BaseIcon;
			Base3.FromPixbuf = BaseIcon;
			Base4.FromPixbuf = BaseIcon;

			Test1.FromPixbuf = TestIcon;
			Test2.FromPixbuf = TestIcon;
			Test3.FromPixbuf = TestIcon;
			Test4.FromPixbuf = TestIcon;

			Odd1.FromPixbuf = BaseIcon;
			Odd2.FromPixbuf = TestIcon;
			Odd3.FromPixbuf = BaseIcon;
			Odd4.FromPixbuf = TestIcon;
			Odd5.FromPixbuf = BaseIcon;
			Odd6.FromPixbuf = TestIcon;

			First1.FromPixbuf = TestIcon;
			First2.FromPixbuf = TestIcon;
			First3.FromPixbuf = TestIcon;
			First4.FromPixbuf = BaseIcon;
			First5.FromPixbuf = BaseIcon;
			First6.FromPixbuf = BaseIcon;

			Last1.FromPixbuf = BaseIcon;
			Last2.FromPixbuf = BaseIcon;
			Last3.FromPixbuf = BaseIcon;
			Last4.FromPixbuf = TestIcon;
			Last5.FromPixbuf = TestIcon;
			Last6.FromPixbuf = TestIcon;

			bool rbase = false, rtest = false;
			Random r = new Random();

			if(r.NextDouble() < 0.5)
			{
				rbase = true;
				Rand1.FromPixbuf = BaseIcon;
			}
			else
			{
				rtest = true;
				Rand1.FromPixbuf = TestIcon;
			}
			if(r.NextDouble() < 0.5)
			{
				rbase = true;
				Rand2.FromPixbuf = BaseIcon;
			}
			else
			{
				rtest = true;
				Rand2.FromPixbuf = TestIcon;
			}
			if(r.NextDouble() < 0.5)
			{
				rbase = true;
				Rand3.FromPixbuf = BaseIcon;
			}
			else
			{
				rtest = true;
				Rand3.FromPixbuf = TestIcon;
			}
			if(r.NextDouble() < 0.5)
			{
				rbase = true;
				Rand4.FromPixbuf = BaseIcon;
			}
			else
			{
				rtest = true;
				Rand4.FromPixbuf = TestIcon;
			}
			if(r.NextDouble() < 0.5)
			{
				rbase = true;
				Rand5.FromPixbuf = BaseIcon;
			}
			else
			{
				rtest = true;
				Rand5.FromPixbuf = TestIcon;
			}
			if(!rbase)
				Rand6.FromPixbuf = BaseIcon;
			else if(!rtest)
				Rand6.FromPixbuf = TestIcon;
			else if(r.NextDouble() < 0.5)
				Rand6.FromPixbuf = BaseIcon;
			else
				Rand6.FromPixbuf = TestIcon;

			ModifyWindow.ShowAll();
		}

		private void InverseClicked(object o, EventArgs args)
		{
			Category.Inverse(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void BaseClicked(object o, EventArgs args)
		{
			Category.ToBase(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void TestClicked(object o, EventArgs args)
		{
			Category.ToTest(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void OddEvenClicked(object o, EventArgs args)
		{
			Category.OddEven(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void FirstHalfClicked(object o, EventArgs args)
		{
			Category.FirstHalf(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void SecondHalfClicked(object o, EventArgs args)
		{
			Category.SecondHalf(cat);
			filelist.Update();
			ModifyWindow.Destroy();
			b.Invalidate();
		}

		private void RandomClicked(object o, EventArgs args)
		{
			new RandomSplit(RandomAction);
			ModifyWindow.Destroy();
		}

		private void RandomAction(int percentage, bool exact)
		{
			Category.RandomSplit(cat, percentage/100f, exact);
			filelist.Update();
			b.Invalidate();
		}

		private void CloseWindow(object o, EventArgs args)
		{
			ModifyWindow.Destroy();
		}
	}
}
