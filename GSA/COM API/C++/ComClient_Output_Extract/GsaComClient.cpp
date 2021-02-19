// GsaComClient.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "GsaComClient.h"
#include "GsaComClientDlg.h"
#include <string>
#import "C:\\Program Files\\Oasys\\GSA 10.0\\Gsa.tlb" no_namespace
// replace the above with path to GSA.tlb in the program files folder

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CGsaComClientApp

BEGIN_MESSAGE_MAP(CGsaComClientApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


// CGsaComClientApp construction

CGsaComClientApp::CGsaComClientApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CGsaComClientApp object

CGsaComClientApp theApp;

enum Output_Init_Flags
{
    OP_INIT_2D_BOTTOM   =  0x01,     // output 2D stresses at bottom layer
    OP_INIT_2D_MIDDLE   =  0x02,     // output 2D stresses at middle layer
    OP_INIT_2D_TOP      =  0x04,        // output 2D stresses at top layer
    OP_INIT_2D_BENDING  =  0x08,    // output 2D stresses at bending layer
    OP_INIT_2D_AVGE     =  0x10,      // average 2D element stresses at nodes
    OP_INIT_1D_AUTO_PTS =  0x20,  // calculate 1D results at interesting points
};

// CGsaComClientApp initialization

BOOL CGsaComClientApp::InitInstance()
{
	CWinApp::InitInstance();

	SetRegistryKey(_T("GSA Com Client"));

	CGsaComClientDlg dlg;
	m_pMainWnd = &dlg;
	INT_PTR nResponse = dlg.DoModal();
	if (nResponse == IDOK)
	{
		try 
		{
			invokeGsa(dlg.m_filename, dlg.m_analysed_filename,dlg.m_analysed_filename_report);
		}
		catch(_com_error* e)
		{
			std::string error(e->Description());
			TRACE(error.c_str());
		}

	}
	else if (nResponse == IDCANCEL)
	{
		// TODO: Place code here to handle when the dialog is
		//  dismissed with Cancel
	}

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
void CGsaComClientApp::invokeGsa(CString filename, CString analysed_filename,CString analysed_filename_report)
{
	if(FAILED(CoInitializeEx(0, COINIT_APARTMENTTHREADED)))
		return;

	IComAutoPtr pObj(__uuidof(ComAuto));
	short ret_code = 0;
	
	_bstr_t bsFileName = (LPCTSTR)filename;
	ret_code = pObj->Open(bsFileName);
	if(ret_code ==1)
		return;
	
	_bstr_t bsContent(_T("RESULTS"));
	ret_code = pObj->Delete(bsContent);
	ASSERT(ret_code != 1);	// file not open!
	
	_variant_t vCase(0L);
	ret_code = pObj->Analyse(vCase);
	ASSERT(ret_code ==0);
	long NumberOfIntermediatPoint = 3;
	CFile theFile;
	_variant_t vt = pObj->GwaCommand("HIGHEST,EL");
	 vt.ChangeType(VT_I4);
	long Highest =  vt.iVal;
	try
	{
		theFile.Open(analysed_filename_report,CFile::modeCreate | CFile::modeWrite);
		for (int ele = 1; ele <= Highest; ele++)
		{
		
			CString strEle;strEle.Format(_T("%d"), ele);
			CString gwa =  _T("EXIST,EL,") + strEle;
			BSTR bstrUser = gwa.AllocSysString();
			vt = pObj->GwaCommand(bstrUser);
			vt.ChangeType(VT_BOOL);
			bool elemExist = vt.iVal ;
			::SysFreeString(bstrUser);
			if(elemExist)
			{
				//FX
				WriteString(theFile,_T("Forces in Element: " + strEle));
				HRESULT iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002001, NumberOfIntermediatPoint);
                long Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": FX(kN)=") + val );
				}
				//FY
				iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002002, NumberOfIntermediatPoint);
                Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": FY(kN)=") + val );
				}
				//FZ
				iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002003, NumberOfIntermediatPoint);
                Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": FZ(kN)=") + val );
				}

				//MXX
				iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002005, NumberOfIntermediatPoint);
                Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": MXX(kN-m)=") + val );
				}
				//MYY
				iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002006, NumberOfIntermediatPoint);
                Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": MYY(kN-m)=") + val );
				}
				//MZZ
				iStat = pObj->Output_Init(OP_INIT_1D_AUTO_PTS, "default",  _bstr_t("A1") , 14002007, NumberOfIntermediatPoint);
                Totpos = pObj->Output_NumElemPos(ele);
				for(int pos =0;pos< Totpos;pos++)
				{
                   CString  val = pObj->Output_Extract(ele,pos);
				   CString Location;Location.Format(_T("%d"),pos);
				   WriteString(theFile,_T("At location ") + Location + _T(": MZZ(kN-m)=") + val );
				}

			}
		}
	    theFile.Close();
	}
	catch(...)
	{
		theFile.Abort();
	}
	_bstr_t bsAnalysedFileName = (LPCTSTR)analysed_filename;
	ret_code = pObj->SaveAs(bsAnalysedFileName);
	ASSERT(ret_code ==0);
	pObj->Close();

}
void CGsaComClientApp::WriteString(CFile& cfile,CString str)
{
	CString cstr=_T("\r\n");
	cfile.Write((LPCTSTR)str,str.GetLength()* sizeof(TCHAR));
	cfile.Write((LPCTSTR)cstr,cstr.GetLength()* sizeof(TCHAR));
}