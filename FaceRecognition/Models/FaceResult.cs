using FaceRecognition.Services;

namespace FaceRecognition.Models;

/// <summary>Görüntüde bulunan tek bir yüz: sıra numarası, konumu ve tanıma sonucu.</summary>
public sealed record FaceResult(int Index, OpenCvSharp.Rect Bounds, RecognitionResult Recognition);
