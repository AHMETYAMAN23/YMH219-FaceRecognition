using System.Text;

namespace FaceRecognition.Services;

/// <summary>
/// Uygulamanın kullandığı klasörler. Veriler kullanıcının Belgeler klasöründe tutulur:
/// Belgeler\YMH219-FaceRecognition\{dataset, detected_faces, model}
/// </summary>
public static class AppPaths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "YMH219-FaceRecognition");

    /// <summary>Etiket başına alt klasörlerde eğitim örnekleri.</summary>
    public static string Dataset => Path.Combine(Root, "dataset");

    /// <summary>"Algılanan yüzleri kaydet" seçeneği açıkken kaydedilen yüzler.</summary>
    public static string DetectedFaces => Path.Combine(Root, "detected_faces");

    public static string ModelDirectory => Path.Combine(Root, "model");

    /// <summary>Haar cascade yüz algılama modeli (uygulama ile birlikte dağıtılır).</summary>
    public static string CascadeFile => Path.Combine(AppContext.BaseDirectory, "Models", "haarcascade_frontalface_default.xml");

    /// <summary>
    /// OpenCV'nin yerel (C++) dosya fonksiyonları ASCII dışı karakter içeren yolları
    /// (ör. "Masaüstü") açamayabilir. Böyle bir yol varsa dosyanın geçici klasördeki
    /// bir kopyasının yolunu döndürür.
    /// </summary>
    public static string ToNativeSafeReadPath(string path)
    {
        if (IsAscii(path))
        {
            return path;
        }

        string temp = Path.Combine(Path.GetTempPath(), "YMH219Face_" + Path.GetFileName(path));
        File.Copy(path, temp, overwrite: true);
        return temp;
    }

    public static bool IsAscii(string text) => Encoding.UTF8.GetByteCount(text) == text.Length;
}
