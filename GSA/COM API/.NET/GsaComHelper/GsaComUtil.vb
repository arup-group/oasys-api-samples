'
' © 2008 Oasys Ltd.
'
Imports System.IO
Imports System.Math
Imports System.Reflection
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Text.RegularExpressions
Public Class GsaComUtil

    Const RoundPrecision As Integer = 6 'precision for rounding real numbers
    Const SectionSid_Usage As String = "usage"
    Const SectionSid_Symbol As String = "symbol"
    Private _MemList As Dictionary(Of Integer, MemberElement)
    Private _EleList As Dictionary(Of Integer, MemberElement)
    Private _EleListExist As Dictionary(Of Integer, Boolean)
    Private _MemListExist As Dictionary(Of Integer, Boolean)
    Private _IsHoz As Dictionary(Of Integer, Boolean)
    Private _IsVet As Dictionary(Of Integer, Boolean)
    Private _SecList As Dictionary(Of Integer, GSASection)
    Private _SecListExist As Dictionary(Of Integer, Boolean)
    Private _Connection As Dictionary(Of Integer, String)
    Enum RefPt
        CENTROID = 0
        TOP_LEFT
        TOP_CENTRE
        TOP_RIGHT
        MIDDLE_LEFT
        DUMMY
        MIDDLE_RIGHT
        BOTTOM_LEFT
        BOTTOM_CENTRE
        BOTTOM_RIGHT
    End Enum
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
        UNDEF = -1
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
        UNDEF = -1
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
        SLAB = 4
        WALL = 5
        CANTILEVER = 6
        RIBSLAB = 7
        COMPOS = 8
        PILE = 9
        EXPLICIT = 10
        VOID_CUTTER_1D = 11
        VOID_CUTTER_2D = 12
        GENERIC_3D = 13
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
    Public Function Type2DFromStr(ByVal mType As String) As Type2D
        mType = mType.ToUpper()
        Select Case mType
            Case "UNDEF"
                Return Type2D.UNDEF
            Case "PL_STRESS"
                Return Type2D.PL_STRESS
            Case "PL_STRAIN"
                Return Type2D.PL_STRAIN
            Case "AXISYMMETRIC"
                Return Type2D.AXISYMMETRIC
            Case "FABRIC"
                Return Type2D.FABRIC
            Case "PLATE"
                Return Type2D.PLATE
            Case "SHELL"
                Return Type2D.SHELL
            Case "CURVED_SHELL"
                Return Type2D.CURVED_SHELL
            Case "TORSION"
                Return Type2D.TORSION
            Case "WALL"
                Return Type2D.WALL
            Case "LOAD"
                Return Type2D.LOAD
            Case Else
                Return Type2D.UNDEF
        End Select
    End Function
    Public Function MembTypeStr(ByVal mType As MembType) As String
        Return mType.ToString()
    End Function
    Public Function MembTypeFromStr(ByVal mType As String) As MembType
        mType = mType.ToUpper()
        Select Case mType
            Case "UNDEF"
                Return MembType.UNDEF
            Case "GENERIC_1D"
            Case "1D_GENERIC"
                Return MembType.GENERIC_1D
            Case "GENERIC_2D"
            Case "2D_GENERIC"
                Return MembType.GENERIC_2D
            Case "GENERIC_3D"
                Return MembType.GENERIC_3D
            Case "BEAM"
                Return MembType.BEAM
            Case "COLUMN"
                Return MembType.COLUMN
            Case "SLAB"
                Return MembType.SLAB
            Case "WALL"
                Return MembType.WALL
            Case "CANTILEVER"
                Return MembType.CANTILEVER
            Case "RIBSLAB"
                Return MembType.RIBSLAB
            Case "COMPOS"
                Return MembType.COMPOS
            Case "PILE"
                Return MembType.PILE
            Case "EXPLICIT"
                Return MembType.EXPLICIT
            Case "VOID_CUTTER_1D"
                Return MembType.VOID_CUTTER_1D
            Case "VOID_CUTTER_2D"
                Return MembType.VOID_CUTTER_2D
            Case "VOID_CUTTER_1D"
                Return MembType.GENERIC_3D
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

    Public Structure ToMilliMeter
        public Shared FromMeter As Double = 1000.0
        public Shared FromFeet As Double = 304.8
        public Shared FromInch As Double = 25.4
        public Shared FromCentimeter As Double = 10.0
    End Structure

    'GSA object
    Private m_GSAObject As ComAuto
    Private m_eSelType As EntType
    Private m_eUnit As GsRevit_Units ' Units of the REVIT MODEL!! Careful
    Public m_cfactor As Double = 1
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
    Public Function RevitFamilyToSection(ByVal familyType As String, ByVal revitFamily As String, ByVal usage As SectionUsage) As String
        Dim gsrevit_usage As GsRevit_Usage = GsRevit_Usage.FRAMING
        If SectionUsage.COLUMNS = usage Then
            gsrevit_usage = GsRevit_Usage.COLUMNS
        End If
        Return m_GSAObject.Gen_SectTransltnGsRevit(familyType, GsRevit_SectTrnsDir.REVIT_TO_GSA, gsrevit_usage, revitFamily)
    End Function

    Public Function IsDescValid(ByVal sDes As String) As Boolean
        If String.IsNullOrEmpty(sDes) Then
            Return False
        End If
        Dim strOut As String = m_GSAObject.Gen_SectionMatchDesc(sDes, SectionMatch_Flags.BOTH, False)
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
            'i.e. 5.0 should be same as 5
            Dim newGsaDesc As String = gsaDesc
            If Not newGsaDesc.Contains(".0") Then
                newGsaDesc = newGsaDesc + ".0"
            Else
                newGsaDesc = newGsaDesc.Replace(".0", "")
            End If
            familyType = m_GSAObject.Gen_SectTransltnGsRevit(newGsaDesc, GsRevit_SectTrnsDir.GSA_TO_REVIT, gsrevit_usage, familyName)
            If Not String.IsNullOrEmpty(familyType) And Not String.IsNullOrEmpty(familyName) Then
                bFamilyTypeFound = True
                Return familyType
            End If
            familyType = TrySNFamilies(gsaDesc, usage, bFamilyTypeFound, familyName)
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
        dData(0) = Math.Round(dData(0) * m_cfactor, 4)
        dData(1) = Math.Round(dData(1) * m_cfactor, 4)
        dData(2) = Math.Round(dData(2) * m_cfactor, 4)
    End Sub


    Public Sub ChangeUnits(ByRef dData As Double)
        dData = Math.Round(dData * m_cfactor, 4)
    End Sub
    Public Sub ExtractNodeCoor(ByVal iNode As Integer, ByRef x As Double, ByRef y As Double, ByRef z As Double)

        Dim SIToFeet As Double = 3.28084
        'initialize the coords to 0 first
        x = 0
        y = 0
        z = 0

        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + iNode.ToString()
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, x, y, z)
        End If
        'change SI unit to user unit as NodeCord function return in SI unit
        x = x * SIToFeet / m_cfactor
        y = y * SIToFeet / m_cfactor
        z = z * SIToFeet / m_cfactor

    End Sub
    Public Function ExtractNodeCoor(ByVal iNode As Integer) As Double()

        Dim SIToFeet As Double = 3.28084
        'initialize the coords to 0 first
        Dim dbNoderCord() As Double = {0.0, 0.0, 0.0}
        Dim check As Object
        Dim gwaCommand As String
        gwaCommand = "EXIST,NODE," + iNode.ToString()
        check = m_GSAObject.GwaCommand(gwaCommand)
        Dim iCheck As Integer = CType(check, Integer)
        If 1 = iCheck Then
            m_GSAObject.NodeCoor(iNode, dbNoderCord(0), dbNoderCord(1), dbNoderCord(2))
        End If
        'change SI unit to user unit as NodeCord function return in SI unit
        dbNoderCord(0) = dbNoderCord(0) * SIToFeet / m_cfactor
        dbNoderCord(1) = dbNoderCord(1) * SIToFeet / m_cfactor
        dbNoderCord(2) = dbNoderCord(2) * SIToFeet / m_cfactor
        Return dbNoderCord
    End Function
    Public Shared Function Arg(ByVal pos As Integer, ByVal source As String) As String
        Dim strList As New List(Of String)
        Dim strArray As String() = source.Split(New [Char]() {","c})
        Dim bQuoteStart As Boolean = False
        Dim bQuoteEnd As Boolean = False
        Dim QuoteRecord As String = ""
        Dim index As Integer = 0
        For Each value As String In strArray
            value = value.Trim()
            If (bQuoteStart) Then
                bQuoteEnd = value.EndsWith(ControlChars.Quote)
            End If
            If (Not bQuoteStart) Then
                QuoteRecord = ""
                bQuoteStart = value.StartsWith(ControlChars.Quote)
            End If

            If bQuoteStart OrElse bQuoteEnd Then
                If (index > 0) Then
                    QuoteRecord = QuoteRecord + ","
                End If
                QuoteRecord = QuoteRecord + value
                index = index + 1
            Else
                QuoteRecord = value
            End If

            If bQuoteEnd Then
                bQuoteStart = False
            End If

            If Not bQuoteStart Then
                strList.Add(QuoteRecord)
            End If
        Next
        If strList.Count > pos Then
            Return CType(strList(pos), String)
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
        m_cfactor = 3.28084 / m_cfactor
        ' A factor which can convert the values to feet - This is what the revit API expects.
    End Sub
    'write a GSA Grid Plane
    Public Function SetGridPlane(ByVal iGrid As Integer, ByVal sName As String, ByVal sid As String, ByVal iAxis As Integer, ByVal dElev As Double, ByVal dTol As Double, Optional ByVal type As GsaComUtil.GridPlaneType = GsaComUtil.GridPlaneType.STOREY) As Integer
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
        If (iGrid <= 0) Then
            iGrid = Me.HighestGridPlane() + 1
        End If
        'round
        dElev = Math.Round(dElev, RoundPrecision)
        'write grid plane
        sGwaCommand = "GRID_PLANE:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," + iGrid.ToString()       'number
        sGwaCommand += "," & sName                  'name
        sGwaCommand += "," + type.ToString()        'type [GENERAL :: general grid plane] or [STOREY	:: storey]
        sGwaCommand += "," + iAxis.ToString()       'axis
        sGwaCommand += "," + dElev.ToString()       'elevation 
        sGwaCommand += "," + dTol.ToString()        'above tolerance 
        sGwaCommand += "," + dTol.ToString()        'below tolerance 
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iGrid
    End Function
    
    Public Function GridPlane(ByVal iNum As Integer, ByRef sName As String, ByRef uid As String, ByRef dElev As Double) As Boolean
        Dim type As GridPlaneType = GridPlaneType.STOREY
        Dim iAxis As Integer = 0
        Dim dTol As Double = 0.0
        Return Me.GridPlane(iNum, sName, uid, iAxis, dElev, type, dTol)
    End Function
    'read a GSA Grid Plane
    Public Function GridPlane(ByVal iGrid As Integer, ByRef sName As String, ByRef uid As String, ByRef iAxis As Integer, ByRef dElev As Double, ByRef eType As GsaComUtil.GridPlaneType, ByRef dTol As Double) As Boolean

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
        Try
            If Not Me.GridPlaneExists(iGrid) Then
                Return False
            End If
            Dim sGwaCommand As String = ""
            Dim sArg As String
            sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,GRID_PLANE," & iGrid.ToString))
            If String.IsNullOrEmpty(sGwaCommand) Then
                Return False
            End If
            sArg = GsaComUtil.Arg(0, sGwaCommand)
            Dim idString As String = GsaComUtil.ExtractId(sArg)
            If Not String.IsNullOrEmpty(idString) Then
                uid = idString
            End If
            sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
            sArg = GsaComUtil.Arg(2, sGwaCommand) 'name
            sName = sArg
            sArg = GsaComUtil.Arg(3, sGwaCommand) 'type
            If (sArg.ToLower().Equals("storey")) Then
                eType = GridPlaneType.STOREY
            Else
                eType = GridPlaneType.GENERAL
            End If
            sArg = GsaComUtil.Arg(4, sGwaCommand) 'axis
            iAxis = CInt(sArg)

            sArg = GsaComUtil.Arg(5, sGwaCommand) 'elevation [feet]
            dElev = Val(sArg)
            sArg = GsaComUtil.Arg(6, sGwaCommand) 'elements
            dTol = Val(sArg)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    'write GSA grid line
    Public Function SetGridLine(ByVal iGrLine As Integer, ByVal sName As String, ByVal bArc As Boolean, ByVal sid As String,
                                   ByVal coorX As Double, ByVal coorY As Double, ByVal length As Double,
                                   Optional ByVal theta1 As Double = 0.0, Optional ByVal theta2 As Double = 0.0) As Integer

        Dim sGwaCommand As String = ""
        If (iGrLine <= 0) Then
            iGrLine = Me.HighestGridLine() + 1
        End If
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
        sGwaCommand = "GRID_LINE:"
        sGwaCommand += "{RVT:" & sid & "}"
        sGwaCommand += "," + iGrLine.ToString() + ","
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
        Return iGrLine
    End Function
    Public Function GridLine(ByVal iGrLine As Integer, ByRef name As String, ByRef bArc As Boolean, ByRef sid As String,
                                    ByRef coorX As Double, ByRef coorY As Double, ByRef len As Double,
                                    ByRef theta1 As Double, ByRef theta2 As Double) As Boolean


        'GRID_LINE | num | name | arc | coor_x | coor_y | length | theta1 | theta2
        Try
            If Not Me.GridLineExists(iGrLine) Then
                Return False
            End If

            Dim result As String = CStr(m_GSAObject.GwaCommand("GET,GRID_LINE," & iGrLine.ToString()))
            Dim arg As String = GsaComUtil.Arg(0, result)
            Dim idString As String = GsaComUtil.ExtractId(arg)
            If Not String.IsNullOrEmpty(idString) Then
                sid = idString
            End If

            arg = GsaComUtil.Arg(1, result)
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

        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    'write a GSA 1D element
    Public Function IsPolyLineExist(ByVal Desc As String, ByRef iPolyLine As Integer) As Boolean
        If String.IsNullOrEmpty(Desc.Trim()) Then
            Return False
        End If
        Dim uid As String = "", name As String = "", _desc As String = ""
        For iPoly As Integer = 0 To HighestPolyLine() - 1
            If PolyLine(iPoly, uid, name, _desc) Then
                If _desc.Trim() = Desc.Trim() Then
                    iPolyLine = iPoly
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function PolyLine(ByVal iPoly As Integer, ByRef sID As String, ByRef sName As String, ByRef sDec As String) As Boolean

        Dim sGwaCommand As String = "", sResult As Integer = 0
        sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,POLYLINE," & iPoly.ToString))
        Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
        Dim idString As String = GsaComUtil.ExtractId(sArg)
        If Not String.IsNullOrEmpty(idString) Then
            sID = idString
        End If
        sName = GsaComUtil.Arg(2, sGwaCommand)

        sDec = GsaComUtil.Arg(6, sGwaCommand)

        Return True
    End Function
    Public Function SetPolyLine(ByVal iPoly As Integer, ByVal sID As String, ByVal sName As String, ByVal sDec As String) As Integer
        If (iPoly <= 0) Then
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
        sGwaCommand += "," + sDec                         'point description

        m_GSAObject.GwaCommand(sGwaCommand)
        Return iPoly
    End Function
    Public Function SetGridAreaLoad(ByVal iGridAreaLoad As Integer, ByVal sID As String, ByVal sName As String, ByVal iGrid As Integer, ByVal sPolyDesc As String, ByVal iDir As String, ByVal LoadValue1 As Double, ByVal LoadValue2 As Double, ByVal loadCase As Integer, ByVal bProjected As Boolean, ByVal bGlobal As Boolean, ByVal loadtype As GridLoadType) As Integer
        'LOAD_GRID_AREA.2 | name | grid_surface | area | poly | case | axis | proj | dir | value @end
        'LOAD_GRID_LINE.2 | name | grid_surface | line | poly | case | axis | proj | dir | value_1 | value_2 @end
        'LOAD_GRID_POINT.2 | name | grid_surface | x | y | case | axis | dir | value @end

        If (iGridAreaLoad <= 0) Then
            If loadtype = GridLoadType.AREA Then
                iGridAreaLoad = Me.HighestGridAreaLoad() + 1
            ElseIf loadtype = GridLoadType.LINE Then
                iGridAreaLoad = Me.HighestGridLineLoad() + 1
            Else
                iGridAreaLoad = Me.HighestGridPointLoad() + 1
            End If
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
        sGwaCommand += "," + iGrid.ToString()             'grid surface
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
    Public Function IsGridAreaLoadExistInDirection(ByVal sidIn As String, ByVal sDirIn As String, ByRef AreaLoadRec As Integer, ByVal loadtype As GridLoadType) As Boolean
        For iGridArea As Integer = 0 To HighestGridAreaLoad() - 1
            Dim sID As String = "", sName As String = "", iGrid As Integer = 0, iPolyRef As String = ""
            Dim LoadCase As Integer = 0
            Dim sDir As String = "", LoadValue1 As Double = 0, LoadValue2 As Double = 0, bProjected As Boolean = False, bGlobal As Boolean = False
            If GetGridAreaLoad(iGridArea, sID, sName, iGrid, iPolyRef, LoadCase, sDir, LoadValue1, LoadValue2, bProjected, bGlobal, loadtype) Then
                If sidIn.Trim() = sID.Trim() AndAlso sDirIn.ToLower().Trim() = sDir.ToLower().Trim() Then
                    AreaLoadRec = iGridArea
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function GetGridAreaLoad(ByVal iGridAreaLoad As Integer, ByRef sID As String, ByRef sName As String, ByRef iGrid As Integer, ByRef sPolyDesc As String, ByRef LoadCase As Integer, ByRef sDir As String, ByRef LoadValue1 As Double, ByRef LoadValue2 As Double, ByRef bProjected As Boolean, ByRef bGlobal As Boolean, ByVal loadtype As GridLoadType) As Boolean
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
        sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
        If Not Integer.TryParse(sArg, LoadCase) Then
            Return False
        End If

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
            iNextIndex = iNextIndex + 1
            sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand)
            Dim fLoadValue As Double = 0
            If Double.TryParse(sArg, fLoadValue) Then
                LoadValue1 = fLoadValue
            Else
                Return False
            End If
        End If
        Return True
    End Function
    Public Function IsGridAssociatedWithGridSurface(ByVal iGrid As Integer, ByRef iGridSurf As Integer) As Boolean
        Dim uid As String = "", name As String = "", grid As Integer = -1, elem2d As Boolean = False, bSpan2d As Boolean = False
        For iSurf As Integer = 0 To HighestGridSurface() - 1
            If GridSurface(iSurf + 1, uid, name, grid, elem2d, bSpan2d) Then
                If grid = iGrid Then
                    iGridSurf = iSurf + 1
                    Return True
                End If
            End If
        Next
        Return False
    End Function
    Public Function SetGridSurface(ByVal iGridSurf As Integer, ByVal uid As String, ByVal name As String, ByVal iGrid As Integer, ByVal elem2d As Boolean, ByVal bSpan2d As Boolean) As Integer

        If (iGridSurf <= 0) Then
            iGridSurf = Me.HighestGridSurface() + 1
        End If
        If String.IsNullOrEmpty(name) Then
            name = "Grid Surface " + iGridSurf.ToString()
        End If
        Dim sGwaCommand As String = ""
        sGwaCommand = "GRID_SURFACE:"
        sGwaCommand += "{RVT:" & uid & "}"
        sGwaCommand += "," + iGridSurf.ToString()        'grid id
        sGwaCommand += "," + name                        'name
        sGwaCommand += "," + iGrid.ToString()            'grid plane
        If elem2d Then
            sGwaCommand += ",2"                          'area type
        Else
            sGwaCommand += ",1"                          'area type
        End If
        sGwaCommand += ",all"                            'element list
        sGwaCommand += ",0.001"                          'tolerance
        If bSpan2d Then
            sGwaCommand += ",TWO_GENERAL"                 'span direction
        Else
            sGwaCommand += ",ONE"                         'span direction
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

        If (iLoadCase <= 0) Then
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
            name = GsaComUtil.Arg(2, sGwaCommand)
            If String.IsNullOrEmpty(name.Trim()) Then
                Return False
            End If
            nature = GsaComUtil.Arg(3, sGwaCommand)
        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function
    Public Function GridSurface(ByVal iGridSurf As Integer, ByRef uid As String, ByRef name As String, ByRef iGrid As Integer, ByRef elem2d As Boolean, ByRef bSpan2d As Boolean) As Boolean
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

            name = GsaComUtil.Arg(2, sGwaCommand)

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

    Public Function ElementCord(ByVal iElem As Integer) As List(Of Integer)
        Dim sName As String = "", iProp As Integer = 0, uid As String = "", sTopo As String = "", dBeta As Double = 0
        Dim sRelease As New List(Of String), dOffset As New List(Of Double()), typeMemb As MembType = MembType.BEAM
        Dim eType As ElemType = ElemType.EL_BEAM
        Dim iTopo As New List(Of Integer), iOrNode As Integer = 0
        Elem1d(iElem, sName, iProp, uid, iTopo, iOrNode, dBeta, sRelease, dOffset, eType, "")
        Return iTopo
    End Function
    Public Function MembCord(ByVal iElem As Integer) As String
        Dim sName As String = "", iProp As Integer = 0, uid As String = "", sTopo As String = "", dBeta As Double = 0
        Dim sRelease As New List(Of String), dOffset As New List(Of Double()), typeMemb As MembType = MembType.BEAM
        Dim eType As ElemType = ElemType.EL_BEAM
        Dim endRestraint1 As String = ""
        Dim endRestraint2 As String = ""
        Dim iOrNode As Integer = 0
        Member(iElem, sName, iProp, uid, sTopo, iOrNode, dBeta, sRelease, dOffset, typeMemb, endRestraint1, endRestraint2)
        Return sTopo
    End Function
    Public Function RestraintType(ByVal release As String) As String
        'A pin joint is a connection between two objects that allows only relative
        'rotation about a single axis. All translations as well as rotations about
        'any other axis are prevented
        release = release.ToUpper()
        If (release.Equals("FFFFRR")) Then
            Return "Pinned"
        ElseIf (release.Equals("RRRRRR")) Then
            Return "Free"
        Else
            Return "Fixed"
        End If
    End Function
    'write a GSA 1D element
    Public Function SetMember(ByVal iElem As Integer, ByVal sName As String, ByVal iProp As Integer,
                                ByVal sID As String, ByVal sTopo As String, ByVal iOrNode As Integer,
                                ByVal dBeta As Double, ByVal sRelease As List(Of String),
                                ByVal dOffset As List(Of Double()), ByVal typeMemb As MembType,
                                ByVal endRestraint1 As String, ByVal endRestraint2 As String) As Integer

        If (iElem <= 0) Then
            iElem = Me.HighestEnt("MEMB") + 1
        End If
        'MEMB0.8 | num | name | colour | type (1D) | prop | group | topology | node | angle |  @end
        'mesh_size | is_intersector | analysis_type | fire | limit | time[4] | dummy | @End
        'is_rls { | rls { | k } } | restraint_end_1 | restraint_end_2 | AUTOMATIC | height | load_ref | @end
        'off_auto_x1 | off_auto_x2 | off_auto_internal | off_x1 | off_x2 | off_y | off_z | exposure@End
        'MEMB0.8 | num | name | colour | type (1D) | prop | group | topology | node | angle | @end
        'mesh_size | is_intersector | analysis_type | fire | time[4] | dummy | @End
        'is_rls { | rls { | k } } | restraint_end_1 | restraint_end_2 | EXPLICIT | nump | { point | rest | } | nums | { span | rest | } @end 
        'height | load_ref | stud_layout | off_auto_x1 | off_auto_x2 | off_auto_internal | off_x1 | off_x2 | off_y | off_z | exposure @End
        'MEMB0.8 | num | name | colour | type (2D) | prop | group | topology | node | angle |  @end
        'mesh_size | is_intersector | analysis_type | fire | time[4] | dummy | @End
        'off_auto_internal | off_z | exposure

        Dim b1D As Boolean = Not Is2D(typeMemb)
        dBeta = Math.Round(dBeta, RoundPrecision)
        'Write beam element
        Dim sGwaCommand As String = ""
        sGwaCommand = "MEMB.8:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()                 'number
        sGwaCommand += "," + sName                            'name
        sGwaCommand += ",NO_RGB"                              'colour
        sGwaCommand += "," + MembTypeStr(typeMemb)            'member type
        sGwaCommand += "," + ""                               'exposed face
        sGwaCommand += "," + iProp.ToString()                 'section
        sGwaCommand += ",1"                                   'group
        sGwaCommand += "," + sTopo                            'number of topo
        sGwaCommand += "," + iOrNode.ToString()                'orientation node
        sGwaCommand += "," + dBeta.ToString()                 'orientation angle
        sGwaCommand += ",1,NO"                                'mesh size and mesh option + Intersector
        If b1D Then
            sGwaCommand += "," + "BEAM"                       'Analysis type ID
        Else
            sGwaCommand += "," + "LINEAR"                     'Analysis type 2D
        End If
        sGwaCommand += "," + "0,0.0,0,0,0,0"                  'fire resistance, temperature, stages
        sGwaCommand += ",ACTIVE"                              'member status
        If b1D Then
            sGwaCommand += "," + sRelease(0)                  'start release
            sGwaCommand += "," + sRelease(1)                  'end release
            If (String.IsNullOrEmpty(endRestraint1)) Then
                sGwaCommand += "," + RestraintType(sRelease(0))    'fixity at end1
            Else
                sGwaCommand += "," + endRestraint1                'fixity at end1
            End If

            If (String.IsNullOrEmpty(endRestraint2)) Then
                sGwaCommand += "," + RestraintType(sRelease(1))    'fixity at end1
            Else
                sGwaCommand += "," + endRestraint2                'fixity at end2
            End If

            sGwaCommand += ",AUTOMATIC"                        'Automatic(0)
            sGwaCommand += ",0"                                'load height
            sGwaCommand += "," + CType(LoadPos.SHR_CENTRE, Integer).ToString() 'reference point

            sGwaCommand += ",YES"                               'offset flag
            sGwaCommand += ",MAN"                               'manual offset
            sGwaCommand += ",MAN"                               'manual offset

            sGwaCommand += "," + dOffset.Item(0)(0).ToString()  'axial offset at start
            sGwaCommand += "," + dOffset.Item(1)(0).ToString()  'axial offset at end
            sGwaCommand += "," + dOffset.Item(0)(1).ToString()  'transaverse offset
            sGwaCommand += "," + dOffset.Item(0)(2).ToString()  'transverse offset
        Else
            sGwaCommand += "," + dOffset.Item(0)(2).ToString()  'axial offset at start
            sGwaCommand += ",NO"                                'auto internal offset 
        End If
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iElem
    End Function
    Public Function Member(ByVal iElem As Integer, ByRef sName As String, ByRef iProp As Integer,
                                ByRef uid As String, ByRef sTopo As String, ByRef iOrNode As Integer, ByRef dBeta As Double,
                                ByRef sRelease As List(Of String), ByRef dOffset As List(Of Double()), ByRef eMembType As GsaComUtil.MembType, ByRef endRestraint1 As String, ByRef endRestraint2 As String) As Boolean

        If _MemList.ContainsKey(iElem) Then

            Dim nEle As MemberElement = _MemList(iElem)
            iElem = nEle.Element
            sName = nEle.Name
            iProp = nEle.SecPro
            uid = nEle.UID
            sTopo = nEle.sTopo
            sRelease = nEle.Release
            iOrNode = nEle.OrientationNode
            dBeta = nEle.Beta
            dOffset = nEle.Offset
            eMembType = nEle.MemberType
            endRestraint1 = nEle.Endrestraint1
            endRestraint2 = nEle.Endrestraint2
        Else
            Try
                If Not Me.EntExists("MEMB", iElem) Then
                    Return False
                End If
                uid = ""
                dBeta = 0.0
                dOffset = New List(Of Double())
                sRelease = New List(Of String)
                Dim sGwaCommand As String = ""
                Dim sArg As String = ""
                sGwaCommand = CStr(m_GSAObject.GwaCommand("GET,MEMB," & iElem.ToString))
                If String.IsNullOrEmpty(sGwaCommand) Then
                    Return False
                End If

                'MEMB.8 | num | name | colour | type (1D) | prop | group | topology | node | angle |  @end
                'mesh_size | is_intersector | analysis_type | fire | limit | time[4] | dummy | @End
                'is_rls { | rls { | k } } | restraint_end_1 | restraint_end_2 | AUTOMATIC | height | load_ref | @end
                'off_auto_x1 | off_auto_x2 | off_auto_internal | off_x1 | off_x2 | off_y | off_z | exposure@End
                'MEMB0.8 | num | name | colour | type (1D) | prop | group | topology | node | angle | @end
                'mesh_size | is_intersector | analysis_type | fire | time[4] | dummy | @End
                'is_rls { | rls { | k } } | restraint_end_1 | restraint_end_2 | EXPLICIT | nump | { point | rest | } | nums | { span | rest | } @end 
                'height | load_ref | stud_layout | off_auto_x1 | off_auto_x2 | off_auto_internal | off_x1 | off_x2 | off_y | off_z | exposure @End
                'MEMB0.8 | num | name | colour | type (2D) | prop | group | topology | node | angle |  @end
                'mesh_size | is_intersector | analysis_type | fire | time[4] | dummy | @End
                'off_auto_internal | off_z | exposure
                sArg = GsaComUtil.Arg(0, sGwaCommand)
                Dim idString As String = GsaComUtil.ExtractId(sArg)
                Dim moduleVesrion As String = ExtractVesrion(sArg)

                If Not String.IsNullOrEmpty(idString) Then
                    uid = idString
                End If
                Dim iNextIndex As Integer = 1
                sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
                Debug.Assert(Integer.Equals(iElem, CInt(sArg)))

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'name
                sName = sArg

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'color

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'member type
                eMembType = MembTypeFromStr(sArg)

                Dim b1D As Boolean = Not Is2D(eMembType)

                iNextIndex = iNextIndex + 1 ' exposed face

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
                iOrNode = CInt(sArg)

                iNextIndex = iNextIndex + 1
                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'beta
                dBeta = CDbl(sArg)


                iNextIndex = iNextIndex + 1                    'mesh size

                iNextIndex = iNextIndex + 1                    'Intersector

                iNextIndex = iNextIndex + 1                    'analysis type

                iNextIndex = iNextIndex + 1 'fire
                iNextIndex = iNextIndex + 1 'limiting temperature
                iNextIndex = iNextIndex + 1 'time 1
                iNextIndex = iNextIndex + 1 'time 2
                iNextIndex = iNextIndex + 1 'time 3
                iNextIndex = iNextIndex + 1 'time 4
                iNextIndex = iNextIndex + 1 'dummy

                sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'dummy
                Dim bDummy As Boolean = False
                If sArg.Equals("DUMMY") Then
                    bDummy = True
                Else
                    bDummy = False
                End If
                If b1D Then

                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'release 0
                    For Each c As Char In sArg
                        If c.Equals("K") Then
                            iNextIndex = iNextIndex + 1
                        End If
                    Next
                    sRelease.Add(sArg)
                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'release 1
                    For Each c As Char In sArg
                        If c.Equals("K") Then
                            iNextIndex = iNextIndex + 1
                        End If
                    Next
                    sRelease.Add(sArg)

                    iNextIndex = iNextIndex + 1 'connection 1
                    endRestraint1 = GsaComUtil.Arg(iNextIndex, sGwaCommand)

                    iNextIndex = iNextIndex + 1 'connection 2
                    endRestraint2 = GsaComUtil.Arg(iNextIndex, sGwaCommand)

                    iNextIndex = iNextIndex + 1

                    'steel member
                    Dim restraintOption As Integer = RestraintOptID(Arg(iNextIndex, sGwaCommand))
                    If restraintOption.Equals(1) Then 'explicit
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

                    ElseIf restraintOption.Equals(2) Then 'effective length
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
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'offset

                    If sArg.Equals("OFF") Then
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
                        Dim offset0() As Double = {X1, y, z}
                        Dim offset1() As Double = {X2, y, z}
                        dOffset.Add(offset0)
                        dOffset.Add(offset1)
                    Else
                        dOffset.Add({0, 0, 0})
                        dOffset.Add({0, 0, 0})

                    End If
                Else
                    iNextIndex = iNextIndex + 1
                    sArg = GsaComUtil.Arg(iNextIndex, sGwaCommand) 'z
                    iNextIndex = iNextIndex + 1   'auto internal offset
                    Dim z As Double = CDbl(sArg)
                    Dim offset0() As Double = {0, 0, z}
                    Dim offset1() As Double = {0, 0, z}
                    dOffset.Add(offset0)
                    dOffset.Add(offset1)
                End If


                Dim nEle As New MemberElement
                nEle.Element = iElem
                nEle.Name = sName
                nEle.SecPro = iProp
                nEle.UID = uid
                nEle.sTopo = sTopo
                nEle.Beta = dBeta
                nEle.Offset = dOffset
                nEle.MemberType = eMembType
                nEle.Release = sRelease
                nEle.OrientationNode = iOrNode
                'GSA-5631
                nEle.Endrestraint1 = endRestraint1
                nEle.Endrestraint2 = endRestraint2
                _MemList.Add(iElem, nEle)

            Catch ex As Exception
                Return False
            End Try
        End If
        Return True
    End Function

    Public Function SetElem1d(ByVal iElem As Integer, ByVal name As String, ByVal iProp As Integer, ByVal sID As String,
                               ByVal iTopoList As List(Of Integer), ByVal iOrNode As Integer, ByVal dBeta As Double,
                               ByVal sRelease As List(Of String), ByVal dOffset As List(Of Double()), ByVal type As ElemType, Optional ByVal strDummy As String = "") As Integer

        '	EL.4 | num | name | colour | type | prop | group | topo() | orient_node | orient_angle | @br
        '	is_rls { | rls { | k } }
        '	off_x1 | off_x2 | off_y | off_z | parent_member | dummy  @end

        Dim sGwaCommand As String = ""
        Dim clr As Color = Color.Black()

        If (iElem <= 0) Then
            iElem = Me.HighestEnt("EL") + 1
        End If
        'round
        dBeta = Math.Round(dBeta, RoundPrecision)
        sGwaCommand = "EL:"
        sGwaCommand += "{RVT:" & sID & "}"
        sGwaCommand += "," + iElem.ToString()        'number
        sGwaCommand += "," + name                    'name
        sGwaCommand += "," + clr.ToArgb().ToString() 'color
        sGwaCommand += "," + ElemDesc(type)           'element type
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
            For Each c As Char In rels
                If c.Equals("K") Then
                    sGwaCommand += ",1.0"
                End If
            Next
        Next

        If dOffset.Count > 1 Then
            sGwaCommand += "," + dOffset(0)(0).ToString()      'X1 
            sGwaCommand += "," + dOffset(1)(0).ToString()      'X2                           
            sGwaCommand += "," + dOffset(0)(1).ToString()      'Y 
            sGwaCommand += "," + dOffset(0)(2).ToString()      'Z
        Else
            sGwaCommand += ",0" 'X1 
            sGwaCommand += ",0" 'X2 
            sGwaCommand += ",0" 'Y 
            sGwaCommand += ",0" 'Z
        End If
        sGwaCommand += "," + strDummy
        m_GSAObject.GwaCommand(sGwaCommand)

        Return iElem
    End Function


    'read a GSA 1D element
    Public Function Elem1d(ByVal iElem As Integer, ByRef name As String, ByRef iProp As Integer, ByRef uid As String,
            ByRef iTopoList As List(Of Integer), ByRef iOrNode As Integer, ByRef dBeta As Double,
            ByRef sRelease As List(Of String), ByRef dOffset As List(Of Double()), ByRef elemType As ElemType,
            ByRef strDummy As String) As Boolean

        '	EL.4 | num | name | colour | type | prop | group | topo() | orient_node | orient_angle | @br
        '	is_rls { | rls { | k } }
        '	off_x1 | off_x2 | off_y | off_z | parent_member | dummy  @end

        If _EleList.ContainsKey(iElem) Then
            Dim nEle As MemberElement = _EleList(iElem)
            iElem = nEle.Element
            name = nEle.Name
            iProp = nEle.SecPro
            uid = nEle.UID
            iTopoList = nEle.Topo

            dBeta = nEle.Beta
            sRelease = nEle.Release
            dOffset = nEle.Offset
            iOrNode = nEle.OrientationNode
            strDummy = nEle.Dummy
            elemType = nEle.ElementType
        Else
            iTopoList = New List(Of Integer)
            dOffset = New List(Of Double())
            sRelease = New List(Of String)

            If Not Me.EntExists("EL", iElem) Then
                Return False
            End If

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

            name = GsaComUtil.Arg(2, sGwaCommand)

            sArg = GsaComUtil.Arg(4, sGwaCommand)
            elemType = Me.ElemTypeFromString(sArg)

            If Not GsaComUtil.ElemTypeIsBeamOrTruss(elemType) Then
                Return False
            End If

            sArg = GsaComUtil.Arg(1, sGwaCommand) 'number
            Debug.Assert(Integer.Equals(iElem, CInt(sArg)))
            sArg = GsaComUtil.Arg(5, sGwaCommand) 'property

            'https://ovearup.atlassian.net/browse/GSA-5998
            Dim splitProperty As String() = sArg.Split("["c)
            iProp = CInt(splitProperty(0))

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
                For Each c As Char In sArg
                    If c.Equals("K") Then
                        position += 1
                    End If
                Next
                sRelease.Add(sArg)
                position += 1
                sArg = GsaComUtil.Arg(position, sGwaCommand) 'release 1
                For Each c As Char In sArg
                    If c.Equals("K") Then
                        position += 1
                    End If
                Next
                sRelease.Add(sArg)
            Else
                sRelease.Add("FFFFFF")
                sRelease.Add("FFFFFF")
            End If

            'offsets
            position += 1
            Dim dOff() As Double = New Double() {0, 0, 0, 0}
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
            nEle.Name = name
            nEle.SecPro = iProp
            nEle.UID = uid
            nEle.Topo = iTopoList
            ' nEle.OrientationNode = iOrNode
            nEle.Beta = dBeta
            nEle.Release = sRelease
            nEle.Offset = dOffset
            nEle.Dummy = strDummy
            nEle.OrientationNode = iOrNode
            nEle.ElementType = elemType
            _EleList.Add(iElem, nEle)
        End If
        Return True
    End Function

    Public Function Prop2D(ByVal i2dprop As Integer, ByRef sidFromGsa As String, ByRef usage As SectionUsage, ByRef name As String, ByRef description As String, ByRef ematType As MembMat, ByRef iMat As Integer, ByRef eType As Type2D) As Boolean
        '	PROP_2D.8 | num | name | colour | type | axis | mat | mat_type | grade | design | profile | ref_pt | ref_z |
        '		mass | flex | shear | inplane | weight | @end
        Try

            Dim sGwaCommand As String = CStr(m_GSAObject.GwaCommand("GET,PROP_2D," & i2dprop.ToString))
            Dim sArg As String = GsaComUtil.Arg(0, sGwaCommand)
            usage = SectionUsage.INVALID
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
            eType = Type2DFromStr(sArg)
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
            description = GsaComUtil.Arg(iIndex, sGwaCommand)
        Catch ex As Exception
            Return False
        End Try
        Return True
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

    Public Function SetSection(ByVal iSec As Integer, ByVal sName As String, ByVal uid As String, ByVal usage As SectionUsage,
                               ByVal iAnalysisMat As Integer, ByVal sDesc As String, ByVal eMatType As MembMat,
                               ByVal justification As String, Optional ByVal bNameMap As Boolean = False, Optional ByVal bDescMap As Boolean = False) As Integer
        '//	PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
        Dim sGwaCommand As String = ""
        If (iSec <= 0) Then
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
        sGwaCommand += "," & justification          'justification
        sGwaCommand += ",0,0"                       'offset
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
    Public Function Section(ByVal iSec As Integer, ByRef sName As String, ByRef sid As String, ByRef usage As GsaComUtil.SectionUsage, ByRef iAnalysisMat As Integer, ByRef sDesc As String, ByRef eMatType As GsaComUtil.MembMat, ByRef justification As String, Optional ByRef MapOp As String = "") As Boolean

        Try
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
                justification = gsaSec.justification
            Else
                'PROP_SEC.3 | num | name | colour | mat | grade | anal | desc | cost @end
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
                MapOp = "SID"
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
                sDesc = sArg.Replace("%", " ")
                gsaSec.Desc = sDesc

                justification = GsaComUtil.Arg(9, sGwaCommand) 'justification
                gsaSec.justification = justification
                _SecList.Add(iSec, gsaSec)
            End If
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function
    Function Is2D(ByVal type As MembType) As Boolean
        If type.Equals(MembType.GENERIC_2D) OrElse type.Equals(MembType.WALL) OrElse type.Equals(MembType.SLAB) Then
            Return True
        End If
        Return False
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
            Dim endRestraint1 As String = ""
            Dim endRestraint2 As String = ""
            ' Dim Mat As GsaComUtil.MembMat
            Dim bOut As Boolean = False
            Dim iTopoList As New List(Of Integer)
            Dim sTopo As String = ""
            Dim eEntType As EntType = EntType.SEL_MEMBER
            If "MEMB" = strEnt Then
                bOut = Member(iEnt, sName, iProp, uID, sTopo, iOrNode, dBeta, sRelease, dOffset, eMembtype, endRestraint1, endRestraint2)
                eEntType = EntType.SEL_MEMBER
                If Not bOut Then
                    Continue For
                End If
                If Not b2D.Equals(Is2D(eMembtype)) Then
                    Continue For
                End If
            Else
                eEntType = EntType.SEL_ELEM
                bOut = Elem1d(iEnt, sName, iProp, uID, iTopoList, iOrNode, dBeta, sRelease, dOffset, eletype, strDummy)
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


            Dim bVertical As Boolean = EntIsVertical(iEnt, eEntType)
            Dim bHoriZontal As Boolean = EntIsHorizontal(iEnt, eEntType)
            If b2D Then
                bVertical = Not bHoriZontal
            End If

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
            Else
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
            End If

            If b2D Then
                If bSlab AndAlso bWall Then
                    Return SectionUsage.INVALID
                End If
            Else
                If bFram AndAlso bColm Then
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

    Public Shared Function IsEqual(ByVal d1 As Double, ByVal d2 As Double, ByVal Tol As Double) As Boolean
        If Abs(d1 - d2) < Tol Then
            Return True
        Else
            Return False
        End If
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
    Public Function SetProp2D(ByVal iProp As Integer, ByVal uid As String, ByVal usage As GsaComUtil.SectionUsage, ByVal sName As String, ByVal description As String, ByVal eType As Type2D, ByVal eMaterType As GsaComUtil.MembMat, ByVal iMat As Integer) As Integer
        'PROP_2D.8 | num | name | colour | type | axis | mat | mat_type | grade | design | profile | ref_pt | ref_z |
        'mass | flex | shear | inplane | weight | @end
        If (iProp <= 0) Then
            iProp = HighestProp2d() + 1
        End If
        sName = sName.Replace("""", String.Empty)
        Dim sid As String = "{" & GsaComUtil.SectionSid_Symbol & ":" & uid & "}{" & GsaComUtil.SectionSid_Usage & ":" & usage.ToString() & "}"
        Dim sGwaCommand As String = "PROP_2D:"
        sGwaCommand += "{RVT:" & sid & "}" + ","
        sGwaCommand += iProp.ToString() + ","
        sGwaCommand += sName + ","
        sGwaCommand += "NO_RGB,"    'colour
        sGwaCommand += eType.ToString() + ","  ' 2D element type
        sGwaCommand += "GLOBAL,"
        sGwaCommand += "0," ' analysis material
        sGwaCommand += eMaterType.ToString() + ","
        sGwaCommand += iMat.ToString() + "," 'undefined
        sGwaCommand += "0," 'slab design peoperty
        sGwaCommand += description + ","
        sGwaCommand += "CENTROID," 'refrence point centroid
        sGwaCommand += "0.0," 'set defuault offset to 0
        sGwaCommand += "0.0,100%,100%,100%,100%"
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iProp
    End Function
    'write a GSA Material
    Public Function SetMaterial(ByVal iMat As Integer, ByVal sName As String, ByVal sid As String, ByVal sDesc As String,
                                  ByVal dE As Double, ByVal dNu As Double, ByVal dG As Double, ByVal dRho As Double, ByVal dAlpha As Double,
                                  ByVal dDamp As Double) As Integer
        Dim sGwaCommand As String = ""

        If (iMat <= 0) Then
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
        sGwaCommand += "," & dDamp.ToString
        sGwaCommand += ",0,0"
        m_GSAObject.GwaCommand(sGwaCommand)
        Return iMat
    End Function

    'read a GSA Material
    Public Function Material(ByVal iMat As Integer, ByRef sName As String, ByRef sid As String, ByRef sDesc As String,
                                    ByRef dE As Double, ByRef dNu As Double, ByRef dG As Double, ByRef dRho As Double, ByRef dAlpha As Double,
                                    ByRef dDamp As Double) As Boolean
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
        sid = GsaComUtil.ExtractId(sArg) ' sid
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

        iCount = iCount + 1
        sArg = GsaComUtil.Arg(iCount, sGwaCommand)
        dDamp = Val(sArg)

        Return True

    End Function
    Function RestraintOptID(ByVal opt As String) As Integer
        Dim iOption As Integer = 0
        Select Case opt
            Case "AUTOMATIC", "0"
                iOption = 0
            Case "EXPLICIT", "1"
                iOption = 1
            Case "EFF_LEN", "2"
                iOption = 2
            Case Else
                iOption = 0
        End Select
        Return iOption
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
    Public Function GetGsaModelUnit(ByVal units As String) As String
        Dim commandObj As Object = m_GSAObject.GwaCommand("GET,UNIT_DATA," + units.ToUpper())
        If commandObj Is Nothing Then
            Return ""
        End If
        Dim commandResult As String = commandObj.ToString()
        Return GsaComUtil.Arg(2, commandResult)
    End Function
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
        Return cStrItem.Trim()
    End Function
    Public Function ListToArcString(ByRef lists As List(Of Integer)) As String
        Debug.Assert(lists.Count.Equals(3))
        Dim cStrItem As String = lists(0).ToString() + " a " + lists(1).ToString() + " " + lists(2).ToString()
         Return cStrItem.Trim()
    End Function
    Public Sub StringTolist(ByVal sTopo As String, ByRef orderedNodes As List(Of List(Of Integer)), ByRef arcNode As List(Of Integer), ByRef voids As List(Of List(Of Integer)))
       
        orderedNodes = New List(Of List(Of Integer))
        arcNode = New List(Of Integer)
        voids = New List(Of List(Of Integer))
        If sTopo Is Nothing
            Return
        End If
        Dim pattern As String = "(\(?[0-9a]\s*\)?)+|([AVLP][(]([0-9a]\s*)+[)])"
        Dim rg As New Regex(pattern)
        Dim items As New List(Of String)
        For Each item As Match In rg.Matches(sTopo)
            items.Add(item.Value)
        Next

       
        For Each item As String In items
            Dim voidList As New List(Of Integer)
            Dim solidList As New List(Of Integer)
            Dim words As String() = item.Split(New Char() {" "c})
            Dim isAcrNode As Boolean = False
            If item.Contains("P") OrElse item.Contains("L") Then
                Continue For
            End If
            For Each word As String In words
                If word.Trim().Equals("a") Then
                    isAcrNode = True
                    Continue For
                End If
                Dim wordString As String = word.Trim().Replace("(", "").Replace("V", "").Replace("A", "").Replace(")", "")
                Dim nodeId As Integer = 0
                If Integer.TryParse(wordString, nodeId) Then
                    If item.Contains("V(") Then
                        voidList.Add(nodeId)
                    Else
                        solidList.Add(nodeId)
                    End If
                    If isAcrNode Then
                        arcNode.Add(nodeId)
                        isAcrNode = False
                    End If
                End If
            Next
            If solidList.Count > 0 Then
                orderedNodes.Add(solidList)
            End If
            If voidList.Count > 0 Then
                voids.Add(voidList)
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
        Dim sidString As String = m_GSAObject.GetSidTagValue("SID", 1, "RVT")
        If String.IsNullOrEmpty(sidString) Then
            Return False
        End If
        Return Me.ParseNestedSid(sidString, sids)

    End Function

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
        If nRecord < 0 Then
            nRecord = 0
        End If
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
        Return Me.HighestRecord("LOAD_GRID_POINT")
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
    Function EntSid(ByVal entity As String, ByVal record As Integer) As String
        Return GetSid(entity, record)
    End Function

    Function EntSection(ByVal entity As String, ByVal iEnt As Integer) As Integer
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
        Dim endRestraint1 As String = ""
        Dim endRestraint2 As String = ""
        If String.Equals(entity, "EL") Then
            Me.Elem1d(iEnt, sName, iProp, uid, iTopoList, iOrNode, dBeta, release, dOffset, eleType, strDummy)
        Else
            Me.Member(iEnt, sName, iProp, uid, sTopo, iOrNode, dBeta, release, dOffset, eMembtype, endRestraint1, endRestraint2)
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
    Public Shared Function ExtractVesrion(ByVal sArg As String) As String
        Dim tag As String = "RVT:"
        If Not sArg.Contains(tag) Then
            Return sArg
        End If
        Dim pos_tag As Integer = sArg.IndexOf(tag)
        Return sArg.Substring(0, pos_tag - 2)
    End Function
    Public Property SelType() As EntType
        Get
            Return m_eSelType
        End Get
        Set(ByVal value As EntType)
            m_eSelType = value
        End Set
    End Property
     Public Shared Function DisplacementUnitFactor(ByVal unit As String) As Double
        Dim dSectionDispFactor As Double = 1.0
            If unit.Contains("(m)") Then
                dSectionDispFactor = ToMilliMeter.FromMeter
            ElseIf unit.Contains("(ft)") Then
                dSectionDispFactor = ToMilliMeter.FromFeet
            ElseIf unit.Contains("in") Then
                dSectionDispFactor = ToMilliMeter.FromInch
             ElseIf unit.Contains("cm") Then
                dSectionDispFactor = ToMilliMeter.FromCentimeter
            Else
                dSectionDispFactor = 1.0
            End If
         Return dSectionDispFactor
    End Function
End Class
