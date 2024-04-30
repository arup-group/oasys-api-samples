using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Interop.Gsa_10_2;
using Oasys.Gsa.DotNetHelpers;

namespace demo_sinosoidal_roof_Forces
{
    public partial class Form1 : Form
    {
        public GsaComUtil m_gsaObj = null;
        private string strLoadcase = "A1";
        private string strElemenNo = "All";
        private int iResPos = 2;
        private List<GsaResults[]> EleArrayList = new List<GsaResults[]>();
        DataTable table = new DataTable();

        public Form1()
        {
            InitializeComponent();
            table.Columns.Add("Element", typeof(string));
            table.Columns.Add("Position", typeof(string));
            table.Columns.Add("FX(kN)", typeof(string));
            table.Columns.Add("FY(kN)", typeof(string));
            table.Columns.Add("FZ(kN)", typeof(string));
            table.Columns.Add("FRC(kN)", typeof(string));
            table.Columns.Add("MXX(kN-m)", typeof(string));
            table.Columns.Add("MYY(kN-m)", typeof(string));
            table.Columns.Add("MZZ(kN-m)", typeof(string));
            table.Columns.Add("MOM(kN-m)", typeof(string));
        }

        private void btnResult_Click(object sender, EventArgs e)
        {
            GetResult();
        }

        enum Flag
        {
            LCL_2D_BOTTOM = 0x00000001, // output 2D stresses at bottom layer
            LCL_2D_MIDDLE = 0x00000002, // output 2D stresses at middle layer
            LCL_2D_TOP = 0x00000004, // output 2D stresses at top layer
            LCL_2D_BENDING = 0x00000008, // output 2D stresses at bending layer
            LCL_2D_AVGE = 0x00000010, // average 2D element stresses at nodes
            LCL_1D_AUTO_PTS = 0x00000020, // calculate 1D results at interesting points
            LCL_INFINITY = 0x00000040, // report infinity and NaN as that, else as 0
            LCL_1D_WALL_RES_SECONDARY = 0x00000080, // output secondary stick of wall element 1D results
        };

        public void GetResult()
        {
            string filePath = txtFilePath.Text.ToString();
            int nComponent = 0;
            int Highest = 0;
            m_gsaObj = new GsaComUtil();
            m_gsaObj.GsaOpenFile(ref filePath);
            try
            {
                int iStart = 1;
                if (strElemenNo == "All")
                {
                    Highest = (int)m_gsaObj.GsaObj().GwaCommand("HIGHEST,EL");
                }
                else
                {
                    int.TryParse(strElemenNo, out Highest);
                    iStart = Highest;
                }

                List<int> exist_element = new List<int>();
                for (int i = iStart; i <= Highest; i++)
                {
                    if ((int)m_gsaObj.GsaObj().GwaCommand("EXIST,EL," + i.ToString()) != 0)
                    {
                        exist_element.Add(i);
                    }
                }
                Flag eOutFlag = (chkInter.Checked) ? Flag.LCL_1D_AUTO_PTS : 0;
                int iStat = m_gsaObj
                    .GsaObj()
                    .Output_Init_Arr(
                        (int)eOutFlag,
                        "default",
                        strLoadcase,
                        ResHeader.REF_DISP_EL1D,
                        iResPos
                    );
                if (iStat == 0)
                {
                    foreach (int element_id in exist_element)
                    {
                        GsaResults[] gsResult = null;
                        {
                            if (
                                m_gsaObj
                                    .GsaObj()
                                    .Output_Extract_Arr(element_id, out gsResult, out nComponent)
                                == 0
                            )
                            {
                                for (int j = 0; j < gsResult.Count(); j++)
                                {
                                    List<string> strList = new List<string>();
                                    strList.Add(element_id.ToString());
                                    strList.Add((j + 1).ToString());
                                    for (int k = 0; k < nComponent; k++)
                                    {
                                        strList.Add(gsResult[j].dynaResults[k].ToString());
                                    }
                                    table.Rows.Add(strList.ToArray());
                                }
                            }
                        }
                    }
                    grdResult.AutoGenerateColumns = true;
                    grdResult.DataSource = table;
                }
                m_gsaObj.GsaCloseFile();
            }
            catch
            {
                MessageBox.Show("Check for correct input parameter");
                m_gsaObj.GsaCloseFile();
            }
        }

        private void txtLoadCase_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLoadCase.Text))
            {
                strLoadcase = txtLoadCase.Text;
            }
            else
            {
                strLoadcase = "";
            }
        }

        private void txtElementNumber_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtElementNumber.Text))
            {
                if (txtResultPos.Text == "All")
                {
                    strElemenNo = txtElementNumber.Text.ToString();
                }
                else
                {
                    try
                    {
                        Convert.ToInt16(txtElementNumber.Text);
                        strElemenNo = txtElementNumber.Text.ToString();
                    }
                    catch
                    {
                        MessageBox.Show("Input value is not in correct format");
                        strElemenNo = "";
                    }
                }
            }
        }

        private void txtResultPos_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtResultPos.Text))
            {
                try
                {
                    iResPos = Convert.ToInt16(txtResultPos.Text);
                }
                catch
                {
                    MessageBox.Show("Input value is not in correct format");
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog flgOpen = new OpenFileDialog();
            DialogResult result = flgOpen.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = flgOpen.FileName;
                txtFilePath.Text = file;
            }
        }
    }
}
