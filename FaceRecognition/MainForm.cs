using System.Diagnostics;
using FaceRecognition.Models;
using FaceRecognition.Services;
using OpenCvSharp;

namespace FaceRecognition;

/// <summary>
/// Ana form: kamera veya dosyadan alınan görüntüde yüzleri algılar, eğitilmiş LBPH modeli
/// ile tanır ve sonuçları gösterir. Sağdaki bölümden yerel örnek eklenip model eğitilir.
/// </summary>
public partial class MainForm : Form
{
    private static readonly TimeSpan SaveDetectedInterval = TimeSpan.FromSeconds(1);

    private readonly CameraService _camera = new();
    private readonly FaceRecognizerService _recognizer = new();
    private readonly FaceDataset _dataset = new(AppPaths.Dataset);
    private readonly FaceDetector? _detector;
    private readonly FaceProcessor? _processor;
    private readonly string? _startupError;

    /// <summary>Son işlenen kare (örnek eklemek için) ve o karedeki yüzler.</summary>
    private readonly object _frameSync = new();
    private Mat? _lastFrame;
    private IReadOnlyList<FaceResult> _lastFaces = Array.Empty<FaceResult>();

    /// <summary>Arayüz önceki kareyi göstermeden yeni kare işlenmesin (birikmeyi önler).</summary>
    private int _uiBusy;
    private DateTime _lastDetectedSave = DateTime.MinValue;

    /// <summary>
    /// Onay kutusunun durumu; arka plan iş parçacığı kontrole doğrudan erişmesin diye ayrı tutulur.
    /// </summary>
    private volatile bool _saveDetectedEnabled;

    public MainForm()
    {
        InitializeComponent();

        try
        {
            _detector = new FaceDetector(AppPaths.CascadeFile);
            _processor = new FaceProcessor(_detector, _recognizer);
        }
        catch (Exception ex) when (ex is FileNotFoundException or InvalidDataException)
        {
            _startupError = ex.Message;
        }

        try
        {
            if (_recognizer.TryLoad(AppPaths.ModelDirectory))
            {
                lblStatusBar.Text = "Kayıtlı model yüklendi.";
            }
        }
        catch (Exception ex) when (ex is IOException or OpenCVException or UnauthorizedAccessException)
        {
            lblStatusBar.Text = "Kayıtlı model yüklenemedi: " + ex.Message;
        }

        _camera.FrameCaptured += Camera_FrameCaptured;
        _camera.CameraError += (_, message) => RunOnUi(() =>
        {
            _camera.Stop();
            UpdateUi();
            ShowError(message, "Kamera Hatası");
        });

        RefreshLabels();
        UpdateUi();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        if (_startupError != null)
        {
            ShowError(_startupError + "\n\nYüz algılama özellikleri devre dışı bırakıldı.", "Model Dosyası Bulunamadı");
        }
    }

    private void RunOnUi(Action action)
    {
        if (IsDisposed || Disposing)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
        }
        else
        {
            action();
        }
    }

    // =====================================================================
    //  Kamera ve görüntü
    // =====================================================================

    private void btnStartCamera_Click(object? sender, EventArgs e)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            _camera.Start((int)numCamera.Value);
            lblStatusBar.Text = $"Kamera {numCamera.Value} çalışıyor.";
        }
        catch (CameraException ex)
        {
            ShowError(ex.Message, "Kamera Hatası");
        }
        finally
        {
            Cursor = Cursors.Default;
            UpdateUi();
        }
    }

    private void btnStopCamera_Click(object? sender, EventArgs e)
    {
        _camera.Stop();
        lblStatusBar.Text = "Kamera durduruldu.";
        UpdateUi();
    }

    /// <summary>Kamera karesi (arka plan iş parçacığında) geldiğinde çalışır.</summary>
    private void Camera_FrameCaptured(object? sender, Mat frame)
    {
        if (_processor is null || Interlocked.CompareExchange(ref _uiBusy, 1, 0) != 0)
        {
            return; // Arayüz hâlâ önceki kareyi gösteriyor; bu kareyi atla
        }

        try
        {
            ProcessedFrame processed = _processor.Process(frame);
            StoreLastFrame(frame, processed.Faces);
            SaveDetectedFacesIfEnabled(frame, processed.Faces);

            RunOnUi(() =>
            {
                ShowFrame(processed);
                Interlocked.Exchange(ref _uiBusy, 0);
            });
        }
        catch (OpenCVException)
        {
            Interlocked.Exchange(ref _uiBusy, 0);
        }
    }

    private void btnOpenImage_Click(object? sender, EventArgs e)
    {
        if (openImageDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        LoadImage(openImageDialog.FileName);
    }

    /// <summary>Seçilen resmi yükler ve üzerinde yüz algılama/tanıma yapar.</summary>
    private void LoadImage(string path)
    {
        if (_processor is null)
        {
            ShowError(_startupError ?? "Yüz algılama kullanılamıyor.");
            return;
        }

        if (_camera.IsRunning)
        {
            _camera.Stop();
            UpdateUi();
        }

        try
        {
            // ImDecode, Türkçe karakterli dosya yollarında da çalışır
            using Mat image = Cv2.ImDecode(File.ReadAllBytes(path), ImreadModes.Color);
            if (image.Empty())
            {
                ShowError("Görüntü okunamadı. Dosya bozuk veya desteklenmeyen bir formatta olabilir.");
                return;
            }

            ProcessedFrame processed = _processor.Process(image);
            StoreLastFrame(image, processed.Faces);
            SaveDetectedFacesIfEnabled(image, processed.Faces);
            ShowFrame(processed);
            lblStatusBar.Text = $"{Path.GetFileName(path)}: {processed.Faces.Count} yüz algılandı.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OpenCVException)
        {
            ShowError("Görüntü işlenemedi:\n" + ex.Message);
        }
    }

    /// <summary>Son kareyi tekrar işler (ör. model eğitildikten sonra yeni etiketleri göstermek için).</summary>
    private void ReprocessLastFrame()
    {
        if (_processor is null || _camera.IsRunning)
        {
            return;
        }

        Mat? frame;
        lock (_frameSync)
        {
            frame = _lastFrame?.Clone();
        }

        if (frame is null)
        {
            return;
        }

        using (frame)
        {
            ProcessedFrame processed = _processor.Process(frame);
            StoreLastFrame(frame, processed.Faces);
            ShowFrame(processed);
        }
    }

    private void StoreLastFrame(Mat frame, IReadOnlyList<FaceResult> faces)
    {
        lock (_frameSync)
        {
            _lastFrame?.Dispose();
            _lastFrame = frame.Clone();
            _lastFaces = faces;
        }
    }

    /// <summary>İşlenmiş kareyi PictureBox'ta ve sonuç listesinde gösterir.</summary>
    private void ShowFrame(ProcessedFrame processed)
    {
        Image? old = picView.Image;
        picView.Image = processed.Image;
        old?.Dispose();

        lvFaces.BeginUpdate();
        lvFaces.Items.Clear();
        foreach (var face in processed.Faces)
        {
            var item = new ListViewItem("#" + face.Index);
            item.SubItems.Add(face.Recognition.Label);
            item.SubItems.Add(face.Recognition.ConfidencePercent is { } c ? $"%{c:0}" : "-");
            item.SubItems.Add(face.Recognition.Distance is { } d ? d.ToString("0.0") : "-");
            item.ForeColor = face.Recognition.IsKnown ? Color.DarkGreen : Color.Black;
            lvFaces.Items.Add(item);
        }

        lvFaces.EndUpdate();
        grpResults.Text = $"Algılama / Tanıma Sonuçları ({processed.Faces.Count} yüz)";
    }

    /// <summary>"Algılanan yüzleri kaydet" açıksa yüzleri en fazla saniyede bir kez klasöre kaydeder.</summary>
    private void SaveDetectedFacesIfEnabled(Mat frame, IReadOnlyList<FaceResult> faces)
    {
        if (!_saveDetectedEnabled || faces.Count == 0 || DateTime.Now - _lastDetectedSave < SaveDetectedInterval)
        {
            return;
        }

        _lastDetectedSave = DateTime.Now;
        try
        {
            Directory.CreateDirectory(AppPaths.DetectedFaces);
            foreach (var face in faces)
            {
                Rect safe = face.Bounds.Intersect(new Rect(0, 0, frame.Width, frame.Height));
                using var crop = new Mat(frame, safe);
                Cv2.ImEncode(".png", crop, out byte[] bytes);
                File.WriteAllBytes(Path.Combine(AppPaths.DetectedFaces,
                    $"{DateTime.Now:yyyyMMdd_HHmmss_fff}_yuz{face.Index}.png"), bytes);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            RunOnUi(() => lblStatusBar.Text = "Yüz kaydedilemedi: " + ex.Message);
        }
    }

    private void chkSaveDetected_CheckedChanged(object? sender, EventArgs e)
    {
        _saveDetectedEnabled = chkSaveDetected.Checked;
        if (_saveDetectedEnabled)
        {
            lblStatusBar.Text = "Algılanan yüzler şu klasöre kaydedilecek: " + AppPaths.DetectedFaces;
        }
    }

    // =====================================================================
    //  Eğitim / veri ekleme
    // =====================================================================

    private void btnAddSample_Click(object? sender, EventArgs e)
    {
        string label;
        try
        {
            label = FaceDataset.ValidateLabel(txtLabel.Text);
        }
        catch (ArgumentException ex)
        {
            ShowError(ex.Message, "Geçersiz Etiket");
            return;
        }

        Mat? frame;
        FaceResult? largest;
        lock (_frameSync)
        {
            frame = _lastFrame?.Clone();
            largest = _lastFaces.OrderByDescending(f => f.Bounds.Width * f.Bounds.Height).FirstOrDefault();
        }

        using (frame)
        {
            if (frame is null || largest is null)
            {
                ShowError("Görüntüde yüz bulunamadı. Kamerayı başlatın veya yüz içeren bir resim seçin.", "Yüz Yok");
                return;
            }

            using Mat prepared = FaceDetector.PrepareFace(frame, largest.Bounds);
            _dataset.AddSample(label, prepared);
        }

        RefreshLabels();
        lblStatusBar.Text = $"'{label}' etiketine yeni örnek eklendi (yüz #{largest.Index}).";
    }

    private void btnAddFromFiles_Click(object? sender, EventArgs e)
    {
        if (_detector is null)
        {
            return;
        }

        string label;
        try
        {
            label = FaceDataset.ValidateLabel(txtLabel.Text);
        }
        catch (ArgumentException ex)
        {
            ShowError(ex.Message, "Geçersiz Etiket");
            return;
        }

        if (openSamplesDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int added = 0, skipped = 0;
        Cursor = Cursors.WaitCursor;
        try
        {
            foreach (string file in openSamplesDialog.FileNames)
            {
                try
                {
                    using Mat image = Cv2.ImDecode(File.ReadAllBytes(file), ImreadModes.Color);
                    Rect[] faces = image.Empty() ? Array.Empty<Rect>() : _detector.Detect(image);
                    if (faces.Length == 0)
                    {
                        skipped++;
                        continue;
                    }

                    Rect largest = faces.OrderByDescending(r => r.Width * r.Height).First();
                    using Mat prepared = FaceDetector.PrepareFace(image, largest);
                    _dataset.AddSample(label, prepared);
                    added++;
                }
                catch (Exception ex) when (ex is IOException or OpenCVException)
                {
                    skipped++;
                }
            }
        }
        finally
        {
            Cursor = Cursors.Default;
        }

        RefreshLabels();
        lblStatusBar.Text = $"'{label}': {added} örnek eklendi, {skipped} dosyada yüz bulunamadı/okunamadı.";
    }

    private void btnDeleteLabel_Click(object? sender, EventArgs e)
    {
        if (lvLabels.SelectedItems.Count == 0)
        {
            return;
        }

        string label = lvLabels.SelectedItems[0].Text;
        if (MessageBox.Show(this, $"'{label}' etiketine ait tüm örnekler silinsin mi?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _dataset.DeleteLabel(label);
            RefreshLabels();
            lblStatusBar.Text = $"'{label}' etiketi silindi. Değişikliğin tanımaya yansıması için modeli yeniden eğitin.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError("Etiket silinemedi:\n" + ex.Message);
        }
    }

    private void btnTrain_Click(object? sender, EventArgs e)
    {
        Cursor = Cursors.WaitCursor;
        try
        {
            int count = _recognizer.Train(_dataset);
            _recognizer.Save(AppPaths.ModelDirectory);
            lblStatusBar.Text = $"Model {count} örnekle eğitildi ve kaydedildi.";
            UpdateUi();
            ReprocessLastFrame();
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message, "Yetersiz Veri");
        }
        catch (Exception ex) when (ex is OpenCVException or IOException or UnauthorizedAccessException)
        {
            ShowError("Model eğitilemedi:\n" + ex.Message);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void btnOpenFolder_Click(object? sender, EventArgs e)
    {
        Directory.CreateDirectory(AppPaths.Root);
        Process.Start(new ProcessStartInfo { FileName = AppPaths.Root, UseShellExecute = true });
    }

    private void RefreshLabels()
    {
        lvLabels.BeginUpdate();
        lvLabels.Items.Clear();
        foreach (var label in _dataset.GetLabels())
        {
            var item = new ListViewItem(label.Name);
            item.SubItems.Add(label.SampleCount.ToString());
            lvLabels.Items.Add(item);
        }

        lvLabels.EndUpdate();
        UpdateUi();
    }

    // =====================================================================
    //  Durum
    // =====================================================================

    private void UpdateUi()
    {
        bool detectorReady = _processor != null;
        bool cameraRunning = _camera.IsRunning;

        btnStartCamera.Enabled = detectorReady && !cameraRunning;
        btnStartCamera.BackColor = btnStartCamera.Enabled ? Color.FromArgb(39, 174, 96) : Color.FromArgb(189, 195, 199);
        btnStopCamera.Enabled = cameraRunning;
        btnOpenImage.Enabled = detectorReady;
        numCamera.Enabled = !cameraRunning;
        btnAddSample.Enabled = btnAddFromFiles.Enabled = detectorReady;

        int labelCount = lvLabels.Items.Count;
        if (_recognizer.IsTrained)
        {
            lblModelState.Text = $"Model: eğitildi ({string.Join(", ", _recognizer.LabelNames)})";
            lblModelState.ForeColor = Color.DarkGreen;
        }
        else
        {
            lblModelState.Text = $"Model: eğitilmedi ({labelCount}/{FaceRecognizerService.MinimumLabelCount} etiket)";
            lblModelState.ForeColor = Color.FromArgb(192, 57, 43);
        }
    }

    private void ShowError(string message, string title = "Hata") =>
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _camera.FrameCaptured -= Camera_FrameCaptured;
        _camera.Dispose();
        _recognizer.Dispose();
        _detector?.Dispose();
        lock (_frameSync)
        {
            _lastFrame?.Dispose();
        }

        picView.Image?.Dispose();
    }
}
