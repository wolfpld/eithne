using System;
using System.Reflection;
using Gtk;
using Glade;
using Mono.Unix;

namespace Eithne
{
	public class DialogQuestion
	{
		[Widget] Window		DialogQuestionWindow;
		[Widget] Image		DialogImage;
		[Widget] Button		NoButton;
		[Widget] Button		YesButton;
		[Widget] Label		Message;

		private bool result = false;

		public DialogQuestion(string msg)
		{
			Glade.XML gxml = new Glade.XML(Assembly.GetExecutingAssembly(), "DialogQuestion.glade", "DialogQuestionWindow", null);
			gxml.BindFields(this);

			// FIXME use correct icons
			DialogQuestionWindow.IconList = new Gdk.Pixbuf[2] {new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-warning.png"), new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-warning-16.png")};
			DialogQuestionWindow.Title = Catalog.GetString("Question");

			DialogQuestionWindow.DeleteEvent += CloseWindow;
			NoButton.Clicked += CloseWindow;
			YesButton.Clicked += YesAction;

			// FIXME use correct icons
			DialogImage.FromPixbuf = new Gdk.Pixbuf(Assembly.GetEntryAssembly(), "dialog-warning.png");

			Message.Text = msg;
			Message.UseMarkup = true;

			DialogQuestionWindow.ShowAll();
		}

		public bool Run()
		{
			Application.Run();

			return result;
		}

		private void CloseWindow(object o, EventArgs args)
		{
			DialogQuestionWindow.Destroy();
			Application.Quit();
		}

		private void YesAction(object o, EventArgs args)
		{
			result = true;
			DialogQuestionWindow.Destroy();
			Application.Quit();
		}
	}
}
