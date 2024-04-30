using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Oasys.Gsa.DotNetHelpers;
using Interop.Gsa_10_2;
using System.Xml.Linq;

namespace demo_sinosoidal_roof
{
    class Program
    {
        static void Main(string[] args)
        {
            double pi_by_32 = Math.PI / 32;
            double scale = 5;
            double tol = 0.01;

            double[] offset1 = { 0, 0, 0 };
            double[] offset2 = { 0, 0, 0 };
            int n1_previous = 0;
            int n2_previous = 0;
            string strEnt = "EL";

            string filePath = Utils.DownloadExampleFile("Env.gwb", "sinosoidal_roof.gwb");
            string SavefilePath = @"C:\temp\updated_sinosoidal_roof.gwb";
            GsaComUtil m_gsaObj = new GsaComUtil();

            m_gsaObj.GsaOpenFile(ref filePath);
            m_gsaObj.GsaDeleteResults();

            #region Node & Element operatopn(Old method)

            for (int i = 0; i <= 50; i++)
            {
                double x = pi_by_32 * i * scale;
                double z = Math.Sin(x) * scale / 4;

                double y = 0;

                int n1 = m_gsaObj.NodeAt(x, y, z, tol);
                y += 10;
                int n2 = m_gsaObj.NodeAt(x, y, z, tol);

                int elem = m_gsaObj.HighestEnt(ref strEnt) + 1;

                List<int> _topo = new List<int>();
                _topo.Add(n1);
                _topo.Add(n2);

                List<string> _Release = new List<string>();
                List<double[]> _Offset = new List<double[]>();
                _Offset.Add(offset1);
                _Offset.Add(offset1);

                m_gsaObj.SetElem1d(
                    elem,
                    "",
                    1,
                    "",
                    _topo,
                    0,
                    0,
                    _Release,
                    _Offset,
                    GsaComUtil.ElemType.EL_BAR
                );

                elem = m_gsaObj.HighestEnt(ref strEnt) + 1;

                if ((n1_previous != 0))
                {
                    _topo = new List<int>();
                    _topo.Add(n1);
                    _topo.Add(n1_previous);

                    m_gsaObj.SetElem1d(
                        elem,
                        "",
                        1,
                        "",
                        _topo,
                        0,
                        0,
                        _Release,
                        _Offset,
                        GsaComUtil.ElemType.EL_BAR
                    );
                }

                elem = m_gsaObj.HighestEnt(ref strEnt) + 1;
                if ((n2_previous != 0))
                {
                    _topo = new List<int>();
                    _topo.Add(n2);
                    _topo.Add(n2_previous);
                    m_gsaObj.SetElem1d(
                        elem,
                        "",
                        1,
                        "",
                        _topo,
                        0,
                        0,
                        _Release,
                        _Offset,
                        GsaComUtil.ElemType.EL_BAR
                    );
                }
                n1_previous = n1;
                n2_previous = n2;
            }
            #endregion Node & Element operatopn(Old method)

            #region Node & Element operatopn(New method)

            strEnt = "NODE";
            int nodeHigh = m_gsaObj.HighestEnt(ref strEnt);
            strEnt = "EL";
            int elemHigh = m_gsaObj.HighestEnt(ref strEnt);
            List<GsaElement> lstElem = new List<GsaElement>();
            List<GsaNode> lstNode = new List<GsaNode>();
            double[] Stiff = { 0, 0, 0, 0, 0, 0 };
            int[] restraint = { 0, 0, 0, 0, 0, 1 };

            for (int i = 51; i <= 100; i++)
            {
                double x = pi_by_32 * i * scale;
                double z = Math.Sin(x) * scale / 4;
                double y = 0;

                //start node
                double[] Coor0 = { x, y, z };
                GsaNode gNode0 = new GsaNode();
                gNode0.Coor = Coor0;
                gNode0.SpringProperty = 1;
                gNode0.Restraint = restraint;
                gNode0.Ref = ++nodeHigh;
                lstNode.Add(gNode0);

                //end node
                y += 10;
                double[] Coor1 = { x, y, z };
                GsaNode gNode1 = new GsaNode();
                gNode1.Coor = Coor1;
                gNode1.SpringProperty = 1;
                gNode1.Restraint = restraint;
                gNode1.Ref = ++nodeHigh;
                lstNode.Add(gNode1);

                int[] iTop = { gNode0.Ref, gNode1.Ref };
                GsaElement gElem = new GsaElement();
                gElem.eType = (int)ElementType.BAR;
                gElem.Ref = ++elemHigh;
                gElem.Property = 1;
                gElem.Topo = iTop;
                gElem.NumTopo = iTop.Count();
                lstElem.Add(gElem);

                if ((n1_previous != 0))
                {
                    GsaElement gElemSide1 = new GsaElement();
                    gElemSide1.Ref = ++elemHigh;
                    gElemSide1.eType = (int)ElementType.BAR;
                    gElemSide1.Property = 1;
                    int[] iTopSide1 = { gNode0.Ref, n1_previous };
                    gElemSide1.Topo = iTopSide1;
                    gElemSide1.NumTopo = iTopSide1.Count();
                    lstElem.Add(gElemSide1);
                }

                if ((n2_previous != 0))
                {
                    GsaElement gElemSide2 = new GsaElement();
                    gElemSide2.Ref = ++elemHigh;
                    gElemSide2.eType = (int)ElementType.BAR;
                    gElemSide2.Property = 1;
                    int[] iTopSide2 = { gNode1.Ref, n2_previous };
                    gElemSide2.Topo = iTopSide2;
                    gElemSide2.NumTopo = iTopSide2.Count();
                    lstElem.Add(gElemSide2);
                }
                n1_previous = gNode0.Ref;
                n2_previous = gNode1.Ref;
            }
            m_gsaObj.GsaObj().SetNodes(lstNode.ToArray(), true);
            m_gsaObj.GsaObj().SetElements(lstElem.ToArray(), true);

            #endregion Node & Element operatopn(New method)

            #region Node Manipulation


            int[] nodeRefs = null;
            GsaNode[] nodes = null;
            GsaEntity Ent = GsaEntity.NODE;

            short s = m_gsaObj.GsaObj().EntitiesInList("all", ref Ent, out nodeRefs);

            Debug.Assert(s.Equals(0) && (nodeRefs != null));

            s = m_gsaObj.GsaObj().Nodes(nodeRefs, out nodes);

            for (Int32 i = 0; i <= nodes.GetUpperBound(0); i++)
            {
                GsaNode node = (GsaNode)nodes.GetValue(i);
                double z = node.Coor[2];
                node.Coor.SetValue(-z, 2);
                nodes.SetValue(node, i);
            }
            s = m_gsaObj.GsaObj().SetNodes(nodes, true);
            m_gsaObj.GsaObj().SaveAs(SavefilePath);

            #endregion Node Manipulation

            #region Adding new section

            GsaSection cNewSec = new GsaSection();
            cNewSec.SectDesc = "STD R 500 500";
            cNewSec.Name = "New Rectangular Section";
            cNewSec.Material = -1;
            int iNewSec = Convert.ToInt32(m_gsaObj.GsaObj().GwaCommand("HIGHEST,PROP_SEC"));
            cNewSec.Ref = iNewSec + 1;
            GsaSection[] cArrNewSec = { cNewSec };
            s = m_gsaObj.GsaObj().SetSections(cArrNewSec, true);
            Debug.Assert(s.Equals(0));
            m_gsaObj.GsaObj().SaveAs(SavefilePath);

            #endregion Adding new section

            #region Element Manipulation

            s = m_gsaObj.GsaObj().Delete("RESULTS");
            int[] ElemRefs = null;
            GsaElement[] elements = null;
            Ent = GsaEntity.ELEMENT;
            s = m_gsaObj.GsaObj().EntitiesInList("all", ref Ent, out ElemRefs);
            Debug.Assert(s.Equals(0) && (ElemRefs != null));
            s = m_gsaObj.GsaObj().Elements(ElemRefs, out elements);
            for (int i = 0; i <= elements.GetUpperBound(0); i++)
            {
                GsaElement elem = (GsaElement)elements.GetValue(i);
                elem.Property = (int)cArrNewSec[cArrNewSec.Count() - 1].Ref;
                elem.eType = (int)ElementType.BAR;
                elements.SetValue(elem, i);
            }
            s = m_gsaObj.GsaObj().SetElements(elements, true);

            #endregion Node Manipulation


            m_gsaObj.GsaSaveFile(SavefilePath);
            m_gsaObj.GsaCloseFile();
        }
    }
}
