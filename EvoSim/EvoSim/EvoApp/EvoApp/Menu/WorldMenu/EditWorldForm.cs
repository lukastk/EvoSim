using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using EvoSim;
using TakaGUI.DrawBoxes;
using System.Reflection;
using EvoApp.NeuralNets.RMP;
using EvoSim.Genes;

namespace EvoApp.Menu.WorldMenu
{
	public class EditWorldForm : Dialogue
	{
		IWorld world;

		Dictionary<Type, Dictionary<string, object>> valueHolders = new Dictionary<Type, Dictionary<string, object>>();

		public void Initialize(IWorld _world, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			world = _world;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditWorldForm ShowDialogue(Window window, IWorld _world, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditWorldForm();
			form.Initialize(_world, closeFunction, "Edit World-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			Panel panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			var isReadyFunc = (Func<bool>)world.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { panel, world });

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (isReadyFunc())
					Close();
				else
					AlertForm.ShowDialogue(Parent, null, "All fields have not been filled out.");
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
