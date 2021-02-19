// GsaComClientDlg.h : header file
//

#pragma once
#include "afxwin.h"


// CGsaComClientDlg dialog
class CGsaComClientDlg : public CDialog
{
// Construction
public:
	CGsaComClientDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_GSACOMCLIENT_DIALOG };
	//CString m_filename;
	//CString m_analysed_filename;

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedButtonFileopen();
	CString m_filename;
	CString m_analysed_filename;
	CString m_analysed_filename_report;
};
