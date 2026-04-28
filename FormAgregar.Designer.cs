namespace Proyecto_Reuniones
{
    partial class FormAgregar
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnLimpiar = new System.Windows.Forms.Button();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.clbListaInvestigadores = new System.Windows.Forms.CheckedListBox();
            this.txtIdSemillero = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cboLugarReunion = new System.Windows.Forms.ComboBox();
            this.txtIdLider = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtLiderResponsable = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtMotivoReunion = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpHoraFinalReunion = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpHoraInicioReunion = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dtpFechaReunion = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.txtIdReunion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAgregar);
            this.groupBox1.Controls.Add(this.btnLimpiar);
            this.groupBox1.Controls.Add(this.btnGuardar);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.clbListaInvestigadores);
            this.groupBox1.Controls.Add(this.txtIdSemillero);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.cboLugarReunion);
            this.groupBox1.Controls.Add(this.txtIdLider);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtLiderResponsable);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtMotivoReunion);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.dtpHoraFinalReunion);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.dtpHoraInicioReunion);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.dtpFechaReunion);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtIdReunion);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(16, 24);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(583, 706);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Agregar Reunión";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btnAgregar
            // 
            this.btnAgregar.Location = new System.Drawing.Point(465, 568);
            this.btnAgregar.Name = "btnAgregar";
            this.btnAgregar.Size = new System.Drawing.Size(75, 23);
            this.btnAgregar.TabIndex = 23;
            this.btnAgregar.Text = "Agregar";
            this.btnAgregar.UseVisualStyleBackColor = true;
            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            // 
            // btnLimpiar
            // 
            this.btnLimpiar.Location = new System.Drawing.Point(272, 642);
            this.btnLimpiar.Margin = new System.Windows.Forms.Padding(2);
            this.btnLimpiar.Name = "btnLimpiar";
            this.btnLimpiar.Size = new System.Drawing.Size(74, 40);
            this.btnLimpiar.TabIndex = 22;
            this.btnLimpiar.Text = "Limpiar Campos";
            this.btnLimpiar.UseVisualStyleBackColor = true;
            // 
            // btnGuardar
            // 
            this.btnGuardar.Location = new System.Drawing.Point(142, 642);
            this.btnGuardar.Margin = new System.Windows.Forms.Padding(2);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(74, 40);
            this.btnGuardar.TabIndex = 21;
            this.btnGuardar.Text = "Guardar Reunión";
            this.btnGuardar.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(27, 514);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(116, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Agregar Investigadores";
            // 
            // clbListaInvestigadores
            // 
            this.clbListaInvestigadores.FormattingEnabled = true;
            this.clbListaInvestigadores.Location = new System.Drawing.Point(218, 514);
            this.clbListaInvestigadores.Margin = new System.Windows.Forms.Padding(2);
            this.clbListaInvestigadores.Name = "clbListaInvestigadores";
            this.clbListaInvestigadores.ScrollAlwaysVisible = true;
            this.clbListaInvestigadores.Size = new System.Drawing.Size(209, 79);
            this.clbListaInvestigadores.TabIndex = 19;
            // 
            // txtIdSemillero
            // 
            this.txtIdSemillero.Location = new System.Drawing.Point(218, 470);
            this.txtIdSemillero.Margin = new System.Windows.Forms.Padding(2);
            this.txtIdSemillero.Name = "txtIdSemillero";
            this.txtIdSemillero.Size = new System.Drawing.Size(209, 20);
            this.txtIdSemillero.TabIndex = 18;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(27, 470);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(63, 13);
            this.label9.TabIndex = 17;
            this.label9.Text = "ID Semillero";
            // 
            // cboLugarReunion
            // 
            this.cboLugarReunion.FormattingEnabled = true;
            this.cboLugarReunion.Items.AddRange(new object[] {
            "Oficina de reuniones"});
            this.cboLugarReunion.Location = new System.Drawing.Point(218, 384);
            this.cboLugarReunion.Margin = new System.Windows.Forms.Padding(2);
            this.cboLugarReunion.Name = "cboLugarReunion";
            this.cboLugarReunion.Size = new System.Drawing.Size(209, 21);
            this.cboLugarReunion.TabIndex = 16;
            this.cboLugarReunion.SelectedIndexChanged += new System.EventHandler(this.cboLugarReunion_SelectedIndexChanged);
            // 
            // txtIdLider
            // 
            this.txtIdLider.Location = new System.Drawing.Point(372, 428);
            this.txtIdLider.Margin = new System.Windows.Forms.Padding(2);
            this.txtIdLider.Name = "txtIdLider";
            this.txtIdLider.Size = new System.Drawing.Size(55, 20);
            this.txtIdLider.TabIndex = 15;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(352, 431);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "ID";
            // 
            // txtLiderResponsable
            // 
            this.txtLiderResponsable.Location = new System.Drawing.Point(218, 428);
            this.txtLiderResponsable.Margin = new System.Windows.Forms.Padding(2);
            this.txtLiderResponsable.Name = "txtLiderResponsable";
            this.txtLiderResponsable.Size = new System.Drawing.Size(115, 20);
            this.txtLiderResponsable.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 428);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "Líder Responsable";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 384);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Lugar de la Reunión";
            // 
            // txtMotivoReunion
            // 
            this.txtMotivoReunion.Location = new System.Drawing.Point(218, 249);
            this.txtMotivoReunion.Margin = new System.Windows.Forms.Padding(2);
            this.txtMotivoReunion.Multiline = true;
            this.txtMotivoReunion.Name = "txtMotivoReunion";
            this.txtMotivoReunion.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMotivoReunion.Size = new System.Drawing.Size(209, 109);
            this.txtMotivoReunion.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 249);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(108, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Motivo de la Reunión";
            // 
            // dtpHoraFinalReunion
            // 
            this.dtpHoraFinalReunion.CustomFormat = "hh:mm tt";
            this.dtpHoraFinalReunion.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpHoraFinalReunion.Location = new System.Drawing.Point(218, 194);
            this.dtpHoraFinalReunion.Margin = new System.Windows.Forms.Padding(2);
            this.dtpHoraFinalReunion.Name = "dtpHoraFinalReunion";
            this.dtpHoraFinalReunion.ShowUpDown = true;
            this.dtpHoraFinalReunion.Size = new System.Drawing.Size(151, 20);
            this.dtpHoraFinalReunion.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 198);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(146, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Hora de Finalización Reunión";
            // 
            // dtpHoraInicioReunion
            // 
            this.dtpHoraInicioReunion.CustomFormat = "hh:mm tt";
            this.dtpHoraInicioReunion.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpHoraInicioReunion.Location = new System.Drawing.Point(218, 148);
            this.dtpHoraInicioReunion.Margin = new System.Windows.Forms.Padding(2);
            this.dtpHoraInicioReunion.Name = "dtpHoraInicioReunion";
            this.dtpHoraInicioReunion.ShowUpDown = true;
            this.dtpHoraInicioReunion.Size = new System.Drawing.Size(151, 20);
            this.dtpHoraInicioReunion.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 152);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Hora de inicio Reunión";
            // 
            // dtpFechaReunion
            // 
            this.dtpFechaReunion.Location = new System.Drawing.Point(218, 88);
            this.dtpFechaReunion.Margin = new System.Windows.Forms.Padding(2);
            this.dtpFechaReunion.Name = "dtpFechaReunion";
            this.dtpFechaReunion.Size = new System.Drawing.Size(209, 20);
            this.dtpFechaReunion.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 93);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Fecha Reunión";
            // 
            // txtIdReunion
            // 
            this.txtIdReunion.Location = new System.Drawing.Point(218, 39);
            this.txtIdReunion.Margin = new System.Windows.Forms.Padding(2);
            this.txtIdReunion.Name = "txtIdReunion";
            this.txtIdReunion.Size = new System.Drawing.Size(209, 20);
            this.txtIdReunion.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 41);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "ID Reunión";
            // 
            // btnCancelar
            // 
            this.btnCancelar.Location = new System.Drawing.Point(639, 63);
            this.btnCancelar.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(56, 19);
            this.btnCancelar.TabIndex = 1;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = true;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // FormAgregar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 627);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "FormAgregar";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.FormAgregar_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpFechaReunion;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIdReunion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpHoraInicioReunion;
        private System.Windows.Forms.TextBox txtMotivoReunion;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpHoraFinalReunion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLiderResponsable;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtIdLider;
        private System.Windows.Forms.ComboBox cboLugarReunion;
        private System.Windows.Forms.TextBox txtIdSemillero;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckedListBox clbListaInvestigadores;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btnLimpiar;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Button btnAgregar;
    }
}