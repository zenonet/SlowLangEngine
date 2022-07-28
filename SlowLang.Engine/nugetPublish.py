import os
import sys

csproj = open("SlowLang.Engine.csproj", "r+")


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

    oldVersion = get_old_version(txt)
    new = get_increased_version(oldVersion)
    print(oldVersion)
    print(new)
    csproj.write(csproj.read().replace(oldVersion, new))
    csproj.flush()


def pack():
    # pack the package
    os.popen("dotnet pack --no-build --output \\bin\\nuget\\")


def push():
    # get api key
    apiKey = open("nugetApiKey.txt").read()
    os.popen(
        "dotnet nuget push \\bin\\nuget\\SlowLang.Engine.1.0.0.nupkg --api-key " +
        apiKey +
        "--source https://api.nuget.org/v3/index.json"
    )


if sys.argv[1] == "updateVersion":
    update_version()
elif sys.argv[1] == "packAndPush":
    pack()
    push()
