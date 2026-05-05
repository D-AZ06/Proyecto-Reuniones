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
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboFiltro = new System.Windows.Forms.ComboBox();
            this.panelFiltro = new System.Windows.Forms.Panel();
            this.btn_Consultar_con_parametros = new System.Windows.Forms.Button();
            this.btnVerReunion = new System.Windows.Forms.Button();
            this.btnAtras = new System.Windows.Forms.Button();
            this.btnAgregarReunión = new System.Windows.Forms.Button();
            this.lblFechaHora = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblNombreYApellido = new System.Windows.Forms.Label();
            this.btnConfirmarAsistencia = new System.Windows.Forms.Button();
            this.btnReporte = new System.Windows.Forms.Button();
            this.btnEliminarReunion = new System.Windows.Forms.Button();
            this.btnModificarReunion = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lblRol = new System.Windows.Forms.Label();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.icono_asistencia = new System.Windows.Forms.PictureBox();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.icono_asistencia)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(249)))), ((int)(((byte)(254)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(302, 250);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(865, 427);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(249)))), ((int)(((byte)(254)))));
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.cboFiltro);
            this.groupBox1.Controls.Add(this.panelFiltro);
            this.groupBox1.Controls.Add(this.btn_Consultar_con_parametros);
            this.groupBox1.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(302, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(865, 125);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Navy;
            this.label5.Location = new System.Drawing.Point(288, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 15);
            this.label5.TabIndex = 29;
            this.label5.Text = "Datos";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Navy;
            this.label4.Location = new System.Drawing.Point(28, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 15);
            this.label4.TabIndex = 28;
            this.label4.Text = "Buscar por";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Navy;
            this.label2.Location = new System.Drawing.Point(6, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 21);
            this.label2.TabIndex = 26;
            this.label2.Text = "🝖  Filtro";
            // 
            // cboFiltro
            // 
            this.cboFiltro.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFiltro.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cboFiltro.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.cboFiltro.Location = new System.Drawing.Point(31, 62);
            this.cboFiltro.Name = "cboFiltro";
            this.cboFiltro.Size = new System.Drawing.Size(206, 25);
            this.cboFiltro.TabIndex = 4;
            this.cboFiltro.SelectedIndexChanged += new System.EventHandler(this.cboFiltro_SelectedIndexChanged);
            // 
            // panelFiltro
            // 
            this.panelFiltro.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFiltro.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelFiltro.Location = new System.Drawing.Point(291, 62);
            this.panelFiltro.Name = "panelFiltro";
            this.panelFiltro.Size = new System.Drawing.Size(290, 29);
            this.panelFiltro.TabIndex = 3;
            this.panelFiltro.Paint += new System.Windows.Forms.PaintEventHandler(this.panelFiltro_Paint);
            // 
            // btn_Consultar_con_parametros
            // 
            this.btn_Consultar_con_parametros.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btn_Consultar_con_parametros.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Consultar_con_parametros.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Consultar_con_parametros.ForeColor = System.Drawing.Color.White;
            this.btn_Consultar_con_parametros.Location = new System.Drawing.Point(628, 48);
            this.btn_Consultar_con_parametros.Name = "btn_Consultar_con_parametros";
            this.btn_Consultar_con_parametros.Size = new System.Drawing.Size(221, 51);
            this.btn_Consultar_con_parametros.TabIndex = 2;
            this.btn_Consultar_con_parametros.Text = "🔍︎  Consultar ";
            this.btn_Consultar_con_parametros.UseVisualStyleBackColor = false;
            this.btn_Consultar_con_parametros.Click += new System.EventHandler(this.btn_Consultar_con_parametros_Click);
            // 
            // btnVerReunion
            // 
            this.btnVerReunion.BackColor = System.Drawing.Color.SkyBlue;
            this.btnVerReunion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerReunion.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerReunion.ForeColor = System.Drawing.Color.Navy;
            this.btnVerReunion.Location = new System.Drawing.Point(24, 296);
            this.btnVerReunion.Name = "btnVerReunion";
            this.btnVerReunion.Size = new System.Drawing.Size(221, 51);
            this.btnVerReunion.TabIndex = 5;
            this.btnVerReunion.Text = "👀    Ver Reuniones ";
            this.btnVerReunion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnVerReunion.UseVisualStyleBackColor = false;
            this.btnVerReunion.Click += new System.EventHandler(this.btnVerReunion_Click);
            // 
            // btnAtras
            // 
            this.btnAtras.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnAtras.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAtras.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAtras.ForeColor = System.Drawing.Color.White;
            this.btnAtras.Location = new System.Drawing.Point(25, 626);
            this.btnAtras.Name = "btnAtras";
            this.btnAtras.Size = new System.Drawing.Size(221, 50);
            this.btnAtras.TabIndex = 3;
            this.btnAtras.Text = "Cerrar Sesión";
            this.btnAtras.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAtras.UseVisualStyleBackColor = false;
            this.btnAtras.Click += new System.EventHandler(this.btnAtras_Click);
            // 
            // btnAgregarReunión
            // 
            this.btnAgregarReunión.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnAgregarReunión.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAgregarReunión.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAgregarReunión.ForeColor = System.Drawing.Color.Navy;
            this.btnAgregarReunión.Location = new System.Drawing.Point(21, 362);
            this.btnAgregarReunión.Name = "btnAgregarReunión";
            this.btnAgregarReunión.Size = new System.Drawing.Size(221, 51);
            this.btnAgregarReunión.TabIndex = 4;
            this.btnAgregarReunión.Text = "Agregar Reunión";
            this.btnAgregarReunión.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnAgregarReunión.UseVisualStyleBackColor = false;
            this.btnAgregarReunión.Click += new System.EventHandler(this.btnAgregarReunión_Click);
            // 
            // lblFechaHora
            // 
            this.lblFechaHora.AutoSize = true;
            this.lblFechaHora.Font = new System.Drawing.Font("Microsoft Tai Le", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFechaHora.ForeColor = System.Drawing.Color.White;
            this.lblFechaHora.Location = new System.Drawing.Point(29, 18);
            this.lblFechaHora.Name = "lblFechaHora";
            this.lblFechaHora.Size = new System.Drawing.Size(216, 23);
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
            this.lblNombreYApellido.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNombreYApellido.ForeColor = System.Drawing.Color.White;
            this.lblNombreYApellido.Location = new System.Drawing.Point(62, 239);
            this.lblNombreYApellido.Name = "lblNombreYApellido";
            this.lblNombreYApellido.Size = new System.Drawing.Size(148, 20);
            this.lblNombreYApellido.TabIndex = 8;
            this.lblNombreYApellido.Text = "Nombre Apellido";
            this.lblNombreYApellido.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblNombreYApellido.Click += new System.EventHandler(this.lblNombreYApellido_Click);
            // 
            // btnConfirmarAsistencia
            // 
            this.btnConfirmarAsistencia.BackColor = System.Drawing.Color.LightSteelBlue;
            this.btnConfirmarAsistencia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirmarAsistencia.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirmarAsistencia.ForeColor = System.Drawing.Color.Black;
            this.btnConfirmarAsistencia.Location = new System.Drawing.Point(21, 362);
            this.btnConfirmarAsistencia.Name = "btnConfirmarAsistencia";
            this.btnConfirmarAsistencia.Size = new System.Drawing.Size(221, 51);
            this.btnConfirmarAsistencia.TabIndex = 9;
            this.btnConfirmarAsistencia.Text = "Confirmar Asistencia";
            this.btnConfirmarAsistencia.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConfirmarAsistencia.UseVisualStyleBackColor = false;
            this.btnConfirmarAsistencia.Click += new System.EventHandler(this.btnConfirmarAsistencia_Click);
            // 
            // btnReporte
            // 
            this.btnReporte.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnReporte.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReporte.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReporte.ForeColor = System.Drawing.Color.Navy;
            this.btnReporte.Location = new System.Drawing.Point(21, 430);
            this.btnReporte.Name = "btnReporte";
            this.btnReporte.Size = new System.Drawing.Size(221, 51);
            this.btnReporte.TabIndex = 13;
            this.btnReporte.Text = "Generar Reporte";
            this.btnReporte.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnReporte.UseVisualStyleBackColor = false;
            this.btnReporte.Click += new System.EventHandler(this.btnReporte_Click);
            // 
            // btnEliminarReunion
            // 
            this.btnEliminarReunion.BackColor = System.Drawing.Color.AliceBlue;
            this.btnEliminarReunion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEliminarReunion.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEliminarReunion.ForeColor = System.Drawing.Color.Red;
            this.btnEliminarReunion.Location = new System.Drawing.Point(662, 179);
            this.btnEliminarReunion.Name = "btnEliminarReunion";
            this.btnEliminarReunion.Size = new System.Drawing.Size(221, 51);
            this.btnEliminarReunion.TabIndex = 15;
            this.btnEliminarReunion.Text = "   🗑️    Eliminar      ";
            this.btnEliminarReunion.UseVisualStyleBackColor = false;
            this.btnEliminarReunion.Click += new System.EventHandler(this.btnEliminarReunion_Click);
            // 
            // btnModificarReunion
            // 
            this.btnModificarReunion.BackColor = System.Drawing.Color.AliceBlue;
            this.btnModificarReunion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnModificarReunion.Font = new System.Drawing.Font("Arial Rounded MT Bold", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnModificarReunion.ForeColor = System.Drawing.Color.Blue;
            this.btnModificarReunion.Location = new System.Drawing.Point(930, 179);
            this.btnModificarReunion.Name = "btnModificarReunion";
            this.btnModificarReunion.Size = new System.Drawing.Size(221, 51);
            this.btnModificarReunion.TabIndex = 16;
            this.btnModificarReunion.Text = "✏️    Modificar ";
            this.btnModificarReunion.UseVisualStyleBackColor = false;
            this.btnModificarReunion.Click += new System.EventHandler(this.btnModificarReunion_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.SteelBlue;
            this.panel1.Controls.Add(this.btnVerReunion);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.lblRol);
            this.panel1.Controls.Add(this.pictureBox8);
            this.panel1.Controls.Add(this.icono_asistencia);
            this.panel1.Controls.Add(this.pictureBox7);
            this.panel1.Controls.Add(this.pictureBox6);
            this.panel1.Controls.Add(this.pictureBox3);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.btnAgregarReunión);
            this.panel1.Controls.Add(this.btnAtras);
            this.panel1.Controls.Add(this.lblNombreYApellido);
            this.panel1.Controls.Add(this.btnConfirmarAsistencia);
            this.panel1.Controls.Add(this.btnReporte);
            this.panel1.Location = new System.Drawing.Point(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(264, 690);
            this.panel1.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Elephant", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(17, 259);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(199, 22);
            this.label1.TabIndex = 26;
            this.label1.Text = "_____________________";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.HotTrack;
            this.panel2.Controls.Add(this.lblFechaHora);
            this.panel2.Location = new System.Drawing.Point(1, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(275, 54);
            this.panel2.TabIndex = 28;
            // 
            // lblRol
            // 
            this.lblRol.AutoSize = true;
            this.lblRol.Font = new System.Drawing.Font("Arial Rounded MT Bold", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRol.ForeColor = System.Drawing.Color.White;
            this.lblRol.Location = new System.Drawing.Point(122, 221);
            this.lblRol.Name = "lblRol";
            this.lblRol.Size = new System.Drawing.Size(25, 15);
            this.lblRol.TabIndex = 27;
            this.lblRol.Text = "rol";
            this.lblRol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = global::Proyecto_Reuniones.Properties.Resources.mas;
            this.pictureBox8.Location = new System.Drawing.Point(24, 367);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(46, 40);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox8.TabIndex = 26;
            this.pictureBox8.TabStop = false;
            // 
            // icono_asistencia
            // 
            this.icono_asistencia.Image = global::Proyecto_Reuniones.Properties.Resources.asistencia;
            this.icono_asistencia.Location = new System.Drawing.Point(24, 364);
            this.icono_asistencia.Name = "icono_asistencia";
            this.icono_asistencia.Size = new System.Drawing.Size(46, 46);
            this.icono_asistencia.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.icono_asistencia.TabIndex = 25;
            this.icono_asistencia.TabStop = false;
            // 
            // pictureBox7
            // 
            this.pictureBox7.Image = global::Proyecto_Reuniones.Properties.Resources.salir;
            this.pictureBox7.Location = new System.Drawing.Point(28, 628);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(46, 46);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox7.TabIndex = 23;
            this.pictureBox7.TabStop = false;
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = global::Proyecto_Reuniones.Properties.Resources.reporte;
            this.pictureBox6.Location = new System.Drawing.Point(24, 432);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(46, 46);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 22;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::Proyecto_Reuniones.Properties.Resources.usuario;
            this.pictureBox3.Location = new System.Drawing.Point(81, 133);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(115, 103);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 19;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::Proyecto_Reuniones.Properties.Resources.logo_gessi;
            this.pictureBox2.Location = new System.Drawing.Point(152, 68);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(90, 69);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 18;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::Proyecto_Reuniones.Properties.Resources.Captura_de_pantalla_2026_05_03_094224;
            this.pictureBox1.Location = new System.Drawing.Point(29, 68);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(129, 69);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Rounded MT Bold", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Navy;
            this.label3.Location = new System.Drawing.Point(306, 198);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 32);
            this.label3.TabIndex = 27;
            this.label3.Text = "Reuniones";
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1193, 689);
            this.ControlBox = false;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnModificarReunion);
            this.Controls.Add(this.btnEliminarReunion);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.icono_asistencia)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.Label lblFechaHora;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblNombreYApellido;
        private System.Windows.Forms.Button btnConfirmarAsistencia;
        private System.Windows.Forms.Button btnReporte;
        private System.Windows.Forms.Button btnEliminarReunion;
        private System.Windows.Forms.Button btnModificarReunion;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.PictureBox icono_asistencia;
        private System.Windows.Forms.Label lblRol;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
    }
}