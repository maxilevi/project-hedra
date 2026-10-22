#!/usr/bin/env python3
"""Check out the pinned assets, fetching only files referenced by the game project."""
import argparse
from pathlib import Path, PurePosixPath
import subprocess
import xml.etree.ElementTree as ET


def referenced_assets(project):
    prefix = '../project-hedra-assets/'
    assets = set()
    for element in ET.parse(project).iter():
        source = element.get('Include', '').replace('\\', '/')
        if source.startswith(prefix):
            path = source[len(prefix):]
            if not path or '..' in PurePosixPath(path).parts or any(c in path for c in '*?['):
                raise ValueError(f'Expected an explicit asset path: {source}')
            assets.add(path)
    if not assets:
        raise ValueError('No game assets found in the project manifest')
    return sorted(assets)


def run(*arguments, cwd, **kwargs):
    return subprocess.run(arguments, cwd=cwd, check=True, text=True, **kwargs)


def checkout(root):
    root = Path(root).resolve()
    assets = referenced_assets(root / 'Hedra/Hedra.csproj')
    destination = root / 'project-hedra-assets'
    if destination.exists() and any(destination.iterdir()):
        raise ValueError('Asset checkout must be empty; this helper is intended for fresh CI jobs')
    url = run('git', 'config', '--file', '.gitmodules', '--get',
              'submodule.project-hedra-assets.url', cwd=root, capture_output=True).stdout.strip()
    tree = run('git', 'ls-tree', 'HEAD', '--', 'project-hedra-assets',
               cwd=root, capture_output=True).stdout.split()
    if not tree or tree[0] != '160000':
        raise ValueError('Missing pinned asset submodule revision')
    revision = tree[2]
    run('git', 'clone', '--filter=blob:none', '--depth=1', '--no-checkout', '--no-tags',
        '--', url, str(destination), cwd=root)
    # Anchored non-cone patterns avoid downloading authoring files and unused models.
    patterns = ''.join('/' + path.replace('\\', '\\\\') + '\n' for path in assets)
    run('git', 'sparse-checkout', 'set', '--no-cone', '--stdin', cwd=destination, input=patterns)
    run('git', 'fetch', '--depth=1', '--filter=blob:none', '--no-tags', 'origin', revision,
        cwd=destination)
    run('git', 'checkout', '--detach', revision, cwd=destination)
    missing = [path for path in assets if not (destination / path).is_file()]
    if missing:
        raise FileNotFoundError(f'Missing referenced assets: {missing[:10]}')
    print(f'Checked out {len(assets)} referenced assets at {revision}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repository-root', type=Path, default=Path(__file__).resolve().parents[2])
    checkout(parser.parse_args().repository_root)
