using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;

namespace EvoApp.NeuralNets.RMP.Forms
{
	public class RandomizeForm : Dialogue
	{
		RMP_Chromosome chromosome;

		public void Initialize(RMP_Chromosome _chromosome, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			chromosome = _chromosome;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static RandomizeForm ShowDialogue(Window window, RMP_Chromosome _chromosome, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new RandomizeForm();
			form.Initialize(_chromosome, closeFunction, "Randomize RMP-Chromosome", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			var eye = builder.AddIntegerField("Eye Neurons: ");
			var input = builder.AddIntegerField("Input Neurons: ");
			var output = builder.AddIntegerField("Output Neurons: ");
			var hidden = builder.AddIntegerField("Hidden Neurons: ");

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				chromosome.CreateRandom((int)eye.Value, (int)input.Value, (int)output.Value, (int)hidden.Value);
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Right);

			builder.MoveUpOneField();

			builder.AddResizableButtonField("Abort", delegate(object sender)
			{
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
