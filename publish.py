from __future__ import print_function

import os, shutil, glob

prjdir = "RoughGrep"
version = "3.1"


def c(s):
    print(">", s)
    err = os.system(s)
    assert not err


def nuke(pth):
    if os.path.isdir(pth):
        shutil.rmtree(pth)


def rm_globs(*globs):
    for g in globs:
        files = glob.glob(g)
        for f in files:
            print("Del", f)
            os.remove(f)


nuke(prjdir + "/bin")
nuke(prjdir + "/obj")
nuke("deploy")

c("msbuild RoughGrep.sln /p:Configuration=Release")
os.mkdir("deploy")
os.chdir("%s/bin" % prjdir)

rm_globs("Release/*.pdb", "Release/*.xml")
os.rename("Release", "RoughGrep")

c("7za a ../../deploy/RoughGrep-%s.zip RoughGrep" % version)
