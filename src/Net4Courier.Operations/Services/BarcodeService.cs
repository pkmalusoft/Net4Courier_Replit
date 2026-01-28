using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ZXing;
using ZXing.Common;
using ZXing.ImageSharp.Rendering;

namespace Net4Courier.Operations.Services;

public class BarcodeService
{
    public byte[] GenerateBarcode(string content, int width = 300, int height = 80)
    {
        var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 5,
                PureBarcode = false
            },
            Renderer = new ImageSharpRenderer<Rgba32>()
        };

        using var image = writer.Write(content);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    public byte[] GenerateBarcodeVertical(string content, int width = 80, int height = 300)
    {
        var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = height,
                Height = width,
                Margin = 5,
                PureBarcode = false
            },
            Renderer = new ImageSharpRenderer<Rgba32>()
        };

        using var horizontalBarcode = writer.Write(content);
        horizontalBarcode.Mutate(x => x.Rotate(90));

        using var ms = new MemoryStream();
        horizontalBarcode.SaveAsPng(ms);
        return ms.ToArray();
    }

    public (byte[] horizontal, byte[] vertical) GenerateBothBarcodes(string content)
    {
        var horizontal = GenerateBarcode(content, 300, 80);
        var vertical = GenerateBarcodeVertical(content, 40, 250);
        return (horizontal, vertical);
    }
}
