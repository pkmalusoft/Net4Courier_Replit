using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;
using ZXing.SkiaSharp.Rendering;

namespace Net4Courier.Operations.Services;

public class BarcodeService
{
    public byte[] GenerateBarcode(string content, int width = 300, int height = 80)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 5,
                PureBarcode = false
            },
            Renderer = new SKBitmapRenderer()
        };

        using var bitmap = writer.Write(content);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return data.ToArray();
    }

    public byte[] GenerateBarcodeVertical(string content, int width = 80, int height = 300)
    {
        var writer = new BarcodeWriter<SKBitmap>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = height,
                Height = width,
                Margin = 5,
                PureBarcode = false
            },
            Renderer = new SKBitmapRenderer()
        };

        using var bitmap = writer.Write(content);
        
        using var rotatedBitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(rotatedBitmap);
        
        canvas.Translate(width, 0);
        canvas.RotateDegrees(90);
        canvas.DrawBitmap(bitmap, 0, 0);
        
        using var image = SKImage.FromBitmap(rotatedBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        
        return data.ToArray();
    }

    public (byte[] horizontal, byte[] vertical) GenerateBothBarcodes(string content)
    {
        var horizontal = GenerateBarcode(content, 300, 80);
        var vertical = GenerateBarcodeVertical(content, 40, 250);
        return (horizontal, vertical);
    }
}
