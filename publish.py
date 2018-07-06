from __future__ import print_function

import os,shutil

prjdir = "RoughGrep"
version = "1.0"
def c(s):
    print(">",s)
    err = os.system(s)
    assert not err

def nuke(pth):
    if os.path.isdir(pth):
        shutil.rmtree(pth)

nuke(prjdir + "/bin")
nuke(prjdir + "/obj")
nuke("deploy")

c("msbuild RoughGrep.sln /p:Configuration=Release")
os.mkdir("deploy")
os.chdir("%s/bin/Release" % prjdir)
os.remove("rgg.pdb")

c("7za a ../../../deploy/RoughGrep-%s.zip ./*" % version)


