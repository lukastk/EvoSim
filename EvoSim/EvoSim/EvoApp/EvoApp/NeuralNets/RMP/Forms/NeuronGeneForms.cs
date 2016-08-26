using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI;
using TakaGUI.Data;
using TakaGUI.DrawBoxes;
using System.Reflection;

namespace EvoApp.NeuralNets.RMP.Forms
{
	public class EditChromosomeNeuronsForm : Dialogue
	{
		Dictionary<string, object> valueHolder;
		RMP_Chromosome chromosome;

		/// <summary>
		/// Not Implemented.
		/// </summary>
		/// <param name="closeFunction"></param>
		/// <param name="title"></param>
		/// <param name="resizable"></param>
		/// <param name="isDialog"></param>
		/// <param name="category"></param>
		/// <param name="file"></param>
		public override void Initialize(CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			throw new NotImplementedException();
		}

		public void Initialize(Dictionary<string, object> _valueHolder, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			valueHolder = _valueHolder;

			chromosome = (RMP_Chromosome)valueHolder["RMP_Chromosome"];

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditChromosomeNeuronsForm ShowDialogue(Window window, Dictionary<string, object> valueHolder, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditChromosomeNeuronsForm();
			form.Initialize(valueHolder, closeFunction, "Edit Chromosome-Neurons", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			EditNeuronGeneListForm.GenerateNeuronsEvent addNeuronsDelegate = delegate(int neuronsToAdd, List<RMP_NeuronGene> neuronList)
			{
				AddRandomNeurons(neuronList, neuronsToAdd);
			};

			builder.AddResizableButtonField("Edit Red Eye-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.EyeRNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Green Eye-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.EyeGNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Blue Eye-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.EyeBNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Distance Eye-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.DistanceNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddVerticalMargin(4);

			builder.AddResizableButtonField("Edit Input-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.InputNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Hidden-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.HiddenNeuronGenes, addNeuronsDelegate);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Output-Neurons", delegate(object sender)
			{
				EditNeuronGeneListForm.ShowDialogue(Parent, chromosome.OutputNeuronGenes, delegate(int neuronsToAdd, List<RMP_NeuronGene> neuronList)
				{
					AddRandomOutputNeurons(neuronList, neuronsToAdd);
				});
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddVerticalMargin(5);

			builder.AddResizableButtonField("Connect Randomly", delegate(object sender)
			{
				chromosome.AllNeuronsConnectRandomly();
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);

			CanResizeFormVertically = false;
		}

		void AddRandomNeurons(List<RMP_NeuronGene> neuronList, int neuronsToAdd)
		{
			for (int i = 0; i < neuronsToAdd; i++)
				neuronList.Add(chromosome.GetRandomNeuronGene());
		}
		void AddRandomOutputNeurons(List<RMP_NeuronGene> neuronList, int neuronsToAdd)
		{
			for (int i = 0; i < neuronsToAdd; i++)
				neuronList.Add(chromosome.GetRandomOutputNeuronGene());
		}
	}

	public class EditNeuronGeneListForm : Dialogue
	{
		public delegate void GenerateNeuronsEvent(int neuronsToAdd, List<RMP_NeuronGene> neuronList);

		List<RMP_NeuronGene> neuronList;

		ColumnListBox columnListBox;

		public event GenerateNeuronsEvent GenerateNeurons;

		public void Initialize(List<RMP_NeuronGene> _neuronList, GenerateNeuronsEvent generateNeurons, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			neuronList = _neuronList;
			GenerateNeurons += generateNeurons;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditNeuronGeneListForm ShowDialogue(Window window, List<RMP_NeuronGene> _neuronList, GenerateNeuronsEvent generateNeurons, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditNeuronGeneListForm();
			form.Initialize(_neuronList, generateNeurons, closeFunction, "Edit Neuron-gene list", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(5);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(true, false, true, true, true);
			columnListBox.SetColumnName(0, "Order");
			columnListBox.SetColumnName(1, "Name");
			columnListBox.SetColumnName(2, "ID");
			columnListBox.SetColumnName(3, "Bias");
			columnListBox.SetColumnName(4, "Connections");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, TakaGUI.DrawBoxes.ColumnListBox.ListBoxRow item, int index)
			{
				RMP_NeuronGene neuronGene = null;
				foreach (var gene in neuronList)
					if (gene.ID == (uint)item.Values[2])
						neuronGene = gene;

				if (neuronGene != null)
					EditNeuronGeneForm.ShowDialogue(Parent, neuronGene, delegate(object _sender)
					{
						ReloadListBox();
					});
			};

			ReloadListBox();

			var label = new Label("Neurons To Add: ");
			label.Initialize();
			AddDrawBox(label);
			Push.ToTheBottomSideOf(label, columnListBox, 5, Push.VerticalAlign.Left);

			var neuronsToAddIntegerField = new IntegerField();
			neuronsToAddIntegerField.Initialize();
			AddDrawBox(neuronsToAddIntegerField);
			Push.ToTheRightSideOf(neuronsToAddIntegerField, label, 3, Push.HorizontalAlign.Top);
			neuronsToAddIntegerField.Width = 200 - neuronsToAddIntegerField.X;

			var generateNeuronsButton = new ResizableButton();
			generateNeuronsButton.Initialize();
			AddDrawBox(generateNeuronsButton);
			generateNeuronsButton.Title = "Generate Neurons";
			generateNeuronsButton.FitToText();
			Push.ToTheBottomSideOf(generateNeuronsButton, label, 5, Push.VerticalAlign.Left);
			generateNeuronsButton.Width = 200;
			generateNeuronsButton.Click += delegate(object sender)
			{
				GenerateNeurons((int)neuronsToAddIntegerField.Value, neuronList);
				ReloadListBox();
			};

			var deleteNeuronsButton = new ResizableButton();
			deleteNeuronsButton.Initialize();
			AddDrawBox(deleteNeuronsButton);
			deleteNeuronsButton.Title = "Delete Neurons";
			deleteNeuronsButton.FitToText();
			Push.ToTheBottomSideOf(deleteNeuronsButton, generateNeuronsButton, 5, Push.VerticalAlign.Left);
			deleteNeuronsButton.Width = 200;
			deleteNeuronsButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRowIndex == -1)
					return;

				uint searchId = (uint)columnListBox.Values[columnListBox.SelectedRowIndex].Values[2];

				foreach (var gene in neuronList)
					if (gene.ID == searchId)
					{
						neuronList.Remove(gene);
						break;
					}

				ReloadListBox();
			};

			var moveUpButton = new ResizableButton();
			moveUpButton.Initialize();
			AddDrawBox(moveUpButton);
			moveUpButton.Title = "Move Up";
			moveUpButton.FitToText();
			Push.ToTheBottomSideOf(moveUpButton, deleteNeuronsButton, 3, Push.VerticalAlign.Left);
			moveUpButton.Width = 200;
			moveUpButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRowIndex < 1)
					return;

				int index = columnListBox.SelectedRowIndex;

				var selected = neuronList[index];
				var upper = neuronList[index - 1];
				neuronList[index - 1] = selected;
				neuronList[index] = upper;

				ReloadListBox();

				columnListBox.SelectedRowIndex = index - 1;
			};

			var moveDownButton = new ResizableButton();
			moveDownButton.Initialize();
			AddDrawBox(moveDownButton);
			moveDownButton.Title = "Move Down";
			moveDownButton.FitToText();
			Push.ToTheBottomSideOf(moveDownButton, moveUpButton, 3, Push.VerticalAlign.Left);
			moveDownButton.Width = 200;
			moveDownButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRowIndex == -1 ||
					columnListBox.SelectedRowIndex == columnListBox.Values.Count - 1)
					return;

				int index = columnListBox.SelectedRowIndex;

				var selected = neuronList[index];
				var lower = neuronList[index + 1];
				neuronList[index + 1] = selected;
				neuronList[index] = lower;

				ReloadListBox();

				columnListBox.SelectedRowIndex = index + 1;
			};

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, moveDownButton, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			label.Alignment = DrawBoxAlignment.GetLeftBottom();
			neuronsToAddIntegerField.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			generateNeuronsButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			deleteNeuronsButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			moveUpButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			moveDownButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var gene in neuronList)
				columnListBox.AddRow(neuronList.IndexOf(gene), gene.Name, gene.ID, gene.Bias.Value, gene.Connections.Count);
		}
	}

	public class EditNeuronGeneForm : Dialogue
	{
		RMP_NeuronGene neuronGene;

		TextField nameField;
		CheckBox isMutableCheckBox;
		DoubleField biasMin;
		DoubleField biasMax;
		DoubleField biasVal;

		public void Initialize(RMP_NeuronGene _neuronGene, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			neuronGene = _neuronGene;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditNeuronGeneForm ShowDialogue(Window window, RMP_NeuronGene _neuronGene, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditNeuronGeneForm();
			form.Initialize(_neuronGene, closeFunction, "Edit Neuron-gene", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			nameField = builder.AddTextField("Name: ");
			nameField.Text = neuronGene.Name;

			isMutableCheckBox = builder.AddCheckBoxField("Gene Mutability: ");
			isMutableCheckBox.Checked = neuronGene.IsMutable;

			builder.AddLabelField("Bias:");
			biasMin = builder.AddDoubleField("Min: ");
			biasMax = builder.AddDoubleField("Max: ");
			biasVal = builder.AddDoubleField("Value: ");
			

			biasMin.Value = neuronGene.Bias.Min;
			biasMax.Value = neuronGene.Bias.Max;
			biasVal.Value = neuronGene.Bias.Value;

			builder.AddResizableButtonField("Edit Sigmoid Function", delegate(object sender)
			{
				neuronGene.SigmoidFunction.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { Parent, neuronGene.SigmoidFunction });
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Connections", delegate(object sender)
			{
				EditConnectionGeneListForm.ShowDialogue(Parent, neuronGene.Connections);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		public override void Close()
		{
			neuronGene.Name = nameField.Text;
			neuronGene.IsMutable = isMutableCheckBox.Checked;

			neuronGene.Bias = new EvoSim.DoubleMinMax(biasMin.Value, biasMax.Value);
			neuronGene.Bias.Value = biasVal.Value;

			base.Close();
		}
	}

	public class EditConnectionGeneListForm : Dialogue
	{
		List<RMP_ConnectionGene> connections;

		ColumnListBox columnListBox;

		public void Initialize(List<RMP_ConnectionGene> _connections, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			connections = _connections;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditConnectionGeneListForm ShowDialogue(Window window, List<RMP_ConnectionGene> _connectionGene, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditConnectionGeneListForm();
			form.Initialize(_connectionGene, closeFunction, "Edit Connection-gene list", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(4);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(true, true, false, true);
			columnListBox.SetColumnName(0, "ID");
			columnListBox.SetColumnName(1, "Target");
			columnListBox.SetColumnName(2, "IsOutputConnection");
			columnListBox.SetColumnName(3, "Weight");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, TakaGUI.DrawBoxes.ColumnListBox.ListBoxRow item, int index)
			{
				RMP_ConnectionGene connectionGene = null;
				foreach (var gene in connections)
					if (gene.ID == (uint)item.Values[0])
						connectionGene = gene;

				if (connectionGene != null)
					EditConnectionGeneForm.ShowDialogue(Parent, connectionGene, delegate(object _sender)
					{
						ReloadListBox();
					});
			};

			ReloadListBox();

			var label = new Label("Connections To Add: ");
			label.Initialize();
			AddDrawBox(label);
			Push.ToTheBottomSideOf(label, columnListBox, 5, Push.VerticalAlign.Left);

			var connectionsToAddIntegerField = new IntegerField();
			connectionsToAddIntegerField.Initialize();
			AddDrawBox(connectionsToAddIntegerField);
			Push.ToTheRightSideOf(connectionsToAddIntegerField, label, 3, Push.HorizontalAlign.Top);
			connectionsToAddIntegerField.Width = 200 - connectionsToAddIntegerField.X;

			var generateConnectionsButton = new ResizableButton();
			generateConnectionsButton.Initialize();
			AddDrawBox(generateConnectionsButton);
			generateConnectionsButton.Title = "Generate Connections";
			generateConnectionsButton.FitToText();
			Push.ToTheBottomSideOf(generateConnectionsButton, label, 5, Push.VerticalAlign.Left);
			generateConnectionsButton.Width = 200;
			generateConnectionsButton.Click += delegate(object sender)
			{
				GenerateConnections((int)connectionsToAddIntegerField.Value);
				ReloadListBox();
			};

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, generateConnectionsButton, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			label.Alignment = DrawBoxAlignment.GetLeftBottom();
			connectionsToAddIntegerField.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			generateConnectionsButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var gene in connections)
				columnListBox.AddRow(gene.ID, gene.Target, gene.IsOutputConnection.ToString(), gene.Weight.Value);
		}

		void GenerateConnections(int amount)
		{
			for (int i = 0; i < amount; i++)
				connections.Add(new RMP_ConnectionGene());
		}
	}

	public class EditConnectionGeneForm : Dialogue
	{
		RMP_ConnectionGene connection;

		IntegerField targetIntegerField;
		CheckBox isOutputConnectionCheckBox;
		CheckBox isMutableCheckBox;

		DoubleField weightMin;
		DoubleField weightMax;
		DoubleField weightVal;

		public void Initialize(RMP_ConnectionGene _connection, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			connection = _connection;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditConnectionGeneForm ShowDialogue(Window window, RMP_ConnectionGene _connection, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditConnectionGeneForm();
			form.Initialize(_connection, closeFunction, "Edit Connection-gene", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			targetIntegerField = builder.AddIntegerField("Target: ");
			targetIntegerField.Value = connection.Target;

			isOutputConnectionCheckBox = builder.AddCheckBoxField("Is Output-Connection: ");
			isOutputConnectionCheckBox.Checked = connection.IsOutputConnection;

			isMutableCheckBox = builder.AddCheckBoxField("Connection Mutability: ");
			isMutableCheckBox.Checked = connection.IsMutable;

			builder.AddLabelField("Weight:");
			weightMin = builder.AddDoubleField("Min: ");
			weightMax = builder.AddDoubleField("Max: ");
			weightVal = builder.AddDoubleField("Value: ");

			weightMin.Value = connection.Weight.Min;
			weightMax.Value = connection.Weight.Max;
			weightVal.Value = connection.Weight.Value;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		public override void Close()
		{
			connection.Target = (int)targetIntegerField.Value;
			connection.IsOutputConnection = isOutputConnectionCheckBox.Checked;
			connection.IsMutable = isMutableCheckBox.Checked;

			connection.Weight = new EvoSim.DoubleMinMax(weightMin.Value, weightMax.Value);
			connection.Weight.Value = weightVal.Value;

			base.Close();
		}
	}
}
