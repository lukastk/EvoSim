using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;

namespace EvoApp.Forms
{
	class EditIntForm : Dialogue
	{
		public int Result;

		public void Initialize(int value, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			Result = value;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditIntForm ShowDialogue(Window window, int value, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditIntForm();
			form.Initialize(value, closeFunction, "Edit Int", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var valueField = builder.AddIntegerField("Value: ");
			valueField.Value = Result;

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result = (int)valueField.Value;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}

	class EditByteForm : Dialogue
	{
		public byte Result;

		public void Initialize(byte value, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			Result = value;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditByteForm ShowDialogue(Window window, byte value, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditByteForm();
			form.Initialize(value, closeFunction, "Edit Byte", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var valueField = builder.AddIntegerField("Value: ");
			valueField.Value = Result;

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result = (byte)valueField.Value;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}

	class EditDoubleForm : Dialogue
	{
		public double Result;

		public void Initialize(double value, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			Result = value;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditDoubleForm ShowDialogue(Window window, double value, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditDoubleForm();
			form.Initialize(value, closeFunction, "Edit Double", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var valueField = builder.AddDoubleField("Value: ");
			valueField.Value = Result;

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result = valueField.Value;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}

	class EditBoolForm : Dialogue
	{
		public bool Result;

		public void Initialize(bool value, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			Result = value;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditBoolForm ShowDialogue(Window window, bool value, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditBoolForm();
			form.Initialize(value, closeFunction, "Edit Bool", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var valueField = builder.AddCheckBoxField("Value: ");
			valueField.Checked = Result;

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Result = valueField.Checked;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			CanResizeFormVertically = false;

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}
}
