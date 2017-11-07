using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
	class MainClass
	{
		static TcpListener listener;
        static byte[] bytes;

		public static void Main(string[] args)
		{
			listener = new TcpListener(IPAddress.Any, 1432);
			listener.Start();

			Console.WriteLine("ScreenShare está ligado na porta 1432");
			Console.WriteLine();

			Task.Run(() => TakeScreenShots());

			while (true)
			{
				TcpClient cl = listener.AcceptTcpClient();
				Task.Run(() => ListenTo(cl));
			}
		}


		private static void ListenTo(TcpClient cl)
		{
			Console.WriteLine("" + cl.Client.RemoteEndPoint + " conectado");
            Stream stream = new GZipStream(cl.GetStream(), CompressionLevel.Optimal);

            using (BinaryWriter writer = new BinaryWriter(stream))
			    try
			    {
				    while (true)
				    {
                        stream.WriteByte(1);
                        stream.Flush();

                        if (bytes != null)
                        {
                            writer.Write(bytes.Length);
                            writer.Write(bytes);
                            writer.Flush();
                        }

					    Thread.Sleep(16);
				    }
			    }
			    catch (Exception e)
			    {
				    Console.WriteLine("" + cl.Client.RemoteEndPoint + " desconectado (" + e.Message + ")");
				    try { cl.Close(); } catch { }
			    }
		}

        public static Bitmap Resize(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static void TakeScreenShots()
		{
            Bitmap screenShot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(screenShot);

            while (true)
			{
                g.CopyFromScreen(0, 0, 0, 0, screenShot.Size);

                using (MemoryStream ms = new MemoryStream())
                {
                    screenShot.Save(ms, ImageFormat.Jpeg);
                    bytes = ms.ToArray();
                }

                Thread.Sleep(16);
			}
		}
	}
}
