Imports Interop.Gsa_10_3
Imports System.Diagnostics

Module Module1

    Sub Main()
        Dim gsa As New Interop.Gsa_10_3.ComAuto

        ' open existing file
        gsa.Open("c:\GSA_training\portalframe02.gwb")

        Dim ret As Integer

        Dim ents() As Integer ' array to store node numbers in the list

        'add code here

        ' loop over the contents of the list
        For Each ent In ents
            ' fetch node as a GWA string
            Dim gwa As String = gsa.GwaCommand("GET, NODE," & ent)
            ' write it to the console
            Console.WriteLine(gwa)
            ' Fix all nodes
            gwa += ",NO_GRID,0,REST,1,1,1,1,1,1"
            ' store back into GSA
            gsa.GwaCommand(gwa)
        Next

        gsa.SaveAs("c:\GSA_training\portalframe03.gwb")
        gsa.Close()
        gsa = Nothing

    End Sub

End Module
