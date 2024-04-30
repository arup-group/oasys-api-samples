// GsaComClient.cpp : Defines the class behaviors for the application.
//

#include "GsaComClient.h"
#include "GsaComClientDlg.h"
#include "stdafx.h"

#import "C:\\Program Files\\Oasys\\GSA 10.2\\Gsa.tlb" no_namespace
#include <string>
// replace the above with path to GSA.tlb in the program files folder

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CGsaComClientApp

BEGIN_MESSAGE_MAP(CGsaComClientApp, CWinApp)
ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

// CGsaComClientApp construction

CGsaComClientApp::CGsaComClientApp() {
  // TODO: add construction code here,
  // Place all significant initialization in InitInstance
}

// The one and only CGsaComClientApp object

CGsaComClientApp theApp;

// CGsaComClientApp initialization
enum Output_Init_Flags {
  OP_INIT_2D_BOTTOM = 0x01,   // output 2D stresses at bottom layer
  OP_INIT_2D_MIDDLE = 0x02,   // output 2D stresses at middle layer
  OP_INIT_2D_TOP = 0x04,      // output 2D stresses at top layer
  OP_INIT_2D_BENDING = 0x08,  // output 2D stresses at bending layer
  OP_INIT_2D_AVGE = 0x10,     // average 2D element stresses at nodes
  OP_INIT_1D_AUTO_PTS = 0x20, // calculate 1D results at interesting points
};
BOOL CGsaComClientApp::InitInstance() {
  CWinApp::InitInstance();

  SetRegistryKey(_T("GSA Com Client"));

  CGsaComClientDlg dlg;
  m_pMainWnd = &dlg;
  INT_PTR nResponse = dlg.DoModal();
  if (nResponse == IDOK) {
    try {
      invokeGsa(dlg.m_filename, dlg.m_analysed_filename,
                dlg.m_analysed_filename_report);
    } catch (_com_error *e) {
      std::string error(e->Description());
      TRACE(error.c_str());
    }

  } else if (nResponse == IDCANCEL) {
    // TODO: Place code here to handle when the dialog is
    //  dismissed with Cancel
  }

  // Since the dialog has been closed, return FALSE so that we exit the
  //  application, rather than start the application's message pump.
  return FALSE;
}
void CGsaComClientApp::invokeGsa(CString filename, CString analysed_filename,
                                 CString analysed_filename_report) {
  if (FAILED(CoInitializeEx(0, COINIT_APARTMENTTHREADED)))
    return;

  IComAutoPtr pObj(__uuidof(ComAuto));
  short ret_code = 0;

  _bstr_t bsFileName = (LPCTSTR)filename;
  ret_code = pObj->Open(bsFileName);
  if (ret_code == 1)
    return;

  _bstr_t bsContent(_T("RESULTS"));
  ret_code = pObj->Delete(bsContent);
  ASSERT(ret_code != 1); // file not open!

  _variant_t vCase(0L);
  ret_code = pObj->Analyse(vCase);
  ASSERT(ret_code == 0);
  long NumberOfIntermediatPoint = 4;
  CFile theFile;

  _variant_t vt = pObj->GwaCommand("HIGHEST,EL");
  vt.ChangeType(VT_I4);
  long Highest = vt.iVal;
  try {
    theFile.Open(analysed_filename_report,
                 CFile::modeCreate | CFile::modeWrite);
    for (int ele = 0; ele <= Highest; ele++) {
      SAFEARRAY *arryResult = nullptr;
      long component;
      GsaResults *gsRes = nullptr;
      CString strEle(std::to_string(ele + 1).c_str());
      CString gwa = _T("EXIST,EL,") + strEle;
      BSTR bstrUser = gwa.AllocSysString();
      vt = pObj->GwaCommand(bstrUser);
      vt.ChangeType(VT_BOOL);
      bool elemExist = vt.iVal;
      ::SysFreeString(bstrUser);
      if (elemExist) {
        WriteString(theFile, _T("Forces in Element: " + strEle));
        pObj->Output_Init_Arr(
            (long)Output_Init_Flags::OP_INIT_1D_AUTO_PTS, _bstr_t(L"default"),
            _bstr_t("A1"), ResHeader::REF_DISP_EL1D, NumberOfIntermediatPoint);
        pObj->Output_Extract_Arr(ele + 1, &arryResult, &component);
        HRESULT hr = SafeArrayAccessData(arryResult, (void **)&gsRes);

        long lowerBound, upperBound; // get array bounds
        SafeArrayGetLBound(arryResult, 1, &lowerBound);
        SafeArrayGetUBound(arryResult, 1, &upperBound);
        const int cnt_elements = upperBound - lowerBound + 1;

        for (int i = 0; i < cnt_elements;
             ++i) // iterate through returned values
        {
          CString Location(std::to_string(i + 1).c_str());
          WriteString(theFile, _T("At location ") + Location +
                                   _T(": ")
                                   _T("FX(kN),FY(kN),FZ(kN),FRC(kN),MXX(kN-m),")
                                   _T("MYY(kN-m),MZZ(kN-m),MOM(kN-m)"));
          SAFEARRAY *dynaRes = gsRes[i].dynaResults;
          double *gsDynaRes = nullptr;
          hr = SafeArrayAccessData(dynaRes, (void **)&gsDynaRes);
          CString append = _T("");
          for (int j = 0; j < component; j++) {
            CString val(std::to_string(gsDynaRes[j]).c_str());
            append = append + val;
          }
          WriteString(theFile, append);
          SafeArrayUnaccessData(dynaRes);
        }
        SafeArrayUnaccessData(arryResult);
      }
    }
    theFile.Close();
  } catch (...) {
    theFile.Abort();
  }

  //

  bstr_t bsAnalysedFileName = (LPCTSTR)analysed_filename;
  ret_code = pObj->SaveAs(bsAnalysedFileName);
  ASSERT(ret_code == 0);
  pObj->Close();
}
void CGsaComClientApp::WriteString(CFile &cfile, CString str) {
  CString cstr = _T("\r\n");
  cfile.Write((LPCTSTR)str, str.GetLength() * sizeof(TCHAR));
  cfile.Write((LPCTSTR)cstr, cstr.GetLength() * sizeof(TCHAR));
}