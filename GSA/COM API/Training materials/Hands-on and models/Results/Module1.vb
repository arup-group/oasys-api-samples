Imports Interop.gsa_8_7
Imports System.Diagnostics

Module Module1

    Sub Main()


        Dim gsa As New Interop.gsa_8_7.ComAuto

        ' open existing file
        gsa.Open("c:\GSA_training\portalframe04.gwb")
        gsa.Analyse()

        Dim ret As Integer

        ' Assume we're interested in windloads (L2)
        Dim analysiscase As Integer = 2
        Dim acaseString As String = "A" & analysiscase

        ret = gsa.CaseExist("A", analysiscase)

        If 0 = ret Then
            Console.WriteLine("Analysis case {0} does not exist. ", analysiscase)
            Exit Sub
        End If

        ret = gsa.CaseResultsExist("A", analysiscase, 0)
        If 0 = ret Then
            Console.WriteLine("No results for case A" & analysiscase)
            Exit Sub
        End If

        ' Lets look at reactions in X
        Dim dataref As Integer = 12004002
        Dim OP_INIT_INFINITY As Integer = &H40
        Dim OP_IS_PER_REC As Integer = &H2


        gsa.SaveAs("c:\GSA_training\portalframe05.gwb")
        gsa.Close()
        gsa = Nothing

    End Sub

End Module
