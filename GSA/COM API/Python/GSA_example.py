import win32com.client  # https://pypi.org/project/pywin32/
import requests  # https://pypi.org/project/requests/    HTTP library

gsa_obj = win32com.client.Dispatch("Gsa_10_1.ComAuto")

print("Opening sample stair model")
sample_model = requests.get(
    "https://samples.oasys-software.com/gsa/10.1/General/Stair.gwb",
    allow_redirects=True)
# saves sample_model in Temp folder
open('c:\\Temp\\Stair.gwb', 'wb').write(sample_model.content)
gsa_obj.Open("c:\\Temp\\Stair.gwb")
gsa_obj.Delete("RESULTS")
gsa_obj.Analyse()
gsa_obj.SaveAs("c:\\Temp\\stair_analysed.gwb")
gsa_obj.Close()
gsa_obj = None

print("finished")
