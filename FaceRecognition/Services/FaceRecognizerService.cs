using System.Text;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace FaceRecognition.Services;

/// <summary>
/// LBPH (Local Binary Patterns Histograms) yüz tanıyıcısı. Yerel örneklerle eğitilir,
/// tahmin edilen etiketi ve mesafe (düşük = daha benzer) değerini döndürür.
/// </summary>
public sealed class FaceRecognizerService : IDisposable
{
    /// <summary>En az kaç farklı etiket (kişi/sınıf) ile eğitim yapılacağı.</summary>
    public const int MinimumLabelCount = 2;

    private const string ModelFileName = "lbph_model.yml";
    private const string LabelsFileName = "labels.txt";

    private readonly object _sync = new();
    private LBPHFaceRecognizer? _recognizer;
    private Dictionary<int, string> _names = new();

    /// <summary>
    /// Bu mesafenin üzerindeki tahminler "Bilinmiyor" kabul edilir.
    /// LBPH için tipik olarak 60-90 arası bir değer uygundur.
    /// </summary>
    public double UnknownThreshold { get; set; } = 80;

    public bool IsTrained
    {
        get
        {
            lock (_sync)
            {
                return _recognizer != null;
            }
        }
    }

    public IReadOnlyCollection<string> LabelNames
    {
        get
        {
            lock (_sync)
            {
                return _names.Values.ToList();
            }
        }
    }

    /// <summary>Veri kümesindeki örneklerle modeli eğitir.</summary>
    /// <returns>Eğitimde kullanılan örnek sayısı.</returns>
    /// <exception cref="InvalidOperationException">Yeterli etiket yoksa.</exception>
    public int Train(FaceDataset dataset)
    {
        using TrainingData data = dataset.LoadAll();
        if (data.Names.Count < MinimumLabelCount)
        {
            throw new InvalidOperationException(
                $"Eğitim için en az {MinimumLabelCount} farklı etiket (kişi) gerekir. " +
                $"Şu anda {data.Names.Count} etiket var.");
        }

        var recognizer = LBPHFaceRecognizer.Create(radius: 1, neighbors: 8, gridX: 8, gridY: 8);
        recognizer.Train(data.Images, data.Labels);

        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = recognizer;
            _names = new Dictionary<int, string>(data.Names);
        }

        return data.Images.Count;
    }

    /// <summary>Hazırlanmış (gri, 200x200) yüz için tahmin yapar.</summary>
    public RecognitionResult Predict(Mat preparedFace)
    {
        lock (_sync)
        {
            if (_recognizer is null)
            {
                return RecognitionResult.NotTrained;
            }

            _recognizer.Predict(preparedFace, out int label, out double distance);
            bool known = label >= 0 && distance <= UnknownThreshold && _names.ContainsKey(label);
            string name = known ? _names[label] : "Bilinmiyor";
            return new RecognitionResult(name, distance, known);
        }
    }

    /// <summary>Eğitilmiş modeli ve etiket adlarını diske kaydeder.</summary>
    public void Save(string directory)
    {
        lock (_sync)
        {
            if (_recognizer is null)
            {
                return;
            }

            Directory.CreateDirectory(directory);
            string target = Path.Combine(directory, ModelFileName);

            // OpenCV yerel kodu ASCII dışı yolları yazamayabilir; önce geçici klasöre yaz, sonra taşı
            string temp = Path.Combine(Path.GetTempPath(), "YMH219Face_" + ModelFileName);
            _recognizer.Write(temp);
            File.Copy(temp, target, overwrite: true);
            File.Delete(temp);

            File.WriteAllLines(Path.Combine(directory, LabelsFileName),
                _names.OrderBy(p => p.Key).Select(p => $"{p.Key};{p.Value}"), Encoding.UTF8);
        }
    }

    /// <summary>Daha önce kaydedilmiş modeli yükler. Model yoksa false döner.</summary>
    public bool TryLoad(string directory)
    {
        string modelPath = Path.Combine(directory, ModelFileName);
        string labelsPath = Path.Combine(directory, LabelsFileName);
        if (!File.Exists(modelPath) || !File.Exists(labelsPath))
        {
            return false;
        }

        var names = new Dictionary<int, string>();
        foreach (string line in File.ReadAllLines(labelsPath, Encoding.UTF8))
        {
            string[] parts = line.Split(';', 2);
            if (parts.Length == 2 && int.TryParse(parts[0], out int id))
            {
                names[id] = parts[1];
            }
        }

        var recognizer = LBPHFaceRecognizer.Create();
        recognizer.Read(AppPaths.ToNativeSafeReadPath(modelPath));

        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = recognizer;
            _names = names;
        }

        return true;
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _recognizer?.Dispose();
            _recognizer = null;
        }
    }
}

/// <summary>Tanıma sonucu.</summary>
/// <param name="Label">Tahmin edilen etiket ya da "Bilinmiyor".</param>
/// <param name="Distance">LBPH mesafesi; küçük değer daha yüksek benzerlik demektir.</param>
/// <param name="IsKnown">Mesafe eşik değerinin altında mı?</param>
public sealed record RecognitionResult(string Label, double? Distance, bool IsKnown)
{
    public static RecognitionResult NotTrained { get; } = new("Model eğitilmedi", null, false);

    /// <summary>Mesafeden türetilen yaklaşık güven yüzdesi (0-100).</summary>
    public double? ConfidencePercent => Distance is { } d ? Math.Clamp(100 - d, 0, 100) : null;
}
