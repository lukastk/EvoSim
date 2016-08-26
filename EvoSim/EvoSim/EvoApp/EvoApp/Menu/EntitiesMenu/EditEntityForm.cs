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

namespace EvoApp.Menu.EntitiesMenu
{
	public class EditEntityForm : Dialogue
	{
		IEntity entity;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IEntity _entity, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			entity = _entity;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditEntityForm ShowDialogue(Window window, IEntity _entity, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditEntityForm();
			form.Initialize(_entity, closeFunction, "Edit Entity", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var panel = new Panel();
			panel.Initialize();
			builder.AddDrawBoxAsField(panel, DrawBoxAlignment.GetFull());

			var guiMethod = entity.GetType().GetMethod("GUI_Edit");
			var isReadyFunc = (Func<bool>)guiMethod.Invoke(entity, new object[] { panel });
			panel.Width = builder.FieldWidth;

			Wrap();

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			okButton.X = builder.FieldWidth - okButton.Width;
			okButton.Y = Height + 5;
			okButton.Click += delegate(object sender)
			{
				if (isReadyFunc())
				{
					Close();
				}
				else
				{
					AlertForm.ShowDialogue(Parent, null, "All fields have not been filled out.");
				}
			};

			builder.BuildSessionEnd();

			okButton.Alignment = DrawBoxAlignment.GetRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = true;
		}
	}
}
