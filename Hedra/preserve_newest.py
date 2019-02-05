import re


with open('./Hedra.csproj', 'r+') as fp:
    contents = fp.read()
    fixed = re.sub(r'</Link>\s*</None>', '</Link>\n      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n    </None>', contents)
    second_fixed = re.sub(r'</Link>\s*</Content>', '</Link>\n      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n    </Content>', fixed)
    fp.seek(0)
    fp.write(second_fixed)
    fp.truncate()