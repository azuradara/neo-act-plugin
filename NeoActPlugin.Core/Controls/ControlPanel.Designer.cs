using System.Windows.Forms;

namespace NeoActPlugin.Core
{
    partial class ControlPanel
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanel));
            this.tabPageMain = new System.Windows.Forms.TabPage();
            this.label_ListEmpty = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.logBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonClearLog = new System.Windows.Forms.Button();
            this.checkBoxFollowLog = new System.Windows.Forms.CheckBox();
            this.regionPicker = new System.Windows.Forms.ComboBox();
            this.tabPageMain.SuspendLayout();
            this.flowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPageMain
            // 
            this.tabPageMain.Controls.Add(this.label_ListEmpty);
            resources.ApplyResources(this.tabPageMain, "tabPageMain");
            this.tabPageMain.Name = "tabPageMain";
            this.tabPageMain.UseVisualStyleBackColor = true;
            // 
            // label_ListEmpty
            // 
            resources.ApplyResources(this.label_ListEmpty, "label_ListEmpty");
            this.label_ListEmpty.Name = "label_ListEmpty";
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // logBox
            // 
            this.logBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            resources.ApplyResources(this.logBox, "logBox");
            this.logBox.HideSelection = false;
            this.logBox.Name = "logBox";
            this.logBox.ReadOnly = true;
            // 
            // flowLayoutPanel
            // 
            resources.ApplyResources(this.flowLayoutPanel, "flowLayoutPanel");
            this.flowLayoutPanel.Controls.Add(this.buttonClearLog);
            this.flowLayoutPanel.Controls.Add(this.checkBoxFollowLog);
            this.flowLayoutPanel.Controls.Add(this.regionPicker);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            // 
            // buttonClearLog
            // 
            resources.ApplyResources(this.buttonClearLog, "buttonClearLog");
            this.buttonClearLog.Name = "buttonClearLog";
            this.buttonClearLog.UseVisualStyleBackColor = true;
            this.buttonClearLog.Click += new System.EventHandler(this.ButtonClearLog_Click);
            // 
            // checkBoxFollowLog
            // 
            resources.ApplyResources(this.checkBoxFollowLog, "checkBoxFollowLog");
            this.checkBoxFollowLog.Name = "checkBoxFollowLog";
            this.checkBoxFollowLog.UseVisualStyleBackColor = true;
            this.checkBoxFollowLog.CheckedChanged += new System.EventHandler(this.checkBoxFollowLog_CheckedChanged);
            // 
            // regionPicker
            // 
            resources.ApplyResources(this.regionPicker, "regionPicker");
            this.regionPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.regionPicker.Name = "regionPicker";
            this.regionPicker.Items.AddRange(new string[] { "Global", "Japan (EN Patch)", "Taiwan (EN Patch)" });
            this.regionPicker.SelectedIndexChanged += new System.EventHandler(this.RegionPicker_SelectedIndexChanged);
            this.regionPicker.SelectedIndex = 0;
            // 
            // ControlPanel
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.flowLayoutPanel);
            this.Name = "ControlPanel";
            this.tabPageMain.ResumeLayout(false);
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private NeoActPlugin.Core.TabControlExt tabControl;
        private System.Windows.Forms.TabPage tabPageMain;
        private System.Windows.Forms.GroupBox groupBox2;
        private Label label_ListEmpty;
        private TextBox logBox;
        private FlowLayoutPanel flowLayoutPanel;
        private Button buttonClearLog;
        private CheckBox checkBoxFollowLog;
        private ComboBox regionPicker;
    }
}