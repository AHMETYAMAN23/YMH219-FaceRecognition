using System.Drawing.Drawing2D;
using FaceRecognition.Models;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace FaceRecognition.Services;

/// <summary>
/// Bir görüntü karesini işler: yüzleri algılar, her yüz için tanıma yapar ve
/// sonuçları dikdörtgen + etiket olarak görüntü üzerine çizer.
/// </summary>
public sealed class FaceProcessor
{
    private readonly FaceDetector _detector;
    private readonly FaceRecognizerService _recognizer;

    public FaceProcessor(FaceDetector detector, FaceRecognizerService recognizer)
    {
        _detector = detector;
        _recognizer = recognizer;
    }

    /// <summary>Kareyi işler ve işaretlenmiş Bitmap ile yüz sonuçlarını döndürür.</summary>
    public ProcessedFrame Process(Mat bgrFrame)
    {
        Rect[] faces = _detector.Detect(bgrFrame);
        var results = new List<FaceResult>(faces.Length);

        for (int i = 0; i < faces.Length; i++)
        {
            using Mat prepared = FaceDetector.PrepareFace(bgrFrame, faces[i]);
            RecognitionResult recognition = _recognizer.Predict(prepared);
            results.Add(new FaceResult(i + 1, faces[i], recognition));
        }

        Bitmap bitmap = bgrFrame.ToBitmap();
        DrawResults(bitmap, results);
        return new ProcessedFrame(bitmap, results);
    }

    /// <summary>
    /// Dikdörtgen ve etiketleri GDI+ ile çizer (Cv2.PutText Türkçe karakterleri desteklemez).
    /// </summary>
    private static void DrawResults(Bitmap bitmap, IReadOnlyList<FaceResult> results)
    {
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        float scale = Math.Max(1f, bitmap.Width / 640f);
        using var font = new Font("Segoe UI", 11f * scale, FontStyle.Bold, GraphicsUnit.Pixel);

        foreach (var face in results)
        {
            Color color = face.Recognition.IsKnown ? Color.LimeGreen
                : face.Recognition.Distance is null ? Color.DeepSkyBlue
                : Color.OrangeRed;

            var r = face.Bounds;
            using var pen = new Pen(color, 3 * scale);
            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);

            string text = $"#{face.Index} {face.Recognition.Label}";
            if (face.Recognition.ConfidencePercent is { } c)
            {
                text += $" (%{c:0})";
            }

            SizeF size = g.MeasureString(text, font);
            float labelY = r.Y - size.Height - 2 >= 0 ? r.Y - size.Height - 2 : r.Bottom + 2;
            using var back = new SolidBrush(Color.FromArgb(200, color));
            g.FillRectangle(back, r.X, labelY, size.Width + 4, size.Height);
            g.DrawString(text, font, Brushes.Black, r.X + 2, labelY);
        }
    }
}

/// <summary>İşlenmiş kare: ekranda gösterilecek görüntü ve yüz sonuçları.</summary>
public sealed record ProcessedFrame(Bitmap Image, IReadOnlyList<FaceResult> Faces);
