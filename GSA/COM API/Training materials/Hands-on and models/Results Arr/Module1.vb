Imports Interop.gsa_10_2
Imports System.Diagnostics

Module Module1

    Sub Main()
        Dim gsa As New Interop.gsa_10_2.ComAuto

        ' open existing file
        gsa.Open("c:\GSA_Training\portalfram04.gwb")
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

        Dim OP_INIT_INFINITY As Integer = &H40
        Dim OP_IS_PER_REC As Integer = &H2

        ret = gsa.Output_Init_Arr(OP_INIT_INFINITY, "global", acaseString, ResHeader.REF_REAC, 3)

        If Not Integer.Equals(0, ret) Then
            Console.WriteLine("Output_Init_Arr failed.")
            Exit Sub
        End If

        Dim ents() As Integer ' array to store node numbers in the list

        ret = gsa.EntitiesInList("XY1", GsaEntity.NODE, ents)

        ' loop over the contents of the list
        For Each ent In ents
            ret = gsa.Output_DataExist(ent)
            If 0 = ret Then
                Console.WriteLine("Output_DataExist failed for " & ent)
                Exit For
            End If

            Dim results() As GsaResults = Nothing

            ret = gsa.Output_Extract_Arr(ent, results, 0)

            If results.Length <> 1 Then
                Console.WriteLine("More than 1 results components returned.") ' we dont expect this to happen in the case of nodes
                Exit For
            End If

            Dim index As Integer
            For index = 0 To results(0).dynaResults.Length - 1
                Console.WriteLine("Result for node/elem " & ent & " in direction " & index & " = " & results(0).dynaResults(index))
            Next

        Next

        gsa.SaveAs("c:\GSA_training\portalframe05.gwb")
        gsa.Close()
        gsa = Nothing

    End Sub

End Module
