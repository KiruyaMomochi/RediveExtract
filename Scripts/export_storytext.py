from sys import argv
from UnityPy import AssetsManager

storytext = argv[1]

if len(argv) == 2:
    writefile = f"{storytext}.bytes"
else:
    writefile = argv[2]

def get_dbfile(path: str):
    am = AssetsManager(path)
    for asset in am.assets.values():
        for obj in asset.objects.values():
            if obj.type == "TextAsset":
                return obj.read().script

with open(writefile, 'wb') as f:
    f.write(get_dbfile(storytext))
    f.close()
