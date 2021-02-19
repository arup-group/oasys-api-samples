// GsaComClient.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CGsaComClientApp:
// See GsaComClient.cpp for the implementation of this class
//

class CGsaComClientApp : public CWinApp
{
public:
	CGsaComClientApp();
	void invokeGsa(CString filename, CString analysed_filename,CString analysed_filename_report);

// Overrides
	public:
	virtual BOOL InitInstance();
private:
	void WriteString(CFile& cfile,CString str);

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CGsaComClientApp theApp;