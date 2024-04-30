Imports System.IO
Imports Interop.Gsa_10_2

Module Module1
    Dim sv_gwa As String = "GR_VIEW.17	134218690	63	test	0	0	755	296	1	9	\
 	4   1	1	1	1	-4	2	1	-4	3	1	-4	8	1	-4	2	\
 	0   \
 	0.000000000000	90.0000000000	90.0000000000	0.000000000000	6.50000000000	9.00000000000	2.00000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	0.0272383000000	1	0.000000000000	0.000000000000	1.00000000000	0	0.000000000000	1.00000000000	0	1.00000000000	30.0000000000	2	0.000000000000	0.000000000000	1000000.00000	WHITE	RGB(0xaa0000)	0	0.850000000000	0.750000000000	0.950000000000	2	1	-5	3	1	-5	\
 	0   0	1.00000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	0.000000000000	1.00000000000	1.00000000000	1.00000000000	1.00000000000	3.00000000000	1.20000000000	55.0000000000	3	2	0	1	\
 	2   \
 	25231545    9	0	10	0	8	0	0	0	1	0	1	2	2	1	1	1	7	0	0	2	0	3	1	0	1	0	2	0	3	0	6	0	7	0	8	0	1	0	2	0	3	0	6	0	7	0	8	0	0	1	-4	0	1	-4	\
 	0   0	GLOBAL	0.000000000000	0	-<I>	<I>	\
 	0   \
 	N	4.e-311	m	4.e-311	kg	4.e-311	s	4.e-311	°C	4.e-311	m	4.e-311	Pa	4.e-311	m/s²	4.e-311	m	4.e-311	\
 	1   4	2	4	4.94066e-324	\
 	1.00000000000	-<I>	<I>	8	\
 	0   \
 	<I>	3	5	2	1	\
 	25231364    1	0	0	0	0	0	0	0	1	0	1	2	2	1	1	1	7	0	0	2	0	3	1	0	1	0	2	0	3	0	6	0	7	0	8	0	1	0	2	0	3	0	6	0	7	0	8	0	0	1	-4	0	1	-4	\
 	12001003    6	GLOBAL	0.0500000000000	1	-0.0197872761637	0.00167759228498	\
 	0   \
 	N	4.e-311	m	4.e-311	kg	4.e-311	s	4.e-311	°C	4.e-311	m	4.e-311	Pa	4.e-311	m/s²	4.e-311	m	4.e-311	\
 	1   4	2	4	4.94066e-324	\
 	1.00000000000	-<I>	<I>	8	\
 	0   \
 	<I>	3	5	2	1"
    Sub Main()
        Dim gsa As New ComAuto
        ' this is the GSA file that has our view templates

        Dim current_directory As String = Directory.GetCurrentDirectory()
        Dim template_relative_path As String = "..\..\sample_files\view_templates.gwb"
        Dim gsa_file_relative_path As String = "..\..\sample_files\steel_design_medium.gwb"
        Dim full_path As String = Path.Combine(current_directory, template_relative_path)

        gsa.Open(full_path)
        Dim ref_template_contour = gsa.ViewRefFromName("SGV", "template-contour")
        Dim ref_template_labels = gsa.ViewRefFromName("SGV", "template-labels")

        Dim contour_template As String = gsa.GwaCommand("GET, GR_VIEW," & ref_template_contour)
        Dim label_template As String = gsa.GwaCommand("GET, GR_VIEW," & ref_template_labels)
        gsa.Close()

        full_path = Path.Combine(current_directory, gsa_file_relative_path)
        gsa.Open(full_path)

        ' Create a new saved view that will display a contour
        Dim viewRef As Integer = gsa.CreateNewView("contour")

        ' Now "apply" contour_template to this view and set 
        ' the result component to nodal displacement, z
        gsa.SetViewContour(viewRef, 12001003, contour_template)

        ' Turn labels on by apply the labels template
        gsa.SetViewLabels(viewRef, label_template)

        viewRef = gsa.CreateNewView("diag")

        ' Rescale and 
        gsa.RescaleViewData(viewRef)

        gsa.UpdateViews()

        gsa.SaveViewToFile("ALL_SGV", "PNG")

        Console.ReadKey()
        gsa.Save()


        gsa.Close()

    End Sub

End Module
