namespace SeyirMobil.Desktop;

partial class AracHareketleriForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private System.Windows.Forms.FlowLayoutPanel flowUst;

    private System.Windows.Forms.FlowLayoutPanel groupPlaka;
    private System.Windows.Forms.Label lblPlaka;
    private System.Windows.Forms.ComboBox cmbPlaka;

    private System.Windows.Forms.FlowLayoutPanel groupTarih;
    private System.Windows.Forms.Label lblTarih;
    private System.Windows.Forms.DateTimePicker dtpTarih;

    private System.Windows.Forms.FlowLayoutPanel groupHiz;
    private System.Windows.Forms.Label lblHiz;
    private System.Windows.Forms.NumericUpDown nudHiz;

    private System.Windows.Forms.FlowLayoutPanel groupKm;
    private System.Windows.Forms.Label lblKm;
    private System.Windows.Forms.NumericUpDown nudKm;
    private System.Windows.Forms.Label lblSinirBilgisi;

    private System.Windows.Forms.Button btnEkle;
    private System.Windows.Forms.Button btnSil;
    private System.Windows.Forms.Button btnYenile;
    private System.Windows.Forms.Button btnHareketRaporu;
    private System.Windows.Forms.DataGridView dgvHareketler;
    private System.Windows.Forms.Label lblStatus;

    private void InitializeComponent()
    {
        this.flowUst = new System.Windows.Forms.FlowLayoutPanel();
        this.groupPlaka = new System.Windows.Forms.FlowLayoutPanel();
        this.lblPlaka = new System.Windows.Forms.Label();
        this.cmbPlaka = new System.Windows.Forms.ComboBox();
        this.groupTarih = new System.Windows.Forms.FlowLayoutPanel();
        this.lblTarih = new System.Windows.Forms.Label();
        this.dtpTarih = new System.Windows.Forms.DateTimePicker();
        this.groupHiz = new System.Windows.Forms.FlowLayoutPanel();
        this.lblHiz = new System.Windows.Forms.Label();
        this.nudHiz = new System.Windows.Forms.NumericUpDown();
        this.groupKm = new System.Windows.Forms.FlowLayoutPanel();
        this.lblKm = new System.Windows.Forms.Label();
        this.nudKm = new System.Windows.Forms.NumericUpDown();
        this.lblSinirBilgisi = new System.Windows.Forms.Label();
        this.btnEkle = new System.Windows.Forms.Button();
        this.btnSil = new System.Windows.Forms.Button();
        this.btnYenile = new System.Windows.Forms.Button();
        this.btnHareketRaporu = new System.Windows.Forms.Button();
        this.dgvHareketler = new System.Windows.Forms.DataGridView();
        this.lblStatus = new System.Windows.Forms.Label();
        this.flowUst.SuspendLayout();
        this.groupPlaka.SuspendLayout();
        this.groupTarih.SuspendLayout();
        this.groupHiz.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudHiz)).BeginInit();
        this.groupKm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudKm)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).BeginInit();
        this.SuspendLayout();
        //
        // flowUst  -- pencere daralinca gruplar/butonlar alt satira kayar (responsive)
        //
        this.flowUst.AutoSize = true;
        this.flowUst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowUst.Dock = System.Windows.Forms.DockStyle.Top;
        this.flowUst.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowUst.WrapContents = true;
        this.flowUst.Padding = new System.Windows.Forms.Padding(10);
        this.flowUst.Controls.Add(this.groupPlaka);
        this.flowUst.Controls.Add(this.groupTarih);
        this.flowUst.Controls.Add(this.groupHiz);
        this.flowUst.Controls.Add(this.groupKm);
        this.flowUst.Controls.Add(this.btnEkle);
        this.flowUst.Controls.Add(this.btnSil);
        this.flowUst.Controls.Add(this.btnYenile);
        this.flowUst.Controls.Add(this.btnHareketRaporu);
        this.flowUst.Name = "flowUst";
        this.flowUst.TabIndex = 0;
        //
        // groupPlaka  -- Adim 1: plaka secimi (mevcut araclardan, lookup)
        //
        this.groupPlaka.AutoSize = true;
        this.groupPlaka.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupPlaka.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupPlaka.Controls.Add(this.lblPlaka);
        this.groupPlaka.Controls.Add(this.cmbPlaka);
        this.groupPlaka.Name = "groupPlaka";
        this.groupPlaka.TabIndex = 0;
        //
        // lblPlaka
        //
        this.lblPlaka.AutoSize = true;
        this.lblPlaka.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblPlaka.Name = "lblPlaka";
        this.lblPlaka.Text = "1) Plaka:";
        //
        // cmbPlaka
        //
        this.cmbPlaka.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPlaka.Margin = new System.Windows.Forms.Padding(0);
        this.cmbPlaka.Name = "cmbPlaka";
        this.cmbPlaka.Size = new System.Drawing.Size(160, 23);
        this.cmbPlaka.TabIndex = 0;
        this.cmbPlaka.SelectedIndexChanged += new System.EventHandler(this.cmbPlaka_SelectedIndexChanged);
        //
        // groupTarih  -- Adim 2: veri tarihi (varsayilan bugun, degistirilebilir)
        //
        this.groupTarih.AutoSize = true;
        this.groupTarih.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupTarih.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupTarih.Visible = false;
        this.groupTarih.Controls.Add(this.lblTarih);
        this.groupTarih.Controls.Add(this.dtpTarih);
        this.groupTarih.Name = "groupTarih";
        this.groupTarih.TabIndex = 1;
        //
        // lblTarih
        //
        this.lblTarih.AutoSize = true;
        this.lblTarih.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblTarih.Name = "lblTarih";
        this.lblTarih.Text = "2) Veri Tarihi:";
        //
        // dtpTarih
        //
        this.dtpTarih.Format = System.Windows.Forms.DateTimePickerFormat.Short;
        this.dtpTarih.Margin = new System.Windows.Forms.Padding(0);
        this.dtpTarih.Name = "dtpTarih";
        this.dtpTarih.Size = new System.Drawing.Size(160, 23);
        this.dtpTarih.TabIndex = 0;
        this.dtpTarih.ValueChanged += new System.EventHandler(this.dtpTarih_ValueChanged);
        //
        // groupHiz  -- Adim 3: hiz (NumericUpDown kendi sinirini zaten uyguluyor -> "dogrulanmis")
        //
        this.groupHiz.AutoSize = true;
        this.groupHiz.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupHiz.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupHiz.Visible = false;
        this.groupHiz.Controls.Add(this.lblHiz);
        this.groupHiz.Controls.Add(this.nudHiz);
        this.groupHiz.Name = "groupHiz";
        this.groupHiz.TabIndex = 2;
        //
        // lblHiz
        //
        this.lblHiz.AutoSize = true;
        this.lblHiz.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblHiz.Name = "lblHiz";
        this.lblHiz.Text = "3) Hız (km/s):";
        //
        // nudHiz
        //
        this.nudHiz.Margin = new System.Windows.Forms.Padding(0);
        this.nudHiz.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
        this.nudHiz.Name = "nudHiz";
        this.nudHiz.Size = new System.Drawing.Size(160, 23);
        this.nudHiz.TabIndex = 0;
        this.nudHiz.ValueChanged += new System.EventHandler(this.nudHiz_ValueChanged);
        //
        // groupKm  -- Adim 4: km sayaci, min/max onceki/sonraki okumaya gore CANLI hesaplaniyor
        //
        this.groupKm.AutoSize = true;
        this.groupKm.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
        this.groupKm.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
        this.groupKm.Visible = false;
        this.groupKm.Controls.Add(this.lblKm);
        this.groupKm.Controls.Add(this.nudKm);
        this.groupKm.Controls.Add(this.lblSinirBilgisi);
        this.groupKm.Name = "groupKm";
        this.groupKm.TabIndex = 3;
        //
        // lblKm
        //
        this.lblKm.AutoSize = true;
        this.lblKm.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.lblKm.Name = "lblKm";
        this.lblKm.Text = "4) Km Sayacı:";
        //
        // nudKm
        //
        this.nudKm.DecimalPlaces = 2;
        this.nudKm.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
        this.nudKm.Maximum = new decimal(new int[] { 99999999, 0, 0, 0 });
        this.nudKm.Name = "nudKm";
        this.nudKm.Size = new System.Drawing.Size(200, 23);
        this.nudKm.TabIndex = 0;
        //
        // lblSinirBilgisi
        //
        this.lblSinirBilgisi.AutoSize = true;
        this.lblSinirBilgisi.ForeColor = System.Drawing.Color.SteelBlue;
        this.lblSinirBilgisi.Margin = new System.Windows.Forms.Padding(0);
        this.lblSinirBilgisi.MaximumSize = new System.Drawing.Size(220, 0);
        this.lblSinirBilgisi.Name = "lblSinirBilgisi";
        this.lblSinirBilgisi.Text = "";
        //
        // btnEkle
        //
        this.btnEkle.Enabled = false;
        this.btnEkle.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnEkle.Name = "btnEkle";
        this.btnEkle.Size = new System.Drawing.Size(90, 30);
        this.btnEkle.TabIndex = 4;
        this.btnEkle.Text = "Ekle";
        this.btnEkle.UseVisualStyleBackColor = true;
        this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
        //
        // btnSil
        //
        this.btnSil.Enabled = false;
        this.btnSil.Margin = new System.Windows.Forms.Padding(3, 26, 10, 3);
        this.btnSil.Name = "btnSil";
        this.btnSil.Size = new System.Drawing.Size(90, 30);
        this.btnSil.TabIndex = 5;
        this.btnSil.Text = "Sil";
        this.btnSil.UseVisualStyleBackColor = true;
        this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
        //
        // btnYenile
        //
        this.btnYenile.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnYenile.Name = "btnYenile";
        this.btnYenile.Size = new System.Drawing.Size(90, 30);
        this.btnYenile.TabIndex = 6;
        this.btnYenile.Text = "Yenile";
        this.btnYenile.UseVisualStyleBackColor = true;
        this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
        //
        // btnHareketRaporu
        //
        this.btnHareketRaporu.Margin = new System.Windows.Forms.Padding(3, 26, 3, 3);
        this.btnHareketRaporu.Name = "btnHareketRaporu";
        this.btnHareketRaporu.Size = new System.Drawing.Size(200, 30);
        this.btnHareketRaporu.TabIndex = 7;
        this.btnHareketRaporu.Text = "Araç Hareket Raporu...";
        this.btnHareketRaporu.UseVisualStyleBackColor = true;
        this.btnHareketRaporu.Click += new System.EventHandler(this.btnHareketRaporu_Click);
        //
        // dgvHareketler
        //
        this.dgvHareketler.AllowUserToAddRows = false;
        this.dgvHareketler.AllowUserToDeleteRows = false;
        this.dgvHareketler.AutoGenerateColumns = false;
        this.dgvHareketler.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dgvHareketler.ReadOnly = true;
        this.dgvHareketler.RowHeadersVisible = false;
        this.dgvHareketler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvHareketler.Name = "dgvHareketler";
        this.dgvHareketler.TabIndex = 1;
        this.dgvHareketler.SelectionChanged += new System.EventHandler(this.dgvHareketler_SelectionChanged);
        //
        // lblStatus
        //
        this.lblStatus.AutoSize = false;
        this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
        this.lblStatus.Height = 28;
        this.lblStatus.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
        this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.TabIndex = 2;
        //
        // AracHareketleriForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1000, 600);
        this.Controls.Add(this.flowUst);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.dgvHareketler);
        this.MinimumSize = new System.Drawing.Size(420, 320);
        this.Name = "AracHareketleriForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Seyir Mobil - Araç Hareketleri";
        this.Load += new System.EventHandler(this.AracHareketleriForm_Load);
        this.flowUst.ResumeLayout(false);
        this.flowUst.PerformLayout();
        this.groupPlaka.ResumeLayout(false);
        this.groupPlaka.PerformLayout();
        this.groupTarih.ResumeLayout(false);
        this.groupTarih.PerformLayout();
        this.groupHiz.ResumeLayout(false);
        this.groupHiz.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudHiz)).EndInit();
        this.groupKm.ResumeLayout(false);
        this.groupKm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.nudKm)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).EndInit();
        this.ResumeLayout(false);
    }
}
