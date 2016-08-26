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
	public class EditCreatureForm : Dialogue
	{
		ICreature creature;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(ICreature _creature, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			creature = _creature;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditCreatureForm ShowDialogue(Window window, ICreature _creature, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditCreatureForm();
			form.Initialize(_creature, closeFunction, "Edit Creature", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			builder.AddResizableButtonField("Edit ANN", delegate(object sender)
			{
				EditANNForm.ShowDialogue(Parent, creature.Brain);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);
			builder.AddResizableButtonField("Edit ANN-Chromosome", delegate(object sender)
			{
				EditANNChromosomeForm.ShowDialogue(Parent, creature.CreatureGenome.NetChromosome);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);
			builder.AddResizableButtonField("Edit Genome", delegate(object sender)
			{
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);
			builder.AddResizableButtonField("Edit Body", delegate(object sender)
			{
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}

	public class EditANNForm : Dialogue
	{
		INeuralNet ann;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(INeuralNet _ann, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			ann = _ann;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditANNForm ShowDialogue(Window window, INeuralNet _ann, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditANNForm();
			form.Initialize(_ann, closeFunction, "Edit ANN", true, true, category, file);
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

			var guiMethod = ann.GetType().GetMethod("GUI_Edit");
			var isReadyFunc = (Func<bool>)guiMethod.Invoke(ann, new object[] { panel });

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

	public class EditANNChromosomeForm : Dialogue
	{
		INeuralNetChromosome chromosome;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(INeuralNetChromosome _chromosome, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			chromosome = _chromosome;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditANNChromosomeForm ShowDialogue(Window window, INeuralNetChromosome _chromosome, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditANNChromosomeForm();
			form.Initialize(_chromosome, closeFunction, "Edit ANN-Chromosome", true, true, category, file);
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

			var isReadyFunc = (Func<bool>)chromosome.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { panel, chromosome });

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
