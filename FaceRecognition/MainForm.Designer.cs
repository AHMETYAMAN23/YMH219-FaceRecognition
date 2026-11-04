namespace FaceRecognition;

partial class MainForm
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

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        picView = new PictureBox();
        btnStartCamera = new Button();
        btnStopCamera = new Button();
        btnOpenImage = new Button();
        lblCamera = new Label();
        numCamera = new NumericUpDown();
        chkSaveDetected = new CheckBox();
        lblHelp = new Label();
        grpResults = new GroupBox();
        lblModelState = new Label();
        lvFaces = new ListView();
        colIndex = new ColumnHeader();
        colLabel = new ColumnHeader();
        colConfidence = new ColumnHeader();
        colDistance = new ColumnHeader();
        grpTraining = new GroupBox();
        btnOpenFolder = new Button();
        btnTrain = new Button();
        btnDeleteLabel = new Button();
        lvLabels = new ListView();
        colLabelName = new ColumnHeader();
        colSampleCount = new ColumnHeader();
        btnAddFromFiles = new Button();
        btnAddSample = new Button();
        txtLabel = new TextBox();
        lblLabel = new Label();
        grpEthics = new GroupBox();
        lblEthics = new Label();
        statusStrip = new StatusStrip();
        lblStatusBar = new ToolStripStatusLabel();
        openImageDialog = new OpenFileDialog();
        openSamplesDialog = new OpenFileDialog();
        ((System.ComponentModel.ISupportInitialize)picView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numCamera).BeginInit();
        grpResults.SuspendLayout();
        grpTraining.SuspendLayout();
        grpEthics.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // picView
        //
        picView.BackColor = Color.FromArgb(30, 30, 30);
        picView.BorderStyle = BorderStyle.FixedSingle;
        picView.Location = new Point(12, 12);
        picView.Name = "picView";
        picView.Size = new Size(720, 540);
        picView.SizeMode = PictureBoxSizeMode.Zoom;
        picView.TabIndex = 0;
        picView.TabStop = false;
        //
        // btnStartCamera
        //
        btnStartCamera.BackColor = Color.FromArgb(39, 174, 96);
        btnStartCamera.FlatStyle = FlatStyle.Flat;
        btnStartCamera.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnStartCamera.ForeColor = Color.White;
        btnStartCamera.Location = new Point(12, 562);
        btnStartCamera.Name = "btnStartCamera";
        btnStartCamera.Size = new Size(130, 36);
        btnStartCamera.TabIndex = 1;
        btnStartCamera.Text = "Kamera Başlat";
        btnStartCamera.UseVisualStyleBackColor = false;
        btnStartCamera.Click += btnStartCamera_Click;
        //
        // btnStopCamera
        //
        btnStopCamera.Location = new Point(148, 562);
        btnStopCamera.Name = "btnStopCamera";
        btnStopCamera.Size = new Size(130, 36);
        btnStopCamera.TabIndex = 2;
        btnStopCamera.Text = "Kamera Durdur";
        btnStopCamera.UseVisualStyleBackColor = true;
        btnStopCamera.Click += btnStopCamera_Click;
        //
        // btnOpenImage
        //
        btnOpenImage.Location = new Point(284, 562);
        btnOpenImage.Name = "btnOpenImage";
        btnOpenImage.Size = new Size(130, 36);
        btnOpenImage.TabIndex = 3;
        btnOpenImage.Text = "Görüntü Seç...";
        btnOpenImage.UseVisualStyleBackColor = true;
        btnOpenImage.Click += btnOpenImage_Click;
        //
        // lblCamera
        //
        lblCamera.AutoSize = true;
        lblCamera.Location = new Point(428, 572);
        lblCamera.Name = "lblCamera";
        lblCamera.Size = new Size(64, 15);
        lblCamera.Text = "Kamera no:";
        //
        // numCamera
        //
        numCamera.Location = new Point(500, 568);
        numCamera.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
        numCamera.Name = "numCamera";
        numCamera.Size = new Size(45, 23);
        numCamera.TabIndex = 4;
        //
        // chkSaveDetected
        //
        chkSaveDetected.AutoSize = true;
        chkSaveDetected.Location = new Point(560, 570);
        chkSaveDetected.Name = "chkSaveDetected";
        chkSaveDetected.Size = new Size(170, 19);
        chkSaveDetected.TabIndex = 5;
        chkSaveDetected.Text = "Algılanan yüzleri kaydet";
        chkSaveDetected.UseVisualStyleBackColor = true;
        chkSaveDetected.CheckedChanged += chkSaveDetected_CheckedChanged;
        //
        // lblHelp
        //
        lblHelp.ForeColor = Color.DimGray;
        lblHelp.Location = new Point(12, 604);
        lblHelp.Name = "lblHelp";
        lblHelp.Size = new Size(720, 48);
        lblHelp.Text = "Yeşil: tanınan kişi   •   Turuncu: bilinmeyen yüz   •   Mavi: model henüz eğitilmedi. "
            + "Tanıma için sağdaki bölümden en az iki etiket için örnek ekleyip \"Modeli Eğit\" butonuna basın.";
        //
        // grpResults
        //
        grpResults.Controls.Add(lblModelState);
        grpResults.Controls.Add(lvFaces);
        grpResults.Location = new Point(744, 8);
        grpResults.Name = "grpResults";
        grpResults.Size = new Size(344, 200);
        grpResults.TabIndex = 6;
        grpResults.TabStop = false;
        grpResults.Text = "Algılama / Tanıma Sonuçları";
        //
        // lvFaces
        //
        lvFaces.Columns.AddRange(new ColumnHeader[] { colIndex, colLabel, colConfidence, colDistance });
        lvFaces.FullRowSelect = true;
        lvFaces.GridLines = true;
        lvFaces.Location = new Point(10, 22);
        lvFaces.Name = "lvFaces";
        lvFaces.Size = new Size(324, 140);
        lvFaces.TabIndex = 0;
        lvFaces.UseCompatibleStateImageBehavior = false;
        lvFaces.View = View.Details;
        //
        // colIndex
        //
        colIndex.Text = "Yüz";
        colIndex.Width = 40;
        //
        // colLabel
        //
        colLabel.Text = "Tahmin / Etiket";
        colLabel.Width = 130;
        //
        // colConfidence
        //
        colConfidence.Text = "Güven";
        colConfidence.TextAlign = HorizontalAlignment.Right;
        colConfidence.Width = 65;
        //
        // colDistance
        //
        colDistance.Text = "Mesafe";
        colDistance.TextAlign = HorizontalAlignment.Right;
        colDistance.Width = 65;
        //
        // lblModelState
        //
        lblModelState.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblModelState.Location = new Point(10, 168);
        lblModelState.Name = "lblModelState";
        lblModelState.Size = new Size(324, 24);
        lblModelState.Text = "Model: eğitilmedi";
        //
        // grpTraining
        //
        grpTraining.Controls.Add(btnOpenFolder);
        grpTraining.Controls.Add(btnTrain);
        grpTraining.Controls.Add(btnDeleteLabel);
        grpTraining.Controls.Add(lvLabels);
        grpTraining.Controls.Add(btnAddFromFiles);
        grpTraining.Controls.Add(btnAddSample);
        grpTraining.Controls.Add(txtLabel);
        grpTraining.Controls.Add(lblLabel);
        grpTraining.Location = new Point(744, 214);
        grpTraining.Name = "grpTraining";
        grpTraining.Size = new Size(344, 338);
        grpTraining.TabIndex = 7;
        grpTraining.TabStop = false;
        grpTraining.Text = "Eğitim / Veri Ekleme";
        //
        // lblLabel
        //
        lblLabel.AutoSize = true;
        lblLabel.Location = new Point(10, 28);
        lblLabel.Name = "lblLabel";
        lblLabel.Size = new Size(98, 15);
        lblLabel.Text = "Etiket (kişi adı):";
        //
        // txtLabel
        //
        txtLabel.Location = new Point(120, 24);
        txtLabel.MaxLength = 30;
        txtLabel.Name = "txtLabel";
        txtLabel.PlaceholderText = "ör. Ahmet";
        txtLabel.Size = new Size(214, 23);
        txtLabel.TabIndex = 0;
        //
        // btnAddSample
        //
        btnAddSample.Location = new Point(10, 56);
        btnAddSample.Name = "btnAddSample";
        btnAddSample.Size = new Size(324, 32);
        btnAddSample.TabIndex = 1;
        btnAddSample.Text = "Görüntüdeki Yüzü Örnek Olarak Ekle";
        btnAddSample.UseVisualStyleBackColor = true;
        btnAddSample.Click += btnAddSample_Click;
        //
        // btnAddFromFiles
        //
        btnAddFromFiles.Location = new Point(10, 92);
        btnAddFromFiles.Name = "btnAddFromFiles";
        btnAddFromFiles.Size = new Size(324, 32);
        btnAddFromFiles.TabIndex = 2;
        btnAddFromFiles.Text = "Dosyalardan Örnek Ekle...";
        btnAddFromFiles.UseVisualStyleBackColor = true;
        btnAddFromFiles.Click += btnAddFromFiles_Click;
        //
        // lvLabels
        //
        lvLabels.Columns.AddRange(new ColumnHeader[] { colLabelName, colSampleCount });
        lvLabels.FullRowSelect = true;
        lvLabels.GridLines = true;
        lvLabels.Location = new Point(10, 132);
        lvLabels.MultiSelect = false;
        lvLabels.Name = "lvLabels";
        lvLabels.Size = new Size(324, 110);
        lvLabels.TabIndex = 3;
        lvLabels.UseCompatibleStateImageBehavior = false;
        lvLabels.View = View.Details;
        //
        // colLabelName
        //
        colLabelName.Text = "Etiket";
        colLabelName.Width = 200;
        //
        // colSampleCount
        //
        colSampleCount.Text = "Örnek sayısı";
        colSampleCount.TextAlign = HorizontalAlignment.Right;
        colSampleCount.Width = 100;
        //
        // btnDeleteLabel
        //
        btnDeleteLabel.Location = new Point(10, 248);
        btnDeleteLabel.Name = "btnDeleteLabel";
        btnDeleteLabel.Size = new Size(158, 32);
        btnDeleteLabel.TabIndex = 4;
        btnDeleteLabel.Text = "Seçili Etiketi Sil";
        btnDeleteLabel.UseVisualStyleBackColor = true;
        btnDeleteLabel.Click += btnDeleteLabel_Click;
        //
        // btnOpenFolder
        //
        btnOpenFolder.Location = new Point(176, 248);
        btnOpenFolder.Name = "btnOpenFolder";
        btnOpenFolder.Size = new Size(158, 32);
        btnOpenFolder.TabIndex = 5;
        btnOpenFolder.Text = "Veri Klasörünü Aç";
        btnOpenFolder.UseVisualStyleBackColor = true;
        btnOpenFolder.Click += btnOpenFolder_Click;
        //
        // btnTrain
        //
        btnTrain.BackColor = Color.FromArgb(41, 128, 185);
        btnTrain.FlatStyle = FlatStyle.Flat;
        btnTrain.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        btnTrain.ForeColor = Color.White;
        btnTrain.Location = new Point(10, 288);
        btnTrain.Name = "btnTrain";
        btnTrain.Size = new Size(324, 38);
        btnTrain.TabIndex = 6;
        btnTrain.Text = "Modeli Eğit";
        btnTrain.UseVisualStyleBackColor = false;
        btnTrain.Click += btnTrain_Click;
        //
        // grpEthics
        //
        grpEthics.Controls.Add(lblEthics);
        grpEthics.Location = new Point(744, 556);
        grpEthics.Name = "grpEthics";
        grpEthics.Size = new Size(344, 96);
        grpEthics.TabIndex = 8;
        grpEthics.TabStop = false;
        grpEthics.Text = "Kapsam ve Güvenlik Sınırı";
        //
        // lblEthics
        //
        lblEthics.ForeColor = Color.FromArgb(120, 40, 40);
        lblEthics.Location = new Point(10, 20);
        lblEthics.Name = "lblEthics";
        lblEthics.Size = new Size(324, 70);
        lblEthics.Text = "Yalnızca ders kapsamında, yerel ve gönüllü örneklerle kullanın. "
            + "İzinsiz kimlik takibi veya gizli görüntü toplama yapılmamalıdır. "
            + "Tüm veriler yalnızca bu bilgisayarda saklanır.";
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { lblStatusBar });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
        statusStrip.TabIndex = 9;
        //
        // lblStatusBar
        //
        lblStatusBar.Name = "lblStatusBar";
        lblStatusBar.Size = new Size(39, 17);
        lblStatusBar.Text = "Hazır.";
        //
        // openImageDialog
        //
        openImageDialog.Filter = "Görüntü dosyaları (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
        openImageDialog.Title = "Görüntü seçin";
        //
        // openSamplesDialog
        //
        openSamplesDialog.Filter = "Görüntü dosyaları (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
        openSamplesDialog.Multiselect = true;
        openSamplesDialog.Title = "Örnek görüntüleri seçin (her görüntüdeki en büyük yüz eklenir)";
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 680);
        Controls.Add(statusStrip);
        Controls.Add(grpEthics);
        Controls.Add(grpTraining);
        Controls.Add(grpResults);
        Controls.Add(lblHelp);
        Controls.Add(chkSaveDetected);
        Controls.Add(numCamera);
        Controls.Add(lblCamera);
        Controls.Add(btnOpenImage);
        Controls.Add(btnStopCamera);
        Controls.Add(btnStartCamera);
        Controls.Add(picView);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "YMH219 Yüz Algılama ve Tanıma — OpenCV";
        FormClosing += MainForm_FormClosing;
        Shown += MainForm_Shown;
        ((System.ComponentModel.ISupportInitialize)picView).EndInit();
        ((System.ComponentModel.ISupportInitialize)numCamera).EndInit();
        grpResults.ResumeLayout(false);
        grpTraining.ResumeLayout(false);
        grpTraining.PerformLayout();
        grpEthics.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PictureBox picView;
    private Button btnStartCamera;
    private Button btnStopCamera;
    private Button btnOpenImage;
    private Label lblCamera;
    private NumericUpDown numCamera;
    private CheckBox chkSaveDetected;
    private Label lblHelp;
    private GroupBox grpResults;
    private ListView lvFaces;
    private ColumnHeader colIndex;
    private ColumnHeader colLabel;
    private ColumnHeader colConfidence;
    private ColumnHeader colDistance;
    private Label lblModelState;
    private GroupBox grpTraining;
    private Label lblLabel;
    private TextBox txtLabel;
    private Button btnAddSample;
    private Button btnAddFromFiles;
    private ListView lvLabels;
    private ColumnHeader colLabelName;
    private ColumnHeader colSampleCount;
    private Button btnDeleteLabel;
    private Button btnOpenFolder;
    private Button btnTrain;
    private GroupBox grpEthics;
    private Label lblEthics;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblStatusBar;
    private OpenFileDialog openImageDialog;
    private OpenFileDialog openSamplesDialog;
}
