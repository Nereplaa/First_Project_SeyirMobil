namespace SeyirMobil.Desktop;

partial class VehiclesForm
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

    private System.Windows.Forms.Label lblPlaka;
    private System.Windows.Forms.TextBox txtPlaka;
    private System.Windows.Forms.Label lblTotalKm;
    private System.Windows.Forms.TextBox txtTotalKm;
    private System.Windows.Forms.Button btnEkle;
    private System.Windows.Forms.Button btnSil;
    private System.Windows.Forms.Button btnYenile;
    private System.Windows.Forms.DataGridView dgvVehicles;
    private System.Windows.Forms.Label lblStatus;

    private void InitializeComponent()
    {
        this.lblPlaka = new System.Windows.Forms.Label();
        this.txtPlaka = new System.Windows.Forms.TextBox();
        this.lblTotalKm = new System.Windows.Forms.Label();
        this.txtTotalKm = new System.Windows.Forms.TextBox();
        this.btnEkle = new System.Windows.Forms.Button();
        this.btnSil = new System.Windows.Forms.Button();
        this.btnYenile = new System.Windows.Forms.Button();
        this.dgvVehicles = new System.Windows.Forms.DataGridView();
        this.lblStatus = new System.Windows.Forms.Label();
        ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).BeginInit();
        this.SuspendLayout();
        //
        // lblPlaka
        //
        this.lblPlaka.AutoSize = true;
        this.lblPlaka.Location = new System.Drawing.Point(12, 18);
        this.lblPlaka.Name = "lblPlaka";
        this.lblPlaka.Size = new System.Drawing.Size(40, 15);
        this.lblPlaka.TabIndex = 0;
        this.lblPlaka.Text = "Plaka:";
        //
        // txtPlaka
        //
        this.txtPlaka.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        this.txtPlaka.Location = new System.Drawing.Point(58, 15);
        this.txtPlaka.MaxLength = 15;
        this.txtPlaka.Name = "txtPlaka";
        this.txtPlaka.Size = new System.Drawing.Size(110, 23);
        this.txtPlaka.TabIndex = 1;
        //
        // lblTotalKm
        //
        this.lblTotalKm.AutoSize = true;
        this.lblTotalKm.Location = new System.Drawing.Point(184, 18);
        this.lblTotalKm.Name = "lblTotalKm";
        this.lblTotalKm.Size = new System.Drawing.Size(75, 15);
        this.lblTotalKm.TabIndex = 2;
        this.lblTotalKm.Text = "Toplam KM:";
        //
        // txtTotalKm
        //
        this.txtTotalKm.Location = new System.Drawing.Point(265, 15);
        this.txtTotalKm.Name = "txtTotalKm";
        this.txtTotalKm.Size = new System.Drawing.Size(110, 23);
        this.txtTotalKm.TabIndex = 3;
        //
        // btnEkle
        //
        this.btnEkle.Location = new System.Drawing.Point(391, 14);
        this.btnEkle.Name = "btnEkle";
        this.btnEkle.Size = new System.Drawing.Size(80, 25);
        this.btnEkle.TabIndex = 4;
        this.btnEkle.Text = "Ekle";
        this.btnEkle.UseVisualStyleBackColor = true;
        this.btnEkle.Click += new System.EventHandler(this.btnEkle_Click);
        //
        // btnSil
        //
        this.btnSil.Enabled = false;
        this.btnSil.Location = new System.Drawing.Point(477, 14);
        this.btnSil.Name = "btnSil";
        this.btnSil.Size = new System.Drawing.Size(80, 25);
        this.btnSil.TabIndex = 5;
        this.btnSil.Text = "Sil";
        this.btnSil.UseVisualStyleBackColor = true;
        this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
        //
        // btnYenile
        //
        this.btnYenile.Location = new System.Drawing.Point(597, 14);
        this.btnYenile.Name = "btnYenile";
        this.btnYenile.Size = new System.Drawing.Size(80, 25);
        this.btnYenile.TabIndex = 6;
        this.btnYenile.Text = "Yenile";
        this.btnYenile.UseVisualStyleBackColor = true;
        this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
        //
        // dgvVehicles
        //
        this.dgvVehicles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
        this.dgvVehicles.AllowUserToAddRows = false;
        this.dgvVehicles.AllowUserToDeleteRows = false;
        this.dgvVehicles.AutoGenerateColumns = false;
        this.dgvVehicles.ReadOnly = true;
        this.dgvVehicles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.dgvVehicles.Location = new System.Drawing.Point(12, 52);
        this.dgvVehicles.Name = "dgvVehicles";
        this.dgvVehicles.RowHeadersVisible = false;
        this.dgvVehicles.Size = new System.Drawing.Size(665, 390);
        this.dgvVehicles.TabIndex = 7;
        this.dgvVehicles.SelectionChanged += new System.EventHandler(this.dgvVehicles_SelectionChanged);
        //
        // lblStatus
        //
        this.lblStatus.AutoSize = true;
        this.lblStatus.ForeColor = System.Drawing.Color.DimGray;
        this.lblStatus.Location = new System.Drawing.Point(12, 452);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(0, 15);
        this.lblStatus.TabIndex = 8;
        this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        //
        // VehiclesForm
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(689, 481);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.dgvVehicles);
        this.Controls.Add(this.btnYenile);
        this.Controls.Add(this.btnSil);
        this.Controls.Add(this.btnEkle);
        this.Controls.Add(this.txtTotalKm);
        this.Controls.Add(this.lblTotalKm);
        this.Controls.Add(this.txtPlaka);
        this.Controls.Add(this.lblPlaka);
        this.MinimumSize = new System.Drawing.Size(500, 350);
        this.Name = "VehiclesForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Seyir Mobil - Araç Takip Sistemi";
        this.Load += new System.EventHandler(this.VehiclesForm_Load);
        ((System.ComponentModel.ISupportInitialize)(this.dgvVehicles)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
