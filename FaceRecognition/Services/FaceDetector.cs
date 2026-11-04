using OpenCvSharp;

namespace FaceRecognition.Services;

/// <summary>
/// OpenCV Haar Cascade sınıflandırıcısı ile görüntüdeki yüz bölgelerini bulur.
/// </summary>
public sealed class FaceDetector : IDisposable
{
    /// <summary>Tanıma için tüm yüzler bu boyuta getirilir.</summary>
    public const int FaceSize = 200;

    private readonly CascadeClassifier _cascade;
    private readonly object _sync = new();

    /// <exception cref="FileNotFoundException">Model dosyası bulunamazsa.</exception>
    /// <exception cref="InvalidDataException">Model dosyası okunamazsa.</exception>
    public FaceDetector(string cascadePath)
    {
        if (!File.Exists(cascadePath))
        {
            throw new FileNotFoundException(
                "Yüz algılama model dosyası bulunamadı. 'Models' klasöründe " +
                "'haarcascade_frontalface_default.xml' dosyasının olduğundan emin olun.", cascadePath);
        }

        _cascade = new CascadeClassifier(AppPaths.ToNativeSafeReadPath(cascadePath));
        if (_cascade.Empty())
        {
            _cascade.Dispose();
            throw new InvalidDataException("Yüz algılama model dosyası okunamadı: " + cascadePath);
        }
    }

    /// <summary>Renkli (BGR) görüntüdeki yüzlerin dikdörtgenlerini döndürür.</summary>
    public Rect[] Detect(Mat bgrImage)
    {
        using var gray = ToGray(bgrImage);
        Cv2.EqualizeHist(gray, gray);

        // Büyük görüntülerde en küçük yüz boyutunu görüntüye oranla ayarla
        int minFace = Math.Max(40, Math.Min(gray.Width, gray.Height) / 12);

        lock (_sync) // CascadeClassifier aynı anda iki iş parçacığından kullanılmamalı
        {
            return _cascade.DetectMultiScale(
                gray,
                scaleFactor: 1.1,
                minNeighbors: 5,
                flags: HaarDetectionTypes.ScaleImage,
                minSize: new OpenCvSharp.Size(minFace, minFace));
        }
    }

    /// <summary>
    /// Yüz bölgesini kesip gri tonlamaya çevirir, histogramını eşitler ve
    /// <see cref="FaceSize"/> boyutuna getirir. Eğitim ve tanıma aynı ön işlemi kullanır.
    /// </summary>
    public static Mat PrepareFace(Mat bgrImage, Rect faceRect)
    {
        Rect safe = faceRect.Intersect(new Rect(0, 0, bgrImage.Width, bgrImage.Height));
        using var roi = new Mat(bgrImage, safe);
        using var gray = ToGray(roi);
        var result = new Mat();
        Cv2.Resize(gray, result, new OpenCvSharp.Size(FaceSize, FaceSize));
        Cv2.EqualizeHist(result, result);
        return result;
    }

    private static Mat ToGray(Mat image)
    {
        var gray = new Mat();
        if (image.Channels() == 1)
        {
            image.CopyTo(gray);
        }
        else
        {
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        }

        return gray;
    }

    public void Dispose() => _cascade.Dispose();
}
