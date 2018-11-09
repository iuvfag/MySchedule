namespace MySchedule
{
    partial class AdminForm
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
            this.userInfoGrid = new System.Windows.Forms.DataGridView();
            this.updateHistoryInfoGrid = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.userInfoGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.updateHistoryInfoGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // userInfoGrid
            // 
            this.userInfoGrid.AllowUserToAddRows = false;
            this.userInfoGrid.AllowUserToDeleteRows = false;
            this.userInfoGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.userInfoGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.userInfoGrid.Location = new System.Drawing.Point(12, 57);
            this.userInfoGrid.Name = "userInfoGrid";
            this.userInfoGrid.ReadOnly = true;
            this.userInfoGrid.RowHeadersVisible = false;
            this.userInfoGrid.RowTemplate.Height = 21;
            this.userInfoGrid.Size = new System.Drawing.Size(889, 256);
            this.userInfoGrid.TabIndex = 0;
            // 
            // updateHistoryInfoGrid
            // 
            this.updateHistoryInfoGrid.AllowUserToAddRows = false;
            this.updateHistoryInfoGrid.AllowUserToDeleteRows = false;
            this.updateHistoryInfoGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.updateHistoryInfoGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.updateHistoryInfoGrid.Location = new System.Drawing.Point(12, 373);
            this.updateHistoryInfoGrid.Name = "updateHistoryInfoGrid";
            this.updateHistoryInfoGrid.ReadOnly = true;
            this.updateHistoryInfoGrid.RowHeadersVisible = false;
            this.updateHistoryInfoGrid.RowTemplate.Height = 21;
            this.updateHistoryInfoGrid.Size = new System.Drawing.Size(889, 274);
            this.updateHistoryInfoGrid.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 27);
            this.label1.TabIndex = 2;
            this.label1.Text = "ユーザー情報一覧";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label2.Location = new System.Drawing.Point(12, 333);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(300, 27);
            this.label2.TabIndex = 3;
            this.label2.Text = "スケジュール登録履歴一覧";
            // 
            // AdminForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(913, 766);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.updateHistoryInfoGrid);
            this.Controls.Add(this.userInfoGrid);
            this.Name = "AdminForm";
            this.Text = "管理者用ページ";
            this.Load += new System.EventHandler(this.AdminForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.userInfoGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.updateHistoryInfoGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView userInfoGrid;
        private System.Windows.Forms.DataGridView updateHistoryInfoGrid;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}