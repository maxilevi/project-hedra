import re


with open('./Hedra.csproj', 'r+') as fp:
	contents = fp.read()
	fixed = re.sub(r'</Link>\s*</None>', '</Link>\n      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>\n    </None>', contents)
	fp.write(fixed)