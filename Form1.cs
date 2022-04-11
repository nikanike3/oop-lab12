using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Security.Cryptography;
using System.IO;

namespace oop_lab12
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        List<string> selectedFiles = new List<string>();

        OpenFileDialog openFileDialog = new OpenFileDialog();

        const string password = "1234512345678976";

        public Form1()
        {
            InitializeComponent();

            openFileDialog.Multiselect = true;

            richTextBox1.DragEnter += new DragEventHandler(richTextBox1_DragOver);
            richTextBox1.DragDrop += new DragEventHandler(richTextBox1_DragDrop);
            richTextBox1.AllowDrop = true;
        }

        private void addFiles_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            foreach (string file in openFileDialog.FileNames)
            {
                selectedFiles.Add(file);
            }
            selectedFiles.ForEach(file =>
            {
                richTextBox1.Text += file + "\n";
            });
            MessageBox.Show("Успішно добавлено!");
        }

        private void richTextBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                foreach (string file in fileList)
                {
                    selectedFiles.Add(file);
                    richTextBox1.Text += file + "\n";
                }
                MessageBox.Show("Успішно добавлено!");
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            archiveName.Text = "";
            selectedFiles.Clear();
        }

        private static void EncryptFile(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);

                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(skey);

                    using (FileStream fsCrypt = new FileStream(outputFile, FileMode.Create))
                    {
                        using (ICryptoTransform encryptor = aes.CreateEncryptor(key, IV))
                        {
                            using (CryptoStream cs = new CryptoStream(fsCrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                                {
                                    int data;
                                    while ((data = fsIn.ReadByte()) != -1)
                                    {
                                        cs.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void DecryptFile(string inputFile, string outputFile, string skey)
        {
            try
            {
                using (RijndaelManaged aes = new RijndaelManaged())
                {
                    byte[] key = ASCIIEncoding.UTF8.GetBytes(skey);

                    byte[] IV = ASCIIEncoding.UTF8.GetBytes(skey);

                    using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                    {
                        using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                        {
                            using (ICryptoTransform decryptor = aes.CreateDecryptor(key, IV))
                            {
                                using (CryptoStream cs = new CryptoStream(fsCrypt, decryptor, CryptoStreamMode.Read))
                                {
                                    int data;
                                    while ((data = cs.ReadByte()) != -1)
                                    {
                                        fsOut.WriteByte((byte)data);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void encryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                EncryptFile(openFileDialog.FileName, commonOpenFileDialog.FileName + "\\" + System.IO.Path.GetFileName(openFileDialog.FileName) + ".crypt", password);
                MessageBox.Show("Успішно зашифрований файл");
            }
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Crypted Files|*.crypt";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string decryptedFilePath = commonOpenFileDialog.FileName + "\\" + System.IO.Path.GetFileName(openFileDialog.FileName);
                decryptedFilePath = decryptedFilePath.Remove(decryptedFilePath.Length - 6);
                DecryptFile(openFileDialog.FileName, decryptedFilePath, password);
                MessageBox.Show("Успішно розшифрований файл");
            }
        }

        private void archiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;

            if (archiveName.Text.Length >= 1 && richTextBox1.Text.Length >= 1)
            {
                if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    using (ZipArchive zip = ZipFile.Open(commonOpenFileDialog.FileName + "\\" + archiveName.Text + ".gzar", ZipArchiveMode.Create))
                    {
                        for (int i = 0; i < selectedFiles.Count; i++)
                        {
                            zip.CreateEntryFromFile(selectedFiles[i], System.IO.Path.GetFileName(selectedFiles[i]));
                        }

                        MessageBox.Show("Успішно створений архів");
                    }
                }
            }
            else
                MessageBox.Show("Для архівації потрібно добавити файли та назвати архів!");
        }

        private void unzipToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog commonOpenFileDialog = new CommonOpenFileDialog();
            commonOpenFileDialog.IsFolderPicker = true;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Zip Files|*.zip;*.rar;*.gzar";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            if (commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                using (ZipArchive zip = ZipFile.Open(openFileDialog.FileName, ZipArchiveMode.Read))
                {
                    zip.ExtractToDirectory(commonOpenFileDialog.FileName);
                }
                MessageBox.Show("Успішно розархівовано в: " + commonOpenFileDialog.FileName);
            }
        }
    }
}
