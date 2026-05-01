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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrincipal));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cboFiltro = new System.Windows.Forms.ComboBox();
            this.panelFiltro = new System.Windows.Forms.Panel();
            this.btn_Consultar_con_parametros = new System.Windows.Forms.Button();
            this.btnAtras = new System.Windows.Forms.Button();
            this.btnAgregarReunión = new System.Windows.Forms.Button();
            this.btnVerReunion = new System.Windows.Forms.Button();
            this.cboEstadoReunion = new System.Windows.Forms.ComboBox();
            this.lblFechaHora = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblNombreYApellido = new System.Windows.Forms.Label();
            this.btnConfirmarAsistencia = new System.Windows.Forms.Button();
            this.cboAsistencia = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnReporte = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(65, 181);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(809, 555);
            this.dataGridView1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cboFiltro);
            this.groupBox1.Controls.Add(this.panelFiltro);
            this.groupBox1.Controls.Add(this.btn_Consultar_con_parametros);
            this.groupBox1.Location = new System.Drawing.Point(65, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(809, 130);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Consultar con parametros ";
            // 
            // cboFiltro
            // 
            this.cboFiltro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFiltro.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cboFiltro.FormattingEnabled = true;
            this.cboFiltro.Items.AddRange(new object[] {
            "",
            "idReunion",
            "fechaReunion",
            "horaInicio",
            "horaFin",
            "motivoReunion",
            "lugarReunion",
            "idInvestigadores"});
            this.cboFiltro.Location = new System.Drawing.Point(22, 56);
            this.cboFiltro.Name = "cboFiltro";
            this.cboFiltro.Size = new System.Drawing.Size(206, 24);
            this.cboFiltro.TabIndex = 4;
            this.cboFiltro.SelectedIndexChanged += new System.EventHandler(this.cboFiltro_SelectedIndexChanged);
            // 
            // panelFiltro
            // 
            this.panelFiltro.Location = new System.Drawing.Point(271, 56);
            this.panelFiltro.Name = "panelFiltro";
            this.panelFiltro.Size = new System.Drawing.Size(204, 33);
            this.panelFiltro.TabIndex = 3;
            // 
            // btn_Consultar_con_parametros
            // 
            this.btn_Consultar_con_parametros.Location = new System.Drawing.Point(648, 46);
            this.btn_Consultar_con_parametros.Name = "btn_Consultar_con_parametros";
            this.btn_Consultar_con_parametros.Size = new System.Drawing.Size(118, 43);
            this.btn_Consultar_con_parametros.TabIndex = 2;
            this.btn_Consultar_con_parametros.Text = "Consultar ";
            this.btn_Consultar_con_parametros.UseVisualStyleBackColor = true;
            this.btn_Consultar_con_parametros.Click += new System.EventHandler(this.btn_Consultar_con_parametros_Click);
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
            // cboEstadoReunion
            // 
            this.cboEstadoReunion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEstadoReunion.FormattingEnabled = true;
            this.cboEstadoReunion.Items.AddRange(new object[] {
            "Todas",
            "Programadas",
            "En Ejecución",
            "Finalizadas"});
            this.cboEstadoReunion.Location = new System.Drawing.Point(933, 369);
            this.cboEstadoReunion.Name = "cboEstadoReunion";
            this.cboEstadoReunion.Size = new System.Drawing.Size(121, 24);
            this.cboEstadoReunion.TabIndex = 6;
            // 
            // lblFechaHora
            // 
            this.lblFechaHora.AutoSize = true;
            this.lblFechaHora.Location = new System.Drawing.Point(981, 464);
            this.lblFechaHora.Name = "lblFechaHora";
            this.lblFechaHora.Size = new System.Drawing.Size(149, 16);
            this.lblFechaHora.TabIndex = 7;
            this.lblFechaHora.Text = "dd/mm/yyyy - hh/mm/ss";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lblNombreYApellido
            // 
            this.lblNombreYApellido.AutoSize = true;
            this.lblNombreYApellido.Location = new System.Drawing.Point(1000, 548);
            this.lblNombreYApellido.Name = "lblNombreYApellido";
            this.lblNombreYApellido.Size = new System.Drawing.Size(109, 16);
            this.lblNombreYApellido.TabIndex = 8;
            this.lblNombreYApellido.Text = "Nombre Apellido";
            // 
            // btnConfirmarAsistencia
            // 
            this.btnConfirmarAsistencia.Location = new System.Drawing.Point(990, 638);
            this.btnConfirmarAsistencia.Name = "btnConfirmarAsistencia";
            this.btnConfirmarAsistencia.Size = new System.Drawing.Size(91, 54);
            this.btnConfirmarAsistencia.TabIndex = 9;
            this.btnConfirmarAsistencia.Text = "Confirmar Asistencia";
            this.btnConfirmarAsistencia.UseVisualStyleBackColor = true;
            this.btnConfirmarAsistencia.Click += new System.EventHandler(this.btnConfirmarAsistencia_Click);
            // 
            // cboAsistencia
            // 
            this.cboAsistencia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAsistencia.FormattingEnabled = true;
            this.cboAsistencia.Location = new System.Drawing.Point(933, 423);
            this.cboAsistencia.Name = "cboAsistencia";
            this.cboAsistencia.Size = new System.Drawing.Size(121, 24);
            this.cboAsistencia.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1065, 377);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "Estado Reunión";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1065, 426);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Confirmar Asistencia";
            // 
            // btnReporte
            // 
            this.btnReporte.Location = new System.Drawing.Point(1223, 669);
            this.btnReporte.Name = "btnReporte";
            this.btnReporte.Size = new System.Drawing.Size(89, 67);
            this.btnReporte.TabIndex = 13;
            this.btnReporte.Text = "Generar Reporte";
            this.btnReporte.UseVisualStyleBackColor = true;
            this.btnReporte.Click += new System.EventHandler(this.btnReporte_Click);
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1512, 791);
            this.ControlBox = false;
            this.Controls.Add(this.btnReporte);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboAsistencia);
            this.Controls.Add(this.btnConfirmarAsistencia);
            this.Controls.Add(this.lblNombreYApellido);
            this.Controls.Add(this.lblFechaHora);
            this.Controls.Add(this.cboEstadoReunion);
            this.Controls.Add(this.btnVerReunion);
            this.Controls.Add(this.btnAgregarReunión);
            this.Controls.Add(this.btnAtras);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_Consultar_con_parametros;
        private System.Windows.Forms.Button btnAtras;
        private System.Windows.Forms.Button btnAgregarReunión;
        private System.Windows.Forms.Button btnVerReunion;
        private System.Windows.Forms.ComboBox cboFiltro;
        private System.Windows.Forms.Panel panelFiltro;
        private System.Windows.Forms.ComboBox cboEstadoReunion;
        private System.Windows.Forms.Label lblFechaHora;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblNombreYApellido;
        private System.Windows.Forms.Button btnConfirmarAsistencia;
        private System.Windows.Forms.ComboBox cboAsistencia;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnReporte;
    }
}