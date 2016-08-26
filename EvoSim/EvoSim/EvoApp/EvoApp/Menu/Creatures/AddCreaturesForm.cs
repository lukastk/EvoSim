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
using EvoExt.NeuralNets.RMP;
using EvoSim.Genes;

namespace EvoApp.Menu.Creatures
{
	public class AddCreaturesForm : Dialogue
	{
		IWorld world;

		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(IWorld _world, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			world = _world;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static AddCreaturesForm ShowDialogue(Window window, IWorld _world, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new AddCreaturesForm();
			form.Initialize(_world, closeFunction, "Create ANN-Template", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();

			builder.BuildSessionStart(this);

			var genomeTemplatesComboBox = builder.AddComboBoxField("Genome: ");
			genomeTemplatesComboBox.Items.AddRange(EditorData.GenomeTemplates.Select(s => s.TemplateName));
			genomeTemplatesComboBox.Index = 0;

			var annTemplatesComboBox = builder.AddComboBoxField("ANN: ");
			annTemplatesComboBox.Items.AddRange(EditorData.ANNTemplates.Select(s => s.TemplateName));
			annTemplatesComboBox.Index = 0;

			var creatureTemplatesComboBox = builder.AddComboBoxField("Creature: ");
			creatureTemplatesComboBox.Items.AddRange(EditorData.CreatureTemplates.Select(s => s.TemplateName));
			creatureTemplatesComboBox.Index = 0;

			var mutateCheckBox = builder.AddCheckBoxField("Mutate the genome: ");

			var amountField = builder.AddIntegerField("Amount to add: ");
			amountField.MinValue = 1;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				string name = genomeTemplatesComboBox.Items[genomeTemplatesComboBox.Index];
				Genome genome = null;
				foreach (var g in EditorData.GenomeTemplates)
					if (g.TemplateName == name)
						genome = g.Genome;

				name = annTemplatesComboBox.Items[annTemplatesComboBox.Index];
				INeuralNetChromosome ann = null;
				foreach (var a in EditorData.ANNTemplates)
					if (a.TemplateName == name)
						ann = a.ANN;

				name = creatureTemplatesComboBox.Items[creatureTemplatesComboBox.Index];
				ICreature creature = null;
				foreach (var c in EditorData.CreatureTemplates)
					if (c.TemplateName == name)
						creature = c.Creature;

				for (int i = 0; i < amountField.Value; i++)
				{
					var clone = (ICreature)creature.Clone();
					var cloneGenome = genome.Clone();
					cloneGenome.SetNetChromosome(ann.Clone());

					if (mutateCheckBox.Checked)
						cloneGenome.Mutate();

					clone.ImprintGenome(cloneGenome);

					world.AddEntity(clone);

					clone.Position = new EntityPosition(Globals.Random.Next(world.Width), Globals.Random.Next(world.Height));
				}
			});

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}
	}
}
