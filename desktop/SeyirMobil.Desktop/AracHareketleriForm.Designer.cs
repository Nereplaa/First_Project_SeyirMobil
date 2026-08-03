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
    private System.Windows.Forms.Button btnYenile;
    private System.Windows.Forms.Button btnHareketRaporu;
    private System.Windows.Forms.DataGridView dgvHareketler;
    private System.Windows.Forms.Label lblStatus;

    private void InitializeComponent()
    {
        this.flowUst = new System.Windows.Forms.FlowLayoutPanel();
        this.btnYenile = new System.Windows.Forms.Button();
        this.btnHareketRaporu = new System.Windows.Forms.Button();
        this.dgvHareketler = new System.Windows.Forms.DataGridView();
        this.lblStatus = new System.Windows.Forms.Label();
        this.flowUst.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).BeginInit();
        this.SuspendLayout();
        //
        // flowUst  -- pencere daralinca butonlar alt satira kayar (responsive)
        //
        this.flowUst.AutoSize = true;
        this.flowUst.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        this.flowUst.Dock = System.Windows.Forms.DockStyle.Top;
        this.flowUst.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
        this.flowUst.WrapContents = true;
        this.flowUst.Padding = new System.Windows.Forms.Padding(10);
        this.flowUst.Controls.Add(this.btnYenile);
        this.flowUst.Controls.Add(this.btnHareketRaporu);
        this.flowUst.Name = "flowUst";
        this.flowUst.TabIndex = 0;
        //
        // btnYenile
        //
        this.btnYenile.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
        this.btnYenile.Name = "btnYenile";
        this.btnYenile.Size = new System.Drawing.Size(90, 30);
        this.btnYenile.TabIndex = 0;
        this.btnYenile.Text = "Yenile";
        this.btnYenile.UseVisualStyleBackColor = true;
        this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
        //
        // btnHareketRaporu
        //
        this.btnHareketRaporu.Margin = new System.Windows.Forms.Padding(3);
        this.btnHareketRaporu.Name = "btnHareketRaporu";
        this.btnHareketRaporu.Size = new System.Drawing.Size(200, 30);
        this.btnHareketRaporu.TabIndex = 1;
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
        this.ClientSize = new System.Drawing.Size(760, 520);
        this.Controls.Add(this.flowUst);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.dgvHareketler);
        this.MinimumSize = new System.Drawing.Size(420, 320);
        this.Name = "AracHareketleriForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Seyir Mobil - Araç Hareketleri";
        this.Load += new System.EventHandler(this.AracHareketleriForm_Load);
        this.flowUst.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.dgvHareketler)).EndInit();
        this.ResumeLayout(false);
    }
}
