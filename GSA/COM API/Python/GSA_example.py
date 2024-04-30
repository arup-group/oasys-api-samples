# This example:
#  - downloads a sample model
#  - opens it in GSA
#  - deletes any existing results
#  - analyses all analysis tasks
#  - saves the analysed model with a new file name
#  - closes GSA

import win32com.client  # https://pypi.org/project/pywin32/
import requests  # https://pypi.org/project/requests/    HTTP library

gsa_obj = win32com.client.Dispatch("Gsa_10_2.ComAuto")

print("Downloading sample stair model")

sample_model = requests.get(
    "https://samples.oasys-software.com/gsa/10.2/General/Stair.gwb", allow_redirects=True, verify=False
)
# saves sample_model in Temp folder
open("c:\\Temp\\Stair.gwb", "wb").write(sample_model.content)

print("Opening sample stair model")
gsa_obj.Open("c:\\Temp\\Stair.gwb")

print("Deleting existing results")
gsa_obj.Delete("RESULTS")

print("Analysing all analysis tasks")
gsa_obj.Analyse()

print("Saving the analysed model")
gsa_obj.SaveAs("c:\\Temp\\stair_analysed.gwb")

print("Closing GSA")
gsa_obj.Close()
gsa_obj = None

print("finished")
