using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.DrawBoxes;
using System.Reflection;
using EvoSim;

namespace EvoApp.Menu.FileMenu
{
	public class NewSimulationForm : Dialogue
	{
		public Simulation Result;

		public static NewSimulationForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new NewSimulationForm();
			form.Initialize(closeFunction, "New Simulation", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var fileNameTextField = builder.AddBrowseField(FileForm.FileFormTypes.File, FileForm.OperationTypes.Save, "File Directory: ").DrawBox1;

			var worldTemplatesComboBox = builder.AddComboBoxField("World: ");
			worldTemplatesComboBox.Items.AddRange(EditorData.WorldTemplates.Select(s => s.TemplateName));
			worldTemplatesComboBox.Index = 0;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				if (worldTemplatesComboBox.Index == -1)
				{
					AlertForm.ShowDialogue(Parent, null, "Choose a world-template");
				}
				else
				{
					Result = new Simulation();
					Result.World = EditorData.WorldTemplates[worldTemplatesComboBox.Index].World.Clone();
					Result.FileName = fileNameTextField.Text;

					AddDefaultEntityPainters(Result);

					Close();
				}
			});

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}

		void AddDefaultEntityPainters(Simulation sim)
		{
			EntityPainters.IEntityPainter entityPainter = new EntityPainters.DefaultEntityPainter();
			entityPainter.Initialize(DrawBox.DefaultSkinFile);
			entityPainter.EntityTypeList.Add(typeof(IEntity));
			entityPainter.ExcludedEntityTypeList.Add(typeof(ICreature));
			sim.EntityPainters.Add(entityPainter);

			entityPainter = new EntityPainters.DefaultCreaturePainter();
			entityPainter.Initialize(DrawBox.DefaultSkinFile);
			entityPainter.EntityTypeList.Add(typeof(ICreature));
			sim.EntityPainters.Add(entityPainter);
		}
	}
}
