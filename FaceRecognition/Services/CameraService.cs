using OpenCvSharp;

namespace FaceRecognition.Services;

/// <summary>
/// Kameradan arka plan görevinde kare okur ve her kareyi <see cref="FrameCaptured"/> olayı ile bildirir.
/// Olay arka plan iş parçacığında tetiklenir; arayüz güncellemesi Invoke ile yapılmalıdır.
/// </summary>
public sealed class CameraService : IDisposable
{
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    /// <summary>Yeni kare yakalandığında. Olayı işleyen Mat'i kullandıktan sonra serbest bırakmaz; servis bırakır.</summary>
    public event EventHandler<Mat>? FrameCaptured;

    /// <summary>Kamera beklenmedik şekilde koptuğunda.</summary>
    public event EventHandler<string>? CameraError;

    public bool IsRunning => _loop is { IsCompleted: false };

    /// <exception cref="CameraException">Kamera açılamazsa.</exception>
    public void Start(int cameraIndex)
    {
        if (IsRunning)
        {
            return;
        }

        var capture = new VideoCapture(cameraIndex, VideoCaptureAPIs.DSHOW);
        if (!capture.IsOpened())
        {
            capture.Dispose();
            throw new CameraException(
                $"{cameraIndex} numaralı kamera açılamadı. Kameranın bağlı olduğundan, başka bir uygulama " +
                "tarafından kullanılmadığından ve Windows gizlilik ayarlarında kamera izninin açık olduğundan emin olun.");
        }

        capture.Set(VideoCaptureProperties.FrameWidth, 640);
        capture.Set(VideoCaptureProperties.FrameHeight, 480);

        _capture = capture;
        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => CaptureLoop(capture, _cts.Token));
    }

    private void CaptureLoop(VideoCapture capture, CancellationToken ct)
    {
        int emptyFrames = 0;
        using var frame = new Mat();

        while (!ct.IsCancellationRequested)
        {
            if (!capture.Read(frame) || frame.Empty())
            {
                // Arka arkaya çok sayıda boş kare: kamera bağlantısı kopmuş olabilir
                if (++emptyFrames > 50)
                {
                    CameraError?.Invoke(this, "Kameradan görüntü alınamıyor. Bağlantıyı kontrol edin.");
                    return;
                }

                Thread.Sleep(20);
                continue;
            }

            emptyFrames = 0;
            FrameCaptured?.Invoke(this, frame);
        }
    }

    public void Stop()
    {
        _cts?.Cancel();
        try
        {
            _loop?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Döngüdeki hata zaten bildirildi
        }

        _capture?.Release();
        _capture?.Dispose();
        _capture = null;
        _loop = null;
        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose() => Stop();
}

/// <summary>Kamera ile ilgili, kullanıcıya gösterilecek hata.</summary>
public class CameraException : Exception
{
    public CameraException(string message)
        : base(message)
    {
    }
}
