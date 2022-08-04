"""
To use this script, copy it into your project directory
and replace project_name with the name of your C# project.

You also need to create another text file in the same directory,
call it "nugetApiKey.txt" and paste your API-key from nuget.org in there

Then you can run the script using these arguments:
- "updateVersion" or "0"
    to increase <PackageVersion> by 0.0.1 in your csproj file.
    You might need this because you have to increase your PackageVersion when 
    pushing an update to nuget.

- "packAndPublish" or "1"
    to create a nuget package (.nupkg file) in /<project>/bin/nuget
    and push it to nuget.org



Note:
I had some problems with my IDE (Jetbrains Rider) because
it blocks some files for write access.
For example updating the PackageVersion doesn't work while Rider has the project loaded.

I am not sure (and I am to lazy to try), but I think VS Code shouldn't make problems with this
"""

import os
import subprocess
import sys

# config

project_name = "SlowLang.Engine"  # The name of your csproj file without the file extension

source = "https://api.nuget.org/v3/index.json"  # The nuget server to push to. By default nuget.org




csproj = open(project_name + ".csproj", "r+")

def popen(cmd: str):
    process = subprocess.Popen(
        cmd,
        stdout=subprocess.PIPE,
        stderr=None,
        shell=True
    )
    return str(process.communicate()[0])


def get_increased_version(oldVersion: str) -> str:
    # decode the oldVersion string
    parts = oldVersion.split(".")

    # increase the smallest part of the oldVersion
    parts[2] = str(int(parts[-1]) + 1)

    # encode the oldVersion
    version = ""

    for i in parts:
        version += i + "."

    return version.strip('.')


def get_old_version(txt: str):
    oldVersion = txt.split("<PackageVersion>")[1].split("</PackageVersion>")[0]
    return oldVersion


def update_version() -> None:
    txt = csproj.read()

    old_version = get_old_version(txt)
    new = get_increased_version(old_version)
    print(old_version)
    print(new)
    csproj.write(csproj.read().replace(old_version, new))
    csproj.flush()


def pack():
    # pack the package
    cmd = ("dotnet pack --no-build --output " + os.getcwd() + "/bin/nuget/").replace("\\", "/")
    print(cmd + "\n\n")
    print(popen(cmd))


def push():
    # get api key
    apiKey = open("nugetApiKey.txt").read()

    txt = csproj.read()

    cmd = "dotnet nuget push " + os.getcwd() + "/bin/nuget/" + project_name + "." + get_old_version(
        txt) + ".nupkg --api-key " + apiKey + " --source " + source

    print("Generated command to push:\n    " + cmd)

    output = popen(cmd)

    if "409" in str(output):
        print("Package with this package version was pushed already")
    elif "Created" in output:
        print("success")
    else:
        print("unknown error:\n    " + output)


if sys.argv[1] == "updateVersion" or sys.argv[1] == "0":
    update_version()
elif sys.argv[1] == "packAndPush" or sys.argv[1] == "1":
    pack()
    push()
