
Imports System
Imports System.IO
Imports System.Data
Imports System.Text
Imports System.Collections
Imports System.Collections.Generic
Imports System.Math
Imports System.Net

Public Structure OasysPoint
    Dim x As Integer
    Dim y As Integer
End Structure

''' <summary>
''' Class to store one CSV row
''' </summary>
Public Class CsvRow
    Inherits List(Of String)
    Public Property LineText() As String
        Get
            Return m_LineText
        End Get
        Set(value As String)
            m_LineText = value
        End Set
    End Property
    Private m_LineText As String
End Class

''' <summary>
''' Class to write data to a CSV file
''' </summary>
Public Class CsvFileWriter
    Inherits StreamWriter
    Public Sub New(stream As Stream)
        MyBase.New(stream)
    End Sub

    Public Sub New(filename As String)
        MyBase.New(filename)
    End Sub

    ''' <summary>
    ''' Writes a single row to a CSV file.
    ''' </summary>
    ''' <param name="row">The row to be written</param>
    Public Sub WriteRow(row As CsvRow)
        Dim builder As New StringBuilder()
        Dim firstColumn As Boolean = True
        For Each value As String In row
            ' Add separator if this isn't the first value
            If Not firstColumn Then
                builder.Append(","c)
            End If
            ' Implement special handling for values that contain comma or quote
            ' Enclose in quotes and double up any double quotes
            If value.IndexOfAny(New Char() {""""c, ","c}) <> -1 Then
                builder.AppendFormat("""{0}""", value.Replace("""", """"""))
            Else
                builder.Append(value)
            End If
            firstColumn = False
        Next
        row.LineText = builder.ToString()
        WriteLine(row.LineText)
    End Sub
End Class

''' <summary>
''' Class to read data from a CSV file
''' </summary>
Public Class CsvFileReader
    Inherits StreamReader
    Public Sub New(stream As Stream)
        MyBase.New(stream)
    End Sub

    Public Sub New(filename As String)
        MyBase.New(filename)
    End Sub

    ''' <summary>
    ''' Reads a row of data from a CSV file
    ''' </summary>
    ''' <param name="row"></param>
    ''' <returns></returns>
    Public Function ReadRow(row As CsvRow) As Boolean
        row.LineText = ReadLine()
        If [String].IsNullOrEmpty(row.LineText) Then
            Return False
        End If

        Dim pos As Integer = 0
        Dim rows As Integer = 0

        While pos < row.LineText.Length
            Dim value As String

            ' Special handling for quoted field
            If row.LineText(pos) = """"c Then
                ' Skip initial quote
                pos += 1

                ' Parse quoted value
                Dim start As Integer = pos
                While pos < row.LineText.Length
                    ' Test for quote character
                    If row.LineText(pos) = """"c Then
                        ' Found one
                        pos += 1

                        ' If two quotes together, keep one
                        ' Otherwise, indicates end of value
                        If pos >= row.LineText.Length OrElse row.LineText(pos) <> """"c Then
                            pos -= 1
                            Exit While
                        End If
                    End If
                    pos += 1
                End While
                value = row.LineText.Substring(start, pos - start)
                value = value.Replace("""""", """")
            Else
                ' Parse unquoted value
                Dim start As Integer = pos
                While pos < row.LineText.Length AndAlso row.LineText(pos) <> ","c
                    pos += 1
                End While
                value = row.LineText.Substring(start, pos - start)
            End If

            ' Add field to list
            If rows < row.Count Then
                row(rows) = value
            Else
                row.Add(value)
            End If
            rows += 1

            ' Eat up to and including next comma
            While pos < row.LineText.Length AndAlso row.LineText(pos) <> ","c
                pos += 1
            End While
            If pos < row.LineText.Length Then
                pos += 1
            End If
        End While
        ' Delete any unused items
        While row.Count > rows
            row.RemoveAt(rows)
        End While

        ' Return true if any columns read
        Return (row.Count > 0)
    End Function
End Class

Public Class GSASection
    Public Sub New()

    End Sub
    Private _Sid As String
    Public Property Sid() As String
        Get
            Return _Sid
        End Get
        Set(ByVal value As String)
            _Sid = value
        End Set
    End Property
    Private _Num As Integer
    Public Property Number() As Integer
        Get
            Return _Num
        End Get
        Set(ByVal value As Integer)
            _Num = value
        End Set
    End Property
    Private _Name As String
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property
    Private _AnalysisMat As Integer
    Public Property AnalysisMat() As Integer
        Get
            Return _AnalysisMat
        End Get
        Set(ByVal value As Integer)
            _AnalysisMat = value
        End Set
    End Property
    Private _Desc As String
    Public Property Desc() As String
        Get
            Return _Desc
        End Get
        Set(ByVal value As String)
            _Desc = value
        End Set
    End Property
    Private _MapOp As String
    Public Property MapOp() As String
        Get
            Return _MapOp
        End Get
        Set(ByVal value As String)
            _MapOp = value
        End Set
    End Property
    Private _iSecUsage As Integer
    Public Property SecUsage() As Integer
        Get
            Return _iSecUsage
        End Get
        Set(ByVal value As Integer)
            _iSecUsage = value
        End Set
    End Property

    Private _typeMaterial As GsaComUtil.MembMat
    Public Property MaterialType() As GsaComUtil.MembMat
        Get
            Return _typeMaterial
        End Get
        Set(ByVal value As GsaComUtil.MembMat)
            _typeMaterial = value
        End Set
    End Property
    Private _justification As String
    Public Property justification() As String
        Get
            Return _justification
        End Get
        Set(ByVal value As String)
            _justification = value
        End Set
    End Property

End Class
Public Class MemberElement
    Public Sub New()

    End Sub
    Public Sub New(ByVal cIn As MemberElement)
        _iEle = cIn.Element
        _Name = cIn.Name
        _dummy = cIn.Dummy
        _iProp = cIn.SecPro
        _iOrNode = cIn.OrientationNode
        _uid = cIn.UID
        _sRelease = cIn.Release
        _dOffset = cIn.Offset
        _TopoList = New List(Of Integer)(cIn.Topo)
        _dBeta = cIn.Beta
        _mtype = cIn.MemberType
    End Sub

    Private _iEle As Integer
    Public Property Element() As Integer
        Get
            Return _iEle
        End Get
        Set(ByVal value As Integer)
            _iEle = value
        End Set
    End Property
    Private _Name As String
    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
        End Set
    End Property
    Private _dummy As String
    Public Property Dummy() As String
        Get
            Return _dummy
        End Get
        Set(ByVal value As String)
            _dummy = value
        End Set
    End Property

    Private _iProp As Integer
    Public Property SecPro() As Integer
        Get
            Return _iProp
        End Get
        Set(ByVal value As Integer)
            _iProp = value
        End Set
    End Property

    Private _iOrNode As Integer
    Public Property OrientationNode() As Integer
        Get
            Return _iOrNode
        End Get
        Set(ByVal value As Integer)
            _iOrNode = value
        End Set
    End Property
    Private _uid As String
    Public Property UID() As String
        Get
            Return _uid
        End Get
        Set(ByVal value As String)
            _uid = value
        End Set
    End Property
    Private _sTop As String
    Public Property sTopo() As String
        Get
            Return _sTop
        End Get
        Set(ByVal value As String)
            _sTop = value
        End Set
    End Property

    Private _sRelease As List(Of String)
    Public Property Release() As List(Of String)
        Get
            Return _sRelease
        End Get
        Set(ByVal value As List(Of String))
            _sRelease = value
        End Set
    End Property

    Private _TopoList As List(Of Integer)
    Public Property Topo() As List(Of Integer)
        Get
            Return _TopoList
        End Get
        Set(ByVal value As List(Of Integer))
            Dim i As Integer
            _TopoList = New List(Of Integer)
            For Each i In value
                _TopoList.Add(i)
            Next
        End Set
    End Property

    Private _dBeta As Double
    Public Property Beta() As Double
        Get
            Return _dBeta
        End Get
        Set(ByVal value As Double)
            _dBeta = value
        End Set
    End Property

    Private _dOffset As List(Of Double())
    Public Property Offset() As List(Of Double())
        Get
            Return _dOffset
        End Get
        Set(ByVal value As List(Of Double()))
            _dOffset = value
        End Set
    End Property
    Private _etype As GsaComUtil.ElemType
    Public Property ElementType() As GsaComUtil.ElemType
        Get
            Return _etype
        End Get
        Set(ByVal value As GsaComUtil.ElemType)
            _etype = value
        End Set
    End Property
    Private _mtype As GsaComUtil.MembType
    Public Property MemberType() As GsaComUtil.MembType
        Get
            Return _mtype
        End Get
        Set(ByVal value As GsaComUtil.MembType)
            _mtype = value
        End Set
    End Property
    Private _endrestraint1 As String
    Public Property Endrestraint1() As String
        Get
            Return _endrestraint1
        End Get
        Set(ByVal value As String)
            _endrestraint1 = value
        End Set
    End Property
    Private _endrestraint2 As String
    Public Property Endrestraint2() As String
        Get
            Return _endrestraint2
        End Get
        Set(ByVal value As String)
            _endrestraint2 = value
        End Set
    End Property

End Class

Public Class Utils

    Public Sub New()

    End Sub

    '/ <summary>
    '/ Returns true for a precision 
    '/ </summary>
    '/ <param name="s"></param>
    '/ <param name="t"></param>
    '/ <returns></returns>
    Public Shared Function IsApproxEqual(ByVal s As Double, ByVal t As Double, ByVal precision As Double) As Boolean
        If Math.Abs(s - t) < precision Then
            Return True
        Else
            Return False
        End If
    End Function

    'append an extension to a filename removing the existing extension if there is one
    Shared Function ChangeFileExt(ByVal filenameIn As String, ByVal extIn As String) As String
        ChangeFileExt = filenameIn
        Dim i As Integer = ChangeFileExt.LastIndexOf(".")
        If (i >= 0) Then
            ChangeFileExt = ChangeFileExt.Remove(i, ChangeFileExt.Length() - i) 'strip extension
        End If
        ChangeFileExt &= "."
        ChangeFileExt &= extIn
    End Function


    '  Helper function - recursively search the given file name under the current directory. 
    '
    Public Shared Function SearchFile(ByVal path As String, ByVal fileName As String) As String
        '  recursively search child directories.  
        Dim directoryName As String
        For Each directoryName In Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
            If Not directoryName.Contains("Structural") Then
                Continue For
            End If

            For Each revitRFAfileName As String In Directory.GetFiles(directoryName)
                If (fileName.Equals(System.IO.Path.GetFileName(revitRFAfileName))) Then
                    Return revitRFAfileName
                End If
            Next
        Next
        Return Nothing
    End Function
    Public Shared Function TrimSpace(ByVal source As String) As String
        Dim sReturn As String = ""
        Dim arr As String() = source.Split(New Char() {" "c}, System.StringSplitOptions.RemoveEmptyEntries)
        For Each str As String In arr
            sReturn = sReturn + str
        Next
        Return sReturn
    End Function

    Public Shared Function CreateDirectory(ByVal path As String) As Boolean

        Dim info As System.IO.DirectoryInfo = Nothing
        Try
            info = System.IO.Directory.CreateDirectory(path)
            If True = info.Exists Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function OnSegment(ByRef p As Double(), ByRef q As Double(), ByRef r As Double()) As Boolean
        If q(0) <= Max(p(0), r(0)) AndAlso q(0) >= Min(p(0), r(0)) AndAlso q(1) <= Max(p(1), r(1)) AndAlso q(1) >= Min(p(1), r(1)) Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function Orientation(ByRef p As Double(), ByRef q As Double(), ByRef r As Double()) As Integer
        Dim val As Double = (q(1) - p(1)) * (r(0) - q(0)) - (q(0) - p(0)) * (r(1) - q(1))
        If IsApproxEqual(val, 0, 0.0001) Then
            Return 0
        End If
        If val > 0 Then
            Return 1
        Else
            Return 2
        End If
    End Function
    Public Shared Function DoInterSect(ByRef p1 As Double(), ByRef q1 As Double(), ByRef p2 As Double(), ByRef q2 As Double()) As Boolean
        'Find the four orientations needed for general and
        'special cases
        'http://www.geeksforgeeks.org/how-to-check-if-a-given-point-lies-inside-a-polygon/
        Dim o1 As Integer = Orientation(p1, q1, p2)
        Dim o2 As Integer = Orientation(p1, q1, q2)
        Dim o3 As Integer = Orientation(p2, q2, p1)
        Dim o4 As Integer = Orientation(p2, q2, q1)
        'General case
        If Not o1.Equals(o2) AndAlso Not o3.Equals(o4) Then
            Return True
        End If
        'Special Cases
        'p1, q1 and p2 are collinear and p2 lies on segment p1q1
        If o1.Equals(0) AndAlso OnSegment(p1, p2, q1) Then
            Return True
        End If
        'p1, q1 and q2 are collinear and q2 lies on segment p1q1
        If o2.Equals(0) AndAlso OnSegment(p1, q2, q1) Then
            Return True
        End If
        'p2, q2 and p1 are collinear and p1 lies on segment p2q2
        If o3.Equals(0) AndAlso OnSegment(p2, p1, q2) Then
            Return True
        End If
        ' p2, q2 and q1 are collinear and q1 lies on segment p2q2
        If o4.Equals(0) AndAlso OnSegment(p2, q1, q2) Then
            Return True
        End If
        Return False 'Doesn't fall in any of the above cases
    End Function
    Public Shared Function IsInsidePolygon(ByVal points As List(Of Double()), ByVal point As Double()) As Boolean
        Dim n As Integer = points.Count
        If n < 3 Then
            Return False
        End If
        Dim extreme As Double() = New Double() {10000, point(1), 0}
        Dim count As Integer = 0
        Dim i As Integer = 0
        Do
            Dim j As Integer = ((i + 1) Mod n)
            If DoInterSect(points.Item(i), points.Item(j), point, extreme) Then
                If Orientation(points.Item(i), point, points.Item(j)) = 0 Then
                    Return OnSegment(points.Item(i), point, points.Item(j))
                End If
                count = count + 1
            End If
            i = j
        Loop While i <> 0
        Return CType(count And 1, Boolean)
    End Function
    Public Shared Function DisplacementUnitLabel(ByVal description As String) As String
        Dim dSectionDispFactor As Double = 1.0
        If description.Contains("(m)") Then
            Return "(m)"
        ElseIf description.Contains("(mm)") Then
            Return "(mm)"
        ElseIf description.Contains("(ft)") Then
            Return "(ft)"
        ElseIf description.Contains("in") Then
            Return "(in)"
        ElseIf description.Contains("cm") Then
            Return "(cm)"
        End If
        Return "(m)"
    End Function

    Public Shared Function SectionDescription2D(ByVal thickness As Double, ByVal unitDescription As String) As String
        Return thickness.ToString() + unitDescription
    End Function

    Public Shared Function thinknessFrom2dProfile(ByVal profile As String) As Double
        Dim profiles As String() = profile.Split(New Char() {"("c, ")"c})
        Dim thinkness As Double = 0.0
        If profiles.Length > 1 Then
            Double.TryParse(profiles(0), thinkness)
            Return thinkness
        End If
        Return 0.0
    End Function

    Public Shared Function DownloadExampleFile(ByVal file_name_to_be_download_from_general_folder As String, ByVal file_name_to_be_saved As String) As String
        System.IO.Directory.CreateDirectory(Environment.CurrentDirectory)
        Dim webClient As WebClient = New WebClient()
        Dim path As String = Environment.CurrentDirectory & "\" & file_name_to_be_saved
        Dim uri As Uri = New Uri("https://samples.oasys-software.com/gsa/10.2/General/" & file_name_to_be_download_from_general_folder)
        webClient.DownloadFile(uri, path)
        Return path
    End Function
End Class
