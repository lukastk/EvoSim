using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TakaGUI.DrawBoxes.Forms;
using TakaGUI.DrawBoxes;
using TakaGUI;
using TakaGUI.Data;
using EvoSim;

namespace EvoApp.Menu.Templates.WorldTemplateForms
{
	public class ManageWorldTemplatesForm : Dialogue
	{
		ColumnListBox columnListBox;

		public static ManageWorldTemplatesForm ShowDialogue(Window window, CloseEvent closeFunction = null, string category = null, ISkinFile file = null)
		{
			var form = new ManageWorldTemplatesForm();
			form.Initialize(closeFunction, "Manage World-Templates", true, true, category, file);
			form.Show(window);

			return form;
		}

		public override void AddedToContainer()
		{
			base.AddedToContainer();

			CloseButtonOn = false;

			columnListBox = new ColumnListBox();
			columnListBox.Initialize(1);
			AddDrawBox(columnListBox);
			columnListBox.SetIntOrStringSort(false);
			columnListBox.SetColumnName(0, "Name");
			columnListBox.Width = 200;
			columnListBox.Height = 200;

			columnListBox.ItemDoubleClicked += delegate(object sender, ColumnListBox.ListBoxRow item, int index)
			{
				WorldTemplate template = null;
				foreach (var g in EditorData.WorldTemplates)
					if (g.TemplateName == (string)item.Values[0])
						template = g;

				string oldName = template.TemplateName;
				EditWorldTemplateForm.ShowDialogue(Parent, template, delegate(object _sender)
				{
					foreach (var g in EditorData.WorldTemplates)
						if (g.TemplateName == template.TemplateName && g != template)
						{
							AlertForm.ShowDialogue(Parent, null, "There is a template with that name already.");
							template.TemplateName = oldName;
						}

					ReloadListBox();
				});
			};

			ReloadListBox();

			var createTemplateButton = new ResizableButton();
			createTemplateButton.Initialize();
			AddDrawBox(createTemplateButton);
			createTemplateButton.Title = "Create New Template";
			createTemplateButton.FitToText();
			Push.ToTheBottomSideOf(createTemplateButton, columnListBox, 3, Push.VerticalAlign.Left);
			createTemplateButton.Width = 200;
			createTemplateButton.Click += delegate(object sender)
			{
				CreateWorldTemplateForm.ShowDialogue(Parent, delegate(object _sender)
				{
					var dialogue = (CreateWorldTemplateForm)_sender;

					if (dialogue.Result != null)
					{
						bool nameExists = false;
						foreach (var t in EditorData.WorldTemplates)
						{
							if (t.TemplateName == dialogue.Result.TemplateName)
							{
								AlertForm.ShowDialogue(Parent, null, "A template called: \"" + dialogue.Result.TemplateName + "\" already exists");
								nameExists = true;
							}
						}

						if (!nameExists)
						{
							EditorData.WorldTemplates.Add(dialogue.Result);
							ReloadListBox();
						}
					}
				});
			};

			var deleteTemplateButton = new ResizableButton();
			deleteTemplateButton.Initialize();
			AddDrawBox(deleteTemplateButton);
			deleteTemplateButton.Title = "Delete Template";
			deleteTemplateButton.FitToText();
			Push.ToTheBottomSideOf(deleteTemplateButton, createTemplateButton, 3, Push.VerticalAlign.Left);
			deleteTemplateButton.Width = 200;
			deleteTemplateButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRow != null)
				{
					var findName = (string)columnListBox.SelectedRow.Values[0];

					foreach (var g in EditorData.WorldTemplates)
					{
						if (g.TemplateName == findName)
						{
							EditorData.WorldTemplates.Remove(g);
							ReloadListBox();
							break;
						}
					}
				}
			};

			var editTemplateButton = new ResizableButton();
			editTemplateButton.Initialize();
			AddDrawBox(editTemplateButton);
			editTemplateButton.Title = "Edit Template";
			editTemplateButton.FitToText();
			Push.ToTheBottomSideOf(editTemplateButton, deleteTemplateButton, 3, Push.VerticalAlign.Left);
			editTemplateButton.Width = 200;
			editTemplateButton.Click += delegate(object sender)
			{
				if (columnListBox.SelectedRow == null)
					return;

				WorldTemplate template = null;
				foreach (var g in EditorData.WorldTemplates)
					if (g.TemplateName == (string)columnListBox.SelectedRow.Values[0])
						template = g;

				string oldName = template.TemplateName;
				EditWorldTemplateForm.ShowDialogue(Parent, template, delegate(object _sender)
				{
					foreach (var g in EditorData.WorldTemplates)
						if (g.TemplateName == template.TemplateName && g != template)
						{
							AlertForm.ShowDialogue(Parent, null, "There is a template with that name already.");
							template.TemplateName = oldName;
						}

					ReloadListBox();
				});
			};

			var okButton = new ResizableButton();
			okButton.Initialize();
			AddDrawBox(okButton);
			okButton.Title = "OK";
			okButton.FitToText();
			Push.ToTheBottomSideOf(okButton, editTemplateButton, 3, Push.VerticalAlign.Left);
			okButton.Width = 200;
			okButton.Click += delegate(object sender)
			{
				EditorData.Save(Globals.EditorDataSaveDir);
				Close();
			};

			Wrap();

			columnListBox.Alignment = DrawBoxAlignment.GetFull();
			createTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			deleteTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			editTemplateButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();
			okButton.Alignment = DrawBoxAlignment.GetLeftRightBottom();

			X = (Parent.Width / 2) - (Width / 2);
			Y = (Parent.Height / 2) - (Height / 2);
		}

		void ReloadListBox()
		{
			columnListBox.ClearRows();
			foreach (var g in EditorData.WorldTemplates)
				columnListBox.AddRow(g.TemplateName);
		}
	}
}
