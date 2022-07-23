"""
Program for linking GSA with Compos for composite beam design
This script is intended to
1. open GSA model using gsapy
2. extract member sections and lengths based on first saved list
   - write to member_list
3. extract defined profiles and write to sections_dictionary
4. write compos_file.csv file using the member_list
5. open compos_file.csv in Compos, design the members &
   extract the new sections to compos_results
6. reopen the GSA model,
   use compos_results and sections_dictionary to update the members
"""

import win32com.client  # https://pypi.org/project/pywin32/
from datetime import datetime
from tempfile import gettempdir
from gsapy import GSA

# gsapy references
# https://gitlab.arup.com/david.dekoning/gsapy/-/blob/master/docs/readme.rst
# https://gitlab.arup.com/hongkong-rd/JupyterSamples/blob/master/pyGSA/pyGSA_gsapy.ipynb


dead_load = 3.0  # kN/m^2
live_load = 5.0  # kN/m^2
# add additional variables as necessary to control what is added to Compos


def gsa2compos2gsa():

    # get GSA data
    print("Opening GSA model")
    gsa_model = GSA(r'Composite test.gwb', version='10.1')

#    member_list = [[i, gsa_model.get_section_info(i), gsa_model.get_member_length(i)] for i in gsa_model._get_entities_in_named_list("Secondary beams")]
    members = gsa_model.get_members(gsa_model._get_entities_in_named_list("Secondary beams"))
    sections = gsa_model.get(Section)

    member_list = [[i, sections[m.prop].desc.replace("%"," "), gsa_model.get_member_length(i)] for i, m in members.items()]

    sections_dictionary = {s.desc.replace("%", " "): i for i, s in sections.items()}

    # create Compos file
    print("Creating Compos file")
    now = datetime.now().strftime("%A, %d %b %Y at %H:%m:%S")
    # creates Compos file in system temp directory
    with open(gettempdir() + '\\compos_file.csv', 'w') as compos_file:
        compos_file.write("! This file was originally written by GSA2Compos.py"
                          " on " + now)
        compos_file.write("\nCOMPOS_FILE_VERSION,1")
        compos_file.write("\nTITLE,,,,,")
        compos_file.write("\nUNIT_DATA,FORCE,kN,0.00100000")
        compos_file.write("\nUNIT_DATA,LENGTH,m,1.00000")
        compos_file.write("\nUNIT_DATA,DISP,mm,1000.00")
        compos_file.write("\nUNIT_DATA,SECTION,cm,100.000")
        compos_file.write("\nUNIT_DATA,STRESS,N/mm²,1.00000e-006")
        compos_file.write("\nUNIT_DATA,MASS,t,0.00100000")
        for i in range(len(member_list)):
            beam_number = str(member_list[i][0])
            beam_section = "CAT BSI-UB " + str(member_list[i][1])
            beam_length = str(member_list[i][2])
            compos_file.write("\nMEMBER_TITLE,MEMBER-" + beam_number +
                              ",Latitude **  Longitude **,Note")
            compos_file.write("\nDESIGN_OPTION,MEMBER-" + beam_number +
                              ",EN1994-1-1:2004,UNPROPPED,BEAM_WEIGHT_YES,"
                              "SLAB_WEIGHT_YES,SHEAR_DEFORM_NO,"
                              "THIN_SECTION_YES,2.00000,2.00000")
            compos_file.write("\nCRITERIA_DEF_LIMIT,MEMBER-" + beam_number +
                              ",CONSTRUCTION_DEAD_LOAD,SPAN/DEF_RATIO,360.000")
            compos_file.write("\nCRITERIA_DEF_LIMIT,MEMBER-" + beam_number +
                              ",FINAL_LIVE_LOAD,SPAN/DEF_RATIO,360.000")
            compos_file.write("\nCRITERIA_DEF_LIMIT,MEMBER-" + beam_number +
                              ",TOTAL,SPAN/DEF_RATIO,200.000")
            compos_file.write("\nCRITERIA_BEAM_SIZE_LIMIT,MEMBER-" +
                              beam_number +
                              ",20.0000,100.000,10.0000,50.0000")
            compos_file.write("\nCRITERIA_OPTIMISE_OPTION,MEMBER-"
                              + beam_number +
                              ",MINIMUM_WEIGHT")
            compos_file.write("\nCRITERIA_SECTION_TYPE,MEMBER-" + beam_number +
                              ",255")
            compos_file.write("\nCRITERIA_FREQUENCY,MEMBER-" + beam_number +
                              ",CHECK_NATURAL_FREQUENCY,4.00000,1.00000"
                              ",0.100000")
            compos_file.write("\nBEAM_STEEL_MATERIAL_STD,MEMBER-" + beam_number
                              + ",S355")
            compos_file.write("\nBEAM_WELDING_MATERIAL,MEMBER-" + beam_number +
                              ",Grade 35")
            compos_file.write("\nBEAM_SPAN_LENGTH,MEMBER-" + beam_number + ",1,"
                              + beam_length)
            compos_file.write("\nBEAM_SECTION_AT_X,MEMBER-" + beam_number +
                              ",1,1,0.000000," + beam_section + ",TAPERED_NO")
            compos_file.write("\nRESTRAINT_POINT,MEMBER-" + beam_number +
                              ",STANDARD,0")
            compos_file.write("\nRESTRAINT_TOP_FALNGE,MEMBER-" + beam_number +
                              ",TOP_FLANGE_FIXED")
            compos_file.write("\nRESTRAINT_2ND_BEAM,MEMBER-" + beam_number +
                              ",SEC_BEAM_AS_REST")
            compos_file.write("\nEND_FLANGE_FREE_ROTATE,MEMBER-" + beam_number +
                              ",FREE_TO_ROTATE")
            compos_file.write("\nFINAL_RESTRAINT_POINT,MEMBER-" + beam_number +
                              ",STANDARD,0")
            compos_file.write("\nFINAL_RESTRAINT_NOSTUD,MEMBER-" + beam_number +
                              ",NOSTUD_ZONE_LATERAL_FIXED")
            compos_file.write("\nFINAL_RESTRAINT_2ND_BEAM,MEMBER-" +
                              beam_number +
                              ",SEC_BEAM_AS_REST")
            compos_file.write("\nFINAL_END_FLANGE_FREE_ROTATE,MEMBER-" +
                              beam_number + ",FREE_TO_ROTATE")
            compos_file.write("\nSLAB_CONCRETE_MATERIAL,MEMBER-" + beam_number +
                              ",C40/50,NORMAL,CODE_DENSITY,2.40000,NOT_APPLY,"
                              "0.330000,CODE_E_RATIO,CODE_STRAIN")
            compos_file.write("\nSLAB_DIMENSION,MEMBER-" + beam_number +
                              ",1,1,0.000000,0.130000,1.00000,1.00000,"
                              "TAPERED_YES,EFFECTIVE_WIDTH_NO")
            compos_file.write("\nREBAR_WESH,MEMBER-" + beam_number +
                              ",A142,2.50000,PARALLEL")
            compos_file.write("\nREBAR_MATERIAL,MEMBER-" + beam_number +
                              ",STANDARD,500A")
            compos_file.write("\nREBAR_LONGITUDINAL,MEMBER-" + beam_number +
                              ",PROGRAM_DESIGNED")
            compos_file.write("\nREBAR_TRANSVERSE,MEMBER-" + beam_number +
                              ",PROGRAM_DESIGNED")
            compos_file.write("\nDECKING_CATALOGUE,MEMBER-" + beam_number +
                              ",RLD,Ribdeck AL (0.9),S280,90.0000,"
                              "DECKING_JOINTED,JOINT_NOT_WELD")
            compos_file.write("\nSTUD_DEFINITION,MEMBER-" + beam_number +
                              ",STANDARD,19mm/100mm,WELDED_YES")
            compos_file.write("\nSTUD_LAYOUT,MEMBER-" + beam_number +
                              ",AUTO_MINIMUM_STUD,0.200000")
            compos_file.write("\nSTUD_NO_STUD_ZONE,MEMBER-" + beam_number +
                              ",-0.000000,-0.000000")
            compos_file.write("\nSTUD_EC4_APPLY,MEMBER-" + beam_number +
                              ",YES")
            compos_file.write("\nLOAD,MEMBER-" + beam_number +
                              ",Uniform,Area,1.00000,1.50000,"
                              + str(dead_load) + "," + str(live_load) +
                              ",Line,1.00000,1.50000," + str(dead_load) + "," +
                              str(live_load) +
                              ",3.00000,4.50000,6.00000,5.00000")
            compos_file.write("\nFLOOR_RESPONSE,MEMBER-" + beam_number +
                              ",FLOOR_RESPONSE_ANALYSIS_NO")
            compos_file.write("\nEC4_DESIGN_OPTION,MEMBER-" + beam_number +
                              ",SHRINKAGE_DEFORM_EC4_YES,"
                              "IGNORE_SHRINKAGE_DEFORM_NO,"
                              "APPROXIMATE_E_RATIO_YES,United Kingdom,CLASS_N"
                              ",1.10000,0.550000,28.0000,1.00000,36500.0"
                              ",36500.0,50.0000,50.0000")
            compos_file.write("\nSTUD_NCCI_LIMIT_APPLY,MEMBER-" + beam_number +
                              ",YES")
            compos_file.write("\nSTUD_EC4_RFT_POS,MEMBER-" + beam_number +
                              ",0.0300000")
            compos_file.write("\nEC4_STUD_GRADE,MEMBER-" + beam_number +
                              ",CODE_GRADE_YES,SD1_EN13918")
        compos_file.write("\nEND")
        compos_file.close()

    # opening file in Compos and designing
    print("Opening model in Compos")
    compos_model = win32com.client.Dispatch("Compos.Automation")
    compos_model.Open(gettempdir() + '\\compos_file.csv')
    compos_results = []
    for i in range(len(member_list)):
        member_name = compos_model.MemberName(i)
        compos_model.Design(member_name)
        section = compos_model.BeamSectDesc(member_name)
        print(member_name, section)
        compos_results.append([member_name[7:], section])
        # member[7:] strips "MEMBER-"
    compos_model.Save()
    # compos_model.SaveAs(gettempdir() + '\\compos_file_results.csv')
    compos_model.Close()
    print("  Compos model closed")

    print("Opening GSA model")
    gsa_model = GSA(r'C:\Temp\Composite test.gwb', version='10.1')
    for i in range(len(compos_results)):
        member_number = int(compos_results[i][0])
        member_new_section = compos_results[i][1].split()[2]
        new_section = sections_dictionary[member_new_section]
        # gets section number of Compos
        # result
        member = gsa_model.get_members(member_number)
        member.prop = new_section
        gsa_model.set(member)
    gsa_model.save()
    gsa_model.close()
    print("  GSA model closed")


if __name__ == "__main__":
    gsa2compos2gsa()
