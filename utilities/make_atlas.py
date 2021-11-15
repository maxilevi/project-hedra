import argparse
import subprocess
import sys
import os

parser = argparse.ArgumentParser(description='Folder with images')
parser.add_argument('--folder', type=str)


args = parser.parse_args()

if not args.folder:
	print("Invalid folder")
	sys.exit(-1)

size = 4096
SLICE_SIZE = 200
good = True
files = [f"{args.folder}/{x}" for x in os.listdir(args.folder) if 'atlas' not in x]
for i in range(0, len(files), SLICE_SIZE):
	cmd = ['atlasc.exe'] + files[i:i+SLICE_SIZE] + ['-o', f'{args.folder}/atlas_{i}-{i+SLICE_SIZE}.json', '-W', str(size), '-H', str(size)]
	command = subprocess.run(cmd, capture_output=True)
	sys.stdout.buffer.write(command.stdout)
	sys.stderr.buffer.write(command.stderr)
	good &= (command.returncode == 0)
	print(f"atlasc exited with code {command.returncode}")

if good:
	for f in files:
		os.remove(f)
	