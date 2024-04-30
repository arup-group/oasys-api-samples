using Oasys.Gsa.DotNetHelpers;
using System.Net;

namespace demo_sinosoidal_roof_Forces
{
    partial class Form1
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
            this.grdResult = new System.Windows.Forms.DataGridView();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.btnResult = new System.Windows.Forms.Button();
            this.txtElementNumber = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLoadCase = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtResultPos = new System.Windows.Forms.TextBox();
            this.chkInter = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.grdResult)).BeginInit();
            this.SuspendLayout();
            // 
            // grdResult
            // 
            this.grdResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdResult.Location = new System.Drawing.Point(41, 241);
            this.grdResult.Name = "grdResult";
            this.grdResult.Size = new System.Drawing.Size(703, 191);
            this.grdResult.TabIndex = 0;
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(161, 21);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(507, 20);
            this.txtFilePath.TabIndex = 1;
            this.txtFilePath.Text = Utils.DownloadExampleFile("Env.gwb", "sinosoidal_roof_forces.gwb");
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(674, 20);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(28, 23);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "GSA File Path";
            // 
            // btnResult
            // 
            this.btnResult.Location = new System.Drawing.Point(310, 212);
            this.btnResult.Name = "btnResult";
            this.btnResult.Size = new System.Drawing.Size(87, 23);
            this.btnResult.TabIndex = 4;
            this.btnResult.Text = "Get Result";
            this.btnResult.UseVisualStyleBackColor = true;
            this.btnResult.Click += new System.EventHandler(this.btnResult_Click);
            // 
            // txtElementNumber
            // 
            this.txtElementNumber.Location = new System.Drawing.Point(161, 65);
            this.txtElementNumber.Name = "txtElementNumber";
            this.txtElementNumber.Size = new System.Drawing.Size(52, 20);
            this.txtElementNumber.TabIndex = 5;
            this.txtElementNumber.Text = "All";
            this.txtElementNumber.TextChanged += new System.EventHandler(this.txtElementNumber_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(39, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Element Number";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Load Case";
            // 
            // txtLoadCase
            // 
            this.txtLoadCase.Location = new System.Drawing.Point(161, 109);
            this.txtLoadCase.Name = "txtLoadCase";
            this.txtLoadCase.Size = new System.Drawing.Size(52, 20);
            this.txtLoadCase.TabIndex = 7;
            this.txtLoadCase.Text = "A1";
            this.txtLoadCase.TextChanged += new System.EventHandler(this.txtLoadCase_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(39, 157);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "No of Result Position";
            // 
            // txtResultPos
            // 
            this.txtResultPos.Location = new System.Drawing.Point(161, 153);
            this.txtResultPos.Name = "txtResultPos";
            this.txtResultPos.Size = new System.Drawing.Size(52, 20);
            this.txtResultPos.TabIndex = 9;
            this.txtResultPos.Text = "2";
            this.txtResultPos.TextChanged += new System.EventHandler(this.txtResultPos_TextChanged);
            // 
            // chkInter
            // 
            this.chkInter.AutoSize = true;
            this.chkInter.Checked = true;
            this.chkInter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkInter.Location = new System.Drawing.Point(42, 196);
            this.chkInter.Name = "chkInter";
            this.chkInter.Size = new System.Drawing.Size(149, 17);
            this.chkInter.TabIndex = 11;
            this.chkInter.Text = "Include intersecting points";
            this.chkInter.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 445);
            this.Controls.Add(this.chkInter);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtResultPos);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtLoadCase);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtElementNumber);
            this.Controls.Add(this.btnResult);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.grdResult);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(787, 483);
            this.Name = "Form1";
            this.Text = "Forces";
            ((System.ComponentModel.ISupportInitialize)(this.grdResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grdResult;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnResult;
        private System.Windows.Forms.TextBox txtElementNumber;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLoadCase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtResultPos;
        private System.Windows.Forms.CheckBox chkInter;
    }
}

