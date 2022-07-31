import os
import subprocess
import sys

project_name = "SlowLang.Engine"

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
    print(popen("dotnet pack --no-build --output " + os.getcwd() + "\\bin\\nuget\\"))


def push():
    # get api key
    apiKey = open("nugetApiKey.txt").read()

    txt = csproj.read()

    cmd = "dotnet nuget push " + os.getcwd() + "\\bin\\nuget\\" + project_name + "." + get_old_version(
        txt) + ".nupkg --api-key " + apiKey + " --source https://api.nuget.org/v3/index.json"

    print("Generated command to push\n" + cmd)

    output = popen(cmd)

    if "409" in str(output):
        print("Package with this package version was pushed already")
    elif "Created" in output:
        print("success")
    else:
        print("unknown error:")
        print(output.replace("\\\\r\\\\n", "\n"))


if sys.argv[1] == "updateVersion" or sys.argv[1] == "0":
    update_version()
elif sys.argv[1] == "packAndPush" or sys.argv[1] == "1":
    pack()
    push()
