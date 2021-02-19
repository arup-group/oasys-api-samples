
Imports System
Imports System.IO
Imports System.Data
Imports System.Text
Imports System.Collections
Imports System.Collections.Generic
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


Public Class GSALine
    Public Sub New()

    End Sub
    Public Sub New(ByVal cIn As GSALine)
        _iLine = cIn.LineNumber
        _iNode0 = cIn.Node0
        _iNode0 = cIn.Node1
        _iNode2 = cIn.Node2
        _iLineType = cIn.LineType
        _StartPoint = cIn.StartPoint
        _EndPoint = cIn.EndPoint
        _MidPoint = cIn.MidPoint
    End Sub
    Private _iLine As Integer
    Public Property LineNumber() As Integer
        Get
            Return _iLine
        End Get
        Set(ByVal value As Integer)
            _iLine = value
        End Set
    End Property
    Private _iNode0 As Integer
    Public Property Node0() As Integer
        Get
            Return _iNode0
        End Get
        Set(ByVal value As Integer)
            _iNode0 = value
        End Set
    End Property
    Private _iNode1 As Integer
    Public Property Node1() As Integer
        Get
            Return _iNode1
        End Get
        Set(ByVal value As Integer)
            _iNode1 = value
        End Set
    End Property
    Private _iNode2 As Integer
    Public Property Node2() As Integer
        Get
            Return _iNode2
        End Get
        Set(ByVal value As Integer)
            _iNode2 = value
        End Set
    End Property

    Private _iLineType As GsaComUtil.LineType
    Public Property LineType() As GsaComUtil.LineType
        Get
            Return _iLineType
        End Get
        Set(ByVal value As GsaComUtil.LineType)
            _iLineType = value
        End Set
    End Property

    Private _StartPoint As Double()
    Public Property StartPoint() As Double()
        Get
            Return _StartPoint
        End Get
        Set(ByVal value As Double())
            _StartPoint = value
        End Set
    End Property
    Private _EndPoint As Double()
    Public Property EndPoint() As Double()
        Get
            Return _EndPoint
        End Get
        Set(ByVal value As Double())
            _EndPoint = value
        End Set
    End Property
    Private _MidPoint As Double()
    Public Property MidPoint() As Double()
        Get
            Return _MidPoint
        End Get
        Set(ByVal value As Double())
            _MidPoint = value
        End Set
    End Property
End Class
Public Class GSASection
    Public Sub New()

    End Sub
    Public Sub New(ByVal cIn As GSASection)

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

    Private _typeMemb As GsaComUtil.MembType
    Public Property MembType() As GsaComUtil.MembType
        Get
            Return _typeMemb
        End Get
        Set(ByVal value As GsaComUtil.MembType)
            _typeMemb = value
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
        _dRadius = cIn.Radius
        _TopoList = New List(Of Integer)(cIn.Topo)
        _dBeta = cIn.Beta
        _mtype = cIn.MemberType
        _typeMaterial = cIn.MembMaterialType
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

    Private _dRadius As Double
    Public Property Radius() As Double
        Get
            Return _dRadius
        End Get
        Set(ByVal value As Double)
            _dRadius = value
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
    Private _mtype As GsaComUtil.MembType
    Public Property MemberType() As GsaComUtil.MembType
        Get
            Return _mtype
        End Get
        Set(ByVal value As GsaComUtil.MembType)
            _mtype = value
        End Set
    End Property
    Private _typeMaterial As GsaComUtil.MembMat
    Public Property MembMaterialType() As GsaComUtil.MembMat
        Get
            Return _typeMaterial
        End Get
        Set(ByVal value As GsaComUtil.MembMat)
            _typeMaterial = value
        End Set
    End Property

End Class
Public Class Vector3
    Public X As Double
    ' Length of a box
    Public Y As Double
    ' Breadth of a box
    Public Z As Double
    ' Height of a box
    Public Sub New()
        X = 0
        Y = 0
        Z = 0
    End Sub
    Public Sub New(InX As Double, InY As Double, InZ As Double)
        X = InX
        Y = InY
        Z = InZ
    End Sub

    Public Sub setX(InX As Double)
        X = InX
    End Sub
    Public Shared Function Empty() As Vector3
        Dim box As New Vector3()
        box.X = 0
        box.Y = 0
        box.Z = 0
        Return box
    End Function
    Public Function LengthSq() As Double
        Return X * X + Y * Y + Z * Z
    End Function
    Public Sub setY(InY As Double)
        Y = InY
    End Sub
    Public Sub setZ(InZ As Double)
        Z = InZ
    End Sub
    ' Overload + operator to add two Box objects.
    Public Shared Operator +(b As Vector3, c As Vector3) As Vector3
        Dim box As New Vector3()
        box.X = b.X + c.X
        box.Y = b.Y + c.Y
        box.Z = b.Z + c.Z
        Return box
    End Operator
    ' Overload + operator to add two Box objects.
    Public Shared Operator -(b As Vector3, c As Vector3) As Vector3
        Dim box As New Vector3()
        box.X = b.X - c.X
        box.Y = b.Y - c.Y
        box.Z = b.Z - c.Z
        Return box
    End Operator
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
    Public Shared Function onSegment(ByVal p As OasysPoint, ByVal q As OasysPoint, ByVal r As OasysPoint) As Boolean
        If (q.x <= Math.Max(p.x, r.x)) AndAlso (q.x >= Math.Min(p.x, r.x)) AndAlso
            (q.y <= Math.Max(p.y, r.y)) AndAlso (q.y >= Math.Min(p.y, r.y)) Then
            Return True
        End If
        Return False
    End Function
    Public Shared Function CalculateLineLineIntersection(line1Point1 As Vector3, line1Point2 As Vector3, line2Point1 As Vector3, line2Point2 As Vector3, ByRef resultSegmentPoint1 As Vector3, ByRef resultSegmentPoint2 As Vector3) As Boolean
        'http://paulbourke.net/geometry/pointlineplane/
        resultSegmentPoint1 = Vector3.Empty
        resultSegmentPoint2 = Vector3.Empty
        Dim p1 As Vector3 = line1Point1
        Dim p2 As Vector3 = line1Point2
        Dim p3 As Vector3 = line2Point1
        Dim p4 As Vector3 = line2Point2
        Dim p13 As Vector3 = p1 - p3
        Dim p43 As Vector3 = p4 - p3

        If p43.LengthSq() < 0 Then
            Return False
        End If
        Dim p21 As Vector3 = p2 - p1
        If p21.LengthSq() < 0 Then
            Return False
        End If

        Dim d1343 As Double = p13.X * CDbl(p43.X) + CDbl(p13.Y) * p43.Y + CDbl(p13.Z) * p43.Z
        Dim d4321 As Double = p43.X * CDbl(p21.X) + CDbl(p43.Y) * p21.Y + CDbl(p43.Z) * p21.Z
        Dim d1321 As Double = p13.X * CDbl(p21.X) + CDbl(p13.Y) * p21.Y + CDbl(p13.Z) * p21.Z
        Dim d4343 As Double = p43.X * CDbl(p43.X) + CDbl(p43.Y) * p43.Y + CDbl(p43.Z) * p43.Z
        Dim d2121 As Double = p21.X * CDbl(p21.X) + CDbl(p21.Y) * p21.Y + CDbl(p21.Z) * p21.Z

        Dim denom As Double = d2121 * d4343 - d4321 * d4321
        If Math.Abs(denom) < 0 Then
            Return False
        End If
        Dim numer As Double = d1343 * d4321 - d1321 * d4343

        Dim mua As Double = numer / denom
        Dim mub As Double = (d1343 + d4321 * (mua)) / d4343

        resultSegmentPoint1.X = CSng(p1.X + mua * p21.X)
        resultSegmentPoint1.Y = CSng(p1.Y + mua * p21.Y)
        resultSegmentPoint1.Z = CSng(p1.Z + mua * p21.Z)
        resultSegmentPoint2.X = CSng(p3.X + mub * p43.X)
        resultSegmentPoint2.Y = CSng(p3.Y + mub * p43.Y)
        resultSegmentPoint2.Z = CSng(p3.Z + mub * p43.Z)

        Return True
    End Function

    Public Shared Function Get_Line_Intersection(ByVal p0_x As Double, ByVal p0_y As Double, ByVal p1_x As Double, ByVal p1_y As Double,
    ByVal p2_x As Double, ByVal p2_y As Double, ByVal p3_x As Double, ByVal p3_y As Double, ByRef i_x As Double, ByRef i_y As Double, ByRef bIntersect As Boolean) As Boolean
        Dim s1_x, s1_y, s2_x, s2_y As Double
        s1_x = 0
        s1_y = 0
        s2_x = 0
        s2_y = 0
        s1_x = p1_x - p0_x
        s1_y = p1_y - p0_y
        s2_x = p3_x - p2_x
        s2_y = p3_y - p2_y

        Dim s, t As Double
        s = 0
        t = 0
        s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y)
        t = (s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y)
        If Math.Abs(-s2_x * s1_y + s1_x * s2_y) > 0 Then
            If (s >= 0 AndAlso s <= 1 AndAlso t >= 0 AndAlso t <= 1) Then
                bIntersect = True
                i_x = p0_x + (t * s1_x)
                i_y = p0_y + (t * s1_y)
            End If
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
        'MessageBox.Show("ChangeFileExt - exit - ChangeFileExt <" & ChangeFileExt & "> i <" & i & ">")
    End Function


    '  Helper function - recursively search the given file name under the current directory. 
    '
    Public Shared Function SearchFile(ByVal path As String, ByVal fileName As String) As String

        '  search this directory 
        Dim fname As String
        For Each fname In Directory.GetFiles(path, fileName) ', SearchOption.AllDirectories)
            'The above overload searches recursively
            Return path
        Next
        '  recursively search child directories.  
        Dim dname As String
        For Each dname In Directory.GetDirectories(path)
            If Not dname.Contains("Structural") Then
                Continue For
            End If
            Dim filePath As String = SearchFile(dname, fileName)
            If Not (filePath Is Nothing) Then
                Return filePath
            End If
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

End Class
