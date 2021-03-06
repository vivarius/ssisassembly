﻿using System.Windows.Forms;

namespace SSISExecuteAssemblyTask100
{
    partial class frmAssembly
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAssembly));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.chkConfigFile = new System.Windows.Forms.CheckBox();
            this.cmbConfigurationFile = new System.Windows.Forms.ComboBox();
            this.optChooseVariable = new System.Windows.Forms.RadioButton();
            this.optChooseConfigFileConnector = new System.Windows.Forms.RadioButton();
            this.btConfigFileExpression = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbConnection = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbMethod = new System.Windows.Forms.ComboBox();
            this.cmbClasses = new System.Windows.Forms.ComboBox();
            this.cmbNamespace = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblCodeplex = new System.Windows.Forms.Label();
            this.linkCodeplex = new System.Windows.Forms.LinkLabel();
            this.pbCodeplex = new System.Windows.Forms.PictureBox();
            this.grdParameters = new System.Windows.Forms.DataGridView();
            this.grdColParams = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdColDirection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdColVars = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.grdColExpression = new System.Windows.Forms.DataGridViewButtonColumn();
            this.lbOutputValue = new System.Windows.Forms.Label();
            this.cmbBoxReturnVariable = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCodeplex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdParameters)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(482, 425);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 21;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(401, 425);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 22;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(11, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(550, 203);
            this.tabControl1.TabIndex = 23;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.chkConfigFile);
            this.tabPage1.Controls.Add(this.cmbConfigurationFile);
            this.tabPage1.Controls.Add(this.optChooseVariable);
            this.tabPage1.Controls.Add(this.optChooseConfigFileConnector);
            this.tabPage1.Controls.Add(this.btConfigFileExpression);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.cmbConnection);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.cmbMethod);
            this.tabPage1.Controls.Add(this.cmbClasses);
            this.tabPage1.Controls.Add(this.cmbNamespace);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(542, 177);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Assembly specifics";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // chkConfigFile
            // 
            this.chkConfigFile.AutoSize = true;
            this.chkConfigFile.Location = new System.Drawing.Point(76, 153);
            this.chkConfigFile.Name = "chkConfigFile";
            this.chkConfigFile.Size = new System.Drawing.Size(118, 17);
            this.chkConfigFile.TabIndex = 41;
            this.chkConfigFile.Text = "Attach a Config File";
            this.chkConfigFile.UseVisualStyleBackColor = true;
            this.chkConfigFile.CheckedChanged += new System.EventHandler(this.chkConfigFile_CheckedChanged);
            // 
            // cmbConfigurationFile
            // 
            this.cmbConfigurationFile.Enabled = false;
            this.cmbConfigurationFile.FormattingEnabled = true;
            this.cmbConfigurationFile.Location = new System.Drawing.Point(76, 126);
            this.cmbConfigurationFile.Name = "cmbConfigurationFile";
            this.cmbConfigurationFile.Size = new System.Drawing.Size(409, 21);
            this.cmbConfigurationFile.TabIndex = 40;
            // 
            // optChooseVariable
            // 
            this.optChooseVariable.AutoSize = true;
            this.optChooseVariable.Enabled = false;
            this.optChooseVariable.Location = new System.Drawing.Point(366, 152);
            this.optChooseVariable.Name = "optChooseVariable";
            this.optChooseVariable.Size = new System.Drawing.Size(173, 17);
            this.optChooseVariable.TabIndex = 39;
            this.optChooseVariable.Text = "Path from Variable / Expression";
            this.optChooseVariable.UseVisualStyleBackColor = true;
            this.optChooseVariable.CheckedChanged += new System.EventHandler(this.optChooseVariable_CheckedChanged);
            this.optChooseVariable.Click += new System.EventHandler(this.optChooseVariable_Click);
            // 
            // optChooseConfigFileConnector
            // 
            this.optChooseConfigFileConnector.AutoSize = true;
            this.optChooseConfigFileConnector.Checked = true;
            this.optChooseConfigFileConnector.Enabled = false;
            this.optChooseConfigFileConnector.Location = new System.Drawing.Point(217, 153);
            this.optChooseConfigFileConnector.Name = "optChooseConfigFileConnector";
            this.optChooseConfigFileConnector.Size = new System.Drawing.Size(140, 17);
            this.optChooseConfigFileConnector.TabIndex = 38;
            this.optChooseConfigFileConnector.TabStop = true;
            this.optChooseConfigFileConnector.Text = "Path from File connector";
            this.optChooseConfigFileConnector.UseVisualStyleBackColor = true;
            this.optChooseConfigFileConnector.CheckedChanged += new System.EventHandler(this.optChooseConfigFileConnector_CheckedChanged);
            this.optChooseConfigFileConnector.Click += new System.EventHandler(this.optChooseConfigFileConnector_Click);
            // 
            // btConfigFileExpression
            // 
            this.btConfigFileExpression.Enabled = false;
            this.btConfigFileExpression.Location = new System.Drawing.Point(491, 124);
            this.btConfigFileExpression.Name = "btConfigFileExpression";
            this.btConfigFileExpression.Size = new System.Drawing.Size(45, 23);
            this.btConfigFileExpression.TabIndex = 37;
            this.btConfigFileExpression.Text = "(fx)";
            this.btConfigFileExpression.UseVisualStyleBackColor = true;
            this.btConfigFileExpression.Click += new System.EventHandler(this.btConfigFile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(6, 133);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "Config. file";
            // 
            // cmbConnection
            // 
            this.cmbConnection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConnection.FormattingEnabled = true;
            this.cmbConnection.Location = new System.Drawing.Point(76, 15);
            this.cmbConnection.Name = "cmbConnection";
            this.cmbConnection.Size = new System.Drawing.Size(460, 21);
            this.cmbConnection.TabIndex = 34;
            this.cmbConnection.SelectedIndexChanged += new System.EventHandler(this.cmbConnection_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 33;
            this.label4.Text = "Connection";
            // 
            // cmbMethod
            // 
            this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMethod.FormattingEnabled = true;
            this.cmbMethod.Location = new System.Drawing.Point(76, 100);
            this.cmbMethod.Name = "cmbMethod";
            this.cmbMethod.Size = new System.Drawing.Size(460, 21);
            this.cmbMethod.TabIndex = 26;
            this.cmbMethod.SelectedIndexChanged += new System.EventHandler(this.cmbMethod_SelectedIndexChanged);
            // 
            // cmbClasses
            // 
            this.cmbClasses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbClasses.FormattingEnabled = true;
            this.cmbClasses.Location = new System.Drawing.Point(76, 74);
            this.cmbClasses.Name = "cmbClasses";
            this.cmbClasses.Size = new System.Drawing.Size(460, 21);
            this.cmbClasses.TabIndex = 25;
            this.cmbClasses.SelectedIndexChanged += new System.EventHandler(this.cmbClasses_SelectedIndexChanged);
            // 
            // cmbNamespace
            // 
            this.cmbNamespace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNamespace.FormattingEnabled = true;
            this.cmbNamespace.Location = new System.Drawing.Point(76, 46);
            this.cmbNamespace.Name = "cmbNamespace";
            this.cmbNamespace.Size = new System.Drawing.Size(460, 21);
            this.cmbNamespace.TabIndex = 24;
            this.cmbNamespace.SelectedIndexChanged += new System.EventHandler(this.cmbNamespace_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(6, 75);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(32, 13);
            this.label12.TabIndex = 23;
            this.label12.Text = "Class";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(6, 101);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(43, 13);
            this.label13.TabIndex = 22;
            this.label13.Text = "Method";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(6, 49);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(64, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Namespace";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lblAuthor);
            this.tabPage2.Controls.Add(this.lblVersion);
            this.tabPage2.Controls.Add(this.lblCodeplex);
            this.tabPage2.Controls.Add(this.linkCodeplex);
            this.tabPage2.Controls.Add(this.pbCodeplex);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(542, 177);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Info";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(305, 117);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(96, 13);
            this.lblAuthor.TabIndex = 22;
            this.lblAuthor.Text = "by Cosmin VLASIU";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(305, 130);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(84, 13);
            this.lblVersion.TabIndex = 21;
            this.lblVersion.Text = "Version 1.0 beta";
            // 
            // lblCodeplex
            // 
            this.lblCodeplex.AutoSize = true;
            this.lblCodeplex.Location = new System.Drawing.Point(6, 100);
            this.lblCodeplex.Name = "lblCodeplex";
            this.lblCodeplex.Size = new System.Drawing.Size(301, 13);
            this.lblCodeplex.TabIndex = 20;
            this.lblCodeplex.Text = "This project is hosted on CodePlex and licensed under MS-PL.";
            // 
            // linkCodeplex
            // 
            this.linkCodeplex.AutoSize = true;
            this.linkCodeplex.Location = new System.Drawing.Point(6, 130);
            this.linkCodeplex.Name = "linkCodeplex";
            this.linkCodeplex.Size = new System.Drawing.Size(180, 13);
            this.linkCodeplex.TabIndex = 19;
            this.linkCodeplex.TabStop = true;
            this.linkCodeplex.Text = "http://SSISAssembly.codeplex.com/";
            // 
            // pbCodeplex
            // 
            this.pbCodeplex.Dock = System.Windows.Forms.DockStyle.Top;
            this.pbCodeplex.Image = ((System.Drawing.Image)(resources.GetObject("pbCodeplex.Image")));
            this.pbCodeplex.Location = new System.Drawing.Point(3, 3);
            this.pbCodeplex.Name = "pbCodeplex";
            this.pbCodeplex.Size = new System.Drawing.Size(536, 94);
            this.pbCodeplex.TabIndex = 18;
            this.pbCodeplex.TabStop = false;
            // 
            // grdParameters
            // 
            this.grdParameters.AllowUserToAddRows = false;
            this.grdParameters.AllowUserToDeleteRows = false;
            this.grdParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdParameters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.grdColParams,
            this.grdColDirection,
            this.grdColVars,
            this.grdColExpression});
            this.grdParameters.Location = new System.Drawing.Point(11, 221);
            this.grdParameters.Name = "grdParameters";
            this.grdParameters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.grdParameters.Size = new System.Drawing.Size(550, 160);
            this.grdParameters.TabIndex = 24;
            this.grdParameters.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdParameters_CellContentClick);
            // 
            // grdColParams
            // 
            this.grdColParams.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.grdColParams.Frozen = true;
            this.grdColParams.HeaderText = "Parameters";
            this.grdColParams.Name = "grdColParams";
            this.grdColParams.ReadOnly = true;
            this.grdColParams.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.grdColParams.Width = 66;
            // 
            // grdColDirection
            // 
            this.grdColDirection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.grdColDirection.FillWeight = 40F;
            this.grdColDirection.HeaderText = "Param Type";
            this.grdColDirection.Name = "grdColDirection";
            this.grdColDirection.ReadOnly = true;
            this.grdColDirection.Width = 89;
            // 
            // grdColVars
            // 
            this.grdColVars.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.grdColVars.DropDownWidth = 300;
            this.grdColVars.HeaderText = "Variables";
            this.grdColVars.MaxDropDownItems = 10;
            this.grdColVars.Name = "grdColVars";
            this.grdColVars.Sorted = true;
            this.grdColVars.Width = 240;
            // 
            // grdColExpression
            // 
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.Padding = new System.Windows.Forms.Padding(2);
            this.grdColExpression.DefaultCellStyle = dataGridViewCellStyle3;
            this.grdColExpression.HeaderText = "f(x)";
            this.grdColExpression.Name = "grdColExpression";
            this.grdColExpression.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.grdColExpression.Text = "f(x)";
            this.grdColExpression.ToolTipText = "Expressions...";
            this.grdColExpression.Width = 30;
            // 
            // lbOutputValue
            // 
            this.lbOutputValue.AutoSize = true;
            this.lbOutputValue.Location = new System.Drawing.Point(8, 401);
            this.lbOutputValue.Name = "lbOutputValue";
            this.lbOutputValue.Size = new System.Drawing.Size(69, 13);
            this.lbOutputValue.TabIndex = 25;
            this.lbOutputValue.Text = "Output Value";
            // 
            // cmbBoxReturnVariable
            // 
            this.cmbBoxReturnVariable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxReturnVariable.FormattingEnabled = true;
            this.cmbBoxReturnVariable.Location = new System.Drawing.Point(91, 398);
            this.cmbBoxReturnVariable.Name = "cmbBoxReturnVariable";
            this.cmbBoxReturnVariable.Size = new System.Drawing.Size(470, 21);
            this.cmbBoxReturnVariable.TabIndex = 26;
            // 
            // frmAssembly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(571, 455);
            this.Controls.Add(this.cmbBoxReturnVariable);
            this.Controls.Add(this.lbOutputValue);
            this.Controls.Add(this.grdParameters);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAssembly";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Task Configuration";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCodeplex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdParameters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        public System.Windows.Forms.ComboBox cmbMethod;
        public System.Windows.Forms.ComboBox cmbClasses;
        public System.Windows.Forms.ComboBox cmbNamespace;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TabPage tabPage2;
        internal System.Windows.Forms.Label lblAuthor;
        internal System.Windows.Forms.Label lblVersion;
        internal System.Windows.Forms.Label lblCodeplex;
        internal System.Windows.Forms.LinkLabel linkCodeplex;
        private System.Windows.Forms.PictureBox pbCodeplex;
        public System.Windows.Forms.ComboBox cmbConnection;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView grdParameters;
        private System.Windows.Forms.Label lbOutputValue;
        private ComboBox cmbBoxReturnVariable;
        private Button btConfigFileExpression;
        private Label label1;
        private RadioButton optChooseVariable;
        private RadioButton optChooseConfigFileConnector;
        private ComboBox cmbConfigurationFile;
        private CheckBox chkConfigFile;
        private DataGridViewTextBoxColumn grdColParams;
        private DataGridViewTextBoxColumn grdColDirection;
        private DataGridViewComboBoxColumn grdColVars;
        private DataGridViewButtonColumn grdColExpression;

    }
}