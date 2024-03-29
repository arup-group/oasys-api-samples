import os
import oasys.adsec
import pathlib 

# Import modules from namespace
from Oasys.AdSec.IO.Serialization import JsonParser
from pathlib import Path

# This example shows how to convert a .ads file into API objects.
#
# You might like to run the 'version.py' example first, just to check
# that the API is installed correctly.

def ads_to_api():
    # Get the path of the .ads file. 
    dirname = os.path.dirname(__file__)
    common_directory = Path(dirname).parents[1]  # Third party library 'pathlib' is used to access required directory level
    ads_file_path = os.path.join(common_directory, 'DocumentationOnly', 'Python', 'PythonCodeSnippets','api2section.ads')
    
    # Read the .ads file 
    with open(ads_file_path, mode="r") as file:
        json = file.read()
    
    # Use JsonParser's Deserialize method to convert from JSON to API objects
    api = JsonParser.Deserialize(json);
    
    # Access the ISection
    api_sections = api.Sections;
    section_one = api_sections[0];

    # Access and print warnings to check for warning messages while converting JSON to API 
    api_warnings = api.Warnings;
    for i in range (api_warnings.Count):
        print(api_warnings[i].Description)

if __name__ == "__main__":
    ads_to_api()

    # Note: This feature works for Standard Materials 
    # Note: Tasks are ignored         

