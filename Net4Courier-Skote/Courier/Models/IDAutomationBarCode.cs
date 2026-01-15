// Decompiled with JetBrains decompiler
// Type: Net4Courier.Models.IDAutomationBarCode
// Assembly: Courier_27_09_16, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B3B4E05-393A-455A-A5DE-86374CE9B081
// Assembly location: D:\Courier09022018\Decompiled\obj\Release\Package\PackageTmp\bin\Net4Courier.dll

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Net4Courier.Models
{
  public class IDAutomationBarCode
  {
    public static string BarcodeImageGenerator(string Code)
    {
      Bitmap bitmap = new Bitmap(Code.Length * 28, 100);
      using (Graphics graphics = Graphics.FromImage((Image) bitmap))
      {
        Font font = new Font("Another barcode font", 40f, FontStyle.Regular);
        PointF point = new PointF(10f, 30f);
        SolidBrush solidBrush1 = new SolidBrush(Color.Black);
        SolidBrush solidBrush2 = new SolidBrush(Color.White);
        graphics.FillRectangle((Brush) solidBrush2, 0, 0, bitmap.Width, bitmap.Height);
        graphics.DrawString("*" + Code + "*", font, (Brush) solidBrush1, point);
      }
      using (MemoryStream memoryStream = new MemoryStream())
      {
        bitmap.Save((Stream) memoryStream, ImageFormat.Png);
        byte[] buffer = memoryStream.GetBuffer();
        return buffer != null ? "data:image/jpg;base64," + Convert.ToBase64String(buffer) : "";
      }
    }
  }
}
