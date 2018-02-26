using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ConvertCNCHPGL
{
    public partial class Form_Main : Form
    {
        private OpenFileDialog input = new OpenFileDialog();
        private SaveFileDialog output = new SaveFileDialog();
        private Module_XML xml = new Module_XML();
        public static string CNCType = "";

        public Form_Main()
        {
            InitializeComponent();
            this.cBoxCNC.Enabled = true;
            this.txtboxScaling.Enabled = false;
            this.label6.Enabled = true;
            this.label3.Enabled = false;
            this.cBoxCNC.Hide();
            this.label6.Hide();
            //cBoxCNC_Items();
        }

        private void btnInput_Click(object sender, EventArgs e)
        {
            input.Filter = xml.ReadXml("Settings.Xml", "inputExtentions") +"All files (*.*)|*.*";
            input.ShowDialog();
            txtboxInput.Text = input.FileName;
            
            if (txtboxInput.Text != "")
            {
                //Sets the output text
                txtboxOutput.Text = (Module_Main.IsHPGL(input.FileName)) ? input.FileName.Replace(Path.GetExtension(input.FileName), ".E") : input.FileName.Replace(Path.GetExtension(input.FileName), ".M");
                output.FileName = txtboxOutput.Text.Substring(txtboxOutput.Text.LastIndexOf("\\") + 1);

                //Enable or disable the items on extension type
                if (Path.GetExtension(input.FileName).ToUpper() == ".CNC")
                {
                    this.txtboxScaling.Enabled = false;
                    this.label3.Enabled = false;
                }
                else if (Path.GetExtension(input.FileName).ToUpper() == ".HPGL")
                {
                    this.txtboxScaling.Enabled = true;
                    this.label3.Enabled = true;
                }
                else if (Path.GetExtension(input.FileName).ToUpper()== ".PLT")
                {
                    //PLT can be CNC or HPGL, detect CNC or HPGL file
                    if (!Module_Main.IsHPGL(input.FileName))
                    {
                        this.txtboxScaling.Enabled = false;
                    }
                    else
                    {
                        this.txtboxScaling.Enabled = true;
                        this.label3.Enabled = true;
                    }
                }
                else
                {
                    this.txtboxScaling.Enabled = true;
                    this.label3.Enabled = true;
                }
            }
        }
        
        private void btnOutput_Click(object sender, EventArgs e)
        {
            output.Filter = xml.ReadXml("Settings.Xml", "outputExtentions") + "All files (*.*)|*.*";
            
            output.ShowDialog();
            if (output.FileName != "")
            {
                txtboxOutput.Text = output.FileName;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(txtboxInput.Text) && Directory.Exists(txtboxOutput.Text.Substring(0, txtboxOutput.Text.LastIndexOf("\\"))))
                {
                    if (Module_Main.IsHPGL(txtboxInput.Text) && !txtboxInput.Text.Contains(".cnc") && !txtboxInput.Text.Contains(".nc"))
                    {
                        Module_Main.StartConversion(txtboxInput.Text, txtboxOutput.Text, double.Parse(txtboxScaling.Text));
                    }
                    else
                    {
                        CNCType = cBoxCNC.Text;
                        Converter conv = new Converter(new FileReader(txtboxInput.Text).Lines, double.Parse(txtboxScaling.Text));
                        ErrorHandler.FileName = txtboxInput.Text.Substring(0, txtboxInput.Text.IndexOf('.')) + ".txt";
                        conv.CreateProgram();
                        Class_Output outp = new Class_Output();
                        outp.OpenFile(txtboxOutput.Text);
                        outp.OutputFileHeader(txtboxOutput.Text);

                        for (int i = 0; i < conv.GetProgram.Count; i++)
                        {
                            conv.GetProgram[i].Write(outp);
                        }

                        outp.OutputFileEnd();
                        outp.CloseFile();
                        ErrorHandler.WriteOutputFile();
                    }
                    MessageBox.Show("File succesfully converted.\nSaved file as " + txtboxOutput.Text + "\nLogfile saved as:" + txtboxOutput.Text.Substring(0, txtboxOutput.Text.Length - 1) + "txt\nDate: " + DateTime.UtcNow.ToLocalTime().ToString(), "(C) BCSI Systems BV CNC and HPGL Converter");
                }
                else
                    MessageBox.Show("Input file or output folder was not found.", "(C) BCSI Systems BV CNC and HPGL Converter");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Module_Main.types.Clear();
            //Application.Restart();
        }
        
        private void txtboxScaling_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
                && !char.IsDigit(e.KeyChar)
                && e.KeyChar != '.')
            {
                e.Handled = true;
            }
            
            // only allow one decimal point
            if (e.KeyChar == '.'
                && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        void cBoxCNC_Items()
        {
            foreach (XmlNode item in xml.ReturnXml("Settings.xml", "CNCitems"))
            {
                this.cBoxCNC.Items.Add(item.InnerText.Substring(0, item.InnerText.IndexOf('{')).Trim());
            }

            foreach (XmlNode item in xml.ReturnXml("Settings.xml", "HPGLItems"))
            {
                this.cBoxCNC.Items.Add(item.InnerText.Substring(0, item.InnerText.IndexOf('{')).Trim());
            }
        }
        
        [STAThread]
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            try
            {
                int num;

                if (int.TryParse(args[2], out num))
                    Module_Main.StartConversion(args[0], args[1], double.Parse(args[2]));
                else if (args[2] != null)
                {
                    if (File.Exists(args[0]) && Directory.Exists(args[1].Substring(0, args[1].LastIndexOf("\\"))))
                    {
                        if (Module_Main.IsHPGL(args[0]) && !args[0].Contains(".cnc") && !args[0].Contains(".nc"))
                        {
                            Module_Main.StartConversion(args[0], args[1], double.Parse(args[2]));
                        }
                        else
                        {
                            Module_Main.RelativeArc = (args[2].ToLower() == "true") ? true : false;

                            Converter conv = new Converter(new FileReader(args[0]).Lines, double.Parse(args[3]));
                            ErrorHandler.FileName = args[0].Substring(0, args[0].IndexOf('.')) + ".txt";
                            conv.CreateProgram();
                            Class_Output outp = new Class_Output();
                            outp.OpenFile(args[1]);
                            outp.OutputFileHeader(args[1]);

                            for (int i = 0; i < conv.GetProgram.Count; i++)
                            {
                                conv.GetProgram[i].Write(outp);
                            }

                            outp.OutputFileEnd();
                            outp.CloseFile();
                            ErrorHandler.WriteOutputFile();
                        }
                    }
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new Form_Main());
                }
            }
            catch
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form_Main());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Module_Main.RelativeArc = checkBox1.Checked;
        }
    }
}
