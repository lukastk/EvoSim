using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using EvoSim.Genes;
using TakaGUI;
using TakaGUI.DrawBoxes;
using TakaGUI.Data;

namespace EvoApp.Forms
{
	public class EditDoubleGeneListForm : Dialogue
	{
		ColumnListBox columnListBox;
		List<DoubleGene> geneList;

		public void Initialize(List<DoubleGene> _geneList, CloseEvent closeFunction = null, string title = null, bool resizable = false, bool isDialog = true, string category = null, ISkinFile file = null)
		{
			geneList = _geneList;

			base.Initialize(closeFunction, title, resizable, isDialog, category, file);
		}

		public static EditDoubleGeneListForm ShowDialogue(Window window, List<DoubleGene> _geneList, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new EditDoubleGeneListForm();
			form.Initialize(_geneList, closeFunction, "Edit Gene", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(7);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(true, false, true, true, true, true, false);
			columnListBox.SetColumnName(0, "Order");
			columnListBox.SetColumnName(1, "Name");
			columnListBox.SetColumnName(2, "ID");
			columnListBox.SetColumnName(3, "Value");
			columnListBox.SetColumnName(4, "Min");
			columnListBox.SetColumnName(5, "Max");
			columnListBox.SetColumnName(6, "IsMutable");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, ColumnListBox.ListBoxRow item, int index)
			{
				int n = 0;
				foreach (var g in geneList)
				{
					if (g.ID == (uint)item.Values[2])
						break;
					n++;
				}

				EditDoubleGeneForm.ShowDialogue(Parent, geneList[n], delegate(object _sender)
				{
					geneList[n] = ((EditDoubleGeneForm)_sender).Result;
					ReloadListBox();
				});
			};

			ReloadListBox();

			var moveUpButton = new ResizableButton();
			moveUpButton.Initialize();
			AddDrawBox(moveUpButton);
			moveUpButton.Title = "Move Up";
			moveUpButton.FitToText();
			Push.ToTheBottomSideOf(moveUpButton, columnListBox, 3, Push.VerticalAlign.Left);
			moveUpButton.Width = 200;
			moveUpButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRowIndex < 1)
					return;

				int index = columnListBox.SelectedRowIndex;

				var selected = geneList[index];
				var upper = geneList[index - 1];
				geneList[index - 1] = selected;
				geneList[index] = upper;

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

				var selected = geneList[index];
				var lower = geneList[index + 1];
				geneList[index + 1] = selected;
				geneList[index] = lower;

				ReloadListBox();

				columnListBox.SelectedRowIndex = index + 1;
			};

			var createGeneButton = new ResizableButton();
			createGeneButton.Initialize();
			AddDrawBox(createGeneButton);
			createGeneButton.Title = "Create New Gene";
			createGeneButton.FitToText();
			Push.ToTheBottomSideOf(createGeneButton, moveDownButton, 3, Push.VerticalAlign.Left);
			createGeneButton.Width = 200;
			createGeneButton.Click += delegate(object sender)
			{
				var dGene = new DoubleGene();
				dGene.SetMinMaxValue(0, 1, 0);
				dGene.IsMutable = true;

				EditDoubleGeneForm.ShowDialogue(Parent, dGene, delegate(object _sender)
				{
					geneList.Add(((EditDoubleGeneForm)_sender).Result);
					ReloadListBox();
				});
			};

			var deleteGeneButton = new ResizableButton();
			deleteGeneButton.Initialize();
			AddDrawBox(deleteGeneButton);
			deleteGeneButton.Title = "Delete Gene";
			deleteGeneButton.FitToText();
			Push.ToTheBottomSideOf(deleteGeneButton, createGeneButton, 3, Push.VerticalAlign.Left);
			deleteGeneButton.Width = 200;
			deleteGeneButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRow != null)
				{
					var findID = (uint)columnListBox.SelectedRow.Values[2];

					int n = 0;
					foreach (var g in geneList)
					{
						if (g.ID == findID)
						{
							geneList.RemoveAt(n);
							ReloadListBox();
							break;
						}
						n++;
					}
				}
			};

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, deleteGeneButton, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			moveUpButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			moveDownButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			createGeneButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			deleteGeneButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			int order = 0;
			
			columnListBox.ClearRows();
			foreach (var g in geneList)
				columnListBox.AddRow(order++, g.Name, g.ID, g.Value, g.Min, g.Max, g.IsMutable);
		}
	}
}
