namespace Proyecto_Reuniones
{
    partial class FormPrincipal
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_Consultar_con_parametros = new System.Windows.Forms.Button();
            this.btnAtras = new System.Windows.Forms.Button();
            this.btnAgregarReunión = new System.Windows.Forms.Button();
            this.btnVerReunion = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(65, 181);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(809, 555);
            this.dataGridView1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_Consultar_con_parametros);
            this.groupBox1.Location = new System.Drawing.Point(65, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(809, 130);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Consultar con parametros ";
            // 
            // btn_Consultar_con_parametros
            // 
            this.btn_Consultar_con_parametros.Location = new System.Drawing.Point(648, 46);
            this.btn_Consultar_con_parametros.Name = "btn_Consultar_con_parametros";
            this.btn_Consultar_con_parametros.Size = new System.Drawing.Size(118, 43);
            this.btn_Consultar_con_parametros.TabIndex = 2;
            this.btn_Consultar_con_parametros.Text = "Consultar ";
            this.btn_Consultar_con_parametros.UseVisualStyleBackColor = true;
            // 
            // btnAtras
            // 
            this.btnAtras.Location = new System.Drawing.Point(922, 51);
            this.btnAtras.Name = "btnAtras";
            this.btnAtras.Size = new System.Drawing.Size(94, 50);
            this.btnAtras.TabIndex = 3;
            this.btnAtras.Text = "Cerrar Sesión";
            this.btnAtras.UseVisualStyleBackColor = true;
            this.btnAtras.Click += new System.EventHandler(this.btnAtras_Click);
            // 
            // btnAgregarReunión
            // 
            this.btnAgregarReunión.Location = new System.Drawing.Point(967, 171);
            this.btnAgregarReunión.Name = "btnAgregarReunión";
            this.btnAgregarReunión.Size = new System.Drawing.Size(87, 51);
            this.btnAgregarReunión.TabIndex = 4;
            this.btnAgregarReunión.Text = "Agregar Reunión";
            this.btnAgregarReunión.UseVisualStyleBackColor = true;
            this.btnAgregarReunión.Click += new System.EventHandler(this.btnAgregarReunión_Click);
            // 
            // btnVerReunion
            // 
            this.btnVerReunion.Location = new System.Drawing.Point(959, 281);
            this.btnVerReunion.Name = "btnVerReunion";
            this.btnVerReunion.Size = new System.Drawing.Size(95, 50);
            this.btnVerReunion.TabIndex = 5;
            this.btnVerReunion.Text = "Ver Reuniones";
            this.btnVerReunion.UseVisualStyleBackColor = true;
            this.btnVerReunion.Click += new System.EventHandler(this.btnVerReunion_Click);
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1512, 791);
            this.ControlBox = false;
            this.Controls.Add(this.btnVerReunion);
            this.Controls.Add(this.btnAgregarReunión);
            this.Controls.Add(this.btnAtras);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "FormPrincipal";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_Consultar_con_parametros;
        private System.Windows.Forms.Button btnAtras;
        private System.Windows.Forms.Button btnAgregarReunión;
        private System.Windows.Forms.Button btnVerReunion;
    }
}