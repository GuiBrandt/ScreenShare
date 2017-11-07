using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare.Client
{
    public partial class Form1 : Form
    {
        TcpClient client;
        
        private TcpClient Client
        {
            get
            {
                return client;
            }

            set
            {
                client = value;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            Client = new TcpClient();
            try
            {
                Client.Connect(new IPEndPoint(IPAddress.Parse(txtEndereco.Text), 1432));
            }
            catch
            {
                MessageBox.Show(this, "Conexão recusada", "Screen Share", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtEndereco.Enabled = btnConn.Enabled = false;

            Task.Run(() =>
            {
                using (Stream stream = new GZipStream(Client.GetStream(), CompressionMode.Decompress))
                {
                    try
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            while (stream.ReadByte() == 1)
                            {
                                Image img;

                                byte[] bytes = reader.ReadBytes(reader.ReadInt32());

                                using (MemoryStream ms = new MemoryStream(bytes))
                                    img = Image.FromStream(ms);

                                Invoke((MethodInvoker)(() =>
                                {
                                    if (pictureBox1.Image != null)
                                        pictureBox1.Image.Dispose();
                                    pictureBox1.Image = img;
                                }));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Invoke((MethodInvoker)(() => MessageBox.Show(this, "Conexão perdida (" + ex.Message + ")", "Screen Share", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                }

                Invoke((MethodInvoker)(() =>
                {
                    pictureBox1.Image = pictureBox1.ErrorImage;
                    txtEndereco.Enabled = btnConn.Enabled = true;
                }));
            });
        }
    }
}
