Imports Interop.gsa_8_7

Module Module1

    Sub Main()
        Dim gsa As New Interop.gsa_8_7.ComAuto

        gsa.NewFile()

        Dim tol As Double = 0.000001

        ' generate nodes here
        Dim n1 As Integer = gsa.Gen_NodeAt(0, 0, 0, tol)
        Dim n2 As Integer = gsa.Gen_NodeAt(0, 0, 5, tol)
        Dim n3 As Integer = gsa.Gen_NodeAt(0, 5, 5, tol)
        Dim n4 As Integer = gsa.Gen_NodeAt(0, 5, 0, tol)

        ' generate elements between them
        ' EL,1,,NO_RGB,BEAM,1,1,1,2
        ' EL.2 | num | name | colour | type | prop | group | topo() | orient_node | orient_angle | 
        ' is_rls { | rls { | k } } is_offset { | ox | oy | oz } | dummy

        Dim ret As Integer ' to store the return code
        Dim gwaRecord As String ' to construct the GWA string
        Dim elRef As Integer ' to store element number


        gsa.SaveAs("c:\GSA_training\portalframe01.gwb")
        gsa.Close()
        gsa = Nothing

    End Sub

End Module

