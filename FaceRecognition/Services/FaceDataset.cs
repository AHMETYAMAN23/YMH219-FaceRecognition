using OpenCvSharp;

namespace FaceRecognition.Services;

/// <summary>
/// Yerel eğitim örneklerini yönetir. Her etiket (kişi) için ayrı bir klasör bulunur:
/// dataset\Ahmet\20261104_101500_123.png
/// </summary>
public class FaceDataset
{
    public FaceDataset(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        Directory.CreateDirectory(rootDirectory);
    }

    public string RootDirectory { get; }

    /// <summary>Etiketleri ve her etiketteki örnek sayısını döndürür.</summary>
    public IReadOnlyList<LabelInfo> GetLabels() =>
        Directory.GetDirectories(RootDirectory)
            .Select(dir => new LabelInfo(Path.GetFileName(dir), Directory.GetFiles(dir, "*.png").Length))
            .Where(l => l.SampleCount > 0)
            .OrderBy(l => l.Name)
            .ToList();

    /// <summary>Hazırlanmış (gri, 200x200) bir yüz görüntüsünü verilen etiket altına kaydeder.</summary>
    /// <exception cref="ArgumentException">Etiket geçersizse.</exception>
    public string AddSample(string label, Mat preparedFace)
    {
        string dir = Path.Combine(RootDirectory, ValidateLabel(label));
        Directory.CreateDirectory(dir);

        string path = Path.Combine(dir, $"{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
        // Cv2.ImWrite ASCII dışı yollarda sorun çıkarabildiği için önce belleğe kodlanır
        Cv2.ImEncode(".png", preparedFace, out byte[] bytes);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    public void DeleteLabel(string label)
    {
        string dir = Path.Combine(RootDirectory, ValidateLabel(label));
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    /// <summary>
    /// Tüm örnekleri eğitim için yükler. Etiket adları 0'dan başlayan sayılara eşlenir
    /// (LBPH tanıyıcı sayısal etiketlerle çalışır).
    /// </summary>
    public TrainingData LoadAll()
    {
        var images = new List<Mat>();
        var ids = new List<int>();
        var names = new Dictionary<int, string>();

        int id = 0;
        foreach (var label in GetLabels())
        {
            names[id] = label.Name;
            foreach (string file in Directory.GetFiles(Path.Combine(RootDirectory, label.Name), "*.png"))
            {
                var image = Cv2.ImDecode(File.ReadAllBytes(file), ImreadModes.Grayscale);
                if (image.Empty())
                {
                    image.Dispose();
                    continue; // bozuk dosyayı atla
                }

                if (image.Width != FaceDetector.FaceSize || image.Height != FaceDetector.FaceSize)
                {
                    Cv2.Resize(image, image, new OpenCvSharp.Size(FaceDetector.FaceSize, FaceDetector.FaceSize));
                }

                images.Add(image);
                ids.Add(id);
            }

            id++;
        }

        return new TrainingData(images, ids, names);
    }

    /// <summary>Etiketin klasör adı olarak kullanılabilir olduğunu kontrol eder.</summary>
    public static string ValidateLabel(string label)
    {
        string trimmed = (label ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("Etiket (kişi adı) boş olamaz.");
        }

        if (trimmed.Length > 30 || trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || trimmed is "." or "..")
        {
            throw new ArgumentException("Etiket en fazla 30 karakter olmalı ve \\ / : * ? \" < > | karakterlerini içermemelidir.");
        }

        return trimmed;
    }
}

/// <summary>Etiket adı ve örnek sayısı.</summary>
public sealed record LabelInfo(string Name, int SampleCount);

/// <summary>Eğitim için yüklenmiş görüntüler, sayısal etiketler ve etiket adları.</summary>
public sealed class TrainingData : IDisposable
{
    public TrainingData(List<Mat> images, List<int> labels, Dictionary<int, string> names)
    {
        Images = images;
        Labels = labels;
        Names = names;
    }

    public List<Mat> Images { get; }
    public List<int> Labels { get; }
    public Dictionary<int, string> Names { get; }

    public void Dispose()
    {
        foreach (var image in Images)
        {
            image.Dispose();
        }
    }
}
