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
	public class EditNeuronListForm<T> : Dialogue
		where T : RMP_Neuron
	{
		public delegate void GenerateNeuronsEvent(int neuronsToAdd, List<T> neuronList);

		IList<T> neuronList;

		ColumnListBox columnListBox;

		public void Initialize(IList<T> _neuronList, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			neuronList = _neuronList;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditNeuronListForm<T> ShowDialogue(Window window, IList<T> _neuronList, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditNeuronListForm<T>();
			form.Initialize(_neuronList, closeFunction, "Edit Neuron list", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(4);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(false, true, true, true);
			columnListBox.SetColumnName(0, "Name");
			columnListBox.SetColumnName(1, "GeneID");
			columnListBox.SetColumnName(2, "Bias");
			columnListBox.SetColumnName(3, "Connections");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, TakaGUI.DrawBoxes.ColumnListBox.ListBoxRow item, int index)
			{
				T neuron = null;
				foreach (var n in neuronList)
					if (n.ID == (uint)item.ExtraValues[0])
						neuron = n;

				if (neuron != null)
					EditNeuronForm.ShowDialogue(Parent, neuron, delegate(object _sender)
					{
						ReloadListBox();
					});
			};

			ReloadListBox();

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, columnListBox, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var n in neuronList)
			{
				columnListBox.AddRow(n.Name, n.GeneID, n.Bias, n.Connections.Count);
				columnListBox.Values[columnListBox.Values.Count - 1].ExtraValues = new object[] { n.ID };
			}
		}
	}

	public class EditNeuronForm : Dialogue
	{
		RMP_Neuron neuron;

		public void Initialize(RMP_Neuron _neuronGene, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			neuron = _neuronGene;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditNeuronForm ShowDialogue(Window window, RMP_Neuron _neuronGene, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditNeuronForm();
			form.Initialize(_neuronGene, closeFunction, "Edit Neuron", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			var builder = new FieldBuilder();
			builder.BuildSessionStart(this);

			builder.AddLabelField("Name: " + neuron.Name);
			builder.AddLabelField("GeneID: " + neuron.GeneID);

			var activation = builder.AddDoubleField("Activation: ");
			activation.Value = neuron.Activation;

			var bias = builder.AddDoubleField("Bias: ");
			bias.Value = neuron.Bias;

			builder.AddResizableButtonField("Edit Sigmoid Function", delegate(object sender)
			{
				neuron.SigmoidFunction.GetType().InvokeMember("GUI_Edit", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, new object[] { Parent, neuron.SigmoidFunction });
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("Edit Connections", delegate(object sender)
			{
				EditConnectionListForm.ShowDialogue(Parent, neuron.Connections);
			}, FieldBuilder.ResizableButtonOrientation.FillWidth);

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				neuron.Activation = activation.Value;
				neuron.Bias = bias.Value;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}

	public class EditConnectionListForm : Dialogue
	{
		List<RMP_Connection> connections;

		ColumnListBox columnListBox;

		public void Initialize(List<RMP_Connection> _connections, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			connections = _connections;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditConnectionListForm ShowDialogue(Window window, List<RMP_Connection> _connectionGene, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditConnectionListForm();
			form.Initialize(_connectionGene, closeFunction, "Edit Connection list", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(4);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(true, true, true, true);
			columnListBox.SetColumnName(0, "ID");
			columnListBox.SetColumnName(1, "Source");
			columnListBox.SetColumnName(1, "Target");
			columnListBox.SetColumnName(3, "Weight");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, TakaGUI.DrawBoxes.ColumnListBox.ListBoxRow item, int index)
			{
				RMP_Connection connection = null;
				foreach (var c in connections)
					if (c.ID == (uint)item.Values[0])
						connection = c;

				if (connection != null)
					EditConnectionForm.ShowDialogue(Parent, connection, delegate(object _sender)
					{
						ReloadListBox();
					});
			};

			ReloadListBox();

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, columnListBox, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var c in connections)
			{
				columnListBox.AddRow(c.ID, c.Source.GeneID, c.Target.GeneID, c.Weight);
			}
		}
	}

	public class EditConnectionForm : Dialogue
	{
		RMP_Connection connection;

		public void Initialize(RMP_Connection _connection, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			connection = _connection;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditConnectionForm ShowDialogue(Window window, RMP_Connection _connection, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditConnectionForm();
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

			builder.AddLabelField("Name: " + connection.Name);
			builder.AddLabelField("Source: " + connection.Source.GeneID);
			builder.AddLabelField("Target: " + connection.Target.GeneID);

			var weightField = builder.AddDoubleField("Weight:");
			weightField.Value = connection.Weight;

			builder.AddResizableButtonField("OK", delegate(object sender)
			{
				connection.Weight = weightField.Value;

				Close();
			}, FieldBuilder.ResizableButtonOrientation.Left);

			builder.BuildSessionEnd();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}
	}
}
