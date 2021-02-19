'
' © 2008 Oasys Ltd.
'
Imports System.IO
Imports System.Math
Imports System.Reflection
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Public Class GsaComUtil

    Const RoundPrecision As Integer = 6 'precision for rounding real numbers
    Const SectionSid_Usage As String = "usage"
    Const SectionSid_Symbol As String = "symbol"
    Private _MemList As Dictionary(Of Integer, MemberElement)
    Private _EleList As Dictionary(Of Integer, MemberElement)
    Private _EleListExist As Dictionary(Of Integer, Boolean)
    Private _MemListExist As Dictionary(Of Integer, Boolean)
    Private _LineList As Dictionary(Of Integer, GSALine)
    Private _LineListExist As Dictionary(Of Integer, Boolean)
    Private _IsHoz As Dictionary(Of Integer, Boolean)
    Private _IsVet As Dictionary(Of Integer, Boolean)
    Private _SecList As Dictionary(Of Integer, GSASection)
    Private _SecListExist As Dictionary(Of Integer, Boolean)
    Private _Connection As Dictionary(Of String, Integer)
    Enum AreaType
        VOID = 1
        TWO_WAY = 2
    End Enum
    Enum LineType
        ARC_THIRD_PT
        ARC_RADIUS
        LINEAR
    End Enum
    Enum GridPlaneType
        STOREY = 0
        GENERAL = 1
    End Enum
    Enum EntType
        SEL_ELEM = 2
        SEL_MEMBER = 3
        SEL_AREA = 7
        SEL_REGION = 8
    End Enum
    Enum GridLoadType
        AREA = 0
        LINE = 1
        POINT = 3
    End Enum


    Enum ElemType
        EL_UNDEF = 0
        EL_BEAM = 201
        EL_BAR = 202
        EL_TIE = 203
        EL_STRUT = 204
        EL_SPRING = 205
        EL_LINK = 206
        EL_CABLE = 207
        EL_SPACER = 208
        EL_GROUND = 101
        EL_MASS = 102
        EL_QUAD4 = 401
        EL_QUAD8 = 801
        EL_TRI3 = 301
        EL_TRI6 = 601
        EL_PLANESTRESS = 901
        EL_FLATPLATE = 902
    End Enum
    Public Enum MembType
        UNDEF = -1
        GENERIC_1D = 0
        GENERIC_2D = 1
        BEAM = 2
        COLUMN = 3
        CANTILEVER = 4
        PILE = 5
        SLAB = 6
        RIBSLAB = 7
        COMPOS = 8
        WALL = 9
        EXPLICIT = 10

    End Enum
    Public Enum Type2D
        UNDEF = 0
        PL_STRESS = 1
        PL_STRAIN = 2
        AXISYMMETRIC = 3
        FABRIC = 4
        PLATE = 5
        SHELL = 6
        CURVED_SHELL = 7
        TORSION = 8
        WALL = 9
        LOAD = 10
    End Enum
    Public Function MembTypeStr(ByVal mType As MembType) As String
        Return mType.ToString()
    End Function
    Public Function MembTypeFromStr(ByVal mType As String) As MembType
        Select Case mType
            Case "BEAM"
            Case "GENERIC_1D"
                Return MembType.BEAM
            Case "SLAB"
                Return MembType.SLAB
            Case "WALL"
                Return MembType.WALL
            Case "COLUMN"
                Return MembType.COLUMN
            Case "PERIM"
                Return MembType.SLAB
            Case "PILE"
                Return MembType.PILE
            Case Else
                Return MembType.UNDEF
        End Select
    End Function
    Public Enum MembBarLayout
        BEAM = 0
        COLUMN = 1
        PERIM = 2
        UNDEF = 3
    End Enum
    Public Function MembLayoutStr(ByVal mType As MembBarLayout) As String
        Return mType.ToString()
    End Function
    Public Function MembLayoutFromStr(ByVal mType As String) As MembBarLayout
        Select Case mType
            Case "BEAM"
                Return MembBarLayout.BEAM
            Case "COLUMN"
                Return MembBarLayout.COLUMN
            Case "PERIM"
                Return MembBarLayout.PERIM
            Case Else
                Return MembBarLayout.UNDEF
        End Select
    End Function

    Public Enum MembMat
        UNDEF = -1
        STEEL = 1
        CONCRETE = 2
        TIMBER = 3
        ALUMINIUM = 4
        GLASS = 5
        FRP = 6
    End Enum
    Public Function MembMatStr(ByVal mType As MembMat) As String
        Select Case mType
            Case MembMat.CONCRETE
                Return "CONCRETE"
            Case MembMat.STEEL
                Return "STEEL"
            Case MembMat.TIMBER
                Return "TIMBER"
            Case MembMat.ALUMINIUM
                Return "ALUMINIUM"
            Case MembMat.GLASS
                Return "GLASS"
            Case MembMat.FRP
                Return "FRP"
            Case Else
                Return "GENERIC"
        End Select
    End Function
    Public Function MembMatFromStr(ByVal mType As String) As MembMat
        Select Case mType
            Case "STEEL"
                Return MembMat.STEEL
            Case "CONCRETE"
                Return MembMat.CONCRETE
            Case "TIMBER"
                Return MembMat.TIMBER
            Case "ALUMINIUM"
                Return MembMat.ALUMINIUM
            Case "GLASS"
                Return MembMat.GLASS
            Case "FRP"
                Return MembMat.FRP
            Case Else
                Return MembMat.UNDEF
        End Select
    End Function

    Public Enum LoadPos
        SHR_CENTRE = 0
        TOP_FLANGE = 1
        BOT_FLANGE = 2
    End Enum
    Public Enum SectionMatch_Flags
        NONE = 0
        SEC_INCL_SS = 1
        SEC_ATTEMPT_STD = 2
        BOTH = (SectionMatch_Flags.SEC_INCL_SS Or SectionMatch_Flags.SEC_ATTEMPT_STD)
    End Enum

    Public Enum SectionUsage
        NOT_USED = 0
        FRAMING = GsRevit_Usage.FRAMING
        COLUMNS = GsRevit_Usage.COLUMNS
        SLAB = 3
        WALL = 4
        INVALID = 5
    End Enum
    Public Enum Units
        IMPERIAL = GsRevit_Units.IMPERIAL
        METRIC = GsRevit_Units.METRIC
    End Enum

    'GSA object
    Private m_GSAObject As ComAuto
    Private m_eSelType As EntType
    Private m_eUnit As GsRevit_Units ' Units of the REVIT MODEL!! Careful
    Public m_cfactor As Double = 1
    Public m_cfLength As Double = 1
    ' Public m_cfactor As Double = 1

    Public Sub New()
        Try
            m_GSAObject = New ComAuto()
        Catch ex As System.Runtime.InteropServices.COMException
            Throw New System.Exception("Cannot initialise GSA object. " & ex.Message)
        End Try

        If m_GSAObject Is Nothing Then
            Throw New System.Exception("Cannot initialise GSA object")
        End If
        _EleList = New Dictionary(Of Integer, MemberElement)
        _MemList = New Dictionary(Of Integer, MemberElement)
        _EleListExist = New Dictionary(Of Integer, Boolean)
        _MemListExist = New Dictionary(Of Integer, Boolean)
        _IsHoz = New Dictionary(Of Integer, Boolean)
        _IsVet = New Dictionary(Of Integer, Boolean)
        _LineList = New Dictionary(Of Integer, GSALine)
        _LineListExist = New Dictionary(Of Integer, Boolean)
        _SecList = New Dictionary(Of Integer, GSASection)
        _SecListExist = New Dictionary(Of Integer, Boolean)
        ' we parse using the EN_GB locale
        ' we parse using the EN_GB locale

        m_GSAObject.SetLocale(Locale.LOC_EN_GB)
    End Sub

    'release GSA object
    Public Sub ReleaseGsa()
        m_GSAObject = Nothing
    End Sub

    Public Sub GsaUpdateViews()
        Me.m_GSAObject.UpdateViews()
    End Sub

    'open a new GSA file
    Public Function GsaNewFile() As Short
        Return Me.m_GSAObject.NewFile()
    End Function

    'open an existing GSA file
    Public Function GsaOpenFile(ByRef sFileName As String) As Short
        Return Me.m_GSAObject.Open(sFileName)
    End Function

    'save the current GSA model to file
    Public Function GsaSaveFile(Optional ByVal sFileName As String = "") As Short
        If Not String.IsNullOrEmpty(sFileName) Then
            Return Me.m_GSAObject.SaveAs(sFileName)
        Else
            Return Me.m_GSAObject.Save()
        End If
    End Function
    Public Function GsaObj() As ComAuto
        Return m_GSAObject
    End Function
    'close GSA model
    Public Function GsaCloseFile() As Short
        Dim returnVal As Short = 0
        Try
            returnVal = m_GSAObject.Close()
            Me.ReleaseGsa()
            Return returnVal
        Catch ex As Exception
            'Throw New System.Exception("GSA COM object do not exist" & ex.Message)
        End Try
        Return returnVal
    End Function
    Public Function DeleteResults(ByVal Key As String, ByVal iRecLow As Integer, ByVal iRecHigh As Integer) As Boolean
        Dim gwaCommand As String
        gwaCommand = "DELETE," + Key + "," + iRecLow.ToString() + "," + iRecHigh.ToString()
        Dim check As Object = m_GSAObject.GwaCommand(gwaCommand)
        Dim bOut As Boolean = CType(check, Boolean)
        Return bOut
    End Function
    'delete GSA results
    Public Function GsaDeleteResults() As Short
        Return Me.m_GSAObject.Delete("RESULTS")
    End Function

    'delete the GSA results and cases
    Public Function GsaDeleteResultsAndCases() As Short
        Return Me.m_GSAObject.Delete("RESULTS_AND_CASES")
    End Function

    'analyse the GSA model
    Public Sub GsaAnalyse()
        Me.m_GSAObject.Analyse()
    End Sub
    Public Sub LogFeatureUsage(ByVal LogName As String)
        Me.m_GSAObject.LogFeatureUsage(LogName)
    End Sub

    Public ReadOnly Property GsaComObject() As ComAuto
        Get
            Return m_GSAObject
        End Get
    End Property
    Public Property RevitUnits() As GsaComUtil.Units
        Get
            Return CType(m_eUnit, GsaComUtil.Units)
        End Get
        Set(ByVal value As GsaComUtil.Units)
            m_eUnit = CType(value, GsRevit_Units)
        End Set
    End Property
    Public Function MappingPath() As String
        Dim cPath As String = ""
        m_GSAObject.MappingDBPath(cPath)
        Return cPath
    End Function
    Public Function RevitFamilyToSection(ByVal familyName As String, ByVal familyType As String, ByVal usage As SectionUsage) As String
        Dim gsrevit_usage As GsRevit_Usage = gsrevit_usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsrevit_usage.COLUMNS
        End If
        Return m_GSAObject.Gen_SectTransltnGsRevit(familyName, GsRevit_SectTrnsDir.REVIT_TO_GSA, gsrevit_usage, familyType)
    End Function
    Public Function RevitFamilyToSectionUsingCSV(ByVal familyName As String, ByVal familyType As String, ByVal usage As SectionUsage) As String
        '<Revit family name>, <GSA section shape>,<name of Revit attribute holding dim 1>,< name of Revit attribute holding dim 2>,…
        '“my rect family”,”R”,”h”,”b”
        '“my rect hollow family”,”RHS”,”h”,”b”,”t”,”T”
        Dim GsaSecDesc As String = ""
        Using reader As New CsvFileReader("C:\\Test.csv")
            Dim row As New CsvRow()
            While reader.ReadRow(row)
                If row(0).Equals(familyName) Then
                    Dim shape As String = row(1).ToString()
                    Select Case shape
                        Case "R"
                            GsaSecDesc = "STD " + shape + " " + row(1) + " " + row(2)
                        Case "RHS"
                            'write code
                        Case "C"
                            'write code
                        Case "CHS"
                            'write code
                    End Select
                End If
            End While
        End Using
        Return ""
    End Function
    Public Function IsDescValid(ByVal sDes As String) As Boolean
        If String.IsNullOrEmpty(sDes) Then
            Return False
        End If
        Dim strOut As String = m_GSAObject.Gen_SectionMatchDesc(sDes, 1, False)
        If String.IsNullOrEmpty(strOut) Then
            Return False
        End If
        Return True
    End Function
    Public Function SectionToRevitFamily(ByVal gsaDesc As String,
                                         ByVal usage As SectionUsage,
                                         ByRef familyName As String,
                                         ByRef bFamilyTypeFound As Boolean) As String

        Dim familyType As String = ""
        Dim gsrevit_usage As GsRevit_Usage = gsrevit_usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsrevit_usage.COLUMNS
        End If
        familyName = ""
        familyType = m_GSAObject.Gen_SectTransltnGsRevit(gsaDesc, GsRevit_SectTrnsDir.GSA_TO_REVIT, gsrevit_usage, familyName)
        If Not String.IsNullOrEmpty(familyType) And Not String.IsNullOrEmpty(familyName) Then
            bFamilyTypeFound = True
        Else
            familyType = TrySNFamilies(gsaDesc, usage, bFamilyTypeFound, familyName)
        End If
        Return familyType
    End Function
    Public Function SectionToRevitFamily(ByVal gsaDesc As String,
                                        ByVal usage As SectionUsage,
                                        ByRef familyName As String,
                                        ByRef bFamilyTypeFound As Boolean, ByRef bFamilyFromTrial As Boolean) As String
        Dim familyType As String = ""
        Dim gsrevit_usage As GsRevit_Usage = gsrevit_usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = gsrevit_usage.COLUMNS
        End If
        familyName = ""
        familyType = m_GSAObject.Gen_SectTransltnGsRevit(gsaDesc, GsRevit_SectTrnsDir.GSA_TO_REVIT, gsrevit_usage, familyName)
        If Not String.IsNullOrEmpty(familyType) And Not String.IsNullOrEmpty(familyName) Then
            bFamilyTypeFound = True
            bFamilyFromTrial = False
        Else
            familyType = TrySNFamilies(gsaDesc, usage, bFamilyTypeFound, familyName)
            bFamilyFromTrial = True
        End If
        Return familyType
    End Function
    Private Function TrySNFamilies(ByVal desc As String, ByVal usage As GsaComUtil.SectionUsage, ByRef familyTypeFound As Boolean, ByRef familyName As String) As String
        Dim parts As String() = Nothing
        parts = desc.Split(New [Char]() {"%"c, " "c}, StringSplitOptions.RemoveEmptyEntries)
        Dim typeName As String = ""
        If String.Equals(parts.GetValue(0), "CAT") Then
            'CAT W W14x43
            If Me.CATSectionToSNFamily(parts, usage, familyName) Then
                familyTypeFound = True
                Return parts(2)
            End If
        ElseIf String.Equals(parts.GetValue(0), "STD") Then
            If Me.STDSectionToSNFamily(parts, usage, familyName) Then
                familyTypeFound = False
                Return Nothing
            End If
        End If
        'Debug.Assert(False)
        familyName = ""
        familyTypeFound = False
        Return Nothing

    End Function
    ''' <summary>
    ''' calls Gen_MatchDesc. Options include bSuperSeded, bAttemptStd
    ''' </summary>
    ''' <param name="sDesc"></param>
    ''' <param name="options"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function MatchDescription(ByVal sDesc As String, ByVal options As GsaComUtil.SectionMatch_Flags) As String
        If String.IsNullOrEmpty(sDesc) Then
            Return String.Empty
        End If
        Dim result As String = ""
        Try
            result = m_GSAObject.Gen_SectionMatchDesc(sDesc, options, False)
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return result
    End Function

    'find existing GSA node at required position or create a new one
    Public Function NodeAt(ByVal dX As Double, ByVal dY As Double, ByVal dZ As Double,
                                ByVal dCoincidenceTol As Double) As Integer
        Dim iNode As Integer = 0
        'round
        dX = Math.Round(dX, RoundPrecision)
        dY = Math.Round(dY, RoundPrecision)
        dZ = Math.Round(dZ, RoundPrecision)

        'find existing node within tolerance or create new node
        iNode = Me.m_GSAObject.Gen_NodeAt(dX, dY, dZ, dCoincidenceTol)
        Return iNode
    End Function
    Public Sub SetGridNode(ByVal iNode As Integer, ByVal iGrid As Integer)
        'NODE_GRID | ref | name | grid plane | grid | grid line a | grid line b | edge length | radius | column rigidity
        'NODE_GRID	77	a	0	ORIGIN			0.000000	0.000000	NO

        'NODE.2 | num | name | colour | x | y | z |
        '   is_grid { | grid_plane | datum | grid_line_a | grid_line_b } | axis |
        '   is_rest { | rx | ry | rz | rxx | ryy | rzz } |
        '   is_stiff { | Kx | Ky | Kz | Kxx | Kyy | Kzz } |
        '   is_mesh { | edge_length | radius | column_rigidity | column_prop | column_node | column_angle | column_factor | column_slab_factor }

        'Dim sGwaCommand As String = "NODE_GRID,"
        'sGwaCommand += iNode.ToString() + ","
        'sGwaCommand += "" + ","                     'Name
        'sGwaCommand += ",NO_RGB"                    'colour
        'sGwaCommand += iGrid.ToString() + ","       'Global grid
        'sGwaCommand += "ORIGIN,,,"                  'Grid lines
        'sGwaCommand += "0.0,0.0,NO"
        'm_GSAObject.GwaCommand(sGwaCommand)

    End Sub
    Public Function BlankElement(ByVal iELem As Integer) As Boolean
        Dim gwaCommand As String
        gwaCommand = "BLANK,EL," + iELem.ToString()
        Dim check As Object = m_GSAObject.GwaCommand(gwaCommand)
        Return True
    End Function
    Public Function BlankMember(ByVal iMem As Integer) As Boolean
        Dim gwaCommand As String
        gwaCommand = "BLANK,MEMB," + iMem.ToString()
        Dim check As Object = m_GSAObject.GwaCommand(gwaCommand)
        Return True
    End Function
    Public Sub ChangeUnits(ByRef dData() As Double)
        dData(0) = Math.Round(dData(0) * m_cfLength, 4)
        dData(1) = Math.Round(dData(1) * m_cfLength, 4)
        dData(2) = Math.Round(dData(2) * m_cfLength, 4)
    End Sub


    Public Sub ChangeUnits(ByRef dData As Double)
        dData = Math.Round(dData * m_cfLength, 4)
    End Sub
    Public Sub ExtractNodeCoor(ByVal strNode As String, ByRef x As Double, ByRef y As Double, ByRef z As Double)

        Dim iNode As Integer
        'initialize the coords to 0 first
        x = 0
        y = 0
        z = 0
        If Not (Integer.TryParse(strNode, iNode)) Then
            Exit Sub
        End If
        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + strNode
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, x, y, z)
        End If
        'change SI unit to user unit
        x = x * m_cfactor
        y = y * m_cfactor
        z = z * m_cfactor

    End Sub
    Public Function ExtractInterMediateNodeCoorOnCurve(ByVal strMembRef As String, Optional ByVal mem As Boolean = True) As Double()

        Dim iMemb As Integer
        'initialize the coords to 0 first
        Dim dbNoderCord() As Double = {0.0, 0.0, 0.0}
        If Not (Integer.TryParse(strMembRef, iMemb)) Then
            Return dbNoderCord
        End If
        Dim check As String = ""
        If mem Then
            check = CStr(m_GSAObject.GwaCommand("GET,MEMB," & strMembRef))

        Else
            check = CStr(m_GSAObject.GwaCommand("GET,LINE," & strMembRef))
        End If
        If Not String.IsNullOrEmpty(check) Then
            If mem Then
                m_GSAObject.MembCoorOnCurve(iMemb, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))

            Else
                m_GSAObject.LineCoorOnCurve(iMemb, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))

            End If
        End If
        Return dbNoderCord
    End Function
    Public Function ExtractNodeCoor(ByVal strNode As String) As Double()

        Dim iNode As Integer
        'initialize the coords to 0 first
        Dim dbNoderCord() As Double = {0.0, 0.0, 0.0}
        If Not (Integer.TryParse(strNode, iNode)) Then
            Return dbNoderCord
        End If
        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + strNode
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))
        End If
        'change SI unit to gsa user unit
        dbNoderCord(0) = dbNoderCord(0) * m_cfactor
        dbNoderCord(1) = dbNoderCord(1) * m_cfactor
        dbNoderCord(2) = dbNoderCord(2) * m_cfactor
        Return dbNoderCord


    End Function
    Public Shared Function Arg(ByVal pos As Integer, ByVal source As String) As String
        Dim strArray As String() = source.Split(New [Char]() {","c})
        If strArray.Length > pos Then
            Return CType(strArray.GetValue(pos), String)
        Else
            Return String.Empty
        End If
    End Function
    Public Sub AssignUnitsCF()

        'Revit store length in feet i.e in Impherial and rest other in SI units 
        Dim commandObj As Object = m_GSAObject.GwaCommand("GET,UNIT_DATA, LENGTH")
        If commandObj Is Nothing Then
            Exit Sub
        End If
        Dim commandResult As String = commandObj.ToString()
        'UNIT_DATA | option | name | factor
        'UNIT_DATA,LENGTH,ft,3.28084


        Double.TryParse(GsaComUtil.Arg(3, commandResult), m_cfactor)
        m_cfLength = 3.28084 / m_cfactor
        ' A factor which can convert the values to feet - This is what the revit API expects.
    End Sub
    'write a GSA Grid Plane
    Public Function SetGridPlane(ByVal sid As String, ByVal iGrid As Integer, ByVal sName As String, ByVal iAxis As Integer, ByVal dElev As Double, ByVal dTol As Double) As Integer
        '// ++
        '//	GRID_PLANE.2 | num | name | type | axis | elev | tol | below | above
        '//
        '//	@desc			Grid plane definition
        '//
        '//	@param
        '//	num				record number
        '//	name			name
        '//	type			type +
        '//					GENERAL : general grid plane +
        '//					STOREY	: storey
        '//	axis			grid plane axis
        '//	elev			grid elevation [m]
        '//	tol	        	grid plane tolerance
        '//  below			storey tolerance below grid plane if STOREY
        '//	above			storey tolerance above grid plane if STOREY
        ' // --
        Dim sGwaCommand As String = ""
        If (0 = iGrid) Then
            iGrid = Me.HighestGridPlane() + 1
        End If
        'round
        dElev = Math.Round(dElev, RoundPrecision)
        'write grid plane
        sGwaCommand = "GRID_PLANE:"
        sGwaCommand += sid
        sGwaCommand += "," + iGrid.ToString()       'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += "," + "STOREY"               'type [GENERAL :: general grid plane] or [STOREY	:: storey]
        sGwaCommand += "," + iAxis.ToString()       'axis
        sGwaCommand += "," + dElev.ToString()       'elevation 
        sGwaCommand += "," + dTol.ToString()        'grid plane tolerance 
        sGwaCommand += "," + dTol.ToString()        'below tolerance 
        sGwaCommand += "," + dTol.ToString()        'above_tol 
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iGrid
    End Function
    Public Function GridPlane(ByVal iNum As Integer, ByRef sName As String, ByRef uid As String, ByRef dElev As Double) As Boolean
        Dim bResult As Boolean = False
        If Me.GridPlane(iNum, sName, dElev) Then
            bResult = True
        Else
            Return False
        End If
        uid = m_GSAObject.GetSidTagValue("GRID_PLANE", iNum, "RVT")
        Return bResult
    End Function
    Public Function GridPlane(ByVal iNum As Integer, ByRef sName As String, ByRef dElev As Double) As Boolean
        Dim type As GridPlaneType = GridPlaneType.STOREY
        Dim iAxis As Integer = 0
        Dim dTol As Double = 0
        Return Me.GridPlane(iNum, sName, iAxis, dElev, type, dTol)

    End Function
    'read a GSA Grid Plane
    Public Function GridPlane(ByVal iGrid As Integer, ByRef sName As String, ByRef iAxis As Integer, ByRef dElev As Double, ByRef Type As GridPlaneType, ByRef dTol As Double) As Boolean

        '// ++
        '//	GRID_PLANE.2 | num | name | type | axis | elev | tol | below | above
        '//
        '//	@desc			Grid plane definition
        '//
        '//	@param
        '//	num				record number
        '//	name			name
        '//	type			type +
        '//					GENERAL : general grid plane +
        '//					STOREY	: storey
        '//	axis			grid plane axis
        '//	elev			grid elevation [m]
        '//	tol	        	grid plane tolerance
        '// below			storey tolerance below grid plane if STOREY
        '//	above			storey tolerance above grid plane if STOREY
        ' // --
        If Not Me.GridPlaneExists(iGrid) Then
            Return False
        End If
        Dim sGwaCommand As String = ""
        Dim sArg As String
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,GRID_PLANE," & iGrid.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If
        sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
        sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
        sName = sArg
        sArg = GsaComUtil.Arg(3, sGwaCommand) 'type
        If (sArg.ToLower().Equals("storey")) Then
            Type = GridPlaneType.STOREY
        Else
            Type = GridPlaneType.GENERAL
        End If
        sArg = GsaComUtil.Arg(4, sGwaCommand) 'axis
        iAxis = CInt(sArg)

        sArg = GsaComUtil.Arg(5, sGwaCommand) 'elevation [feet]
        dElev = Val(sArg)
        sArg = GsaComUtil.Arg(6, sGwaCommand) 'elements
        dTol = Val(sArg)

        Return True
    End Function

    'write GSA grid line
    Public Function SetGridLine(ByVal iGrLine As Integer, ByVal sName As String, ByVal bArc As Boolean, ByVal sid As String,
                                   ByVal coorX As Double, ByVal coorY As Double, ByVal length As Double,
                                   Optional ByVal theta1 As Double = 0.0, Optional ByVal theta2 As Double = 0.0) As Boolean

        Dim sGwaCommand As String = ""
        '// ++
        '// GRID_LINE | num | name | arc | coor_x | coor_y | length | theta1 | theta2
        '//
        '//	@desc				Grid line definition
        '//
        '//	@param
        '//	num					record number
        '//	name				name
        '//	arc					line Is circular arc +
        '//						ARC				: +
        '//						LINE (Or blank)	:
        '//	coor_x				X coordinate of start of line Or centre of arc [m]
        '//	coor_y				Y coordinate of start of line Or centre of arc [m]
        '//	length				length of line Or radius of arc [m]
        '//	theta1				angle of line from X Or Or to start of arc [°]
        '//	theta2				angle to end of arc (ignored if line) [°]
        '// --
        sName = sName.Replace("""", String.Empty)
        sGwaCommand = "GRID_LINE,"
        sGwaCommand += iGrLine.ToString() + ","
        sGwaCommand += sName + ","
        If bArc Then
            sGwaCommand += "ARC"
        End If
        sGwaCommand += ","
        sGwaCommand += coorX.ToString() + ","
        sGwaCommand += coorY.ToString() + ","
        sGwaCommand += length.ToString() + ","
        sGwaCommand += theta1.ToString() + "," + theta2.ToString() + ","

        m_GSAObject.GwaCommand(sGwaCommand)
        Dim iGrLineNew As Integer = Me.HighestGridLine()

        If Int32.Equals(iGrLine, iGrLineNew) Then
            m_GSAObject.WriteSidTagValue("GRID_LINE", iGrLine, "RVT", sid)
            Return True
        Else
            Return False
        End If

    End Function
    Public Function GridLine(ByVal iGrLine As Integer, ByRef name As String, ByRef bArc As Boolean, ByRef sid As String,
                                    ByRef coorX As Double, ByRef coorY As Double, ByRef len As Double,
                                    ByRef theta1 As Double, ByRef theta2 As Double) As Boolean

        'GRID_LINE | num | name | arc | coor_x | coor_y | length | theta1 | theta2

        If Not Me.GridLineExists(iGrLine) Then
            Return False
        End If

        Dim result As String = CStr(m_GSAObject.GwaCommand("GET,GRID_LINE," & iGrLine.ToString()))
        Dim arg As String = GsaComUtil.Arg(1, result)
        Dim iLine As Integer = CInt(arg)
        If Not Int32.Equals(iLine, iGrLine) Then
            Return False
        End If

        arg = GsaComUtil.Arg(2, result)
        name = arg

        arg = GsaComUtil.Arg(3, result)
        If arg.Equals("LINE") Then
            bArc = False
        Else
            bArc = True
        End If

        arg = GsaComUtil.Arg(4, result)
        Double.TryParse(arg, coorX)

        arg = GsaComUtil.Arg(5, result)
        Double.TryParse(arg, coorY)

        arg = GsaComUtil.Arg(6, result)
        Double.TryParse(arg, len)

        arg = GsaComUtil.Arg(7, result)
        Double.TryParse(arg, theta1)

        arg = GsaComUtil.Arg(8, result)
        Double.TryParse(arg, theta2)

        sid = m_GSAObject.GetSidTagValue("GRID_LINE", iGrLine, "RVT")

        Return True
    End Function
    'write a GSA 1D element
    Public Function NumElementInMember(ByVal i As Integer) As Integer
        Try
            Return m_GSAObject.MembNumElem(i)
        Catch ex As Exception
            Dim str As String = ex.Message
        End Try

    End Function
    Public Function ElementNumber(ByVal iMemb As Integer, ByVal iNdex As Integer) As Integer
        Return m_GSAObject.MembElemNum(iMemb, iNdex)
    End Function
    Public Sub ReadAllConnection()
        Dim nNoEnd As Integer = Me.HighestEnt("MEMB_END")
        For iEnd As Integer = 1 To nNoEnd
            If Not ConnectionExist(iEnd) Then
                Continue For
            End If
            If (_Connection Is Nothing) Then
                _Connection = New Dictionary(Of String, Integer)
            End If
            _Connection.Add(GetConnection(iEnd), iEnd)
        Next

    End Sub

    Public Function GetConnection(ByVal iRec As Integer) As String
        Dim uid As String
        Dim name As String
        Dim restraint As String
        Dim restraintRelease As String
        Dim formatedrelease As String = "FFFFFF"
        If Not ConnectionExist(iRec) Then
            Return formatedrelease
        End If
        Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,MEMB_END," & iRec.ToString))
        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)

        If Not String.IsNullOrEmpty(idString) Then
            uid = idString
        End If
        name = GsaComUtil.Arg(2, sGwaCommand)
        restraint = GsaComUtil.Arg(3, sGwaCommand)
        restraintRelease = GsaComUtil.Arg(4, sGwaCommand)
        Dim characters As Char() = restraintRelease.ToCharArray()
        Dim Rel As String = String.Empty
        For Each ch As Char In characters
            If ch.Equals("1"c) Then
                Rel = Rel + "F"
            Else
                Rel = Rel + "R"
            End If
        Next
        If Not String.IsNullOrEmpty(Rel) Then
            formatedrelease = Rel
        End If

        Return formatedrelease
    End Function

    Public Function SetConnection(ByRef uid As String, ByRef formatedrelease As String) As Integer
        If _Connection Is Nothing Then
            _Connection = New Dictionary(Of String, Integer)
        End If
        Dim iRec As Integer = 0
        Dim CheckValue As String = formatedrelease
        If (_Connection.ContainsKey(formatedrelease)) Then
            If _Connection.TryGetValue(formatedrelease, iRec) Then
                Return iRec
            End If
        End If

        If (0 = iRec) Then
            iRec = Me.HighestEnt("MEMB_END") + 1
        End If
        Dim sGwaCommand As String = "MEMB_END"
        sGwaCommand += "," + iRec.ToString()               'number
        sGwaCommand += "," + formatedrelease               'name
        sGwaCommand += "," + "fixed"                       'restraint

        Dim characters As Char() = formatedrelease.ToCharArray()
        Dim Rel As String = String.Empty
        For Each ch As Char In characters
            If ch.Equals("F"c) Then
                Rel = Rel + "1"
            Else
                Rel = Rel + "0"
            End If
        Next
        sGwaCommand += "," + Rel                            'release

        m_GSAObject.GwaCommand(sGwaCommand)
        _Connection(formatedrelease) = iRec
        Return iRec

    End Function
    Public Function RevitToGSAReleaseDescription(ByVal sRelease As String) As String
        sRelease = sRelease.Replace("F", "1")
        sRelease = sRelease.Replace("R", "0")
        Return sRelease
    End Function

    Public Function CheckIfPolyLineWithDescExist(ByVal Desc As String, ByRef iPolyLine As Integer) As Boolean
        If String.IsNullOrEmpty(Desc.Trim()) Then
            Return False
        End If
        Dim uid As String = "", name As String = "", _desc As String = ""
        For iPoly As Integer = 0 To HighestPolyLine() - 1
            If ReadPolyLine(iPoly, uid, name, _desc) Then
                If _desc.Trim() = Desc.Trim() Then
                    iPolyLine = iPoly
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function ReadPolyLine(ByVal iPoly As Integer, ByRef sID As String, ByRef sName As String, ByRef sDec As String) As Boolean

        Dim sGwaCommand As String = "", sResult As Integer = 0
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,POLYLINE," & iPoly.ToString))
        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)
        If Not String.IsNullOrEmpty(idString) Then
            sID = idString
        End If
        sArg = GsaComUtil.Arg(1, sGwaCommand)
        If Integer.TryParse(sArg, sResult) Then
            iPoly = sResult
        Else
            Return False
        End If

        sName = GsaComUtil.Arg(1, sGwaCommand)

        sDec = GsaComUtil.Arg(6, sGwaCommand)

        Return True
    End Function
    Public Function SetPolyLine(ByVal iPoly As Integer, ByVal sID As String, ByVal sName As String, ByVal sDec As String) As Integer
        If (0 = iPoly) Then
            iPoly = Me.HighestPolyLine() + 1
        End If
        If String.IsNullOrEmpty(sName) Then
            sName = "PolyLine " + iPoly.ToString()
        End If
        Dim sGwaCommand As String = ""
        sGwaCommand = "POLYLINE:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iPoly.ToString()             'number
        sGwaCommand += "," + sName                        'name
        sGwaCommand += ",NO_RGB"                          'colour
        sGwaCommand += ",-1"                              'undefined grid plane
        sGwaCommand += ",2"                               'number of dimension
        sGwaCommand += "," + """" + sDec + """"         'number of description

        m_GSAObject.GwaCommand(sGwaCommand)
        Return iPoly
    End Function
    Public Function SetGridAreaLoad(ByVal iGridAreaLoad As Integer, ByVal sID As String, ByVal sName As String, ByVal iGrid As String, ByVal sPolyDesc As String, ByVal iDir As String, ByVal LoadValue1 As Double, ByVal LoadValue2 As Double, ByVal loadCase As Integer, ByVal bProjected As Boolean, ByVal bGlobal As Boolean, ByVal loadtype As GridLoadType) As Integer
        'LOAD_GRID_AREA.2 | name | grid_surface | area | poly | case | axis | proj | dir | value @end
        'LOAD_GRID_LINE.2 | name | grid_surface | line | poly | case | axis | proj | dir | value_1 | value_2 @end
        'LOAD_GRID_POINT.2 | name | grid_surface | x | y | case | axis | dir | value @end

        If (0 = iGridAreaLoad) Then
            iGridAreaLoad = Me.HighestGridAreaLoad() + 1
        End If
        If String.IsNullOrEmpty(sName) Then
            sName = "Grid Area Load " + iGridAreaLoad.ToString()
        End If

        Dim sGwaCommand As String = ""
        If loadtype = GridLoadType.AREA Then
            sGwaCommand = "LOAD_GRID_AREA:"
        ElseIf loadtype = GridLoadType.LINE Then
            sGwaCommand = "LOAD_GRID_LINE:"
        Else
            sGwaCommand = "LOAD_GRID_POINT:"
        End If

        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + sName                        'name
        sGwaCommand += "," + iGrid                        'grid surface
        If Not loadtype = GridLoadType.POINT Then
            sGwaCommand += ",POLYREF"
        End If

        sGwaCommand += "," + sPolyDesc.ToString()          'polygon ref

        sGwaCommand += "," + loadCase.ToString()          'load case

        If Not bGlobal Then
            sGwaCommand += ",LOCAL"                       'Global axis
        Else
            sGwaCommand += ",GLOBAL"                      'Global axis
        End If

        If Not loadtype = GridLoadType.POINT Then
            If Not bProjected Then
                sGwaCommand += ",NO"                      'projected Load False
            Else
                sGwaCommand += ",YES"                     'projected Load False
            End If
        End If

        sGwaCommand += "," + iDir                         'Direction
        sGwaCommand += "," + LoadValue1.ToString()        'Load Value 1

        If loadtype = GridLoadType.LINE Then
            sGwaCommand += "," + LoadValue2.ToString()    'Load Value
        End If
        sGwaCommand += ", "                               'Is Bridge
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iGridAreaLoad
    End Function
    Public Function CheckIfGridAreaLoadExistInDirection(ByVal sidIn As String, ByVal sDirIn As String, ByRef AreaLoadRec As Integer, ByVal loadtype As GridLoadType) As Boolean
        For iGridArea As Integer = 0 To HighestGridAreaLoad() - 1
            Dim sID As String = "", sName As String = "", iGrid As Integer = 0, iPolyRef As String = "", LoadCase As String = ""
            Dim sDir As String = "", LoadValue1 As Double = 0, LoadValue2 As Double = 0, bProjected As Boolean = False, bGlobal As Boolean = False
            If GetGridAreaLoad(iGridArea, sID, sName, iGrid, iPolyRef, LoadCase, sDir, LoadValue1, LoadValue2, bProjected, bGlobal, loadtype) Then
                If sidIn.Trim() = sID.Trim() AndAlso sDirIn.Trim() = sDir.Trim() Then
                    AreaLoadRec = iGridArea
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function GetGridAreaLoad(ByVal iGridAreaLoad As Integer, ByRef sID As String, ByRef sName As String, ByRef iGrid As Integer, ByRef sPolyDesc As String, ByRef LoadCase As String, ByRef sDir As String, ByRef LoadValue1 As Double, ByRef LoadValue2 As Double, ByRef bProjected As Boolean, ByRef bGlobal As Boolean, ByVal loadtype As GridLoadType) As Boolean
        'LOAD_GRID_AREA.2 | name | grid_surface | area | poly | case | axis | proj | dir | value @end
        'LOAD_GRID_LINE.2 | name | grid_surface | line | poly | case | axis | proj | dir | value_1 | value_2 @end
        'LOAD_GRID_POINT.2 | name | grid_surface | x | y | case | axis | dir | value @end

        Dim sGwaCommand As String = "", sResult As Integer = 0
        If loadtype = GridLoadType.AREA Then
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,LOAD_GRID_AREA," & iGridAreaLoad.ToString))
        ElseIf loadtype = GridLoadType.LINE Then
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,LOAD_GRID_LINE," & iGridAreaLoad.ToString))
        Else
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,LOAD_GRID_POINT," & iGridAreaLoad.ToString))
        End If

        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)
        If Not String.IsNullOrEmpty(idString) Then
            sID = idString
        End If

        Dim iNextIndex As Integer = 1

        sName = GsaComUtil.Arg(iNextIndex, sGwaCommand)
        iNextIndex = iNextIndex + 1
        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)

        If Integer.TryParse(sArg, sResult) Then
            iGrid = sResult
        Else
            Return False
        End If

        If loadtype = GridLoadType.POINT Then
            iNextIndex = iNextIndex + 1 'X-coordinate
            sPolyDesc = GsaComUtil.Arg(iNextIndex, sGwaCommand)

            iNextIndex = iNextIndex + 1 'Y-coordinate
            sPolyDesc = sPolyDesc + "," + GsaComUtil.Arg(iNextIndex, sGwaCommand)
        Else

            iNextIndex = iNextIndex + 1 'area/line
            iNextIndex = iNextIndex + 1 'polygon description
            sPolyDesc = GsaComUtil.Arg(iNextIndex, sGwaCommand)

        End If


        iNextIndex = iNextIndex + 1
        LoadCase = GsaComUtil.Arg(iNextIndex, sGwaCommand)

        iNextIndex = iNextIndex + 1
        bGlobal = True
        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
        If sArg = "LOCAL" Then
            bGlobal = False
        End If


        If Not loadtype = GridLoadType.POINT Then
            iNextIndex = iNextIndex + 1
            bProjected = True
            sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
            If sArg = "NO" Then
                bProjected = False
            End If

            iNextIndex = iNextIndex + 1
            sDir = GsaComUtil.Arg(iNextIndex, sGwaCommand)

            iNextIndex = iNextIndex + 1
            sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)

            Dim fLoadValue As Double = 0
            If Double.TryParse(sArg, fLoadValue) Then
                LoadValue1 = fLoadValue
            Else
                Return False
            End If

            If loadtype = GridLoadType.LINE Then
                fLoadValue = 0
                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
                If Double.TryParse(sArg, fLoadValue) Then
                    LoadValue2 = fLoadValue
                Else
                    Return False
                End If
            End If

        Else
            iNextIndex = iNextIndex + 1
            sDir = GsaComUtil.Arg(iNextIndex, sGwaCommand)
        End If



        Return True
    End Function
    Public Function CheckGridSurfaceAssociatedWithGrid(ByVal iGrid As Integer, ByRef iGridSurf As Integer) As Boolean
        Dim uid As String = "", name As String = "", grid As Integer = -1, elem2d As Boolean = False, bSpan2d As Boolean = False
        For iSurf As Integer = 0 To HighestGridSurface() - 1
            If ReadGridSurface(iSurf + 1, uid, name, grid, elem2d, bSpan2d) Then
                If grid = iGrid Then
                    iGridSurf = iSurf + 1
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function SetGridAreaSurface(ByVal iGridSurf As Integer, ByVal uid As String, ByVal name As String, ByVal iGrid As String, ByVal elem2d As Boolean, ByVal bSpan2d As Boolean) As Integer

        If (0 = iGridSurf) Then
            iGridSurf = Me.HighestGridSurface() + 1
        End If
        If String.IsNullOrEmpty(name) Then
            name = "Grid Surface " + iGridSurf.ToString()
        End If
        Dim sGwaCommand As String = ""
        sGwaCommand = "GRID_SURFACE:"
        sGwaCommand += "{RVT:" & uid & "}"
        sGwaCommand += "," + iGridSurf.ToString()         'name
        sGwaCommand += "," + name                        'name
        sGwaCommand += "," + iGrid                       'grid plane
        If elem2d Then
            sGwaCommand += ",2"                          'area type
        Else
            sGwaCommand += ",1"                          'area type
        End If
        sGwaCommand += ",all"                            'element list
        sGwaCommand += ",0.001"                          'tolerance
        If bSpan2d Then
            sGwaCommand += ",ONE"                         'span direction
        Else
            sGwaCommand += ",TWO_GENERAL"                 'span direction
        End If
        sGwaCommand += ",0"                               'angle
        sGwaCommand += ",LEGACY"                          'grid expension option
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iGridSurf

    End Function
    Public Function NatureToGsaType(ByVal nature As String) As String

        If nature.ToLower().Contains("dead") Then
            Return "LC_PERM_SELF"
        End If
        If nature.ToLower().Contains("soil") Then
            Return "LC_PERM_SOIL"
        End If
        If nature.ToLower().Contains("notional") Then
            Return "LC_PERM_EQUIV"
        End If
        If nature.ToLower().Contains("prestress") Then
            Return "LC_PRESTRESS"
        End If
        If nature.ToLower().Contains("live") Then
            Return "LC_VAR_IMP"
        End If
        If nature.ToLower().Contains("live") Then
            Return "LC_VAR_IMP"
        End If
        If nature.ToLower().Contains("roof") Then
            Return "LC_VAR_ROOF"
        End If
        If nature.ToLower().Contains("wind") Then
            Return "LC_VAR_WIND"
        End If
        If nature.ToLower().Contains("snow") Then
            Return "LC_VAR_SNOW"
        End If
        If nature.ToLower().Contains("rain") Then
            Return "LC_VAR_RAIN"
        End If
        If nature.ToLower().Contains("ther") OrElse nature.ToLower().Contains("temp") Then
            Return "LC_VAR_TEMP"
        End If
        If nature.ToLower().Contains("equi") Then
            Return "LC_VAR_EQUIV"
        End If
        If nature.ToLower().Contains("accid") Then
            Return "LC_ACCIDENTAL"
        End If
        If nature.ToLower().Contains("earthqua") OrElse nature.ToLower().Contains("seis") Then
            Return "LC_EQE_ACC"
        End If
        Return "LC_PERM_SELF"
    End Function

    Public Function SetLoadCase(ByVal iLoadCase As Integer, ByVal uid As String, ByVal name As String, ByVal nature As String) As Integer

        If (0 = iLoadCase) Then
            iLoadCase = Me.HighestLoadCase + 1
        End If
        If String.IsNullOrEmpty(name) Then
            name = "Load Case " + iLoadCase.ToString()
        End If
        Dim sGwaCommand As String = ""
        sGwaCommand = "LOAD_TITLE:"
        sGwaCommand += "{RVT:" & uid & "}"
        sGwaCommand += "," + iLoadCase.ToString()        'case no
        sGwaCommand += "," + name                        'name
        sGwaCommand += "," + NatureToGsaType(nature)     'nature
        sGwaCommand += ",1,A,NONE,INC_BOTH"
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iLoadCase

    End Function
    Public Function LoadCase(ByVal iLoadCase As Integer, ByRef uid As String, ByRef name As String, ByRef nature As String) As Boolean
        Try
            Dim sGwaCommand As String = "", sResult As Integer = 0
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,LOAD_TITLE," & iLoadCase.ToString))
            If String.IsNullOrEmpty(sGwaCommand.Trim()) Then
                Return False
            End If
            Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
            Dim idString As String = GsaComUtil.ExtractId(sArg)
            If Not String.IsNullOrEmpty(idString) Then
                uid = idString
            End If
            name = GsaComUtil.Arg(3, sGwaCommand)
            If String.IsNullOrEmpty(name.Trim()) Then
                Return False
            End If
            nature = GsaComUtil.Arg(4, sGwaCommand)
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function
    Public Function ReadGridSurface(ByVal iGridSurf As Integer, ByRef uid As String, ByRef name As String, ByRef iGrid As Integer, ByRef elem2d As Boolean, ByRef bSpan2d As Boolean) As Boolean
        Try
            Dim sGwaCommand As String = "", sResult As Integer = 0
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,GRID_SURFACE," & iGridSurf.ToString))
            Dim sArg As String = GsaComUtil.Arg(3, sGwaCommand)
            If Integer.TryParse(sArg, sResult) Then
                iGrid = sResult
            End If
            sArg = GsaComUtil.Arg(0, sGwaCommand)
            Dim idString As String = GsaComUtil.ExtractId(sArg)
            If Not String.IsNullOrEmpty(idString) Then
                uid = idString
            End If


            sArg = GsaComUtil.Arg(4, sGwaCommand)
            elem2d = False
            If Integer.TryParse(sArg, sResult) Then
                If sResult = 2 Then
                    elem2d = True
                End If
            End If
            sArg = GsaComUtil.Arg(7, sGwaCommand)
            bSpan2d = True
            If sArg = "ONE" Then
                bSpan2d = False
            End If
            Return True
        Catch ex As Exception
            Return False
        End Try


    End Function
    'write a GSA 1D element
    Public Function SetMember(ByVal iElem As Integer, ByVal sName As String, ByVal iProp As Integer,
                                ByVal sID As String, ByRef sTopo As String,
                                ByVal dBeta As Double, ByVal sRelease As List(Of String),
                                ByRef dOffset As List(Of Double()), ByRef typeMemb As MembType,
                                ByVal iGroup As Integer, ByVal iPool As Integer) As Integer

        If (0 = iElem) Then
            iElem = Me.HighestEnt("MEMB") + 1
        End If
        '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | node | angle | fire | time[3] | dummy | @end
        '		con_1 | con_2 | AUTOMATIC | pool | height | load_ref | @end
        '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | angle | fire | time[3] | dummy | @end
        '		con_1 | con_2 | EXPLICIT | nump | { point | rest | } | nums | { span | rest | } @end 
        '		height | load_ref | pool | off_auto_x1 | off_auto_x2 | off_x1 | off_x2 | off_y | off_z @end
        '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | angle | fire | time[3] | dummy | @end
        '		con_1 | con_2 | AUTOMATIC | pool | height | load_ref | @end
        '		height | load_ref | pool | off_auto_x1 | off_auto_x2 | off_x1 | off_x2 | off_y | off_z @end
        '

        dBeta = Math.Round(dBeta, RoundPrecision)
        'Write beam element
        Dim sGwaCommand As String = ""
        sGwaCommand = "MEMB:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()                 'number
        sGwaCommand += "," + sName                            'name
        sGwaCommand += ",NO_RGB"                              'colour
        sGwaCommand += "," + MembTypeStr(typeMemb)            'member type
        sGwaCommand += "," + iProp.ToString()                 'section
        Dim Grp As Integer = If(iGroup > 0, iGroup, 1)
        sGwaCommand += "," + Grp.ToString()                   'group
        sGwaCommand += "," + sTopo                            'number of topo
        sGwaCommand += ",0"                                   'orientation node
        sGwaCommand += "," + dBeta.ToString()                 'orientation angle
        sGwaCommand += ",0,0,0,0,0"                           'fire and time
        sGwaCommand += ",ACTIVE"                              'member status

        Dim iRec As Integer = SetConnection("", sRelease(0))
        sGwaCommand += "," + If(iRec > 0, iRec.ToString(), "0")  'fixity at end1

        iRec = SetConnection("", sRelease(1))
        sGwaCommand += "," + If(iRec > 0, iRec.ToString(), "0")  'fixity at end1

        sGwaCommand += ",AUTOMATIC"                         'member status/Override length/

        sGwaCommand += ",0"                                'load height
        sGwaCommand += "," + CType(LoadPos.SHR_CENTRE, Integer).ToString() 'reference point

        sGwaCommand += ",AUTO"                              'auto offset
        sGwaCommand += ",AUTO"                              'auto offset

        sGwaCommand += "," + dOffset.Item(0)(0).ToString() 'axial offset at start
        sGwaCommand += "," + dOffset.Item(1)(0).ToString() 'axial offset at end
        sGwaCommand += "," + dOffset.Item(0)(1).ToString() 'transaverse offset
        sGwaCommand += "," + dOffset.Item(1)(2).ToString() 'transverse offset

        m_GSAObject.GwaCommand(sGwaCommand)
        Return iElem
    End Function

    Public Function ElementCord(ByVal iElem As Integer) As List(Of Integer)
        'Dim sName As String, iProp As Integer, uid As String, sTopo As String, dBeta As Double
        'Dim sRelease As List(Of String), dOffset As List(Of Double()), typeMemb As MembType
        Dim sName As String = "", iProp As Integer = 0, uid As String = "", sTopo As String = "", dBeta As Double = 0
        Dim sRelease As New List(Of String), dOffset As New List(Of Double()), typeMemb As MembType = MembType.BEAM
        Dim eType As ElemType = ElemType.EL_BEAM
        Dim iTopo As New List(Of Integer), iOrNode As Integer = 0
        Elem1d(iElem, iProp, uid, iTopo, iOrNode, dBeta, sRelease, dOffset, eType, typeMemb, "")
        Return iTopo
    End Function
    Public Function MembCord(ByVal iElem As Integer) As String
        'Dim sName As String, iProp As Integer, uid As String, sTopo As String, dBeta As Double
        'Dim sRelease As List(Of String), dOffset As List(Of Double()), typeMemb As MembType
        Dim sName As String = "", iProp As Integer = 0, uid As String = "", sTopo As String = "", dBeta As Double = 0
        Dim sRelease As New List(Of String), dOffset As New List(Of Double()), typeMemb As MembType = MembType.BEAM
        Dim eType As ElemType = ElemType.EL_BEAM
        Member(iElem, sName, iProp, uid, sTopo, dBeta, sRelease, dOffset, typeMemb)
        Return sTopo
    End Function
    Public Function Member(ByVal iElem As Integer, ByRef sName As String, ByRef iProp As Integer,
                                ByRef uid As String, ByRef sTopo As String, ByRef dBeta As Double,
                                ByRef sRelease As List(Of String), ByRef dOffset As List(Of Double()),
                                ByRef typeMemb As MembType) As Boolean

        If _MemList.ContainsKey(iElem) Then

            Dim nEle As MemberElement = _MemList(iElem)
            iElem = nEle.Element
            sName = nEle.Name
            iProp = nEle.SecPro
            uid = nEle.UID
            sTopo = nEle.sTopo
            sRelease = nEle.Release
            dBeta = nEle.Beta
            dOffset = nEle.Offset
            typeMemb = nEle.MemberType
        Else
            Try
                If Not Me.EntExists("MEMB", iElem) Then
                    Return False
                End If
                uid = ""
                dBeta = 0.0
                dOffset = New List(Of Double())
                sRelease = New List(Of String)

                Dim offset0() As Double = {0, 0, 0}
                Dim offset1() As Double = {0, 0, 0}
                dOffset.Add(offset0)
                dOffset.Add(offset1)

                Dim sGwaCommand As String = ""
                Dim sArg As String
                sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MEMB," & iElem.ToString))
                If String.IsNullOrEmpty(sGwaCommand) Then
                    Return False
                End If
                '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | node | angle | fire | time[4] | dummy | @end
                '		con_1 | con_2 | AUTOMATIC | pool | height | load_ref | @end
                '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | angle | fire | time[4] | dummy | @end
                '		con_1 | con_2 | EXPLICIT | nump | { point | rest | } | nums | { span | rest | } @end 
                '		height | load_ref | pool | off_auto_x1 | off_auto_x2 | off_x1 | off_x2 | off_y | off_z @end
                '	MEMB.7 | num | name | colour | type | mat | prop | group | topology | angle | fire | time[3] | dummy | @end
                '		con_1 | con_2 | AUTOMATIC | pool | height | load_ref | @end
                '		height | load_ref | pool | off_auto_x1 | off_auto_x2 | off_x1 | off_x2 | off_y | off_z @end
                '
                sArg = GsaComUtil.Arg(0, sGwaCommand)
                Dim idString As String = GsaComUtil.ExtractId(sArg)
                If Not String.IsNullOrEmpty(idString) Then
                    uid = idString
                End If

                sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
                Debug.Assert(Integer.Equals(iElem, CInt(sArg)))
                Dim iNextIndex As Integer = 2
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'name
                sName = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'color

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'member type
                typeMemb = MembTypeFromStr(sArg)


                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'member section property
                iProp = CInt(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'group

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'string of topo
                sTopo = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'orientation node
                Dim iOrNode As Integer = CInt(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'beta
                dBeta = CDbl(sArg)

                iNextIndex = iNextIndex + 1 'fire
                iNextIndex = iNextIndex + 1 'time 1
                iNextIndex = iNextIndex + 1 'time 2
                iNextIndex = iNextIndex + 1 'time 3
                iNextIndex = iNextIndex + 1 'time 3
                iNextIndex = iNextIndex + 1 'dummy
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'dummy
                Dim bDummy As Boolean = False
                If sArg.Equals("DUMMY") Then
                    bDummy = True
                Else
                    bDummy = False
                End If

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'connection 1
                Dim Conn1 As String = GetConnection(CType(sArg, Integer))

                sRelease.Add(Conn1)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'connection 2
                Dim Conn2 As String = GetConnection(CType(sArg, Integer))
                sRelease.Add(Conn2)


                iNextIndex = iNextIndex + 1
                'steel member
                Dim restraintOption As String = GsaComUtil.Arg(iNextIndex, sGwaCommand) ' member status
                If restraintOption.Equals("EXPLICIT") Then
                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
                    Dim nPoint As Integer = 0
                    Integer.TryParse(sArg, nPoint)
                    For value As Integer = 0 To nPoint - 1
                        iNextIndex = iNextIndex + 1
                        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'point

                        iNextIndex = iNextIndex + 1
                        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'restraint description
                    Next
                    'for span
                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
                    nPoint = 0 : Integer.TryParse(sArg, nPoint)
                    For value As Integer = 0 To nPoint - 1
                        iNextIndex = iNextIndex + 1
                        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'span

                        iNextIndex = iNextIndex + 1
                        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'span's restraint description
                    Next

                ElseIf restraintOption.Equals("EFF_LEN") Then
                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'Lyy

                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'Lzz

                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'Llt

                End If

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'load height
                Dim Loadheight As Double = CDbl(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'Ref point
                Dim RefPoint As String = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'offset type 1 
                Dim OffsetType1 As String = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'offset type 2
                Dim OffsetType2 As String = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'x1 
                Dim X1 As Double = CDbl(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'x2
                Dim X2 As Double = CDbl(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'y
                Dim y As Double = CDbl(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'z
                Dim z As Double = CDbl(sArg)

                Dim nEle As New MemberElement
                nEle.Element = iElem
                nEle.Name = sName
                nEle.SecPro = iProp
                nEle.UID = uid
                nEle.sTopo = sTopo
                nEle.Beta = dBeta
                nEle.Offset = dOffset
                nEle.MemberType = typeMemb
                nEle.Release = sRelease
                nEle.OrientationNode = iOrNode
                _MemList.Add(iElem, nEle)

            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        End If
        Return True
    End Function


    Public Function SetElem1d(ByVal iElem As Integer, ByVal iProp As Integer, ByVal sID As String,
                               ByVal iTopoList As List(Of Integer), ByVal iOrNode As Integer, ByVal dBeta As Double,
                               ByVal sRelease As List(Of String), ByVal dOffset As List(Of Double()), ByVal type As ElemType) As Integer

        '// ++
        '//	EL.2 | num | name | colour | type | prop | group | topo() | orient_node | orient_angle | @br
        '//		is_rls { | rls { | k } }
        '//		is_offset { | ox | oy | oz } | dummy @end
        '//
        '//	EL.1 - for use with COM GwaCommand GET command to return element in EL_BAR, etc. format
        '//
        '//	@desc			Element release definition
        '//
        '//	@param
        '//	num				element number
        '//	name			name
        '//	colour			colour (ref. <a href="#colour_syntax">colour syntax</a>)
        '//	type			element type +
        '//					BAR		: +
        '//					BEAM	: +
        '//					TIE		: +
        '//					STRUT	: +
        '//					SPRING	: +
        '//					LINK	: +
        '//					CABLE	: +
        '//					SPACER	: +
        '//					MASS	: +
        '//					GROUND	: +
        '//					TRI3	: +
        '//					TRI6	: +
        '//					QUAD4	: +
        '//					QUAD8	: +
        '//					BRICK8	: 
        '//	prop			property number
        '//	group			group number
        '//	topo()			topology - number of items depends on type
        '//	orient_node		orientation node
        '//	orient_angle	orientation angle
        '//	is_rls			releases are included, Or Not +
        '//					NO_RLS | RLS | STIFF  :
        '//	{
        '//	rls				release code for elements (XYZ Or xyzXYZ) +
        '//					3 characters - rotations only +
        '//					6 characters - translation + rotation +
        '//					F : Fix(no release) +
        '//					R : pin(release) +
        '//					K : stiff(release + stiffness)
        '//	{
        '//	k				stiffness - one value for each K above
        '//	}
        '//	}
        '//	is_offset		offsets are included, Or Not +
        '//					OFFSET | LOCAL_OFFSET | NO_OFFSET :
        '//	{
        '//	ox				offset x
        '//	oy				offset y
        '//	oz				offset z
        '//	}
        '//	dummy			dummy flag
        '// --
        Dim sGwaCommand As String = ""
        Dim name As String = ""
        Dim clr As Color = Color.Black()

        If (0 = iElem) Then
            iElem = Me.HighestEnt("EL") + 1
        End If
        'round
        dBeta = Math.Round(dBeta, RoundPrecision)
        sGwaCommand = "EL:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()        'number
        sGwaCommand += "," + name                    'name
        sGwaCommand += "," + clr.ToArgb().ToString() 'color
        sGwaCommand += "," + "BAR"                   'element type
        sGwaCommand += "," + iProp.ToString()       'property
        sGwaCommand += ",1"                         'group
        For Each topo As String In iTopoList
            sGwaCommand += "," + topo               'topo 
        Next
        sGwaCommand += "," + iOrNode.ToString()     'orientation node
        sGwaCommand += "," + dBeta.ToString()       'orientation angle
        sGwaCommand += "," + "RLS"                  'is_rls
        For Each rels As String In sRelease
            sGwaCommand += "," + rels      'topo 
        Next
        If dOffset.Count > 1 Then
            sGwaCommand += "," + dOffset(0)(0).ToString()      'X1 
            sGwaCommand += "," + dOffset(0)(0).ToString()      'X1                           'X2 ?????????????
            sGwaCommand += "," + dOffset(0)(1).ToString()      'Y 
            sGwaCommand += "," + dOffset(0)(2).ToString()      'Z
        Else
            sGwaCommand += ",0" 'X1 
            sGwaCommand += ",0" 'X2 
            sGwaCommand += ",0" 'Y 
            sGwaCommand += ",0" 'Z
        End If

        m_GSAObject.GwaCommand(sGwaCommand)

        Return iElem
    End Function


    'read a GSA 1D element
    Public Function Elem1d(ByVal iElem As Integer, ByRef iProp As Integer, ByRef uid As String,
            ByRef iTopoList As List(Of Integer), ByRef iOrNode As Integer, ByRef dBeta As Double,
            ByRef sRelease As List(Of String), ByRef dOffset As List(Of Double()), ByRef elemType As ElemType,
            ByRef eMembType As MembType, ByRef strDummy As String) As Boolean

        'EL | num | name | colour | type | prop | group | topo() | node | angle |
        'is_rls { | rls() } | is_offset { | ox | oy | oz } | dummy 
        'EL,1,,NO_RGB,BEAM,1,1,1,2,0,0.000000,RLS,FPF,FFF
        If _EleList.ContainsKey(iElem) Then
            Dim nEle As MemberElement = _EleList(iElem)
            iElem = nEle.Element
            iProp = nEle.SecPro
            uid = nEle.UID
            iTopoList = nEle.Topo
            '  iOrNode = nEle.OrientationNode
            dBeta = nEle.Beta
            sRelease = nEle.Release
            dOffset = nEle.Offset
            iOrNode = nEle.OrientationNode
            strDummy = nEle.Dummy
            eMembType = nEle.MemberType
        Else
            iTopoList = New List(Of Integer)
            dOffset = New List(Of Double())
            sRelease = New List(Of String)

            If Not Me.EntExists("EL", iElem) Then
                Return False
            End If
            Dim eType As ElemType
            Dim sGwaCommand As String = ""
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,EL," & iElem.ToString))

            If String.IsNullOrEmpty(sGwaCommand) Then
                Return False
            End If

            Dim sArg As String
            uid = ""
            sArg = GsaComUtil.Arg(0, sGwaCommand)
            Dim idString As String = GsaComUtil.ExtractId(sArg)
            If Not String.IsNullOrEmpty(idString) Then
                uid = idString
            End If

            sArg = GsaComUtil.Arg(4, sGwaCommand)
            eType = Me.ElemTypeFromString(sArg)

            If Not GsaComUtil.ElemTypeIsBeamOrTruss(eType) Then
                Return False
            End If

            sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
            Debug.Assert(Integer.Equals(iElem, CInt(sArg)))
            sArg = GsaComUtil.Arg(5, sGwaCommand) 'property
            iProp = CInt(sArg)

            eMembType = MemberType(iProp)

            sArg = GsaComUtil.Arg(7, sGwaCommand) 'topo 0
            iTopoList.Add(CInt(sArg))

            sArg = GsaComUtil.Arg(8, sGwaCommand) 'topo 1
            iTopoList.Add(CInt(sArg))

            sArg = GsaComUtil.Arg(9, sGwaCommand) 'orientation node
            Integer.TryParse(sArg, iOrNode)
            sArg = GsaComUtil.Arg(10, sGwaCommand) 'orientation angle
            Double.TryParse(sArg, dBeta)

            Dim position As Integer = 11
            'releases
            sArg = GsaComUtil.Arg(position, sGwaCommand)
            If String.Equals("RLS", sArg) Then

                position += 1
                sArg = GsaComUtil.Arg(position, sGwaCommand) 'release 0
                position += 1
                sArg = GsaComUtil.Arg(position, sGwaCommand) 'release 1
            Else
                sRelease.Add("FFFFFF")
                sRelease.Add("FFFFFF")
            End If

            'offsets
            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)
            Dim dOff() As Double = New Double() {0, 0, 0, 0}
            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, X1
            Dim X1 As Double = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, X2
            Dim X2 As Double = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, Y
            Dim Y As Double = Val(sArg)

            position += 1
            sArg = GsaComUtil.Arg(position, sGwaCommand)    'offset 0, Z
            Dim Z As Double = Val(sArg)

            Dim dOff_i() As Double = New Double() {0, 0, 0}
            dOff_i(0) = X1
            dOff_i(1) = Y
            dOff_i(2) = Z
            dOffset.Add(dOff_i)

            Dim dOff_j() As Double = New Double() {0, 0, 0}
            dOff_j(0) = X2
            dOff_j(1) = Y
            dOff_j(2) = Z
            dOffset.Add(dOff_j)

            position += 1
            strDummy = GsaComUtil.Arg(position, sGwaCommand)

            Dim nEle As MemberElement = New MemberElement()
            nEle.Element = iElem
            nEle.SecPro = iProp
            nEle.UID = uid
            nEle.Topo = iTopoList
            ' nEle.OrientationNode = iOrNode
            nEle.Beta = dBeta
            nEle.Release = sRelease
            nEle.Offset = dOffset
            nEle.Dummy = strDummy
            nEle.OrientationNode = iOrNode
            nEle.MemberType = eMembType
            _EleList.Add(iElem, nEle)
        End If
        Return True
    End Function

    Function NumTopoEL(ByVal ELType As ElemType) As Integer

        Dim etype As ElemType = ElemType.EL_UNDEF

        Select Case ELType
            Case ElemType.EL_BAR, ElemType.EL_BEAM, ElemType.EL_TIE, ElemType.EL_STRUT, ElemType.EL_SPRING
                Return 2
            Case ElemType.EL_TRI3
                Return 3
            Case ElemType.EL_TRI6
                Return 6
            Case ElemType.EL_QUAD4
                Return 4
            Case ElemType.EL_QUAD4
                Return 8
            Case Else
                Return 2
        End Select

        Return 2
    End Function

    ' int NumTopo() Const
    '{	
    '             switch( m_iGeom )
    '	{
    '	Case MembGeom : LINE:		    Return 2;
    '	Case MembGeom : ARC_THIRD
    '	Case MembGeom : ARC_RADIUS:      Return 3;
    '	Case MembGeom : EXPLICIT:		Return NumExplicitNode() ? NumExplicitNode() : 2;
    '	Default:     ASSERT(false);      throw std:runtime_error("");
    '	}
    '}



    Public Function GetMembType(ByVal iElem As Integer) As MembType
        Dim Type As MembType = MembType.UNDEF
        If Not Me.EntExists("MEMB", iElem) Then
            Exit Function
        End If
        Dim sGwaCommand As String = ""
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MEMB," & iElem.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Exit Function
        End If
        Dim sArg As String = GsaComUtil.Arg(5, sGwaCommand) 'membmat

        Type = MembTypeFromStr(sArg)

        Return Type

    End Function
    'read a GSA 1D element
    Public Function IsExplicit(ByVal membtype As MembType, ByVal cTopo As String) As Boolean
        Dim orderdNodes As List(Of List(Of Integer)) = Nothing
        Dim voids As List(Of List(Of Integer)) = Nothing
        Dim arcNodes As List(Of Integer) = Nothing
        StringTolist(cTopo, orderdNodes, arcNodes, voids)
        If membtype.Equals(MembType.BEAM) OrElse membtype.Equals(MembType.COLUMN) Then
            If arcNodes.Count > 0 AndAlso orderdNodes.Count > 0 Then
                If orderdNodes(0).Count = 3 Then
                    Return False
                End If
            End If
            If orderdNodes.Count > 0 Then
                Dim iCount As Integer = 0
                For Each item As List(Of Integer) In orderdNodes
                    iCount = iCount + item.Count
                Next
                If iCount > 2 Then
                    Return True
                End If
            End If
        End If
            Return False
    End Function
    Public Function IsCurve(ByVal membtype As MembType, ByVal cTopo As String) As Boolean
        Dim orderdNodes As List(Of List(Of Integer)) = Nothing
        Dim voids As List(Of List(Of Integer)) = Nothing
        Dim arcNodes As List(Of Integer) = Nothing
        StringTolist(cTopo, orderdNodes, arcNodes, voids)
        If arcNodes.Count > 0 Then
            Return True
        End If
        Return False
    End Function

    'write a GSA Line
    Public Function SetLine(ByVal iNode0 As Integer, ByVal iNode1 As Integer, Optional ByVal iNode2 As Integer = 0, Optional ByVal dRad As Double = 0.0) As Integer


        '	LINE.3 | ref | name | colour | type | topology_1 | topology_2 | topology_3 | radius |
        '	axis | x | y | z | xx | yy | zz | Kx | Ky | Kz | Kxx | Kyy | Kzz |
        '	CM2_MESHTOOLS{step_definition | elem_len_start | elem_len_end | num_elem} |
        '	tied_int @end

        If iNode2 = 0 Then
            Dim iLineExist As Integer = IsLineExist(iNode0, iNode1)
            If (iLineExist > -1) Then
                Return iLineExist
            End If
        End If

        Dim iLine As Integer = HighestLine()
        iLine += 1
        Dim sGwaCommand As String = ""
        sGwaCommand += "LINE"
        sGwaCommand += "," + iLine.ToString()       'ref
        sGwaCommand += ","                          'name
        sGwaCommand += ",NO_RGB"                    'colour    


        If iNode2 = 0 Then
            sGwaCommand += ",LINE"                  'type
        Else
            sGwaCommand += ",ARC_THIRD_PT"
        End If
        sGwaCommand += "," + iNode0.ToString()      'topo1
        sGwaCommand += "," + iNode1.ToString()      'topo2
        sGwaCommand += "," + iNode2.ToString()      'topo3
        sGwaCommand += "," + dRad.ToString()        'radius
        sGwaCommand += ",GLOBAL"                    'axis

        ' Hard code these for now - 14/12/06
        sGwaCommand += ", 0, 0, 0, 0, 0, 0"         'Axis defn & constriants
        sGwaCommand += ", 0.0, 0.0, 0.0, 0.0, 0.0, 0.0," 'Kx | Ky | Kz | Kxx | Kyy | Kzz 
        sGwaCommand += "CM2_MESHTOOLS,USE_REGION_STEP_SIZE, 1, 1, 6, NO"   'step defn, StepSize, Num_Seg, Ratio, tied_int
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iLine
    End Function
    'Get a GSA line
    Public Function LineOfArcType() As List(Of Integer)
        Dim iLineList As New List(Of Integer)
        For iLine As Integer = 1 To HighestLine()
            Dim iStart As Integer = 0
            Dim iEnd As Integer = 0
            Dim iMid As Integer = 0
            Dim Ltype As GsaComUtil.LineType = GsaComUtil.LineType.LINEAR
            Dim sidFromGsa As String = ""
            Dim bExist As Boolean = GetLine(iLine, sidFromGsa, iStart, iEnd, iMid, Ltype)
            If (Ltype.Equals(LineType.ARC_THIRD_PT)) OrElse (Ltype.Equals(LineType.ARC_RADIUS)) Then
                iLineList.Add(iLine)
            End If
        Next
        Return iLineList
    End Function
    Public Function IsLineExist(ByRef iNode0 As Integer, ByRef iNode1 As Integer) As Integer
        For iLine As Integer = 1 To HighestLine()
            Dim iStart As Integer = 0
            Dim iEnd As Integer = 0
            Dim iMid As Integer = 0
            Dim Ltype As GsaComUtil.LineType = GsaComUtil.LineType.LINEAR
            Dim sidFromGsa As String = ""
            Dim bExist As Boolean = GetLine(iLine, sidFromGsa, iStart, iEnd, iMid, Ltype)
            If iStart.Equals(iNode0) AndAlso iEnd.Equals(iNode1) Then
                Return iLine
            End If
            If iStart.Equals(iNode1) AndAlso iEnd.Equals(iNode0) Then
                Return iLine
            End If
        Next
        Return -1
    End Function
    Public Function GetLine(ByRef iLine As Integer, ByRef sidFromGsa As String, ByRef iNode0 As Integer, ByRef iNode1 As Integer, ByRef iNode2 As Integer, ByRef type As LineType) As Boolean
        If _LineList.ContainsKey(iLine) Then
            Dim nLine As GSALine = _LineList(iLine)
            iLine = nLine.LineNumber()
            iNode0 = nLine.Node0
            iNode1 = nLine.Node1
            iNode2 = nLine.Node2
            type = nLine.LineType
            Return True
        Else
            If Not LineExists(iLine) Then
                Return False
            End If

            '	LINE.3 | ref | name | colour | type | topology_1 | topology_2 | topology_3 | radius |
            '	axis | x | y | z | xx | yy | zz | Kx | Ky | Kz | Kxx | Kyy | Kzz |
            '	CM2_MESHTOOLS{step_definition | elem_len_start | elem_len_end | num_elem} |
            '	tied_int @ends
            Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,LINE," & iLine.ToString))
            Try
                Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
                sidFromGsa = GsaComUtil.ExtractId(sArg) ' sid
                sArg = GsaComUtil.Arg(4, sGwaCommand)
                If (sArg.Equals("ARC_THIRD_PT")) Then
                    sArg = GsaComUtil.Arg(5, sGwaCommand)
                    iNode0 = CType(sArg, Integer)
                    sArg = GsaComUtil.Arg(6, sGwaCommand)
                    iNode1 = CType(sArg, Integer)
                    sArg = GsaComUtil.Arg(7, sGwaCommand)
                    iNode2 = CType(sArg, Integer)
                    type = LineType.ARC_THIRD_PT
                ElseIf (sArg.Equals("ARC_RADIUS")) Then
                    sArg = GsaComUtil.Arg(5, sGwaCommand)
                    iNode0 = CType(sArg, Integer)
                    sArg = GsaComUtil.Arg(6, sGwaCommand)
                    iNode1 = CType(sArg, Integer)
                    sArg = GsaComUtil.Arg(7, sGwaCommand)
                    iNode2 = CType(sArg, Integer)
                    type = LineType.ARC_RADIUS
                Else
                    sArg = GsaComUtil.Arg(5, sGwaCommand)
                    iNode0 = CType(sArg, Integer)
                    sArg = GsaComUtil.Arg(6, sGwaCommand)
                    iNode1 = CType(sArg, Integer)
                    type = LineType.LINEAR
                End If
                Dim nEle As New GSALine
                nEle.LineNumber = iLine
                nEle.Node0 = iNode0
                nEle.Node1 = iNode1
                nEle.Node2 = iNode2
                nEle.LineType = type
                _LineList.Add(iLine, nEle)
                Return True
            Catch ex As Exception
                Return False
            End Try
            Return True

        End If
    End Function
    Public Function Get2dProp(ByVal i2dprop As Integer, ByRef sidFromGsa As String, ByRef usage As SectionUsage, ByRef name As String, ByRef dThick As Double, ByRef ematType As MembMat, ByRef iMat As Integer) As Boolean

        '	PROP_2D.3 | num | name | colour | type | axis | mat | mat_type | grade | design | thick | ref_pt | ref_z | @end
        '	mass | flex | inplane | weight | @end
        '	is_env { | energy | CO2A | CO2B | CO2C | CO2D | recycle | user }
        Try

            Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,PROP_2D," & i2dprop.ToString))
            Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)

            Dim sidList As New SortedList(Of String, String)
            Dim sid_string As String = GsaComUtil.ExtractId(sArg)
            Me.ParseNestedSid(sid_string, sidList)
            If sidList.ContainsKey(GsaComUtil.SectionSid_Usage) Then
                Dim usageString As String = sidList(GsaComUtil.SectionSid_Usage)
                If String.Equals(usageString, SectionUsage.COLUMNS.ToString()) Then
                    usage = SectionUsage.COLUMNS
                ElseIf String.Equals(usageString, SectionUsage.FRAMING.ToString()) Then
                    usage = SectionUsage.FRAMING
                ElseIf String.Equals(usageString, SectionUsage.SLAB.ToString()) Then
                    usage = SectionUsage.SLAB
                ElseIf String.Equals(usageString, SectionUsage.WALL.ToString()) Then
                    usage = SectionUsage.WALL
                End If
            Else
                usage = SectionUsage.INVALID ' for now
            End If

            If sidList.ContainsKey(GsaComUtil.SectionSid_Symbol) Then
                sidFromGsa = sidList(GsaComUtil.SectionSid_Symbol)
            End If

            name = GsaComUtil.Arg(2, sGwaCommand)
            sArg = GsaComUtil.Arg(4, sGwaCommand)

            Dim iIndex As Integer = 6
            sArg = GsaComUtil.Arg(iIndex, sGwaCommand)
            Dim nLayer As Integer = CType(sArg, Integer)
            If nLayer < 0 Then
                iIndex = iIndex + 3 * Math.Abs(nLayer)
            Else
                iIndex = iIndex + 1
            End If
            sArg = GsaComUtil.Arg(iIndex, sGwaCommand)
            ematType = MembMatFromStr(sArg)

            iIndex = iIndex + 1
            sArg = GsaComUtil.Arg(iIndex, sGwaCommand)
            iMat = CType(sArg, Integer)

            iIndex = iIndex + 2
            sArg = GsaComUtil.Arg(iIndex, sGwaCommand)
            dThick = CType(sArg, Double)

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    Public Function GetLineEndNode(ByVal iLine As Integer, Optional ByVal bEnd As Boolean = True) As Integer
        Dim sidline As String = ""
        Dim N0 As Integer = -1
        Dim N1 As Integer = -1
        Dim N2 As Integer = -1, Ltype As GsaComUtil.LineType = GsaComUtil.LineType.LINEAR
        GetLine(iLine, sidline, N0, N1, N2, Ltype)
        If bEnd Then
            Return N1
        End If
        Return N0
    End Function
    Public Function AreaInsideArea(ByRef iArea As Integer, ByRef jArea As Integer) As Boolean
        Dim bResult As Boolean = CType(m_GSAObject.IsAreaInsideArea(iArea, jArea), Boolean)
        Return bResult
    End Function
    Public Sub SetVoid(ByRef areas As List(Of Integer))
        For Each area As Integer In areas
            For Each areaIn As Integer In areas
                If area.Equals(areaIn) Then
                    Continue For
                End If
                If AreaInsideArea(area, areaIn) Then
                    Dim _sid As String = String.Empty
                    Dim _gsa2DProp As Integer = 0
                    Dim _lines As List(Of Integer) = New List(Of Integer)
                    Dim _areaType As AreaType = AreaType.TWO_WAY
                    If GetArea(areaIn, _sid, _gsa2DProp, _lines, _areaType) Then
                        EditArea(areaIn, _lines, _gsa2DProp, _sid, AreaType.VOID)
                    End If
                End If
            Next
        Next
    End Sub

    Public Function EditArea(ByVal iArea As Integer, ByRef lines As List(Of Integer), ByRef iProp As Integer, ByVal uid As String, ByVal area As AreaType) As Integer

        'AREA.2 | ref | name | colour | type | span | property | group | lines | coefficient @end
        'AREA.1 | ref | name | type | span | property | group | lines | coefficient 
        Dim sGwaCommand As String = "SET, AREA"
        sGwaCommand += "," + iArea.ToString()
        sGwaCommand += ","                      'name
        sGwaCommand += ",NO_RGB"                'colour   
        sGwaCommand += "," + area.ToString()    'type
        sGwaCommand += "," + "0.0"              'span
        sGwaCommand += "," + iProp.ToString()   'property
        sGwaCommand += "," + "1"                'group
        sGwaCommand += ","
        For Each line As Integer In lines
            sGwaCommand += " " + line.ToString() 'lines
        Next
        sGwaCommand += ","
        sGwaCommand += ",0.0"                     'coefficient
        m_GSAObject.GwaCommand(sGwaCommand)
        If AreaExists(iArea) Then
            m_GSAObject.WriteSidTagValue("AREA", iArea, "RVT", uid)
        End If
        Return iArea
    End Function
    Public Function IsAreaVoid(ByRef iArea As Integer) As Boolean
        Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,AREA," & iArea.ToString))
        Dim sArg As String = GsaComUtil.Arg(4, sGwaCommand)
        If (sArg.Equals("TWO_WAY")) Then
            Return False
        End If
        Return True
    End Function

    Public Function GetArea(ByRef iArea As Integer, ByRef sidFromGsa As String, ByRef gsa2Dprop As Integer, ByRef lines As List(Of Integer), ByRef type As AreaType) As Boolean
        'AREA.2 | ref | name | colour | type | span | property | group | lines | coefficient 
        'AREA.1 | ref | name | type | span | property | group | lines | coefficient 
        If Not AreaExists(iArea) Then
            Return False
        End If

        lines = New List(Of Integer)
        Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,AREA," & iArea.ToString))
        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        sidFromGsa = GsaComUtil.ExtractId(sArg) ' sid
        sArg = GsaComUtil.Arg(4, sGwaCommand)
        If (sArg.Equals("TWO_WAY")) Then
            sArg = GsaComUtil.Arg(6, sGwaCommand)
            gsa2Dprop = CType(sArg, Integer)
            sArg = GsaComUtil.Arg(8, sGwaCommand)
            sArg = FormatLine(sArg)
            Dim words As String() = sArg.Trim().Split(New Char() {","c, " "c})
            For Each word As String In words
                lines.Add(CType(word.Trim(), Integer))
            Next
            type = AreaType.TWO_WAY
        Else
            sArg = GsaComUtil.Arg(6, sGwaCommand)
            gsa2Dprop = CType(sArg, Integer)
            sArg = GsaComUtil.Arg(8, sGwaCommand)
            Dim words As String() = sArg.Trim().Split(New Char() {","c, " "c})
            For Each word As String In words
                lines.Add(CType(word.Trim(), Integer))
            Next
            type = AreaType.VOID
        End If
        Return True
    End Function
    Private Function FormatLine(ByVal StrLine As String) As String
        Dim iExclude As New List(Of Integer), strOut As String = ""
        Dim delimiters As String() = New String() {",", " "}
        Dim strAppend As New List(Of String)
        Dim strsplit As String() = StrLine.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
        For i As Integer = 0 To strsplit.Length - 1
            If strsplit(i).Equals("to") Then
                Dim strReplace As String = ""
                Dim istart As Integer = Convert.ToInt32(strsplit(i - 1))
                Dim iend As Integer = Convert.ToInt32(strsplit(i + 1))
                For j As Integer = istart To iend
                    Dim strLineNum As String = j.ToString().Trim()
                    If Not strAppend.Contains(strLineNum) Then
                        strAppend.Add(strLineNum)
                    End If
                Next
                Continue For
            End If
            If Not strAppend.Contains(strsplit(i).Trim()) Then
                strAppend.Add(strsplit(i).Trim())
            End If
        Next
        For Each str As String In strAppend
            strOut = strOut + str + " "
        Next
        Return strOut.Trim()
    End Function

    Private Function FormatArea(ByVal strlist As String) As String
        Dim delimiters As String() = New String() {",", " "}
        Dim strsplit As String() = strlist.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
        For i As Integer = 0 To strsplit.Length - 1
            If strsplit(i).Equals("to") Then
                Dim strReplace As String = ""
                Dim istart As Integer = Convert.ToInt32(strsplit(i - 1))
                Dim iend As Integer = Convert.ToInt32(strsplit(i + 1))
                For j As Integer = istart To iend
                    strReplace = (strReplace & Convert.ToString(" ")) + j.ToString()
                Next
                strReplace = strReplace.Trim()
                If Not String.IsNullOrEmpty(strReplace) Then
                    strlist = strlist.Replace(istart.ToString() + " to " + iend.ToString(), strReplace).Replace(istart.ToString() + "to" + iend.ToString(), strReplace)
                End If
            End If
        Next
        strlist = strlist.Trim().Replace(" ", ",")
        Return strlist
    End Function
    Public Function GetRegion(ByRef iRegion As Integer, ByRef sidFromGsa As String, ByRef area As String) As Boolean
        'REGION.4 | ref | name | colour | plane | region | nodes | lines | areas |
        'CM2_MESHTOOLS | split | triangle | odd | opti | target | gradation | quality | 
        'shape | offset | elem | soil | size @end 
        Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,REGION," & iRegion.ToString))
        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        sidFromGsa = GsaComUtil.ExtractId(sArg) ' sid
        area = GsaComUtil.Arg(8, sGwaCommand)
        area = FormatArea(area)
    End Function
    Public Function IsEqual(ByRef pt1 As Double(), ByRef pt2 As Double()) As Boolean
        Dim bEqual As Boolean = False
        bEqual = IsEqual(pt1(0), pt2(0), 0.01)
        If bEqual.Equals(False) Then
            Return False
        End If
        bEqual = IsEqual(pt1(1), pt2(1), 0.01)
        If bEqual.Equals(False) Then
            Return False
        End If
        bEqual = IsEqual(pt1(2), pt2(2), 0.01)
        If bEqual.Equals(False) Then
            Return False
        End If
        Return bEqual
    End Function
    Public Function LineTypeOfLine(ByRef pt1 As Double(), ByRef pt2 As Double(), ByRef pt3 As Double(), ByRef type As LineType) As Integer
        Dim ListofLine As List(Of Integer) = LineOfArcType()
        For Each i As Integer In ListofLine

            Dim iNode0, iNode1, iNode2 As Integer
            Dim sidline As String = ""
            Dim bEqual As Boolean = False
            GetLine(i, sidline, iNode0, iNode1, iNode2, type)
            If type.Equals(LineType.ARC_THIRD_PT) Then

                Dim dNodeCord() As Double = ExtractNodeCoor(iNode0.ToString())
                ChangeUnits(dNodeCord)
                bEqual = IsEqual(dNodeCord, pt1)

                Dim dNodeCord1() As Double = ExtractNodeCoor(iNode1.ToString())
                ChangeUnits(dNodeCord1)
                bEqual = bEqual AndAlso IsEqual(dNodeCord1, pt3)

                Dim dNodeCord2() As Double = ExtractNodeCoor(iNode2.ToString())
                ChangeUnits(dNodeCord2)
                bEqual = bEqual AndAlso IsEqual(dNodeCord2, pt2)
                If (bEqual) Then
                    Return i
                End If


            Else

                Dim dNodeCord() As Double = ExtractNodeCoor(iNode0.ToString())
                ChangeUnits(dNodeCord)
                bEqual = IsEqual(dNodeCord, pt1)

                Dim dNodeCord1() As Double = ExtractNodeCoor(iNode1.ToString())
                ChangeUnits(dNodeCord1)
                bEqual = bEqual AndAlso IsEqual(dNodeCord1, pt2)
                If (bEqual) Then
                    Return i
                End If
            End If

        Next
        Return -1
    End Function

    Public Function GetRegion(ByRef iRegion As Integer, ByRef Lines As List(Of GSALine), ByRef iArea As List(Of Integer), ByRef iVoid As List(Of Integer)) As Boolean
        'recreate lines to draw arc properly in GSA.
        Try
            Dim ListLine As New List(Of Double())
            Dim ptToRemove As New List(Of Double())
            Dim ptFinal As New List(Of Double())
            Dim strLines() As String = Nothing
            Dim arrArea() As Integer = Nothing
            Dim arrVoid() As Integer = Nothing
            Lines = New List(Of GSALine)
            m_GSAObject.RegionPoints(iRegion, strLines, arrArea, arrVoid)

            'Area
            iArea = New List(Of Integer)
            If (Not arrArea Is Nothing) Then
                iArea.AddRange(arrArea)
            End If

            'void
            iVoid = New List(Of Integer)
            If (Not arrVoid Is Nothing) Then
                iVoid.AddRange(arrVoid)
            End If

            'line
            If (Not strLines Is Nothing) Then
                For Each str As String In strLines
                    Dim parts As String() = str.Replace("(", "").Replace(")", "").Trim().Split(New Char() {","c})
                    Dim dbPoint() As Double = {0.0, 0.0, 0.0}
                    dbPoint(0) = CType(parts(0), Double)
                    dbPoint(1) = CType(parts(1), Double)
                    dbPoint(2) = CType(parts(2), Double)
                    ChangeUnits(dbPoint)
                    ListLine.Add(dbPoint)
                Next
            End If



            For Each pt As Double() In ListLine
                Dim bFound As Boolean = False
                For iLine As Integer = 1 To HighestLine()
                    Dim iStart As Integer = 0
                    Dim iEnd As Integer = 0
                    Dim iMid As Integer = 0
                    Dim Ltype As GsaComUtil.LineType = GsaComUtil.LineType.LINEAR
                    Dim sidFromGsa As String = ""

                    GetLine(iLine, sidFromGsa, iStart, iEnd, iMid, Ltype)
                    If Ltype.Equals(LineType.ARC_THIRD_PT) Then
                        Dim dNodeCord() As Double = ExtractNodeCoor(iStart.ToString())
                        ChangeUnits(dNodeCord)

                        If (IsEqual(pt, dNodeCord)) Then
                            bFound = True
                            Exit For
                        End If


                        Dim dNodeCord1() As Double = ExtractNodeCoor(iEnd.ToString())
                        ChangeUnits(dNodeCord1)

                        If (IsEqual(pt, dNodeCord1)) Then
                            bFound = True
                            Exit For
                        End If

                        Dim dNodeCord2() As Double = ExtractNodeCoor(iMid.ToString())
                        ChangeUnits(dNodeCord2)

                        If (IsEqual(pt, dNodeCord2)) Then
                            bFound = True
                            Exit For
                        End If

                    Else
                        Dim dNodeCord() As Double = ExtractNodeCoor(iStart.ToString())
                        ChangeUnits(dNodeCord)
                        If (IsEqual(pt, dNodeCord)) Then
                            bFound = True
                            Exit For
                        End If

                        Dim dNodeCord1() As Double = ExtractNodeCoor(iEnd.ToString())
                        ChangeUnits(dNodeCord1)
                        If (IsEqual(pt, dNodeCord1)) Then
                            bFound = True
                            Exit For
                        End If
                    End If
                Next
                If (Not bFound) Then
                    ptToRemove.Add(pt)
                End If

            Next

            For Each pt As Double() In ListLine
                If ptToRemove.Contains(pt) Then
                    Continue For
                End If
                ptFinal.Add(pt)
            Next

            'GSALineDetail

            For pt As Integer = 0 To ptFinal.Count - 1
                Dim pt0 As Double() = ptFinal(pt)
                Dim pt1 As Double() = Nothing
                Dim pt2 As Double() = Nothing
                If pt.Equals(ptFinal.Count - 1) Then
                    pt1 = ptFinal(0)
                    pt2 = ptFinal(1)
                ElseIf pt.Equals(ptFinal.Count - 2) Then
                    pt1 = ptFinal(pt + 1)
                    pt2 = ptFinal(0)
                Else
                    pt1 = ptFinal(pt + 1)
                    pt2 = ptFinal(pt + 2)
                End If

                Dim type As LineType = LineType.LINEAR
                Dim intLineNumber As Integer = LineTypeOfLine(pt0, pt1, pt2, type)
                If intLineNumber > 0 Then

                    If type.Equals(LineType.ARC_THIRD_PT) Then
                        Dim line As New GSALine
                        line.StartPoint = pt0
                        line.EndPoint = pt1
                        line.MidPoint = pt2
                        line.LineType = LineType.ARC_THIRD_PT
                        Lines.Add(line)
                    End If
                    If type.Equals(LineType.ARC_RADIUS) Then
                        Dim pt0Middle As Double() = ExtractInterMediateNodeCoorOnCurve(intLineNumber.ToString(), False)
                        ChangeUnits(pt0Middle)
                        Dim line As New GSALine
                        line.StartPoint = pt0
                        line.EndPoint = pt1
                        line.MidPoint = pt0Middle
                        line.LineType = LineType.ARC_THIRD_PT
                        Lines.Add(line)
                    End If

                Else
                    Dim line As New GSALine
                    line.StartPoint = pt0
                    line.EndPoint = pt1
                    line.LineType = LineType.LINEAR
                    Lines.Add(line)
                End If

            Next

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    Public Function SetArea(ByRef lines As ArrayList, ByRef iProp As Integer, ByVal uid As String) As Integer

        'AREA.2 | ref | name | colour | type | span | property | group | lines | coefficient @end
        'AREA.1 | ref | name | type | span | property | group | lines | coefficient 

        Dim iArea As Integer = HighestArea()
        iArea += 1
        Dim sGwaCommand As String = "AREA"
        sGwaCommand += "," + iArea.ToString()
        sGwaCommand += ","                      'name
        sGwaCommand += ",NO_RGB"                'colour   
        sGwaCommand += "," + "TWO_WAY"          'type
        sGwaCommand += "," + "0.0"              'span
        sGwaCommand += "," + iProp.ToString()   'property
        sGwaCommand += "," + "1"                'group
        sGwaCommand += ","
        For Each line As Integer In lines
            sGwaCommand += " " + line.ToString() 'lines
        Next
        sGwaCommand += ","
        sGwaCommand += ",0.0"                     'coefficient
        m_GSAObject.GwaCommand(sGwaCommand)
        If AreaExists(iArea) Then
            m_GSAObject.WriteSidTagValue("AREA", iArea, "RVT", uid)
        End If
        Return iArea
    End Function

    'write a GSA Section
    'Public Function SetSection(ByVal iSec As Integer, ByVal sName As String, ByVal uid As String, ByVal usage As SectionUsage,
    '                           ByVal iAnalysisMat As Integer, ByVal sDesc As String, ByVal eMembType As MembType, ByVal eMatType As MembMat,
    '                           Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Integer
    '    '//	PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
    '    '//	PROP_SEC.2 | num | name | colour | member | desc | anal | mat | grade @end

    '    Dim sGwaCommand As String = ""
    '    If (0 = iSec) Then
    '        iSec = HighestSection() + 1
    '    End If
    '    Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
    '    If (bDescMap) Then
    '        sid = "{" & "DESCRIPTION" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
    '    End If
    '    If (bNameMap) Then
    '        sid = "{" & "NAME" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
    '    End If

    '    sGwaCommand = "PROP_SEC:"
    '    sGwaCommand += "{RVT:" & sid & "}"
    '    sGwaCommand += "," & iSec.ToString()        'number
    '    sGwaCommand += "," & sName                  'name
    '    sGwaCommand += ",NO_RGB"                    'colour
    '    sGwaCommand += "," & MembMatStr(eMatType)   'material type
    '    sGwaCommand += ",0"                         'grade
    '    sGwaCommand += "," & iAnalysisMat.ToString() 'analysis material
    '    sGwaCommand += "," & sDesc                  'description
    '    sGwaCommand += ",0"                        'cost

    '    m_GSAObject.GwaCommand(sGwaCommand)
    '    Return iSec
    'End Function

    Public Function SetSection(ByVal iSec As Integer, ByVal sName As String, ByVal uid As String, ByVal usage As SectionUsage,
                               ByVal iAnalysisMat As Integer, ByVal sDesc As String, ByVal eMembType As MembType, ByVal eMatType As MembMat,
                               Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Integer
        '//	PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
        '//	PROP_SEC.2 | num | name | colour | member | desc | anal | mat | grade @end

        Dim sGwaCommand As String = ""
        If (0 = iSec) Then
            iSec = HighestSection() + 1
        End If
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        If (bDescMap) Then
            sid = "{" & "DESCRIPTION" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        If (bNameMap) Then
            sid = "{" & "NAME" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If

        sGwaCommand = "PROP_SEC:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," & iSec.ToString()        'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += ",NO_RGB"                    'colour
        sGwaCommand += "," & MembMatStr(eMatType)   'material type
        sGwaCommand += "," & iAnalysisMat.ToString() 'analysis material
        sGwaCommand += ",0"                         'grade
        sGwaCommand += "," & sDesc                  'description
        sGwaCommand += ",0"                         'cost
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iSec
    End Function


    Public Function SetSectionSid(ByVal iSec As Integer, ByVal uid As String, ByVal usage As SectionUsage, Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Boolean
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        If (bDescMap) Then
            sid = "{" & "DESCRIPTION" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        If (bNameMap) Then
            sid = "{" & "NAME" & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        End If
        Return Me.SetSid("PROP_SEC", iSec, sid)
    End Function

    Public Function SetSectionSid2D(ByVal iSec As Integer, ByVal uid As String, ByVal usage As SectionUsage) As Boolean
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        Return Me.SetSid("PROP_2D", iSec, sid)
    End Function
    'read a GSA Section
    Public Function MemberType(ByVal iSec As Integer) As MembType
        Dim sName As String = ""
        Dim sid As String = ""
        Dim iMat As Integer = 0
        Dim sDes As String = ""
        Dim eMembType As MembType = MembType.BEAM
        Dim eMatType As MembMat = MembMat.CONCRETE
        Section(iSec, sName, sid, iMat, sDes, eMembType, eMatType)
        Return eMembType
    End Function
    Public Function MemberMaterial(ByVal iSec As Integer) As MembMat
        Dim sName As String = ""
        Dim sid As String = ""
        Dim iMat As Integer = 0
        Dim sDes As String = ""
        Dim eMembType As MembType = MembType.BEAM
        Dim eMatType As MembMat = MembMat.CONCRETE
        Section(iSec, sName, sid, iMat, sDes, eMembType, eMatType)
        Return eMatType
    End Function
    'read a GSA Section
    Public Function Section(ByVal iSec As Integer, ByRef sName As String, ByRef sid As String, ByRef iMat As Integer, ByRef sDesc As String, ByRef eMembType As MembType, ByRef eMatType As MembMat) As Boolean
        Dim usage As SectionUsage
        Dim ret As Boolean = Section(iSec, sName, sid, usage, iMat, sDesc, eMembType, eMatType)
        Return ret
    End Function
    Public Function Section(ByVal iSec As Integer, ByRef sName As String, ByRef sid As String, ByRef usage As SectionUsage, ByRef iAnalysisMat As Integer, ByRef sDesc As String, ByRef eMembType As MembType, ByRef eMatType As MembMat, Optional ByRef MapOp As String = "") As Boolean

        If _SecList.ContainsKey(iSec) Then
            Dim gsaSec As GSASection = _SecList(iSec)
            iSec = gsaSec.Number
            sName = gsaSec.Name
            sid = gsaSec.Sid
            usage = CType(gsaSec.SecUsage, SectionUsage)
            sDesc = gsaSec.Desc
            iAnalysisMat = gsaSec.AnalysisMat
            MapOp = gsaSec.MapOp
            eMatType = gsaSec.MaterialType
            eMembType = gsaSec.MembType
        Else
            'PROP_SEC.2 | num | name | colour | member | desc | anal | mat | grade @end
            '//	PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
            If Not SectionExists(iSec) Then
                Return False
            End If
            Dim gsaSec As New GSASection()
            Dim sGwaCommand As String = ""
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,PROP_SEC," & iSec.ToString))
            If String.IsNullOrEmpty(sGwaCommand) Then
                Return False
            End If

            Dim sArg As String
            sArg = GsaComUtil.Arg(0, sGwaCommand)
            Dim sidList As New SortedList(Of String, String)
            Dim sid_string As String = GsaComUtil.ExtractId(sArg)
            Me.ParseNestedSid(sid_string, sidList)

            If sidList.ContainsKey(GsaComUtil.SectionSid_Usage) Then
                Dim usageString As String = sidList(GsaComUtil.SectionSid_Usage)
                If String.Equals(usageString, SectionUsage.COLUMNS.ToString()) Then
                    usage = SectionUsage.COLUMNS
                ElseIf String.Equals(usageString, SectionUsage.FRAMING.ToString()) Then
                    usage = SectionUsage.FRAMING
                End If
            Else
                usage = SectionUsage.INVALID ' for now
            End If
            gsaSec.SecUsage = usage
            If sidList.ContainsKey(GsaComUtil.SectionSid_Symbol) Then
                sid = sidList(GsaComUtil.SectionSid_Symbol)
            End If
            If sidList.ContainsKey("DESCRIPTION") Then
                MapOp = "DESC"
                sid = sidList("DESCRIPTION")
            End If
            If sidList.ContainsKey("NAME") Then
                MapOp = "NAME"
                sid = sidList("NAME")
            End If
            gsaSec.Sid = sid
            gsaSec.MapOp = MapOp
            '//PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
            sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
            Debug.Assert(Integer.Equals(iSec, CInt(sArg)))
            gsaSec.Number = iSec
            sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
            sName = sArg
            gsaSec.Name = sName

            sArg = GsaComUtil.Arg(4, sGwaCommand) 'material type
            eMatType = MembMatFromStr(sArg)
            gsaSec.MaterialType = eMatType

            sArg = GsaComUtil.Arg(5, sGwaCommand) 'Analysis material
            iAnalysisMat = CInt(sArg)
            gsaSec.AnalysisMat = iAnalysisMat


            sArg = GsaComUtil.Arg(7, sGwaCommand) 'description
            sDesc = sArg
            gsaSec.Desc = sDesc


            _SecList.Add(iSec, gsaSec)
        End If

        Return True
    End Function
    Function SectionUsageType(ByVal b2D As Boolean, ByVal strEnt As String, ByVal iSecNum As Integer, ByVal strVert As String, ByVal strInc As String, ByVal bFromMemeb As Boolean) As SectionUsage

        Dim iHgstelem As Integer = HighestEnt(strEnt)
        Dim iEnt As Integer
        Dim bFram As Boolean = False
        Dim bColm As Boolean = False
        Dim bSlab As Boolean = False
        Dim bWall As Boolean = False


        For iEnt = 1 To iHgstelem

            Dim iProp As Integer = 1, uID As String = ""
            Dim iOrNode As Integer = 0, dBeta As Double = 0
            Dim sRelease As List(Of String) = Nothing
            Dim dOffset As List(Of Double()) = Nothing
            Dim eMembtype As MembType = MembType.UNDEF
            Dim eMembMattype As MembMat = MembMat.UNDEF

            Dim eletype As ElemType = ElemType.EL_BAR

            Dim sName As String = "", sEleList As String = ""
            Dim x As Double = 0.0, y As Double = 0.0, z As Double = 0.0
            Dim x1 As Double = 0.0, y1 As Double = 0.0, z1 As Double = 0.0
            Dim strDummy As String = ""
            Dim dFacetval As Double = 0
            Dim iGeom As Integer = 0
            Dim dRadius As Double = 0
            Dim iTopo2 As Integer = 0
            ' Dim Mat As GsaComUtil.MembMat
            Dim bOut As Boolean = False
            Dim iTopoList As New List(Of Integer)
            Dim sTopo As String = ""
            If "MEMB" = strEnt Then

                bOut = Member(iEnt, sName, iProp, uID, sTopo, dBeta, sRelease, dOffset, eMembtype)

                If Not bOut Then
                    Continue For
                End If
                If eMembtype.Equals(MembType.WALL) OrElse eMembtype.Equals(MembType.SLAB) Then
                    If Not b2D Then
                        Continue For
                    End If
                End If
                If eMembtype.Equals(MembType.BEAM) OrElse eMembtype.Equals(MembType.COLUMN) Then
                    If b2D Then
                        Continue For
                    End If
                End If
            Else

                bOut = Elem1d(iEnt, iProp, uID, iTopoList, iOrNode, dBeta, sRelease, dOffset, eletype, eMembtype, strDummy)
                If Not bOut Then
                    Continue For
                End If
                If Not ElemTypeIsBeamOrTruss(eletype) Then
                    Continue For
                End If
            End If

            If (iProp <> iSecNum) Then
                Continue For
            End If

            'if option = Select Revit member type from GSA Member type.
            If (bFromMemeb) Then
                If eMembtype.Equals(MembType.BEAM) Then
                    bFram = True
                    Continue For
                ElseIf eMembtype.Equals(MembType.COLUMN) Then
                    bColm = True
                    Continue For
                ElseIf eMembtype.Equals(MembType.SLAB) Then
                    bSlab = True
                    Continue For
                ElseIf eMembtype.Equals(MembType.WALL) Then
                    bWall = True
                    Continue For
                End If
            End If

            Dim bVertical As Boolean = True
            Dim bHoriZontal As Boolean = True
            If "MEMB" = strEnt Then
                Dim orderdNodes As List(Of List(Of Integer)) = Nothing
                Dim voids As List(Of List(Of Integer)) = Nothing
                Dim arcNodes As List(Of Integer) = Nothing
                StringTolist(sTopo, orderdNodes, arcNodes, voids)

                For Each nodes As List(Of Integer) In orderdNodes
                    Dim iMax As Integer = Math.Max(nodes.Count - 3, 0)
                    For node As Integer = 0 To iMax

                        'first node
                        Dim iNode As Integer = node
                        Dim sNodeCord() As Double = ExtractNodeCoor(nodes.Item(iNode).ToString())

                        'second node
                        iNode = iNode + 1
                        If iNode > nodes.Count - 1 Then
                            iNode = iNode - 1
                        End If
                        Dim eMidCord() As Double = ExtractNodeCoor(nodes.Item(iNode).ToString())

                        'third node
                        iNode = iNode + 1
                        If iNode > nodes.Count - 1 Then
                            iNode = iNode - 1
                        End If
                        Dim eNodeCord() As Double = ExtractNodeCoor(nodes.Item(iNode).ToString())

                        bVertical = IsInVerticalPlane(sNodeCord, eNodeCord, eMidCord)
                        bHoriZontal = IsInHorizontalPlane(sNodeCord, eNodeCord, eMidCord)

                    Next node
                Next
            Else
                bVertical = IsVertical(iEnt)
                bHoriZontal = IsHorizontal(iEnt)
            End If

            Dim bColTemp As Boolean = bColm
            Dim bFrmTemp As Boolean = bFram
            Dim bWallTemp As Boolean = bWall
            Dim bSlabTemp As Boolean = bSlab

            If bVertical Then
                If strVert.Contains("Columns") Then
                    bColm = True
                Else
                    bFram = True
                End If
                bWall = True
            Else
                If bHoriZontal Then
                    'horizontal
                    bFram = True
                Else
                    'inclined
                    If strInc.Contains("Columns") Then
                        bColm = True
                    Else
                        bFram = True
                    End If
                End If
                bSlab = True
            End If

            If b2D Then
                If Not bWallTemp.Equals(bWall) AndAlso Not bSlabTemp.Equals(bSlab) Then
                    'same section referred by wall/slab
                    Return SectionUsage.INVALID
                End If
            Else
                If Not bColTemp.Equals(bColm) AndAlso Not bFrmTemp.Equals(bFram) Then
                    'same section referred by beam/column
                    Return SectionUsage.INVALID
                End If
            End If
        Next

        Dim iFram As Integer = 1
        Dim iColm As Integer = 2
        Dim iSlab As Integer = 3
        Dim iWall As Integer = 4
        If b2D Then
            If bSlab AndAlso bWall Then
                Return SectionUsage.INVALID
            End If
            If (bSlab) Then
                Return SectionUsage.SLAB
            End If
            If (bWall) Then
                Return SectionUsage.WALL
            End If
        Else
            If bFram AndAlso bColm Then
                Return SectionUsage.INVALID
            End If
            If (bFram) Then
                Return SectionUsage.FRAMING
            End If
            If (bColm) Then
                Return SectionUsage.COLUMNS
            End If
        End If
        Return SectionUsage.NOT_USED

    End Function

    Public Shared Function IsIncline(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean
        Dim vect() As Double = {EndPoint(0) - startpoint(0), EndPoint(1) - startpoint(1), EndPoint(2) - startpoint(2)}
        Dim denom As Double = (Math.Sqrt(Math.Pow(vect(0), 2) + Math.Pow(vect(1), 2) + Math.Pow(vect(2), 2)))
        Dim alpha As Double = Abs(vect(0) / denom)
        Dim beta As Double = Abs(vect(1) / denom)
        Dim gamma As Double = Abs(vect(2) / denom)
        If alpha > 0.0001 OrElse beta > 0.0001 OrElse gamma > 0.0001 Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsEqual(ByVal d1 As Double, ByVal d2 As Double, ByVal Tol As Double) As Boolean
        If Abs(d1 - d2) < Tol Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Shared Function IsInVerticalPlane(ByVal startpoint() As Double, ByVal EndPoint() As Double, ByVal MidPoint() As Double) As Boolean
        Dim x1 As Double = startpoint(0)
        Dim x2 As Double = EndPoint(0)
        Dim x3 As Double = MidPoint(0)
        Dim Tolerance As Double = 0.1
        Dim b1 As Boolean = IsEqual(x1, x2, Tolerance)
        Dim b2 As Boolean = IsEqual(x2, x3, Tolerance)

        Dim y1 As Double = startpoint(1)
        Dim y2 As Double = EndPoint(1)
        Dim y3 As Double = MidPoint(1)
        Dim b3 As Boolean = IsEqual(y1, y2, Tolerance)
        Dim b4 As Boolean = IsEqual(y2, y3, Tolerance)

        If b1 AndAlso b2 AndAlso b3 AndAlso b4 Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsInHorizontalPlane(ByVal startpoint() As Double, ByVal EndPoint() As Double, ByVal MidPoint() As Double) As Boolean
        Dim Tolerance As Double = 0.1
        Dim z1 As Double = startpoint(2)
        Dim z2 As Double = EndPoint(2)
        Dim z3 As Double = MidPoint(2)
        Dim b1 As Boolean = IsEqual(z1, z2, Tolerance)
        Dim b2 As Boolean = IsEqual(z2, z3, Tolerance)
        If b1.Equals(True) AndAlso b2.Equals(True) Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsInclined(ByVal startpoint() As Double, ByVal EndPoint() As Double) As Boolean
        Dim Vec As Double() = {EndPoint(0) - startpoint(0), EndPoint(1) - startpoint(1), EndPoint(2) - startpoint(2)}
        Dim denom As Double = Math.Sqrt(Math.Pow(Vec(0), 2) + Math.Pow(Vec(1), 2) + Math.Pow(Vec(2), 2))
        Dim alpha As Double = Abs(Vec(0) / denom)
        Dim beta As Double = Abs(Vec(1) / denom)
        Dim gamma As Double = Abs(Vec(2) / denom)
        If alpha > 0.0001 OrElse beta > 0.0001 OrElse gamma > 0.0001 Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function IsInHorizontal(ByVal CrossProduct() As Double) As Boolean
        Dim EndPoint As Double() = {0, 0, 1}
        Dim x1 As Double = CrossProduct(0)
        Dim y1 As Double = CrossProduct(1)
        Dim z1 As Double = CrossProduct(2)

        Dim x2 As Double = EndPoint(0)
        Dim y2 As Double = EndPoint(1)
        Dim z2 As Double = EndPoint(2)
        Dim VecCross As Double = (y1 * z2 - z1 * y2) - (x1 * z2 - z1 * x2) + (x1 * y2 - y1 * x2)
        Return False
    End Function
    Public Shared Function IsInVertical(ByVal CrossProduct() As Double) As Boolean
        Dim EndPoint As Double() = {1, 0, 0}
        Dim x1 As Double = CrossProduct(0)
        Dim y1 As Double = CrossProduct(1)
        Dim z1 As Double = CrossProduct(2)

        Dim x2 As Double = EndPoint(0)
        Dim y2 As Double = EndPoint(1)
        Dim z2 As Double = EndPoint(2)
        Dim VecCross As Double = (y1 * z2 - z1 * y2) - (x1 * z2 - z1 * x2) + (x1 * y2 - y1 * x2)
        Return False
    End Function
    Public Function EntIsVertical(ByVal element As Integer, ByVal eType As EntType) As Boolean
        Dim iResult As Boolean = CType(m_GSAObject.ElemIsVertical(element, eType), Boolean)
        Return iResult
    End Function
    Public Function EntIsHorizontal(ByVal element As Integer, ByVal eType As EntType) As Boolean
        Dim iResult As Boolean = CType(m_GSAObject.ElemIsHorizontal(element, eType), Boolean)
        Return iResult

    End Function
    Public Function IsVertical(ByVal element As Integer) As Boolean
        If (_IsVet.ContainsKey(element)) Then
            Return _IsVet(element)
        Else
            Dim iResult As Integer = m_GSAObject.ElemIsVertical(element, m_eSelType)
            _IsVet.Add(element, CType(iResult, Boolean))
            Return CType(iResult, Boolean)
        End If
    End Function
    Public Function IsHorizontal(ByVal element As Integer) As Boolean
        If (_IsHoz.ContainsKey(element)) Then
            Return _IsHoz(element)
        Else
            Dim iResult As Integer = m_GSAObject.ElemIsHorizontal(element, m_eSelType)
            _IsHoz.Add(element, CType(iResult, Boolean))
            Return CType(iResult, Boolean)
        End If
    End Function
    Public Function Set2dProp(ByVal iProp As Integer, ByVal uid As String, ByVal usage As GsaComUtil.SectionUsage, ByVal sName As String, ByVal dThick As Double, ByVal eType As Type2D, ByVal eMaterType As GsaComUtil.MembMat, ByVal iMat As Integer) As Integer
        'PROP_2D.5 | num | name | colour | type | axis | mat | mat_type | grade | design | thick | ref_pt | ref_z | @end
        'mass | flex | shear | inplane | weight | @end
        'PROP_2D0.3 | num | name | colour | type | axis | mat | mat_type | grade | design | thick | ref_pt | ref_z | @end
        'mass | flex | inplane | weight| bending | @End
        'is_env { | energy | CO2A | CO2B | CO2C | CO2D | recycle | user }
        'PROP_2D.2 | num | name | colour | axis | mat | type | thick | mass | bending
        'PROP_2D.3 | num | name | colour | type | axis | mat | mat_type | grade | thick | ref_pt | @end
        If (0 = iProp) Then
            iProp = HighestProp2d() + 1
        End If
        sName = sName.Replace("""", String.Empty)
        Dim sType As String = eType.ToString()
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        Dim sGwaCommand As String = "PROP_SEC:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += iProp.ToString() + ","
        sGwaCommand += sName + ","
        sGwaCommand += "NO_RGB,"    'colour
        sGwaCommand += eType.ToString() + ","  ' 2D element type
        sGwaCommand += "GLOBAL,"
        sGwaCommand += "0," ' analysis material
        sGwaCommand += eMaterType.ToString() + ","
        sGwaCommand += iMat.ToString() + "," 'undefined
        sGwaCommand += "0," 'slab design peoperty
        sGwaCommand += dThick.ToString() + ","
        sGwaCommand += "CENTROID," 'refrence point centroid
        sGwaCommand += "0.0," 'set defuault offset to 0
        sGwaCommand += "0.0,100%,100%,100%,100%,NO_ENV"
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iProp
    End Function
    'write a GSA Material
    Public Function SetMaterial(ByVal iMat As Integer, ByVal sName As String, ByVal sid As String, ByVal sDesc As String,
                                  ByVal dE As Double, ByVal dNu As Double, ByVal dG As Double, ByVal dRho As Double, ByVal dAlpha As Double,
                                  ByVal dYield As Double, ByVal dUltimate As Double, ByVal dEh As Double, ByVal dBeta As Double, ByVal dDamp As Double) As Integer
        Dim sGwaCommand As String = ""

        If (iMat = 0) Then
            iMat = HighestMaterial() + 1
        End If
        ' ++
        'MAT_ANAL | num | MAT_ELAS_ISO | name | colour | 6 | E | nu | rho | alpha | G | damp | 0 | 0 | env | rebar |
        'country | variant | grade | eE | eCO2 | recycle | user |
        '{ country | variant | grade | eE | eCO2 | recycle | user }
        sGwaCommand = "MAT_ANAL:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," & iMat.ToString()                   'number
        sGwaCommand += ",MAT_ELAS_ISO"                         '@desc
        sGwaCommand += "," & sName                             'name
        sGwaCommand += ",NO_RGB"                               'color
        sGwaCommand += ",6"                                    'number of parameter
        sGwaCommand += "," & dE.ToString
        sGwaCommand += "," & dNu.ToString
        sGwaCommand += "," & dRho.ToString
        sGwaCommand += "," & dAlpha.ToString
        sGwaCommand += "," & dG.ToString
        ' sGwaCommand += "," & dYield.ToString
        'sGwaCommand += "," & dUltimate.ToString
        ' sGwaCommand += "," & dEh.ToString
        'sGwaCommand += "," & dBeta.ToString
        sGwaCommand += "," & dDamp.ToString
        sGwaCommand += ",0,0"
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iMat
    End Function

    'read a GSA Material
    Public Function Material(ByVal iMat As Integer, ByRef sName As String, ByRef sDesc As String, ByRef sidFromGsa As String,
                                    ByRef dE As Double, ByRef dNu As Double, ByRef dG As Double, ByRef dRho As Double, ByRef dAlpha As Double,
                                    ByRef dYield As Double, ByRef dUltimate As Double, ByRef dEh As Double, ByRef dBeta As Double, ByRef dDamp As Double) As Boolean
        If Not MaterialExists(iMat) Then
            Return False
        End If
        ' ++
        'MAT_ANAL | num | MAT_ELAS_ISO | name | colour | 6 | E | nu | rho | alpha | G | damp | 0 | 0 | env | rebar |
        'country | variant | grade | eE | eCO2 | recycle | user |
        '{ country | variant | grade | eE | eCO2 | recycle | user }

        Dim sGwaCommand As String = ""
        Dim sArg As String

        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MAT_ANAL," & iMat.ToString))
        If String.IsNullOrEmpty(sGwaCommand) Then
            Return False
        End If
        Dim iCount As Integer = 0
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        sidFromGsa = GsaComUtil.ExtractId(sArg) ' sid
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand) 'number
        Debug.Assert(Integer.Equals(iMat, CInt(sArg)))
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand) 'description
        sDesc = sArg
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand) 'name
        sName = sArg
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand) 'color
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand) 'num parameter

        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dE = Val(sArg)
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dNu = Val(sArg)
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dRho = Val(sArg)
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dAlpha = Val(sArg)
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dG = Val(sArg)

        'sArg = GsaComUtil.Arg(10, sGwaCommand)
        'dYield = Val(sArg)

        'sArg = GsaComUtil.Arg(11, sGwaCommand)
        'dUltimate = Val(sArg)

        'sArg = GsaComUtil.Arg(12, sGwaCommand)
        'dEh = Val(sArg)

        'sArg = GsaComUtil.Arg(13, sGwaCommand)
        'dBeta = Val(sArg)
        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dDamp = Val(sArg)

        Return True

    End Function
    Public Function Material(ByVal iMat As Integer, ByRef sName As String, ByRef sDesc As String, ByRef sid As String) As Boolean

        Dim dE As Double = 0.0,
            dNu As Double = 0.0,
            dG As Double = 0.0,
            dRho As Double = 0.0,
            dAlpha As Double = 0.0,
            dYield As Double = 0.0,
            dUltimate As Double = 0.0,
            dEh As Double = 0.0,
            dBeta As Double = 0.0,
            dDamp As Double = 0.0

        Dim ret As Boolean = Me.Material(iMat, sName, sDesc, sid, dE, dNu, dG, dRho, dAlpha, dYield, dUltimate, dEh, dBeta, dDamp)
        Return ret
    End Function
    Public Function MaterialIsIsotropic(ByVal iMat As Integer) As Boolean
        If Not Me.MaterialExists(iMat) Then
            Return False
        End If
        Dim commandResult As String = CStr(m_GSAObject.GwaCommand("GET, MAT," & iMat))
        If String.IsNullOrEmpty(commandResult) Then
            Return False
        End If

        Dim arg As String = GsaComUtil.Arg(0, commandResult)
        If arg.Contains("MAT_ISO") Then
            Return True
        Else
            Return False
        End If
    End Function

    Function ElemTypeFromString(ByVal sKey As String) As ElemType

        ' strip the string of sid if any
        If sKey.Contains(":") Then
            sKey = sKey.Substring(0, sKey.IndexOf(":"))
        End If
        ' Get the element type corresponding to a keyword
        Dim etype As ElemType = ElemType.EL_UNDEF

        Select Case sKey
            Case "MEMB", "MEMB"
                etype = ElemType.EL_BEAM
            Case "BEAM"
                etype = ElemType.EL_BEAM
            Case "BAR"
                etype = ElemType.EL_BAR
            Case "TIE"
                etype = ElemType.EL_TIE
            Case "STRUT"
                etype = ElemType.EL_STRUT
            Case "SPRING"
                etype = ElemType.EL_SPRING
            Case "LINK"
                etype = ElemType.EL_LINK
            Case "CABLE"
                etype = ElemType.EL_CABLE
            Case "QUAD4"
                etype = ElemType.EL_QUAD4
            Case "QUAD8"
                etype = ElemType.EL_QUAD8
            Case "TRI3"
                etype = ElemType.EL_TRI3
            Case "TRI6"
                etype = ElemType.EL_TRI6
            Case Else
                etype = ElemType.EL_UNDEF
        End Select

        Return etype
    End Function
    Function ElemTypeIsBeam(ByVal etype As ElemType) As Boolean
        Select Case etype
            Case ElemType.EL_BAR, ElemType.EL_BEAM, ElemType.EL_STRUT, ElemType.EL_TIE
                Return True
            Case Else
                Return False
        End Select

    End Function
    Function ElemTypeString(ByVal eType As ElemType) As String
        Dim sType As String = ""
        Select Case eType
            Case ElemType.EL_FLATPLATE
                sType = "PLATE"
            Case ElemType.EL_PLANESTRESS
                sType = "STRESS"
            Case Else
                sType = "PLATE"
        End Select
        Return sType
    End Function
    Public Sub SetGsaModelUnits(ByVal units As GsaComUtil.Units)
        Select Case units
            Case GsaComUtil.Units.IMPERIAL
                m_GSAObject.GwaCommand("UNIT_DATA,FORCE,lbf")
                m_GSAObject.GwaCommand("UNIT_DATA,LENGTH,ft") 'used for section property unit conversion: A, I...
                m_GSAObject.GwaCommand("UNIT_DATA,DISP,in") 'Gen_SectionMatchDesc uses the DISP units for interpreting general section dimensions
                m_GSAObject.GwaCommand("UNIT_DATA,SECTION,in")
                m_GSAObject.GwaCommand("UNIT_DATA,MASS,lb")
                m_GSAObject.GwaCommand("UNIT_DATA,TIME,s")
                m_GSAObject.GwaCommand("UNIT_DATA,TEMP,°F")
                m_GSAObject.GwaCommand("UNIT_DATA,STRESS,kip/in²")
                m_GSAObject.GwaCommand("UNIT_DATA,ACCEL,ft/s²")
            Case GsaComUtil.Units.METRIC
                m_GSAObject.GwaCommand("UNIT_DATA,FORCE,N")
                m_GSAObject.GwaCommand("UNIT_DATA,LENGTH,m") 'used for section property unit conversion: A, I...
                m_GSAObject.GwaCommand("UNIT_DATA,DISP,m") 'Gen_SectionMatchDesc uses the DISP units for interpreting general section dimensions
                m_GSAObject.GwaCommand("UNIT_DATA,SECTION,m")
                m_GSAObject.GwaCommand("UNIT_DATA,MASS,kg")
                m_GSAObject.GwaCommand("UNIT_DATA,TIME,s")
                m_GSAObject.GwaCommand("UNIT_DATA,TEMP,°F")
                m_GSAObject.GwaCommand("UNIT_DATA,STRESS,N/m²")
                m_GSAObject.GwaCommand("UNIT_DATA,ACCEL,m/s²")
        End Select
    End Sub
    ''' <summary>
    ''' CAUTION: Special function for use ONLY for setting material units
    ''' </summary>
    ''' <param name="unitStrings"></param>
    ''' <remarks></remarks>
    Public Sub SetGsaTemporaryUnitsMaterial(ByRef unitStrings As String())
        ' for setting material bizzare unit factors only
        For Each unitString As String In unitStrings
            m_GSAObject.GwaCommand(unitString)
        Next
    End Sub

    Function ElemNumNode(ByVal eType As ElemType) As Integer

        ' Get the number of nodes associated with the element type

        ElemNumNode = 0

        If (eType = ElemType.EL_GROUND) Then
            ElemNumNode = 1
        ElseIf (eType = ElemType.EL_MASS) Then
            ElemNumNode = 1
        ElseIf (eType = ElemType.EL_BEAM) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_BAR) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_TIE) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_STRUT) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_SPRING) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_LINK) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_CABLE) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_SPACER) Then
            ElemNumNode = 2
        ElseIf (eType = ElemType.EL_QUAD4) Then
            ElemNumNode = 4
        ElseIf (eType = ElemType.EL_QUAD8) Then
            ElemNumNode = 8
        ElseIf (eType = ElemType.EL_TRI3) Then
            ElemNumNode = 3
        ElseIf (eType = ElemType.EL_TRI6) Then
            ElemNumNode = 6
        End If

    End Function

    Function ListName(ByVal iList As Integer) As String
        'LIST | num | name | type | list

        If Not Me.ListExists(iList) Then
            Return Nothing
        End If
        Dim result As String = CStr(m_GSAObject.GwaCommand("GET,LIST," & iList.ToString()))
        Dim parts As String() = result.Split(New Char() {","c})
        Debug.Assert(parts.Length > 2)
        Return parts(2)

    End Function
    Public Function ListToString(ByRef lists As List(Of Integer)) As String
        Dim cStrItem As String = ""
        For Each it As Integer In lists
            cStrItem = cStrItem + " " + it.ToString()
        Next
        Return cStrItem
    End Function
    Public Sub StringTolist(ByVal sTopo As String, ByRef orderedNodes As List(Of List(Of Integer)), ByRef arcNode As List(Of Integer), ByRef voids As List(Of List(Of Integer)))
        Dim bStart As Boolean = False
        Dim bStartL As Boolean = False
        Dim bNewKey As Boolean = False
        Dim sVoids As List(Of Integer) = Nothing
        Dim Nodes As New List(Of Integer)
        orderedNodes = New List(Of List(Of Integer))
        arcNode = New List(Of Integer)
        voids = New List(Of List(Of Integer))
        Dim words As String() = sTopo.Split(New Char() {" "c})
        For i As Integer = 0 To words.Length - 1
            Dim str As String = words(i)
            Dim iNode As Integer = -1
            ' do not read 
            If str.Contains("L(") OrElse str.Contains("P(") Then
                bNewKey = True
                bStartL = True
                If Nodes.Count > 0 Then
                    orderedNodes.Add(Nodes)
                    Nodes = New List(Of Integer)
                End If
                Continue For
            ElseIf str.Contains(")") AndAlso bStartL Then
                bStartL = False
                Continue For
            ElseIf bStartL Then
                Continue For
            End If
            'reading void
            If str.Contains("V(") Then
                bNewKey = True
                If Nodes.Count > 0 Then
                    orderedNodes.Add(Nodes)
                    Nodes = New List(Of Integer)
                End If
                sVoids = New List(Of Integer)
                bStart = True
                str = str.Replace("V(", "")
                If Integer.TryParse(str, iNode) Then
                    sVoids.Add(iNode)
                End If
                Continue For
            ElseIf str.Contains(")") AndAlso bStart Then
                bStart = False
                str = str.Replace(")", "")
                If Integer.TryParse(str, iNode) Then
                    sVoids.Add(iNode)
                End If
                If sVoids.Count > 0 Then
                    voids.Add(sVoids)
                End If
                Continue For
            ElseIf bStart Then
                If Integer.TryParse(str, iNode) Then
                    sVoids.Add(iNode)
                End If
                Continue For
            End If
            If str.Contains("A") Then
                str = str.Replace("A", "")
                If Integer.TryParse(str, iNode) AndAlso iNode > 0 Then
                    arcNode.Add(iNode)
                End If
            End If
            If str.Contains("R") Then
                str = str.Replace("R", "")
            End If
            If Not bNewKey Then
                If Nodes.Count < 1 Then
                    Nodes = New List(Of Integer)
                End If
            End If

            If Integer.TryParse(str, iNode) Then
                Nodes.Add(iNode)
            End If

            If i = words.Length - 1 Then
                orderedNodes.Add(Nodes)
                Nodes = New List(Of Integer)
            End If
        Next
    End Sub
    Private Function ListString(ByVal iList As Integer) As String
        If Not Me.ListExists(iList) Then
            Return Nothing
        End If
        Dim list As String = CStr(m_GSAObject.GwaCommand("GET, LIST," & iList.ToString()))
        Return list
    End Function
    Public Function ListTypeIsMember(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("MEMB") Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function ListTypeIsArea(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("AREA") Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function ListTypeIsRegion(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("AREA") Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function ListTypeIsElement(ByVal iList As Integer) As Boolean
        Dim listString As String = Me.ListString(iList)
        If String.IsNullOrEmpty(listString) Then
            Return False
        End If
        If listString.Contains("ELEM") Then
            Return True
        Else
            Return False
        End If

    End Function
    Function ListItemsInList(ByVal iList As Integer) As List(Of Integer)
        Dim check As Object = m_GSAObject.GwaCommand("EXIST,LIST," & iList.ToString())
        Dim iCheck As Integer = 0

        Dim items As New List(Of Integer)
        Int32.TryParse(check.ToString(), iCheck)
        If 0 = iCheck Then
            Return items
        End If

        Dim resultObj As Object = m_GSAObject.GwaCommand("GET,LIST," & iList.ToString())
        If resultObj Is Nothing Then
            Return items
        End If

        Dim result As String = resultObj.ToString(), sEnt As String = "", listType As String = ""
        If result.Contains("ELEMENT") Then
            sEnt = "EL"
            listType = "ELEM"
        ElseIf result.Contains("MEMB") Then
            listType = "MEMB"
            sEnt = "MEMB"
        ElseIf result.Contains("AREA") Then
            listType = "AREA"
            sEnt = "AREA"
        ElseIf result.Contains("REGION") Then
            listType = "REGION"
            sEnt = "REGION"
        Else
            Return items
        End If

        Dim parts As String() = result.Split(New Char() {","c}, StringSplitOptions.None)

        Dim nElem As Integer = CInt(m_GSAObject.GwaCommand("HIGHEST," & sEnt))
        If nElem = 0 Then
            Return items
        End If
        For i As Integer = 1 To nElem
            If Val(m_GSAObject.GwaCommand("EXIST," & sEnt & "," & i.ToString())) = 0 Then
                Continue For
            End If
            If (CBool(m_GSAObject.IsItemIncluded(listType, i, parts(4)))) Then
                items.Add(i)
            End If
        Next
        Return items

    End Function
    Function ElemDesc(ByVal eType As ElemType) As String

        ' Get a string that describes the element

        ElemDesc = "UNDEF"

        If (eType = ElemType.EL_GROUND) Then
            ElemDesc = "GROUND"
        ElseIf (eType = ElemType.EL_MASS) Then
            ElemDesc = "MASS"
        ElseIf (eType = ElemType.EL_BEAM) Then
            ElemDesc = "BEAM"
        ElseIf (eType = ElemType.EL_BAR) Then
            ElemDesc = "BAR"
        ElseIf (eType = ElemType.EL_TIE) Then
            ElemDesc = "TIE"
        ElseIf (eType = ElemType.EL_STRUT) Then
            ElemDesc = "STRUT"
        ElseIf (eType = ElemType.EL_SPRING) Then
            ElemDesc = "SPRING"
        ElseIf (eType = ElemType.EL_LINK) Then
            ElemDesc = "LINK"
        ElseIf (eType = ElemType.EL_CABLE) Then
            ElemDesc = "CABLE"
        ElseIf (eType = ElemType.EL_SPACER) Then
            ElemDesc = "SPACER"
        ElseIf (eType = ElemType.EL_QUAD4) Then
            ElemDesc = "QUAD4"
        ElseIf (eType = ElemType.EL_QUAD8) Then
            ElemDesc = "QUAD8"
        ElseIf (eType = ElemType.EL_TRI3) Then
            ElemDesc = "TRI3"
        ElseIf (eType = ElemType.EL_TRI6) Then
            ElemDesc = "TRI6"
        End If

    End Function

    'Function GsaGwaCommandObj(ByVal cGwaCommand As String) As System.Object
    '    GsaGwaCommandObj = m_GSAObject.GwaCommand(cGwaCommand)
    'End Function
    Function SectUsage(ByVal sectionNum As Integer) As SectionUsage
        Dim usageInt As Integer = m_GSAObject.SectionUsage(sectionNum, m_eSelType) ' Do this for member right now
        Dim usage As SectionUsage = CType(usageInt, SectionUsage)
        Return usage
    End Function

    Function CATSectionToSNFamily(ByVal parts As String(), ByVal usage As SectionUsage,
                                ByRef familyName As String) As Boolean

        familyName = ""
        Dim catAbr As String = parts(1)
        If SectionUsage.FRAMING = usage Then
            Select Case catAbr
                Case "C", "HP", "L", "M", "MC", "MT", "P", "PX", "PXX", "S", "ST", "TS", "W", "WT"
                    familyName += "American_"
                Case "A-CHS250", "A-CHS350", "A-EA", "A-PFC", "A-RHS350", "A-RHS450", "A-RSJs", "A-SHS350", "A-SHS450", "A-UA", "A-UB", "A-UBP", "A-UC"
                    familyName += "Australian_"
                Case "BP", "CH", "CHS", "EA", "PFC", "RHS", "SHS", "TUB", "TUC", "UA", "UB", "UC", "UJ"
                    familyName += "British_"
                Case "UKA", "UKB", "UKBP", "UKC", "UKPFC"
                    familyName += "Corus Advance_"
                Case Else
                    familyName = ""
                    Return False
            End Select
        ElseIf SectionUsage.COLUMNS = usage Then
            Select Case catAbr
                Case "HP", "M", "P", "S", "W"
                    familyName += "American_"
                Case "A-CHS250", "A-CHS350", "A-RHS350", "A-RHS450", "A-RSJs", "A-SHS350", "A-SHS450", "A-UB", "A-UBP", "A-UC"
                    familyName += "Australian_"
                Case "BP", "CH", "CHS", "RHS", "SHS", "UB", "UC", "UJ"
                    familyName += "British_"
                Case "UKB", "UKBP", "UKC"
                    familyName += "Corus Advance_"
                Case Else
                    familyName = ""
                    Return False
            End Select
        End If
        familyName += catAbr + "_" + usage.ToString().ToLower()
        Return True
    End Function
    ' From an STD section, find out which rfa file is to be used
    ' and the dimensions of the section
    Function STDSectionToSNFamily(ByRef parts As String(), ByVal usage As SectionUsage, ByRef familyName As String) As Boolean

        Debug.Assert(String.Equals(parts(0), "STD"))
        Dim cf As Double = 3.28 / 1000

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New Char() {"("c, ")"c})
        If shapeSubString.Length > 1 Then
            sShape = shapeSubString(0)
            Select Case shapeSubString(1)
                Case "m"
                    cf = 1 / 3.28
                Case "mm"
                    cf = 1000 / 3.28
                Case "in"
                    cf = 12
                Case "ft"
                    cf = 1
                    'Case ""
            End Select
        End If
        familyName = ""

        Select Case sShape
            Case "GI", "I"
                familyName = "Generic_I_"
            Case "CH"
                familyName = "Generic_CH_"
            Case "CHS"
                familyName = "Generic_CHS_"
            Case "C"
                familyName = "Generic_C_"
            Case "RHS"
                familyName = "Oasys_Generic_RHS_"
            Case "R"
                familyName = "Generic_R_"
            Case "T"
                familyName = "Generic_T_"
            Case Else
                Return False
        End Select

        familyName += usage.ToString().ToLower()
        Return True
    End Function
    ''' <summary>
    ''' Given a descriptionm, return a map of dimension name to dimension value
    ''' </summary>
    ''' <param name="desc"></param>
    ''' <param name="dimensions"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function SectionDimensions(ByVal desc As String, ByRef dimensions As SortedList) As Boolean

        Dim parts As String() = Nothing
        parts = desc.Split(New [Char]() {"%"c, " "c}, StringSplitOptions.RemoveEmptyEntries)

        Debug.Assert(String.Equals(parts(0), "STD"))
        Dim cf As Double = 3.28 / 1000

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New Char() {"("c, ")"c})
        If shapeSubString.Length > 1 Then
            sShape = shapeSubString(0)
            Select Case shapeSubString(1)
                Case "m"
                    cf = 1 / 3.28
                Case "mm"
                    cf = 1000 / 3.28
                Case "in"
                    cf = 12
                Case "ft"
                    cf = 1
                    'Case ""
            End Select
        End If

        Select Case sShape
            Case "GI", "I"
                ExtractDimensions_I(parts, dimensions, cf)
            Case "CH"
                ExtractDimensions_CH_T(parts, dimensions, cf)
            Case "CHS"
                ExtractDimensions_CHS(parts, dimensions, cf)
            Case "C"
                ExtractDimensions_C(parts, dimensions, cf)
            Case "RHS"
                ExtractDimensions_RECT(parts, dimensions, cf)
            Case "R"
                ExtractDimensions_RECT(parts, dimensions, cf)
            Case "T"
                ExtractDimensions_CH_T(parts, dimensions, cf)
            Case Else
                Return False
        End Select
        Return True

    End Function

    Function ExtractDimensions_I(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean
        Dim D As Double = 0.0,
            Wt As Double = 0.0,
            Wb As Double = 0.0,
            Tt As Double = 0.0,
            Tb As Double = 0.0,
            t As Double = 0.0

        Dim b1, b2, b3, b4, b5, b6 As Boolean
        Select Case parts(1)
            Case "GI"
                ' D Wt Wb t Tt Tb

                Debug.Assert(8 = parts.Length)
                b1 = Double.TryParse(parts(2), D)
                dimensions.Add("D", D * cf)

                b2 = Double.TryParse(parts(3), Wt)
                dimensions.Add("Wt", Wt * cf)

                b3 = Double.TryParse(parts(4), Wb)
                dimensions.Add("Wb", Wb * cf)

                b4 = Double.TryParse(parts(5), t)
                dimensions.Add("t", t * cf)

                b5 = Double.TryParse(parts(6), Tt)
                dimensions.Add("Tt", Tt * cf)

                b6 = Double.TryParse(parts(7), Tb)
                dimensions.Add("Tb", Tb * cf)

                Debug.Assert(b1 And b2 And b3 And b4 And b5 And b6)

            Case "I"
                ' D W t T
                'STD I(m) 0.9 0.4 2.E-002 3.E-002
                Debug.Assert(6 = parts.Length)
                b1 = Double.TryParse(parts(2), D)
                dimensions.Add("D", D * cf)

                b2 = Double.TryParse(parts(3), Wt)
                dimensions.Add("Wt", Wt * cf)
                dimensions.Add("Wb", Wt * cf)

                b3 = Double.TryParse(parts(4), t)
                dimensions.Add("t", t * cf)

                b4 = Double.TryParse(parts(5), Tt)
                dimensions.Add("Tt", Tt * cf)
                dimensions.Add("Tb", Tt * cf)

                Debug.Assert(b1 And b2 And b3 And b4)
        End Select
        Return True
    End Function
    Function ExtractDimensions_CH_T(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0,
            W As Double = 0.0,
            T As Double = 0.0,
            Tt As Double = 0.0
        'STD CH(m) 0.5 0.25 2.E-002 3.E-002
        'STD T(m) 0.5 0.25 3.E-002 2.E-002

        Dim b1, b2, b3, b4 As Boolean
        Debug.Assert(6 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), W)
        dimensions.Add("W", W * cf)

        b3 = Double.TryParse(parts(4), T)
        dimensions.Add("T", T * cf)

        b4 = Double.TryParse(parts(5), Tt)
        dimensions.Add("t", Tt * cf)

        Debug.Assert(b1 And b2 And b3 And b4)
        Return True

    End Function
    Function ExtractDimensions_CHS(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0,
            t As Double = 0.0
        'STD CHS(m) 0.25 1.E-002

        Dim b1, b2 As Boolean
        Debug.Assert(4 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), t)
        dimensions.Add("t", t * cf)

        Debug.Assert(b1 And b2)
        Return True

    End Function
    Function ExtractDimensions_C(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean
        Dim D As Double = 0.0

        Dim b1 As Boolean
        Debug.Assert(3 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        Debug.Assert(b1)
        Return True

    End Function
    Function ExtractDimensions_RECT(ByRef parts As String(), ByRef dimensions As SortedList, ByVal cf As Double) As Boolean

        Dim D As Double = 0.0,
            W As Double = 0.0,
            T As Double = 0.0,
            Tt As Double = 0.0
        'STD RHS(m) 0.25 0.3 3.E-002 3.E-002

        Dim b1, b2, b3, b4 As Boolean
        Debug.Assert(6 = parts.Length Or 4 = parts.Length)

        b1 = Double.TryParse(parts(2), D)
        dimensions.Add("D", D * cf)

        b2 = Double.TryParse(parts(3), W)
        dimensions.Add("W", W * cf)

        Dim sShape As String = parts(1)
        Dim shapeSubString As String() = sShape.Split(New [Char]() {"("c, ")"c})
        Debug.Assert(shapeSubString.Length > 0)

        If (String.Equals(shapeSubString(0), "RHS")) Then
            b3 = Double.TryParse(parts(4), T)
            dimensions.Add("T", T * cf)

            b4 = Double.TryParse(parts(5), Tt)
            dimensions.Add("t", Tt * cf)
        End If

        'Debug.Assert(b1 And b2 And b3 And b4)
        Return True

    End Function

    Function SetSid(ByVal keyword As String, ByVal record As Integer, ByVal sid As String) As Boolean
        Debug.Assert(Not String.IsNullOrEmpty(keyword) And Not String.IsNullOrEmpty(sid) And Not (0 = record))

        ' ensure record exists
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If 1 <> iCheck Then
            Return False
        End If
        m_GSAObject.WriteSidTagValue(keyword, record, "RVT", sid)
        Return True

    End Function

    ''' <summary>
    ''' Fetches value associated with the RVT key from the sid of the record
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSid(ByRef keyword As String, ByRef record As Integer) As String

        Debug.Assert(Not String.IsNullOrEmpty(keyword) AndAlso Not 0 = record)
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If iCheck <> 1 Then
            Return Nothing
        End If
        Return m_GSAObject.GetSidTagValue(keyword, record, "RVT")

    End Function
    Private Function GetSidMaterial(ByRef keyword As String, ByRef record As Integer) As String

        Debug.Assert(Not String.IsNullOrEmpty(keyword))
        Dim iCheck As Integer = CInt(m_GSAObject.GwaCommand("EXIST," & keyword & "," & record.ToString()))
        If iCheck <> 1 Then
            Return Nothing
        End If
        Return m_GSAObject.GetSidTagValue(keyword, record, "RVT")

    End Function

    Function ParseModelSid(ByRef sids As SortedList(Of String, String)) As Boolean
        'Dim oSid As Object = m_GSAObject.GwaCommand("GET,SID")
        Dim sid_string As String = m_GSAObject.GetSidTagValue("SID", 1, "RVT")
        If String.IsNullOrEmpty(sid_string) Then
            'Debug.Assert(False)
            Return False
        End If
        Return Me.ParseNestedSid(sid_string, sids)

    End Function
    'Function ParseSectionSid(ByVal iSec As Integer, ByRef sids As SortedList) As Boolean

    '    Dim sid_string As String = m_GSAObject.GetSidTagValue("SEC_BEAM", iSec, "RVT")
    '    If String.IsNullOrEmpty(sid_string) Then
    '        Return False
    '    End If
    '    Return Me.ParseNestedSid(sid_string, sids)

    'End Function
    Function ParseNestedSid(ByRef sid_string As String, ByRef sids As SortedList(Of String, String)) As Boolean
        'sid is of format {RVT:{key1:value1}{key2:value2}...}

        'sid = sid.Substring(5)
        'sid = sid.Remove(sid.Length - 1)
        'Dim params As New SortedList

        Dim parts As String() = sid_string.Split(New Char() {"{"c, "}"c}, System.StringSplitOptions.RemoveEmptyEntries)
        If parts Is Nothing Then
            Return False
        End If

        For Each s As String In parts
            Dim pair As String() = s.Split(New Char() {":"c})
            If pair.Length <> 2 Then
                Continue For
            End If
            sids(pair(0)) = pair(1)
        Next
        Return True

    End Function
    Sub SetModelSid(ByRef sid As String)
        m_GSAObject.GwaCommand("SID," & sid)
    End Sub
    ''' <summary>
    ''' Highest record number for a given module
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function HighestRecord(ByRef keyword As String) As Integer
        Dim command As String = "HIGHEST," + keyword
        Dim obj As Object = m_GSAObject.GwaCommand(command)
        Dim nRecord As Integer = CType(obj, Integer)
        Return nRecord
    End Function
    Function HighestGridPlane() As Integer
        Return Me.HighestRecord("GRID_PLANE")
    End Function
    Function HighestGridSurface() As Integer
        Return Me.HighestRecord("GRID_SURFACE")
    End Function
    Function HighestGridLine() As Integer
        Return Me.HighestRecord("GRID_LINE")
    End Function
    Function HighestNode() As Integer
        Return Me.HighestRecord("NODE")
    End Function
    Function HighestLine() As Integer
        Return Me.HighestRecord("LINE")
    End Function
    Function HighestArea() As Integer
        Return Me.HighestRecord("AREA")
    End Function
    Function HighestSection() As Integer
        Return Me.HighestRecord("PROP_SEC")
    End Function
    Function HighestProp2d() As Integer
        Return Me.HighestRecord("PROP_2D")
    End Function
    Function HighestMaterial() As Integer
        Return Me.HighestRecord("MAT_ANAL")
    End Function
    Function HighestEnt(ByRef ent As String) As Integer
        ent = ent.Replace("MEMBER", "MEMB")
        Return Me.HighestRecord(ent)
    End Function
    Function HighestList() As Integer
        Return Me.HighestRecord("LIST")
    End Function
    Function HighestRegion() As Integer
        Return Me.HighestRecord("REGION")
    End Function
    Function HighestPolyLine() As Integer
        Return Me.HighestRecord("POLYLINE")
    End Function
    Function HighestGridAreaLoad() As Integer
        Return Me.HighestRecord("LOAD_GRID_AREA")
    End Function
    Function HighestGridLineLoad() As Integer
        Return Me.HighestRecord("LOAD_GRID_LINE")
    End Function
    Function HighestGridPointLoad() As Integer
        Return Me.HighestRecord("GRID_POINT_LOAD")
    End Function
    Function HighestLoadCase() As Integer
        Return Me.HighestRecord("LOAD_TITLE")
    End Function


    ''' <summary>
    ''' Does record exist for keyword
    ''' </summary>
    ''' <param name="keyword"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function ModuleRecordExists(ByRef keyword As String, ByVal record As Integer) As Boolean
        Dim command As String = "EXIST," + keyword + "," + record.ToString()
        Dim obj As Object = m_GSAObject.GwaCommand(command)
        Dim bExist As Boolean = False
        Dim iResult As Integer = CType(obj, Integer)
        If 0 = iResult Then
            Return False
        Else
            Return True
        End If

    End Function
    Function RegionExists(ByVal iRegion As Integer) As Boolean
        Return Me.ModuleRecordExists("REGION", iRegion)
    End Function
    Function ConnectionExist(ByVal iConnection As Integer) As Boolean
        Return Me.ModuleRecordExists("MEMB_END", iConnection)
    End Function
    Function SectionExists(ByVal iSec As Integer, Optional ByVal bFresh As Boolean = False) As Boolean
        If bFresh Then
            Return Me.ModuleRecordExists("PROP_SEC", iSec)
        Else
            If Not _SecListExist.ContainsKey(iSec) Then
                _SecListExist.Add(iSec, Me.ModuleRecordExists("PROP_SEC", iSec))
            End If
            Return _SecListExist.Item(iSec)
        End If
    End Function
    Function Section2DExists(ByVal iSec As Integer) As Boolean
        Return Me.ModuleRecordExists("PROP_2D", iSec)
    End Function
    Function ClearSectionExistsInfo() As Boolean
        _SecListExist.Clear()
        _SecList.Clear()
        Return True
    End Function
    Function MaterialExists(ByVal iMat As Integer) As Boolean
        Return Me.ModuleRecordExists("MAT_ANAL", iMat)
    End Function
    Function GridPlaneExists(ByVal iGridPl As Integer) As Boolean
        Return Me.ModuleRecordExists("GRID_PLANE", iGridPl)
    End Function
    Function GridLineExists(ByVal iGridLine As Integer) As Boolean
        Return Me.ModuleRecordExists("GRID_LINE", iGridLine)
    End Function
    Function LineExists(ByRef iLine As Integer) As Boolean
        If Not _LineListExist.ContainsKey(iLine) Then
            _LineListExist.Add(iLine, Me.ModuleRecordExists("LINE", iLine))
        End If
        Return _LineListExist.Item(iLine)
    End Function
    Function AreaExists(ByRef iArea As Integer) As Boolean
        Return Me.ModuleRecordExists("AREA", iArea)
    End Function
    Function EntExists(ByRef ent As String, ByVal iEnt As Integer) As Boolean
        If (ent.Equals("EL")) Then
            If Not _EleListExist.ContainsKey(iEnt) Then
                _EleListExist.Add(iEnt, Me.ModuleRecordExists(ent, iEnt))
            End If
            Return _EleListExist.Item(iEnt)
        Else
            If Not _MemListExist.ContainsKey(iEnt) Then
                _MemListExist.Add(iEnt, Me.ModuleRecordExists(ent, iEnt))
            End If
            Return _MemListExist.Item(iEnt)
        End If

    End Function
    Function ListExists(ByRef iList As Integer) As Boolean
        Return Me.ModuleRecordExists("LIST", iList)
    End Function

    ''' <summary>
    ''' gets revit id for the material record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function MaterialSid(ByRef record As Integer) As String
        Return GetSidMaterial("MAT_ANAL", record)
    End Function
    ''' <summary>
    ''' gets revit id for the element record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function ElemSid(ByRef record As Integer) As String
        Return GetSid("EL", record)
    End Function
    ''' <summary>
    ''' gets revit id for the grid plane record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function GridPlaneSid(ByRef record As Integer) As String
        Return GetSid("GRID_PLANE", record)
    End Function
    ''' <summary>
    ''' gets revit id for the grid line record
    ''' </summary>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks>No need to extract RevitID again when using this</remarks>
    Function GridLineSid(ByRef record As Integer) As String
        Return GetSid("GRID_LINE", record)
    End Function
    ''' <summary>
    ''' gets revit id for a member or element
    ''' </summary>
    ''' <param name="entity"></param>
    ''' <param name="record"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function EntSid(ByRef entity As String, ByRef record As Integer) As String
        Return GetSid(entity, record)
    End Function
    Function SectionSid(ByVal record As Integer) As SortedList(Of String, String)
        Dim sid As String = GetSid("PROP_SEC", record)
        Dim sidsMap As New SortedList(Of String, String)(2)
        If ParseNestedSid(sid, sidsMap) Then
            Return sidsMap
        Else
            Debug.Assert(False, "sid parsing failed for section " + record.ToString())
            Return Nothing
        End If
    End Function
    Function ElemIsSection(ByVal iElem As Integer) As Boolean
        Dim sGwaCommand As String = "GET,EL," + iElem.ToString()
        sGwaCommand = CStr(m_GSAObject.GwaCommand(sGwaCommand))
        Dim sArg As String = GsaComUtil.Arg(4, sGwaCommand)
        Dim eType As ElemType = Me.ElemTypeFromString(sArg)

        If ElemTypeIsBeamOrTruss(eType) Then
            Return True
        Else
            Return False
        End If
    End Function
    Function EntSection(ByRef entity As String, ByVal iEnt As Integer) As Integer
        Debug.Assert(String.Equals(entity, "EL") Or String.Equals(entity, "MEMB"))
        If Not Me.EntExists(entity, iEnt) Then
            Debug.Assert(False)
            Return 0
        End If

        Dim sName As String = "", sTopo As String = "", iProp As Integer = 0
        Dim type As MembType = MembType.UNDEF, Mat As MembMat = MembMat.UNDEF
        Dim eMembtype As MembType = MembType.UNDEF
        Dim eMembMattype As MembMat = MembMat.UNDEF
        Dim uid As String = "", strDummy As String = ""
        Dim iOrNode As Integer = 0, dBeta As Double = 0.0,
        iTopoList As List(Of Integer) = Nothing,
        dOffset As List(Of Double()) = Nothing,
        release As List(Of String) = Nothing
        Dim eleType As ElemType = ElemType.EL_BAR

        If String.Equals(entity, "EL") Then
            Me.Elem1d(iEnt, iProp, uid, iTopoList, iOrNode, dBeta, release, dOffset, eleType, eMembtype, strDummy)
        Else
            Me.Member(iEnt, sName, iProp, uid, sTopo, dBeta, release, dOffset, eMembtype)
        End If

        Return iProp
    End Function
    Private Shared Function ElemTypeIsBeamOrTruss(ByVal eType As ElemType) As Boolean
        If (ElemType.EL_BEAM = eType _
            Or ElemType.EL_BAR = eType _
            Or ElemType.EL_STRUT = eType _
            Or ElemType.EL_TIE = eType) Then
            Return True
        Else
            Return False
        End If
    End Function
    ''' <summary>
    ''' returns sid from string {RVT:sid}
    ''' </summary>
    ''' <param name="sArg"></param>
    ''' <returns></returns>
    ''' <remarks>sid can be of form {tag1:{subtag1:data}{subtag2:data}}{tag2:data}</remarks>
    Public Shared Function ExtractId(ByVal sArg As String) As String

        Dim tag As String = "RVT:"
        Dim value As String = ""
        If Not sArg.Contains(tag) Then
            'Debug.Assert(False)
            Return value
        End If

        Dim pos_tag As Integer = sArg.IndexOf(tag)
        ' sid will be of form {tag1:{subtag1:data}{subtag2:data}}{tag2:data}
        Dim pos_value As Integer = pos_tag + tag.Length
        Dim nBraces As Integer = 1
        Dim sOpn As String = "{", sCls As String = "}"
        'Debug.Assert(Char.Equals(sArg.Chars(pos_value), sOpn.Chars(0)))

        Dim i As Integer = 0
        While nBraces > 0
            If Char.Equals(sArg.Chars(pos_value + i), sOpn.Chars(0)) Then
                nBraces += 1
            End If
            If Char.Equals(sArg.Chars(pos_value + i), sCls.Chars(0)) Then
                nBraces -= 1
            End If
            i += 1
        End While
        If sArg.Length < pos_value + i Then
            Debug.Assert(False) ' something's wrong
            Return value
        End If
        value = sArg.Substring(pos_value, i - 1)
        Return value
    End Function
    Public Property SelType() As EntType
        Get
            Return m_eSelType
        End Get
        Set(ByVal value As EntType)
            m_eSelType = value
        End Set
    End Property

End Class
